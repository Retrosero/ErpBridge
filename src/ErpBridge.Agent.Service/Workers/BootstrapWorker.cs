using ErpBridge.Agent.Service.Configuration;
using ErpBridge.Core.Sync;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Hosts <see cref="AgentSyncLoop"/> inside the Windows Service. One iteration =
/// one change-log pass plus (in trigger mode) one bootstrap snapshot delta.
///
/// <para>The cadence, the mode switch and the error handling all live in
/// <see cref="AgentSyncLoop"/> so the WPF agent — which has no generic host and
/// therefore cannot register a <see cref="BackgroundService"/> — runs the exact
/// same cycle. This class is only the hosting shim.</para>
/// </summary>
public sealed class BootstrapWorker : BackgroundService
{
    private readonly AgentSyncLoop _loop;
    private readonly ILogger<BootstrapWorker> _logger;

    /// <summary>
    /// Build the worker. <paramref name="services"/> is the root provider —
    /// the loop opens a fresh scope per iteration.
    /// </summary>
    public BootstrapWorker(
        IServiceProvider services,
        IOptions<AgentServiceOptions> options,
        ILogger<BootstrapWorker> logger,
        ILogger<AgentSyncLoop> loopLogger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(options);
        _loop = new AgentSyncLoop(services, options.Value.ToSyncLoopOptions(), loopLogger);
    }

    /// <inheritdoc />
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BootstrapWorker starting ({Cadence}).", _loop.DescribeCadence());
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _loop.RunAsync(stoppingToken).ConfigureAwait(false);
        _logger.LogInformation("BootstrapWorker stopped.");
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BootstrapWorker stopping; cancelling in-flight iteration.");
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
