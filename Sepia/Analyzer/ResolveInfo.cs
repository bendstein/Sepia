using Sepia.Value.Type;
using System;

namespace Sepia.Analyzer;

public class ResolveInfo
{
    public SepiaTypeInfo Type { get; set; }

    public FunctionResolveInfo? FunctionResolveInfo { get; set; } = null;

    public int Index { get; set; } = 0;

    public int Steps { get; set; } = 0;

    public bool AlwaysReturns { get; set; } = false;    

    public ResolveInfo()
    {
        Type = SepiaTypeInfo.Void;
    }

    public ResolveInfo(SepiaTypeInfo type)
    {
        Type = type;
    }

    public virtual ResolveInfo Clone(int? steps = null) => new()
    {
        Type = Type,
        Index = Index,
        Steps = steps?? Steps,
        AlwaysReturns = AlwaysReturns,
        FunctionResolveInfo = FunctionResolveInfo?.Clone()
    };

    public virtual bool TypeEqual(ResolveInfo other)
    {
        if (this == null) return other == null;
        if (other == null) return false;
        if (FunctionResolveInfo == null ^ other.FunctionResolveInfo == null) return false;
        return Type == other.Type && (FunctionResolveInfo == null || other.FunctionResolveInfo == null || FunctionResolveInfo.TypeEqual(other.FunctionResolveInfo));
    }
}

public class FunctionResolveInfo
{
    public SepiaTypeInfo ReturnType { get; set; }

    public List<ResolveInfo> Arguments { get; set; }

    public FunctionResolveInfo()
    {
        ReturnType = SepiaTypeInfo.Void;
        Arguments = new();
    }

    public FunctionResolveInfo(SepiaTypeInfo returnType)
    {
        ReturnType = returnType;
        Arguments = new();
    }

    public FunctionResolveInfo(SepiaTypeInfo returnType, List<ResolveInfo> arguments)
    {
        ReturnType = returnType;
        Arguments = arguments;
    }

    public FunctionResolveInfo Clone() => new FunctionResolveInfo()
    {
        ReturnType = ReturnType,
        Arguments = Arguments.Select(a => a.Clone()).ToList()
    };

    public bool TypeEqual(FunctionResolveInfo other)
    {
        if (this == null) return other == null;
        if (other == null) return false;

        if (ReturnType != other.ReturnType) return false;
        if (Arguments.Count != other.Arguments.Count) return false;

        for (int i = 0; i < Arguments.Count; i++)
        {
            var this_arg = Arguments[i];
            var other_arg = other.Arguments[i];

            if (!this_arg.TypeEqual(other_arg)) return false;
        }

        return true;
    }
}