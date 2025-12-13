using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public class IdentifierExpressionNode(string name) : ExpressionNode
{
    public string Name { get; set; } = name;
}
