using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Declarations;

public class StructDeclarationNode : DeclarationNode
{
    public string Name { get; set; }
    public List<DeclarationNode> Members { get; set; } = [];
}
