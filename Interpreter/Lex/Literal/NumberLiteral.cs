namespace Interpreter.Lex.Literal;
public class NumberLiteral : LiteralBase
{
    public string Value { get; init; } = string.Empty;

    public NumberType NumberType { get; init; } = NumberType.INTEGER;

    public NumberBase NumberBase { get; init; } = NumberBase.DECIMAL;

    public NumberLiteral(string? value = null, NumberType numberType = NumberType.INTEGER, NumberBase numberBase = NumberBase.DECIMAL)
    {
        Value = value ?? 0.ToString();
        NumberType = numberType;
        NumberBase = numberBase;
    }

    public override string ToString() => $"{NumberBase.GetPrefix()}{Value}";
}

public enum NumberBase
{
    DECIMAL,
    BINARY,
    HEX
}

public enum NumberType
{
    INTEGER,
    FLOAT
}

public static class NumberBaseExtensions
{
    public static string GetPrefix(this NumberBase nbase) => nbase switch
    {
        NumberBase.BINARY => TokenTypeValues.ZERO_B,
        NumberBase.HEX => TokenTypeValues.ZERO_X,
        NumberBase.DECIMAL or _ => string.Empty
    };

    public static int GetBaseNum(this NumberBase nbase) => nbase switch
    {
        NumberBase.BINARY => 2,
        NumberBase.HEX => 16,
        NumberBase.DECIMAL or _ => 10
    };
}