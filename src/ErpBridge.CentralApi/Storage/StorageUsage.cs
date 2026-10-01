using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// A company's storage figures for the usage endpoints (GOAL_DEPOLAMA_R2 S8): the counter row (what the quota checks)
/// and the ledger grouped by area and status (what the counter is recounted from).
/// </summary>
public sealed record StorageUsageSnapshot(TenantStorage? Counter, StorageAreaUsageDto[] Areas, long TrashedBytes, int TrashedCount)
{
    public long UsedBytes => Counter?.UsedBytes ?? 0;

    public static async Task<StorageUsageSnapshot> ReadAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        var counter = await db.TenantStorage.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        var groups = await db.StoredFiles.AsNoTracking()
            .Where(f => f.TenantId == tenantId && (f.Status == StoredFileStatuses.Active || f.Status == StoredFileStatuses.Trashed))
            .GroupBy(f => new { f.Area, f.Status })
            .Select(g => new { g.Key.Area, g.Key.Status, Bytes = g.Sum(f => f.SizeBytes), Count = g.Count() })
            .ToListAsync(ct);
        var active = groups.Where(g => g.Status == StoredFileStatuses.Active).ToList();
        var areas = StorageAreas.All.Select(area => new StorageAreaUsageDto
        {
            Area = area,
            UsedBytes = active.Where(g => g.Area == area).Sum(g => g.Bytes),
            FileCount = active.Where(g => g.Area == area).Sum(g => g.Count),
        }).ToArray();
        var trashed = groups.Where(g => g.Status == StoredFileStatuses.Trashed).ToList();
        return new StorageUsageSnapshot(counter, areas, trashed.Sum(g => g.Bytes), trashed.Sum(g => g.Count));
    }
}
