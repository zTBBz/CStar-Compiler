namespace CStarCompiler.Parsing.Nodes;

public sealed class BinaryExpressionNode(ExpressionNode left, OperatorType operatorType, ExpressionNode right) : ExpressionNode
{
    public readonly ExpressionNode Left = left;
    public readonly OperatorType Operator = operatorType;
    public readonly ExpressionNode Right = right;
}
