using CStarCompiler.Lexing;
using CStarCompiler.Parsing;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.SemanticAnalyze;
using CStarCompiler.Shared.Logs;
using CStarCompiler.Tester;

var directory = args.Length == 0 ? Directory.GetCurrentDirectory() : Path.GetFullPath(args[0]);

var files = Directory.GetFiles(directory, "*.cstar", SearchOption.AllDirectories);

var lexer = new Lexer();
var parser = new Parser();
var semanticAnalyzer = new SemanticAnalyzer();

var preprocessor = new TestPreprocessor();

foreach (var file in files)
{
    var source = File.ReadAllText(file);
    var fileName = Path.GetRelativePath(directory, file);

    var testFile = preprocessor.Process(fileName, source);
    
    // no sub files
    if (testFile.SubFiles.Count == 1)
    {
        var (subFileName, sourceCode, asserts) = testFile.SubFiles[0];
        
        var tokens = lexer.Tokenize(subFileName, sourceCode);
        var module = parser.Parse(tokens);

        var modules = new List<ModuleNode>();
        if (module != null) modules.Add(module);

        semanticAnalyzer.Analyze(modules);
        
        // assert and write logs
        bool status;
        
        switch (asserts.Count)
        {
            // expect no logs
            case 0:
                status = !CompilerLogger.HaveErrors(subFileName);
                TestLogger.WriteNoErrors(status, subFileName);
                break;
            
            // expect one log
            case 1:
            {
                var assert = asserts[0];
                
                status = assert.AssertHaveLog(subFileName);
                TestLogger.WriteSingle(status, subFileName, assert);
                break;
            }
            
            // expect many logs
            default:
                var statusAsserts = new List<(bool, TestAssert)>();
                
                foreach (var assert in asserts)
                {
                    status = assert.AssertHaveLog(subFileName);
                    statusAsserts.Add((status, assert));
                }
                
                TestLogger.WriteMany(subFileName, statusAsserts);
                break;
        }
    }
    // with sub files
    else
    {
        foreach (var (subFileName, sourceCode, _) in testFile.SubFiles)
        {
            var tokens = lexer.Tokenize(subFileName, sourceCode);
            var module = parser.Parse(tokens);

            var modules = new List<ModuleNode>();
            if (module != null) modules.Add(module);

            semanticAnalyzer.Analyze(modules);
        }
        
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"[SUB FILES] [TOTAL {testFile.SubFiles.Count}] ");
            
        Console.ResetColor();
        Console.WriteLine($"{testFile.TestFileName}");

        // assert and write logs 
        foreach (var (subFileName, _, asserts) in testFile.SubFiles)
        {
            bool status;
        
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("\t[SUBFILE] ");
            
            Console.ResetColor();
            Console.WriteLine($"{subFileName}");
            
            switch (asserts.Count)
            {
                // expect no logs
                case 0:
                    status = !CompilerLogger.HaveErrors(subFileName);
                    
                    TestLogger.WriteNoErrors(status, subFileName, "\t\t");
                    break;
            
                // expect one log
                case 1:
                {
                    var assert = asserts[0];
                    status = assert.AssertHaveLog(subFileName);
                    
                    TestLogger.WriteSingle(status, subFileName, assert, "\t\t");
                    break;
                }
            
                // expect many logs
                default:
                    var statusAsserts = new List<(bool, TestAssert)>();
                
                    foreach (var assert in asserts)
                    {
                        status = assert.AssertHaveLog(subFileName);
                        statusAsserts.Add((status, assert));
                    }
                
                    TestLogger.WriteMany(subFileName, statusAsserts, "\t\t");
                    break;
            }
        }
    }
    
    semanticAnalyzer.Clear();
    CompilerLogger.Clear();
}

Console.ReadLine();
