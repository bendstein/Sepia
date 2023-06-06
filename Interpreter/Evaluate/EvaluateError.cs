using Interpreter.Common;

namespace Interpreter.Evaluate;

public class EvaluateError : InterpretError
{
    public EvaluateError(string? message = null, Location? location = null, Dictionary<string, object>? data = null)
        : base(message, location, data) { }

    public EvaluateError(string? message = null, Location? location = null, params (string key, object value)[] data)
        : base(message, location, data) { }
}
