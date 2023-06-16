using Sepia.Value.Type;

namespace Sepia.Analyzer;

public class ScopeInfo
{
    public SepiaTypeInfo Type { get; set; }

    public string Name { get; set; }

    public bool Initialized { get; set; }

    public ScopeInfo(SepiaTypeInfo type, string name, bool initialized)
    {
        Type = type;
        Name = name;
        Initialized = initialized;
    }
}