using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Modifiers;

public sealed class StackableModifiersNode(List<StackableModifierType> modifiers, Token location) : StatementNode(location)
{
    public List<StackableModifierType> Modifiers = modifiers;
}

public enum StackableModifierType
{
    Array,
    Pointer
}
