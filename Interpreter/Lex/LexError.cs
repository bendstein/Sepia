using Interpreter.Common;

namespace Interpreter.Lex;

public class LexError : InterpretError
{
    public LexError(string? message = null) : base(message)
    {

    }
}