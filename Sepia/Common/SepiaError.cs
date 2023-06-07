namespace Sepia.Common;

public class SepiaError
{
    public string Message { get; init; }

    public Location Location { get; init; } = (0, 0, 0, 0);

    public Dictionary<string, object> Data { get; init; } = new();

    public SepiaError(string? message = null, Location? location = null, Dictionary<string, object>? data = null)
    {
        Message = message?? string.Empty;
        Location = location?? new();
        Data = data?? new();
    }

    public SepiaError(string? message = null, Location? location = null, params (string key, object value)[] data)
        : this(message, location, data.ToDictionary(d => d.key, d => d.value)) { }

    public override string ToString() => $"{Message} (at: {Location})";
}