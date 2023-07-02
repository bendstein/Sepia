using Sepia.Value.Type;

namespace Sepia.Value;

public interface ISepiaValue
{
    public SepiaTypeInfo Type { get; }

    public object? Value { get; }

    public Dictionary<string, ISepiaValue> Members { get; }

    public SepiaCallSignature? CallSignature { get; }

    public ISepiaValue Clone();
}