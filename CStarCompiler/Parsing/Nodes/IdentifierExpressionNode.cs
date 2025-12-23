using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class IdentifierNode(string name, Token location) : ExpressionNode(location)
{
    public string Name { get; set; } = name;
}
