using System.Collections.Generic;
using System.Linq;

namespace ErpBridge.Shared;

/// <summary>
/// Per-table descriptor carried by the agent when it pushes a
/// <see cref="SyncChangeSet"/> to the central API. The agent's metadata
/// travels with the payload so the central API can re-emit the same shape
/// to its downstream consumers without re-querying the source ERP.
///
/// <para>
/// <b>Faz 20 — ERP-neutral.</b> The old <c>TabloID</c> int (a Mikro
/// <c>TrackedTableCatalog</c> id) is gone. Tables are identified by
/// <see cref="TableKey"/>, a stable string that every adapter mints from
/// its own catalog (Mikro: the physical table name; Logo: the
/// firm/period-independent logical name). <see cref="TableName"/> is the
/// physical object the change was captured from.
/// </para>
/// </summary>
public sealed record SyncTableDescriptor(
    string TableKey,
    string TableName,
    string KeyField,
    IReadOnlyList<string> Fields,
    bool RequiresSoftDeleteFilter);

/// <summary>
/// One upserted row: the source row's stable identifier as a string
/// (<see cref="RecordKey"/> — a base-10 int RECno or a canonical GUID) plus
/// the whitelisted column→value map. Carrying the key explicitly means the
/// consumer never has to guess which column is the primary key, and works
/// for GUID-keyed tables that have no <c>*_RECno</c> column at all.
/// </summary>
public sealed record SyncUpsertRow(
    string RecordKey,
    IReadOnlyDictionary<string, object?> Columns);

/// <summary>
/// One chunk of <i>new</i> rows. The column order inside each row matches
/// <see cref="SyncTableDescriptor.Fields"/>. <see cref="HighestSequence"/>
/// is the largest change-log sequence the agent observed in this chunk;
/// the next page is requested from it.
/// </summary>
public sealed record SyncNewChunk(
    SyncTableDescriptor Table,
    IReadOnlyList<SyncUpsertRow> Rows,
    long HighestSequence,
    bool MoreAvailable);

/// <summary>
/// One chunk of <i>changed</i> rows (source side: UPDATE, or an INSERT the
/// shadow log cannot tell apart from an edit). The consumer upserts.
/// <see cref="HighestSequence"/> is monotonic across the chunk.
/// </summary>
public sealed record SyncChangedChunk(
    SyncTableDescriptor Table,
    IReadOnlyList<SyncUpsertRow> Rows,
    long HighestSequence,
    bool MoreAvailable);

/// <summary>
/// One deleted-row reference. <see cref="RecordKey"/> is the source row's
/// stable identifier as a string — an int RECno rendered in base-10 for
/// int-keyed tables, or the canonical <c>D</c>-format GUID for
/// GUID-keyed tables (Mikro V16, Logo has none). <see cref="Sequence"/> is
/// the change-log sequence of the delete event.
/// </summary>
public sealed record SyncDeletedRow(string RecordKey, long Sequence);

/// <summary>
/// One chunk of <i>deleted</i> row references (source side: DELETE). Carries
/// only the key + sequence per row, never the table's full schema.
/// </summary>
public sealed record SyncDeletedChunk(
    SyncTableDescriptor Table,
    IReadOnlyList<SyncDeletedRow> Rows,
    long HighestSequence,
    bool MoreAvailable);

/// <summary>
/// Per-table snapshot of the three change-tracking directions. The central
/// API persists the chunk lists verbatim in <c>change_sets.payload_json</c>;
/// each downstream consumer pulls the direction it needs.
/// </summary>
public sealed record SyncTableChangeSet(
    SyncTableDescriptor Table,
    SyncNewChunk? New,
    SyncChangedChunk? Changed,
    SyncDeletedChunk? Deleted,
    long PreviousSequence,
    long NewSequence);

/// <summary>
/// One agent → central API push. The bundle is the smallest unit of work
/// the central API accepts: one tenant, one ERP, one source database, one
/// or more tracked tables. The agent builds one per cycle and re-uses the
/// same payload across retries.
/// </summary>
public sealed record SyncChangeSet(
    string TenantId,
    string ErpType,
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
