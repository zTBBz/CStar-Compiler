using System.Collections.Generic;
using CStarCompiler.Lexing;
using CStarCompiler.Parsing;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.SemanticAnalyze;
using CStarCompiler.Shared.Logs;
using NUnit.Framework;

namespace CStarCompiler.Tests;

public static class Tester
{
    private static readonly Lexer _lexer = new();
    private static readonly Parser _parser = new();
    private static readonly SemanticAnalyzer _semanticAnalyzer = new();

    public static void ParseWithoutErrors(string code)
    {
        Parse(code);
        AssertNoErrors();
    }
    
    public static void ParseWithError(string code)
    {
        Parse(code);
        AssertCode(CompilerLogCode.ParserExpectToken);
    }

    public static void AnalyzeWithoutErrors(string code)
    {
        Analyze(code);
        AssertNoErrors();
    }
    
    public static void AnalyzeWithoutErrors(params string[] codes)
    {
        Analyze(codes);
        AssertNoErrors();
    }
    
    public static void AnalyzeWithCode(CompilerLogCode logCode, string code)
    {
        Analyze(code);
        AssertCode(logCode);
    }
    
    public static void AnalyzeWithCode(CompilerLogCode logCode, params string[] codes)
    {
        Analyze(codes);
        AssertCode(logCode);
    }
    
    private static void Parse(string code) => _parser.Parse(_lexer.Tokenize("TEST.cstar", code));

    private static void Analyze(string code)
    {
        var tokens = _lexer.Tokenize("TEST.cstar", code);
        var ast = _parser.Parse(tokens);
        
        _semanticAnalyzer.Clear();
        if (ast != null) _semanticAnalyzer.Analyze([ast]);
    }
    
    private static void Analyze(params string[] codes)
    {
        var modules = new List<ModuleNode>();
        
        foreach (var code in codes)
        {
            var tokens = _lexer.Tokenize("TEST.cstar", code);

            var ast = _parser.Parse(tokens);
            
            if (ast != null) modules.Add(ast);
        }
        
        _semanticAnalyzer.Clear();
        _semanticAnalyzer.Analyze(modules);
    }
    
    private static void AssertNoErrors()
    {
        var errors = CompilerLogger.HaveErrors(); 
        Assert.That(!errors);
        CompilerLogger.Clear();
    }

    private static void AssertCode(CompilerLogCode code)
    {
        var error = CompilerLogger.HaveLogCode(code); 
        CompilerLogger.WriteLogs();
        Assert.That(error);
    }

    private static void AssertCode(CompilerLogCode code, string locationValue)
    {
        var error = CompilerLogger.HaveLogCode(code, locationValue); 
        CompilerLogger.WriteLogs();
        Assert.That(error);
    }
}
