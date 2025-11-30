namespace CStarCompiler.Parsing.Nodes;

public class MemberAccessExpressionNode : ExpressionNode
{
    public ExpressionNode Object { get; set; }
    public string MemberName { get; set; }
}