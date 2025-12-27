using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests.Struct.Parsing;

public partial class StructTests
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

        [Test]
        public void SynchronizeDeclarationWithoutName()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct 
{
    int Field;
}

struct MyStruct
{
    MyStruct Field;
}
");

        [Test]
        public void SynchronizeFieldWithoutSemicolon()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct int {}

struct BadStruct
{
    int Field
}

struct MyStruct
{
    MyStruct Field;
}
");

        [Test]
        public void SynchronizeFieldWithoutName()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct int {}

struct BadStruct
{
    int ;
}

struct MyStruct
{
    MyStruct Field;
}
");

        [Test]
        public void SynchronizeMultipleStructErrors()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct int {}

struct Bad1

struct Bad2
{
    int Field


struct MyStruct
{
    MyStruct Field;
}
");

        [Test]
        public void SynchronizeNestedStructWithoutCloseBrace()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct int {}

struct Outer
{
    struct Inner
    {
        int Field;
    
}

struct MyStruct
{
    MyStruct Field;
}
");

        [Test]
        public void SynchronizeFieldWithBadModifier()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct int {}

struct BadStruct
{
    public public int Field;
}

struct MyStruct
{
    MyStruct Field;
}
");

        [Test]
        public void SynchronizeStructAfterBadField()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct int {}

struct BadStruct
{
    int 
    int Field2;
}

struct MyStruct
{
    MyStruct Field;
}
");

        [Test]
        public void SynchronizeMultipleFieldErrors()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct int {}

struct BadStruct
{
    int 
    int 
    int Field;
}

struct MyStruct
{
    MyStruct Field;
}
");

        [Test]
        public void SynchronizeDeclarationWithBadToken()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct int {}

struct + BadStruct
{
    int Field;
}

struct MyStruct
{
    MyStruct Field;
}
");

        [Test]
        public void SynchronizeEmptyStructWithoutBody()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct BadStruct

struct MyStruct
{
    MyStruct Field;
}
");

        [Test]
        public void SynchronizeMixedDeclarationsWithErrors()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct int {}

struct Bad1 {
struct Bad2
    int Field;
}

int BadFunction(

struct MyStruct
{
    MyStruct Field;
}
");
}