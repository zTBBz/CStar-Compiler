using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests;

public class StructTests
{
    private class Parsing
    {
        [Test]
        public void DeclarationGood()
            => Tester.AnalyzeWithoutErrors(@"
module Main;

struct MyStruct
{

}
");
        
        [Test]
        public void DeclarationBadWithoutName()
            => Tester.AnalyzeWithoutErrors(@"
module Main;

struct 
{

}
");
        
        [Test]
        public void DeclarationBadWithoutOpenBrace()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct


}
");
        
        [Test]
        public void DeclarationBadWithoutCloseBrace()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct
{


");
        
        [Test]
        public void DeclarationGoodField()
            => Tester.ParseWithoutErrors(@"
module Main;

struct MyStruct
{
    int Field;
}
");
        
        [Test]
        public void DeclarationBadFieldWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct
{
    int Field
}
");
        
        [Test]
        public void DeclarationBadFieldWithoutName()
            => Tester.ParseWithError(@"
module Main;

struct MyStruct
{
    int 
}
");

        #region Synchronize

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

        #endregion
    }
    
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
