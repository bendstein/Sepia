namespace Interpreter.Lex.Literal;

public class NullLiteral : LiteralBase
{
    public static readonly NullLiteral Instance = new();

    private NullLiteral() { }

    public override string ToString() => TokenType.NULL.GetSymbol();
}