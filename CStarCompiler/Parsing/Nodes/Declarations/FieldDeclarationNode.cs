using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Declarations;

public sealed class FieldDeclarationNode(TypeNode type, string name, ExpressionNode? initializer = null) : DeclarationNode
{
    public TypeNode Type { get; set; } = type;
    public string Name { get; set; } = name;
    public ExpressionNode? Initializer { get; set; } = initializer;
}
