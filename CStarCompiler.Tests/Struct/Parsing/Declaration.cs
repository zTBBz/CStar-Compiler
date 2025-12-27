namespace CStarCompiler.Tests.Struct.Parsing;

public partial class StructTests
{
        [Test]
        public void GoodDeclaration()
            => Tester.AnalyzeWithoutErrors(@"
module Main;

struct MyStruct
{

}
");

        [Test]
        public void BadDeclarationWithoutName()
            => Tester.ParseWithError(@"
module Main;

struct 
{

}
");

        [Test]
        public void BadDeclarationWithoutOpenBrace()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct


}
");

        [Test]
        public void BadDeclarationWithoutCloseBrace()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct
{


");
}
