using System.Collections.Generic;
using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Declarations;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.SemanticAnalyze.Units;
using CStarCompiler.SemanticAnalyze.Units.Module;
using CStarCompiler.SemanticAnalyze.Units.Type;
using CStarCompiler.Shared.Logs;

namespace CStarCompiler.SemanticAnalyze;

public sealed class SemanticAnalyzer
{
    private readonly SemanticContext _context = new();
    
    public void Analyze(List<ModuleNode> modules)
    {
        // collect data
        foreach (var module in modules)
        {
            _context.ModuleTable.AddModule(module);

            foreach (var declaration in module.Declarations)
            {
                switch (declaration)
                {
                    case FieldDeclarationNode fieldDeclarationNode:
                        // todo
                        break;
                    case FunctionDeclarationNode functionDeclarationNode:
                        // todo
                        break;
                    case StructDeclarationNode structDeclarationNode:
                        _context.StructTable.AddStruct(module.Identifier.Name, structDeclarationNode);
                        break;
                }
            }
        }
        
        // analyze modules
        _context.ModuleTable.AnalyzeImports();
        
        // analyze structs
        _context.StructTable.AnalyzeFieldsTypes();
        _context.StructTable.AnalyzeTypeDependencies();
        
        _context.StructTable.ExcludeUnusedStructs();
    }
    
    public void Clear()
    {
        _context.StructTable.Clear();
        _context.ModuleTable.Clear();
        _context.LocationTable.Clear();
        _context.ScopeTable.Clear();
    }
}
