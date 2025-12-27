using System.Collections.Generic;
using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class BlockStatementNode(Token location) : StatementNode(location)
{
    public List<StatementNode> Statements { get; } = [];
}
