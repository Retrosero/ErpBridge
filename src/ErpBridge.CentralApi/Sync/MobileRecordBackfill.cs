using System.Text.Json;
using ErpBridge.CentralApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Sync;

/// <summary>
/// Seeds <see cref="Domain.MobileRecord"/> from a tenant's existing bootstrap
/// snapshot.
///
/// <para>Without this, <c>mobile_records</c> would only fill up as the ERP
/// happened to change: a tenant whose catalogue is quiet would serve a nearly
/// empty feed, and a device installing for the first time would receive almost
/// nothing until the agent was forced through a full rebuild. The active
/// snapshot already holds exactly the state the feed should start from, so the
/// backfill projects it once and the feed is complete from the first day.</para>
///
/// <para>Deliberately an upsert, never a sweep. The snapshot is a good starting
/// point but it is not newer than the change log, so reading "absent from the
/// snapshot" as "deleted" could tombstone rows a later delta had already
/// corrected. Running it twice is therefore a no-op rather than a hazard.</para>
/// </summary>
public sealed class MobileRecordBackfill
{
    private readonly MobileRecordProjector _projector;

    /// <summary>DI constructor.</summary>
    /// <param name="projector">The single writer of <c>mobile_records</c>.</param>
    public MobileRecordBackfill(MobileRecordProjector projector)
    {
        _projector = projector ?? throw new ArgumentNullException(nameof(projector));
    }

    /// <summary>What one backfill run did.</summary>
    /// <param name="SnapshotId">Snapshot it read, or null when the tenant has none.</param>
    /// <param name="Sections">Sections projected.</param>
    /// <param name="Seeded">Records written for the first time or updated.</param>
    /// <param name="AlreadyCurrent">Records the feed already held byte-identically.</param>
    public readonly record struct BackfillResult(Guid? SnapshotId, int Sections, int Seeded, int AlreadyCurrent);

    /// <summary>
    /// Projects the tenant's active snapshot into the mobile feed. Safe to run
    /// repeatedly; a second run finds every row unchanged and moves no cursor.
    /// </summary>
    /// <param name="db">Context to read and write through.</param>
    /// <param name="tenantId">Tenant to seed.</param>
    /// <param name="ct">Cancellation.</param>
    public async Task<BackfillResult> RunAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);

        var snapshot = await db.BootstrapSnapshots.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .Select(x => new { x.Id, x.SourceDatabase })
            .FirstOrDefaultAsync(ct);
        if (snapshot is null) return new BackfillResult(null, 0, 0, 0);

        var chunks = await db.BootstrapSnapshotChunks.AsNoTracking()
            .Where(x => x.SnapshotId == snapshot.Id)
            .OrderBy(x => x.Section).ThenBy(x => x.ChunkIndex)
            .Select(x => new { x.Section, x.PayloadJson })
            .ToListAsync(ct);

        var bySection = new Dictionary<string, List<JsonElement>>(StringComparer.OrdinalIgnoreCase);
        foreach (var chunk in chunks)
        {
            if (!bySection.TryGetValue(chunk.Section, out var rows))
            {
                rows = [];
                bySection[chunk.Section] = rows;
            }
            using var document = JsonDocument.Parse(chunk.PayloadJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array) continue;
            foreach (var element in document.RootElement.EnumerateArray())
                rows.Add(element.Clone());
        }

        var sections = bySection
            .Select(pair => new MobileRecordProjector.SectionRows(pair.Key, pair.Value))
            .ToList();

        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;

        var result = await _projector.ProjectAsync(db, tenantId, sections, fullUpload: false, snapshot.SourceDatabase, ct);
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);

        return new BackfillResult(snapshot.Id, sections.Count, result.Changed, result.Unchanged);
    }
}
