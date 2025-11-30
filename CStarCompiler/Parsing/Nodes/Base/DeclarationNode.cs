namespace CStarCompiler.Parsing.Nodes;

public abstract class DeclarationNode : AstNode 
{
    public bool IsPublic { get; set; }
    public bool IsInternal { get; set; }
}