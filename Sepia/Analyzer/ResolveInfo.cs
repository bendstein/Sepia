using Sepia.Value.Type;

namespace Sepia.Analyzer;

public class ResolveInfo
{
    public SepiaTypeInfo Type { get; set; }

    public int Index { get; set; } = 0;

    public int Steps { get; set; } = 0;

    public bool AlwaysReturns { get; set; } = false;    

    public ResolveInfo()
    {
        Type = SepiaTypeInfo.Void();
    }

    public ResolveInfo(SepiaTypeInfo type)
    {
        Type = type;
    }

    public virtual ResolveInfo Clone(int? steps = null) => new()
    {
        Type = Type.Clone(),
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