namespace CStarCompiler.Tests;

public partial class StructTests
{
    private partial class Parsing
    {
        [Test]
        public void DeclarationGood()
            => Tester.AnalyzeWithoutErrors(@"
module Main;

struct MyStruct
{

}
");

        [Test]
        public void DeclarationBadWithoutName()
            => Tester.ParseWithError(@"
module Main;

struct 
{

}
");

        [Test]
        public void DeclarationBadWithoutOpenBrace()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct


}
");

        [Test]
        public void DeclarationBadWithoutCloseBrace()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct
{


");
    }
}