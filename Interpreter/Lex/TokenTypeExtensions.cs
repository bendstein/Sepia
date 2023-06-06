using System.Diagnostics.CodeAnalysis;

namespace Interpreter.Lex;
public static class TokenTypeExtensions
{
    public static string GetSymbol(this TokenType tokenType) => TokenTypeValues.TOKEN_SYMBOLS.TryGetValue(tokenType, out string? symbol)? symbol : tokenType.ToString();

    public static bool HasSymbol(this TokenType tokenType) => TokenTypeValues.TOKEN_SYMBOLS.ContainsKey(tokenType);

    public static bool TryGetSymbol(this TokenType tokenType, [NotNullWhen(true)] out string? symbol) => TokenTypeValues.TOKEN_SYMBOLS.TryGetValue(tokenType, out symbol);

    public static bool IsKeyword(this TokenType tokenType) => TokenTypeValues.KEYWORD_TOKENS.Contains(tokenType);

    public static bool IsSyncPrev(this TokenType tokenType) => TokenTypeValues.TOKEN_SYNC_PREV.Contains(tokenType);
    public static bool IsSyncNext(this TokenType tokenType) => TokenTypeValues.TOKEN_SYNC_NEXT.Contains(tokenType);
}