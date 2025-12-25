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
    TypeUnused,
    
    StructDeclarationDuplicate,
    
    StructFieldRecursive,
    StructFieldNameDuplicate,
    StructFieldNameShadowStructType,
    
    FunctionDeclarationDuplicate,
}
