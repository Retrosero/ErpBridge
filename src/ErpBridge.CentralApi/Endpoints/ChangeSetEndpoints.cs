using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Server-side counterpart of the agent's <see cref="IChangeSetSyncService"/>.
/// Persists the per-table change-set bundles in <c>change_sets</c> and
/// surfaces a <c>/status</c> endpoint the agent can poll to know the highest
/// <c>TriggerRECno</c> the central API has accepted for a given table.
///
/// The change-set path is independent of the legacy <c>bootstrap_packages</c>
/// table; both can coexist so a tenant can roll from one to the other
/// without a one-shot migration.
/// </summary>
public static class ChangeSetEndpoints
{
    private const string ChangeSetScope = "ingest:changeset";

    public static IEndpointRouteBuilder MapChangeSetEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/ingest/changeset").WithTags("ChangeSet");

        group.MapPost("", IngestAsync)
            .WithName("IngestChangeSet")
            .Produces(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status413PayloadTooLarge)
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        group.MapGet("/status", StatusAsync)
            .WithName("ChangeSetStatus")
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> IngestAsync(
        [FromBody] SyncChangeSet body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });
        }
        if (string.IsNullOrWhiteSpace(body.TenantId))
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_TENANT", Message = "tenantId is required." });
        }
        if (string.IsNullOrWhiteSpace(body.SourceDatabase))
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_SOURCE", Message = "sourceDatabase is required." });
        }

        if (!http.User.TryGetTenantId(out var tenantId) ||
            !Guid.TryParse(body.TenantId, out var bundleTenantGuid) ||
            bundleTenantGuid != tenantId)
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Tenant id mismatch or missing." });
        }

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        if (tenant is null || !tenant.IsActive)
        {
            return JsonResults.Status(StatusCodes.Status403Forbidden,
                new ApiError { ErrorCode = "TENANT_INACTIVE", Message = "Tenant is inactive." });
        }

        if (body.Tables is null || body.Tables.Count == 0)
        {
            return Results.Ok(new { accepted = 0 });
        }

        // Cap the per-bundle size. The HTTP layer enforces a global body limit
        // too; this extra check keeps a runaway bundle from spamming the table.
        const int maxTablesPerBundle = 200;
        if (body.Tables.Count > maxTablesPerBundle)
        {
            return JsonResults.Status(StatusCodes.Status413PayloadTooLarge,
                new ApiError { ErrorCode = "BUNDLE_TOO_LARGE", Message = $"At most {maxTablesPerBundle} tables per bundle." });
        }

        var accepted = 0;
        var duplicates = 0;
        foreach (var table in body.Tables)
        {
            ct.ThrowIfCancellationRequested();
            if (table.NewLastTriggerRecNo <= 0) continue;

            var json = JsonSerializer.Serialize(table, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            // The unique index on (TenantId, SourceDatabase, TableName, LastTriggerRecNo)
            // makes a duplicate push a no-op. EF's SaveChanges swallows the
            // unique-violation via the 23505 SQLState for PostgreSQL; we check
            // first to avoid an exception round-trip on the hot path.
            var exists = await db.ChangeSets.AsNoTracking().AnyAsync(c =>
                    c.TenantId == tenantId &&
                    c.SourceDatabase == body.SourceDatabase &&
                    c.TableName == table.Table.TabloAdi &&
                    c.LastTriggerRecNo == table.NewLastTriggerRecNo,
                ct);
            if (exists)
            {
                duplicates++;
                continue;
            }

            db.ChangeSets.Add(new ChangeSetRecord
            {
                TenantId = tenantId,
                SourceDatabase = body.SourceDatabase,
                TableName = table.Table.TabloAdi,
                TabloId = table.Table.TabloID,
                LastTriggerRecNo = table.NewLastTriggerRecNo,
                PayloadJson = json,
                PulledAtUtc = body.PulledAtUtc,
            });
            accepted++;
        }

        if (accepted > 0)
        {
            await db.SaveChangesAsync(ct);
        }

        return Results.Ok(new { accepted, duplicates });
    }

    private static async Task<IResult> StatusAsync(
        [FromQuery] string? sourceDatabase,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });
        }

        var query = db.ChangeSets.AsNoTracking().Where(c => c.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(sourceDatabase))
        {
            query = query.Where(c => c.SourceDatabase == sourceDatabase);
        }

        var rows = await query
            .GroupBy(c => new { c.TableName, c.TabloId, c.SourceDatabase })
            .Select(g => new
            {
                table = g.Key.TableName,
                tabloId = g.Key.TabloId,
                sourceDatabase = g.Key.SourceDatabase,
                lastTriggerRecNo = g.Max(x => x.LastTriggerRecNo),
                lastPulledAtUtc = g.Max(x => x.PulledAtUtc),
            })
            .ToListAsync(ct);

        return Results.Ok(new { tenantId, items = rows });
    }
}
