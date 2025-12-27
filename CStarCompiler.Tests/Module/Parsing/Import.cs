namespace CStarCompiler.Tests.Module.Parsing;

public partial class ModuleTests
{
        [Test]
        public void ImportGoodDeclaration()
            => Tester.ParseWithoutErrors(@"
module Main;

use Some;
");

        [Test]
        public void ImportBadDeclarationWithoutName()
            => Tester.ParseWithError(@"
module Main;

use 
");

        [Test]
        public void ImportBadDeclarationWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main;

use Some
");
        /*
        [Test]
        public void ImportBadDeclarationInFunctionBody()
            => Tester.ParseWithError(@"
module Main;

void Function()
{
    use Some;
}
");*/
        
        [Test]
        public void ImportBadDeclarationInStructBody()
            => Tester.ParseWithError(@"
module Main;

struct Some
{
    use Some;
}
");
        
        [Test]
        public void ImportGoodDeclarationAfterMemberDeclaration()
            => Tester.ParseWithoutErrors(@"
module Main;

struct int {}

void Some();

use Some;
");
}
