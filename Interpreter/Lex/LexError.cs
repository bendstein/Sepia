using Interpreter.Common;

namespace Interpreter.Lex;

public class LexError : InterpretError
{
    public LexError(string? message = null, Location? location = null, Dictionary<string, object>? data = null)
        : base(message, location, data) { }

    public LexError(string? message = null, Location? location = null, params (string key, object value)[] data)
        : base (message, location, data) { }
}