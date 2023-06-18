using Sepia.Value.Type;
using System.Diagnostics.CodeAnalysis;

namespace Sepia.Analyzer;

public class Scope
{
    public readonly Scope? parent;

    public Dictionary<string, List<ScopeInfo>> scopes;

    public int depth { get; private set; }

    public bool allowLoopControls { get; set; } = false;

    public SepiaTypeInfo? currentFunctionReturnType = null;

    public Scope(Scope? parent = null)
    {
        scopes = new();
        this.parent = parent;
        depth = (parent?.depth?? -1) + 1;
        allowLoopControls = parent?.allowLoopControls ?? false;
        currentFunctionReturnType = parent?.currentFunctionReturnType;
    }

    public int Count(string id)
    {
        if(scopes.ContainsKey(id)) 
        { 
            return scopes[id].Count;
        }
        else if(parent != null)
        {
            return parent.Count(id);
        }
        else
        {
            return 0;
        }
    }

    public void Update(string id, int n, ScopeInfo updated)
    {
        if (scopes.ContainsKey(id))
        {
            var infos = scopes[id];

            if(infos.Count > n)
            {
                updated.ResolveInfo.Index = n;
                updated.Index = n;

                infos[n] = updated;
            }
        }
        else if (parent != null)
        {
            parent.Update(id, n, updated);
        }

        throw new Exception();
    }

    public (ScopeInfo info, int steps) Get(string id, int n) 
    { 
        if(scopes.ContainsKey(id))
        {
            var infos = scopes[id];

            if(infos.Count > n)
            {
                return (infos[n].Clone(0), 0);
            }
        }
        else if(parent != null)
        {
            (var info, int steps) = parent.Get(id, n);
            return (info.Clone(steps + 1), steps + 1);
        }

        throw new Exception();
    }

    public bool TryGet(string id, int n, [NotNullWhen(true)] out ScopeInfo? info, out int steps)
    {
        info = null;
        steps = 0;

        if(scopes.TryGetValue(id, out var infos))
        {
            if(infos.Count > n)
            {
                info = infos[n].Clone(steps);
                return true;
            }
        }
        else if(parent != null)
        {
            bool found = parent.TryGet(id, n, out info, out steps);

            if (found)
            {
                steps += 1;
                info = info!.Clone(steps);
            }

            return found;
        }

        return false;
    }

    public bool TryGet(string id, [NotNullWhen(true)] out ScopeInfo? info, out int n, out int steps)
    {
        info = null;
        steps = 0;
        n = Count(id) - 1;

        if(n < 0)
        {
            return false;
        }

        return TryGet(id, n, out info, out steps);
    }

    public int Declare(string id, ScopeInfo info)
    {
        List<ScopeInfo>? infos;

        if(!scopes.TryGetValue(id, out infos))
        {
            infos = new();
            scopes[id] = infos;
        }

        info.ResolveInfo.Index = infos.Count;
        info.Index = infos.Count;

        infos.Add(info);

        return infos.Count - 1;
    }
}