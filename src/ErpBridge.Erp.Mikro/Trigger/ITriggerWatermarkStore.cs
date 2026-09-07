using ErpBridge.Core.Stores;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Trigger;

/// <summary>
/// Per-table resume cursor for the trigger-based change-tracking path. The
/// implementation reuses the existing per-tenant <c>checkpoints</c> SQLite
/// table (no new column or migration needed): each row's
/// <c>sync_scope = "trigger:&lt;TABLO_ADI&gt;"</c> and <c>last_token</c> holds the
/// highest <c>TriggerRECno</c> the agent has already pushed to the central
/// API. The on-disk format is intentionally identical to the watermark path
/// so the WPF UI's "Senkronizasyonu sıfırla" button can clear both modes
/// with a single <c>DELETE</c>.
/// </summary>
/// <remarks>
/// The store is a thin wrapper around <see cref="ICheckpointStore"/> — it
/// is not a parallel persistence layer. Splitting the interface out keeps
/// the call sites in the trigger code readable
/// (<c>await store.GetLastTriggerAsync(schema, ct)</c> vs the raw
/// <c>LoadAsync(tenant, scope)</c>) and gives a single seam to mock in unit
/// tests without touching the SQLite store.
/// </remarks>
public interface ITriggerWatermarkStore
{
    /// <summary>
    /// Read the last <c>TriggerRECno</c> already pushed for
    /// <paramref name="schema"/>. Returns <c>0</c> when the agent has never
    /// pushed this table — that case is the "first-time sync" path and
    /// produces a full snapshot.
    /// </summary>
    Task<int> GetLastTriggerAsync(string tenantId, TrackedTableSchema schema, CancellationToken ct = default);

    /// <summary>
    /// Persist the highest <c>TriggerRECno</c> the agent successfully pushed
    /// for <paramref name="schema"/>. The value is monotonically non-decreasing
    /// — a smaller value is silently ignored to keep a buggy retry from
    /// rewinding the cursor.
    /// </summary>
    Task SetLastTriggerAsync(string tenantId, TrackedTableSchema schema, int lastTriggerRecNo, CancellationToken ct = default);

    /// <summary>
    /// Reset the watermark for every tracked table. Used by the WPF's
    /// "Senkronizasyonu sıfırla" button. Returns the number of rows deleted.
    /// </summary>
    Task<int> ResetAllAsync(string tenantId, CancellationToken ct = default);
}
