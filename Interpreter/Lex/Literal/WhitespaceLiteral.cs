namespace Interpreter.Lex.Literal;

public class WhitespaceLiteral : LiteralBase
{
    public string Value { get; init; } = string.Empty;

    public WhitespaceLiteral(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
}
