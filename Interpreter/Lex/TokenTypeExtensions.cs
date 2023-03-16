using System.Diagnostics.CodeAnalysis;

namespace Interpreter.Lex;
public static class TokenTypeExtensions
{
    public static string? GetSymbol(this TokenType tokenType) => TokenTypeValues.TOKEN_SYMBOLS.TryGetValue(tokenType, out string? symbol)? symbol : null;

    public static bool HasSymbol(this TokenType tokenType) => TokenTypeValues.TOKEN_SYMBOLS.ContainsKey(tokenType);

    public static bool TryGetSymbol(this TokenType tokenType, [NotNullWhen(true)] out string? symbol) => TokenTypeValues.TOKEN_SYMBOLS.TryGetValue(tokenType, out symbol);
}