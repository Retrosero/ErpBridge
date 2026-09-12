using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Snapshots;
using ErpBridge.CentralApi.Sync;
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
        [FromServices] MobileRecordProjector projector,
        [FromServices] ILoggerFactory loggerFactory,
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

        // One transaction for the whole bundle. The change-set rows, the queue
        // rows, the snapshot eviction and the mobile tombstones all describe the
        // same deletion; committing some of them without the rest would leave a
        // device told about a change it can never reconcile. It is also what the
        // cursor allocator needs — its row lock is only meaningful while a
        // transaction is open.
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;

        var accepted = 0;
        var duplicates = 0;
        // Rows the ERP no longer has. They are evicted from the active
        // bootstrap snapshot below so the mobile read endpoints stop serving
        // them; without that the snapshot keeps every deleted row forever.
        var snapshotDeletes = new List<SnapshotDeleteApplier.DeletedRow>();
        var idempotencyKey = ComputeIdempotencyKeyForBundle(body, http);
        var logger = loggerFactory.CreateLogger(typeof(ChangeSetEndpoints));
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

            snapshotDeletes.AddRange(
                await AddMobileQueueItems(db, tenantId, body.SourceDatabase, table, ct));

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
            // Evicting before SaveChanges keeps the queue rows, the change-set
            // record and the snapshot rewrite in one transaction: a device can
            // never observe a delete event whose snapshot eviction was lost.
            var evicted = await SnapshotDeleteApplier.ApplyAsync(db, tenantId, snapshotDeletes, ct);
            if (evicted > 0)
            {
                logger.LogInformation(
                    "Evicted {Count} ERP-deleted rows from the active snapshot for tenant {TenantId}.",
                    evicted, tenantId);
            }
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

            // Faz 26: the same deletions, as tombstones on the cursor feed. This
            // is the only path that learns a row is gone — an incremental read
            // cannot report a row that is no longer there — so a device finds out
            // here or not at all.
            //
            // Reserving the cursor block locks the tenant's counter row until
            // commit, which is why it runs after the snapshot rewrite above
            // rather than around it.
            var tombstoned = await projector.ApplyDeletesAsync(db, tenantId, snapshotDeletes
                .Select(x => new MobileRecordProjector.DeletedRecord(
                    x.TableName, string.IsNullOrWhiteSpace(x.BusinessKey) ? x.RecordKey : x.BusinessKey!))
                .ToList(), ct);
            if (tombstoned > 0)
            {
                await db.SaveChangesAsync(ct);
                logger.LogInformation(
                    "Tombstoned {Count} mobile records for tenant {TenantId}.", tombstoned, tenantId);
            }
        }

        if (transaction is not null)
            await transaction.CommitAsync(ct);

        // Wake any client long-polling /api/v1/android/notify so an ERP change
        // reaches the device in seconds instead of on the next periodic sync.
        if (accepted > 0)
        {
            hub.Publish(tenantId, body.PulledAtUtc);
        }

        return Results.Ok(new { accepted, duplicates });
    }

    /// <summary>
    /// Business-code column per tracked table. A delete event can only name the
    /// physical row key, but every consumer downstream — the snapshot, the
    /// mobile projection — is keyed by the business code, so the code has to be
    /// recovered from an upsert that mentioned the same row.
    /// </summary>
    private static readonly Dictionary<string, string> BusinessKeyColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["STOKLAR"] = "sto_kod",
            ["CARI_HESAPLAR"] = "cari_kod",
        };

    private static string? ReadBusinessKey(IReadOnlyDictionary<string, object?>? columns, string tableName)
    {
        if (columns is null) return null;
        if (!BusinessKeyColumns.TryGetValue(tableName, out var column)) return null;
        foreach (var pair in columns)
        {
            if (!string.Equals(pair.Key, column, StringComparison.OrdinalIgnoreCase)) continue;
            var text = pair.Value switch
            {
                null => null,
                JsonElement element => element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString(),
                _ => pair.Value.ToString(),
            };
            return string.IsNullOrWhiteSpace(text) ? null : text!.Trim();
        }
        return null;
    }

    private static string? ReadBusinessKeyFromJson(string? payloadJson, string tableName)
    {
        if (string.IsNullOrWhiteSpace(payloadJson)) return null;
        if (!BusinessKeyColumns.TryGetValue(tableName, out var column)) return null;
        try
        {
            using var document = JsonDocument.Parse(payloadJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return null;
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!string.Equals(property.Name, column, StringComparison.OrdinalIgnoreCase)) continue;
                var text = property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString()
                    : property.Value.ToString();
                return string.IsNullOrWhiteSpace(text) ? null : text!.Trim();
            }
        }
        catch (JsonException)
        {
            // A corrupted queue payload must not fail the whole ingest.
        }
        return null;
    }

    private static async Task<List<SnapshotDeleteApplier.DeletedRow>> AddMobileQueueItems(
        CentralApiDbContext db,
        Guid tenantId,
        string sourceDatabase,
        SyncTableChangeSet table,
        CancellationToken ct)
    {
        var deletes = new List<SnapshotDeleteApplier.DeletedRow>();
        var entity = table.Table.TableName switch
        {
            "STOKLAR" => "product",
            "CARI_HESAPLAR" => "customer",
            "CARI_HESAP_HAREKETLERI" => "invoice", // shared source; collection consumers also receive it
            "STOK_HAREKETLERI" => "invoice",
            "ODEME_EMIRLERI" => "collection",
            _ => null,
        };
        if (entity is null) return deletes;
        var seen = new HashSet<string>(StringComparer.Ordinal);

        // Faz 20: upsert rows carry an explicit RecordKey, so the queue item's
        // identity no longer depends on guessing a "*RECno" column — which
        // GUID-keyed tables (Mikro V16, Logo) do not have.
        var upsertSequence = Math.Max(table.New?.HighestSequence ?? 0, table.Changed?.HighestSequence ?? 0);
        // A row created and deleted inside the same bundle is only ever
        // described here, so this bundle's own upserts are the first place to
        // look for its business code.
        var codesInBundle = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var row in (table.New?.Rows ?? Array.Empty<SyncUpsertRow>())
                     .Concat(table.Changed?.Rows ?? Array.Empty<SyncUpsertRow>()))
        {
            var code = ReadBusinessKey(row.Columns, table.Table.TableName);
            if (code is not null && !string.IsNullOrWhiteSpace(row.RecordKey))
                codesInBundle[row.RecordKey] = code;
        }

        foreach (var row in table.New?.Rows ?? Array.Empty<SyncUpsertRow>())
            AddUpsertRow(db, tenantId, sourceDatabase, table, entity, row, upsertSequence, seen);
        foreach (var row in table.Changed?.Rows ?? Array.Empty<SyncUpsertRow>())
            AddUpsertRow(db, tenantId, sourceDatabase, table, entity, row, upsertSequence, seen);

        foreach (var row in table.Deleted?.Rows ?? Array.Empty<SyncDeletedRow>())
        {
            var sourceRecordKey = row.RecordKey;
            // The stored queue rows are the durable half of the translation:
            // every upsert this tenant ever pushed kept the row's columns, and
            // the business code is in there. Reading `RecordKey` back instead —
            // as this used to — returned the RECno unchanged, so the mobile
            // delete never matched anything.
            var businessKey = codesInBundle.GetValueOrDefault(sourceRecordKey);
            if (businessKey is null)
            {
                var storedPayload = await db.MobileSyncQueue.AsNoTracking()
                    .Where(x => x.TenantId == tenantId && x.SourceDatabase == sourceDatabase &&
                                x.TableName == table.Table.TableName &&
                                x.SourceRecordKey == sourceRecordKey && x.Operation == "upsert")
                    .OrderByDescending(x => x.Sequence)
                    .Select(x => x.PayloadJson)
                    .FirstOrDefaultAsync(ct);
                businessKey = ReadBusinessKeyFromJson(storedPayload, table.Table.TableName);
            }
            if (businessKey is null
                && MobileRecordKey.DeleteTargetFor(table.Table.TableName) is { BusinessKeyColumn: not null } target
                && MobileRecordKey.NormalizeSourceKey(sourceRecordKey) is { } normalizedKey)
            {
                // Most of a catalogue never changes after the trigger install,
                // so its cards have no queue upsert to read the code back from.
                // The bootstrap row carries the ERP identity; without this the
                // delete stayed keyed by RECno and matched nothing anywhere.
                businessKey = await db.MobileRecords.AsNoTracking()
                    .Where(x => x.TenantId == tenantId && x.Entity == target.Entity && x.SourceRecordKey == normalizedKey)
                    .Select(x => x.RecordKey)
                    .FirstOrDefaultAsync(ct);
            }

            var recordKey = businessKey ?? sourceRecordKey;
            if (!seen.Add($"{recordKey}:delete:{row.Sequence}")) continue;
            deletes.Add(new SnapshotDeleteApplier.DeletedRow(table.Table.TableName, sourceRecordKey, businessKey));
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

        return deletes;
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
