using Sepia.Common;
using Sepia.Lex.Literal;
namespace Sepia.Lex;
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

    public Token Clone(Location? new_location = null) => new Token(this)
    {
        Location = new_location?? Location
    };

    public static implicit operator Location(Token token) => token.Location;

    public override string ToString() => $"{TokenType}:{TokenType.GetSymbol()}:{{{{{Literal?.ToString()?? Error?.ToString()?? string.Empty}}}}}:{Location}".ReplaceLineEndings();
}
