using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.This;

public sealed class ThisNode(SingleModifiers modifier) : ExpressionNode
{
    public SingleModifiers Modifiers = modifier;
}
