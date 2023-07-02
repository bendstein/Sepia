using Sepia.Value.Type;

namespace Sepia.Lex.Literal;

public class InterpolatedExpressionLiteral : LiteralBase
{
    public string Value { get; init; } = string.Empty;

    private SepiaTypeInfo type = SepiaTypeInfo.TypeString();

    public override SepiaTypeInfo Type => type;

    public InterpolatedExpressionLiteral(string value)
    {
        Value = value;
    }
}