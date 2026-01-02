namespace CStarCompiler.Tests.Function.Parsing;

public partial class FunctionTests
{
    [Test]
    public static void BadDeclarationWithoutOpenParen()
        => Tester.ParseWithError(@"
module Main;

int Function) { return 10; }
");
    
    [Test]
    public static void BadDeclarationWithoutCloseParen()
        => Tester.ParseWithError(@"
module Main;

int Function( { return 10; }
");
    
    [Test]
    public static void BadDeclarationWithoutOpenBrace()
        => Tester.ParseWithError(@"
module Main;

int Function()  return 10; }
");
    
    [Test]
    public static void BadDeclarationWithoutCloseBrace()
        => Tester.ParseWithError(@"
module Main;

int Function() { return 10; 
");
    
    [Test]
    public static void BadDeclarationLambdaWithoutSemicolon()
        => Tester.ParseWithError(@"
module Main;

int Function() => 10
");
}
