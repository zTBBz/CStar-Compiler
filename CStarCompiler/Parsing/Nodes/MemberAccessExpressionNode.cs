using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class MemberAccessExpressionNode(ExpressionNode o, IdentifierNode memberIdentifier, Token location) : ExpressionNode(location)
{
    public ExpressionNode Object { get; set; } = o;
    public IdentifierNode MemberIdentifier { get; set; } = memberIdentifier;
}
