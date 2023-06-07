namespace Sepia.Lex.Literal;

public class IdLiteral : LiteralBase
{
    public string Value { get; init; } = string.Empty;

    public IdLiteral(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
}
