namespace CStarCompiler.Parsing.Nodes;

public class StructDeclarationNode : DeclarationNode
{
    public string Name { get; set; }
    public List<string> GenericParams { get; set; } = [];
    public List<string> InheritedContracts { get; set; } = []; // Simple string for now
    public List<DeclarationNode> Members { get; set; } = [];
}