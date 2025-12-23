using CStarCompiler.Logs;
using CStarCompiler.Parsing.Nodes.Declarations;
using CStarCompiler.SemanticAnalyze.Declarations;
using CStarCompiler.SemanticAnalyze.Units.Module;
using CStarCompiler.SemanticAnalyze.Units.Type;

namespace CStarCompiler.SemanticAnalyze.Tables;

public sealed class StructTable(SemanticContext context) : ContextTable(context)
{
    private readonly Dictionary<string, Dictionary<string, StructUnit>> _structs = [];
    
    public void AddStruct(string moduleName, StructDeclarationNode structNode)
    {
        var structUnit = new StructUnit(structNode.Name);
        
        if (!_structs.TryGetValue(moduleName, out var structDict))
        {
            var structs = new Dictionary<string, StructUnit> { { structUnit.Name, structUnit } };
            _structs.Add(moduleName, structs);
        }
        // struct duplicate
        else if (!structDict.TryAdd(structUnit.Name, structUnit))
        {
            CompilerLogger.DumpError(CompilerLogCode.StructDeclarationDuplicate,
                _context.LocationTable.GetLocation(structUnit));
            
            // todo: write string where located original struct

            return; // don't check fields, struct not saved to table
        }

        // inner structs add to table after fields and functions
        var innerStructs = new List<StructDeclarationNode>();
        
        // struct fields and functions 
        foreach (var member in structNode.Members)
        {
            switch (member)
            {
                case FieldDeclarationNode fieldDeclarationNode:
                    AddField(moduleName, structUnit, fieldDeclarationNode);
                    break;
                case FunctionDeclarationNode functionDeclarationNode:
                    // todo: add to FunctionTable
                    break;
                case StructDeclarationNode structDeclarationNode:
                    innerStructs.Add(structDeclarationNode);
                    break;
            }
        }

        // recursive add inner structs
        foreach (var innerStruct in innerStructs) AddStruct(moduleName, innerStruct);
    }
    
    private void AddField(string moduleName, StructUnit structUnit, FieldDeclarationNode fieldNode)
    {
        structUnit.Fields ??= new();

        var typeExist = false;
                    
        // simple recursive type field
        if (fieldNode.Type.Name == structUnit.Name)
        {
            CompilerLogger.DumpError(CompilerLogCode.StructRecursiveField, fieldNode.Type.Location);
            return;
        }
        
        // field type exist
        // todo: add visibility check (private, public, internal)
        // todo: move blocks with type existing to separate function (later be used for parameters check etc.)
        if (!TryGetStruct(moduleName, fieldNode.Type.Name, out var fieldType))
        {
            var imports = _context.ModuleTable.GetModuleImports(moduleName);

            // field type exist in imported modules
            if (imports != null)
            {
                foreach (var import in imports)
                {
                    if (!TryGetStruct(import.Name, fieldNode.Type.Name, out fieldType))
                        continue;

                    typeExist = true;
                    break;
                }
            }

            if (!typeExist)
            {
                CompilerLogger.DumpError(CompilerLogCode.TypeNotExist, fieldNode.Type.Location);
                return;
            }
        }

        if (IsHaveDeepRecursiveField(moduleName, structUnit)) return;

        // field name duplicate
        if (structUnit.Fields.ContainsKey(fieldNode.Name))
        {
            CompilerLogger.DumpError(CompilerLogCode.StructFieldNameDuplicate, fieldNode.Location);
            return;
        }
        
        // todo: add to FieldDeclarationNode modifiers and remove expression initializer
        var field = new VariableUnit(fieldNode.Name, new(fieldNode.Type.Name, false, false, false));
        structUnit.Fields.Add(field.Name, field);
    }
    
    private bool IsHaveDeepRecursiveField(string moduleName, StructUnit structUnit)
    {
        if (structUnit.Fields == null) return false;
        
        // deep recursive type field
        foreach (var field in structUnit.Fields)
        {
            var fieldType = field.Value.Type.Name;

            if (!DeepRecursiveFieldStep(structUnit.Name, fieldType)) continue;
            
            // todo: write string where located recursive type field 
            CompilerLogger.DumpError(CompilerLogCode.StructRecursiveField, _context.LocationTable.GetLocation(field.Value));
            return true;
        }

        return false;
        
        bool DeepRecursiveFieldStep(string structName, string fieldType)
        {
            if (structName == fieldType) return true;
        
            var fields = GetStructFields(moduleName, structName);
        
            if (fields == null) return false;
        
            foreach (var field in fields)
                if (DeepRecursiveFieldStep(structName, field.Value.Type.Name)) return true;
        
            return false;
        }
    }

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
    
    public void Clear() => _structs.Clear();
}
