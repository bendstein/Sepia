using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Lex.Literal;
public class StringLiteral
{
    public string Value { get; set; } = string.Empty;

    public QuoteType StringType { get; set; } = QuoteType.D_QUOTE;

    public StringLiteral(string value, QuoteType stringType = QuoteType.D_QUOTE)
    {
        Value = value;
        StringType = stringType;
    }

    public override string ToString() => $"{quoteString()}{Value}{quoteString()}";

    private string quoteString() => StringType switch
    {
        QuoteType.S_QUOTE => TokenTypeValues.S_QUOTE,
        QuoteType.BACKTICK => TokenTypeValues.BACKTICK,
        QuoteType.D_QUOTE or _ => TokenTypeValues.D_QUOTE
    };
}

public enum QuoteType
{
    D_QUOTE,
    S_QUOTE,
    BACKTICK
}