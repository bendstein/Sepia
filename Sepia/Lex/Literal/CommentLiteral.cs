using Sepia.Value.Type;

namespace Sepia.Lex.Literal;

public class CommentLiteral : LiteralBase
{
    public string Value { get; init; } = string.Empty;

    public CommentType CommentType { get; init; } = CommentType.Line;

    private SepiaTypeInfo type = SepiaTypeInfo.Void;

    public override SepiaTypeInfo Type => type;

    public CommentLiteral(string value, CommentType type = CommentType.Line)
    {
        Value = value;
        CommentType = type;
    }

    public override string ToString() => $"{Value}";
}

public enum CommentType
{
    Line,
    Block
}
