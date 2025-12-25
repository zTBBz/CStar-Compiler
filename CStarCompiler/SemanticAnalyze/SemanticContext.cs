using CStarCompiler.SemanticAnalyze.Tables;
using CStarCompiler.SemanticAnalyze.Tables.Scopes;

namespace CStarCompiler.SemanticAnalyze;

public sealed class SemanticContext
{
    public readonly LocationTable LocationTable;
    
    public readonly ModuleTable ModuleTable;
    public readonly ScopeTable ScopeTable;
    
    public readonly StructTable StructTable;

    public SemanticContext()
    {
        LocationTable = new();
        ScopeTable = new();
        ModuleTable = new(this);
        StructTable = new(this);
    }
}
