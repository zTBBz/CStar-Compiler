namespace CStarCompiler.Lexing;

public sealed class Token(TokenType type, int line, int column, string value)
{
    public TokenType Type { get; } = type;
    public string Value { get; } = value;
    public int Line { get; } = line;
    public int Column { get; } = column;

    public override string ToString() => $"{Type}: '{Value}' at {Line}:{Column}";
}
