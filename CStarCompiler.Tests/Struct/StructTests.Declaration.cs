using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests;

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
}