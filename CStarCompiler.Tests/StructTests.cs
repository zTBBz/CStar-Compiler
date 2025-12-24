using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests;

public class StructTests
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
    
}
