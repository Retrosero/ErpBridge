using ErpBridge.Agent.Service.Configuration.Reconciliation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Result of a single reconciliation iteration. Returned by
/// <see cref="CrossDbReconciliationWorker.ReconcileOnceAsync"/> so the
/// background loop and the unit tests can assert on the same surface. The
/// counts are intentionally simple — the worker is an alarm generator, not a
/// repair bot, and the metrics it surfaces are the ones that the operator
/// needs to triage a cross-DB drift incident.
/// </summary>
public sealed record ReconciliationReport(
    int MappingsScanned,
    int Orphans,
    int ProbeErrors,
    bool ThresholdExceeded)
{
    /// <summary>Convenience: a clean scan with no drift signals.</summary>
    public static ReconciliationReport Empty(int scanned) => new(scanned, Orphans: 0, ProbeErrors: 0, ThresholdExceeded: false);
}

/// <summary>
/// Cross-DB reconciliation worker — the read-only safety net for the Phase 6
/// "SQLite mapping + SQL Server tx" boundary. The Mikro writer commits the
/// SQL Server transaction BEFORE the <c>mappings</c> row is written, so a
/// SQLite write failure leaves the ERP with a document and the agent with
/// no idempotency anchor. The next retry then creates a duplicate.
///
/// This worker closes the loop in the OPPOSITE direction: it scans the recent
/// <c>mappings</c> rows and asks Mikro whether the underlying document still
/// exists. The inverse signals (mapping without a Mikro record, or in the
/// future a Mikro record without a mapping) are logged at <c>Warning</c>;
/// when the rolling 24-hour count crosses <c>DailyAlertThreshold</c> the
/// same event is escalated to <c>Error</c> so the central API can page the
/// operator.
///
/// IMPORTANT: this worker is alarm-only. It MUST NOT delete mappings, must
/// NOT insert mapping rows, and must NOT modify the ERP. Phase 16+ may add
/// the operator-driven repair flow; for now the worker simply makes the
/// drift visible.
/// </summary>
public sealed class CrossDbReconciliationWorker : BackgroundService
{
    /// <summary>
    /// Rolling-window duration for the threshold check. 24 hours matches the
    /// Phase-6 brief — kept as a constant so the test can assert the
    /// behaviour without hard-coding 24 in the test itself.
    /// </summary>
    internal static readonly TimeSpan DailyWindow = TimeSpan.FromHours(24);

    private readonly IMappingHistoryQuery _history;
    private readonly IReconciliationProbe _probe;
    private readonly ILogger<CrossDbReconciliationWorker> _logger;
    private readonly IOptionsMonitor<ReconciliationOptions> _optionsMonitor;

    /// <summary>
    /// Monotonically increasing timeline of drift events inside the last
    /// <see cref="DailyWindow"/>. Backed by a thread-safe queue so the
    /// <see cref="ExecuteAsync"/> loop and the in-process
    /// <see cref="ReconcileOnceAsync"/> calls cannot corrupt the list. Each
    /// enqueue also prunes expired entries so the worker does not have to
    /// walk the entire history to compute the daily count.
    /// </summary>
    private readonly System.Collections.Concurrent.ConcurrentQueue<DateTime> _driftEvents = new();

