namespace CStarCompiler.Shared.Logs;

public enum CompilerLogCode
{
    LexerUnknownToken,
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
