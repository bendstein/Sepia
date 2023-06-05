namespace Interpreter.Lex.Literal;

public class InterpolatedExpressionLiteral : LiteralBase
{
    public string Value { get; init; } = string.Empty;

    public InterpolatedExpressionLiteral(string value)
    {
        Value = value;
    }
}