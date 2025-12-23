using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Modules;

public sealed class UseNode(string moduleName, bool isPublic, bool isGlobal, Token location) : AstNode(location)
{
    public string ModuleName { get; set; } = moduleName;
    public bool IsPublic { get; set; } = isPublic;
    public bool IsGlobal { get; set; } = isGlobal;
}
