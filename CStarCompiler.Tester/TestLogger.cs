using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tester;

public static class TestLogger
{
    public static void WriteMany(string fileName, List<(bool, TestAssert)> asserts, string? prefix = null)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;

        if (prefix != null) Console.Write(prefix);
        Console.Write($"[MULTIPLY ASSERTS] [TOTAL {asserts.Count}] ");
        
        Console.ResetColor();
        Console.WriteLine($"{fileName}");
        
        foreach (var (status, assert) in asserts)
        {
            if (prefix != null) Console.Write(prefix);
            WriteSingle(status, fileName, assert, "\t");
        }
    }
    
    public static void WriteSingle(bool status, string fileName, TestAssert assert, string? prefix = null)
    {
        string statusString;

        if (status)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            statusString = "[OK]";
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            statusString = "[FAIL]";
        }
        
        if (prefix != null) Console.Write(prefix);
        
        Console.Write(statusString);
        Console.ResetColor();
        Console.WriteLine(assert.NoLocationAssert
            ? $" [{assert.ExpectedLogCode}] {fileName}"
            : $" [{assert.ExpectedLogCode}] {fileName}:{assert.Line}:{assert.Column}");
        
        if (!status) CompilerLogger.WriteLogs();
    }
    
    public static void WriteNoErrors(bool status, string fileName, string? prefix = null)
    {
        string statusString;

        if (status)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            statusString = "[OK]";
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            statusString = "[FAIL]";
        }
        
        if (prefix != null) Console.Write(prefix);
        
        Console.Write(statusString);
        Console.ResetColor();
        Console.WriteLine($" [NoErrors] {fileName}");

        if (!status) CompilerLogger.WriteLogs();
    }
}
