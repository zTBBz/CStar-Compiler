using CStarCompiler.Lexing;

namespace CStarCompiler.Logs;

public static class CompilerLogger
{
    private static readonly Dictionary<string, List<CompilerLog>> _logDump = [];
    private static string _currentFile = string.Empty;
    
    public static void SetFile(string file) => _currentFile = file;
    
    public static void DumpInfo(CompilerLogCode compilerLogCode, Token location)
        => DumpLog(CompilerLogLevel.Info, location, compilerLogCode);
    
    public static void DumpWarning(CompilerLogCode compilerLogCode, Token location)
        => DumpLog(CompilerLogLevel.Warning, location, compilerLogCode);
    
    public static void DumpError(CompilerLogCode compilerLogCode, Token location)
        => DumpLog(CompilerLogLevel.Error, location, compilerLogCode);
    
    private static void DumpLog(CompilerLogLevel level, Token location, CompilerLogCode compilerLogCode)
    {
        var log = new CompilerLog(level, location, GetMessage(compilerLogCode), compilerLogCode);
        
        if (_logDump.TryGetValue(_currentFile, out var dump)) dump.Add(log);
        else _logDump.Add(_currentFile, [log]);
    }

    private static string GetMessage(CompilerLogCode compilerLogCode)
        => compilerLogCode switch
        {
            CompilerLogCode.ModuleImportNotExisted => "Cannot import not existed module: ",
            CompilerLogCode.ModuleRecursiveImport => "Cannot resolve cyclic module imports: ",
            CompilerLogCode.ModuleImportDuplicate => "Module import duplicate: ",
            
            CompilerLogCode.TypeNotExist => "Cannot find declaration for type: ",
            
            CompilerLogCode.StructDeclarationDuplicate => "Cannot duplicate struct declaration: ",
            CompilerLogCode.StructRecursiveField => "Struct cannot have recursive field: ",
            
            CompilerLogCode.StructFieldNameDuplicate => "Cannot duplicate field name: ",
        };
    
    public static void WriteLogs()
    {
        foreach (var pair in _logDump)
        {
            Console.WriteLine($"Module: {pair.Key}");
            
            // write infos
            var infos = pair.Value.Where(l => l.Level == CompilerLogLevel.Info);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var info in infos) Console.WriteLine(info.Message);
            
            // write warnings
            var warnings = pair.Value.Where(l => l.Level == CompilerLogLevel.Warning);
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var warning in warnings) Console.WriteLine(warning.Message);
            
            // write errors
            var errors = pair.Value.Where(l => l.Level == CompilerLogLevel.Error);
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var error in errors)
            {
                // todo: add '~~~~' below Token.Value
                var message = $"[ERROR]: {error.Message}. '{error.Location.Value}' at line {error.Location.Line}, column {error.Location.Column}";
                
                Console.WriteLine(message);
            }
            
            Console.ResetColor();
        }
    }
    
    public static void Clear() => _logDump.Clear();
    
#if DEBUG // functions for tests
    public static bool HaveErrors()
        => _logDump.Any(p => p.Value.Count != 0);
    
    public static bool HaveErrorCode(CompilerLogCode code)
        => _logDump.SelectMany(p => p.Value).Any(l => l.CompilerLogCode == code);
    
    public static bool HaveErrorCode(CompilerLogCode code, string locationValue)
        => _logDump.SelectMany(p => p.Value)
            .Any(l => l.CompilerLogCode == code && l.Location.Value == locationValue);
#endif
    
    private readonly struct CompilerLog(CompilerLogLevel level, Token location, string message, CompilerLogCode compilerLogCode)
    {
        public readonly CompilerLogCode CompilerLogCode = compilerLogCode;
    
        public readonly string Message = message;
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
