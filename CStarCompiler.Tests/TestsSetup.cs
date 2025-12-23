using CStarCompiler.Lexing;
using CStarCompiler.Logs;
using CStarCompiler.Parsing;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.SemanticAnalyze;

namespace CStarCompiler.Tests;

public partial class Tests
{
    private Lexer _lexer;
    private Parser _parser;
    private SemanticAnalyzer _semanticAnalyzer;

    [SetUp]
    public void Setup()
    {
        _lexer = new();
        _parser = new();
        _semanticAnalyzer = new();
    }

    private void TestAnalyze(string code)
    {
        var tokens = _lexer.Tokenize(code);
        var ast = _parser.Parse(tokens, "TEST");

        if (ast == null)
        {
            CompilerLogger.DumpError(CompilerLogCode.Expect, new Token(TokenType.Eof, 0, 0, ""));
            return;
        }
        
        _semanticAnalyzer.Clear();
        _semanticAnalyzer.Analyze([ast]);
        _semanticAnalyzer.PrintGraph();
    }
    
    private void TestAnalyze(params string[] codes)
    {
        var modules = new List<ModuleNode>();
        
        foreach (var code in codes)
        {
            var tokens = _lexer.Tokenize(code);

            var ast = _parser.Parse(tokens, "TEST");
            
            if (ast == null)
            {
                CompilerLogger.DumpError(CompilerLogCode.Expect, new Token(TokenType.Eof, 0, 0, ""));
                continue;
            }
            
            modules.Add(ast);
        }
        
        _semanticAnalyzer.Clear();
        _semanticAnalyzer.Analyze(modules);
        _semanticAnalyzer.PrintGraph();
    }
    
    private static void AssertLogNoErrors()
    {
        if (!CompilerLogger.HaveErrors()) Assert.Pass();
        
        CompilerLogger.WriteLogs();
        Assert.Fail();
    }

    private static void AssertLog(CompilerLogCode code)
    {
        Assert.That(CompilerLogger.HaveErrorCode(code));
        CompilerLogger.WriteLogs();
    }

    private static void AssertLog(CompilerLogCode code, string locationValue)
    {
        Assert.That(CompilerLogger.HaveErrorCode(code, locationValue));
        CompilerLogger.WriteLogs();
    }
}
