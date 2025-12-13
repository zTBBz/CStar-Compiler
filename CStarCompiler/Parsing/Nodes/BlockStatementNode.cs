using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public class BlockStatementNode : StatementNode
{
    public List<StatementNode> Statements { get; } = [];
}
