using Sepia.Value.Type;

namespace Sepia.Analyzer;

public class ResolveInfo
{
    public SepiaTypeInfo Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Index { get; set; } = 0;

    public int Steps { get; set; } = 0;

    public bool AlwaysReturns { get; set; } = false;    

    public ResolveInfo(string? name = null)
    {
        Name = name?? string.Empty;
        Type = SepiaTypeInfo.TypeVoid();
    }

    public ResolveInfo(SepiaTypeInfo type, string? name = null)
    {
        Name = name?? string.Empty;
        Type = type;
    }

    public virtual ResolveInfo Clone(int? steps = null) => new(Type.Clone(), Name)
    {
        Index = Index,
        Steps = steps?? Steps,
        AlwaysReturns = AlwaysReturns
    };

    public virtual bool TypeEqual(ResolveInfo other)
    {
        if (this == null) return other == null;
        if (other == null) return false;
        return Type == other.Type;
    }
}