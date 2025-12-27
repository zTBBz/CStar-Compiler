using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests.Module.Parsing;

public partial class ModuleTests
{
        [Test]
        public void SynchronizeModuleDeclarationWithoutSemicolon()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main

struct MyStruct
{
    MyStruct Field; 
}
");

        [Test]
        public void SynchronizeModuleImportWithoutSemicolon1()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

use Second

struct MyStruct
{
    MyStruct Field; 
}
", @"
module Second;
");

        [Test]
        public void SynchronizeModuleImportWithoutSemicolon2()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

use Second
use Third;

struct MyStruct
{
    MyStruct Field; 
}
", @"
module Second;
", @"
module Third;
");

        [Test]
        public void SynchronizeModuleImportWithoutName()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

use 
use Third;

struct MyStruct
{
    MyStruct Field; 
}
", @"
module Second;
", @"
module Third;
");

        [Test]
        public void SynchronizeMultipleImports()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

use Second
use 
use Fourth;

struct MyStruct
{
    MyStruct Field; 
}
", @"
module Second;
", @"
module Third;
", @"
module Fourth;
");

        [Test]
        public void SynchronizeModuleSubnameWithoutDot()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main Sub;

struct MyStruct
{
    MyStruct Field; 
}
");

        [Test]
        public void SynchronizeModuleSubnameWithoutName()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main.;

struct MyStruct
{
    MyStruct Field; 
}
");

        [Test]
        public void SynchronizeImportSubnameWithoutDot()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

use Second Third;

struct MyStruct
{
    MyStruct Field; 
}
", @"
module Second.Third;
");

        [Test]
        public void SynchronizeImportSubnameWithoutName()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

use Second.;

struct MyStruct
{
    MyStruct Field; 
}
", @"
module Second;
");

        [Test]
        public void SynchronizeModuleDeclarationWithBadTokenAfterName()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main +

struct MyStruct
{
    MyStruct Field; 
}
");

        [Test]
        public void SynchronizeImportWithBadTokenAfterName()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

use Second +

struct MyStruct
{
    MyStruct Field; 
}
", @"
module Second;
");

        [Test]
        public void SynchronizeGlobalImportWithoutSemicolon()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

global use Second

struct MyStruct
{
    MyStruct Field; 
}
", @"
module Second;
");

        [Test]
        public void SynchronizePublicImportWithoutSemicolon()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

public use Second

struct MyStruct
{
    MyStruct Field; 
}
", @"
module Second;
");

        [Test]
        public void SynchronizeImportChain()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

use Second
use Third.
public use Fourth
global use 

struct MyStruct
{
    MyStruct Field; 
}
", @"
module Second;
", @"
module Third;
", @"
module Fourth;
");

        [Test]
        public void SynchronizeModuleDeclarationOnlyUseKeyword()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main

use 

struct MyStruct
{
    MyStruct Field; 
}
");

        [Test]
        public void SynchronizeAfterEmptyModuleName()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module ;

struct MyStruct
{
    MyStruct Field; 
}
");

        [Test]
        public void SynchronizeImportAfterStructDeclarationError()
            => Tester.AnalyzeWithCode(CompilerLogCode.StructFieldRecursive, @"
module Main;

struct BadStruct

use Second;

struct MyStruct
{
    MyStruct Field; 
}
", @"
module Second;
");
}
