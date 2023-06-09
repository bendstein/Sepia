using Sepia.Value.Type;

namespace Sepia.Lex.Literal;

public class WhitespaceLiteral : LiteralBase
{
    public string Value { get; init; } = string.Empty;

    private SepiaTypeInfo type = SepiaTypeInfo.Void;

    public override SepiaTypeInfo Type => type;

    public WhitespaceLiteral(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
}
