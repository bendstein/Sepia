using Sepia.Value.Type;

namespace Sepia.Lex.Literal;

public class IdLiteral : LiteralBase
{
    public string Value { get; init; } = string.Empty;

    private SepiaTypeInfo type = SepiaTypeInfo.Void;

    public override SepiaTypeInfo Type => type;

    public IdLiteral(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
}
