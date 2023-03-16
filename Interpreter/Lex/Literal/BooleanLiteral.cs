using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Lex.Literal;

public class BooleanLiteral
{
    public string Value { get; set; } = string.Empty;

    public BooleanLiteral(string value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToLower();
}
