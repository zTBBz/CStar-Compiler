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
            var info = $"Original struct at line {originalLocation.Line}, column {originalLocation.Column}";
            
            CompilerLogger.DumpError(CompilerLogCode.StructDeclarationDuplicate, structNode.Identifier.Location, info);
            
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
        
        // by now no need to check, it checks later in graph sorting
        
        // simple recursive type field
        /*if (fieldNode.Type.Name == structUnit.Name)
        {
            CompilerLogger.DumpError(CompilerLogCode.StructFieldRecursive, fieldNode.Type.Location);
            return;
        }*/

        // field name shadow declared struct type
        if (fieldNode.Identifier.Name == structUnit.Name)
        {
            CompilerLogger.DumpError(CompilerLogCode.StructFieldNameShadowStructType, fieldNode.Identifier.Location);
            return;
        }
        
        // field name duplicate
        if (structUnit.Fields.TryGetValue(fieldNode.Identifier.Name, out var originalField))
        {
            var originalLocation = _context.LocationTable.GetLocation(originalField);
            var info = $"Original field name at line {originalLocation.Line}, column {originalLocation.Column}";
            
            CompilerLogger.DumpError(CompilerLogCode.StructFieldNameDuplicate, fieldNode.Identifier.Location, info);
            return;
        }
        
        // todo: add to FieldDeclarationNode modifiers and remove expression initializer
        var field = new VariableUnit(fieldNode.Identifier.Name, new(fieldNode.Type.Name, false, false, false));
        
        structUnit.Fields.Add(field.Name, field);
        _context.LocationTable.AddLocation(field, fieldNode.Location);
    }

    public void AnalyzeFieldsExisting()
    {
        foreach (var (moduleName, dict) in _structs)
        {
            var module = _context.ModuleTable.GetModule(moduleName);
            var file = _context.LocationTable.GetLocation(module).File;
            
            CompilerLogger.SetFile(file);
            
            foreach (var (_, structUnit) in dict)
            {
                if (structUnit.Fields == null) continue;
            
                foreach (var (_, field) in structUnit.Fields)
                {
                    // todo: add visibility check (private, public, internal)
                    
                    // field type existing in declared module
                    if (!TryGetStruct(moduleName, field.Type.Name, out var fieldStruct))
                    {
                        var imports = module.Imports;
                        if (imports == null) goto TypeNotExist;
                        
                        // field type existing in imported modules
                        foreach (var import in imports)
                            if (TryGetStruct(import.Name, field.Type.Name, out fieldStruct)) goto TypeExist;
                        
                        goto TypeNotExist;
                    }

                    TypeExist:
                    _structGraph.AddField(structUnit, fieldStruct!);
                    continue;
                    
                    TypeNotExist:
                    CompilerLogger.DumpError(CompilerLogCode.TypeNotFound, _context.LocationTable.GetLocation(field));
                }
            }
        }
    }

    public void AnalyzeFieldsRecursion() => _structGraph.Sort(_context.LocationTable);
    
    private Dictionary<string, VariableUnit>? GetStructFields(string moduleName, string structName)
        => _structs[moduleName][structName].Fields;
    
    private bool IsStructHaveFields(string moduleName, string structName)
        => _structs.TryGetValue(moduleName, out var structs)
           && structs.TryGetValue(structName, out var structUnit) && structUnit.Fields != null;
    
    private bool IsStructExist(string moduleName, string structName)
        => _structs.TryGetValue(moduleName, out var structs) && structs.ContainsKey(structName);

    public bool TryGetStruct(string moduleName, string structName, out StructUnit? structUnit)
    {
        structUnit = null;
        if (!_structs.TryGetValue(moduleName, out var structs)) return false;

        if (!structs.TryGetValue(structName, out var result)) return false;

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

        public List<StructUnit>? Sort(LocationTable table)
        {
            var visited = new Dictionary<StructUnit, GraphVisitState>();
            var sorted = new List<StructUnit>();
    
            foreach (var structUnit in _values.Keys) visited[structUnit] = GraphVisitState.NotVisited;
            
            foreach (var structUnit in _values.Keys)
            {
                var moduleLocation = table.GetLocation(structUnit);
                
                CompilerLogger.SetFile(moduleLocation.File);

                if (visited[structUnit] != GraphVisitState.NotVisited) continue;
                if (Step(structUnit, visited, sorted)) continue;
                    
                CompilerLogger.DumpError(CompilerLogCode.StructFieldRecursive, moduleLocation);
                return null;
            }
            
            // from dependents to independents
            return sorted; 
        }
        
        private bool Step(StructUnit structUnit, Dictionary<StructUnit, GraphVisitState> visited, List<StructUnit> result)
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
                    if (!Step(dependency, visited, result)) return false;
            }
    
            visited[structUnit] = GraphVisitState.Visited;
            result.Add(structUnit);
            return true;
        }
        
        public void Clear() => _values.Clear();
    }
}
