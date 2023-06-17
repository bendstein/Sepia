using Sepia.Value.Type;
using System;

namespace Sepia.Analyzer;

public class ResolveInfo
{
    public SepiaTypeInfo Type { get; set; }

    public int Index { get; set; } = 0;

    public int Steps { get; set; } = 0;

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
        Steps = steps?? Steps
    };

    public virtual bool TypeEqual(ResolveInfo other)
    {
        if (this == null) return other == null;
        if (other == null) return false;
        return Type == other.Type;
    }
}

public class FunctionResolveInfo : ResolveInfo
{
    public SepiaTypeInfo ReturnType { get; set; }

    public List<ResolveInfo> Arguments { get; set; }

    public FunctionResolveInfo() : base(SepiaTypeInfo.Function)
    {
        ReturnType = SepiaTypeInfo.Void;
        Arguments = new();
    }

    public FunctionResolveInfo(SepiaTypeInfo returnType) : base(SepiaTypeInfo.Function)
    {
        ReturnType = returnType;
        Arguments = new();
    }

    public FunctionResolveInfo(SepiaTypeInfo returnType, List<ResolveInfo> arguments) : base(SepiaTypeInfo.Function)
    {
        ReturnType = returnType;
        Arguments = arguments;
    }

    public override ResolveInfo Clone(int? steps = null) => new FunctionResolveInfo()
    {
        ReturnType = ReturnType,
        Type = Type,
        Arguments = Arguments.Select(a => a.Clone()).ToList(),
        Index = Index,
        Steps = steps?? Steps
    };

    public override bool TypeEqual(ResolveInfo other)
    {
        if (this == null) return other == null;
        if (other == null) return false;
        if (!base.Equals(other)) return false;

        if(other is FunctionResolveInfo fother)
        {
            if(ReturnType != fother.ReturnType) return false;
            if(Arguments.Count != fother.Arguments.Count) return false;

            for(int i = 0; i < Arguments.Count; i++)
            {
                var this_arg = Arguments[i];
                var other_arg = fother.Arguments[i];

                if (!this_arg.TypeEqual(other_arg)) return false;
            }
        }

        return true;
    }
}