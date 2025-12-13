using CStarCompiler.Parsing.Nodes.Base;
using CStarCompiler.Parsing.Nodes.This;

namespace CStarCompiler.Parsing.Nodes.Declarations;

public sealed class FunctionDeclarationNode(TypeNode returnType, string name, BlockStatementNode? body = null,
    ThisNode? thisParameter = null)
    : DeclarationNode
{
    public TypeNode ReturnType { get; } = returnType;
    public string Name { get; } = name;
    
    public ThisNode? ThisParameter = thisParameter;
    public List<VarDeclarationNode> Parameters { get; } = [];
    public BlockStatementNode? Body { get; set; } = body;
}
