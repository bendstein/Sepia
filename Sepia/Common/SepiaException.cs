using System.Runtime.Serialization;

namespace Sepia.Common;

public class SepiaException : Exception
{
	public SepiaError? Error { get; set; } = null;

	public SepiaException(string? message = null, Exception? innerException = null) : base(message, innerException)
	{
		if (message != null) Error = new SepiaError(message: message);
	}

    public SepiaException(SepiaError error, Exception? innerException = null) : base(error.ToString(), innerException)
	{
		Error = error;
	}
}