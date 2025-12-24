using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests;

public class FunctionTests
{
    [Test]
    public void ParametersEmpty()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

void BadFunction() {}

struct void {}
");
    
    private class Parsing
    {
        #region Synchronize

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
        
        // todo: test synchronize lambda and body contents when be ready function analyze

        #endregion
    }
}