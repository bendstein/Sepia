using Sepia.Value;
using Sepia.Value.Type;

namespace Sepia.Evaluate;

public class SepiaEnvironment
{
    private readonly SepiaEnvironment? parent = null;

    private readonly Dictionary<string, (SepiaTypeInfo type, SepiaValue? value)> values = new();

    public SepiaEnvironment(SepiaEnvironment? parent = null)
    {
        this.parent = parent;
    }

    public bool Defined(string key) => values.ContainsKey(key) || (parent?.Defined(key) ?? false);

    public SepiaTypeInfo Type(string key)
    {
        if(values.TryGetValue(key, out var val))
        {
            return val.type;
        }
        else if(parent != null)
        {
            return parent.Type(key);
        }
        else
        {
            throw new Exception($"Cannot update undefined variable {key}.");
        }
    }

    public void Define(string key, SepiaTypeInfo type, SepiaValue? value)
    {
        values[key] = (type, value);
    }

    public void Update(string key, SepiaValue? value)
    {
        if (values.TryGetValue(key, out var val))
        {
            if (value == null)
                throw new Exception($"Cannot uninitialize variable {key}.");

            SepiaTypeInfo type = val.type;
            values[key] = (type, value);
        }
        else if (parent != null)
        {
            parent.Update(key, value);
        }
        else
        {
            throw new Exception($"Cannot update undefined variable {key}.");
        }
    }

    public SepiaValue Get(string key)
    {
        if (values.TryGetValue(key, out var val))
        { 

            if (val.value == null)
            {
                throw new Exception($"Cannot access uninitialized variable {key}.");
            }

            return val.value;
        }
        else if (parent != null)
        {
            return parent.Get(key);
        }
        else
        {
            throw new Exception($"Cannot access undefined variable {key}.");
        }
    }
}