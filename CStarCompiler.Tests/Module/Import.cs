using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tests.Module;

public partial class ModuleTests
{
    [Test]
    public void ImportNotExist()
        => Tester.AnalyzeWithCode(CompilerLogCode.ModuleImportNotExisted, @" 
module Main;

use NotExistedModule;
");

    [Test]
    public void ImportSelf()
        => Tester.AnalyzeWithCode(CompilerLogCode.ModuleImportSelf, @"
module Main;

use Main;
");

    [Test]
    public void ImportDuplicate()
        => Tester.AnalyzeWithCode(CompilerLogCode.ModuleImportDuplicate, @"
module First;

use Second;
use Second;
", @"
module Second;
");

    [Test]
    public void ImportExist()
        => Tester.AnalyzeWithoutErrors(@"
module First;

use Second;
", @"
module Second;
");

    [Test]
    public void ImportSimpleRecursive()
        => Tester.AnalyzeWithoutErrors(@"
module First;

use Second;
", @"
module Second;

use First;
");

    [Test]
    public void ImportDeepRecursive()
        => Tester.AnalyzeWithoutErrors(@"
module First;

use Second;
", @"
module Second;

use Third;
", @"
module Third;

use First;
");

    [Test]
    public void ImportSubmodule()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

use Sub.Module;
", @"
module Sub.Module;
");

    [Test]
    public void ImportDeepSubmodule()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

use Sub.Deep.Module;
", @"
module Sub.Deep.Module;
");

    [Test]
    public void ImportMultipleModules()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

use First;
use Second;
use Third;
", @"
module First;
", @"
module Second;
", @"
module Third;
");

    [Test]
    public void ImportGlobalModifier()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

global use Second;
", @"
module Second;
");

    [Test]
    public void ImportPublicModifier()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

public use Second;
", @"
module Second;
");

    [Test]
    public void ImportGlobalPublicModifier()
        => Tester.AnalyzeWithoutErrors(@"
module Main;

global public use Second;
", @"
module Second;
");
}