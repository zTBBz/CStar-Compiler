using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public class IfStatementNode : StatementNode
{
    public ExpressionNode Condition { get; set; }
    public StatementNode ThenBranch { get; set; }
    public StatementNode? ElseBranch { get; set; }
}