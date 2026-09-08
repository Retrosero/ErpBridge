using System;

namespace ErpBridge.Core.Domain;

/// <summary>
/// Direction of a single trigger-driven change in a tracked Mikro table.
/// The numeric values are stable because they are persisted in the central
/// API's <c>change_sets.direction</c> column and in audit-log records.
/// </summary>
public enum TriggerChangeType
{
    /// <summary>The source table received an <c>INSERT</c> (Mikro side: <c>Islem = 2</c>).</summary>
    New = 0,

    /// <summary>An existing row in the source table was updated (Mikro side: <c>Islem = 1</c>).</summary>
    Changed = 1,

    /// <summary>The row was hard-deleted from the source table (Mikro side: <c>Islem = 0</c>).</summary>
    Deleted = 2,
}

/// <summary>
/// One trigger-driven change in a tracked Mikro table. The record is the
/// application-level projection of the shadow-table row: the stable
/// identifier of the affected row (<see cref="RecordKey"/>), the resolved
/// business-facing value (<see cref="KeyValue"/>) and a payload snapshot
/// (<see cref="PayloadJson"/>) that the central API persists verbatim.
///
/// <para>
/// This model is intentionally separate from the wire-format
/// <see cref="ErpBridge.Shared.SyncChangeSet"/> bundle. The wire bundle
/// groups the per-row records by direction and table; this record is the
/// per-row payload that the agent pushes through the new
/// <see cref="ErpBridge.Erp.Mikro.Trigger.ITriggerChangeSetReader"/>.
/// </para>
///
/// <para>
/// <b>KeyValue semantics (Faz 15.4):</b>
/// <list type="bullet">
///   <item><see cref="ErpBridge.Shared.RowKeyKind.Int"/> (V15): the int
///         primary key is returned as a base-10 string, e.g. <c>"42"</c>.</item>
///   <item><see cref="ErpBridge.Shared.RowKeyKind.Guid"/> (V16): the Guid
///         primary key is returned in the canonical <c>D</c> format, e.g.
///         <c>"6f9619ff-8b86-d011-b42d-00c04fc964ff"</c>.</item>
///   <item>Custom key fields (e.g. <c>*_uid</c> on V16 or
///         <c>*_RECid_RECno</c> on V15): the value is resolved through a
///         side-table lookup; <c>null</c> when the lookup misses.</item>
/// </list>
/// </para>
/// </summary>
public sealed class TriggerChangeSet
{
    /// <summary>
    /// Locally-unique identifier of this change record. The agent mints a
    /// fresh value per read so retry-safe idempotency can match on
    /// <see cref="IdempotencyKey"/> independently of the
    /// <see cref="TriggerRECno"/> cursor.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Mikro SQL table name (e.g. <c>STOKLAR</c>, <c>CARI_HESAPLAR</c>).</summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Source-table primary key as observed in the shadow row. For the
    /// legacy <c>_ERPB_SENKRONIZASYON</c> shadow this is the int
    /// <c>KayitRECno</c>; for the new <c>_ERPB_SYNC</c> shadow it is the
    /// string from the <c>KayitKey</c> column.
    /// </summary>
    public string RecordKey { get; set; } = string.Empty;

    /// <summary>
    /// Resolved stable identifier used by the central API to upsert the row
    /// into its downstream consumers. <c>null</c> when the resolver could
    /// not determine a value (lookup miss, missing key column, etc.).
    /// </summary>
    public string? KeyValue { get; set; }

    /// <summary>Direction of the change (insert / update / delete).</summary>
    public TriggerChangeType ChangeType { get; set; }

    /// <summary>UTC timestamp of the shadow-row insert.</summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// Monotonic cursor for ordering and replay safety. For the legacy
    /// shadow table this is the int <c>TriggerRECno</c>; for the new
    /// shadow table it carries the same value.
    /// </summary>
    public int TriggerRECno { get; set; }

    /// <summary>
    /// Composite idempotency key, conventionally
    /// <c>{TableName}:{TriggerRECno}:{ChangeType}</c>. The central API
    /// uses this column to deduplicate retry-safe pushes.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// JSON snapshot of the source row at the moment of the change.
    /// <c>"{}"</c> for delete events where no payload is available.
    /// </summary>
    public string PayloadJson { get; set; } = "{}";
}
