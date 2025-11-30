using CStarCompiler;
using CStarCompiler.CodeGeneration;
using CStarCompiler.Lexing;
using CStarCompiler.Parsing;

var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.cstar");

var lexer = new Lexer();
var parser = new Parser();
var analyzer = new MonomorphizationAnalyzer(); // 1. Создаем анализатор
var generator = new CodeGenerator();

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
    
    Console.WriteLine($"Parsed Module: {ast.ModuleName}");
    Console.WriteLine($"Declarations: {ast.Declarations.Count}");

    // 2. Запускаем этап анализа для сбора использования дженериков
    // Анализатор заполнит свои словари (StructUsages, FunctionUsages)
    analyzer.Analyze(ast);
    Console.WriteLine("Analysis completed.");

    // 3. Передаем анализатор в генератор
    var unit = generator.GenerateUnit(ast, analyzer);

    FileWriter.WriteToDisk(unit, Path.Combine(Directory.GetCurrentDirectory(), "output"));
    Console.WriteLine("Code generated successfully.\n");
}

Console.WriteLine("Compilation finished.");
Console.ReadLine();
