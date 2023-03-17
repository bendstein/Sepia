﻿namespace Interpreter.Lex.Literal;

public class IdLiteral : LiteralBase
{
    public IdLiteral(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
}
