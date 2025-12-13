using CStarCompiler.Parsing.Nodes.Declarations;
using CStarCompiler.Parsing.Nodes.Modules;

namespace CStarCompiler.CodeGeneration;

public sealed class DublicateAnalyzer
{
    private readonly HashSet<StructDeclarationNode> _declaredStructs = [];
    private readonly HashSet<FieldDeclarationNode> _declaredGlobalVariables = [];
    private readonly HashSet<FunctionDeclarationNode> _declaredGlobalFunctions = [];
    
    private void Initialize()
    {
        _declaredStructs.Clear();
        _declaredGlobalVariables.Clear();
        _declaredGlobalFunctions.Clear();
    }

    public bool Analyze(ModuleNode module)
    {
        Initialize();
        
        const string msg = "[Error] ";
        Console.ForegroundColor = ConsoleColor.Red;
        
        var hadError = false;
        
        foreach (var declaration in module.Declarations)
        {
            switch (declaration)
            {
                case StructDeclarationNode structDeclaration:
                    if (!_declaredStructs.Add(structDeclaration))
                    {
                        hadError = true;
                        Console.WriteLine(msg + $"struct {structDeclaration.Name} already defined!");
                    }
                    break;
                case FieldDeclarationNode fieldDeclaration:
                    if (!_declaredGlobalVariables.Add(fieldDeclaration))
                    {
                        hadError = true;
                        Console.WriteLine(msg + $"global field '{fieldDeclaration.Type.Name} {fieldDeclaration.Name}' already defined!");
                    }
                    break;
                case FunctionDeclarationNode functionDeclaration:
                    if (!_declaredGlobalFunctions.Add(functionDeclaration))
                    {
                        hadError = true;
                        Console.WriteLine(msg + $"function '{functionDeclaration.ReturnType.Name} {functionDeclaration.Name}' already defined!");
                    }
                    break;
            }
        }
        
        Console.ResetColor();
        
        return !hadError;
    }
}
