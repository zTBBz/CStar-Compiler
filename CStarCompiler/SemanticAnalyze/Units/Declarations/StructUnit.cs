using CStarCompiler.SemanticAnalyze.Units.Type;
using CStarCompiler.Shared;

namespace CStarCompiler.SemanticAnalyze.Units.Declarations;

public sealed class StructUnit(string name) : SemanticUnit
{
    public readonly string Name = name;
    
    public DeclarationVisibilityModifier DeclarationVisibility = DeclarationVisibilityModifier.Private;

    public Dictionary<string, VariableUnit>? Fields;
    public Dictionary<string, FunctionUnit>? Functions;
}
