namespace CStarCompiler.Tests;

public partial class ModuleTests
{
    private partial class Parsing
    {
        [Test]
        public void ModuleDeclarationBad()
            => Tester.ParseWithError("");

        [Test]
        public void ModuleDeclarationBadWithoutName()
            => Tester.ParseWithError(@"
module 
");

        [Test]
        public void ModuleDeclarationGood()
            => Tester.ParseWithoutErrors(@"
module Main;
");

        [Test]
        public void ModuleDeclarationBadWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main
");

        [Test]
        public void ModuleDeclarationGoodWithSubName()
            => Tester.ParseWithoutErrors(@"
module Main.CoolMain;
");

        [Test]
        public void ModuleDeclarationBadWithSubNameWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main.CoolMain
");
    }
}