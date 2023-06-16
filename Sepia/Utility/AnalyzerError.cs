using Sepia.Common;

namespace Sepia.Utility;
public class AnalyzerError : SepiaError
{
    public AnalyzerError(string? message = null, Location? location = null, Dictionary<string, object>? data = null)
        : base(message, location, data) { }

    public AnalyzerError(string? message = null, Location? location = null, params (string key, object value)[] data)
        : base(message, location, data) { }
}
