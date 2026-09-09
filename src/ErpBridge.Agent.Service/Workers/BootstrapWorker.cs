using ErpBridge.Agent.Service.Configuration;
using ErpBridge.Core.Stores;
using ErpBridge.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Periodic background worker that drives <see cref="IBootstrapSyncService"/>.
/// One iteration = one delta (or first-time full) bootstrap cycle
/// (read Mikro → push central API).
///
/// Phase 9 switched the cadence from 60 minutes to 60 seconds so the WPF
/// desktop UI can surface new data within ~1 minute of an ERP change. The
/// first iteration is delayed <see cref="AgentServiceOptions.BootstrapFirstRunDelaySeconds"/>
/// seconds (default 5 s) so the service can finish booting (DB migrations,
/// config load, Mikro connection pool warmup) before the first push is
/// attempted. The interval itself comes from
/// <see cref="AgentServiceOptions.BootstrapIntervalSeconds"/> — operators can
/// override it from <c>appsettings.json</c> without rebuilding the agent.
/// </summary>
/// <remarks>
/// The worker is intentionally lightweight: the actual retry / checkpoint /
/// orchestration logic lives in <see cref="IBootstrapSyncService"/>. The
/// worker only (a) opens a DI scope per iteration (because BootstrapSyncService
/// may eventually be scoped once typed payload readers land) and (b) catches
/// unexpected throws so a programmer bug cannot kill the loop.
/// </remarks>
public sealed class BootstrapWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BootstrapWorker> _logger;
    private readonly AgentServiceOptions _options;

    /// <summary>
    /// Build the worker. <paramref name="services"/> is the root provider —
    /// each iteration creates a fresh scope because <see cref="IBootstrapSyncService"/>
    /// is registered as a singleton but future collaborators (typed readers,
    /// mapping store) are scoped.
    /// </summary>
    public BootstrapWorker(
        IServiceProvider services,
        IOptions<AgentServiceOptions> options,
        ILogger<BootstrapWorker> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        if (_options.BootstrapIntervalSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                $"AgentServiceOptions.BootstrapIntervalSeconds must be positive (got {_options.BootstrapIntervalSeconds}).");
        }
    }

    /// <inheritdoc />
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "BootstrapWorker starting (interval = {IntervalSeconds}s, first run delayed {FirstDelay}s, mode = {Mode}).",
            _options.BootstrapIntervalSeconds, _options.BootstrapFirstRunDelaySeconds,
            _options.UseTriggerBasedSync ? "trigger" : "watermark");
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Defer the first push so the service can finish booting (DB
        // migrations, config load, Mikro connection warmup) without the
        // bootstrap push competing for resources on the first tick.
        var firstDelay = TimeSpan.FromSeconds(Math.Max(0, _options.BootstrapFirstRunDelaySeconds));
        if (firstDelay > TimeSpan.Zero)
        {
            try
            {
                await Task.Delay(firstDelay, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }

        var interval = TimeSpan.FromSeconds(_options.BootstrapIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunSingleIterationAsync(stoppingToken).ConfigureAwait(false);

            try
            {
                await Task.Delay(interval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("BootstrapWorker stopped.");
    }

    /// <summary>
    /// Open a DI scope, resolve either <see cref="IBootstrapSyncService"/>
    /// or <see cref="IErpChangeLogSyncService"/> based on the operator's
    /// <see cref="AgentServiceOptions.UseTriggerBasedSync"/> toggle, and run
    /// a single cycle. Surfaces every result via the logger so the operator
    /// can correlate log lines with the corresponding checkpoint.
    /// </summary>
    private async Task RunSingleIterationAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            if (_options.UseTriggerBasedSync)
            {
                await RunTriggerIterationAsync(scope, stoppingToken).ConfigureAwait(false);

                // The change-log path carries deletes to the mobile master-data
                // consumers but not inserts/updates — those still travel as
                // snapshot deltas. Run that cycle too unless the operator opted
                // out (e.g. an ERP with no *_lastup_date).
                if (_options.RefreshSnapshotInTriggerMode)
                {
                    await RunLegacyIterationAsync(scope, stoppingToken).ConfigureAwait(false);
                }
            }
            else
            {
                await RunLegacyIterationAsync(scope, stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // graceful shutdown
            return;
        }
        catch (Exception ex)
        {
            // The orchestrator is supposed to return a failed result for every
            // known business error. Anything escaping here is a programmer
            // bug — log loudly and keep the loop alive.
            _logger.LogError(ex, "BootstrapWorker iteration crashed unexpectedly.");
            return;
        }
    }

    private async Task RunLegacyIterationAsync(IServiceScope scope, CancellationToken stoppingToken)
    {
        var sync = scope.ServiceProvider.GetRequiredService<IBootstrapSyncService>();
        var result = await sync.RunOnceAsync(stoppingToken).ConfigureAwait(false);
        if (!result.Success)
        {
            _logger.LogWarning(
                "Bootstrap sync failed: code={Code} message={Message} duration={D}ms",
                result.ErrorCode, result.ErrorMessage, result.DurationMs);
            return;
        }
        if (result.CustomersCount == 0 && result.StocksCount == 0 && result.PricesCount == 0
            && result.InventoryCount == 0 && result.OpenOrdersCount == 0
            && result.CashAndBankCount == 0 && result.LookupsCount == 0
            && result.CustomerAddressesCount == 0 && result.CustomerContactsCount == 0
            && result.CustomerTransactionsCount == 0 && result.StockTransactionsCount == 0
            && result.BarcodesCount == 0 && result.SalesConditionsCount == 0)
        {
            _logger.LogDebug("Bootstrap sync skipped (idempotency window active).");
            return;
        }
        _logger.LogInformation(
            "Bootstrap sync completed: ok={Ok} customers={C} stocks={S} prices={P} inventory={I} openOrders={O} cashAndBank={CB} lookups={L} duration={D}ms",
            result.Success, result.CustomersCount, result.StocksCount, result.PricesCount,
            result.InventoryCount, result.OpenOrdersCount, result.CashAndBankCount,
            result.LookupsCount, result.DurationMs);
    }

    private async Task RunTriggerIterationAsync(IServiceScope scope, CancellationToken stoppingToken)
    {
        // The sync service owns installation now: it checks the change log's
        // own IsInstalledAsync and installs when missing, so the worker no
        // longer reaches for a Mikro-specific installer. That is what lets this
        // method stay free of any vendor type.
        var sync = scope.ServiceProvider.GetRequiredService<IErpChangeLogSyncService>();
        var result = await sync.RunOnceAsync(stoppingToken).ConfigureAwait(false);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Change-log sync failed: code={Code} message={Message} duration={D}ms",
                result.ErrorCode, result.ErrorMessage, result.DurationMs);
            return;
        }

        if (result.TotalRowsPushed == 0)
        {
            _logger.LogDebug("Change-log sync skipped (no ERP changes).");
            return;
        }

        _logger.LogInformation(
            "Change-log sync completed: tables={T} upserts={U} deletes={D} more={More} duration={Ms}ms",
            result.TablesTouched, result.UpsertRowsPushed, result.DeleteRowsPushed,
            result.MoreAvailable, result.DurationMs);
    }


    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BootstrapWorker stopping; cancelling in-flight iteration.");
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
