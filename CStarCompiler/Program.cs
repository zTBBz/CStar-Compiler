using CStarCompiler;
using CStarCompiler.Lexing;
using CStarCompiler.Logs;
using CStarCompiler.Parsing;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.SemanticAnalyze;

var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.cstar");

var lexer = new Lexer();
var parser = new Parser();
var ast = new List<ModuleNode>();

var fail = false;

foreach (var file in files)
{
    var sourceCode = File.ReadAllText(file);
    var tokens = lexer.Tokenize(sourceCode);
    
    if (!LexemAnalyser.Validate(tokens, file)) 
    {
        Console.WriteLine("Lexing failed.");
        continue;
    }

    var module = parser.Parse(tokens, file);
    
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
