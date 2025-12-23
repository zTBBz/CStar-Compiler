namespace CStarCompiler.Lexing;

public enum TokenType : byte
{
    // System
    Eof,
    Unknown,

    // Identifiers and Literals
    Identifier,
    IntegerLiteral,
    FloatLiteral,
    StringLiteral,
    CharLiteral,

    Var,
    
    Struct,
    Contract,
    
    This,
    Ref,
    NoWrite,
    
    Module,
    Use,
    Public,
    Internal,
    Global,
    
    Return,
    If,
    Else,
    
    True,
    False,
    
    // Operators
    Plus,           // +
    Minus,          // -
    Star,           // *
    Slash,          // /
    Percent,        // %
    
    Assign,         // =
    Equals,         // ==
    NotEquals,      // !=
    Less,           // <
    Greater,        // >
    LessOrEqual,    // <=
    GreaterOrEqual, // >=
    
    And,            // &&
    Or,             // ||
    Not,            // ! (Logical not)
    
    BitAnd,         // &
    BitOr,          // |
    BitXor,         // ^
    BitNot,         // ~
    
    Arrow,          // => (Syntactic sugar / lambda)
    
    // Punctuation
    Dot,            // .
    Comma,          // ,
    Colon,          // :
    Semicolon,      // ;
    
    OpenParen,      // (
    CloseParen,     // )
    OpenBrace,      // {
    CloseBrace,     // }
    OpenBracket,    // [
    CloseBracket,   // ]
    
    // Special
    At,             // @ (Used for Compiler Directives)
    Question        // ? (Nullable or ternary)
}
