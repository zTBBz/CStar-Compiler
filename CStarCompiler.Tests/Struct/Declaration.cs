using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests.Struct;

public partial class StructTests
{
    [Test]
    public void DeclarationDuplicate()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructDeclarationDuplicate, @"
module Main;

struct MyStruct
{

}

struct MyStruct
{

}
");

    [Test]
    public void DeclarationUnused()
        => Tester.AnalyzeWithCode(CompilerLogCode.TypeUnused, @"
module Main;

struct MyStruct
{
     
}
");

    [Test]
    public void DeclarationEmpty()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

struct int {}

struct MyStruct
{
    int Field;
}
");

    [Test]
    public void DeclarationPublic()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

struct int {}

public struct MyStruct
{
    int Field;
}
");

    [Test]
    public void DeclarationInternal()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

struct int {}

internal struct MyStruct
{
    int Field;
}
");

    [Test]
    public void DeclarationNested()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

struct Outer
{
    Inner Field;

    struct Inner
    {
    }
}
");

    [Test]
    public void DeclarationNestedMultiple()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

struct Outer
{
    Inner1 Field1;
    Inner2 Field2;

    struct Inner1 {}
    struct Inner2 {}
}
");

    [Test]
    public void DeclarationNestedDeep()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

struct Level1
{
    Level2 Field;

    struct Level2
    {
        Level3 Field;

        struct Level3 {}
    }
}
");

    [Test]
    public void DeclarationNestedDuplicate()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructDeclarationDuplicate, @"
module Main;

struct Outer
{
    struct Inner {}
    struct Inner {}
}
");

    [Test]
    public void DeclarationMultiple()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

struct int {}

struct First
{
    int Field;
}

struct Second
{
    int Field;
}

struct Third
{
    First Field1;
    Second Field2;
}
");
}
