namespace ErpBridge.Core.Logging;

/// <summary>
/// Log Merkezi L3g: the trace id of whatever the agent is doing right now. It travels with the async flow, so
/// every log line and every HTTP request made while handling a job carries the id the job was booked with —
/// and one search in the Log Centre shows the phone's request, the ERP write and the ack together.
/// </summary>
public static class AgentCorrelation
{
    /// <summary>The header both sides use.</summary>
    public const string HeaderName = "X-Correlation-Id";

    public const int MaxLength = 128;

    private static readonly AsyncLocal<string?> Ambient = new();

    /// <summary>The id of the work in progress, or null outside any scope.</summary>
    public static string? Current => Ambient.Value;

    /// <summary>
    /// Runs the following work under <paramref name="correlationId"/>. A null or malformed id gets a fresh
    /// one, so an older server that books jobs without an id still produces a traceable agent side.
    /// </summary>
    public static IDisposable Begin(string? correlationId)
    {
        var previous = Ambient.Value;
        Ambient.Value = Sanitize(correlationId) ?? Guid.NewGuid().ToString();
        return new Scope(previous);
    }

    /// <summary>The same shape the server accepts: 1–128 characters of <c>[A-Za-z0-9._:-]</c>.</summary>
    public static string? Sanitize(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxLength) return null;
        return trimmed.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '.' or '_' or ':' or '-') ? trimmed : null;
    }

    private sealed class Scope(string? previous) : IDisposable
    {
        public void Dispose() => Ambient.Value = previous;
    }
}
