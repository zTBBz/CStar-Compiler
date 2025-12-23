using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class IndexExpressionNode(ExpressionNode o, ExpressionNode index, Token location) : ExpressionNode(location)
{
    public ExpressionNode Object { get; set; } = o;
    public ExpressionNode Index { get; set; } = index;
}