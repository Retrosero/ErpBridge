using ErpBridge.Core.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Log Merkezi L3c/L3d: runs <see cref="AgentLogShipper"/> for the Windows Service and reports the service's own
/// lifecycle — started (with version), stopped, and a previous run that ended without stopping (crash, kill, power
/// loss), detected by a marker file left in the log folder.
/// </summary>
public sealed class AgentLogShipperWorker : BackgroundService
{
    private readonly AgentLogShipper _shipper;
    private readonly ILogger<AgentLogShipperWorker> _logger;
    private readonly string _runningMarker;

    public AgentLogShipperWorker(AgentLogShipper shipper, ILogger<AgentLogShipperWorker> logger)
        : this(shipper, logger, Path.Combine(ErpBridge.Core.Logging.AgentLogLocation.Directory(), "agent-service.running")) { }

    internal AgentLogShipperWorker(AgentLogShipper shipper, ILogger<AgentLogShipperWorker> logger, string runningMarker)
    {
        _shipper = shipper;
        _logger = logger;
        _runningMarker = runningMarker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ReportStart();
        await _shipper.RunAsync(stoppingToken).ConfigureAwait(false);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Ship(LogLevel.Information, "AGENT_STOPPED", "ErpBridge agent service stopping.");
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
        // Last chance for what was logged during shutdown; bounded so a dead network cannot hold the stop.
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await _shipper.FlushOnceAsync(timeout.Token).ConfigureAwait(false);
        TryDelete(_runningMarker);
    }

    private void ReportStart()
    {
        if (File.Exists(_runningMarker))
        {
            Ship(LogLevel.Warning, "AGENT_UNCLEAN_SHUTDOWN",
                "The previous agent service run ended without a normal stop (crash, killed process or power loss).");
        }
        try { File.WriteAllText(_runningMarker, DateTimeOffset.UtcNow.ToString("O")); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }

        var version = typeof(AgentLogShipperWorker).Assembly.GetName().Version?.ToString() ?? "unknown";
        Ship(LogLevel.Information, "AGENT_STARTED", $"ErpBridge agent service {version} started on {Environment.OSVersion.VersionString}.");
    }

    private void Ship(LogLevel level, string kind, string message)
    {
        using (_logger.BeginScope(new Dictionary<string, object> { ["Kind"] = kind, [ErpBridge.Agent.Logging.AgentLogBufferSink.ShipProperty] = true }))
        {
            _logger.Log(level, "{Message}", message);
        }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }
}
