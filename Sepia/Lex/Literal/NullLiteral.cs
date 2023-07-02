using Sepia.Value.Type;

namespace Sepia.Lex.Literal;

public class NullLiteral : LiteralBase
{
    public static readonly NullLiteral Instance = new();

    private SepiaTypeInfo type = SepiaTypeInfo.TypeNull();

    public override SepiaTypeInfo Type => type;

    private NullLiteral() { }

    public override string ToString() => TokenType.NULL.GetSymbol();
}