using CStarCompiler.Logs;

namespace CStarCompiler.Tests;

public partial class Tests
{
    [Test]
    public void ModuleEmpty()
    {
        const string code = @"
module Main;
";

        TestAnalyze(code);
        AssertLogNoErrors();
    }
    
    [Test]
    public void ModuleNotExist()
    {
        const string code = @"
module Main;

use NotExistedModule;
";

        TestAnalyze(code);
        AssertLog(CompilerLogCode.ModuleImportNotExisted);
    }
    
    [Test]
    public void ModuleExist()
    {
        const string firstModule = @"
module First;

use Second;
";

        const string secondModule = @"
module Second;
";
        
        TestAnalyze(firstModule, secondModule);
        AssertLogNoErrors();
    }
    
    [Test]
    public void ModuleSimpleRecursiveImports()
    {
        const string firstModule = @"
module First;

use Second;
";

        const string secondModule = @"
module Second;

use First;
";
        
        TestAnalyze(firstModule, secondModule);
        AssertLog(CompilerLogCode.ModuleRecursiveImport);
    }
    
    [Test]
    public void ModuleDeepRecursiveImports()
    {
        const string firstModule = @"
module First;

use Second;
";

        const string secondModule = @"
module Second;

use Third;
";
        
        const string thirdModule = @"
module Third;

use First;
";
        
        TestAnalyze(firstModule, secondModule, thirdModule);
        AssertLog(CompilerLogCode.ModuleRecursiveImport);
    }
}
