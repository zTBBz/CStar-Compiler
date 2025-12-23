namespace CStarCompiler.Logs;

public enum CompilerLogCode
{
    Expect = 999,
    
    ModuleImportNotExisted = 0,
    ModuleRecursiveImport = 1,
    ModuleImportDuplicate = 2,
    ModuleDeclarationDuplicate = 3,
    
    TypeNotExist = 4,
    
    StructDeclarationDuplicate = 5,
    StructRecursiveField = 6,
    StructFieldNameDuplicate = 7,
}
