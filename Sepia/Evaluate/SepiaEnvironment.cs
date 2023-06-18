using Sepia.Value;
using Sepia.Value.Type;

namespace Sepia.Evaluate;

public class SepiaEnvironment
{
    private readonly SepiaEnvironment? parent = null;

    private readonly Dictionary<string, List<(SepiaTypeInfo type, SepiaValue? value)>> values = new();

    public IEnumerable<string> Keys => values.Keys.Union(parent?.Keys ?? Enumerable.Empty<string>());

    public SepiaEnvironment(SepiaEnvironment? parent = null)
    {
        this.parent = parent;
    }

    public bool Defined(string key)
    {
        if(values.TryGetValue(key, out var val))
        {
            return val.Any();
        }
        else
        {
            return parent?.Defined(key)?? false;
        }
    }

    public SepiaTypeInfo Type(string key, int n)
    {
        if(values.TryGetValue(key, out var val))
        {
            if(val.Count > n)
            {
                return val[n].type;
            }
            else
            {
                throw new Exception($"Cannot update undefined variable {key}.");
            }
        }
        else if(parent != null)
        {
            return parent.Type(key, n);
        }
        else
        {
            throw new Exception($"Cannot update undefined variable {key}.");
        }
    }

    public int Define(string key, SepiaTypeInfo type, SepiaValue? value)
    {
        if (!values.ContainsKey(key))
            values[key] = new();

        values[key].Add((type, value));

        return values[key].Count - 1;
    }

    public void Update(string key, SepiaValue? value, int n)
    {
        if (values.TryGetValue(key, out var val))
        {
            if(val.Count > n)
            {
                if (value == null)
                    throw new Exception($"Cannot uninitialize variable {key}.");

                SepiaTypeInfo type = val[n].type;
                val[n] = (type, value);
            }
            else
            {
                throw new Exception($"Cannot update undefined variable {key}.");
            }
        }
        else if (parent != null)
        {
            parent.Update(key, value, n);
        }
        else
        {
            throw new Exception($"Cannot update undefined variable {key}.");
        }
    }

    public SepiaValue Get(string key, int n)
    {
        if (values.TryGetValue(key, out var val))
        {
            if (val.Count > n)
            {
                var value = val[n].value;
                if (value == null)
                {
                    throw new Exception($"Cannot access uninitialized variable {key}.");
                }

                return value;
            }
            else
            {
                throw new Exception($"Cannot access undefined variable {key}.");
            }
        }
        else if (parent != null)
        {
            return parent.Get(key, n);
        }
        else
        {
            throw new Exception($"Cannot access undefined variable {key}.");
        }
    }

    public bool Initialized(string key, int n)
    {
        if (values.TryGetValue(key, out var val))
        {
            if (val.Count > n)
            {
                return val[n].value != null;
            }
            else
            {
                throw new Exception($"Cannot access undefined variable {key}.");
            }
        }
        else if (parent != null)
        {
            return parent.Initialized(key, n);
        }
        else
        {
            throw new Exception($"Cannot access undefined variable {key}.");
        }
    }

    public SepiaEnvironment Step(int n)
    {
        if (n <= 0)
            return this;
        else if (parent != null)
            return parent.Step(n - 1);
        else
            throw new InvalidOperationException();
    }
}