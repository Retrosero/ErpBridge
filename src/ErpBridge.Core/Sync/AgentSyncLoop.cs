using ErpBridge.Core.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Core.Sync;

/// <summary>
/// Cadence and mode for <see cref="AgentSyncLoop"/>.
/// </summary>
/// <param name="IntervalSeconds">Seconds between iterations. Must be positive.</param>
/// <param name="FirstRunDelaySeconds">Seconds to wait before the first iteration, so the host can finish booting.</param>
/// <param name="UseTriggerBasedSync">Drive the ERP change log; otherwise only the snapshot delta runs.</param>
/// <param name="RefreshSnapshotInTriggerMode">In trigger mode, also run the snapshot-delta cycle each iteration.</param>
public sealed record AgentSyncLoopOptions(
    int IntervalSeconds = 20,
    int FirstRunDelaySeconds = 5,
    bool UseTriggerBasedSync = true,
    bool RefreshSnapshotInTriggerMode = true);

/// <summary>
/// The agent's periodic sync cycle: drive the ERP change log and the bootstrap
/// snapshot delta on a fixed cadence.
///
/// <para>This lives in Core, free of <c>Microsoft.Extensions.Hosting</c>, because
/// it has two hosts with nothing else in common. The Windows Service wraps it in
/// a <c>BackgroundService</c>; the WPF agent runs it on a plain background task,
/// since that process builds a bare <c>ServiceCollection</c> and has no generic
/// host to hang a hosted service off. Before this existed only the service ran a
/// loop, so an operator using just the desktop app got no periodic sync at all —
/// change-sets were pushed only when someone clicked the button.</para>
/// </summary>
public sealed class AgentSyncLoop
{
    private readonly IServiceProvider _services;
    private readonly AgentSyncLoopOptions _options;
    private readonly ILogger<AgentSyncLoop> _logger;

    /// <summary>Build the loop. <paramref name="services"/> is the root provider — each iteration opens its own scope.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="options"/> carries a non-positive interval.</exception>
    public AgentSyncLoop(
        IServiceProvider services,
        AgentSyncLoopOptions options,
        ILogger<AgentSyncLoop> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (_options.IntervalSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                $"AgentSyncLoopOptions.IntervalSeconds must be positive (got {_options.IntervalSeconds}).");
        }
    }

    /// <summary>Describes the configured cadence; hosts log this on startup.</summary>
    public string DescribeCadence() =>
        $"interval = {_options.IntervalSeconds}s, first run delayed {_options.FirstRunDelaySeconds}s, "
        + $"mode = {(_options.UseTriggerBasedSync ? "trigger" : "watermark")}";

    /// <summary>
    /// Run until <paramref name="stoppingToken"/> is cancelled. Never throws for
    /// a cancelled token — cancellation is the normal way to stop.
    /// </summary>
    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var firstDelay = TimeSpan.FromSeconds(Math.Max(0, _options.FirstRunDelaySeconds));
        if (firstDelay > TimeSpan.Zero && !await DelayAsync(firstDelay, stoppingToken).ConfigureAwait(false))
        {
            return;
        }

        var interval = TimeSpan.FromSeconds(_options.IntervalSeconds);
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunSingleIterationAsync(stoppingToken).ConfigureAwait(false);
            if (!await DelayAsync(interval, stoppingToken).ConfigureAwait(false)) break;
        }
    }

    /// <summary>False when the wait was cut short by cancellation.</summary>
    private static async Task<bool> DelayAsync(TimeSpan delay, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, ct).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return false;
        }
    }

    /// <summary>
    /// One iteration: open a scope, run the configured cycle(s), and swallow
    /// anything unexpected so a programmer bug cannot kill the loop. Public so
    /// a host can force a single pass without starting the timer.
    /// </summary>
    public async Task RunSingleIterationAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            if (_options.UseTriggerBasedSync)
            {
                await RunChangeLogIterationAsync(scope, stoppingToken).ConfigureAwait(false);

                // The change-log path carries deletes to the mobile master-data
                // consumers but not inserts/updates — those still travel as
                // snapshot deltas. Run that cycle too unless the operator opted
                // out (e.g. an ERP with no *_lastup_date).
                if (_options.RefreshSnapshotInTriggerMode)
                {
                    await RunSnapshotIterationAsync(scope, stoppingToken).ConfigureAwait(false);
                }
            }
            else
            {
                await RunSnapshotIterationAsync(scope, stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // graceful shutdown
        }
        catch (Exception ex)
        {
            // The orchestrators return a failed result for every known business
            // error. Anything escaping here is a programmer bug — log loudly and
            // keep the loop alive.
            _logger.LogError(ex, "Agent sync iteration crashed unexpectedly.");
        }
    }

    private async Task RunSnapshotIterationAsync(IServiceScope scope, CancellationToken stoppingToken)
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
        if (IsEmptyResult(result))
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

    private static bool IsEmptyResult(BootstrapSyncResult result) =>
        result.CustomersCount == 0 && result.StocksCount == 0 && result.PricesCount == 0
        && result.InventoryCount == 0 && result.OpenOrdersCount == 0
        && result.CashAndBankCount == 0 && result.LookupsCount == 0
        && result.CustomerAddressesCount == 0 && result.CustomerContactsCount == 0
        && result.CustomerTransactionsCount == 0 && result.StockTransactionsCount == 0
        && result.BarcodesCount == 0 && result.SalesConditionsCount == 0;

    private async Task RunChangeLogIterationAsync(IServiceScope scope, CancellationToken stoppingToken)
    {
        // The sync service owns installation: it checks the change log's own
        // IsInstalledAsync and installs when missing, so this method stays free
        // of any vendor type.
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
}
