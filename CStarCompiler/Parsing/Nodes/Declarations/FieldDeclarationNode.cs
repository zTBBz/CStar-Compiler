using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Declarations;

public sealed class FieldDeclarationNode(IdentifierNode type, string name, Token location, ExpressionNode? initializer = null) : DeclarationNode(location)
{
    public IdentifierNode Type { get; set; } = type;
    public string Name { get; set; } = name;
    public ExpressionNode? Initializer { get; set; } = initializer;
}
