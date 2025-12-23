using CStarCompiler.SemanticAnalyze.Units;
using CStarCompiler.SemanticAnalyze.Units.Type;

namespace CStarCompiler.SemanticAnalyze.Declarations;

public sealed class StructUnit(string name) : SemanticUnit
{
    public readonly string Name = name;
    
    public VisibilityModifier Visibility = VisibilityModifier.Private;

    public Dictionary<string, VariableUnit>? Fields;
    public Dictionary<string, FunctionUnit>? Functions;
}
