using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public class ReturnStatementNode : StatementNode
{
    public ExpressionNode? Value { get; set; }
}