using System.Diagnostics.CodeAnalysis;

namespace Interpreter.Lex;
public static class TokenTypeExtensions
{
    public static string? GetSymbol(this TokenType tokenType) => tokenType switch
    {
        TokenType.PLUS => TokenTypeValues.PLUS,
        TokenType.MINUS => TokenTypeValues.MINUS,
        TokenType.STAR => TokenTypeValues.STAR,
        TokenType.SLASH => TokenTypeValues.SLASH,
        TokenType.PERCENT => TokenTypeValues.PERCENT,
        TokenType.CARET => TokenTypeValues.CARET,
        TokenType.DOT => TokenTypeValues.DOT,
        TokenType.COMMA => TokenTypeValues.COMMA,
        TokenType.SEMICOLON => TokenTypeValues.SEMICOLON,
        TokenType.COLON => TokenTypeValues.COLON,
        TokenType.L_PAREN => TokenTypeValues.L_PAREN,
        TokenType.R_PAREN => TokenTypeValues.R_PAREN,
        TokenType.L_BRACE => TokenTypeValues.L_BRACE,
        TokenType.R_BRACE => TokenTypeValues.R_BRACE,
        TokenType.UNDERSCORE => TokenTypeValues.UNDERSCORE,
        TokenType.BANG => TokenTypeValues.BANG,
        TokenType.BANG_EQUAL => TokenTypeValues.BANG_EQUAL,
        TokenType.EQUAL => TokenTypeValues.EQUAL,
        TokenType.EQUAL_EQUAL => TokenTypeValues.EQUAL_EQUAL,
        TokenType.GREATER => TokenTypeValues.GREATER,
        TokenType.GREATER_EQUAL => TokenTypeValues.GREATER_EQUAL,
        TokenType.LESS => TokenTypeValues.LESS,
        TokenType.LESS_EQUAL => TokenTypeValues.LESS_EQUAL,
        TokenType.ZERO_B => TokenTypeValues.ZERO_B,
        TokenType.ZERO_X => TokenTypeValues.ZERO_X,
        TokenType.FALSE => TokenTypeValues.FALSE,
        TokenType.TRUE => TokenTypeValues.TRUE,
        TokenType.NULL => TokenTypeValues.NULL,
        TokenType.AND => TokenTypeValues.AND,
        TokenType.OR => TokenTypeValues.OR,
        TokenType.IF => TokenTypeValues.IF,
        TokenType.ELSE => TokenTypeValues.ELSE,
        TokenType.WHILE => TokenTypeValues.WHILE,
        TokenType.DO => TokenTypeValues.DO,
        TokenType.FOR => TokenTypeValues.FOR,
        TokenType.BREAK => TokenTypeValues.BREAK,
        TokenType.CONTINUE => TokenTypeValues.CONTINUE,
        TokenType.SWITCH => TokenTypeValues.SWITCH,
        TokenType.CASE => TokenTypeValues.CASE,
        TokenType.FUNC => TokenTypeValues.FUNC,
        TokenType.RETURN => TokenTypeValues.RETURN,
        TokenType.CLASS => TokenTypeValues.CLASS,
        TokenType.THIS => TokenTypeValues.THIS,
        TokenType.BASE => TokenTypeValues.BASE,
        TokenType.ABSTRACT => TokenTypeValues.ABSTRACT,
        TokenType.INTERFACE => TokenTypeValues.INTERFACE,
        TokenType.EOF => TokenTypeValues.EOF,
        _ => null
    };

    public static bool HasSymbol(this TokenType tokenType) => tokenType.GetSymbol() != null;

    public static bool TryGetSymbol(this TokenType tokenType, [NotNullWhen(true)] out string? symbol)
    {
        symbol = tokenType.GetSymbol();
        return symbol != null;
    }
}