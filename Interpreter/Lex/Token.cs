using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Lex;
public class Token
{
    public TokenType TokenType { get; private set; } = TokenType.NULL;

    public object? Literal { get; private set; } = null;

    public int ColumnStart { get; private set; } = 0;

    public int ColumnEnd { get; private set; } = 0;

    public int LineStart { get; private set; } = 0;

    public int LineEnd { get; private set; } = 0;

    public Token(TokenType tokenType, object? literal = null, int col_start = 0, int col_end = 0, int line_start = 0, int line_end = 0)
    {
        TokenType = tokenType;
        Literal = literal;
        ColumnStart = col_start;
        ColumnEnd = col_end;
        LineStart = line_start;
        LineEnd = line_end;
    }

    public Token(Token other) : this(other.TokenType, other.Literal, other.ColumnStart, other.ColumnEnd, other.LineStart, other.LineEnd) { }

    public Token Clone() => new Token(this);

    public override string ToString() => $"{TokenType}:{TokenType.GetSymbol() ?? string.Empty}:{{{{{Literal}}}}}:{RangeString(ColumnStart, ColumnEnd)}:{RangeString(LineStart, LineEnd)}".ReplaceLineEndings();

    private string RangeString(int start, int end) => start == end ? $"{start}" : $"{start}-{end}";
}
