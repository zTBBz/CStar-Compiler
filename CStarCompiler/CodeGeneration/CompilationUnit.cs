namespace CStarCompiler.CodeGeneration;

public sealed class CompilationUnit(string moduleName, string headerCode, string sourceCode)
{
    public readonly string ModuleName = moduleName;
    public readonly string HeaderCode = headerCode; 
    public readonly string SourceCode = sourceCode;  
}
