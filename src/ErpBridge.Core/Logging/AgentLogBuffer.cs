using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ErpBridge.Shared;

namespace ErpBridge.Core.Logging;

/// <summary>One agent log line on its way to the Log Merkezi (<c>POST /api/v1/agents/logs/batch</c>).</summary>
public sealed class AgentLogEvent
{
    [JsonPropertyName("eventId")] public string EventId { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset OccurredAtUtc { get; set; } = DateTimeOffset.UtcNow;
    [JsonPropertyName("severity")] public string Severity { get; set; } = "WARN";
    [JsonPropertyName("kind")] public string Kind { get; set; } = AgentLogBuffer.DefaultKind;
    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
    [JsonPropertyName("operation")] public string Operation { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("exceptionType")] public string? ExceptionType { get; set; }
    [JsonPropertyName("stackTrace")] public string? StackTrace { get; set; }
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; set; }
    [JsonPropertyName("repeatCount")] public int RepeatCount { get; set; } = 1;
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }
    [JsonPropertyName("osVersion")] public string? OsVersion { get; set; }
    [JsonPropertyName("properties")] public Dictionary<string, string> Properties { get; set; } = [];
}

/// <summary>
/// Log Merkezi L3c: in-memory hand-off between the logging pipeline (a Serilog sink, created before the DI container
/// in the desktop app) and <see cref="AgentLogShipper"/>, which persists and sends. Process-wide by design:
/// <see cref="Shared"/>.
/// <para>
/// Everything is masked (<see cref="ConnectionStringMasker.MaskSecrets"/>) and bounded on the way in. The same problem
/// (kind + category + exception type + message with numbers removed) is buffered once per
/// <see cref="ThrottleWindow"/>; repeats inside the window are counted and travel as <see cref="AgentLogEvent.RepeatCount"/>
/// — an agent that fails every 20 seconds while the ERP is down must not flood the server. Never throws.
/// </para>
/// </summary>
public sealed partial class AgentLogBuffer
{
    public const string DefaultKind = "AGENT_LOG";

    /// <summary>Log scope / property that ships an information line too (lifecycle events, sync rounds).</summary>
    public const string ShipProperty = "ShipToLogCenter";
    public const int Capacity = 2_000;
    public static readonly TimeSpan ThrottleWindow = TimeSpan.FromMinutes(10);

    public static AgentLogBuffer Shared { get; } = new(TimeProvider.System);

    private readonly TimeProvider _clock;
    private readonly ConcurrentQueue<AgentLogEvent> _queue = new();
    private readonly ConcurrentDictionary<string, Throttle> _throttles = new(StringComparer.Ordinal);
    private readonly ConditionalWeakTable<Exception, AgentLogEvent> _byException = new();
    private int _count;
    private long _dropped;

    public AgentLogBuffer(TimeProvider clock) => _clock = clock;

    /// <summary>Lines dropped because the buffer was full and not yet reported.</summary>
    public long PendingDropped => Interlocked.Read(ref _dropped);

    /// <summary>
    /// Buffers <paramref name="entry"/> (a log line). When <paramref name="exception"/> was already buffered by
    /// <see cref="AddReported"/> the line is skipped — the reported event carries the richer operation name.
    /// </summary>
    public void Add(AgentLogEvent entry, Exception? exception = null)
    {
        try
        {
            if (exception is not null && _byException.TryGetValue(exception, out _)) return;
            Prepare(entry, exception);
            if (!PassThrottle(entry)) return;
            if (exception is not null) _byException.AddOrUpdate(exception, entry);
            Enqueue(entry);
        }
        catch (Exception)
        {
            // Diagnostics must never break the caller.
        }
    }

