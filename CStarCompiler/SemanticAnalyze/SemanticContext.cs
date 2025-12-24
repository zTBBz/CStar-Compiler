using CStarCompiler.SemanticAnalyze.Tables;
using CStarCompiler.SemanticAnalyze.Tables.Visibility;

namespace CStarCompiler.SemanticAnalyze;

public sealed class SemanticContext
{
    public readonly LocationTable LocationTable;
    
    public readonly ModuleTable ModuleTable;
    public readonly VisibilityTable VisibilityTable;
    
    public readonly StructTable StructTable;

    public SemanticContext()
    {
        LocationTable = new();
        VisibilityTable = new();
        ModuleTable = new(this);
        StructTable = new(this);
    }
}
