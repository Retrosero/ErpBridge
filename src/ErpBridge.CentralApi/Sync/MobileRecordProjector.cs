using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace ErpBridge.CentralApi.Sync;

/// <summary>
/// Maintains <see cref="MobileRecord"/> — the table mobile devices page through.
///
/// <para>Every write goes through here so the cursor keeps its one guarantee:
/// sequence order equals commit order. See <see cref="TenantSyncCounter"/> for
/// why an identity column cannot provide that.</para>
/// </summary>
public sealed class MobileRecordProjector
{
    /// <summary>
    /// Largest share of an entity a single full upload is allowed to tombstone.
    ///
    /// <para>Mark-and-sweep reads "absent from a full upload" as "deleted in the
    /// ERP", which is right when the upload is complete and catastrophic when it
    /// is not: a section that failed halfway would tombstone the customer list on
    /// every device. Wiping most of an entity at once is far more likely to be a
    /// truncated upload than a real mass deletion, so the sweep stands down and
    /// says so rather than propagating it.</para>
    /// </summary>
    public const double MaxSweepFraction = 0.5;

    /// <summary>Keys per round trip when loading the rows an upload touches.</summary>
    private const int KeyBatchSize = 500;

    private readonly ILogger<MobileRecordProjector> _logger;

    /// <summary>DI constructor.</summary>
    public MobileRecordProjector(ILogger<MobileRecordProjector> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>One section of an upload, already parsed.</summary>
    /// <param name="Section">Snapshot section name, which is also the entity name.</param>
    /// <param name="Rows">Mobile-shaped rows as they will be served to devices.</param>
    public readonly record struct SectionRows(string Section, IReadOnlyList<JsonElement> Rows);

    /// <summary>A row the ERP no longer has.</summary>
    /// <param name="TableName">ERP table the change log named, e.g. <c>STOKLAR</c>.</param>
    /// <param name="RecordKey">Business code when it could be resolved, else the physical record number.</param>
    public readonly record struct DeletedRecord(string TableName, string RecordKey);

    /// <summary>What a projection did, for logging and tests.</summary>
    /// <param name="Changed">Rows whose payload actually differed and so moved the cursor.</param>
    /// <param name="Unchanged">Rows re-sent byte-identical, which deliberately did not move it.</param>
    /// <param name="Tombstoned">Rows marked deleted.</param>
    public readonly record struct ProjectionResult(int Changed, int Unchanged, int Tombstoned);

    /// <summary>
    /// Applies an upload to the tenant's mobile records.
    ///
    /// <para>Must run inside the caller's transaction: the sequence block is
    /// reserved by locking the tenant's counter row, and that lock is only
    /// released when the caller commits.</para>
    /// </summary>
    /// <param name="db">Context whose transaction this joins.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="sections">Sections carried by this upload.</param>
    /// <param name="fullUpload">
    /// True for a non-incremental upload, which is a statement about the whole of
    /// each section it carries: rows it does not mention no longer exist and are
    /// tombstoned. An incremental upload says nothing about what it omits.
    /// </param>
    /// <param name="ct">Cancellation.</param>
    public async Task<ProjectionResult> ProjectAsync(
        CentralApiDbContext db,
        Guid tenantId,
        IReadOnlyCollection<SectionRows> sections,
        bool fullUpload,
        string? sourceDatabase,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(sections);
        if (sections.Count == 0) return default;
        if (!IsSupported(db)) return default;

        var runId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Work out everything that changes before touching the counter. The
        // counter lock blocks every other writer for this tenant, so it is taken
        // as late as possible and held over as little work as possible.
        var pending = new List<PendingWrite>();
        var unchanged = 0;

        foreach (var section in sections)
        {
            if (!MobileRecordKey.Entities.Contains(section.Section, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Section {Section} has no mobile entity; skipping projection.", section.Section);
                continue;
            }

            var incoming = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in section.Rows)
            {
                var key = MobileRecordKey.For(section.Section, row);
                // A row with no stable identity cannot be addressed by a device,
                // so it could never be updated or deleted later either. Skipping
                // it beats inventing a key no other writer would agree on.
                if (key is null) continue;
                incoming[key] = row;
            }
            if (incoming.Count == 0 && !fullUpload) continue;

            var existing = await LoadAsync(db, tenantId, section.Section, incoming.Keys, fullUpload, ct)
                .ConfigureAwait(false);

            foreach (var pair in incoming)
            {
                var payload = pair.Value.GetRawText();
                var hash = Sha256(payload);
                existing.TryGetValue(pair.Key, out var current);

                if (current is not null && !current.IsDeleted && current.PayloadSha256 == hash)
                {
                    // Byte-identical re-send. Moving the cursor here would make
                    // every periodic upload re-push the whole catalogue to every
                    // device — the cost this hash exists to avoid.
                    unchanged++;
                    if (fullUpload) current.LastSeenRunId = runId;
                    continue;
                }

                var parents = MobileRecordKey.Parents(section.Section, pair.Value);
                pending.Add(new PendingWrite(
                    current, section.Section, pair.Key, payload, hash, parents.StockKey, parents.CustomerKey,
                    MobileRecordKey.SourceKey(section.Section, pair.Value), sourceDatabase));
            }

            if (fullUpload)
            {
                var swept = await CollectSweepAsync(db, tenantId, section.Section, incoming.Keys, runId, ct)
                    .ConfigureAwait(false);
                foreach (var row in swept) pending.Add(PendingWrite.Tombstone(row));
            }
        }

        if (pending.Count == 0) return new ProjectionResult(0, unchanged, 0);

        var block = await ReserveAsync(db, tenantId, pending.Count, ct).ConfigureAwait(false);
        var tombstoned = 0;

        foreach (var write in pending)
        {
            var record = write.Existing;
            if (record is null)
            {
                record = new MobileRecord
                {
                    TenantId = tenantId,
                    Entity = write.Entity,
                    RecordKey = write.RecordKey,
                };
                db.MobileRecords.Add(record);
            }

            record.PayloadJson = write.PayloadJson;
            record.PayloadSha256 = write.PayloadSha256;
            record.StockKey = write.StockKey;
            record.CustomerKey = write.CustomerKey;
            record.SourceRecordKey = write.SourceRecordKey;
            record.SourceDatabase = write.SourceDatabase;
            record.IsDeleted = write.IsTombstone;
            record.UpdatedAtUtc = now;
            record.UpdatedSeq = block++;
            record.LastSeenRunId = runId;
            if (write.IsTombstone) tombstoned++;
        }

        return new ProjectionResult(pending.Count - tombstoned, unchanged, tombstoned);
    }

    /// <summary>
    /// Turns ERP deletions into tombstones, cascading to the deleted record's
    /// children. Runs in the caller's transaction, like <see cref="ProjectAsync"/>.
    /// </summary>
    /// <param name="db">Context whose transaction this joins.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="deletes">Rows the change log reported as gone.</param>
    /// <param name="ct">Cancellation.</param>
    public async Task<int> ApplyDeletesAsync(
        CentralApiDbContext db,
        Guid tenantId,
        IReadOnlyCollection<DeletedRecord> deletes,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(deletes);
        if (deletes.Count == 0) return 0;
        if (!IsSupported(db)) return 0;

        var stockKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var customerKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var direct = new List<KeyValuePair<string, string>>();

        foreach (var row in deletes)
        {
            if (string.IsNullOrWhiteSpace(row.RecordKey)) continue;
            if (MobileRecordKey.DeleteTargetFor(row.TableName) is not { } target) continue;
            switch (target.Cascade)
            {
                case MobileRecordKey.Cascade.Stock:
                    stockKeys.Add(row.RecordKey);
                    break;
                case MobileRecordKey.Cascade.Customer:
                    customerKeys.Add(row.RecordKey);
                    break;
                default:
                    direct.Add(new KeyValuePair<string, string>(target.Entity, row.RecordKey));
                    break;
            }
        }

        var victims = new List<MobileRecord>();

        // The stock card's own row carries its StockKey, so one indexed lookup
        // reaches the card together with its barcodes, prices, inventory and
        // sales conditions.
        foreach (var batch in Batch(stockKeys))
        {
            victims.AddRange(await db.MobileRecords
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.StockKey != null && batch.Contains(x.StockKey))
                .ToListAsync(ct).ConfigureAwait(false));
        }

        foreach (var batch in Batch(customerKeys))
        {
            victims.AddRange(await db.MobileRecords
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.CustomerKey != null && batch.Contains(x.CustomerKey))
                .ToListAsync(ct).ConfigureAwait(false));
        }

