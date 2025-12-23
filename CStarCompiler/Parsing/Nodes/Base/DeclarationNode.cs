using CStarCompiler.Lexing;

namespace CStarCompiler.Parsing.Nodes.Base;

public abstract class DeclarationNode(Token location, bool isPublic = false, bool isInternal = false) : AstNode(location)
{
    public bool IsPublic { get; set; } = isPublic;
    public bool IsInternal { get; set; } = isInternal;
}
