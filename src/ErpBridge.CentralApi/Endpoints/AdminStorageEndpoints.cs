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

        // The bytea move (S10): all companies at once, so not under a tenant.
        var migration = routes.MapGroup("/api/v1/admin/storage/migration")
            .WithTags("Admin/Storage")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);
        migration.MapGet("/", MigrationAsync).WithName("AdminStorageMigration")
            .Produces<AdminBlobMigrationResponse>(StatusCodes.Status200OK);
        migration.MapPost("/run", RunMigrationAsync).WithName("AdminStorageMigrationRun")
            .Produces<AdminBlobMigrationRunResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict);
        return routes;
    }

    /// <summary>
    /// Where the bytea move stands, per company, and whether the blob tables may be dropped: nothing left, nothing failed and
    /// (unless <c>verify=false</c>) every moved row's file checked against its blob's SHA-256 — which reads every moved blob.
    /// </summary>
    private static async Task<IResult> MigrationAsync(bool? verify, [FromServices] CentralApiDbContext db, [FromServices] BlobMigration migration,
        [FromServices] BlobMigrationState state, [FromServices] FileStore files, [FromServices] Microsoft.Extensions.Options.IOptions<StorageOptions> options, CancellationToken ct)
    {
        var counts = await migration.CountAsync(ct);
        var check = verify == false ? null : await migration.VerifyAsync(ct);
        var tenantIds = counts.Keys.Concat(check?.ProblemsByTenant.Keys ?? Enumerable.Empty<Guid>()).Distinct().ToList();
        var codes = await db.Tenants.AsNoTracking().Where(t => tenantIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.Code, ct);

        var tenants = tenantIds.Select(id =>
        {
            var c = counts.GetValueOrDefault(id) ?? new BlobMigrationCounts();
            return new AdminBlobMigrationTenant
            {
                TenantId = id,
                TenantCode = codes.GetValueOrDefault(id),
                RemainingCount = c.RemainingCount,
                RemainingBytes = c.RemainingBytes,
                MigratedCount = c.MigratedCount,
                MigratedBytes = c.MigratedBytes,
                FailedCount = c.FailedCount,
                FailedBytes = c.FailedBytes,
                SkippedCount = c.SkippedCount,
                SkippedBytes = c.SkippedBytes,
                VerifiedCount = check?.VerifiedByTenant.GetValueOrDefault(id),
                ProblemCount = check?.ProblemsByTenant.GetValueOrDefault(id),
            };
        }).OrderByDescending(t => t.RemainingBytes).ThenBy(t => t.TenantCode).ToArray();

        var totals = new AdminBlobMigrationCounts
        {
            RemainingCount = tenants.Sum(t => t.RemainingCount),
            RemainingBytes = tenants.Sum(t => t.RemainingBytes),
            MigratedCount = tenants.Sum(t => t.MigratedCount),
            MigratedBytes = tenants.Sum(t => t.MigratedBytes),
            FailedCount = tenants.Sum(t => t.FailedCount),
            FailedBytes = tenants.Sum(t => t.FailedBytes),
            SkippedCount = tenants.Sum(t => t.SkippedCount),
            SkippedBytes = tenants.Sum(t => t.SkippedBytes),
            VerifiedCount = check?.Verified,
            ProblemCount = check?.ProblemCount,
        };
        return JsonResults.Ok(new AdminBlobMigrationResponse
        {
            Enabled = options.Value.MaintenanceEnabled && options.Value.BlobMigrationEnabled,
            Available = files.IsAvailable,
            Idle = state.Idle,
            Verified = check is not null,
            ReadyToDrop = check is not null && totals.RemainingCount == 0 && totals.FailedCount == 0 && check.ProblemCount == 0,
            Totals = totals,
            Tenants = tenants,
            Failures = [.. state.Failures.OrderBy(f => f.AtMs).Take(BlobMigration.MaxListed).Select(f => new AdminBlobMigrationItem
            {
                TenantId = f.Key.TenantId, Source = f.Key.Source, Id = f.Key.Id, Variant = f.Key.Variant, Reason = f.Reason, SizeBytes = f.SizeBytes, AtMs = f.AtMs,
            })],
            Problems = [.. (check?.Problems ?? []).Take(BlobMigration.MaxListed).Select(p => new AdminBlobMigrationItem
            {
                TenantId = p.Key.TenantId, Source = p.Key.Source, Id = p.Key.Id, Variant = p.Key.Variant, Reason = p.Reason,
            })],
        });
    }

    /// <summary>
    /// One run now (<c>budget</c> files, default 500, at most 5000; one company with <c>tenantId</c>), also while the worker is
    /// off. <c>retryFailed=true</c> lets rows that failed before be tried again. Wakes the worker up again.
    /// </summary>
    private static async Task<IResult> RunMigrationAsync(int? budget, Guid? tenantId, bool? retryFailed, HttpContext http, [FromServices] CentralApiDbContext db,
        [FromServices] BlobMigration migration, [FromServices] BlobMigrationState state, [FromServices] ILoggerFactory loggers, CancellationToken ct)
    {
        if (budget is < 1 or > BlobMigration.MaxRunBudget)
            return Error(StatusCodes.Status400BadRequest, "INVALID_BUDGET", $"budget must be between 1 and {BlobMigration.MaxRunBudget}.");
        if (tenantId is { } only && !await db.Tenants.AsNoTracking().AnyAsync(t => t.Id == only, ct)) return TenantNotFound();
        if (retryFailed == true) state.ClearFailures();
        state.Idle = false;
        var run = await migration.TryRunAsync(budget ?? BlobMigration.RunBudget, tenantId, ct);
        if (run is null) return Error(StatusCodes.Status409Conflict, "STORAGE_MIGRATION_RUNNING", "A move run is already going on; try again in a minute.");
        loggers.CreateLogger("ErpBridge.CentralApi.Storage").LogInformation(
            "Blob move run by admin {AdminId}: {Migrated} moved, {Failed} failed.", http.User.FindFirstValue("sub"), run.Migrated, run.Failed);
        return JsonResults.Ok(new AdminBlobMigrationRunResponse
        {
            Migrated = run.Migrated,
            MigratedBytes = run.MigratedBytes,
            Failed = run.Failed,
            Lost = run.Lost,
            StoreUnavailable = run.StoreUnavailable,
            More = run.More,
        });
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
