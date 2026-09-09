using System.Security.Cryptography;
using System.Text;
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
            .RequireAuthorization(Program.AgentOrApiKeyPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        group.MapGet("/status", StatusAsync)
            .WithName("ChangeSetStatus")
            .RequireAuthorization(Program.AgentOrApiKeyPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        return routes;
    }

    /// <summary>
    /// Stable idempotency key for one accepted bundle. The audit log uses
    /// the same key plus the direction to deduplicate retries; the snapshot
    /// upsert is already deduped by <c>(TenantId, SourceDatabase, TableName,
    /// LastTriggerRecNo)</c>.
    /// </summary>
    internal static string ComputeIdempotencyKey(SyncChangeSet body, SyncTableChangeSet table)
    {
        var raw = $"{body.SourceDatabase}|{table.Table.TableName}|{table.PreviousUpsertSequence}|{table.NewUpsertSequence}|{table.PreviousDeleteSequence}|{table.NewDeleteSequence}|{body.PulledAtUtc.UtcTicks}";
        var bytes = Encoding.UTF8.GetBytes(raw);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Best-effort SHA-256 over the payload JSON. Used by the audit viewer
    /// to confirm the row has not been silently truncated.
    /// </summary>
    internal static string ComputePayloadSha256(string payloadJson)
    {
        if (string.IsNullOrEmpty(payloadJson)) return string.Empty;
        var bytes = Encoding.UTF8.GetBytes(payloadJson);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static async Task<IResult> IngestAsync(
        [FromBody] SyncChangeSet body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        [FromServices] ErpBridge.CentralApi.Notifications.IBootstrapNotificationHub hub,
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
        var idempotencyKey = ComputeIdempotencyKeyForBundle(body, http);
        var agentId = http.User.FindFirst("sub")?.Value
                       ?? http.User.Identity?.Name
                       ?? string.Empty;

        foreach (var table in body.Tables)
        {
            ct.ThrowIfCancellationRequested();
            // A cycle that only deleted rows advances NewDeleteSequence while
            // NewUpsertSequence stays put — so gate on both, never just upserts.
            if (table.NewUpsertSequence <= 0 && table.NewDeleteSequence <= 0) continue;

            var json = JsonSerializer.Serialize(table, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            // The unique index on (TenantId, SourceDatabase, TableName,
            // LastTriggerRecNo, LastDeleteRecNo) makes a duplicate push a no-op.
            // EF's SaveChanges swallows the 23505 unique-violation for
            // PostgreSQL; we check first to avoid an exception round-trip.
            var exists = await db.ChangeSets.AsNoTracking().AnyAsync(c =>
                    c.TenantId == tenantId &&
                    c.SourceDatabase == body.SourceDatabase &&
                    c.TableName == table.Table.TableName &&
                    c.LastTriggerRecNo == table.NewUpsertSequence &&
                    c.LastDeleteRecNo == table.NewDeleteSequence,
                ct);
            if (exists)
            {
                duplicates++;
                continue;
            }

            db.ChangeSets.Add(new ChangeSetRecord
            {
                TenantId = tenantId,
                ErpType = string.IsNullOrWhiteSpace(body.ErpType) ? "Mikro" : body.ErpType,
                SourceDatabase = body.SourceDatabase,
                TableKey = table.Table.TableKey,
                TableName = table.Table.TableName,
                LastTriggerRecNo = table.NewUpsertSequence,
                LastDeleteRecNo = table.NewDeleteSequence,
                PayloadJson = json,
                PulledAtUtc = body.PulledAtUtc,
            });

            await AddMobileQueueItems(db, tenantId, body.SourceDatabase, table, ct);

            // Faz 15.6: append one audit row per non-empty direction. The
            // (TenantId, IdempotencyKey, Direction) unique index makes a
            // retry of the same bundle a no-op on the audit side too.
            var auditKey = ComputeIdempotencyKey(body, table);
            var erp = string.IsNullOrWhiteSpace(body.ErpType) ? "Mikro" : body.ErpType;
            AppendAuditIfPresent(db, tenantId, erp, body.SourceDatabase, table, "new", auditKey, json, body.PulledAtUtc, agentId, idempotencyKey);
            AppendAuditIfPresent(db, tenantId, erp, body.SourceDatabase, table, "changed", auditKey, json, body.PulledAtUtc, agentId, idempotencyKey);
            AppendAuditIfPresent(db, tenantId, erp, body.SourceDatabase, table, "deleted", auditKey, json, body.PulledAtUtc, agentId, idempotencyKey);

            accepted++;
        }

        if (accepted > 0)
        {
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException) when (duplicates > 0)
            {
                // A retry could have raced a concurrent insert. The
                // 23505 unique-violation is the expected outcome for the
                // audit row; ignore so the rest of the bundle commits.
            }
        }

        // Wake any client long-polling /api/v1/android/notify so an ERP change
        // reaches the device in seconds instead of on the next periodic sync.
        if (accepted > 0)
        {
            hub.Publish(tenantId, body.PulledAtUtc);
        }

        return Results.Ok(new { accepted, duplicates });
    }

    private static async Task AddMobileQueueItems(
        CentralApiDbContext db,
        Guid tenantId,
        string sourceDatabase,
        SyncTableChangeSet table,
        CancellationToken ct)
    {
        var entity = table.Table.TableName switch
        {
            "STOKLAR" => "product",
            "CARI_HESAPLAR" => "customer",
            "CARI_HESAP_HAREKETLERI" => "invoice", // shared source; collection consumers also receive it
            "STOK_HAREKETLERI" => "invoice",
            "ODEME_EMIRLERI" => "collection",
            _ => null,
        };
        if (entity is null) return;
        var seen = new HashSet<string>(StringComparer.Ordinal);

        // Faz 20: upsert rows carry an explicit RecordKey, so the queue item's
        // identity no longer depends on guessing a "*RECno" column — which
        // GUID-keyed tables (Mikro V16, Logo) do not have.
        var upsertSequence = Math.Max(table.New?.HighestSequence ?? 0, table.Changed?.HighestSequence ?? 0);
        foreach (var row in table.New?.Rows ?? Array.Empty<SyncUpsertRow>())
            AddUpsertRow(db, tenantId, sourceDatabase, table, entity, row, upsertSequence, seen);
        foreach (var row in table.Changed?.Rows ?? Array.Empty<SyncUpsertRow>())
            AddUpsertRow(db, tenantId, sourceDatabase, table, entity, row, upsertSequence, seen);

        foreach (var row in table.Deleted?.Rows ?? Array.Empty<SyncDeletedRow>())
        {
            var sourceRecordKey = row.RecordKey;
            var recordKey = await db.MobileSyncQueue
                .Where(x => x.TenantId == tenantId && x.SourceDatabase == sourceDatabase &&
                            x.TableName == table.Table.TableName && x.SourceRecordKey == sourceRecordKey)
                .OrderByDescending(x => x.Sequence)
                .Select(x => x.RecordKey)
                .FirstOrDefaultAsync(ct) ?? sourceRecordKey;
            if (!seen.Add($"{recordKey}:delete:{row.Sequence}")) continue;
            var payload = JsonSerializer.Serialize(new { recordKey, sourceRecordKey, sequence = row.Sequence });
            db.MobileSyncQueue.Add(new MobileSyncQueueItem
            {
                TenantId = tenantId,
                SourceDatabase = sourceDatabase,
                TableName = table.Table.TableName,
                EntityType = entity,
                Operation = "delete",
                RecordKey = recordKey,
                SourceRecordKey = sourceRecordKey,
                TriggerRecNo = row.Sequence,
                PayloadJson = payload,
            });
        }
    }

    private static void AddUpsertRow(
        CentralApiDbContext db,
        Guid tenantId,
        string sourceDatabase,
        SyncTableChangeSet table,
        string entity,
        SyncUpsertRow row,
        long sequence,
        HashSet<string> seen)
    {
        var key = row.RecordKey;
        if (string.IsNullOrWhiteSpace(key)) return;
        if (!seen.Add($"{key}:upsert:{sequence}")) return;

        db.MobileSyncQueue.Add(new MobileSyncQueueItem
        {
            TenantId = tenantId,
            SourceDatabase = sourceDatabase,
            TableName = table.Table.TableName,
            EntityType = entity,
            Operation = "upsert",
            RecordKey = key,
            SourceRecordKey = key,
            TriggerRecNo = sequence,
            PayloadJson = Serialize(row.Columns),
        });
    }

    private static string Serialize(IReadOnlyDictionary<string, object?> row) =>
        JsonSerializer.Serialize(row, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static string ComputeIdempotencyKeyForBundle(SyncChangeSet body, HttpContext http)
    {
        // The bundle-level key is shared by every audit row in this push.
        // It is stable for retries: same body, same key, audit unique index
        // rejects the second insert.
        var raw = $"{body.SourceDatabase}|{body.PulledAtUtc.UtcTicks}|{body.Tables.Count}";
        var bytes = Encoding.UTF8.GetBytes(raw);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static void AppendAuditIfPresent(
        CentralApiDbContext db,
        Guid tenantId,
        string erpType,
        string sourceDatabase,
        SyncTableChangeSet table,
        string direction,
        string idempotencyKey,
        string fullPayloadJson,
        DateTimeOffset pulledAtUtc,
        string agentId,
        string bundleIdempotencyKey)
    {
        object perDirection;
        long highestSequence;
        int rowCount;
        if (direction == "new" && table.New is { Rows.Count: > 0 } newChunk)
        {
            perDirection = new
            {
                table.Table.TableKey,
                table.Table.TableName,
                table.Table.KeyField,
                table.Table.Fields,
                direction,
                rows = newChunk.Rows,
                newChunk.HighestSequence,
            };
            highestSequence = newChunk.HighestSequence;
            rowCount = newChunk.Rows.Count;
        }
        else if (direction == "changed" && table.Changed is { Rows.Count: > 0 } changedChunk)
        {
            perDirection = new
            {
                table.Table.TableKey,
                table.Table.TableName,
                table.Table.KeyField,
                table.Table.Fields,
                direction,
                rows = changedChunk.Rows,
                changedChunk.HighestSequence,
            };
            highestSequence = changedChunk.HighestSequence;
            rowCount = changedChunk.Rows.Count;
        }
        else if (direction == "deleted" && table.Deleted is { Rows.Count: > 0 } deletedChunk)
        {
            perDirection = new
            {
                table.Table.TableKey,
                table.Table.TableName,
                table.Table.KeyField,
                direction,
                rows = deletedChunk.Rows,
                deletedChunk.HighestSequence,
            };
            highestSequence = deletedChunk.HighestSequence;
            rowCount = deletedChunk.Rows.Count;
        }
        else
        {
            return;
        }

        var perDirectionJson = JsonSerializer.Serialize(perDirection, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        db.ChangeSetAuditEntries.Add(new ChangeSetAuditEntry
        {
            TenantId = tenantId,
            ErpType = string.IsNullOrWhiteSpace(erpType) ? "Mikro" : erpType,
            SourceDatabase = sourceDatabase,
            TableKey = table.Table.TableKey,
            TableName = table.Table.TableName,
            Direction = direction,
            FirstTriggerRecNo = highestSequence,
            LastTriggerRecNo = highestSequence,
            RowCount = rowCount,
            PayloadJson = perDirectionJson,
            PayloadSha256 = ComputePayloadSha256(perDirectionJson),
            PulledAtUtc = pulledAtUtc,
            AgentId = agentId,
            IdempotencyKey = $"{idempotencyKey}:{direction}",
        });
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
            .GroupBy(c => new { c.TableKey, c.TableName, c.ErpType, c.SourceDatabase })
            .Select(g => new
            {
                table = g.Key.TableName,
                tableKey = g.Key.TableKey,
                erpType = g.Key.ErpType,
                sourceDatabase = g.Key.SourceDatabase,
                lastTriggerRecNo = g.Max(x => x.LastTriggerRecNo),
                lastDeleteRecNo = g.Max(x => x.LastDeleteRecNo),
                lastPulledAtUtc = g.Max(x => x.PulledAtUtc),
            })
            .ToListAsync(ct);

        return Results.Ok(new { tenantId, items = rows });
    }
}
