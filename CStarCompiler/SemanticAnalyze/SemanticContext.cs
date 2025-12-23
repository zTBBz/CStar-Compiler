using CStarCompiler.SemanticAnalyze.Tables;

namespace CStarCompiler.SemanticAnalyze;

public sealed class SemanticContext
{
    public readonly ModuleTable ModuleTable;
    public readonly LocationTable LocationTable;
    public readonly StructTable StructTable;

    public SemanticContext()
    {
        LocationTable = new();
        ModuleTable = new(this);
        StructTable = new(this);
    }
}
