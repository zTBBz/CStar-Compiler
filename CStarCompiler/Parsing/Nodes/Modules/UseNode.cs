using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Modules;

public sealed class UseNode(IdentifierNode identifier, bool isPublic, bool isGlobal, Token location) : AstNode(location)
{
    public IdentifierNode Identifier { get; set; } = identifier;
    public bool IsPublic { get; set; } = isPublic;
    public bool IsGlobal { get; set; } = isGlobal;
}
