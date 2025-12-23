namespace CStarCompiler.SemanticAnalyze.Units.Module;

public sealed class ModuleUnit(string name) : SemanticUnit
{
    public readonly string Name = name;
    public List<ModuleImportUnit>? Imports;
}
