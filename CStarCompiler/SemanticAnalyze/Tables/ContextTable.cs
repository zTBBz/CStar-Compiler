namespace CStarCompiler.SemanticAnalyze.Tables;

public abstract class ContextTable(SemanticContext context)
{
    protected readonly SemanticContext _context = context;
}
