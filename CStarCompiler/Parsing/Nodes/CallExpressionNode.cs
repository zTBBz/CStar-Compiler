using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class CallExpressionNode(ExpressionNode callee, List<ExpressionNode> arguments, Token location) : ExpressionNode(location)
{
    public ExpressionNode Callee { get; set; } = callee; // Identifier or MemberAccess
    public List<ExpressionNode> Arguments { get; set; } = arguments;
}
