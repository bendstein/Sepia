using Interpreter.Common;

namespace Interpreter.Parse;
public class ParseError : InterpretError
{
    public ParseError(string? message = null) : base(message)
    {

    }
}
