using Sepia.Common;

namespace Sepia.Lex;

public class LexError : SepiaError
{
    public LexError(string? message = null, Location? location = null, Dictionary<string, object>? data = null)
        : base(message, location, data) { }

    public LexError(string? message = null, Location? location = null, params (string key, object value)[] data)
        : base (message, location, data) { }
}