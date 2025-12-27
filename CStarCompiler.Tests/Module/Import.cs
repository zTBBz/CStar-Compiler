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
}