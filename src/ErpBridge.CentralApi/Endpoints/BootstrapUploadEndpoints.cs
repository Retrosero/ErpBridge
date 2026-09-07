using System.Text.Json;
using System.Text.Json.Nodes;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Chunked bootstrap writer. Uploads are staged in separate rows and become
/// visible to Android only after the final commit, so a partial upload can
/// never replace a valid snapshot.
/// </summary>
public static class BootstrapUploadEndpoints
{
    public const int MaxItemsPerChunk = 500;
    private const int MaxChunkBytes = 8 * 1024 * 1024;

    public static IEndpointRouteBuilder MapBootstrapUploadEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/bootstrap/upload").WithTags("Bootstrap");
        group.MapPost("/start", StartAsync).RequireAuthorization(Program.AgentPolicy).RequireRateLimiting(Program.PerAgentRateLimitPolicy);
        group.MapPost("/{uploadId:guid}/chunks", ChunkAsync).RequireAuthorization(Program.AgentPolicy).RequireRateLimiting(Program.PerAgentRateLimitPolicy);
        group.MapPost("/{uploadId:guid}/complete", CompleteAsync).RequireAuthorization(Program.AgentPolicy).RequireRateLimiting(Program.PerAgentRateLimitPolicy);
        return routes;
    }

    private static async Task<IResult> StartAsync(
        [FromBody] BootstrapUploadStartRequest body,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.SourceDatabase))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BOOTSTRAP_UPLOAD", Message = "sourceDatabase is required." });
        if (!http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "INVALID_TOKEN", Message = "JWT missing tenant claim." });

        var idempotencyKey = http.Request.Headers["Idempotency-Key"].FirstOrDefault();
        var uploadId = Guid.TryParse(idempotencyKey?.Split(':').LastOrDefault(), out var parsedUploadId)
            ? parsedUploadId
            : Guid.NewGuid();
        var existingUpload = await db.BootstrapSnapshots.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == uploadId && x.TenantId == tenantId, ct);
        if (existingUpload is not null)
            return Results.Ok(new BootstrapUploadStartResponse { UploadId = existingUpload.Id, MaxItemsPerChunk = MaxItemsPerChunk });

        var stale = await db.BootstrapSnapshots
            .Where(x => x.TenantId == tenantId && !x.IsActive)
            .ToListAsync(ct);
        if (stale.Count > 0) db.BootstrapSnapshots.RemoveRange(stale);

        var staged = new BootstrapSnapshot
        {
            Id = uploadId, TenantId = tenantId, SourceDatabase = body.SourceDatabase,
            PulledAtUtc = body.PulledAtUtc, ReceivedAtUtc = DateTimeOffset.UtcNow,
            ActivatedAtUtc = DateTimeOffset.UtcNow, IsActive = false, IsIncremental = body.IsIncremental,
        };
        db.BootstrapSnapshots.Add(staged);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new BootstrapUploadStartResponse { UploadId = staged.Id, MaxItemsPerChunk = MaxItemsPerChunk });
    }

    private static async Task<IResult> ChunkAsync(
        Guid uploadId,
        [FromBody] BootstrapUploadChunkRequest body,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.Section) || body.ChunkIndex < 0 || body.Items.ValueKind != JsonValueKind.Array)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BOOTSTRAP_CHUNK", Message = "section, non-negative chunkIndex and an items array are required." });
        if (!http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "INVALID_TOKEN", Message = "JWT missing tenant claim." });
        if (body.Items.GetArrayLength() > MaxItemsPerChunk)
            return JsonResults.Status(StatusCodes.Status413PayloadTooLarge, new ApiError { ErrorCode = "CHUNK_TOO_LARGE", Message = $"A chunk may contain at most {MaxItemsPerChunk} items." });

        var payloadJson = body.Items.GetRawText();
        if (System.Text.Encoding.UTF8.GetByteCount(payloadJson) > MaxChunkBytes)
            return JsonResults.Status(StatusCodes.Status413PayloadTooLarge, new ApiError { ErrorCode = "CHUNK_TOO_LARGE", Message = "Bootstrap chunk exceeds the 8 MiB limit." });

        var snapshot = await db.BootstrapSnapshots.FirstOrDefaultAsync(x => x.Id == uploadId && x.TenantId == tenantId && !x.IsActive, ct);
        if (snapshot is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "BOOTSTRAP_UPLOAD_NOT_FOUND", Message = "Bootstrap upload was not found or is already complete." });

        var existing = await db.BootstrapSnapshotChunks.FirstOrDefaultAsync(x => x.SnapshotId == uploadId && x.Section == body.Section && x.ChunkIndex == body.ChunkIndex, ct);
        if (existing is null)
        {
            db.BootstrapSnapshotChunks.Add(new BootstrapSnapshotChunk
            {
                Id = Guid.NewGuid(), SnapshotId = uploadId, Section = body.Section,
                ChunkIndex = body.ChunkIndex, ItemCount = body.Items.GetArrayLength(),
                PayloadJson = payloadJson, ReceivedAtUtc = DateTimeOffset.UtcNow,
            });
        }
        else if (!string.Equals(existing.PayloadJson, payloadJson, StringComparison.Ordinal))
        {
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError { ErrorCode = "BOOTSTRAP_CHUNK_CONFLICT", Message = "The same chunk index was already uploaded with different data." });
        }
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> CompleteAsync(
        Guid uploadId,
        HttpContext http,
        CentralApiDbContext db,
        [FromServices] IBootstrapNotificationHub hub,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "INVALID_TOKEN", Message = "JWT missing tenant claim." });

        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        var staged = await db.BootstrapSnapshots.FirstOrDefaultAsync(x => x.Id == uploadId && x.TenantId == tenantId && !x.IsActive, ct);
        if (staged is null)
        {
            var alreadyActive = await db.BootstrapSnapshots.AnyAsync(x => x.Id == uploadId && x.TenantId == tenantId && x.IsActive, ct);
            if (alreadyActive) return Results.NoContent();
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "BOOTSTRAP_UPLOAD_NOT_FOUND", Message = "Bootstrap upload was not found or is already complete." });
        }

        var chunkCount = await db.BootstrapSnapshotChunks.CountAsync(x => x.SnapshotId == uploadId, ct);
        if (chunkCount == 0)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "BOOTSTRAP_UPLOAD_EMPTY", Message = "At least one bootstrap chunk is required." });

        var old = await db.BootstrapSnapshots.Where(x => x.TenantId == tenantId && x.IsActive).ToListAsync(ct);
        if (staged.IsIncremental && old.Count > 0)
            await MergeIncrementalChunksAsync(db, old[0], staged, ct);
        foreach (var previous in old) previous.IsActive = false;
        staged.IsActive = true;
        staged.ActivatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        if (transaction is not null)
            await transaction.CommitAsync(ct);

        foreach (var previous in old)
            db.BootstrapSnapshots.Remove(previous);
        await db.SaveChangesAsync(ct);
        hub.Publish(tenantId, staged.PulledAtUtc);
        return Results.NoContent();
    }

    private static async Task MergeIncrementalChunksAsync(
        CentralApiDbContext db,
        BootstrapSnapshot previous,
        BootstrapSnapshot staged,
        CancellationToken ct)
    {
        var incoming = await db.BootstrapSnapshotChunks.AsNoTracking()
            .Where(x => x.SnapshotId == staged.Id)
            .OrderBy(x => x.Section).ThenBy(x => x.ChunkIndex)
            .ToListAsync(ct);
        var sections = incoming.Select(x => x.Section).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        foreach (var section in sections)
        {
            var oldChunks = await db.BootstrapSnapshotChunks.AsNoTracking()
                .Where(x => x.SnapshotId == previous.Id && x.Section == section)
                .OrderBy(x => x.ChunkIndex).ToListAsync(ct);
            var records = new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);
            var anonymous = new List<JsonNode>();
            foreach (var chunk in oldChunks)
                AddItems(chunk.PayloadJson, section, records, anonymous);
            foreach (var chunk in incoming.Where(x => x.Section == section))
                AddItems(chunk.PayloadJson, section, records, anonymous);

            var merged = records.Values.Concat(anonymous).ToArray();
            var stagedChunks = await db.BootstrapSnapshotChunks.Where(x => x.SnapshotId == staged.Id && x.Section == section).ToListAsync(ct);
            db.BootstrapSnapshotChunks.RemoveRange(stagedChunks);
            for (var offset = 0; offset < merged.Length; offset += MaxItemsPerChunk)
            {
                var part = merged.Skip(offset).Take(MaxItemsPerChunk).ToArray();
                var payload = new JsonArray();
                foreach (var item in part) payload.Add(item);
                db.BootstrapSnapshotChunks.Add(new BootstrapSnapshotChunk
                {
                    Id = Guid.NewGuid(), SnapshotId = staged.Id, Section = section,
                    ChunkIndex = offset / MaxItemsPerChunk, ItemCount = part.Length,
                    PayloadJson = payload.ToJsonString(), ReceivedAtUtc = DateTimeOffset.UtcNow,
                });
            }
        }
        await db.SaveChangesAsync(ct);
    }

    private static void AddItems(string payload, string section, IDictionary<string, JsonNode> records, ICollection<JsonNode> anonymous)
    {
        using var document = JsonDocument.Parse(payload);
        foreach (var element in document.RootElement.EnumerateArray())
        {
            var node = JsonNode.Parse(element.GetRawText())!;
            var key = RowKey(section, element);
            var deleted = element.ValueKind == JsonValueKind.Object && element.TryGetProperty("isDeleted", out var flag) && flag.ValueKind == JsonValueKind.True;
            if (key is null) { if (!deleted) anonymous.Add(node); continue; }
            if (deleted) records.Remove(key);
            else records[key] = node;
        }
    }

    private static string? RowKey(string section, JsonElement item)
    {
        string? Value(string name) => item.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value.ToString() : null;
        string? Join(params string[] names)
        {
            var values = names.Select(Value).ToArray();
            return values.Any(string.IsNullOrWhiteSpace) ? null : string.Join('|', values);
        }
        return section.ToLowerInvariant() switch
        {
            "customers" => Join("customerCode"),
            "customeraddresses" => Join("customerCode", "addressNo"),
            "customercontacts" => Join("customerCode", "email", "mobile"),
            "stocks" => Join("stockCode"),
            "barcodes" => Join("barcode"),
            "prices" => Join("stockCode", "listNumber"),
            "salesconditions" => Join("stockCode", "customerCode", "warehouseNo", "paymentPlanNo"),
            "inventory" => Join("stockCode", "warehouseNo"),
            "openorders" => Join("series", "number", "lineNo"),
            "cashandbank" or "lookups" => Join("kind", "code"),
            "customertransactions" or "stocktransactions" => Join("id"),
            _ => null,
        };
    }
}
