namespace CStarCompiler.Parsing.Nodes;

public class LiteralExpressionNode : ExpressionNode
{
    public object Value { get; set; }
    public string Type { get; set; } // int, string, etc.
}