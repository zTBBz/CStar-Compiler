namespace CStarCompiler.Parsing.Nodes;

public class BlockStatementNode : StatementNode
{
    public List<StatementNode> Statements { get; } = [];
    public bool IsCCode { get; set; }
    public bool IsCompileBlock { get; set; }
}
