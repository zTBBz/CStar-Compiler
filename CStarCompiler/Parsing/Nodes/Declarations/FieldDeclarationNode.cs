using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;
using CStarCompiler.Shared;

namespace CStarCompiler.Parsing.Nodes.Declarations;

public sealed class FieldDeclarationNode(IdentifierNode type, IdentifierNode identifier, DeclarationVisibilityModifier visibility, Token location) : DeclarationNode(visibility, location)
{
    public IdentifierNode Type { get; set; } = type;
    public IdentifierNode Identifier { get; set; } = identifier;
}
