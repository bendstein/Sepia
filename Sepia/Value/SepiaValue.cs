using Sepia.Value.Type;

namespace Sepia.Value;

public class SepiaValue : ISepiaValue
{
    public static readonly SepiaValue
        Void = new(null, SepiaTypeInfo.TypeVoid()),
        Null = new(null, SepiaTypeInfo.TypeNull());

    public SepiaTypeInfo Type { get; set; }

    public object? Value { get; set; }

    public Dictionary<string, ISepiaValue> Members { get; set; }

    public SepiaCallSignature? CallSignature => Type.CallSignature;

    public SepiaValue(object? value, SepiaTypeInfo type)
    {
        Value = value;
        Type = type;
        Members = new();
    }

    public override string ToString() => Value?.ToString()?? string.Empty;

    public ISepiaValue Clone()
    {
        object? value;

        if(Value == null)
        {
            value = null;
        }
        else if(Value is ICloneable clonable)
        {
            value = clonable.Clone();
        }
        else
        {
            value = Value;
        }

        return new SepiaValue(value, Type.Clone())
        {
            Members = Members.ToDictionary(m => m.Key, m => m.Value.Clone())
        };
    }
}