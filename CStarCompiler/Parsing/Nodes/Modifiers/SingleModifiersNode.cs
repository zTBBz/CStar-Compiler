using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Modifiers;

public sealed class SingleModifiersNode(bool isRef, bool isConst, bool isNoWrite, Token location) : StatementNode(location)
{
    public bool IsRef = isRef;
    public bool IsConst = isConst;
    public bool IsNoWrite = isNoWrite;
}
