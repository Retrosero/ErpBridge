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
/// Android-facing counterpart of <see cref="ChangeSetEndpoints"/>. For each
/// tracked table the mobile client pulls three paged streams: new rows,
/// changed rows (UPDATE) and deleted rows (DELETE). The cursor is the
/// <c>TriggerRECno</c> the mobile last saw for that table; the server
/// returns the next batch and the new cursor in <c>nextCursor</c>.
///
/// The endpoints are deliberately opt-in: legacy Android builds keep using
/// the existing <c>/api/v1/android/sync/cari</c> style endpoints. A mobile
/// app that wants the trigger path registers the new routes against the
/// table names it needs.
/// </summary>
public static class ChangeSetAndroidEndpoints
{
    /// <summary>Default page size for change-set reads. Matches the agent's <c>TriggerPacketSize</c>.</summary>
    public const int DefaultPageSize = 1000;

    public static IEndpointRouteBuilder MapChangeSetAndroidEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/android/changeset").WithTags("AndroidChangeSet");

        group.MapGet("/tables", ListTablesAsync)
            .WithName("AndroidChangeSetTables")
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        group.MapGet("/{table}/new", ReadNewAsync)
            .WithName("AndroidChangeSetNew")
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        group.MapGet("/{table}/changed", ReadChangedAsync)
            .WithName("AndroidChangeSetChanged")
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        group.MapGet("/{table}/deleted", ReadDeletedAsync)
            .WithName("AndroidChangeSetDeleted")
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        return routes;
    }

    /// <summary>
    /// Mobile discoverability: list every tracked table that has at least
    /// one accepted change-set row. Returns the latest <c>TriggerRECno</c>
    /// the central API has for that table so the mobile client can decide
    /// whether to start pulling or to short-circuit.
    /// </summary>
    private static async Task<IResult> ListTablesAsync(
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });
        }

        var rows = await db.ChangeSets.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .GroupBy(c => new { c.TableName, c.TabloId })
            .Select(g => new
            {
                table = g.Key.TableName,
                tabloId = g.Key.TabloId,
                lastTriggerRecNo = g.Max(x => x.LastTriggerRecNo),
                lastPulledAtUtc = g.Max(x => x.PulledAtUtc),
                acceptedBundles = g.Count(),
            })
            .OrderBy(x => x.table)
            .ToListAsync(ct);

        return JsonResults.Ok(new { tenantId, tables = rows });
    }

    private static async Task<IResult> ReadNewAsync(
        string table,
        [FromQuery] long? cursor,
        [FromQuery] int? size,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        return await ReadChunkAsync(table, "new", cursor, size, db, http, ct, payload => payload is JsonElement e ? e : default);
    }

    private static async Task<IResult> ReadChangedAsync(
        string table,
        [FromQuery] long? cursor,
        [FromQuery] int? size,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        return await ReadChunkAsync(table, "changed", cursor, size, db, http, ct, payload => payload is JsonElement e ? e : default);
    }

    private static async Task<IResult> ReadDeletedAsync(
        string table,
        [FromQuery] long? cursor,
        [FromQuery] int? size,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        // Deleted rows only carry the primary-key RECno + TriggerRECno pair;
        // we keep the projection small on purpose.
        var tenantId = ResolveTenantId(http);
        if (tenantId is null)
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });
        }

        if (string.IsNullOrWhiteSpace(table))
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_TABLE", Message = "Table name is required." });
        }

        var pageSize = ClampPageSize(size);
        var startCursor = cursor ?? 0;

        var rows = await db.ChangeSets.AsNoTracking()
            .Where(c => c.TenantId == tenantId.Value &&
                        c.TableName == table &&
                        c.LastTriggerRecNo > startCursor)
            .OrderBy(c => c.LastTriggerRecNo)
            .Take(pageSize)
            .Select(c => new
            {
                triggerRecNo = c.LastTriggerRecNo,
                payload = c.PayloadJson,
            })
            .ToListAsync(ct);

        var flat = new List<object>(rows.Count);
        long maxCursor = startCursor;
        foreach (var row in rows)
        {
            if (string.IsNullOrEmpty(row.payload)) continue;
            try
            {
                using var document = JsonDocument.Parse(row.payload);
                if (!document.RootElement.TryGetProperty("Deleted", out var deletedElement) ||
                    deletedElement.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }
                if (!deletedElement.TryGetProperty("Rows", out var rowsElement) ||
                    rowsElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }
                foreach (var element in rowsElement.EnumerateArray())
                {
                    if (element.TryGetProperty("KayitRecNo", out var kayit) && kayit.TryGetInt32(out var kayitValue) &&
                        element.TryGetProperty("TriggerRecNo", out var tr) && tr.TryGetInt32(out var trValue))
                    {
                        flat.Add(new { kayitRecNo = kayitValue, triggerRecNo = trValue });
                        if (trValue > maxCursor) maxCursor = trValue;
                    }
                }
            }
            catch (JsonException)
            {
                // A corrupted payload should not stop the whole read; skip
                // and let the next bundle overwrite it.
            }
        }

        return JsonResults.Ok(new
        {
            table,
            cursor = startCursor,
            nextCursor = flat.Count == pageSize ? maxCursor : (long?)null,
            rows = flat,
        });
    }

    private static async Task<IResult> ReadChunkAsync(
        string table,
        string direction,
        long? cursor,
        int? size,
        CentralApiDbContext db,
        HttpContext http,
        CancellationToken ct,
        Func<JsonElement, JsonElement> selectChunk)
    {
        var tenantId = ResolveTenantId(http);
        if (tenantId is null)
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });
        }

        if (string.IsNullOrWhiteSpace(table))
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_TABLE", Message = "Table name is required." });
        }

        var pageSize = ClampPageSize(size);
        var startCursor = cursor ?? 0;

        var rows = await db.ChangeSets.AsNoTracking()
            .Where(c => c.TenantId == tenantId.Value &&
                        c.TableName == table &&
                        c.LastTriggerRecNo > startCursor)
            .OrderBy(c => c.LastTriggerRecNo)
            .Take(pageSize)
            .Select(c => new { triggerRecNo = c.LastTriggerRecNo, payload = c.PayloadJson })
            .ToListAsync(ct);

        var merged = new List<object>(rows.Count * 4);
        long maxCursor = startCursor;
        foreach (var row in rows)
        {
            if (string.IsNullOrEmpty(row.payload)) continue;
            try
            {
                using var document = JsonDocument.Parse(row.payload);
                var chunk = selectChunk(document.RootElement);
                if (chunk.ValueKind != JsonValueKind.Object) continue;
                if (!chunk.TryGetProperty("Rows", out var rowsElement) || rowsElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }
                foreach (var element in rowsElement.EnumerateArray())
                {
                    merged.Add(JsonSerializer.Deserialize<JsonElement>(element.GetRawText()));
                }
                maxCursor = Math.Max(maxCursor, row.triggerRecNo);
            }
            catch (JsonException)
            {
                // skip
            }
        }

        return JsonResults.Ok(new
        {
            table,
            direction,
            cursor = startCursor,
            nextCursor = merged.Count >= pageSize ? maxCursor : (long?)null,
            rows = merged,
        });
    }

    private static int ClampPageSize(int? requested)
    {
        if (requested is null) return DefaultPageSize;
        return Math.Clamp(requested.Value, 1, 5000);
    }

    private static Guid? ResolveTenantId(HttpContext http) =>
        http.User.TryGetTenantId(out var tenantId) ? tenantId : null;
}
