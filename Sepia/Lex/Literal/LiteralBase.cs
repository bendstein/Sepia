using Sepia.Value.Type;

namespace Sepia.Lex.Literal;

public abstract class LiteralBase 
{ 
    public abstract SepiaTypeInfo Type { get; }
}