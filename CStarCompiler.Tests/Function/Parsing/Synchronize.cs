using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests.Function.Parsing;

public partial class FunctionTests
{
    [Test]
    public void SynchronizeDeclarationWithoutCloseParen()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction( {}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");
        
    [Test]
    public void SynchronizeDeclarationLambdaWithoutSemicolon()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction() => 10

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");
        
    [Test]
    public void SynchronizeDeclarationWithoutSemicolonAndBody()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction()

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationWithoutOpenParen()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction) {}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationWithMissingParameterType()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction(, int x) {}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationWithMissingParameterName()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction(int) {}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeMultipleBadFunctionDeclarations()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction1( {}
int BadFunction2() => 10
int BadFunction3()

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationWithMissingReturnType()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

BadFunction() {}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationBlockWithoutCloseBrace()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction() {
    int x = 10;

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationBlockWithoutOpenBrace()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction() 
    int x = 10;
}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationLambdaWithoutExpression()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction() => ;

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationWithBadReturnStatement()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction() {
    return 
}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationWithBadIfStatement()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction() {
    if (true 
        return 1;
}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationWithBadVariableDeclaration()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction() {
    int = 10;
}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeFunctionAfterBadField()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadField

int GoodFunction() {}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeMultipleFunctionsWithDifferentErrors()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunc1( {}
int BadFunc2(int {}
int BadFunc3() => 
int GoodFunc() {}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeNestedBlockErrors()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction() {
    {
        int x;
    
}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationWithMissingExpressionInCall()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction() {
    Call(, 1);
}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");

    [Test]
    public void SynchronizeDeclarationWithUnclosedCall()
        => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

int BadFunction() {
    Call(1, 2;
}

struct int {}

struct CheckStruct
{
    CheckStruct Field;
}
");
}
