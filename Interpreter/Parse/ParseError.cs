using Interpreter.Common;
using Interpreter.Lex;

namespace Interpreter.Parse;
public class ParseError : InterpretError
{
    public Token? Token { get; init; }

    public ParseError(string? message = null, Location? location = null, Dictionary<string, object>? data = null)
    : base(message, location, data) { }

    public ParseError(string? message = null, Location? location = null, params (string key, object value)[] data)
        : base(message, location, data) { }

    public ParseError(Token token, string? message = null, Dictionary<string, object>? data = null)
        : base(message, token.Location, data)
    {
        Token = token;
    }

    public ParseError(Token token, string? message = null, params (string key, object value)[] data)
        : base(message, token.Location, data)
    {
        Token = token;
    }
}
