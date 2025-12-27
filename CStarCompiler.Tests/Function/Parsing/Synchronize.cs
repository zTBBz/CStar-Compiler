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
}
