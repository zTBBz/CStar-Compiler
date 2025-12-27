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
}
