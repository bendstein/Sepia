using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter;
public class Environment
{
    private readonly Environment? parent = null;

    private readonly Dictionary<string, List<object?>> values = new();

    public Environment(Environment? parent)
    {
        this.parent = parent;
    }

    public int Put(string key, object? value, int? index = null)
    {
        if(index == null)
        {
            if(!values.ContainsKey(key))
                values[key] = new();

            var list = values[key];
            int next = list.Count;
            list.Add(value);
            return next;
        }
        else
        {
            if(IsDefined(key, index.Value))
            {
                if(values.TryGetValue(key, out var list))
                {
                    list[index.Value] = value;
                    return index.Value;
                }
                else
                {
                    return parent!.Put(key, value, index);
                }
            }
            else
            {
                throw new InvalidOperationException($"Cannot update undefined variable {key}`{index}.");
            }
        }
    }

    public int GetCount(string key)
    {
        if(values.TryGetValue(key, out var list))
        {
            return list.Count;
        }
        else
        {
            return parent?.GetCount(key)?? 0;
        }
    }

    public bool IsDefined(string key, int index)
    {
        if (values.TryGetValue(key, out var list) && list.Count > index)
        {
            return true;
        }

        return parent?.IsDefined(key, index)?? false;
    }

    public object Get(string key, int index) 
    {
        if (!IsDefined(key, index))
            throw new Exception($"Cannot access undefined variable {key}`{index}.");

        if(values.TryGetValue(key, out var list))
        {
            object? rv = list[index];

            if (rv == null)
                throw new Exception($"Cannot access unitialized variable {key}`{index}.");

            return rv!;
        }
        else
        {
            return parent!.Get(key, index);
        }
    }
}