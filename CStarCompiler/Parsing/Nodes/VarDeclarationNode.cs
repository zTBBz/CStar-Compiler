using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;
using CStarCompiler.Parsing.Nodes.Modifiers;

namespace CStarCompiler.Parsing.Nodes;

public sealed class VarDeclarationNode(IdentifierNode type, IdentifierNode identifier, Token location,
    ExpressionNode? initializer = null, SingleModifiersNode? singleModifiers = null, StackableModifiersNode? stackableModifiers = null) : StatementNode(location)
{
    public bool IsThisParam = false;
    public IdentifierNode Type { get; } = type;
    public IdentifierNode Identifier { get; } = identifier;
    public ExpressionNode? Initializer { get; } = initializer;

    public SingleModifiersNode? SingleModifiers { get; set; } = singleModifiers;
    public StackableModifiersNode? StackableModifiers { get; set; } = stackableModifiers;
}
