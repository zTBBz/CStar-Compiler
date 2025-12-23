using CStarCompiler.Lexing;

namespace CStarCompiler.Parsing.Nodes;

public enum OperatorType : byte
{
    Plus = TokenType.Plus,           // +
    Minus = TokenType.Minus,          // -
    Star = TokenType.Star,           // *
    Slash = TokenType.Slash,          // /
    Percent = TokenType.Percent,        // %
    
    Assign = TokenType.Assign,         // =
    Equals = TokenType.Equals,         // ==
    NotEquals = TokenType.NotEquals,      // !=
    Less = TokenType.Less,           // <
    Greater = TokenType.Greater,        // >
    LessOrEqual = TokenType.LessOrEqual,    // <=
    GreaterOrEqual = TokenType.GreaterOrEqual, // >=
    
    And = TokenType.And,            // &&
    Or = TokenType.Or,             // ||
    Not = TokenType.Not,            // ! (Logical not)
    
    BitAnd = TokenType.BitAnd,         // &
    BitOr = TokenType.BitOr,          // |
    BitXor = TokenType.BitXor,         // ^
    BitNot = TokenType.BitNot,         // ~
}
