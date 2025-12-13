namespace CStarCompiler.CodeGeneration;

public sealed class CompilationUnit(string moduleName, string sourceCode)
{
    public readonly string ModuleName = moduleName;
    public readonly string SourceCode = sourceCode;  
}
