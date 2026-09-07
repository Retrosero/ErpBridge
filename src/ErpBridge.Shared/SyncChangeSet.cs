using System.Collections.Generic;
using System.Linq;

namespace ErpBridge.Shared;

/// <summary>
/// Per-table description used by the agent when pushing a <see cref="SyncChangeSet"/>
/// to the central API. The agent's metadata travels with the payload so the
/// central API can re-emit the same shape to the Android client without
/// re-querying Mikro for column lists.
/// </summary>
public sealed record SyncTableDescriptor(
    int TabloID,
    string TabloAdi,
    string RecnoField,
    IReadOnlyList<string> Fields,
    bool RequiresSoftDeleteFilter);

/// <summary>
/// One chunk of *new* rows. Each row is a column→value map; the column
/// order matches <see cref="SyncTableDescriptor.Fields"/>. The
/// <see cref="HighestRecNo"/> is the largest source-table RECno the agent
/// observed; the next page should be requested with
/// <c>lastRecNo = HighestRecNo</c>.
/// </summary>
public sealed record SyncNewChunk(
    SyncTableDescriptor Table,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows,
    int HighestRecNo,
    bool MoreAvailable);

/// <summary>
/// One chunk of *changed* rows (Mikro side: UPDATE). Each row carries a
/// <c>TriggerRECno</c> that is monotonic across the chunk; the agent uses
/// the largest one to advance the watermark.
/// </summary>
public sealed record SyncChangedChunk(
    SyncTableDescriptor Table,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows,
    int HighestTriggerRecNo,
    bool MoreAvailable);

/// <summary>
/// One chunk of *deleted* row references (Mikro side: DELETE). The chunk
/// only carries the primary-key RECno + the <c>TriggerRECno</c>; the column
/// list is fixed and never the table's full schema.
/// </summary>
public sealed record SyncDeletedChunk(
    SyncTableDescriptor Table,
    IReadOnlyList<(int KayitRecNo, int TriggerRecNo)> Rows,
    int HighestTriggerRecNo,
    bool MoreAvailable);

/// <summary>
/// Per-table snapshot of the three change-tracking directions. The central
/// API persists the chunk lists verbatim in <c>change_sets.payload_json</c>;
/// the Android client pulls each direction through its own endpoint.
/// </summary>
public sealed record SyncTableChangeSet(
    SyncTableDescriptor Table,
    SyncNewChunk? New,
    SyncChangedChunk? Changed,
    SyncDeletedChunk? Deleted,
    int PreviousLastTriggerRecNo,
    int NewLastTriggerRecNo);

/// <summary>
/// One agent → central API push. The bundle is the smallest unit of work
/// the central API accepts: one tenant, one Mikro database, one or more
/// tracked tables. The agent builds one of these every 60 s and re-uses
/// the same payload across retries.
/// </summary>
public sealed record SyncChangeSet(
    string TenantId,
    string SourceDatabase,
    DateTimeOffset PulledAtUtc,
    IReadOnlyList<SyncTableChangeSet> Tables)
{
    /// <summary>Sum of every chunk's row count. Used for logging and metrics.</summary>
    public int TotalRowCount => Tables.Sum(t =>
        (t.New?.Rows.Count ?? 0) +
        (t.Changed?.Rows.Count ?? 0) +
        (t.Deleted?.Rows.Count ?? 0));
}
