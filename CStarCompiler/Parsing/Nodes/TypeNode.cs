namespace CStarCompiler.Parsing.Nodes;

public sealed class TypeNode(string type, List<TypeNode>? generics = null) : ExpressionNode
{
    public readonly string Type = type;
    public List<TypeNode>? Generics = generics;
}
