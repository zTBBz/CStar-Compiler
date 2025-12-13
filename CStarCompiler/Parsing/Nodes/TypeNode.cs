namespace CStarCompiler.Parsing.Nodes;

public sealed class TypeNode(string name, List<TypeNode>? generics = null, List<PostfixModifierType>? postfixModifiers = null) : ExpressionNode
{
    public readonly string Name = name;
    public List<TypeNode>? Generics = generics;

    public bool IsRef = false;
    public bool IsOut = false;
    public bool IsConst = false;
    public bool IsNoWrites = false;
    
    // postfix modifiers can stacks
    public List<PostfixModifierType>? PostfixModifiers = postfixModifiers;
}

public enum PostfixModifierType
{
    Array,
    Pointer,
}
