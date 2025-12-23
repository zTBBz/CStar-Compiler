using CStarCompiler.Lexing;

namespace CStarCompiler.Parsing.Nodes.Base;

public abstract class StatementNode(Token location) : AstNode(location);
