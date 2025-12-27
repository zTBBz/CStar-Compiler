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

        [Test]
        public void GoodDeclarationPublic()
            => Tester.ParseWithoutErrors(@"
module Main;

public struct MyStruct
{
}
");

        [Test]
        public void GoodDeclarationInternal()
            => Tester.ParseWithoutErrors(@"
module Main;

internal struct MyStruct
{
}
");

        [Test]
        public void GoodDeclarationNested()
            => Tester.ParseWithoutErrors(@"
module Main;

struct Outer
{
    struct Inner
    {
    }
}
");

        [Test]
        public void GoodDeclarationNestedDeep()
            => Tester.ParseWithoutErrors(@"
module Main;

struct Level1
{
    struct Level2
    {
        struct Level3
        {
        }
    }
}
");

        [Test]
        public void BadDeclarationNestedWithoutCloseBrace()
            => Tester.ParseWithError(@"
module Main;

struct Outer
{
    struct Inner
    {

}
");

        [Test]
        public void BadDeclarationDoubleStruct()
            => Tester.ParseWithError(@"
module Main;

struct struct MyStruct
{
}
");

        [Test]
        public void BadDeclarationWithBadToken()
            => Tester.ParseWithError(@"
module Main;

struct 123
{
}
");
}
