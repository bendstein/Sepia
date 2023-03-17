namespace Interpreter.Lex.Literal;

public class CommentLiteral : LiteralBase
{
    public CommentType Type { get; set; } = CommentType.Line;

    public CommentLiteral(string value, CommentType type = CommentType.Line)
    {
        Value = value;
        Type = type;
    }

    public override string ToString() => $"{Value}";
}

public enum CommentType
{
    Line,
    Block
}