        foreach (var group in direct.GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            var entity = group.Key;
            var keys = new HashSet<string>(group.Select(x => x.Value), StringComparer.Ordinal);
            foreach (var batch in Batch(keys))
            {
                victims.AddRange(await db.MobileRecords
                    .Where(x => x.TenantId == tenantId && x.Entity == entity && !x.IsDeleted && batch.Contains(x.RecordKey))
                    .ToListAsync(ct).ConfigureAwait(false));
            }
        }

        // A sales condition belongs to a stock card and a customer at once, so
        // deleting both in one bundle can reach the same row twice.
        var distinct = victims
            .GroupBy(x => x.Entity + MobileRecordKey.Separator + x.RecordKey, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
        if (distinct.Count == 0) return 0;

        var block = await ReserveAsync(db, tenantId, distinct.Count, ct).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        foreach (var record in distinct)
        {
            record.IsDeleted = true;
            record.PayloadJson = null;
            record.PayloadSha256 = null;
            record.UpdatedAtUtc = now;
            record.UpdatedSeq = block++;
        }

        return distinct.Count;
    }

    /// <summary>
    /// Reserves <paramref name="count"/> consecutive sequence numbers and returns
    /// the first of them.
    ///
    /// <para>The <c>UPDATE … RETURNING</c> holds the tenant's counter row until the
    /// caller commits, so no other writer for this tenant can reserve a later block
    /// and commit ahead of this one. That is the guarantee the whole cursor rests
    /// on: a reader can never step over a sequence that has not committed yet.</para>
    /// </summary>
    /// <param name="db">Context whose transaction this joins.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="count">How many sequence numbers to reserve.</param>
    /// <param name="ct">Cancellation.</param>
    public static async Task<long> ReserveAsync(
        CentralApiDbContext db, Guid tenantId, int count, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        // The row lock only orders anything for as long as a transaction holds
        // it. Reserving outside one returns the block immediately and releases
        // the lock with it, which silently restores the very reordering this
        // allocator exists to prevent — and the damage would only show up as a
        // device that quietly missed a row. Refusing is the one way a future
        // writer cannot get this wrong by omission.
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Mobile cursor positions must be reserved inside an open transaction, so the counter row " +
                "stays locked until the records that use them are committed. Call BeginTransactionAsync first.");
        }

        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct).ConfigureAwait(false);
        var transaction = db.Database.CurrentTransaction.GetDbTransaction();

        await using (var seed = connection.CreateCommand())
        {
            seed.Transaction = transaction;
            // Column names are quoted because the schema keeps them in PascalCase
            // while table names are snake_case; unquoted, PostgreSQL would fold
            // them to lowercase and not find them.
            seed.CommandText =
                "INSERT INTO tenant_sync_counter (\"TenantId\", \"LastSeq\", \"TombstoneHorizonSeq\") " +
                "VALUES (@tenant, 0, 0) ON CONFLICT (\"TenantId\") DO NOTHING";
            AddParameter(seed, "@tenant", tenantId);
            await seed.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        await using var reserve = connection.CreateCommand();
        reserve.Transaction = transaction;
        reserve.CommandText =
            "UPDATE tenant_sync_counter SET \"LastSeq\" = \"LastSeq\" + @count " +
            "WHERE \"TenantId\" = @tenant RETURNING \"LastSeq\"";
        AddParameter(reserve, "@tenant", tenantId);
        AddParameter(reserve, "@count", (long)count);
        var last = await reserve.ExecuteScalarAsync(ct).ConfigureAwait(false);
        var highest = Convert.ToInt64(last, System.Globalization.CultureInfo.InvariantCulture);
        return highest - count + 1;
    }

    /// <summary>
    /// Whether this context can carry the projection at all.
    ///
    /// <para>The cursor's ordering guarantee is a database row lock, so there is
    /// nothing to fall back to on a non-relational provider — it has neither a
    /// connection to issue the reservation on nor a transaction to hold it for.
    /// The in-memory provider backs a number of endpoint tests that care about
    /// other things entirely; they run with the projection switched off rather
    /// than against a weaker imitation of it. Everything about
    /// <see cref="MobileRecord"/> is covered relationally.</para>
    /// </summary>
    private bool IsSupported(CentralApiDbContext db)
    {
        if (db.Database.IsRelational()) return true;
        _logger.LogDebug("Mobile record projection skipped: the configured provider is not relational.");
        return false;
    }

    private async Task<List<MobileRecord>> CollectSweepAsync(
        CentralApiDbContext db,
        Guid tenantId,
        string entity,
        IReadOnlyCollection<string> present,
        Guid runId,
        CancellationToken ct)
    {
        var survivors = await db.MobileRecords
            .Where(x => x.TenantId == tenantId && x.Entity == entity && !x.IsDeleted)
            .CountAsync(ct).ConfigureAwait(false);
        if (survivors == 0) return [];

        var stale = await db.MobileRecords
            .Where(x => x.TenantId == tenantId && x.Entity == entity && !x.IsDeleted
                        && (x.LastSeenRunId == null || x.LastSeenRunId != runId))
            .ToListAsync(ct).ConfigureAwait(false);
        // Rows this upload re-sent unchanged were stamped above; what is left
        // really was absent from it.
        stale = stale
            .Where(x => !present.Contains(x.RecordKey, StringComparer.OrdinalIgnoreCase))
            .ToList();
        if (stale.Count == 0) return [];

        if (stale.Count > survivors * MaxSweepFraction)
        {
            _logger.LogError(
                "Refusing to tombstone {Stale} of {Total} {Entity} rows for tenant {TenantId}: a full upload " +
                "that drops more than {Limit:P0} of an entity is far more likely truncated than correct.",
                stale.Count, survivors, entity, tenantId, MaxSweepFraction);
            return [];
        }

        return stale;
    }

    private static async Task<Dictionary<string, MobileRecord>> LoadAsync(
        CentralApiDbContext db,
        Guid tenantId,
        string entity,
        IReadOnlyCollection<string> keys,
        bool wholeEntity,
        CancellationToken ct)
    {
        var found = new Dictionary<string, MobileRecord>(StringComparer.OrdinalIgnoreCase);

        if (wholeEntity)
        {
            // A full upload touches the whole entity anyway, so one scan beats
            // hundreds of key batches.
            foreach (var row in await db.MobileRecords
                         .Where(x => x.TenantId == tenantId && x.Entity == entity)
                         .ToListAsync(ct).ConfigureAwait(false))
            {
                found[row.RecordKey] = row;
            }
            return found;
        }

        foreach (var batch in Batch(keys))
        {
            foreach (var row in await db.MobileRecords
                         .Where(x => x.TenantId == tenantId && x.Entity == entity && batch.Contains(x.RecordKey))
                         .ToListAsync(ct).ConfigureAwait(false))
            {
                found[row.RecordKey] = row;
            }
        }

        return found;
    }

    private static IEnumerable<List<string>> Batch(IReadOnlyCollection<string> keys)
    {
        if (keys.Count == 0) yield break;
        var batch = new List<string>(Math.Min(KeyBatchSize, keys.Count));
        foreach (var key in keys)
        {
            batch.Add(key);
            if (batch.Count < KeyBatchSize) continue;
            yield return batch;
            batch = new List<string>(KeyBatchSize);
        }
        if (batch.Count > 0) yield return batch;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static string Sha256(string payload) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();

    private readonly record struct PendingWrite(
        MobileRecord? Existing,
        string Entity,
        string RecordKey,
        string? PayloadJson,
        string? PayloadSha256,
        string? StockKey,
        string? CustomerKey,
        string? SourceRecordKey,
        string? SourceDatabase,
        bool IsTombstone = false)
    {
        public static PendingWrite Tombstone(MobileRecord record) =>
            new(record, record.Entity, record.RecordKey, null, null, record.StockKey, record.CustomerKey,
                record.SourceRecordKey, record.SourceDatabase, true);
    }
}
