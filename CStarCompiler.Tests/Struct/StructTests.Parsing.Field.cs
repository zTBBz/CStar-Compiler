namespace CStarCompiler.Tests;

public partial class StructTests
{
    private partial class Parsing
    {
        [Test]
        public void DeclarationGoodField()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    int Field;
}
");

        [Test]
        public void DeclarationBadFieldWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct
{
    int Field
}
");

        [Test]
        public void DeclarationBadFieldWithoutName()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct
{
    int 
}
");
    }
}