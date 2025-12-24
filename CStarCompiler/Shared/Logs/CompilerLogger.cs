using CStarCompiler.Lexing;

namespace CStarCompiler.Shared.Logs;

public static class CompilerLogger
{
    private static readonly Dictionary<string, List<CompilerLog>> _logDump = [];
    private static string _currentFile = string.Empty;
    
    public static void SetFile(string file) => _currentFile = file;
    
    public static void DumpInfo(CompilerLogCode compilerLogCode, Token location, string? info = null)
        => DumpLog(CompilerLogLevel.Info, location, compilerLogCode, info);
    
    public static void DumpWarning(CompilerLogCode compilerLogCode, Token location, string? info = null)
        => DumpLog(CompilerLogLevel.Warning, location, compilerLogCode, info);
    
    public static void DumpError(CompilerLogCode compilerLogCode, Token location, string? info = null)
        => DumpLog(CompilerLogLevel.Error, location, compilerLogCode, info);
    
    private static void DumpLog(CompilerLogLevel level, Token location, CompilerLogCode compilerLogCode, string? info)
    {
        var log = new CompilerLog(level, location, GetMessage(compilerLogCode), compilerLogCode, info);
        
        if (_logDump.TryGetValue(_currentFile, out var dump)) dump.Add(log);
        else _logDump.Add(_currentFile, [log]);
    }

    private static string GetMessage(CompilerLogCode compilerLogCode)
        => compilerLogCode switch
        {
            CompilerLogCode.ParserUnknownToken => "Unknown token",
            CompilerLogCode.ParserExpectToken => "Parser expects token",
            
            CompilerLogCode.ModuleImportNotExisted => "Module import not exist",
            CompilerLogCode.ModuleImportRecursive => "Recursive module imports", // todo: remove
            CompilerLogCode.ModuleImportDuplicate => "Module import duplicate",
            CompilerLogCode.ModuleDeclarationDuplicate => "Module declaration duplicate",
            CompilerLogCode.ModuleImportSelf => "Module import self",
            
            CompilerLogCode.TypeNotFound => "Cannot find type declaration",
            
            CompilerLogCode.StructDeclarationDuplicate => "Struct declaration duplicate",
            
            CompilerLogCode.StructFieldRecursive => "Struct recursive field",
            CompilerLogCode.StructFieldNameDuplicate => "Field name duplicate",
            CompilerLogCode.StructFieldNameShadowStructType => "Field name shadow declared struct type",
        };
    
    public static void WriteLogs()
    {
        foreach (var pair in _logDump)
        {
#if DEBUG
            Console.WriteLine($"File: Debug");
#else
            Console.WriteLine($"File: {Path.GetFullPath(pair.Key)}");
#endif
            
            // write infos
            var infos = pair.Value.Where(l => l.Level == CompilerLogLevel.Info);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            foreach (var info in infos)
            {
                var message = $"[INFO]: {info.Message}. '{info.Location.Value}' at line {info.Location.Line}, column {info.Location.Column}. {info.Info}";
                
                Console.WriteLine(message);
            }
            
            // write warnings
            var warnings = pair.Value.Where(l => l.Level == CompilerLogLevel.Warning);
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var warning in warnings) Console.WriteLine(warning.Message);
            
            // write errors
            var errors = pair.Value.Where(l => l.Level == CompilerLogLevel.Error);
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var error in errors)
            {
                var message = $"[ERROR]: {error.Message}. '{error.Location.Value}' at line {error.Location.Line}, column {error.Location.Column}. {error.Info}";
                
                Console.WriteLine(message);
            }
            
            Console.ResetColor();
        }
        
        _logDump.Clear();
    }
    
    public static void Clear() => _logDump.Clear();
    
#if DEBUG // functions for tests
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
#endif
    
    private readonly struct CompilerLog(CompilerLogLevel level, Token location, string message, CompilerLogCode compilerLogCode, string? info = null)
    {
        public readonly CompilerLogCode CompilerLogCode = compilerLogCode;
    
        public readonly string Message = message;
        public readonly string? Info = info;

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
