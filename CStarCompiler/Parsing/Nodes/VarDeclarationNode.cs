namespace CStarCompiler.Parsing.Nodes;

public sealed class VarDeclarationNode(TypeNode type, string name, bool isConst, ExpressionNode? initializer = null) : StatementNode
{
    public TypeNode Type { get; } = type; // "var" if inferred
    public string Name { get; } = name;
    public bool IsConst { get; } = isConst;
    public ExpressionNode? Initializer { get; set; } = initializer;
}
