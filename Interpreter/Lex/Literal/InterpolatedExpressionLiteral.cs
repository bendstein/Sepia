using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Lex.Literal;

public class InterpolatedExpressionLiteral : LiteralBase
{
    public InterpolatedExpressionLiteral(string value)
    {
        Value = value;
    }
}