using CStarCompiler.Lexing;
using CStarCompiler.Parsing;
using CStarCompiler.SemanticAnalyze;
using CStarCompiler.Shared.Logs;
using CStarCompiler.Tester;

var directory = args.Length == 0 ? Directory.GetCurrentDirectory() : Path.GetFullPath(args[0]);

var files = Directory.GetFiles(directory, "*.cstar", SearchOption.AllDirectories);

var lexer = new Lexer();
var parser = new Parser();
var semanticAnalyzer = new SemanticAnalyzer();

// todo: add many modules in one file by 'FILE' directive

var preprocessor = new TestPreprocessor();

var totalCount = 0;
var totalFailedCount = 0;
var totalOkCount = 0;

foreach (var file in files)
{
    var source = File.ReadAllText(file);
    var fileName = Path.GetRelativePath(directory, file);
    
    var tokens = lexer.Tokenize(fileName, source);
    var module = parser.Parse(tokens);

    if (module != null)
    {
        semanticAnalyzer.Clear();
        semanticAnalyzer.Analyze([module]);
    }
    
    var asserts = preprocessor.Process(file, source);
    
    totalCount += 1;
    
    string status;

    switch (asserts.Count)
    {
        // expect no logs
        case 0:
        {
            var failed = false;
            
            if (CompilerLogger.HaveErrors(fileName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                status = "[FAIL]";
                totalFailedCount += 1;
                failed = true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                status = "[OK]";
                totalOkCount += 1;
            }

            Console.Write(status);
            Console.ResetColor();
            Console.WriteLine($" [NoErrors] {fileName}");
            
            if (failed) CompilerLogger.WriteLogs();

            break;
        }
        
        // expect one log
        case 1:
        {
            var assert = asserts[0];
            var failed = false;
            
            if (AssertTest(fileName, assert))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                status = "[OK]";
                totalOkCount += 1;
            }
            else
            {                
                Console.ForegroundColor = ConsoleColor.Red;
                status = "[FAIL]";
                totalFailedCount += 1;
                failed = true;
            }

            Console.Write(status);
            Console.ResetColor();

            Console.WriteLine(assert.NoLocationAssert
                ? $" [{assert.ExpectedLogCode}] {fileName}"
                : $" [{assert.ExpectedLogCode}] {fileName}:{assert.Line}:{assert.Column}");
            
            if (failed) CompilerLogger.WriteLogs();

            break;
        }
        
        // expect logs
        default:
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine($"[TOTAL {asserts.Count}] {fileName}");

            var failed = false;
            
            foreach (var assert in asserts)
            {
                if (AssertTest(fileName, assert))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    status = "\t[OK]";
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    status = "\t[FAIL]";
                    failed = true;
                }

                Console.Write(status);
                Console.ResetColor();
                
                Console.WriteLine(assert.NoLocationAssert
                    ? $" [{assert.ExpectedLogCode}]"
                    : $" [{assert.ExpectedLogCode}] {assert.Line}:{assert.Column}");
                
                if (failed) CompilerLogger.WriteLogs();
            }
            
            _ = failed ? totalFailedCount += 1 : totalOkCount += 1;

            break;
        }
    }
    
    CompilerLogger.Clear();
}

Console.ForegroundColor = ConsoleColor.Blue;

Console.WriteLine();
Console.WriteLine($"Ok: {totalOkCount}");
Console.WriteLine($"Failed: {totalFailedCount}");
Console.WriteLine($"Total: {totalCount}");

Console.ResetColor();
Console.ReadLine();

bool AssertTest(string fileName, TestAssert assert)
    => assert.NoLocationAssert ?
        CompilerLogger.HaveLogCode(fileName, assert.ExpectedLogCode) 
        : CompilerLogger.HaveLogCode(fileName, assert.Line, assert.Column, assert.ExpectedLogCode);
