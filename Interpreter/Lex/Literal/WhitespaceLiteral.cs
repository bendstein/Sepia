namespace Interpreter.Lex.Literal;

public class WhitespaceLiteral : LiteralBase
{
    public WhitespaceLiteral(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
}
