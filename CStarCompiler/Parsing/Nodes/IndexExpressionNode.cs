using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public class IndexExpressionNode : ExpressionNode
{
    public ExpressionNode Object { get; set; }
    public ExpressionNode Index { get; set; }
}