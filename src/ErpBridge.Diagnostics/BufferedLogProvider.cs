using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Diagnostics;

/// <summary>A log line captured for shipping: already formatted, with the well-known context values it carried.</summary>
public sealed record ShippedLog(
    string EventId,
    DateTimeOffset OccurredAtUtc,
    string Severity,
    string Category,
    string Message,
    string? ExceptionType,
    string? StackTrace,
    IReadOnlyDictionary<string, string> Properties);

public class BufferedLogOptions
{
    /// <summary>Lowest level shipped. Log Merkezi D5: warnings and above.</summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Warning;

    /// <summary>Lines kept in memory while shipping is slow or down; beyond this new lines are dropped (and counted).</summary>
    public int Capacity { get; set; } = 5_000;

    public int BatchSize { get; set; } = 100;

    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>Category prefixes never shipped (e.g. the shipper's own transport, or a category logged elsewhere).</summary>
    public List<string> ExcludedCategoryPrefixes { get; set; } = [];
}

/// <summary>
/// Log Merkezi L2: an <see cref="ILoggerProvider"/> that queues log lines in a bounded channel and hands them to
/// <see cref="SendAsync"/> in batches on a background loop. Logging never waits on, fails because of, or loops
/// through the shipping target: a full queue drops (and later reports how many), a failed batch is dropped,
/// and anything logged while a batch is being sent is ignored by this provider.
/// </summary>
public abstract class BufferedLogProvider : ILoggerProvider, ISupportExternalScope
{
    /// <summary>
    /// Context values copied from log state and scopes into <see cref="ShippedLog.Properties"/>. Anything else stays
    /// out: scopes can carry request paths and business values.
    /// </summary>
    public static readonly IReadOnlySet<string> WellKnownKeys = new HashSet<string>(StringComparer.Ordinal)
    {
        "CorrelationId", "TenantId", "UserId", "AgentId", "HttpMethod", "HttpRoute", "HttpStatus", "DurationMs", "Kind", "Operation",
    };

    private static readonly AsyncLocal<bool> Shipping = new();

    private readonly BufferedLogOptions _options;
    private readonly Channel<ShippedLog> _channel;
    private readonly CancellationTokenSource _stopping = new();
    private readonly Task _loop;
    private IExternalScopeProvider? _scopes;
    private long _dropped;
    private long _failedLines;

    protected BufferedLogProvider(BufferedLogOptions options)
    {
        _options = options;
        _channel = Channel.CreateBounded<ShippedLog>(
            new BoundedChannelOptions(Math.Max(1, options.Capacity)) { FullMode = BoundedChannelFullMode.DropWrite, SingleReader = true },
            _ => Interlocked.Increment(ref _dropped));
        _loop = Task.Run(RunAsync);
    }

    /// <summary>Lines dropped because the queue was full, since start.</summary>
    public long DroppedCount => Interlocked.Read(ref _dropped);

    /// <summary>Lines lost because their batch could not be sent, since start.</summary>
    public long FailedCount => Interlocked.Read(ref _failedLines);

    /// <summary>Delivers one batch. Throwing drops the batch; it is counted, never logged.</summary>
    protected abstract Task SendAsync(IReadOnlyList<ShippedLog> batch, CancellationToken ct);

    public ILogger CreateLogger(string categoryName) => new BufferedLogger(this, categoryName);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopes = scopeProvider;

    internal bool IsEnabled(string category, LogLevel level) =>
        level != LogLevel.None
        && level >= _options.MinimumLevel
        && !Shipping.Value
        && !_options.ExcludedCategoryPrefixes.Any(prefix => category.StartsWith(prefix, StringComparison.Ordinal));

    internal void Enqueue<TState>(string category, LogLevel level, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var properties = new Dictionary<string, string>(StringComparer.Ordinal);
        _scopes?.ForEachScope(static (scope, target) => Collect(scope, target), properties);
        Collect(state, properties);
        if (!string.IsNullOrEmpty(eventId.Name)) properties["EventName"] = eventId.Name;

        string message;
        try { message = formatter(state, exception); }
        catch (Exception) { message = state?.ToString() ?? string.Empty; }

        _channel.Writer.TryWrite(new ShippedLog(
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            Severity(level),
            category,
            message,
            exception?.GetType().FullName,
            exception?.ToString(),
            properties));
    }

