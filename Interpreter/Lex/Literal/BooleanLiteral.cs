namespace Interpreter.Lex.Literal;

public class BooleanLiteral : LiteralBase
{
    public bool BooleanValue => !string.IsNullOrWhiteSpace(Value) && Value == TokenType.TRUE.GetSymbol();

    public BooleanLiteral(string value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToLower();
}
