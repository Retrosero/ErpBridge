using System.Security.Claims;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/admin/tenants/{tenantId}/storage</c> (GOAL_DEPOLAMA_R2 S8, R2): the operator sees a company's storage,
/// sets its own quota (null = back to the default 5 GB) and recounts the counter from the ledger. The tenant comes from
/// the route (admin tokens carry none). A quota below what is already used is allowed: uploads stop until space is freed.
/// </summary>
public static class AdminStorageEndpoints
{
    /// <summary>The largest quota the console accepts (10 TB): a typo of a few zeros must not open the store wide.</summary>
    public const long MaxQuotaBytes = 10L * 1024 * 1024 * 1024 * 1024;

    public static IEndpointRouteBuilder MapAdminStorageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/tenants/{tenantId:guid}/storage")
            .WithTags("Admin/Storage")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);
        group.MapGet("/", GetAsync).WithName("AdminTenantStorage")
            .Produces<AdminTenantStorageResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);
        group.MapPut("/", SetQuotaAsync).WithName("AdminTenantStorageSetQuota")
            .Produces<AdminTenantStorageResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound);
        group.MapPost("/recount", RecountAsync).WithName("AdminTenantStorageRecount")
            .Produces<AdminStorageRecountResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);
        return routes;
    }

    private static async Task<IResult> GetAsync(Guid tenantId, [FromServices] CentralApiDbContext db, [FromServices] FileStore files, CancellationToken ct)
    {
        if (!await db.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantId, ct)) return TenantNotFound();
        return JsonResults.Ok(await ViewAsync(db, files, tenantId, ct));
    }

    private static async Task<IResult> SetQuotaAsync(Guid tenantId, HttpContext http, [FromBody] SetTenantStorageRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] FileStore files, [FromServices] ILoggerFactory loggers, CancellationToken ct)
    {
        if (body is null) return Error(StatusCodes.Status400BadRequest, "INVALID_BODY", "Body required.");
        if (body.QuotaBytes is < 0 or > MaxQuotaBytes)
            return Error(StatusCodes.Status400BadRequest, "INVALID_QUOTA", $"quotaBytes must be between 0 and {MaxQuotaBytes} (or null for the default).");
        if (!await db.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantId, ct)) return TenantNotFound();

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var row = await db.TenantStorage.FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        if (row is null)
        {
            row = new TenantStorage { TenantId = tenantId };
            db.TenantStorage.Add(row);
        }
        var before = row.QuotaBytes;
        row.QuotaBytes = body.QuotaBytes;
        row.UpdatedAtMs = now;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException) when (db.Entry(row).State == EntityState.Added)
        {
            // A first upload created the row at the same moment: set the quota on that row.
            db.Entry(row).State = EntityState.Detached;
            await db.TenantStorage.Where(s => s.TenantId == tenantId)
                .ExecuteUpdateAsync(u => u.SetProperty(s => s.QuotaBytes, body.QuotaBytes).SetProperty(s => s.UpdatedAtMs, now), ct);
        }
        loggers.CreateLogger("ErpBridge.CentralApi.Storage").LogInformation(
            "Storage quota of tenant {TenantId} changed from {Before} to {After} by admin {AdminId}.",
            tenantId, before?.ToString() ?? "default", body.QuotaBytes?.ToString() ?? "default", http.User.FindFirstValue("sub"));
        db.ChangeTracker.Clear();
        return JsonResults.Ok(await ViewAsync(db, files, tenantId, ct));
    }

    private static async Task<IResult> RecountAsync(Guid tenantId, [FromServices] CentralApiDbContext db, [FromServices] FileStore files, CancellationToken ct)
    {
        if (!await db.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantId, ct)) return TenantNotFound();
        var recount = await files.RecountAsync(tenantId, ct);
        return JsonResults.Ok(new AdminStorageRecountResponse
        {
            UsedBytesBefore = recount.UsedBefore,
            UsedBytesAfter = recount.UsedAfter,
            Storage = await ViewAsync(db, files, tenantId, ct),
        });
    }

    internal static async Task<AdminTenantStorageResponse> ViewAsync(CentralApiDbContext db, FileStore files, Guid tenantId, CancellationToken ct)
    {
        var usage = await StorageUsageSnapshot.ReadAsync(db, tenantId, ct);
        return new AdminTenantStorageResponse
        {
            TenantId = tenantId,
            Available = files.IsAvailable,
            UsedBytes = usage.UsedBytes,
            ReservedBytes = usage.Counter?.ReservedBytes ?? 0,
            QuotaBytes = files.QuotaOf(usage.Counter),
            DefaultQuotaBytes = files.QuotaOf(null),
            CustomQuotaBytes = usage.Counter?.QuotaBytes,
            RecountedAtMs = usage.Counter?.RecountedAtMs,
            Areas = usage.Areas,
            TrashedBytes = usage.TrashedBytes,
            TrashedCount = usage.TrashedCount,
        };
    }

    private static IResult TenantNotFound() => Error(StatusCodes.Status404NotFound, "TENANT_NOT_FOUND", "Tenant not found.");

    private static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}
