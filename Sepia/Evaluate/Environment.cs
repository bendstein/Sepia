using Sepia.Value;
using Sepia.Value.Type;

namespace Sepia.Evaluate;

public class Environment
{
    private readonly Environment? parent = null;

    private readonly Dictionary<string, List<(SepiaTypeInfo type, SepiaValue? value)>> values = new();

    public Environment(Environment? parent = null)
    {
        this.parent = parent;
    }

    public bool Defined(string key) => values.ContainsKey(key) || (parent?.Defined(key) ?? false);

    public bool Defined(string key, int specific)
    {
        if (values.TryGetValue(key, out var key_values))
        {
            return key_values.Count > specific && key_values[specific].value != null;
        }
        else
        {
            return parent?.Defined(key, specific) ?? false;
        }
    }

    public SepiaTypeInfo Type(string key, int specific)
    {
        if(values.TryGetValue(key, out var vals))
        {
            if (vals.Count > specific)
            {
                (SepiaTypeInfo typeInfo, _) = vals[specific];
                return typeInfo;
            }
            else
            {
                throw new Exception($"Cannot update undefined variable {key}`{specific}.");
            }
        }
        else if(parent != null)
        {
            return parent.Type(key, specific);
        }
        else
        {
            throw new Exception($"Cannot update undefined variable {key}.");
        }
    }

    public int Define(string key, SepiaTypeInfo type)
    {
        List<(SepiaTypeInfo type, SepiaValue? value)>? key_values;

        if (!values.TryGetValue(key, out key_values))
        {
            key_values = new();
            values[key] = key_values;
        }

        key_values.Add((type, null));
        return key_values.Count - 1;
    }

    public void Update(string key, SepiaValue? value, int specific)
    {
        if (values.TryGetValue(key, out var key_values))
        {
            if (key_values.Count > specific)
            {
                if (value == null)
                    throw new Exception($"Cannot uninitialize variable {key}`{specific}.");

                SepiaTypeInfo type = key_values[specific].type;
                key_values[specific] = (type, value);
            }
            else
            {
                throw new Exception($"Cannot update undefined variable {key}`{specific}.");
            }
        }
        else if (parent != null)
        {
            parent.Update(key, value, specific);
        }
        else
        {
            throw new Exception($"Cannot update undefined variable {key}.");
        }
    }

    public (SepiaValue value, int specific) Get(string key)
    {
        if (values.TryGetValue(key, out var key_values))
        {
            if (key_values.Count == 0)
            {
                throw new Exception($"Cannot access undefined variable {key}.");
            }

            int n = key_values.Count - 1;

            (_, SepiaValue? value) = key_values[n];

            if (value == null)
            {
                throw new Exception($"Cannot access uninitialized variable {key}.");
            }

            return (value, n);
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

    public SepiaValue Get(string key, int specific)
    {
        if (values.TryGetValue(key, out var key_values))
        {
            if (key_values.Count <= specific)
            {
                throw new Exception($"Cannot access undefined variable {key}.");
            }

            (_, SepiaValue? value) = key_values[specific];

            if (value == null)
            {
                throw new Exception($"Cannot access uninitialized variable {key}.");
            }

            return value;
        }
        else if (parent != null)
        {
            return parent.Get(key, specific);
        }
        else
        {
            throw new Exception($"Cannot access undefined variable {key}.");
        }
    }

    public int GetCurrent(string key)
    {
        if (values.TryGetValue(key, out var key_values))
        {
            return key_values.Count - 1;
        }
        else if (parent != null)
        {
            return parent.GetCurrent(key);
        }
        else
        {
            throw new Exception($"Cannot access undefined variable {key}.");
        }
    }
}