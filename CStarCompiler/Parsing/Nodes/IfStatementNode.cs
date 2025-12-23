using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class IfStatementNode(ExpressionNode condition, StatementNode thenBranch, Token location, StatementNode? elseBranch = null) : StatementNode(location)
{
    public readonly ExpressionNode Condition = condition;
    public readonly StatementNode ThenBranch = thenBranch;
    public readonly StatementNode? ElseBranch = elseBranch;
}