using CStarCompiler.Parsing.Nodes.Declarations;
using CStarCompiler.SemanticAnalyze.Units.Declarations;
using CStarCompiler.SemanticAnalyze.Units.Module;
using CStarCompiler.SemanticAnalyze.Units.Type;
using CStarCompiler.Shared;
using CStarCompiler.Shared.Logs;

namespace CStarCompiler.SemanticAnalyze.Tables;

public sealed class StructTable(SemanticContext context) : ContextTable(context)
{
    private readonly Dictionary<string, Dictionary<string, StructUnit>> _structs = [];
    
    private readonly StructGraph _structGraph = new();
    
    private List<StructUnit>? _sortedStructs;
    private List<StructUnit>? _unusedStructs;
    
    public void AddStruct(string moduleName, StructDeclarationNode structNode)
    {
        var structUnit = new StructUnit(structNode.Identifier.Name);
        
        // create dict if module not contains any structs early
        if (!_structs.TryGetValue(moduleName, out var structDict))
        {
            var structs = new Dictionary<string, StructUnit> { { structUnit.Name, structUnit } };
            _structs.Add(moduleName, structs);
        }
        // check for struct declaration duplicate
        else if (structDict.TryGetValue(structUnit.Name, out var originalStruct))
        {
            var originalLocation = _context.LocationTable.GetLocation(originalStruct);
            var hint = $"Original struct at {CompilerLogger.FormatLocation(originalLocation)}";
            
            CompilerLogger.DumpError(CompilerLogCode.StructDeclarationDuplicate, structNode.Identifier.Location, hint);
            
            return; // don't check fields, struct not saved to table
        }
        // ok, add to structs
        else structDict.Add(structUnit.Name, structUnit);

        // add to graph for later recursive fields analyze
        _structGraph.AddStruct(structUnit);
        _context.LocationTable.AddLocation(structUnit, structNode.Identifier.Location);

        // inner structs recursive adds to table after fields and functions
        var nestedStructs = new List<StructDeclarationNode>();
        
        // struct fields and functions 
        foreach (var member in structNode.Members)
        {
            switch (member)
            {
                case FieldDeclarationNode fieldDeclarationNode:
                    AddField(structUnit, fieldDeclarationNode);
                    break;
                case FunctionDeclarationNode functionDeclarationNode:
                    // todo: add to FunctionTable
                    break;
                case StructDeclarationNode structDeclarationNode:
                    nestedStructs.Add(structDeclarationNode);
                    break;
            }
        }

        // recursive add inner structs
        foreach (var innerStruct in nestedStructs) AddStruct(moduleName, innerStruct);
    }
    
    private void AddField(StructUnit structUnit, FieldDeclarationNode fieldNode)
    {
        structUnit.Fields ??= new();

        // todo: check for field name shadowing nested structs
        
        // field name shadow declared struct type
        if (fieldNode.Identifier.Name == structUnit.Name)
        {
            var message = $"Struct field '{fieldNode.Identifier.Name}' shadow struct '{structUnit.Name}'";
            CompilerLogger.DumpError(CompilerLogCode.StructFieldNameShadowStructType,
                fieldNode.Identifier.Location, message);
            
            return;
        }
        
        // field name duplicate
        if (structUnit.Fields.TryGetValue(fieldNode.Identifier.Name, out var originalField))
        {
            var message = $"Field '{fieldNode.Type.Name} {fieldNode.Identifier.Name}' duplicated in struct '{structUnit.Name}'";
            
            var originalLocation = _context.LocationTable.GetLocation(originalField);
            var hint = $"Original field '{originalField.Type.Name} {originalField.Name}' at {CompilerLogger.FormatLocation(originalLocation)}";
            
            CompilerLogger.DumpError(CompilerLogCode.StructFieldNameDuplicate,
                fieldNode.Identifier.Location, message, hint);
            return;
        }
        
        // todo: add to FieldDeclarationNode modifiers
        var field = new VariableUnit(fieldNode.Identifier.Name, new(fieldNode.Type.Name, false, false, false));
        
        structUnit.Fields.Add(field.Name, field);
        _context.LocationTable.AddLocation(field, fieldNode.Location);
    }

    public void AnalyzeFieldsTypes()
    {
        foreach (var (moduleName, dict) in _structs)
        {
            foreach (var (_, structUnit) in dict)
            {
                if (structUnit.Fields == null) continue;
            
                foreach (var (_, field) in structUnit.Fields)
                {
                    // todo: add visibility check (private, public, internal)
                    
                    // field type existing in declared module
                    if (!TryGetStruct(moduleName, field.Type.Name, out var fieldStruct))
                        goto TypeNotFound;

                    _structGraph.AddField(structUnit, fieldStruct!);
                    continue;
                    
                    TypeNotFound:
                    CompilerLogger.Common.DumpTypeNotFound(field.Type.Name, _context.LocationTable.GetLocation(field));
                }
            }
        }
    }

