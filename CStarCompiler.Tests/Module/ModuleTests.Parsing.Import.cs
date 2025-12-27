namespace CStarCompiler.Tests;

public partial class ModuleTests
{
    private partial class Parsing
    {
        [Test]
        public void ImportDeclarationGood()
            => Tester.ParseWithoutErrors(@"
module Main;

use Some;
");

        [Test]
        public void ImportDeclarationBadWithoutName()
            => Tester.ParseWithError(@"
module Main;

use 
");

        [Test]
        public void ImportDeclarationBadWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main;

use Some
");
    }
}