    public CrossDbReconciliationWorker(
        IMappingHistoryQuery history,
        IReconciliationProbe probe,
        IOptionsMonitor<ReconciliationOptions> optionsMonitor,
        ILogger<CrossDbReconciliationWorker> logger)
    {
        _history = history ?? throw new ArgumentNullException(nameof(history));
        _probe = probe ?? throw new ArgumentNullException(nameof(probe));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Read the current options snapshot. Always reads through the monitor so
    /// a live <c>appsettings.json</c> change (e.g. <c>Enabled=false</c> after
    /// an incident) takes effect on the next iteration without a restart.
    /// </summary>
    private ReconciliationOptions CurrentOptions => _optionsMonitor.CurrentValue;

    /// <inheritdoc />
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var snapshot = CurrentOptions;
        _logger.LogInformation(
            "CrossDbReconciliationWorker starting (enabled={Enabled}, period={PeriodSeconds}s, lookback={LookbackMinutes}m, dailyThreshold={DailyAlertThreshold}).",
            snapshot.Enabled, snapshot.PeriodSeconds, snapshot.LookbackMinutes, snapshot.DailyAlertThreshold);
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // First run is delayed by 30 s so the rest of the agent can finish
        // booting (SQLite migrations, Mikro connection pool warmup) before we
        // start hammering the ERP with existence checks.
        try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false); }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            var snapshot = CurrentOptions;
            if (!snapshot.Enabled)
            {
                _logger.LogDebug("CrossDbReconciliationWorker is disabled; sleeping until next tick.");
            }
            else
            {
                try
                {
                    _ = await ReconcileOnceAsync(snapshot, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // A programmer bug must NEVER kill the reconciliation
                    // loop — the next tick will simply try again. Log at
                    // Error so a flapping reconciliation path is visible.
                    _logger.LogError(ex, "CrossDbReconciliationWorker iteration crashed unexpectedly.");
                }
            }

            var period = TimeSpan.FromSeconds(Math.Max(1, snapshot.PeriodSeconds));
            try { await Task.Delay(period, stoppingToken).ConfigureAwait(false); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }

        _logger.LogInformation("CrossDbReconciliationWorker stopped.");
    }

    /// <summary>
    /// Run a single reconciliation pass. Marked <c>internal</c> so the
    /// Agent.Service test project can drive it without spinning up the
    /// <see cref="BackgroundService"/> host loop. Returns a
    /// <see cref="ReconciliationReport"/> the test can assert on.
    /// </summary>
    internal async Task<ReconciliationReport> ReconcileOnceAsync(
        ReconciliationOptions options,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Pull the recent mapping window. The list is small (lookback
        // minutes, not all history) so synchronous processing is fine.
        var recent = await _history.GetRecentAsync(options.LookbackMinutes, ct).ConfigureAwait(false);
        if (recent.Count == 0)
        {
            _logger.LogDebug(
                "Reconciliation scan found no mappings in the last {LookbackMinutes} minutes.",
                options.LookbackMinutes);
            return ReconciliationReport.Empty(0);
        }

        var orphans = 0;
        var probeErrors = 0;

        foreach (var mapping in recent)
        {
            ct.ThrowIfCancellationRequested();

            var result = await _probe.ProbeAsync(mapping, ct).ConfigureAwait(false);
            switch (result.Outcome)
            {
                case ReconciliationProbeOutcome.Exists:
                    continue;

                case ReconciliationProbeOutcome.Missing:
                    orphans++;
                    RecordDrift();
                    var overThreshold = CurrentDriftCount() > options.DailyAlertThreshold;
                    if (overThreshold)
                    {
                        // Daily threshold breached — escalate to Error so
                        // downstream alerting (central API, Windows Event
                        // Log) pages the operator.
                        _logger.LogError(
                            "Reconciliation ORPHAN MAPPING (daily threshold exceeded): {Message}",
                            result.Message);
                    }
                    else
                    {
                        _logger.LogWarning("Reconciliation ORPHAN MAPPING: {Message}", result.Message);
                    }
                    break;

                case ReconciliationProbeOutcome.Error:
                    probeErrors++;
                    // Probe errors are infrastructure noise — log at Warning
                    // but do NOT count them toward the daily threshold
                    // (which is meant to surface real drift, not Mikro
                    // downtime).
                    _logger.LogWarning(
                        "Reconciliation probe error for tenantId={TenantId}, externalId={ExternalId}: {Message}",
                        mapping.TenantId, mapping.ExternalId, result.Message);
                    break;
            }
        }

        var thresholdExceeded = CurrentDriftCount() > options.DailyAlertThreshold;
        if (orphans > 0 || probeErrors > 0)
        {
            _logger.LogInformation(
                "Reconciliation scan complete: scanned={Scanned}, orphans={Orphans}, probeErrors={ProbeErrors}, thresholdExceeded={ThresholdExceeded}.",
                recent.Count, orphans, probeErrors, thresholdExceeded);
        }

        return new ReconciliationReport(recent.Count, orphans, probeErrors, thresholdExceeded);
    }

    /// <summary>
    /// Add a drift event to the rolling timeline. Pruning happens lazily on
    /// every <see cref="CurrentDriftCount"/> call so the queue cannot grow
    /// unbounded even if the worker runs for days without a quiet moment.
    /// </summary>
    private void RecordDrift()
    {
        var now = DateTime.UtcNow;
        _driftEvents.Enqueue(now);
        // Eagerly prune anything older than the daily window — keeps the
        // queue bounded even between scans.
        while (_driftEvents.TryPeek(out var oldest) && now - oldest >= DailyWindow)
        {
            _driftEvents.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Count the drift events still inside the rolling 24-hour window. The
    /// call walks the queue once and discards expired entries as it goes —
    /// the queue is bounded by the number of events in the last 24 hours so
    /// the linear scan is acceptable.
    /// </summary>
    private int CurrentDriftCount()
    {
        var now = DateTime.UtcNow;
        var keep = new List<DateTime>(_driftEvents.Count);
        while (_driftEvents.TryDequeue(out var stamp))
        {
            if (now - stamp < DailyWindow)
            {
                keep.Add(stamp);
            }
        }
        foreach (var stamp in keep)
        {
            _driftEvents.Enqueue(stamp);
        }
        return keep.Count;
    }
}
