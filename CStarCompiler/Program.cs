using CStarCompiler.Lexing;
using CStarCompiler.Parsing;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.SemanticAnalyze;
using CStarCompiler.Shared.Logs;

var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.cstar");

var lexer = new Lexer();
var parser = new Parser();
var ast = new List<ModuleNode>();

var fail = false;

foreach (var file in files)
{
    var sourceCode = File.ReadAllText(file);
    var tokens = lexer.Tokenize(file, sourceCode);

    var module = parser.Parse(tokens);
    
    if (module != null) ast.Add(module);
    else fail = true;
}

if (!fail)
{
    var semanticAnalyzer = new SemanticAnalyzer();
    semanticAnalyzer.Analyze(ast);
    
    CompilerLogger.WriteLogs();
}

Console.ReadLine();

// writer.WriteToDisk(Path.Combine(Directory.GetCurrentDirectory(), ".output"));
