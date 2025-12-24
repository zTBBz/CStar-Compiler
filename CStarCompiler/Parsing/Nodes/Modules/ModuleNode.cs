using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Modules;

public sealed class ModuleNode(Token location) : AstNode(location)
{
    public IdentifierNode Identifier;
    public List<UseNode>? Imports { get; set; }
    public List<DeclarationNode> Declarations { get; set; } = [];
}
