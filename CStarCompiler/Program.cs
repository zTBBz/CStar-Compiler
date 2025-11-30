using CStarCompiler;
using CStarCompiler.CodeGeneration;
using CStarCompiler.Lexing;
using CStarCompiler.Parsing;

var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*cstar");
var lexer = new Lexer();
var parser = new Parser();
var generator = new CodeGenerator();

foreach (var file in files)
{
    var tokens = lexer.Tokenize(File.ReadAllText(file));
    if (!LexemAnalyser.Validate(tokens, file)) continue;

    var ast = parser.Parse(tokens, file);
    
    if (ast == null) continue;
    
    Console.WriteLine($"Parsed Module: {ast.ModuleName}");
    Console.WriteLine($"Declarations: {ast.Declarations.Count}");
    Console.WriteLine("Success building AST.");
        
    var unit = generator.GenerateUnit(ast);

    FileWriter.WriteToDisk(unit, Path.Combine(Directory.GetCurrentDirectory(), "output"));
}

Console.ReadLine();
