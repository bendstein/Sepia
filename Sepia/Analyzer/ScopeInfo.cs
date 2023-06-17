using Sepia.Value.Type;

namespace Sepia.Analyzer;

public class ScopeInfo
{
    public ResolveInfo ResolveInfo { get; set; }

    public string Name { get; set; }

    public bool Initialized { get; set; }

    public int Index { get; set; } = 0;

    public int Steps { get; set; } = 0;

    public ScopeInfo(ResolveInfo info, string name, bool initialized)
    {
        ResolveInfo = info;
        Name = name;
        Initialized = initialized;
    }

    public ScopeInfo Clone(int? steps = null) => new(ResolveInfo.Clone(steps), Name, Initialized)
    {
        Index = Index,
        Steps = steps?? Steps
    };
}