using Interpreter.Common;
using Interpreter.Lex.Literal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Lex;
public class Token
{
    public TokenType TokenType { get; private set; } = TokenType.NULL;

    public LiteralBase? Literal { get; private set; } = null;

    public LexError? Error { get; private set; } = null;

    public Location Location { get; private set; } = (0, 0, 0, 0);

    public Token(TokenType tokenType, Location location, LiteralBase? literal = null, LexError? error = null)
    {
        TokenType = tokenType;
        Literal = literal;
        Location = location;
        Error = error;
    }

    public Token(Token other) : this(other.TokenType, other.Location, other.Literal, other.Error) { }

    public Token Clone() => new Token(this);

    public override string ToString() => $"{TokenType}:{TokenType.GetSymbol() ?? string.Empty}:{{{{{Literal?.ToString()?? Error?.ToString()?? string.Empty}}}}}:{Location}".ReplaceLineEndings();
}
