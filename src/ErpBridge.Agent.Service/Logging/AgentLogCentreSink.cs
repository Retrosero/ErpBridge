using ErpBridge.Core.Logging;
using Serilog.Core;
using Serilog.Events;

namespace ErpBridge.Agent.Logging;

/// <summary>
/// Log Merkezi L3d: every warning and above the agent already writes to its local log also goes to the Log
/// Centre. Hanging this on the existing Serilog pipeline (instead of calling the reporter at each error site)
/// means a path cannot be forgotten — the sync loop, bootstrap, change-log, Mikro connection and version, token
/// renewal, notify, heartbeat, the job worker, the local queue, acks and the reconciliation alarms all report
/// through it. The reporter masks, fingerprints and throttles; the queue drains with the heartbeat (L3c).
/// </summary>
public sealed class AgentLogCentreSink : ILogEventSink
{
    /// <summary>
    /// Categories that must not feed themselves: the reporter and the uploader log their own failures, and
    /// queueing those would only describe the queue.
    /// </summary>
    private static readonly string[] Excluded =
    [
        "ErpBridge.Core.Logging.AgentLogReporter",
        "ErpBridge.Core.Logging.AgentLogUploader",
    ];

    /// <summary>
    /// A call site that reports to the Log Centre itself puts this in its logging scope, so its line is written
    /// locally but not queued twice. Used by the unhandled-exception handlers, which must wait for the queue.
    /// </summary>
    public const string HandledProperty = "LogCentreHandled";

    private readonly Func<IAgentLogReporter?> _reporter;
    private readonly LogEventLevel _minimumLevel;

    /// <param name="reporter">
    /// Resolved on each event, because the logger is built before the container: a null reporter (host still
    /// starting, or a host without the queue) simply means the line stays local.
    /// </param>
    /// <param name="minimumLevel">Lowest level sent; Log Merkezi D5 says warnings and above.</param>
    public AgentLogCentreSink(Func<IAgentLogReporter?> reporter, LogEventLevel minimumLevel = LogEventLevel.Warning)
    {
        _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        _minimumLevel = minimumLevel;
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent is null || logEvent.Level < _minimumLevel) return;
        if (logEvent.Properties.TryGetValue(HandledProperty, out var handled)
            && handled is ScalarValue { Value: true }) return;
        var category = Text(logEvent, "SourceContext");
        if (category is not null && Excluded.Any(prefix => category.StartsWith(prefix, StringComparison.Ordinal))) return;

        var reporter = _reporter();
        if (reporter is null) return;

        var kind = Text(logEvent, "Kind") ?? KindFromCategory(category);
        var operation = Text(logEvent, "Operation");
        var correlationId = Text(logEvent, "CorrelationId");
        var message = logEvent.RenderMessage();
        var properties = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["category"] = category,
            ["level"] = logEvent.Level.ToString(),
        };

        // Fire and forget: logging must never wait on SQLite, and the reporter swallows its own failures.
        _ = reporter.ReportAsync(
            Severity(logEvent.Level), kind, operation, message, logEvent.Exception, properties, correlationId, CancellationToken.None);
    }

    /// <summary>The Log Centre's severities.</summary>
    public static string Severity(LogEventLevel level) => level switch
    {
        LogEventLevel.Fatal => "FATAL",
        LogEventLevel.Error => "ERROR",
        LogEventLevel.Warning => "WARN",
        LogEventLevel.Information => "INFO",
        _ => "DEBUG",
    };

    /// <summary>
    /// <c>ErpBridge.Core.Stores.BootstrapSyncService</c> → <c>BOOTSTRAP_SYNC_SERVICE</c>: the class that logged
    /// the line is what groups it, so a new error path is readable in the Log Centre without any extra wiring.
    /// </summary>
    public static string KindFromCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category)) return "AGENT_LOG";
        var name = category.Split('.').LastOrDefault(part => !string.IsNullOrWhiteSpace(part)) ?? category;
        var builder = new System.Text.StringBuilder(name.Length + 8);
        for (var i = 0; i < name.Length; i++)
        {
            var ch = name[i];
            if (!char.IsAsciiLetterOrDigit(ch))
            {
                builder.Append('_');
                continue;
            }
            if (i > 0 && char.IsAsciiLetterUpper(ch) && !char.IsAsciiLetterUpper(name[i - 1])) builder.Append('_');
            builder.Append(char.ToUpperInvariant(ch));
        }
        return builder.ToString();
    }

    private static string? Text(LogEvent logEvent, string property) =>
        logEvent.Properties.TryGetValue(property, out var value) && value is ScalarValue { Value: { } raw }
            ? raw.ToString()?.Trim('"')
            : null;
}
