namespace CStarCompiler.Tests.Module;

public partial class ModuleTests
{
    [Test]
    public static void ModuleEmpty()
        => Tester.AnalyzeWithoutErrors(@"
module Main;
");

    [Test]
    public static void ModuleWithSubname()
        => Tester.AnalyzeWithoutErrors(@"
module Main.Sub;
");

    [Test]
    public static void ModuleWithDeepSubname()
        => Tester.AnalyzeWithoutErrors(@"
module Main.Sub.Deep.Module;
");
}
