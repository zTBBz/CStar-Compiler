using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Modules;

public class UseNode : AstNode
{
    public string ModuleName { get; set; }
    public bool IsPublic { get; set; }
    public bool IsGlobal { get; set; }
}
