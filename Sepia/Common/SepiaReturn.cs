using Sepia.Lex;
using Sepia.Value;

namespace Sepia.Common;
public class SepiaReturn : SepiaControlFlow
{
    public SepiaValue Value { get; init; }

    public SepiaReturn(Token token, SepiaValue value) : base(token)
    {
        Value = value;
    }
}