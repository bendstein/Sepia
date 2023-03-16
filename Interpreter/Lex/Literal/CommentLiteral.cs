using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Lex.Literal;
public class CommentLiteral
{
    public string Value { get; set; }

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
