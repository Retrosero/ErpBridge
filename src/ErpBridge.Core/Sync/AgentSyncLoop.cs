using ErpBridge.Core.Authentication;
using ErpBridge.Core.Logging;
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
/// <param name="KickMinGapSeconds">
/// Ajan hızı A2: a round requested through <see cref="AgentSyncTrigger"/> starts no sooner than this many seconds after
/// the previous round ended, so a burst of ERP writes cannot keep the ERP busy with back-to-back reads.
/// </param>
public sealed record AgentSyncLoopOptions(
    int IntervalSeconds = 20,
    int FirstRunDelaySeconds = 5,
    bool UseTriggerBasedSync = true,
    bool RefreshSnapshotInTriggerMode = true,
    int KickMinGapSeconds = 5);

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
    private readonly AgentSyncTrigger? _trigger;

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
        // Optional: a host (or a test) without the trigger keeps the plain timer cadence.
        _trigger = services.GetService<AgentSyncTrigger>();
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
        + $"mode = {(_options.UseTriggerBasedSync ? "trigger" : "watermark")}, "
        + $"after an ERP write = {(_trigger is null ? "off" : $"at once (min gap {_options.KickMinGapSeconds}s)")}";

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
        var minGap = TimeSpan.FromSeconds(Math.Max(0, _options.KickMinGapSeconds));
        var kicked = false;
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunSingleIterationAsync(kicked, stoppingToken).ConfigureAwait(false);
            var roundEnded = System.Diagnostics.Stopwatch.GetTimestamp();

            // Ajan hızı A2: wait for the interval OR a request from the job pump, whichever comes first.
            if (_trigger is null)
            {
                if (!await DelayAsync(interval, stoppingToken).ConfigureAwait(false)) break;
                kicked = false;
                continue;
            }
            try
            {
                kicked = await _trigger.WaitAsync(interval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            if (!kicked) continue;

            var sinceLastRound = System.Diagnostics.Stopwatch.GetElapsedTime(roundEnded);
            if (sinceLastRound < minGap && !await DelayAsync(minGap - sinceLastRound, stoppingToken).ConfigureAwait(false)) break;
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
    public Task RunSingleIterationAsync(CancellationToken stoppingToken) => RunSingleIterationAsync(kicked: false, stoppingToken);

    /// <summary>
    /// One iteration. <paramref name="kicked"/>: the job pump asked for it right after an ERP write — the snapshot's
    /// idempotency window is cleared first (<see cref="IBootstrapSyncService.InvalidateAsync"/>: only its last-success
    /// time, never a cursor) so the round reads the ERP instead of skipping, and the round reports trigger <c>job</c>.
    /// </summary>
    public async Task RunSingleIterationAsync(bool kicked, CancellationToken stoppingToken)
    {
        var trigger = kicked ? AgentSyncRound.Triggers.Job : AgentSyncRound.Triggers.Timer;
        try
        {
            // Log Merkezi L3g: one round, one trace id. Without this the client's own warning about a call
            // would be filed with no id while the request it describes carried one.
            using var trace = AgentCorrelation.Begin(null);
            using var scope = _services.CreateScope();

            // Renew the bearer token before doing anything with it. The central
            // API issues 60-minute tokens and has no refresh endpoint for
            // agents, so without this the loop spent every tick after the first
            // hour preparing work that the server rejected with 401.
            var tokens = scope.ServiceProvider.GetRequiredService<IAgentTokenService>();
            if (!await tokens.EnsureValidAsync(stoppingToken).ConfigureAwait(false))
            {
                _logger.LogWarning("Skipping sync iteration: no usable agent token.");
                return;
            }

            var runsSnapshot = !_options.UseTriggerBasedSync || _options.RefreshSnapshotInTriggerMode;
            if (kicked && runsSnapshot) await InvalidateSnapshotWindowAsync(scope, stoppingToken).ConfigureAwait(false);

            if (_options.UseTriggerBasedSync)
            {
                await RunChangeLogIterationAsync(scope, trigger, stoppingToken).ConfigureAwait(false);

                // The change-log path carries deletes to the mobile master-data
                // consumers but not inserts/updates — those still travel as
                // snapshot deltas. Run that cycle too unless the operator opted
                // out (e.g. an ERP with no *_lastup_date).
                if (_options.RefreshSnapshotInTriggerMode)
                {
                    await RunSnapshotIterationAsync(scope, trigger, stoppingToken).ConfigureAwait(false);
                }
            }
            else
            {
                await RunSnapshotIterationAsync(scope, trigger, stoppingToken).ConfigureAwait(false);
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

    /// <summary>
    /// Clears the snapshot's 30-second idempotency window. Never the change-log's InvalidateAsync: that one resets the
    /// change-log cursor and would re-read the whole feed. A failure only costs the speed-up — the round still runs.
    /// </summary>
    private async Task InvalidateSnapshotWindowAsync(IServiceScope scope, CancellationToken stoppingToken)
    {
        try
        {
            await scope.ServiceProvider.GetRequiredService<IBootstrapSyncService>().InvalidateAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Could not clear the snapshot window before a sync requested by an ERP write; the round runs anyway.");
        }
    }

    private async Task RunSnapshotIterationAsync(IServiceScope scope, string trigger, CancellationToken stoppingToken)
    {
        var sync = scope.ServiceProvider.GetRequiredService<IBootstrapSyncService>();
        var result = await sync.RunOnceAsync(stoppingToken).ConfigureAwait(false);

        // Log Merkezi L3e: one INFO event per round, counts only. The throttle turns a round every twenty
        // seconds into one event per window with the repeats counted, so the panel sees the rhythm, not a flood.
        await Reporter(scope).ReportAsync(trigger, result, ct: stoppingToken).ConfigureAwait(false);
        // Log Merkezi L3f: the heartbeat's "last sync" is only honest if a round actually sets it.
        Status(scope)?.RecordSync(result.Success, DateTimeOffset.UtcNow, result.ErrorCode, result.ErrorMessage);

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

    /// <summary>
    /// Optional on purpose: a host without the diagnostic queue (and every test that builds the loop with a
    /// bare provider) still runs its rounds, it just does not report them.
    /// </summary>
    private static IAgentLogReporter? Reporter(IServiceScope scope) =>
        scope.ServiceProvider.GetService<IAgentLogReporter>();

    /// <inheritdoc cref="Reporter" />
    private static AgentRunStatus? Status(IServiceScope scope) =>
        scope.ServiceProvider.GetService<AgentRunStatus>();

    private static bool IsEmptyResult(BootstrapSyncResult result) =>
        result.CustomersCount == 0 && result.StocksCount == 0 && result.PricesCount == 0
        && result.InventoryCount == 0 && result.OpenOrdersCount == 0
        && result.CashAndBankCount == 0 && result.LookupsCount == 0
        && result.CustomerAddressesCount == 0 && result.CustomerContactsCount == 0
        && result.CustomerTransactionsCount == 0 && result.StockTransactionsCount == 0
        && result.BarcodesCount == 0 && result.SalesConditionsCount == 0;

    private async Task RunChangeLogIterationAsync(IServiceScope scope, string trigger, CancellationToken stoppingToken)
    {
        // The sync service owns installation: it checks the change log's own
        // IsInstalledAsync and installs when missing, so this method stays free
        // of any vendor type.
        var sync = scope.ServiceProvider.GetRequiredService<IErpChangeLogSyncService>();
        var result = await sync.RunOnceAsync(stoppingToken).ConfigureAwait(false);
        await Reporter(scope).ReportAsync(trigger, result, stoppingToken).ConfigureAwait(false);
        Status(scope)?.RecordSync(result.Success, DateTimeOffset.UtcNow, result.ErrorCode, result.ErrorMessage);

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
