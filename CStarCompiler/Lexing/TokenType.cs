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

    Module,
    Struct,
    Contract,
    
    This,
    Ref,
    
    Use,
    Public,
    Internal,
    Global,
    
    Return,
    If,
    Else,
    For,
    While,
    Where,
    
    Var,
    Const,
    Void,
    Int,
    UInt,
    Float,
    Bool,
    Char,
    String,
    Byte,
    
    True,
    False,
    Null,
    
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
