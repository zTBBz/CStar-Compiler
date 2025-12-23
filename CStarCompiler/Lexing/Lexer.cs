using System.Text;

namespace CStarCompiler.Lexing;

public sealed class Lexer
{
    private string _source = null!;
    private int _position;
    private int _line;
    private int _column;

    private static TokenType GetKeyword(string keyword) => keyword switch
    {
        "module" => TokenType.Module,
        "use" => TokenType.Use,
        
        "public" => TokenType.Public,
        "internal" => TokenType.Internal,
        "global" => TokenType.Global,
        
        "struct" => TokenType.Struct,
        "contract" => TokenType.Contract,

        "var" => TokenType.Var,
        
        "ref" => TokenType.Ref,
        "this" => TokenType.This,
        
        "return" => TokenType.Return,
        "if" => TokenType.If,
        "else" => TokenType.Else,
        
        "true" => TokenType.True,
        "false" => TokenType.False,
        
        _ => TokenType.Identifier
    };
    
    public List<Token> Tokenize(string source)
    {
        _source = source;
        _position = 0;
        _line = 1;
        _column = 1;

        var tokens = new List<Token>();

        while (_position < _source.Length)
        {
            var current = Peek();

            // Skip whitespaces
            if (char.IsWhiteSpace(current))
            {
                Advance();
                continue;
            }

            // Skip comments
            if (current == '/' && PeekNext() == '/')
            {
                while (Peek() != '\n' && Peek() != '\0') Advance();
                continue;
            }

            // Numbers
            if (char.IsDigit(current))
            {
                tokens.Add(ReadNumber());
                continue;
            }

            // Identifiers and keywords
            if (char.IsLetter(current) || current == '_')
            {
                tokens.Add(ReadIdentifierOrKeyword());
                continue;
            }

            // String and char literals
            switch (current)
            {
                case '"':
                    tokens.Add(ReadString());
                    continue;
                case '\'':
                    tokens.Add(ReadChar());
                    continue;
            }

            var startCol = _column;
            var startLine = _line;

            switch (current)
            {
                // Single chars
                case '(': AddToken(TokenType.OpenParen); Advance(); break;
                case ')': AddToken(TokenType.CloseParen); Advance(); break;
                case '{': AddToken(TokenType.OpenBrace); Advance(); break;
                case '}': AddToken(TokenType.CloseBrace); Advance(); break;
                case '[': AddToken(TokenType.OpenBracket); Advance(); break;
                case ']': AddToken(TokenType.CloseBracket); Advance(); break;
                case ',': AddToken(TokenType.Comma); Advance(); break;
                case ';': AddToken(TokenType.Semicolon); Advance(); break;
                case '.': AddToken(TokenType.Dot); Advance(); break;
                case ':': AddToken(TokenType.Colon); Advance(); break;
                case '@': AddToken(TokenType.At); Advance(); break;
                case '?': AddToken(TokenType.Question); Advance(); break;
                case '~': AddToken(TokenType.BitNot); Advance(); break;
                case '^': AddToken(TokenType.BitXor); Advance(); break;
                case '%': AddToken(TokenType.Percent); Advance(); break;

                // Double chars
                case '+':
                    if (PeekNext() == '+') 
                    { 
                        AddToken(TokenType.Unknown);
                        Advance(); 
                        Advance(); 
                    }
                    else
                    {
                        AddToken(TokenType.Plus);
                        Advance();
                    }
                    break;
                case '-':
                    if (PeekNext() == '>')
                    {
                        AddToken(TokenType.Arrow); 
                        Advance(); 
                        Advance();
                    }
                    else 
                    { 
                        AddToken(TokenType.Minus);
                        Advance();
                    }
                    break;
                case '*':
                    AddToken(TokenType.Star);
                    Advance();
                    break;
                case '/':
                    AddToken(TokenType.Slash);
                    Advance();
                    break;
                case '=':
                    if (PeekNext() == '=')
                    {
                        AddToken(TokenType.Equals); 
                        Advance(); 
                        Advance();
                    }
                    else if (PeekNext() == '>')
                    {
                        AddToken(TokenType.Arrow);
                        Advance();
                        Advance();
                    }
                    else
                    {
                        AddToken(TokenType.Assign); 
                        Advance();
                    }
                    break;
                case '!':
                    if (PeekNext() == '=')
                    {
                        AddToken(TokenType.NotEquals);
                        Advance();
                        Advance();
                    }
                    else
                    {
                        AddToken(TokenType.Not); 
                        Advance();
                    }
                    break;
                case '<':
                    if (PeekNext() == '=')
                    {
                        AddToken(TokenType.LessOrEqual); 
                        Advance(); 
                        Advance();
                    }
                    else
                    {
                        AddToken(TokenType.Less);
                        Advance();
                    }
                    break;
                case '>':
                    if (PeekNext() == '=') 
                    { 
                        AddToken(TokenType.GreaterOrEqual); 
                        Advance();
                        Advance();
                    }
                    else
                    {
                        AddToken(TokenType.Greater);
                        Advance();
                    }
                    break;
                case '&':
                    if (PeekNext() == '&') 
                    { 
                        AddToken(TokenType.And);
                        Advance(); 
                        Advance(); 
                    }
                    else
                    {
                        AddToken(TokenType.BitAnd); 
                        Advance();
                    }
                    break;
                case '|':
                    if (PeekNext() == '|')
                    {
                        AddToken(TokenType.Or);
                        Advance();
                        Advance();
                    }
                    else
                    {
                        AddToken(TokenType.BitOr);
                        Advance();
                    }
                    break;

                default:
                    AddToken(TokenType.Unknown, current.ToString());
                    Advance();
                    break;
            }

            void AddToken(TokenType type, string? val = null) => tokens.Add(new(type, startLine, startCol, val ?? string.Empty));
        }

        tokens.Add(new(TokenType.Eof, _line, _column, string.Empty));
        return tokens;
    }
    
