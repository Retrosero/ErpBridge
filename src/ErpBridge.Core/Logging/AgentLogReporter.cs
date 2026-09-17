using System.Reflection;
using System.Text.Json;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Shared;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Core.Logging;

/// <summary>
/// What the agent reports to the Log Centre (Log Merkezi L3c). Every call is best effort: a reporting failure is
/// a warning in the local log, never an exception into the caller's path. Messages, exception texts and property
/// values go through <see cref="ConnectionStringMasker"/> first, so a connection string or a token never leaves
/// the machine. The same problem is sent once per <see cref="IAgentLogStore.ThrottleWindow"/>; repeats in between
/// only raise the event's repeat count.
/// </summary>
public interface IAgentLogReporter
{
    /// <summary>Reports an event; returns false when the throttle counted it instead of queueing it.</summary>
    Task<bool> ReportAsync(
        string severity,
        string kind,
        string? operation = null,
        string? message = null,
        Exception? exception = null,
        IReadOnlyDictionary<string, object?>? properties = null,
        string? correlationId = null,
        CancellationToken ct = default);
}

/// <inheritdoc cref="IAgentLogReporter" />
public sealed class AgentLogReporter : IAgentLogReporter
{
    /// <summary>Bounds what one event may carry; the server bounds again.</summary>
    public const int MaxMessage = 2000, MaxStackTrace = 8000, MaxProperties = 4000;

    private readonly IAgentLogStore _store;
    private readonly ILogger<AgentLogReporter> _logger;
    private readonly TimeProvider _clock;
    private readonly string _source;
    private readonly string _appVersion;

    public AgentLogReporter(IAgentLogStore store, ILogger<AgentLogReporter> logger)
        : this(store, logger, TimeProvider.System, AgentLogSources.Service)
    {
    }

    public AgentLogReporter(IAgentLogStore store, ILogger<AgentLogReporter> logger, TimeProvider clock, string source)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _source = AgentLogSources.IsKnown(source) ? source : AgentLogSources.Service;
        _appVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";
    }

    /// <inheritdoc />
    public async Task<bool> ReportAsync(
        string severity,
        string kind,
        string? operation = null,
        string? message = null,
        Exception? exception = null,
        IReadOnlyDictionary<string, object?>? properties = null,
        string? correlationId = null,
        CancellationToken ct = default)
    {
        try
        {
            var now = _clock.GetUtcNow();
            var text = Mask(message ?? exception?.Message, MaxMessage);
            var logEvent = new AgentLogEvent(
                EventId: Guid.NewGuid().ToString(),
                OccurredAtUtc: now,
                Severity: Normalize(severity),
                Kind: NormalizeKind(kind),
                Operation: Trim(operation, 160),
                Message: text,
                ExceptionType: exception?.GetType().FullName,
                StackTrace: Mask(exception?.ToString(), MaxStackTrace),
                AppVersion: _appVersion,
                OsVersion: Environment.OSVersion.VersionString,
                MachineName: Environment.MachineName,
                // Log Merkezi L3g: whatever the agent is doing right now names the event, unless the caller said.
                CorrelationId: Trim(correlationId ?? AgentCorrelation.Current, 128),
                PropertiesJson: Properties(properties),
                Source: _source,
                Fingerprint: AgentLogFingerprint.Of(_source, NormalizeKind(kind), exception?.GetType().FullName, operation, text));

            return await _store.EnqueueAsync(logEvent, now - IAgentLogStore.ThrottleWindow, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Diagnostics must never break the work being diagnosed.
            _logger.LogWarning(ex, "Could not queue the diagnostic event {Kind}.", kind);
            return false;
        }
    }

    private static string Normalize(string? severity) => (severity ?? string.Empty).Trim().ToUpperInvariant() switch
    {
        "TRACE" or "VERBOSE" or "DEBUG" => "DEBUG",
        "INFO" or "INFORMATION" => "INFO",
        "WARN" or "WARNING" => "WARN",
        "FATAL" or "CRITICAL" => "FATAL",
        _ => "ERROR",
    };

    /// <summary>Upper-case ASCII, as the Log Centre stores kinds.</summary>
    public static string NormalizeKind(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind)) return "UNKNOWN";
        var chars = kind.Trim().Select(ch => char.IsAsciiLetterOrDigit(ch) ? char.ToUpperInvariant(ch) : '_').ToArray();
        return new string(chars, 0, Math.Min(chars.Length, 64));
    }

    private static string? Trim(string? value, int max) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];

    private static string? Mask(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var masked = ConnectionStringMasker.MaskSecrets(value);
        return masked.Length <= max ? masked : masked[..max];
    }

    private static string? Properties(IReadOnlyDictionary<string, object?>? properties)
    {
        if (properties is null || properties.Count == 0) return null;
        var masked = properties.ToDictionary(
            pair => pair.Key,
            pair => pair.Value is string text ? ConnectionStringMasker.MaskSecrets(text) : pair.Value);
        var json = JsonSerializer.Serialize(masked);
        return json.Length <= MaxProperties ? json : null;
    }
}
