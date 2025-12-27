using System.Collections.Generic;
using CStarCompiler.Parsing.Nodes.Declarations;
using CStarCompiler.SemanticAnalyze.Units.Declarations;
using CStarCompiler.SemanticAnalyze.Units.Type;
using CStarCompiler.Shared.Logs;

namespace CStarCompiler.SemanticAnalyze.Tables;

public sealed class FunctionTable(SemanticContext context) : ContextTable(context)
{
    private readonly Dictionary<string, Dictionary<string, FunctionUnit>> _moduleFunctions = [];
    private readonly Dictionary<(string ModuleName, string StructName), Dictionary<string, FunctionUnit>> _structFunctions = [];
    
    public void AddModuleFunction(string moduleName, FunctionDeclarationNode functionNode)
    {
        // return type existing
        if (!_context.StructTable.TryGetStruct(moduleName, functionNode.ReturnType.Name, out var returnStructUnit))
        {
            CompilerLogger.Common.DumpTypeNotFound(functionNode.ReturnType.Name, functionNode.ReturnType.Location);
            return; // can't analyze function with incorrect returning type
        }
        
        // todo: add modifiers
        var returnType = new TypeUnit(returnStructUnit!.Name, false, false, false);
        
        var functionUnit = new FunctionUnit(functionNode.Identifier.Name, returnType);
        
        if (!_moduleFunctions.TryGetValue(moduleName, out var functions))
        {
            functions = [];
            _moduleFunctions.Add(moduleName, functions);
        }
        else if (functions.TryGetValue(functionUnit.Name, out var originalFunction))
        {
            var originalLocation = _context.LocationTable.GetLocation(originalFunction);
            var hint = $"Original function at {CompilerLogger.FormatLocation(originalLocation)}";
            
            CompilerLogger.DumpError(CompilerLogCode.FunctionDeclarationDuplicate, functionNode.Identifier.Location, hint);
            return; // can't analyze function duplicate
        }
        // ok, add to module functions
        else functions.Add(functionNode.Identifier.Name, functionUnit);
        
        
    }
    
    public void AddStructFunction(string moduleName, FunctionDeclarationNode functionNode)
    {
        
    }
}
