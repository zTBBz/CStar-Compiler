namespace CStarCompiler.Tester;

public readonly struct TestFile(string fileName)
{
    public readonly string FileName = fileName;
    
    private readonly Dictionary<string, List<TestAssert>> _asserts = [];

    public void AddAsserts(string testFileName, List<TestAssert> asserts) => _asserts[testFileName] = asserts;

    public List<TestAssert> GetAsserts(string testFileName) => _asserts[testFileName];
}
