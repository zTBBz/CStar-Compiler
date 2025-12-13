using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class MemberAccessExpressionNode : ExpressionNode
{
    public ExpressionNode Object { get; set; }
    public string MemberName { get; set; }
}
