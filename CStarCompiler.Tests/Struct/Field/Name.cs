using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests.Struct.Field;

public partial class StructTests
{
    [Test]
    public void FieldNameDuplicate()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldNameDuplicate, @"
module Main;

struct MyStruct
{
    int Field;
    int Field;
}

struct int
{

}
");

    [Test]
    public void FieldNameShadowStructType()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldNameShadowStructType, @"
module Main;

struct MyStruct
{
    int MyStruct;
}

struct int
{

}
");
    
    [Test]
    public void FieldNameShadowNestedStructType()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldNameShadowStructType, @"
module Main;

struct MyStruct
{
    int int;

    struct int {}
}
");

    [Test]
    public void FieldNameUnique()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

struct int {}

struct MyStruct
{
    int Field1;
    int Field2;
    int Field3;
}
");
    
    [Test]
    public void FieldNameDuplicateInDifferentStructs()
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
");

    [Test]
    public void FieldNameDuplicateTriple()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldNameDuplicate, @"
module Main;

struct int {}

struct MyStruct
{
    int Field;
    int Field;
    int Field;
}
");

    [Test]
    public void FieldNameInNestedStruct()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

struct int {}

struct Outer
{
    int Field;
    Inner InnerField;

    struct Inner
    {
        int Field;
    }
}
");
}
