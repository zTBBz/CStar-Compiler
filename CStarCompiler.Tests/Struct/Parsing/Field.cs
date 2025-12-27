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

        [Test]
        public void GoodDeclarationFieldPublic()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    public int Field;
}
");

        [Test]
        public void GoodDeclarationFieldInternal()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    internal int Field;
}
");

        [Test]
        public void GoodDeclarationFieldPointer()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    int* Field;
}
");

        [Test]
        public void GoodDeclarationFieldArray()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    int[] Field;
}
");

        [Test]
        public void GoodDeclarationFieldPointerArray()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    int*[] Field;
}
");

        [Test]
        public void GoodDeclarationFieldArrayPointer()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    int[]* Field;
}
");

        [Test]
        public void GoodDeclarationFieldMixedWithNested()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    int Field1;
    struct Inner {}
    int Field2;
}
");

        [Test]
        public void GoodDeclarationFieldRefModifier()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    ref int Field;
}
");

        [Test]
        public void GoodDeclarationFieldNoWriteModifier()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    nowrite int Field;
}
");
}