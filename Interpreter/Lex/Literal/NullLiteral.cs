namespace Interpreter.Lex.Literal;

public class NullLiteral : LiteralBase
{
    public override string ToString() => TokenType.NULL.GetSymbol()!;
}