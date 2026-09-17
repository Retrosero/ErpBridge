using System.Reflection;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Shared;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.UI.Services;

/// <summary>
/// Best-effort, privacy-scrubbed desktop diagnostics. Failures to report must
/// never interfere with the operator's current action or crash the agent.
/// </summary>
public sealed class DesktopAgentTelemetryReporter
{
    private readonly IRemoteApiClient _remoteApi;
    private readonly ILogger<DesktopAgentTelemetryReporter> _logger;

    public DesktopAgentTelemetryReporter(
        IRemoteApiClient remoteApi,
        ILogger<DesktopAgentTelemetryReporter> logger)
    {
        _remoteApi = remoteApi ?? throw new ArgumentNullException(nameof(remoteApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ReportExceptionAsync(Exception exception, string operation, string severity = "ERROR")
    {
        ArgumentNullException.ThrowIfNull(exception);
        try
        {
            var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";
            await _remoteApi.SendAgentTelemetryAsync(new AgentTelemetryEvent
            {
                OccurredAtUtc = DateTimeOffset.UtcNow,
                Kind = "desktop_exception",
                Severity = severity,
                AppVersion = assemblyVersion,
                WindowsVersion = Environment.OSVersion.VersionString,
                MachineName = Environment.MachineName,
                Operation = Limit(operation, 120),
                ExceptionType = Limit(exception.GetType().Name, 160),
                Message = Limit(Scrub(exception.Message), 1000),
                StackTrace = Limit(Scrub(exception.ToString()), 4000),
            }).ConfigureAwait(false);
        }
        catch (Exception reportFailure)
        {
            // Do not use the remote reporter recursively. Local Serilog remains
            // the fallback when the server is offline or the JWT has expired.
            _logger.LogDebug(reportFailure, "Desktop telemetry could not be sent.");
        }
    }

    private static string Limit(string? value, int length)
    {
        var text = value ?? string.Empty;
        return text.Length <= length ? text : text[..length];
    }

    private static string Scrub(string? value) => ConnectionStringMasker.MaskSecrets(value);
}
