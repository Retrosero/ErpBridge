using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Sync;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Operator view of the mobile feed, and the one-shot seed that fills it from a
/// tenant's existing snapshot.
///
/// <para>Admin tokens carry no <c>tenant</c> claim, so the tenant comes from the
/// query string here — reading it from the token would reject every request with
/// 401. See <c>AdminBootstrapEndpoints</c> for the same pattern.</para>
/// </summary>
public static class AdminMobileRecordsEndpoints
{
    public static IEndpointRouteBuilder MapAdminMobileRecordsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/mobile-records")
            .WithTags("Admin/MobileRecords")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/summary", SummaryAsync).WithName("AdminMobileRecordsSummary");
        group.MapPost("/backfill", BackfillAsync).WithName("AdminMobileRecordsBackfill");

        return routes;
    }

    private static async Task<IResult> SummaryAsync(
        [FromQuery] Guid? tenantId,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (Missing(tenantId) is { } error) return error;

        var byEntity = await db.MobileRecords.AsNoTracking()
            .Where(x => x.TenantId == tenantId!.Value)
            .GroupBy(x => x.Entity)
            .Select(g => new
            {
                entity = g.Key,
                live = g.Count(x => !x.IsDeleted),
                tombstones = g.Count(x => x.IsDeleted),
            })
            .OrderBy(x => x.entity)
            .ToListAsync(ct);

        var counter = await db.TenantSyncCounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId!.Value, ct);

        return JsonResults.Ok(new
        {
            tenantId,
            lastSeq = counter?.LastSeq ?? 0,
            tombstoneHorizonSeq = counter?.TombstoneHorizonSeq ?? 0,
            entities = byEntity,
        });
    }

    private static async Task<IResult> BackfillAsync(
        [FromQuery] Guid? tenantId,
        [FromServices] CentralApiDbContext db,
        [FromServices] MobileRecordBackfill backfill,
        CancellationToken ct)
    {
        if (Missing(tenantId) is { } error) return error;

        var result = await backfill.RunAsync(db, tenantId!.Value, ct);
        if (result.SnapshotId is null)
        {
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError
            {
                ErrorCode = "NO_ACTIVE_SNAPSHOT",
                Message = "The tenant has no active bootstrap snapshot to seed from.",
            });
        }

        return JsonResults.Ok(new
        {
            tenantId,
            snapshotId = result.SnapshotId,
            sections = result.Sections,
            seeded = result.Seeded,
            alreadyCurrent = result.AlreadyCurrent,
        });
    }

    private static IResult? Missing(Guid? tenantId) =>
        tenantId is null || tenantId.Value == Guid.Empty
            ? JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError
            {
                ErrorCode = "MISSING_TENANT",
                Message = "tenantId query parameter is required.",
            })
            : null;
}