    /// <summary>
    /// An operator action that failed (desktop app). If the same exception object was just logged, that buffered line
    /// gets this operation name instead of a second event.
    /// </summary>
    public void AddReported(Exception exception, string operation, string severity)
    {
        try
        {
            if (_byException.TryGetValue(exception, out var existing))
            {
                existing.Operation = Bound(operation, 160);
                existing.Kind = "DESKTOP_EXCEPTION";
                return;
            }
            Add(new AgentLogEvent { Severity = severity, Kind = "DESKTOP_EXCEPTION", Category = "ErpBridge.Agent.UI", Operation = operation }, exception);
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// Takes up to <paramref name="max"/> lines, plus a summary for every throttled problem whose window ended with
    /// uncounted repeats, plus one line for lines dropped on a full buffer.
    /// </summary>
    public IReadOnlyList<AgentLogEvent> Drain(int max = 500)
    {
        var batch = new List<AgentLogEvent>();
        while (batch.Count < max && _queue.TryDequeue(out var entry))
        {
            Interlocked.Decrement(ref _count);
            batch.Add(entry);
        }

        var now = _clock.GetUtcNow();
        foreach (var (key, throttle) in _throttles)
        {
            if (now - throttle.WindowStart < ThrottleWindow) continue;
            if (!_throttles.TryRemove(new KeyValuePair<string, Throttle>(key, throttle))) continue;
            var repeats = Interlocked.Exchange(ref throttle.Suppressed, 0);
            if (repeats == 0) continue;
            var summary = Clone(throttle.Last);
            summary.EventId = Guid.NewGuid().ToString();
            summary.OccurredAtUtc = now;
            summary.RepeatCount = repeats;
            summary.Properties["throttled"] = "true";
            batch.Add(summary);
        }

        var dropped = Interlocked.Exchange(ref _dropped, 0);
        if (dropped > 0)
        {
            batch.Add(new AgentLogEvent
            {
                Severity = "WARN", Kind = "LOG_SHIPPING_LOSS", Category = "ErpBridge.Core.Logging",
                Message = $"Agent log buffer was full; {dropped} line(s) dropped.",
            });
        }
        return batch;
    }

    private void Enqueue(AgentLogEvent entry)
    {
        if (Interlocked.Increment(ref _count) > Capacity)
        {
            Interlocked.Decrement(ref _count);
            Interlocked.Increment(ref _dropped);
            return;
        }
        _queue.Enqueue(entry);
    }

    private bool PassThrottle(AgentLogEvent entry)
    {
        var key = string.Join('', entry.Kind, entry.Category, entry.ExceptionType, NumberPattern().Replace(entry.Message, "#"));
        var now = _clock.GetUtcNow();
        var throttle = _throttles.GetOrAdd(key, _ => new Throttle(now, entry));
        if (ReferenceEquals(throttle.Last, entry)) return true;
        if (now - throttle.WindowStart >= ThrottleWindow)
        {
            // Window over: this occurrence starts a new one and carries the repeats of the old one.
            entry.RepeatCount += Interlocked.Exchange(ref throttle.Suppressed, 0);
            _throttles[key] = new Throttle(now, entry);
            return true;
        }
        Interlocked.Increment(ref throttle.Suppressed);
        return false;
    }

    private static void Prepare(AgentLogEvent entry, Exception? exception)
    {
        if (exception is not null)
        {
            entry.ExceptionType ??= exception.GetType().FullName;
            entry.StackTrace ??= exception.ToString();
            if (string.IsNullOrWhiteSpace(entry.Message)) entry.Message = exception.Message;
        }
        entry.Message = Bound(ConnectionStringMasker.MaskSecrets(entry.Message), 2000);
        entry.StackTrace = entry.StackTrace is null ? null : Bound(ConnectionStringMasker.MaskSecrets(entry.StackTrace), 8000);
        entry.Operation = Bound(ConnectionStringMasker.MaskSecrets(entry.Operation), 160);
        entry.Category = Bound(entry.Category, 200);
        entry.AppVersion ??= System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        entry.OsVersion ??= Environment.OSVersion.VersionString;
        foreach (var key in entry.Properties.Keys.ToList())
            entry.Properties[key] = Bound(ConnectionStringMasker.MaskSecrets(entry.Properties[key]), 300);
    }

    private static AgentLogEvent Clone(AgentLogEvent source) => new()
    {
        Severity = source.Severity, Kind = source.Kind, Category = source.Category, Operation = source.Operation,
        Message = source.Message, ExceptionType = source.ExceptionType, StackTrace = source.StackTrace,
        CorrelationId = source.CorrelationId, AppVersion = source.AppVersion, OsVersion = source.OsVersion,
        Properties = new Dictionary<string, string>(source.Properties),
    };

    private static string Bound(string? value, int max)
    {
        var text = value ?? string.Empty;
        return text.Length <= max ? text : text[..max];
    }

    [GeneratedRegex(@"\d+", RegexOptions.CultureInvariant)]
    private static partial Regex NumberPattern();

    private sealed class Throttle(DateTimeOffset windowStart, AgentLogEvent last)
    {
        public DateTimeOffset WindowStart { get; } = windowStart;
        public AgentLogEvent Last { get; } = last;
        public int Suppressed;
    }
}
