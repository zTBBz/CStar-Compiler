using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public class UnaryExpressionNode(OperatorType operatorType, ExpressionNode operand) : ExpressionNode
{
    public readonly OperatorType Operator = operatorType;
    public readonly ExpressionNode Operand = operand;
}
