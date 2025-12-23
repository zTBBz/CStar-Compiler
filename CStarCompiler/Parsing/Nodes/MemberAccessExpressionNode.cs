using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class MemberAccessExpressionNode(ExpressionNode o, string memberName, Token location) : ExpressionNode(location)
{
    public ExpressionNode Object { get; set; } = o;
    public string MemberName { get; set; } = memberName;
}
