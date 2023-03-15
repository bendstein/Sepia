using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Lex;
public class Token
{
    public TokenType TokenType { get; private set; } = TokenType.NULL;

    public object? Literal { get; private set; } = null;

    public int Line { get; private set; } = 0;

    public Token(TokenType tokenType, object? literal = null, int line = 0)
    {
        TokenType = tokenType;
        Literal = literal;
        Line = line;
    }

    public Token(Token other) : this(other.TokenType, other.Literal, other.Line) { }

    public Token Clone() => new Token(this);

    public override string ToString() => $"{TokenType}:{TokenType.GetSymbol()?? string.Empty}:{Literal}:{Line}";
}
