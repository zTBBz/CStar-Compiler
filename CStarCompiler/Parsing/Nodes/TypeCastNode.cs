using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class TypeCastNode(ExpressionNode from, TypeNode to) : ExpressionNode
{
    public ExpressionNode From = from;
    public TypeNode To = to;
}
