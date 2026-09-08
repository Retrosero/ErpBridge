using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErpBridge.Core.Domain;

namespace ErpBridge.Erp.Mikro.Trigger;

/// <summary>
/// High-level, three-direction trigger reader used by the agent when it
/// needs one <see cref="TriggerChangeSet"/> per affected row (Faz 15.3 / 15.4).
/// The interface is deliberately narrower than <see cref="IChangeSetReader"/>:
/// callers ask for everything that changed since a UTC timestamp and receive
/// a flat list of <see cref="TriggerChangeSet"/> records, with the
/// <see cref="TriggerChangeSet.KeyValue"/> already resolved.
///
/// <para>
/// <b>Why two interfaces?</b> The lower-level
/// <see cref="IChangeSetReader.ReadNewAsync"/> / <c>ReadChangedAsync</c> /
/// <c>ReadDeletedAsync</c> deal in raw <see cref="TriggerChunk"/>s and the
/// legacy int <c>KayitRECno</c> cursor; the legacy consumer in
/// <c>MikroAdapter.ReadChangeSetAsync</c> still needs that shape. The new
/// <see cref="ITriggerChangeSetReader"/> is the successor API the agent
/// pushes to the central API with a per-row key, hiding the
/// <c>KayitRECno</c>/<c>KayitKey</c> split between the two shadow-table
/// families (<c>_ERPB_SENKRONIZASYON</c> for legacy, <c>_ERPB_SYNC</c> +
/// <c>_ERPB_SYNC_DEL</c> for FORA-compatible).
/// </para>
///
/// <para>
/// <b>V15 / V16 difference:</b> the implementation resolves
/// <see cref="TriggerChangeSet.KeyValue"/> using
/// <see cref="Shared.TrackedTableSchema.EffectiveKeyField"/> and
/// <see cref="Shared.TrackedTableSchema.EffectiveKeyKind"/>. V15 tables
/// report <see cref="Shared.RowKeyKind.Int"/> and the resolver returns the
/// primary-key int as a string; V16 tables report
/// <see cref="Shared.RowKeyKind.Guid"/> and the resolver returns the Guid
/// in the canonical <c>D</c> format. Custom key fields (e.g. <c>*_uid</c>
/// on V16 or <c>*_RECid_RECno</c> on V15) are resolved through a
/// side-table lookup; a lookup miss is propagated as <c>null</c>, not as
/// an exception.
/// </para>
/// </summary>
public interface ITriggerChangeSetReader
{
    /// <summary>
    /// Read every <see cref="TriggerChangeType.New"/> change in
    /// <paramref name="databaseName"/> that occurred after
    /// <paramref name="sinceUtc"/>. The implementation is responsible for
    /// translating the UTC timestamp into the corresponding shadow-table
    /// cursor.
    /// </summary>
    /// <param name="databaseName">Mikro database (e.g. <c>MikroDB_V15_02</c>).</param>
    /// <param name="sinceUtc">Lower bound, inclusive. Rows older than this value are skipped.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of <see cref="TriggerChangeSet"/> records, one per affected row. Empty when nothing changed.</returns>
    Task<IReadOnlyList<TriggerChangeSet>> ReadNewAsync(
        string databaseName,
        DateTime sinceUtc,
        CancellationToken ct = default);

    /// <summary>
    /// Read every <see cref="TriggerChangeType.Changed"/> change in
    /// <paramref name="databaseName"/> that occurred after
    /// <paramref name="sinceUtc"/>.
    /// </summary>
    Task<IReadOnlyList<TriggerChangeSet>> ReadChangedAsync(
        string databaseName,
        DateTime sinceUtc,
        CancellationToken ct = default);

    /// <summary>
    /// Read every <see cref="TriggerChangeType.Deleted"/> change in
    /// <paramref name="databaseName"/> that occurred after
    /// <paramref name="sinceUtc"/>.
    /// </summary>
    Task<IReadOnlyList<TriggerChangeSet>> ReadDeletedAsync(
        string databaseName,
        DateTime sinceUtc,
        CancellationToken ct = default);

    /// <summary>
    /// Convenience helper — merge the result of
    /// <see cref="ReadNewAsync"/>, <see cref="ReadChangedAsync"/> and
    /// <see cref="ReadDeletedAsync"/> into a single list. The three
    /// directions are read sequentially on a single channel so a slow
    /// read on one direction does not starve the others; callers that
    /// need parallelism should invoke the per-direction methods directly.
    /// </summary>
    Task<IReadOnlyList<TriggerChangeSet>> ReadAllAsync(
        string databaseName,
        DateTime sinceUtc,
        CancellationToken ct = default);
}
