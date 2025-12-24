using CStarCompiler.Parsing.Nodes.Declarations;
using CStarCompiler.SemanticAnalyze.Units.Declarations;
using CStarCompiler.SemanticAnalyze.Units.Module;
using CStarCompiler.SemanticAnalyze.Units.Type;
using CStarCompiler.Shared.Logs;

namespace CStarCompiler.SemanticAnalyze.Tables;

public sealed class StructTable(SemanticContext context) : ContextTable(context)
{
    private readonly Dictionary<string, Dictionary<string, StructUnit>> _structs = [];
    
    private readonly Dictionary<string, List<string>> _structToModules = [];
    
    public void AddStruct(string moduleName, StructDeclarationNode structNode)
    {
        var structUnit = new StructUnit(structNode.Identifier.Name);
        
        // module not contains any structs early
        if (!_structs.TryGetValue(moduleName, out var structDict))
        {
            var structs = new Dictionary<string, StructUnit> { { structUnit.Name, structUnit } };
            _structs.Add(moduleName, structs);
        }
        // struct declaration duplicate
        else if (structDict.TryGetValue(structUnit.Name, out var originalStruct))
        {
            var originalLocation = _context.LocationTable.GetLocation(originalStruct);
            var info = $"Original struct at line {originalLocation.Line}, column {originalLocation.Column}";
            
            CompilerLogger.DumpError(CompilerLogCode.StructDeclarationDuplicate, structNode.Identifier.Location, info);
            
            return; // don't check fields, struct not saved to table
        }
        // ok, add to structs
        else structDict.Add(structUnit.Name, structUnit);

        if (!_structToModules.TryGetValue(structUnit.Name, out var modules))
            _structToModules.Add(structUnit.Name, [moduleName]);
        else modules.Add(moduleName);

        _context.LocationTable.AddLocation(structUnit, structNode.Identifier.Location);

        // inner structs add to table after fields and functions
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
        
        // simple recursive type field
        if (fieldNode.Type.Name == structUnit.Name)
        {
            CompilerLogger.DumpError(CompilerLogCode.StructFieldRecursive, fieldNode.Type.Location);
            return;
        }

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

    public void AnalyzeFields()
    {
        foreach (var (moduleName, dict) in _structs)
        {
            var module = _context.ModuleTable.GetModule(moduleName);
            
            CompilerLogger.SetFile(_context.LocationTable.GetLocation(module).File);
            
            foreach (var (_, structUnit) in dict)
            {
                if (structUnit.Fields == null) continue;
            
                foreach (var (_, field) in structUnit.Fields)
                {
                    // todo: add visibility check (private, public, internal)
                    
                    // field type existing in declared module
                    if (!IsStructExist(moduleName, field.Type.Name))
                    {
                        var imports = module.Imports;
                        if (imports == null) goto TypeNotExist;
                        
                        // field type existing in imported modules
                        foreach (var import in imports)
                            if (IsStructExist(import.Name, field.Type.Name)) goto TypeExist;
                        
                        goto TypeNotExist;
                    }

                    TypeExist:
                    AnalyzeDeepRecursiveField(moduleName, structUnit);
                    continue;
                    
                    TypeNotExist:
                    CompilerLogger.DumpError(CompilerLogCode.TypeNotFound, _context.LocationTable.GetLocation(field));
                }
            }
        }
    }
    
    private void AnalyzeDeepRecursiveField(string moduleName, StructUnit structUnit)
    {
        foreach (var (_, field) in structUnit.Fields!)
        {
            var fieldType = field.Type.Name;

            if (!Step(structUnit.Name, fieldType)) continue;
            
            CompilerLogger.DumpError(CompilerLogCode.StructFieldRecursive,
                _context.LocationTable.GetLocation(field), $"Recursion in '{structUnit.Name}' struct.");
            
            return;
        }
        
        bool Step(string structName, string fieldType)
        {
            if (fieldType == structName) return true;

            if (!IsStructExist(moduleName, fieldType))
            {
                var imports = _context.ModuleTable.GetModuleImports(moduleName);
                if (imports == null) return false; // field type not exist in imports
        
                foreach (var import in imports)
                {
                    if (!TryGetStruct(import.Name, fieldType, out var fieldTypeStruct)) continue;

                    var fields = fieldTypeStruct!.Fields;
                    if (fields == null) return false; // field type not have fields
                
                    foreach (var (_, field) in fields)
                        if (Step(structName, field.Type.Name)) return true;
                }
            }
            else
            {
                var fields = GetStructFields(moduleName, fieldType);
                if (fields == null) return false;
                
                foreach (var field in fields)
                    if (Step(structName, field.Value.Type.Name)) return true;
            }
            
            return false;
        }
    }

    private Dictionary<string, VariableUnit>? GetStructFields(string moduleName, string structName)
        => _structs[moduleName][structName].Fields;

    private bool TryGetStructModules(string structName, out List<string>? modules)
        => _structToModules.TryGetValue(structName, out modules);
    
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
    
    public void Clear() => _structs.Clear();
}
