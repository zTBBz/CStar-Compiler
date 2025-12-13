using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Modules;

public class ModuleNode : AstNode
{
    public string ModuleName { get; set; }
    public List<UseNode> Imports { get; set; } = [];
    public List<DeclarationNode> Declarations { get; set; } = [];
}
