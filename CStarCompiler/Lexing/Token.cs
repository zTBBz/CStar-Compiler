namespace CStarCompiler.Lexing;

public sealed class Token(string file, int line, int column, TokenType type, string value)
{
    public readonly TokenType Type = type;
    public readonly string Value = value;
    
    public readonly string File = file;
    public readonly int Line = line;
    public readonly int Column  = column;
}