    private static void Collect(object? values, Dictionary<string, string> target)
    {
        if (values is not IEnumerable<KeyValuePair<string, object?>> pairs) return;
        foreach (var (key, value) in pairs)
        {
            if (value is null || !WellKnownKeys.Contains(key)) continue;
            var text = value.ToString();
            if (!string.IsNullOrWhiteSpace(text)) target[key] = text;
        }
    }

    private static string Severity(LogLevel level) => level switch
    {
        LogLevel.Trace or LogLevel.Debug => "DEBUG",
        LogLevel.Information => "INFO",
        LogLevel.Warning => "WARN",
        LogLevel.Error => "ERROR",
        _ => "FATAL",
    };

    private async Task RunAsync()
    {
        Shipping.Value = true;
        var reader = _channel.Reader;
        var batch = new List<ShippedLog>(_options.BatchSize);
        long reportedLoss = 0;
        try
        {
            while (await reader.WaitToReadAsync(_stopping.Token).ConfigureAwait(false))
            {
                var deadline = Stopwatch.GetTimestamp() + (long)(_options.FlushInterval.TotalSeconds * Stopwatch.Frequency);
                while (batch.Count < _options.BatchSize)
                {
                    if (reader.TryRead(out var line)) { batch.Add(line); continue; }
                    var remaining = deadline - Stopwatch.GetTimestamp();
                    if (remaining <= 0) break;
                    using var wait = CancellationTokenSource.CreateLinkedTokenSource(_stopping.Token);
                    wait.CancelAfter(TimeSpan.FromSeconds((double)remaining / Stopwatch.Frequency));
                    try { if (!await reader.WaitToReadAsync(wait.Token).ConfigureAwait(false)) break; }
                    catch (OperationCanceledException) when (!_stopping.IsCancellationRequested) { break; }
                }

                var loss = DroppedCount + FailedCount;
                if (loss > reportedLoss)
                {
                    batch.Add(new ShippedLog(Guid.NewGuid().ToString(), DateTimeOffset.UtcNow, "WARN", "ErpBridge.Diagnostics",
                        $"Log shipping lost {loss - reportedLoss} line(s) (queue full or target unavailable).", null, null,
                        new Dictionary<string, string> { ["Kind"] = "LOG_SHIPPING_LOSS" }));
                    reportedLoss = loss;
                }
                await SendBatchAsync(batch, _stopping.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (_stopping.IsCancellationRequested)
        {
        }
    }

    private async Task SendBatchAsync(List<ShippedLog> batch, CancellationToken ct)
    {
        if (batch.Count == 0) return;
        try
        {
            await SendAsync(batch.ToArray(), ct).ConfigureAwait(false);
        }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            Interlocked.Add(ref _failedLines, batch.Count);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            batch.Clear();
        }
    }

    /// <summary>Sends what is queued (best effort, bounded by <paramref name="timeout"/>) and stops the loop.</summary>
    public async Task FlushAndStopAsync(TimeSpan timeout)
    {
        _channel.Writer.TryComplete();
        var finished = await Task.WhenAny(_loop, Task.Delay(timeout)).ConfigureAwait(false);
        if (finished != _loop) await _stopping.CancelAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        try { FlushAndStopAsync(TimeSpan.FromSeconds(3)).GetAwaiter().GetResult(); }
        catch (Exception) { }
        _stopping.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed class BufferedLogger(BufferedLogProvider provider, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => provider._scopes?.Push(state);

        public bool IsEnabled(LogLevel logLevel) => provider.IsEnabled(category, logLevel);

        public void Log<TState>(LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            provider.Enqueue(category, logLevel, eventId, state, exception, formatter);
        }
    }
}
