namespace CStarCompiler.Shared.Logs;

public enum CompilerLogCode
{
    ParserUnknownToken,
    ParserExpectToken,
    
    ModuleImportNotExisted,
    ModuleImportRecursive,
    ModuleImportDuplicate,
    ModuleImportSelf,
    ModuleDeclarationDuplicate,
    
    TypeNotFound,
    
    StructDeclarationDuplicate,
    
    StructFieldRecursive,
    StructFieldNameDuplicate,
    StructFieldNameShadowStructType,
}
