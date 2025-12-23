using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Declarations;

public sealed class StructDeclarationNode(string name, Token location) : DeclarationNode(location)
{
    public string Name { get; set; } = name;
    public List<DeclarationNode> Members { get; set; } = [];
}
