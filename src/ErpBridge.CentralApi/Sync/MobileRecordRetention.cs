using ErpBridge.CentralApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ErpBridge.CentralApi.Sync;

/// <summary>
/// Purges tombstones the feed no longer needs, and records how far the purge has
/// reached.
///
/// <para>A tombstone exists so a device that was offline when a row was deleted
/// still learns to drop it. Once it is older than any device is expected to stay
/// away, keeping it only costs storage. But deleting it silently would be a
/// correctness bug: a device whose cursor predates the purge would never be told
/// about that deletion and would keep the row forever, with nothing to indicate
/// anything was wrong.</para>
///
/// <para>So the purge advances <see cref="Domain.TenantSyncCounter.TombstoneHorizonSeq"/>
/// to the highest position it removed. The pull endpoint compares an incoming
/// cursor against it and answers anything older with a full resync — the copy
/// cannot be repaired incrementally, only replaced.</para>
/// </summary>
public sealed class MobileRecordRetention
{
    private readonly ILogger<MobileRecordRetention> _logger;

    /// <summary>DI constructor.</summary>
    public MobileRecordRetention(ILogger<MobileRecordRetention> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>What one purge removed.</summary>
    /// <param name="Purged">Tombstone rows deleted.</param>
    /// <param name="Tenants">Tenants whose horizon moved.</param>
    public readonly record struct RetentionResult(int Purged, int Tenants);

    /// <summary>
    /// Deletes tombstones older than <paramref name="retentionDays"/> and moves
    /// each affected tenant's horizon to the highest sequence removed.
    /// </summary>
    /// <param name="db">Context to purge through.</param>
    /// <param name="retentionDays">How long a tombstone stays readable.</param>
    /// <param name="maxPerRun">Ceiling on rows removed in one pass.</param>
    /// <param name="ct">Cancellation.</param>
    public async Task<RetentionResult> RunOnceAsync(
        CentralApiDbContext db, int retentionDays, int maxPerRun, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);

        var cutoff = DateTime.UtcNow.AddDays(-Math.Max(1, retentionDays));
        var expired = await db.MobileRecords
            .Where(x => x.IsDeleted && x.UpdatedAtUtc < cutoff)
            .OrderBy(x => x.UpdatedSeq)
            .Take(Math.Max(1, maxPerRun))
            .ToListAsync(ct)
            .ConfigureAwait(false);
        if (expired.Count == 0) return default;

        var highestPerTenant = expired
            .GroupBy(x => x.TenantId)
            .ToDictionary(g => g.Key, g => g.Max(x => x.UpdatedSeq));

        db.MobileRecords.RemoveRange(expired);

        foreach (var pair in highestPerTenant)
        {
            var counter = await db.TenantSyncCounters
                .FirstOrDefaultAsync(x => x.TenantId == pair.Key, ct)
                .ConfigureAwait(false);
            if (counter is null) continue;
            // Monotonic: a shorter retention setting or an out-of-order batch
            // must never walk the horizon backwards and re-admit cursors that
            // were already told to resync.
            if (pair.Value > counter.TombstoneHorizonSeq)
                counter.TombstoneHorizonSeq = pair.Value;
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        _logger.LogInformation(
            "Purged {Count} expired mobile tombstones across {Tenants} tenants (older than {Days} days).",
            expired.Count, highestPerTenant.Count, retentionDays);

        return new RetentionResult(expired.Count, highestPerTenant.Count);
    }
}
