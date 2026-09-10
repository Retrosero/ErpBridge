using ErpBridge.Core.Sync;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.UI.Services;

/// <summary>
/// Runs <see cref="AgentSyncLoop"/> for the lifetime of the desktop agent.
///
/// <para>The Windows Service hosts the same loop through a
/// <c>BackgroundService</c>, but this process builds a bare
/// <c>ServiceCollection</c> with no generic host, so there is nothing for a
/// hosted service to attach to. Without this the desktop agent pushed a
/// change-set only when the operator clicked "Senkronize Et" — an ERP change
/// reached the mobile clients whenever somebody happened to press a button, and
/// never otherwise.</para>
///
/// <para>Lifecycle mirrors <c>IDesktopSignalService</c>: <see cref="Start"/> from
/// <c>App.OnStartup</c>, <see cref="StopAsync"/> before the DI container is
/// disposed so the loop cannot resolve torn-down services.</para>
/// </summary>
public sealed class DesktopBackgroundSyncService : IDisposable
{
    /// <summary>Configuration section shared with the Windows Service so one appsettings.json drives both hosts.</summary>
    public const string SectionName = "AgentService";

    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DesktopBackgroundSyncService> _logger;

    private CancellationTokenSource? _cts;
    private Task? _loop;

    /// <summary>DI constructor.</summary>
    public DesktopBackgroundSyncService(
        IServiceProvider services,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        ILogger<DesktopBackgroundSyncService> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>True while the loop task is alive.</summary>
    public bool IsRunning => _loop is { IsCompleted: false };

    /// <summary>
    /// Start the periodic cycle unless the operator disabled it via
    /// <c>AgentService:BackgroundSyncEnabled</c>. Safe to call twice — the
    /// second call is a no-op.
    /// </summary>
    public void Start()
    {
        if (_loop is not null) return;

        if (!_configuration.GetValue($"{SectionName}:BackgroundSyncEnabled", true))
        {
            _logger.LogInformation(
                "Background sync disabled by configuration; the agent will only sync when the operator asks.");
            return;
        }

        AgentSyncLoop loop;
        try
        {
            loop = new AgentSyncLoop(_services, ReadOptions(), _loggerFactory.CreateLogger<AgentSyncLoop>());
        }
        catch (ArgumentOutOfRangeException ex)
        {
            // A bad interval in appsettings.json must not take the UI down.
            _logger.LogError(ex, "Background sync not started: invalid cadence configuration.");
            return;
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _logger.LogInformation("Desktop background sync starting ({Cadence}).", loop.DescribeCadence());

        // Long-running: the loop owns its own error handling and only returns
        // when the token is cancelled.
        _loop = Task.Run(() => loop.RunAsync(token), CancellationToken.None);
    }

    /// <summary>Cancel the loop and wait for the in-flight iteration to unwind.</summary>
    public async Task StopAsync()
    {
        if (_cts is null || _loop is null) return;

        _logger.LogInformation("Desktop background sync stopping.");
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
            _logger.LogWarning(ex, "Desktop background sync did not stop cleanly.");
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            _loop = null;
        }
    }

    private AgentSyncLoopOptions ReadOptions() => new(
        IntervalSeconds: _configuration.GetValue($"{SectionName}:BootstrapIntervalSeconds", 20),
        FirstRunDelaySeconds: _configuration.GetValue($"{SectionName}:BootstrapFirstRunDelaySeconds", 5),
        UseTriggerBasedSync: _configuration.GetValue($"{SectionName}:UseTriggerBasedSync", true),
        RefreshSnapshotInTriggerMode: _configuration.GetValue($"{SectionName}:RefreshSnapshotInTriggerMode", true));

    /// <inheritdoc />
    public void Dispose()
    {
        _cts?.Dispose();
        _cts = null;
    }
}