    private Token ReadIdentifierOrKeyword()
    {
        var startCol = _column;
        var startLine = _line;
        var sb = new StringBuilder();

        while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
        {
            sb.Append(Peek());
            Advance();
        }

        var text = sb.ToString();
        return new(GetKeyword(text), startLine, startCol, text);
    }

    private Token ReadNumber()
    {
        var startCol = _column;
        var startLine = _line;
        var sb = new StringBuilder();
        var isFloat = false;

        while (char.IsDigit(Peek()))
        {
            sb.Append(Peek());
            Advance();
        }

        if (Peek() == ',' && char.IsDigit(PeekNext()))
        {
            isFloat = true;
            sb.Append(Peek());
            Advance();

            while (char.IsDigit(Peek()))
            {
                sb.Append(Peek());
                Advance();
            }
        }

        if (char.ToLower(Peek()) == 'f')
        {
            isFloat = true;
            sb.Append(Peek());
            Advance();
        }

        return new(isFloat ? TokenType.FloatLiteral : TokenType.IntegerLiteral, startLine, startCol, sb.ToString());
    }

    private Token ReadString()
    {
        var startCol = _column;
        var startLine = _line;
        var sb = new StringBuilder();

        Advance(); // skip "

        while (Peek() != '"' && Peek() != '\0')
        {
            if (Peek() == '\\')
            {
                Advance();
                sb.Append(Peek());
            }
            else sb.Append(Peek());

            Advance();
        }

        if (Peek() == '"')
        {
            Advance();
            return new(TokenType.StringLiteral, startLine, startCol, sb.ToString());
        }

        return new(TokenType.Unknown, startLine, startCol, sb.ToString());
    }

    private Token ReadChar()
    {
        var startCol = _column;
        var startLine = _line;

        Advance(); // skip '
        var c = Peek();

        if (c == '\\')
        {
            Advance();
            c = Peek();
        }
        Advance();

        if (Peek() == '\'')
        {
            Advance();
            return new(TokenType.CharLiteral, startLine, startCol, c.ToString());
        }

        return new(TokenType.Unknown, startLine, startCol, c.ToString());
    }

    private char Peek() => _position >= _source.Length ? '\0' : _source[_position];

    private char PeekNext() => _position + 1 >= _source.Length ? '\0' : _source[_position + 1];

    private void Advance()
    {
        if (_position >= _source.Length) return;
        
        if (_source[_position] == '\n')
        {
            _line++;
            _column = 1;
        }
        else _column++;

        _position++;
    }
}
