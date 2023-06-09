using Sepia.Value.Type;

namespace Sepia.Lex.Literal;

public class VoidLiteral : LiteralBase
{
    public static readonly VoidLiteral Instance = new();

    private SepiaTypeInfo type = SepiaTypeInfo.Void;

    public override SepiaTypeInfo Type => type;

    private VoidLiteral() { }

    public override string ToString() => string.Empty;
}