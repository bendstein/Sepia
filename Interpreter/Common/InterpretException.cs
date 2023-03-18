using System.Runtime.Serialization;

namespace Interpreter.Common;

public class InterpretException : Exception
{
	public InterpretError? Error { get; set; } = null;

	public InterpretException(string? message = null, Exception? innerException = null) : base(message, innerException)
	{
		if (message != null) Error = new InterpretError(message: message);
	}

    public InterpretException(InterpretError error, Exception? innerException = null) : base(error.Message, innerException)
	{
		Error = error;
	}
}