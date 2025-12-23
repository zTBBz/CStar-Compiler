using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes;

public sealed class ReturnStatementNode(Token location, ExpressionNode? value = null) : StatementNode(location)
{
    public ExpressionNode? Value = value;
}
