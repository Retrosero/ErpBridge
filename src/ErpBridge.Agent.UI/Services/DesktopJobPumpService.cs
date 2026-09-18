using ErpBridge.Core.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.UI.Services;

/// <summary>
/// Runs <see cref="AgentJobPump"/> — the inbound half of the bridge — for the lifetime of the
/// desktop agent.
///
/// <para>The Windows Service hosts the same pump through <c>AgentWorker</c>, but this process
/// builds a bare <c>ServiceCollection</c> with no generic host, so there is nothing for a hosted
/// service to attach to. Without this the desktop agent never asked the central API for pending
/// documents: an order sent from the phone stayed <c>pending</c> on the server for ever and was
/// never written to the ERP, with nothing in the log to say so. Same shape and lifecycle as
/// <see cref="DesktopBackgroundSyncService"/>, which fixed the outbound half.</para>
/// </summary>
public sealed class DesktopJobPumpService : IDisposable
{
    /// <summary>Configuration section shared with the Windows Service so one appsettings.json drives both hosts.</summary>
    public const string SectionName = "AgentService";

    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DesktopJobPumpService> _logger;

    private CancellationTokenSource? _cts;
    private Task? _loop;

    /// <summary>DI constructor.</summary>
    public DesktopJobPumpService(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger<DesktopJobPumpService> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>True while the loop task is alive.</summary>
    public bool IsRunning => _loop is { IsCompleted: false };

    /// <summary>
    /// Start the poll loop unless the operator disabled it via
    /// <c>AgentService:JobPumpEnabled</c>. Safe to call twice — the second call is a no-op.
    /// </summary>
    public void Start()
    {
        if (_loop is not null) return;

        if (!_configuration.GetValue($"{SectionName}:JobPumpEnabled", true))
        {
            _logger.LogWarning(
                "Document writing disabled by configuration; documents sent from the phone will stay pending on the server.");
            return;
        }

        var options = ReadOptions();
        if (options.PollIntervalSeconds <= 0)
        {
            // A bad interval in appsettings.json must not take the UI down, and must not silently
            // leave the customer with no inbound path either.
            _logger.LogError(
                "Job pump not started: AgentService:JobPollIntervalSeconds must be positive (got {Interval}).",
                options.PollIntervalSeconds);
            return;
        }

        var pump = _services.GetRequiredService<AgentJobPump>();

        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _logger.LogInformation("Desktop job pump starting ({Cadence}).", AgentJobPump.DescribeCadence(options));

        // Long-running: the pump owns its own error handling and only returns
        // when the token is cancelled.
        _loop = Task.Run(() => pump.RunAsync(options, token), CancellationToken.None);
    }

    /// <summary>Cancel the loop and wait for the in-flight poll to unwind.</summary>
    public async Task StopAsync()
    {
        if (_cts is null || _loop is null) return;

        _logger.LogInformation("Desktop job pump stopping.");
        await _cts.CancelAsync().ConfigureAwait(false);
        try
        {
            await _loop.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected: cancellation is how the loop ends.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Desktop job pump did not stop cleanly.");
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            _loop = null;
        }
    }

    private AgentJobPumpOptions ReadOptions() => new(
        PollIntervalSeconds: _configuration.GetValue($"{SectionName}:JobPollIntervalSeconds", 30),
        FirstRunDelaySeconds: _configuration.GetValue($"{SectionName}:JobPollFirstRunDelaySeconds", 5));

    /// <inheritdoc />
    public void Dispose()
    {
        _cts?.Dispose();
        _cts = null;
    }
}
