using CStarCompiler.Lexing;
using CStarCompiler.Shared;

namespace CStarCompiler.Parsing.Nodes.Base;

public abstract class DeclarationNode(DeclarationVisibilityModifier visibility, Token location) : AstNode(location)
{
    public DeclarationVisibilityModifier Visibility = visibility;
}
