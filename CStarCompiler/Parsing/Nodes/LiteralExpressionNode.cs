using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class LiteralExpressionNode(object value, string type, Token location) : ExpressionNode(location)
{
    public readonly object Value = value;
    
    // int, string, etc.
    public readonly string Type = type;
}
