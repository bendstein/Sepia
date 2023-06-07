namespace Sepia.Lex.Literal;

public class StringLiteral : LiteralBase
{
    public string Value { get; init; } = string.Empty;

    public QuoteType StringType { get; init; } = QuoteType.D_QUOTE;

    public StringLiteral(string value, QuoteType stringType = QuoteType.D_QUOTE)
    {
        Value = value;
        StringType = stringType;
    }

    public override string ToString() => $"{StringType.AsString()}{Value}{StringType.AsString()}";
}

public enum QuoteType
{
    D_QUOTE,
    S_QUOTE,
    BACKTICK
}

public static class QuoteTypeExtensions
{
    public static string AsString(this QuoteType quoteType) => quoteType switch
    {
        QuoteType.S_QUOTE => TokenTypeValues.S_QUOTE,
        QuoteType.BACKTICK => TokenTypeValues.BACKTICK,
        QuoteType.D_QUOTE or _ => TokenTypeValues.D_QUOTE
    };
}