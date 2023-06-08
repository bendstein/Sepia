namespace Sepia;

public class Environment
{
    private readonly Environment? parent = null;

    private readonly Dictionary<string, List<object?>> values = new();

    public Environment(Environment? parent = null)
    {
        this.parent = parent;
    }

    public object? this[string key]
    {
        get => Get(key).value;
        set => Put(key, value);
    }

    public object? this[string key, int specific]
    {
        get => Get(key, specific);
        set => Update(key, value, specific);
    }

    public bool Defined(string key) => values.ContainsKey(key) || (parent?.Defined(key)?? false);

    public bool Defined(string key, int specific)
    {
        if(values.TryGetValue(key, out var key_values))
        {
            return key_values.Count > specific;
        }
        else
        {
            return parent?.Defined(key, specific)?? false;
        }
    }

    public int Put(string key, object? value = null)
    {
        List<object?>? key_values;

        if(!values.TryGetValue(key, out key_values))
        {
            key_values = new();
            values[key] = key_values;
        }

        key_values.Add(value);
        return key_values.Count - 1;
    }

    public void Update(string key, object? value, int specific)
    {
        if(values.TryGetValue(key, out var key_values))
        {
            if(key_values.Count > specific)
            {
                if (value == null)
                    throw new Exception($"Cannot uninitialize variable {key}`{specific}.");
                key_values[specific] = value;
            }
            else
            {
                throw new Exception($"Cannot update undefined variable {key}`{specific}.");
            }
        }
        else if(parent != null)
        {
            parent.Update(key, value, specific);
        }
        else
        {
            throw new Exception($"Cannot update undefined variable {key}.");
        }
    }

    public (object value, int specific) Get(string key)
    {
        if (values.TryGetValue(key, out var key_values))
        {
            int n = key_values.Count - 1;
            object? value = key_values.Count > 0 ? key_values[n] : null;

            if (value == null)
            {
                throw new Exception($"Cannot access uninitialized variable {key}.");
            }

            return (value, n);
        }
        else if(parent != null)
        {
            return parent.Get(key);
        }
        else
        {
            throw new Exception($"Cannot access undefined variable {key}.");
        }
    }

    public object Get(string key, int specific)
    {
        if (values.TryGetValue(key, out var key_values))
        {
            if (key_values.Count > specific)
            {
                object? value = key_values[specific];

                if(value == null)
                {
                    throw new Exception($"Cannot access uninitialized variable {key}`{specific}.");
                }

                return value;
            }
            else
            {
                throw new Exception($"Cannot update undefined variable {key}`{specific}.");
            }
        }
        else if (parent != null)
        {
            return parent.Get(key, specific);
        }
        else
        {
            throw new Exception($"Cannot update undefined variable {key}.");
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