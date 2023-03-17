namespace Interpreter.Lex;

public class LexError
{
    public string Message { get; set; } = string.Empty;

    public LexError(string? message = null)
    {
        Message = message ?? string.Empty;
    }

    public override string ToString() => Message;
}