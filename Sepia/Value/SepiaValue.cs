using Sepia.Lex.Literal;
using Sepia.Value.Type;

namespace Sepia.Value;

public class SepiaValue
{
    public static readonly SepiaValue
        Void = new(null, SepiaTypeInfo.Void),
        Null = new(null, SepiaTypeInfo.Null);

    public SepiaTypeInfo Type { get; set; }

    public object? Value { get; set; }

    public SepiaValue(object? value, SepiaTypeInfo type)
    {
        Value = value;
        Type = type;
    }

    public override string ToString() => Value?.ToString()?? string.Empty;
}