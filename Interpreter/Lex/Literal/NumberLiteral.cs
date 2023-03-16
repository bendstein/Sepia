using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Lex.Literal;
public class NumberLiteral
{
    public string Value { get; set; }

    public NumberType NumberType { get; set; } = NumberType.INTEGER;

    public NumberBase NumberBase { get; set; } = NumberBase.DECIMAL;

    public NumberLiteral(string? value = null, NumberType numberType = NumberType.INTEGER, NumberBase numberBase = NumberBase.DECIMAL)
    {
        Value = value ?? 0.ToString();
        NumberType = numberType;
        NumberBase = numberBase;
    }

    public override string ToString() => $"{getPrefix()}{Value}";

    private string getPrefix() => NumberBase switch
    {
        NumberBase.BINARY => TokenTypeValues.ZERO_B,
        NumberBase.HEX => TokenTypeValues.ZERO_X,
        NumberBase.DECIMAL or _ => string.Empty
    };
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