using CStarCompiler.Lexing;
using CStarCompiler.Logs;
using CStarCompiler.Parsing.Nodes.Declarations;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.SemanticAnalyze.Declarations;
using CStarCompiler.SemanticAnalyze.Units;
using CStarCompiler.SemanticAnalyze.Units.Module;
using CStarCompiler.SemanticAnalyze.Units.Type;

namespace CStarCompiler.SemanticAnalyze;

public sealed class SemanticAnalyzer
{
    private readonly SemanticContext _context = new();
    
    private string _currentModule = string.Empty;
    
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
                        _context.StructTable.AddStruct(module.ModuleName, structDeclarationNode);
                        break;
                }
            }
        }
        
        // analyze modules
        _context.ModuleTable.AnalyzeImports();
        _context.ModuleTable.AnalyzeDependencies();
    }

    public void PrintGraph() => _context.ModuleTable.PrintGraph();
    
    public void Clear()
    {
        _currentModule = string.Empty;
        _context.StructTable.Clear();
        _context.ModuleTable.Clear();
        _context.LocationTable.Clear();
    }
}
