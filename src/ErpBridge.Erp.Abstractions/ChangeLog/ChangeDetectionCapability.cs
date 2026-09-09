namespace ErpBridge.Erp.Abstractions.ChangeLog;

/// <summary>
/// How an ERP adapter is able to tell the agent *what changed* since the last
/// sync. The agent picks its sync strategy from this value — it must never
/// assume a mechanism (Mikro's shadow tables, Logo's CAPIBLOCK_MODIFIEDDATE,
/// Paraşüt's webhooks) is available on every back-end.
/// </summary>
public enum ChangeDetectionCapability
{
    /// <summary>
    /// The adapter can only re-read the whole reference-data snapshot. The
    /// central API is responsible for diffing. Deletes are detected only by
    /// absence from a later snapshot.
    /// </summary>
    FullSnapshotOnly = 0,

    /// <summary>
    /// The adapter can return rows modified after a UTC instant, using a
    /// per-row modification timestamp (Mikro <c>*_lastup_date</c>, Logo
    /// <c>CAPIBLOCK_MODIFIEDDATE</c>, Paraşüt <c>updated_at</c>).
    /// <para>
    /// <b>Deletes are NOT observable on this path</b> — a timestamp filter can
    /// never see a row that no longer exists. A consumer relying on this
    /// capability must run a periodic key-list reconciliation to produce
    /// tombstones.
    /// </para>
    /// </summary>
    TimestampDelta = 1,

    /// <summary>
    /// The adapter exposes a real change log with INSERT / UPDATE / DELETE
    /// events and a monotonic cursor (Mikro's <c>_ERPB_SYNC</c> +
    /// <c>_ERPB_SYNC_DEL</c> shadow tables, SQL Server Change Tracking, or a
    /// vendor webhook feed). This is the preferred mode when available.
    /// </summary>
    ShadowTableChangeLog = 2,
}
