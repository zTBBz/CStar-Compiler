namespace CStarCompiler.Parsing.Nodes;

public sealed class FunctionDeclarationNode(TypeNode returnType, string name, BlockStatementNode? body = null,
    List<TypeNode>? genericParameters = null,
    List<ExpressionNode>? genericConstraints = null)
    : DeclarationNode
{
    public TypeNode ReturnType { get; } = returnType;
    public string Name { get; } = name;
    public List<VarDeclarationNode> Parameters { get; } = [];
    
    public List<TypeNode>? GenericParameters { get; set; } = genericParameters;
    public List<ExpressionNode>? GenericConstraints { get; set; } = genericConstraints; // where T : ...
    public BlockStatementNode? Body { get; set; } = body;
}
