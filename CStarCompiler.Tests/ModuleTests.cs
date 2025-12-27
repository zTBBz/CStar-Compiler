using CStarCompiler.Shared.Logs;
using NUnit.Framework;

namespace CStarCompiler.Tests;

public class ModuleTests
{
    private class Parsing
    {
        #region Module

        [Test]
        public void ModuleDeclarationBad()
            => Tester.ParseWithError("");

        [Test]
        public void ModuleDeclarationBadWithoutName()
            => Tester.ParseWithError( @"
module 
");

        [Test]
        public void ModuleDeclarationGood()
            => Tester.ParseWithoutErrors(@"
module Main;
");
        
        [Test]
        public void ModuleDeclarationBadWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main
");

        [Test]
        public void ModuleDeclarationGoodWithSubName()
            => Tester.ParseWithoutErrors(@"
module Main.CoolMain;
");

        [Test]
        public void ModuleDeclarationBadWithSubNameWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main.CoolMain
");
        
        #endregion

        #region Import

        [Test]
        public void ImportDeclarationGood()
            => Tester.ParseWithoutErrors(@"
module Main;

use Some;
");

        [Test]
        public void ImportDeclarationBadWithoutName()
            => Tester.ParseWithError(@"
module Main;

use 
");
        
        [Test]
        public void ImportDeclarationBadWithoutSemicolon()
            => Tester.ParseWithError(@"
module Main;

use Some
");

        #endregion

        #region Synchronize

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

        #endregion
    }

    [Test]
    public void ModuleEmpty()
        => Tester.AnalyzeWithoutErrors(@"
module Main;
");

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
