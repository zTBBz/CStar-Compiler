using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;

namespace CStarCompiler.Parsing.Nodes.Declarations;

public sealed class FunctionDeclarationNode(IdentifierNode returnType, string name, Token location, BlockStatementNode? body = null)
    : DeclarationNode(location)
{
    public IdentifierNode ReturnType { get; } = returnType;
    public string Name { get; } = name;
    
    public List<VarDeclarationNode> Parameters { get; } = [];
    public BlockStatementNode? Body { get; set; } = body;
}
