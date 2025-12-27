using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests.Struct.Field;

public partial class StructTests
{
    [Test]
    public void FieldTypeNotFound()
        => Tester.AnalyzeWithCode(CompilerLogCode.TypeNotFound, @"
module Main;

struct MyStruct
{
    NotExistedStruct NotExistedStructField;
}
");

    [Test]
    public void FieldTypeFromCurrentModule()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

struct MyStruct
{
    MyStruct2 Field;
}

struct MyStruct2
{
    
}
");

    [Test]
    public void FieldTypeFromOtherModule()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

use Second;

struct MyStruct
{
    MyStruct2 Field;
}
", @"
module Second;

struct MyStruct2
{

}
");

    [Test]
    public void FieldTypeNestedStruct()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct MyStruct
{
    NestedStruct Field;

    struct NestedStruct
    {
        MyStruct DeepRecursiveField;
    }
}
");

    [Test]
    public void FieldTypeSimpleRecursive()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct MyStruct
{
    MyStruct RecursiveField;
}
");

    [Test]
    public void FieldTypeSimpleRecursive2()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct A { B b; }
struct B { A a; } 

struct C { A a; } 
");

    [Test]
    public void FieldTypeDeepRecursive()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct MyStruct
{
    MyStruct2 Field;
}

struct MyStruct2
{
    MyStruct DeepRecursiveField;
}
");

    [Test]
    public void FieldTypeLongDeepRecursive()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct MyStruct
{
    MyStruct2 Field;
}

struct MyStruct2
{
    MyStruct3 DeepRecursiveField;
}

struct MyStruct3
{
    MyStruct4 DeepRecursiveField;
}

struct MyStruct4
{
    MyStruct5 DeepRecursiveField;
}

struct MyStruct5
{
    MyStruct6 DeepRecursiveField;
}

struct MyStruct6
{
    MyStruct7 DeepRecursiveField;
}

struct MyStruct7
{
    MyStruct DeepRecursiveField;
}
");
}