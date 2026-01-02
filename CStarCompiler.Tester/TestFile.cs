namespace CStarCompiler.Tester;

public readonly struct TestFile(string testFileName)
{
    public readonly string TestFileName = testFileName;
    
    public List<(string FileName, string SourceCode, List<TestAssert> Asserts)> SubFiles { get; } = [];

    public void AddTestSubFile(string subFileName, string sourceCode, List<TestAssert> asserts)
        => SubFiles.Add((subFileName, sourceCode, asserts));
}
