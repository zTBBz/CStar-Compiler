namespace CStarCompiler.SemanticAnalyze.Units.Module;

public sealed class ModuleImportUnit(string name, bool isPublic, bool isGlobal) : SemanticUnit
{
    public readonly string Name = name;
    public bool IsPublic = isPublic;
    public bool IsGlobal = isGlobal;
}
