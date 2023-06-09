using Sepia.Lex;

namespace Sepia.Common;

public class SepiaControlFlow : Exception
{
    public Token Token { get; }

    public SepiaControlFlow(Token token)
    {
        Token = token;
    }
}
