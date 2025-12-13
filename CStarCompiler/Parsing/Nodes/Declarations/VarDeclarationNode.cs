using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Declarations;

public sealed class VarDeclarationNode(TypeNode type, string name, ExpressionNode? initializer = null) : StatementNode
{
    public TypeNode Type { get; } = type;
    public string Name { get; } = name;
    public ExpressionNode? Initializer { get; } = initializer;
}
