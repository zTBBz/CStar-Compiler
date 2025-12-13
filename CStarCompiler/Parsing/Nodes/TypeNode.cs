using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class TypeNode(string name, List<StackableModifierType>? stackableModifiers = null) : ExpressionNode
{
    public readonly string Name = name;

    public SingleModifiers SingleModifiers = SingleModifiers.None;
    
    public readonly List<StackableModifierType>? StackableModifiers = stackableModifiers;
}

[Flags]
public enum SingleModifiers
{
    None,
    Ref,
    Out,
    Const,
    NoWrite
}

public enum StackableModifierType
{
    Array,
    Pointer,
}
