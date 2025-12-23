namespace CStarCompiler.SemanticAnalyze.Units.Type;

public sealed class VariableUnit(string name, TypeUnit typeUnit) : SemanticUnit
{
    public readonly string Name = name;
    public readonly TypeUnit Type = typeUnit;
}
