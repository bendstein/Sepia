namespace Sepia.Lex.Literal;

public class CommentLiteral : LiteralBase
{
    public string Value { get; init; } = string.Empty;

    public CommentType Type { get; init; } = CommentType.Line;

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
