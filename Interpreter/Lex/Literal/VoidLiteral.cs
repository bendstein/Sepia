namespace Interpreter.Lex.Literal;

public class VoidLiteral : LiteralBase
{
    public static readonly VoidLiteral Instance = new();

    private VoidLiteral() { }

    public override string ToString() => string.Empty;
}