namespace ErpBridge.Erp.Abstractions.Reconciliation;

/// <summary>
/// Outcome of a single reconciliation probe. The reconciliation worker uses
/// this to distinguish a real drift signal (mapping without a ERP record, or
/// in the future a ERP record without a mapping) from transient
/// infrastructure issues (ERP down, network glitch).
/// </summary>
public enum ReconciliationProbeOutcome
{
    /// <summary>
    /// The ERP record still exists. The mapping is consistent with the ERP
    /// state — no action needed.
    /// </summary>
    Exists,

    /// <summary>
    /// The ERP record is gone (orphan mapping). This is the drift signal the
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
/// The ERP-side identity of one written document, as recorded in the agent's
/// mapping store.
///
/// <para>
/// The probe takes this rather than a mapping row because two
/// <c>MappingRecord</c> shapes exist (one in Core for persistence, one in
/// abstractions for the adapter contract) and a reconciliation probe needs
/// neither in full — only enough to find the row again and to name it in a
/// diagnostic.
/// </para>
/// </summary>
/// <param name="TenantId">Owning tenant, for the alarm message.</param>
/// <param name="DocumentType">Document kind, for the alarm message.</param>
/// <param name="ExternalId">Idempotency key, for the alarm message.</param>
/// <param name="DatabaseName">ERP database the document was written to.</param>
/// <param name="Recno">Int identity (V15-style ERPs); null when the ERP keys by Guid.</param>
/// <param name="Guid">Guid identity (V16-style ERPs); null when the ERP keys by int.</param>
public sealed record ErpDocumentRef(
    string TenantId,
    string DocumentType,
    string ExternalId,
    string DatabaseName,
    int? Recno,
    Guid? Guid)
{
    /// <summary>True when neither identity is present — nothing to probe for.</summary>
    public bool HasNoIdentity => !Recno.HasValue && !Guid.HasValue;
}

/// <summary>
/// Probe that asks the live ERP whether a document the agent previously wrote
/// still exists. Implementations MUST be safe to call from the
/// <c>CrossDbReconciliationWorker</c> loop — transient failures should return
/// <see cref="ReconciliationProbeResult.Error"/> rather than throw, so one bad
/// mapping cannot poison the whole scan.
///
/// <para>
/// The contract lives in the abstractions package so each adapter can ship its
/// own implementation; the worker stays vendor-neutral.
/// </para>
/// </summary>
public interface IErpReconciliationProbe
{
    /// <summary>
    /// Check whether the document identified by <paramref name="document"/> still
    /// exists. Returns <see cref="ReconciliationProbeOutcome.Exists"/> when the
    /// record is found, <see cref="ReconciliationProbeOutcome.Missing"/> when the
    /// row is gone, and <see cref="ReconciliationProbeOutcome.Error"/> when the
    /// probe could not run.
    /// </summary>
    Task<ReconciliationProbeResult> ProbeAsync(ErpDocumentRef document, CancellationToken ct);
}
