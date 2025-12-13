namespace CStarCompiler.Parsing.Nodes;

public class CallExpressionNode : ExpressionNode
{
    public ExpressionNode Callee { get; set; } // Identifier or MemberAccess
    public List<ExpressionNode> Arguments { get; set; } = [];
}
