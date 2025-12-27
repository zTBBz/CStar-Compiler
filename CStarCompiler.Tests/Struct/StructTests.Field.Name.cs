using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests;

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
}