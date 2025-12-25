using CStarCompiler.Lexing;

namespace CStarCompiler.Shared.Logs;

public static class CompilerLogger
{
    private static readonly Dictionary<string, List<CompilerLog>> _logDump = [];
    
    public static void DumpInfo(CompilerLogCode compilerLogCode, Token location, string message, string? hint = null)
        => DumpLog(CompilerLogLevel.Info, location, compilerLogCode, message, hint);
    
    public static void DumpWarning(CompilerLogCode compilerLogCode, Token location, string message, string? hint = null)
        => DumpLog(CompilerLogLevel.Warning, location, compilerLogCode, message, hint);
    
    public static void DumpError(CompilerLogCode compilerLogCode, Token location, string message, string? hint = null)
        => DumpLog(CompilerLogLevel.Error, location, compilerLogCode, message, hint);
    
    private static void DumpLog(CompilerLogLevel level, Token location, CompilerLogCode compilerLogCode, string message, string? hint)
    {
        var log = new CompilerLog(level, location, message, compilerLogCode, hint);
        
        if (_logDump.TryGetValue(location.File, out var dump)) dump.Add(log);
        else _logDump.Add(location.File, [log]);
    }

    private static string FormatLogMessage(CompilerLog log)
    {
        var infoString = log.Hint == null ? "" : $"\n\t{log.Hint}";
        var logType = log.Level switch 
        {
            CompilerLogLevel.Info => "INFO",
            CompilerLogLevel.Warning => "WARNING",
            CompilerLogLevel.Error => "ERROR",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        return $"[{logType}] {FormatLocation(log.Location)} \n\t{log.Message} {infoString}\n";
    }
    
    public static string FormatLocation(Token location)
        => $"{location.File}:{location.Line}:{location.Column}";

    public static void WriteLogs()
    {
        foreach (var pair in _logDump)
        {
            // write infos
            var infos = pair.Value.Where(l => l.Level == CompilerLogLevel.Info);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            foreach (var info in infos) Console.WriteLine(FormatLogMessage(info));
            
            // write warnings
            var warnings = pair.Value.Where(l => l.Level == CompilerLogLevel.Warning);
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var warning in warnings) Console.WriteLine(FormatLogMessage(warning));
            
            // write errors
            var errors = pair.Value.Where(l => l.Level == CompilerLogLevel.Error);
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var error in errors) Console.WriteLine(FormatLogMessage(error));
            
            Console.ResetColor();
        }
        
        _logDump.Clear();
    }
    
    public static void Clear() => _logDump.Clear();
    
    // functions for tests
    public static bool HaveLogs()
        => _logDump.Any(p => p.Value.Count != 0);

    public static bool HaveErrors()
        => _logDump.Select(l => l.Value)
            .Any(v => v.Any(l => l.Level ==  CompilerLogLevel.Error));
    
    public static bool HaveLogCode(CompilerLogCode code)
        => _logDump.SelectMany(p => p.Value).Any(l => l.CompilerLogCode == code);
    
    public static bool HaveLogCode(CompilerLogCode code, string locationValue)
        => _logDump.SelectMany(p => p.Value)
            .Any(l => l.CompilerLogCode == code && l.Location.Value == locationValue);
    
    private readonly struct CompilerLog(CompilerLogLevel level, Token location, string message, CompilerLogCode compilerLogCode, string? hint = null)
    {
        public readonly CompilerLogCode CompilerLogCode = compilerLogCode;
    
        public readonly string Message = message;
        public readonly string? Hint = hint;

        public readonly Token Location = location;
        public readonly CompilerLogLevel Level = level;
    }
    
    private enum CompilerLogLevel : byte
    {
        Info,
        Warning,
        Error
    }
}
