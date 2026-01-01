using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tester;

public readonly struct TestAssert(bool noLocation, int line, int column, CompilerLogCode expectedLogCode)
{
    public readonly bool NoLocationAssert = noLocation;
    
    public readonly int Line = line;
    public readonly int Column = column;

    public readonly CompilerLogCode ExpectedLogCode = expectedLogCode;
}
