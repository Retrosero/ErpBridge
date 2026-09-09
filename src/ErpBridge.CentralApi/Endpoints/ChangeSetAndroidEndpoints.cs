using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Faz 13.3 / Faz 15.7 — Android-facing counterpart of
/// <see cref="ChangeSetEndpoints"/>. Each tracked table exposes a
/// three-way stream (new / changed / deleted). The cursor is the
/// <c>TriggerRECno</c> the mobile client last saw for that table; the
/// server returns the next batch and the new cursor in
/// <c>nextCursor</c>.
///
/// <para>
/// Faz 15.7 — primary direction is <c>new_or_changed</c> which merges
/// the legacy <c>new</c> and <c>changed</c> streams into one payload,
/// matching the FORA reference app's semantics. The two older endpoints
/// remain as aliases for backwards compatibility with Android clients
/// built before the merge. A <c>status</c> endpoint lets the mobile
/// discover whether a given table has anything pending before paging.
/// </para>
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

        // Faz 15.7 — primary direction. Mirrors the FORA semantic of "new +
        // changed rows in one stream". The two older endpoints (/new,
        // /changed) are kept as aliases below for backwards compatibility.
        group.MapGet("/{table}/new_or_changed", ReadNewOrChangedAsync)
            .WithName("AndroidChangeSetNewOrChanged")
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

        // Faz 15.7 — discovery endpoint. Reports the highest TriggerRECno
        // the central API has accepted for a table plus a "has data" flag.
        group.MapGet("/{table}/status", StatusAsync)
            .WithName("AndroidChangeSetStatus")
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        routes.MapGet("/api/v1/android/sync/queue", ReadMobileQueueAsync)
            .WithName("AndroidMobileSyncQueue")
            .WithTags("AndroidMobileSync")
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> ReadMobileQueueAsync(
        [FromQuery] long? cursor,
        [FromQuery] string? entity,
        [FromQuery] int? size,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });

        if (entity is not null && entity is not ("product" or "customer" or "invoice" or "collection"))
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "INVALID_ENTITY", Message = "entity must be product, customer, invoice or collection." });

        var start = cursor ?? 0;
        var pageSize = ClampPageSize(size);
        var query = db.MobileSyncQueue.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Sequence > start);

        if (entity == "product")
            query = query.Where(x => x.EntityType == "product");
        else if (entity == "customer")
            query = query.Where(x => x.EntityType == "customer");
        else if (entity == "invoice")
            query = query.Where(x => x.EntityType == "invoice");
        else if (entity == "collection")
            query = query.Where(x => x.EntityType == "collection" || x.TableName == "CARI_HESAP_HAREKETLERI");

        var rows = await query.OrderBy(x => x.Sequence).Take(pageSize + 1)
            .Select(x => new
            {
                sequence = x.Sequence,
                table = x.TableName,
                entity = x.EntityType,
                operation = x.Operation,
                recordKey = x.RecordKey,
                triggerRecNo = x.TriggerRecNo,
                payload = x.PayloadJson,
                createdAtUtc = x.CreatedAtUtc,
            }).ToListAsync(ct);

        var hasMore = rows.Count > pageSize;
        if (hasMore) rows.RemoveAt(rows.Count - 1);
        var nextCursor = rows.Count == 0 ? start : rows[^1].sequence;
        return JsonResults.Ok(new { cursor = start, nextCursor = hasMore ? nextCursor : (long?)null, items = rows });
    }

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
            .GroupBy(c => new { c.TableKey, c.TableName, c.ErpType })
            .Select(g => new
            {
                table = g.Key.TableName,
                tableKey = g.Key.TableKey,
                erpType = g.Key.ErpType,
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
        return await ReadChunkAsync(table, "new", cursor, size, db, http, ct).ConfigureAwait(false);
    }

    private static async Task<IResult> ReadChangedAsync(
        string table,
        [FromQuery] long? cursor,
        [FromQuery] int? size,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        return await ReadChunkAsync(table, "changed", cursor, size, db, http, ct).ConfigureAwait(false);
    }

    private static async Task<IResult> ReadDeletedAsync(
        string table,
        [FromQuery] long? cursor,
        [FromQuery] int? size,
        [FromServices] CentralApiDbContext db,
        HttpContext http,
        CancellationToken ct)
    {
        // Deleted rows only carry the primary-key RECno + TriggerRECno pair;
        // we keep the projection small on purpose.
        if (!http.User.TryGetTenantId(out var tenantId))
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
            .Where(c => c.TenantId == tenantId &&
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
                    if (element.TryGetProperty("RecordKey", out var keyEl) && keyEl.ValueKind == JsonValueKind.String &&
                        element.TryGetProperty("Sequence", out var seq) && seq.TryGetInt64(out var seqValue))
                    {
                        flat.Add(new { recordKey = keyEl.GetString(), sequence = seqValue });
                        if (seqValue > maxCursor) maxCursor = seqValue;
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

    /// <summary>
    /// Faz 15.7 — primary direction. Combines <c>new</c> and <c>changed</c>
    /// chunks into a single paginated stream. The two legacy endpoints
    /// (<c>/new</c>, <c>/changed</c>) remain as thin wrappers around
    /// <see cref="ReadChunkAsync"/> below.
    /// </summary>
    private static async Task<IResult> ReadNewOrChangedAsync(
        string table,
        [FromQuery] long? cursor,
        [FromQuery] int? size,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
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

        // Fetch the new and changed bundles in parallel — they hit the
        // same payload column so the wall-clock cost is dominated by the
        // network round-trip, not the JSON parse.
        var newTask = ReadChunkRowsAsync(tenantId, table, "new", startCursor, pageSize, db, ct);
        var changedTask = ReadChunkRowsAsync(tenantId, table, "changed", startCursor, pageSize, db, ct);
        await Task.WhenAll(newTask, changedTask).ConfigureAwait(false);

        var newChunk = newTask.Result;
        var changedChunk = changedTask.Result;

        var merged = new List<object>(newChunk.Rows.Count + changedChunk.Rows.Count);
        merged.AddRange(newChunk.Rows);
        merged.AddRange(changedChunk.Rows);

        long nextCursor = startCursor;
        if (merged.Count >= pageSize)
        {
            nextCursor = Math.Max(newChunk.NextCursor ?? startCursor, changedChunk.NextCursor ?? startCursor);
        }

        return JsonResults.Ok(new
        {
            table,
            direction = "new_or_changed",
            cursor = startCursor,
            nextCursor = merged.Count >= pageSize ? (long?)nextCursor : null,
            rows = merged,
        });
    }

    private static async Task<IResult> ReadChunkAsync(
        string table,
        string direction,
        long? cursor,
        int? size,
        CentralApiDbContext db,
        HttpContext http,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
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
        var chunk = await ReadChunkRowsAsync(tenantId, table, direction, startCursor, pageSize, db, ct).ConfigureAwait(false);

        return JsonResults.Ok(new
        {
            table,
            direction,
            cursor = startCursor,
            nextCursor = chunk.NextCursor,
            rows = chunk.Rows,
        });
    }

    private static async Task<ChunkResult> ReadChunkRowsAsync(
        Guid tenantId,
        string table,
        string direction,
        long startCursor,
        int pageSize,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var rows = await db.ChangeSets.AsNoTracking()
            .Where(c => c.TenantId == tenantId &&
                        c.TableName == table &&
                        c.LastTriggerRecNo > startCursor)
            .OrderBy(c => c.LastTriggerRecNo)
            .Take(pageSize)
            .Select(c => new { triggerRecNo = c.LastTriggerRecNo, payload = c.PayloadJson })
            .ToListAsync(ct);

        var merged = new List<object>(rows.Count);
        long maxCursor = startCursor;
        foreach (var row in rows)
        {
            if (string.IsNullOrEmpty(row.payload)) continue;
            try
            {
                using var document = JsonDocument.Parse(row.payload);
                var chunkElement = document.RootElement;
                if (!chunkElement.TryGetProperty(direction switch
                {
                    "new" => "New",
                    "changed" => "Changed",
                    _ => "New",
                }, out chunkElement) || chunkElement.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }
                if (!chunkElement.TryGetProperty("Rows", out var rowsElement) || rowsElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }
                foreach (var element in rowsElement.EnumerateArray())
                {
                    // Faz 20 — each upsert row is { RecordKey, Columns }. Mobile
                    // consumers want the flat column map, so unwrap Columns when
                    // present and fall back to the raw element otherwise.
                    var payload = element.TryGetProperty("Columns", out var cols) && cols.ValueKind == JsonValueKind.Object
                        ? cols
                        : element;
                    merged.Add(JsonSerializer.Deserialize<JsonElement>(payload.GetRawText()));
                }
                maxCursor = Math.Max(maxCursor, row.triggerRecNo);
            }
            catch (JsonException)
            {
                // skip
            }
        }

        return new ChunkResult(merged, merged.Count >= pageSize ? maxCursor : (long?)null);
    }

    private static async Task<IResult> StatusAsync(
        string table,
        [FromServices] CentralApiDbContext db,
        HttpContext http,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });
        }

        if (string.IsNullOrWhiteSpace(table))
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_TABLE", Message = "Table name is required." });
        }

        var lastTriggerRecNo = await db.ChangeSets.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.TableName == table)
            .Select(c => (long?)c.LastTriggerRecNo)
            .DefaultIfEmpty()
            .MaxAsync(ct);

        return JsonResults.Ok(new
        {
            table,
            lastTriggerRecNo = lastTriggerRecNo ?? 0,
            hasPending = (lastTriggerRecNo ?? 0) > 0,
        });
    }

    private static int ClampPageSize(int? requested)
    {
        if (requested is null) return DefaultPageSize;
        return Math.Clamp(requested.Value, 1, 5000);
    }

    private readonly record struct ChunkResult(List<object> Rows, long? NextCursor);
}
