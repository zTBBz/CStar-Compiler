using CStarCompiler.Lexing;

namespace CStarCompiler.Parsing.Nodes.Base;

public abstract class ExpressionNode(Token location) : AstNode(location);