    public void AnalyzeTypeDependencies()
    {
        // not exclude recursive structs from later analyze
        var result = _structGraph.Sort(_context.LocationTable);
        if (result == null) return;

        _sortedStructs = result.Value.Sorted;
        
        // unused structs, later be excluded from structs
        _unusedStructs = result.Value.Unused;
    }

    public void ExcludeUnusedStructs()
    {
        if (_unusedStructs == null) return;
        
        foreach (var unusedStruct in _unusedStructs)
            _structs.Remove(unusedStruct.Name);
    }
    
    private Dictionary<string, VariableUnit>? GetStructFields(string moduleName, string structName)
        => _structs[moduleName][structName].Fields;
    
    private bool IsStructHaveFields(string moduleName, string structName)
        => _structs.TryGetValue(moduleName, out var structs)
           && structs.TryGetValue(structName, out var structUnit) && structUnit.Fields != null;
    
    public bool IsStructExist(string moduleName, string structName)
        => _structs.TryGetValue(moduleName, out var structs) && structs.ContainsKey(structName);

    public bool TryGetStruct(string moduleName, string structName, out StructUnit? structUnit)
    {
        structUnit = null;
        
        // module not have any structs
        if (!_structs.TryGetValue(moduleName, out var structs)) return false;

        if (!structs.TryGetValue(structName, out var result))
        {
            // check in module imports
            var imports = _context.ModuleTable.GetModuleImports(moduleName);
            if (imports == null) return false;

            foreach (var import in imports)
            {
                // import not have any structs
                if (!_structs.TryGetValue(import.Name, out var importStructs)) continue;

                // import module not have struct
                if (!importStructs.TryGetValue(structName, out structUnit)) continue;
                
                return true;
            }
            
            return false;
        }

        structUnit = result;
        return true;
    }
    
    public void Clear()
    {
        _structs.Clear();
        _structGraph.Clear();
    }
    
    private class StructGraph
    {
        private readonly Dictionary<StructUnit, List<StructUnit>> _values = [];
        
        public void AddStruct(StructUnit structUnit) => _values.TryAdd(structUnit, []);
        
        public void AddField(StructUnit structUnit, StructUnit field)
        {
            AddStruct(structUnit);
            AddStruct(field);
            _values[structUnit].Add(field);
        }
        
        public (List<StructUnit> Sorted, List<StructUnit>? Unused)? Sort(LocationTable table)
        {
            var visited = new Dictionary<StructUnit, GraphVisitState>();
            var sorted = new List<StructUnit>();
            var unused = new List<StructUnit>();
            
            foreach (var structUnit in _values.Keys) visited[structUnit] = GraphVisitState.NotVisited;
            
            foreach (var structUnit in _values.Keys)
            {
                var location = table.GetLocation(structUnit);
                
                if (visited[structUnit] != GraphVisitState.NotVisited) continue;
                if (Step(table, structUnit, visited, sorted, unused)) continue;
                    
                // todo move to Step
                var message = $"Struct '{structUnit.Name}' have recursion field";
                CompilerLogger.DumpError(CompilerLogCode.StructFieldRecursive, location, message);
                
                return null;
            }
            
            // from dependents to independents
            return (sorted, unused); 
        }
        
        private bool Step(LocationTable table, StructUnit structUnit, Dictionary<StructUnit, GraphVisitState> visited, List<StructUnit> result, List<StructUnit> unused)
        {
            switch (visited[structUnit])
            {
                // cycle
                case GraphVisitState.Visiting: return false;
                case GraphVisitState.Visited: return true;
            }

            visited[structUnit] = GraphVisitState.Visiting;
    
            if (_values.TryGetValue(structUnit, out var dependencies))
            {
                foreach (var dependency in dependencies)
                    if (!Step(table, dependency, visited, result, unused)) return false;
            }
    
            visited[structUnit] = GraphVisitState.Visited;
            
            // todo: move from graph (later need check functions for type usage)
            // unused struct
            if (_values[structUnit].Count == 0)
            {
                var message = $"Type '{structUnit.Name}' not used in code";
                var location = table.GetLocation(structUnit);
                CompilerLogger.DumpWarning(CompilerLogCode.TypeUnused, location, message);
                
                unused.Add(structUnit);
                return true;
            }
            
            result.Add(structUnit);
            return true;
        }
        
        public void Clear() => _values.Clear();
    }
}
