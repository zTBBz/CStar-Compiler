using CStarCompiler.Lexing;

namespace CStarCompiler.Parsing.Nodes.Base;

public class ExpressionStatementNode(ExpressionNode expression, Token location) : StatementNode(location)
{
    public ExpressionNode Expression { get; set; } = expression;
}
