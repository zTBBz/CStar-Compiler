namespace CStarCompiler.Tests.Module;

public partial class ModuleTests
{
    [Test]
    public static void ModuleEmpty()
        => Tester.AnalyzeWithoutErrors(@"
module Main;
");
}
