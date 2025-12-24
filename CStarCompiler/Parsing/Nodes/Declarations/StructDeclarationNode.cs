using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;
using CStarCompiler.Shared;

namespace CStarCompiler.Parsing.Nodes.Declarations;

public sealed class StructDeclarationNode(IdentifierNode identifier, DeclarationVisibilityModifier visibility, Token location) : DeclarationNode(visibility, location)
{
    public IdentifierNode Identifier { get; set; } = identifier;
    public List<DeclarationNode> Members { get; set; } = [];
}
