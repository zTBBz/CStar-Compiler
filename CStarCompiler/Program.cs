using CStarCompiler;
using CStarCompiler.CodeGeneration;
using CStarCompiler.Lexing;
using CStarCompiler.Parsing;

var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.cstar");

var lexer = new Lexer();
var parser = new Parser();
var generator = new CodeGenerator();
var writer = new ProjectWriter();

foreach (var file in files)
{
    Console.WriteLine($"Compiling: {Path.GetFileName(file)}...");
    
    var sourceCode = File.ReadAllText(file);
    var tokens = lexer.Tokenize(sourceCode);
    
    if (!LexemAnalyser.Validate(tokens, file)) 
    {
        Console.WriteLine("Lexing failed.");
        continue;
    }

    var ast = parser.Parse(tokens, file);
    
    if (ast == null)
    {
        Console.WriteLine("Parsing failed.");
        continue;
    }

    var unit = generator.GenerateUnit(ast);

    writer.AddCompilationUnit(unit);
    Console.WriteLine("Code generated successfully.\n");
}

writer.WriteToDisk(Path.Combine(Directory.GetCurrentDirectory(), ".output"));

Console.WriteLine("Compilation finished.");
Console.ReadLine();
