using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Trigger;

/// <summary>
/// One chunk of a change-tracking read. The shape is shared by all three
/// direction readers so the central API can store them in the same
/// <c>change_sets.payload_json</c> column without per-direction schema.
/// </summary>
/// <param name="Table">The Mikro table the chunk belongs to.</param>
/// <param name="Fields">Column names in the same order as the dictionary values in <paramref name="Rows"/>.</param>
/// <param name="Rows">One entry per row; each entry is a column→value map.</param>
/// <param name="HighestTriggerRecNo">The highest <c>TriggerRECno</c> observed in this chunk. The agent uses this to advance its watermark.</param>
/// <param name="MoreAvailable">True when the chunk returned at least one page and the caller should ask for the next page.</param>
public sealed record TriggerChunk(
    TrackedTableSchema Table,
    IReadOnlyList<string> Fields,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows,
    int HighestTriggerRecNo,
    bool MoreAvailable);

/// <summary>
/// Reads the three change-tracking directions (new / changed / deleted) for a
/// single tracked table. The interface is split per direction because the
/// caller usually wants to schedule them in parallel. All directions share
/// the monotonic <c>TriggerRECno</c> cursor; <c>Islem</c> distinguishes insert,
/// update and delete events.
/// </summary>
public interface IChangeSetReader
{
    /// <summary>
    /// Read the next page of *new* rows for <paramref name="schema"/>. The
    /// implementation issues
    /// <c>SELECT TOP(@packetSize) &lt;fields&gt; FROM &lt;table&gt; JOIN _ERPB_SENKRONIZASYON ... Islem = 2</c>
    /// — the trigger-backed "new rows" query. <paramref name="fields"/> is a
    /// subset of <see cref="TrackedTableSchema.Fields"/>; the implementation
    /// rejects any field outside the whitelist.
    /// </summary>
    Task<TriggerChunk> ReadNewAsync(
        TrackedTableSchema schema,
        int lastRecNo,
        int packetSize,
        IReadOnlyList<string> fields,
        CancellationToken ct = default);

    /// <summary>
    /// Read the next page of *changed* rows for <paramref name="schema"/>:
    /// rows whose <c>_ERPB_SENKRONIZASYON</c> shadow entry with
    /// <c>Islem = 1</c> (UPDATE) has a <c>TriggerRECno &gt; @lastTrigger</c>.
    /// </summary>
    Task<TriggerChunk> ReadChangedAsync(
        TrackedTableSchema schema,
        int lastTriggerRecNo,
        int packetSize,
        IReadOnlyList<string> fields,
        CancellationToken ct = default);

    /// <summary>
    /// Read the next page of *deleted* row references for
    /// <paramref name="schema"/>: shadow entries with
    /// <c>Islem = 0</c> (DELETE) and <c>TriggerRECno &gt; @lastTrigger</c>.
    /// The implementation returns the primary-key + TriggerRECno pair
    /// (no other column is meaningful for a hard-deleted row).
    /// </summary>
    Task<TriggerChunk> ReadDeletedAsync(
        TrackedTableSchema schema,
        int lastTriggerRecNo,
        int packetSize,
        CancellationToken ct = default);
}
