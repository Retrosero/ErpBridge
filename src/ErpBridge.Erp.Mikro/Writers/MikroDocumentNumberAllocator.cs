using System.Data;
using Dapper;
using ErpBridge.Erp.Sql;
using Microsoft.Data.SqlClient;

namespace ErpBridge.Erp.Mikro.Writers;

/// <summary>
/// Allocates the next <c>evrakno_sıra</c> for a document series, inside the
/// caller's transaction.
///
/// <para>
/// <b>Why this exists.</b> Mikro enforces a unique index over
/// <c>(tip, cins, evrakno_seri, evrakno_sıra, satır_no)</c>. Until now the
/// writers used whatever number the payload carried, so two devices that both
/// picked "next number" offline produced a hard unique-violation on the second
/// write — a failed job rather than a document. The reference application's own
/// guidance is that series/number generation must be central and idempotent.
/// </para>
///
/// <para>
/// <b>Concurrency.</b> The <c>MAX</c> probe takes <c>UPDLOCK, HOLDLOCK</c> so
/// concurrent allocations against the same series serialise on the index range
/// instead of racing. The lock is released when the caller's transaction ends,
/// which is also when the row that consumes the number is committed — so the
/// number can never be handed out twice.
/// </para>
///
/// <para>
/// <b>Idempotency is upstream.</b> This allocator runs only after the writer's
/// <c>externalId</c> mapping lookup has missed, so a replayed job returns the
/// original document instead of burning a fresh number.
/// </para>
/// </summary>
public sealed class MikroDocumentNumberAllocator
{
    /// <summary>Sentinel meaning "allocate one for me". Payloads that carry a real number keep it.</summary>
    public const int AutoAllocate = 0;

    /// <summary>
    /// Return <paramref name="requested"/> when the caller supplied a real
    /// number, otherwise allocate the next free one for the series.
    /// </summary>
    /// <param name="conn">Open connection.</param>
    /// <param name="tx">The writer's transaction — the lock must live as long as it does.</param>
    /// <param name="table">Document table, e.g. <c>SIPARISLER</c>.</param>
    /// <param name="prefix">Column prefix for that table, e.g. <c>sip</c>.</param>
    /// <param name="series">Evrak serisi.</param>
    /// <param name="requested">Caller-supplied number, or <see cref="AutoAllocate"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task<int> ResolveAsync(
        SqlConnection conn,
        IDbTransaction tx,
        string table,
        string prefix,
        string series,
        int requested,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(conn);
        ArgumentNullException.ThrowIfNull(tx);

        if (requested > AutoAllocate)
        {
            // The caller pinned a number — respect it. Mikro's unique index is
            // still the backstop if it collides.
            return requested;
        }

        var t = SqlIdentifier.Validate(table);
        var p = SqlIdentifier.Validate(prefix);

        // UPDLOCK+HOLDLOCK holds a range lock for the caller's transaction, so a
        // second allocator on the same series waits rather than reading the same
        // MAX and duplicating the number.
        var sql = $@"
SELECT ISNULL(MAX([{p}_evrakno_sira]), 0) + 1
FROM [{t}] WITH (UPDLOCK, HOLDLOCK)
WHERE [{p}_evrakno_seri] = @Series;";

        return await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            new { Series = series },
            transaction: tx,
            cancellationToken: ct)).ConfigureAwait(false);
    }
}
