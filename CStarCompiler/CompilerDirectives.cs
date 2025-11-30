using CStarCompiler.Parsing.Nodes;

namespace CStarCompiler;

public static class CompilerDirectives
{
    public static CompilerDirective GetDirective(string directiveName)
    {
        var type = GetDirectiveType(directiveName);
        return new(type, GetParameters(type));
    }
    
    public static bool IsDirective(string directiveName) => GetDirectiveType(directiveName) != DirectiveType.None;

    private static CompilerDirectiveParameter[] GetParameters(DirectiveType type) =>
        type switch
        {
            DirectiveType.SizeOf => [CompilerDirectiveParameter.Type],
            DirectiveType.TypeOf => [CompilerDirectiveParameter.Type],
            DirectiveType.AlignOf => [CompilerDirectiveParameter.Type],
            DirectiveType.UseC => [CompilerDirectiveParameter.StringLiteral],
            _ => []
        };

    private static DirectiveType GetDirectiveType(string directiveName) =>
        directiveName switch
        {
            "SizeOf" => DirectiveType.SizeOf,
            "TypeOf" => DirectiveType.TypeOf,
            "AlignOf" => DirectiveType.AlignOf,
            "UseC" => DirectiveType.UseC, 
            _ => DirectiveType.None
        };
}

public sealed class CompilerDirective(DirectiveType type, CompilerDirectiveParameter[] parameters) : ExpressionNode
{
    public readonly DirectiveType Type = type;
    public readonly List<AstNode> Nodes = [];
    public readonly CompilerDirectiveParameter[] Parameters = parameters;
}

public enum DirectiveType : byte
{
    None,
    SizeOf,
    TypeOf,
    AlignOf,
    UseC,
}
    
public enum CompilerDirectiveParameter : byte
{
    Identifier,
    Type,
    StringLiteral,
    Expression,
    Declaration,
}
