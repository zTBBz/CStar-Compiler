using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests;

public partial class StructTests
{
    private partial class Parsing
    {
        [Test]
        public void SynchronizeDeclarationWithoutOpenBrace()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldNameDuplicate, @"
module Main;

struct BadStruct

    BadStruct Field;
}

struct MyStruct
{
    int Field; 
    int Field; 
}

struct int {}
");

        [Test]
        public void SynchronizeDeclarationWithoutCloseBrace()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldNameDuplicate, @"
module Main;

struct BadStruct
{
    BadStruct Field;


struct MyStruct
{
    int Field; 
    int Field; 
}

struct int {}
");
    }
}