using CStarCompiler.Lexing;

namespace CStarCompiler.Parsing.Nodes.Base;

public abstract class AstNode(Token location)
{
    public readonly Token Location = location;
}
