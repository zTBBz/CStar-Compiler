namespace CStarCompiler.Parsing.Nodes;

public sealed class TypeNode(string name, List<TypeNode>? generics = null) : ExpressionNode
{
    public readonly string Name = name;
    public List<TypeNode>? Generics = generics;
}
