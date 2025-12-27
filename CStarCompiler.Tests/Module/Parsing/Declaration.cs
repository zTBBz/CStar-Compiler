namespace CStarCompiler.Tests.Module.Parsing;

public partial class ModuleTests
{
        [Test]
        public void ModuleBadDeclaration()
            => Tester.ParseWithError("");

        [Test]
        public void ModuleBadDeclarationWithoutName()
            => Tester.ParseWithError(@"
module 
");

        [Test]
        public void ModuleGoodDeclaration()
            => Tester.ParseWithoutErrors(@"
module Main;
");

        [Test]
        public void ModuleBadDeclarationWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main
");

        [Test]
        public void ModuleGoodDeclarationWithSubName()
            => Tester.ParseWithoutErrors(@"
module Main.CoolMain;
");

        [Test]
        public void ModuleBadDeclarationWithSubNameWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main.CoolMain
");
}