using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes.Base;
using CStarCompiler.Shared;

namespace CStarCompiler.Parsing.Nodes.Declarations;

public sealed class FunctionDeclarationNode(IdentifierNode returnType, IdentifierNode identifier, DeclarationVisibilityModifier visibility, Token location, BlockStatementNode? body = null)
    : DeclarationNode(visibility, location)
{
    public IdentifierNode ReturnType { get; } = returnType;
    public IdentifierNode Identifier { get; } = identifier;
    
    public List<VarDeclarationNode> Parameters { get; } = [];
    public BlockStatementNode? Body { get; set; } = body;
}
