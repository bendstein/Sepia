namespace Interpreter.Common;

public class Location
{
    public int ColumnStart { get; init; } = 0;

    public int ColumnEnd { get; init; } = 0;

    public int LineStart { get; init; } = 0;

    public int LineEnd { get; init; } = 0;

    public Location()
    {

    }

    public Location(int columnStart, int columnEnd, int lineStart, int lineEnd) : this()
    {
        ColumnStart = columnStart;
        ColumnEnd = columnEnd;
        LineStart = lineStart;
        LineEnd = lineEnd;
    }

    public Location(int columnStart, int lineStart) : this(columnStart, columnStart, lineStart, lineStart) { }

    public static implicit operator (int ColumnStart, int ColumnEnd, int LineStart, int LineEnd)(Location loc) => (loc.ColumnStart, loc.ColumnEnd, loc.LineStart, loc.LineEnd);
    public static implicit operator Location((int ColumnStart, int ColumnEnd, int LineStart, int LineEnd) tuple) => new(tuple.ColumnStart, tuple.ColumnEnd, tuple.LineStart, tuple.LineEnd);
    public static implicit operator (int ColumnStart, int LineStart)(Location loc) => (loc.ColumnStart, loc.LineStart);
    public static implicit operator Location((int ColumnStart, int LineStart) tuple) => new(tuple.ColumnStart, tuple.LineStart);

    public override string ToString() => $"{ColumnStart}:{LineStart}";

    private string RangeString(int start, int end) => start == end ? $"{start}" : $"{start}-{end}";
}
