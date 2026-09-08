using ErpBridge.Core.Domain;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Outcome of a single reconciliation probe. The reconciliation worker uses
/// this to distinguish a real drift signal (mapping without a Mikro record, or
/// in the future a Mikro record without a mapping) from transient
/// infrastructure issues (Mikro down, network glitch).
/// </summary>
public enum ReconciliationProbeOutcome
{
    /// <summary>
    /// The Mikro record still exists. The mapping is consistent with the ERP
    /// state — no action needed.
    /// </summary>
    Exists,

    /// <summary>
    /// The Mikro record is gone (orphan mapping). This is the drift signal the
    /// worker logs at <c>Warning</c> level.
    /// </summary>
    Missing,

    /// <summary>
    /// The probe could not be completed (Mikro unreachable, SQL error, version
    /// detection failure, etc.). The worker logs at <c>Warning</c> level and
    /// counts the event toward the daily threshold so a flaky Mikro does not
    /// stay silent.
    /// </summary>
    Error,
}

/// <summary>
/// Result of a single probe — pairs the outcome with a human-readable message
/// suitable for log enrichment. Keeping the message alongside the outcome lets
/// the worker log a single structured line per mapping without having to
/// inspect the probe's internal state.
/// </summary>
public readonly record struct ReconciliationProbeResult(
    ReconciliationProbeOutcome Outcome,
    string? Message)
{
    /// <summary>Shorthand for the "mapping is consistent" case.</summary>
    public static ReconciliationProbeResult Exists() => new(ReconciliationProbeOutcome.Exists, null);

    /// <summary>Shorthand for the "orphan mapping" case.</summary>
    public static ReconciliationProbeResult Missing(string message) =>
        new(ReconciliationProbeOutcome.Missing, message);

    /// <summary>Shorthand for the "probe failed" case.</summary>
    public static ReconciliationProbeResult Error(string message) =>
        new(ReconciliationProbeOutcome.Error, message);
}

/// <summary>
/// Probe that asks the live ERP (today: only Mikro V15/V16) whether the
/// document referenced by a mapping row still exists. Implementations MUST be
/// safe to call from the <see cref="CrossDbReconciliationWorker"/> loop —
/// transient failures should return <see cref="ReconciliationProbeResult.Error"/>
/// rather than throw, so one bad mapping cannot poison the whole scan.
///
/// The interface lives in the Agent.Service namespace because the reconciliation
/// worker is the only caller — the abstractions project has no business with a
/// reconciliation-shaped contract that depends on <see cref="MappingRecord"/>.
/// </summary>
public interface IReconciliationProbe
{
    /// <summary>
    /// Check whether the Mikro document identified by <paramref name="mapping"/>
    /// still exists. Returns <see cref="ReconciliationProbeOutcome.Exists"/> when
    /// the record is found, <see cref="ReconciliationProbeOutcome.Missing"/>
    /// when the row is gone, and <see cref="ReconciliationProbeOutcome.Error"/>
    /// when the probe could not run.
    /// </summary>
    Task<ReconciliationProbeResult> ProbeAsync(MappingRecord mapping, CancellationToken ct);
}
