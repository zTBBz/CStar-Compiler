namespace CStarCompiler.Tests.Struct.Parsing;

public partial class StructTests
{
        [Test]
        public void GoodDeclarationField()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    int Field;
}
");

        [Test]
        public void BadDeclarationFieldWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct
{
    int Field
}
");

        [Test]
        public void BadDeclarationFieldWithoutName()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct
{
    int 
}
");
}