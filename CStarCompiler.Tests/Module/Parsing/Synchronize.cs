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
}
