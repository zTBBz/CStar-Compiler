namespace CStarCompiler.Tests;

public partial class ModuleTests
{
    [Test]
    public void ModuleEmpty()
        => Tester.AnalyzeWithoutErrors(@"
module Main;
");
}