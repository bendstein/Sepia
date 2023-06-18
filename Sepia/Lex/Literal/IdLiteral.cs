using Sepia.Analyzer;
using Sepia.Value.Type;

namespace Sepia.Lex.Literal;

public class IdLiteral : LiteralBase
{
    public ResolveInfo ResolveInfo { get; set; }

    public override SepiaTypeInfo Type => ResolveInfo.Type;

    public IdLiteral(ResolveInfo info)
    {
        ResolveInfo = info;
    }

    public override string ToString() => ResolveInfo.Name?? Type.ToString();
}
