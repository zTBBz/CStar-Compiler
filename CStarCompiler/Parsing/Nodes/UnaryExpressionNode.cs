using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class UnaryExpressionNode(OperatorType operatorType, ExpressionNode operand, Token location) : ExpressionNode(location)
{
    public readonly OperatorType Operator = operatorType;
    public readonly ExpressionNode Operand = operand;
}
