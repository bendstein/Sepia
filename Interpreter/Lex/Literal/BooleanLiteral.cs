namespace Interpreter.Lex.Literal;

public class BooleanLiteral : LiteralBase
{
    public BooleanLiteral(string value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToLower();
}
