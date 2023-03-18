namespace Interpreter.Common;

public class InterpretError
{
    public string Message { get; set; }

    public InterpretError(string? message = null)
    {
        Message = message?? string.Empty;
    }
    public override string ToString() => Message;
}