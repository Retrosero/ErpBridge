namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// One mobile-visible record, in its current state.
///
/// <para>This is a merge-by-key state table, not an event log: there is exactly
/// one row per <c>(TenantId, Entity, RecordKey)</c> and a change rewrites it in
/// place, moving <see cref="UpdatedSeq"/> forward. A device pages the table in
/// <see cref="UpdatedSeq"/> order, so the same query serves a fresh install
/// (cursor 0) and a routine delta (cursor N) — there is no separate baseline
/// path and therefore no seam between the two where a row can be missed.</para>
///
/// <para>A row updated while a device is still paging simply moves ahead of
/// that device's cursor and is read again. The sequence never moves backwards,
/// so a row can never be skipped.</para>
/// </summary>
public sealed class MobileRecord
{
    /// <summary>Owning tenant. Part of the primary key.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Tenant navigation.</summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Bootstrap section name — <c>stocks</c>, <c>customers</c>, <c>prices</c>…
    /// These are the canonical entity names across the whole pipeline; the
    /// snapshot sections, <see cref="Snapshots.SnapshotDeleteApplier"/> and the
    /// mobile pull endpoint all speak them.
    /// </summary>
    public string Entity { get; set; } = string.Empty;

    /// <summary>
    /// Business key within the entity, composite parts joined with <c>|</c>.
    /// Produced by <see cref="Sync.MobileRecordKey"/> so every writer agrees.
    /// </summary>
    public string RecordKey { get; set; } = string.Empty;

    /// <summary>
    /// Stock card this record belongs to, when it belongs to one. Set on the
    /// stock row itself as well as on its barcodes, prices, inventory and sales
    /// conditions, so deleting a stock card is one indexed update over the whole
    /// family rather than a scan per section.
    /// </summary>
    public string? StockKey { get; set; }

    /// <summary>
    /// Customer this record belongs to — the customer row itself, its addresses,
    /// contacts and sales conditions. Ledger movements deliberately carry neither
    /// key: deleting a stock card or a customer in Mikro does not delete their
    /// movements, and the app should keep showing the documents the ERP still has.
    /// </summary>
    public string? CustomerKey { get; set; }

    /// <summary>The mobile-shaped row. Null once the record is a tombstone.</summary>
    public string? PayloadJson { get; set; }

    /// <summary>
    /// SHA-256 of <see cref="PayloadJson"/>. A re-upload whose hash is unchanged
    /// must not move <see cref="UpdatedSeq"/>, otherwise every periodic sync
    /// would push the entire catalogue to every device again.
    /// </summary>
    public string? PayloadSha256 { get; set; }

    /// <summary>
    /// Per-tenant monotonic cursor position. Allocated from
    /// <see cref="TenantSyncCounter"/> inside the writing transaction so that
    /// allocation order matches commit order.
    /// </summary>
    public long UpdatedSeq { get; set; }

    /// <summary>True once the ERP no longer has the row. The device deletes it and moves on.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>When the row last changed. Drives tombstone retention.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Identifies the full upload that last touched this row. A full (non-incremental)
    /// upload stamps every row it carries, then tombstones the rows in the sections it
    /// covered that still hold an older stamp — the ERP no longer has them.
    /// </summary>
    public Guid? LastSeenRunId { get; set; }
}
