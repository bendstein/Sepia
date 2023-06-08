﻿namespace Sepia.Lex.Literal;

public class BooleanLiteral : LiteralBase
{
    public string Value { get; init; } = string.Empty;

    public bool BooleanValue => !string.IsNullOrWhiteSpace(Value) && Value == TokenType.TRUE.GetSymbol();

    public BooleanLiteral(string value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToLower();
}