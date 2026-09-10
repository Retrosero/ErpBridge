using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Operator-facing read endpoints for <c>mobile_sync_queue</c> — the ERP → mobile
/// event stream.
///
/// <para>Until this existed the queue was reachable only through
/// <c>GET /api/v1/android/sync/queue</c>, which is guarded by a mobile API key,
/// so nobody could see it from the admin console. Bootstrap data was visible and
/// the live queue was not, which made "the snapshot arrives but nothing updates
/// in between" impossible to diagnose from the outside.</para>
/// </summary>
public static class AdminSyncQueueEndpoints
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 500;

    /// <summary>Map the admin sync-queue routes.</summary>
    public static IEndpointRouteBuilder MapAdminSyncQueueEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/sync-queue").WithTags("AdminSyncQueue");

        group.MapGet("/", ListAsync)
            .WithName("AdminSyncQueueList")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/summary", SummaryAsync)
            .WithName("AdminSyncQueueSummary")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? entity,
        [FromQuery] string? operation,
        [FromQuery] string? table,
        [FromQuery] int? page,
        [FromQuery] int? size,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        // The tenant comes from the query, not the token: IJwtIssuer.IssueForAdmin
        // mints sub/scope/jti only, so an admin principal carries no tenant
        // claim and TryGetTenantId would reject every admin request.
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_TENANT", Message = "tenantId query parameter is required." });
        }

        var pageSize = Math.Clamp(size ?? DefaultPageSize, 1, MaxPageSize);
        var pageIndex = Math.Max(1, page ?? 1);

        var query = db.MobileSyncQueue.AsNoTracking().Where(q => q.TenantId == tenantId.Value);
        if (!string.IsNullOrWhiteSpace(entity))
            query = query.Where(q => q.EntityType == entity);
        if (!string.IsNullOrWhiteSpace(operation))
            query = query.Where(q => q.Operation == operation);
        if (!string.IsNullOrWhiteSpace(table))
            query = query.Where(q => q.TableName == table);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(q => q.Sequence)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new
            {
                sequence = q.Sequence,
                sourceDatabase = q.SourceDatabase,
                table = q.TableName,
                entity = q.EntityType,
                operation = q.Operation,
                recordKey = q.RecordKey,
                sourceRecordKey = q.SourceRecordKey,
                triggerRecNo = q.TriggerRecNo,
                createdAtUtc = q.CreatedAtUtc,
            })
            .ToListAsync(ct);

        return JsonResults.Ok(new { tenantId, page = pageIndex, size = pageSize, total, items });
    }

    /// <summary>
    /// Per-entity counts plus the newest row's timestamp. This is the view that
    /// answers "is the agent still pushing?" at a glance — a stale
    /// <c>lastCreatedAtUtc</c> means the periodic cycle has stopped.
    /// </summary>
    private static async Task<IResult> SummaryAsync(
        [FromQuery] Guid? tenantId,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_TENANT", Message = "tenantId query parameter is required." });
        }

        var rows = await db.MobileSyncQueue.AsNoTracking()
            .Where(q => q.TenantId == tenantId.Value)
            .GroupBy(q => new { q.EntityType, q.Operation })
            .Select(g => new
            {
                entity = g.Key.EntityType,
                operation = g.Key.Operation,
                count = g.Count(),
                lastCreatedAtUtc = g.Max(x => x.CreatedAtUtc),
            })
            .ToListAsync(ct);

        var lastCreatedAtUtc = rows.Count == 0
            ? (DateTimeOffset?)null
            : rows.Max(r => r.lastCreatedAtUtc);

        return JsonResults.Ok(new
        {
            tenantId,
            total = rows.Sum(r => r.count),
            lastCreatedAtUtc,
            groups = rows.OrderBy(r => r.entity).ThenBy(r => r.operation),
        });
    }
}
