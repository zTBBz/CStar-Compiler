namespace CStarCompiler.Parsing.Nodes;

public class BlockStatementNode : StatementNode
{
    public List<StatementNode> Statements { get; } = [];
}
