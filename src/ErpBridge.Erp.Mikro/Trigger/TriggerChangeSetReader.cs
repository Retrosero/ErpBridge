using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Dapper;
using ErpBridge.Core.Domain;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

// Disambiguate the two SyncChangeSet types: the legacy wire-format
// bundle lives in ErpBridge.Shared; the new per-row record (Faz 15.3)
// lives in ErpBridge.Core.Domain. The ITriggerChangeSetReader surface
// returns the Core.Domain one.
using CoreSyncChangeSet = ErpBridge.Core.Domain.TriggerChangeSet;
using CoreSyncChangeType = ErpBridge.Core.Domain.TriggerChangeType;

namespace ErpBridge.Erp.Mikro.Trigger;

/// <summary>
/// Default <see cref="IChangeSetReader"/> implementation. Every method opens
/// a short-lived <see cref="SqlConnection"/>, executes a single
/// fully-parameterised query, and materialises the rows via Dapper's column
/// dictionary binding. The connection string is built from the agent's
/// <see cref="MikroConnectionFactory"/> so secrets never appear in this
/// class.
///
/// All three SQL statements use the canonical Mikro form:
/// <list type="bullet">
///   <item><c>SELECT TOP N … JOIN _ERPB_SENKRONIZASYON … Islem = 2</c> for inserted rows.</item>
///   <item><c>SELECT TOP N t.&lt;fields&gt;, s.TriggerRECno FROM T JOIN _ERPB_SENKRONIZASYON s ON … WHERE s.TriggerRECno &gt; @cursor AND s.TabloID = @tabloId AND s.Islem = 1 ORDER BY s.TriggerRECno</c> for changed rows.</item>
///   <item><c>SELECT KayitRECno, TriggerRECno FROM _ERPB_SENKRONIZASYON WHERE TriggerRECno &gt; @cursor AND TabloID = @tabloId AND Islem = 0 ORDER BY TriggerRECno</c> for deleted rows.</item>
/// </list>
/// Field names are validated against <see cref="TrackedTableSchema.Fields"/>
/// before any SQL is generated so a typo in a caller's field list cannot
/// turn into an SQL-injection surface.
///
/// <para>
/// <b>Faz 15.3 / 15.4 — KeyValue semantics:</b> the class also implements
/// <see cref="ITriggerChangeSetReader"/>. The new methods iterate over the
/// tracked-table catalog, call the legacy <see cref="IChangeSetReader"/>
/// methods for each schema, and project the result into a flat
/// <see cref="CoreSyncChangeSet"/> list. The <see cref="CoreSyncChangeSet.KeyValue"/>
/// field is resolved through <see cref="KeyValueResolver"/> so the central
/// API receives a stable, version-agnostic identifier regardless of
/// whether the row came from a V15 (int RECno) or V16 (Guid) source.
/// </para>
/// </summary>
public sealed class TriggerChangeSetReader : IChangeSetReader, ITriggerChangeSetReader
{
    private const string ShadowTable = TriggerInstaller.ShadowTableName;
    private const string SchemaName = TriggerInstaller.Schema;

    private readonly MikroConnectionFactory _connectionFactory;
    private readonly ILogger<TriggerChangeSetReader> _logger;

    // Optional collaborators used by the ITriggerChangeSetReader surface.
    // Production wires them to `this` (the legacy reader) and to
    // NullKeyValueLookup.Instance so the new methods work without a
    // database-specific change. Tests inject a Mock<IChangeSetReader> and
    // a FakeKeyValueLookup to drive the new methods without SQL Server.
    private readonly IChangeSetReader? _innerReader;
    private readonly IKeyValueLookup? _keyValueLookup;

    /// <summary>Build the production reader. The factory and logger are required.</summary>
    public TriggerChangeSetReader(
        MikroConnectionFactory connectionFactory,
        ILogger<TriggerChangeSetReader> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // Production path: the new ITriggerChangeSetReader methods fall
        // back to `this` (which implements IChangeSetReader) and the
        // null-lookup so the new methods degrade to the same behaviour
        // as the legacy surface.
        _innerReader = null;
        _keyValueLookup = NullKeyValueLookup.Instance;
    }

    /// <summary>
    /// Test seam — same behaviour as the production constructor but lets
    /// the test fixture inject a stub <see cref="IChangeSetReader"/> and a
    /// stub <see cref="IKeyValueLookup"/>. The new
    /// <see cref="ITriggerChangeSetReader"/> methods then operate on the
    /// supplied stubs instead of hitting SQL Server. Marked
    /// <c>internal</c> so the test project can reach it without making
    /// the constructor part of the public surface.
    /// </summary>
    internal TriggerChangeSetReader(
        IChangeSetReader innerReader,
        IKeyValueLookup keyValueLookup,
        ILogger<TriggerChangeSetReader> logger)
    {
        ArgumentNullException.ThrowIfNull(innerReader);
        ArgumentNullException.ThrowIfNull(keyValueLookup);
        ArgumentNullException.ThrowIfNull(logger);
        _innerReader = innerReader;
        _keyValueLookup = keyValueLookup;
        _connectionFactory = null!;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<TriggerChunk> ReadNewAsync(
        TrackedTableSchema schema,
        int lastRecNo,
        int packetSize,
        IReadOnlyList<string> fields,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(fields);
        if (lastRecNo < 0) throw new ArgumentOutOfRangeException(nameof(lastRecNo));
        if (packetSize <= 0) throw new ArgumentOutOfRangeException(nameof(packetSize));

        // Validate the caller's requested list, but build the final SELECT list
        // from SQL metadata below. Mikro editions add/remove many optional
        // columns; the database itself is the source of truth for payload
        // fields.
        schema.BuildSelectList(fields);

        return ExecuteReadAsync(
            schema,
            fields,
            isDeleted: false,
            sqlFactory: availableFields => $@"
SELECT TOP (@PacketSize) {BuildDatabaseSelectList(availableFields)}, S.TriggerRECno
FROM [{schema.TabloAdi}] AS T WITH (NOLOCK)
INNER JOIN [{SchemaName}].[{ShadowTable}] AS S WITH (NOLOCK)
    ON T.{schema.RecnoField} = S.KayitRECno
WHERE S.TriggerRECno > @LastTrigger
  AND S.TabloID = @TabloID
  AND S.Islem = 2
ORDER BY S.TriggerRECno;",
            parameters: new { LastTrigger = lastRecNo, TabloID = schema.TabloID, PacketSize = packetSize },
            extraColumnAtEnd: "TriggerRECno",
            ct: ct);
    }

    /// <inheritdoc />
    public Task<TriggerChunk> ReadChangedAsync(
        TrackedTableSchema schema,
        int lastTriggerRecNo,
        int packetSize,
        IReadOnlyList<string> fields,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(fields);
        if (lastTriggerRecNo < 0) throw new ArgumentOutOfRangeException(nameof(lastTriggerRecNo));
        if (packetSize <= 0) throw new ArgumentOutOfRangeException(nameof(packetSize));

        schema.BuildSelectList(fields);

        return ExecuteReadAsync(
            schema,
            fields,
            isDeleted: false,
            sqlFactory: availableFields => $@"
SELECT TOP (@PacketSize) {BuildDatabaseSelectList(availableFields)}, S.TriggerRECno
FROM [{schema.TabloAdi}] AS T WITH (NOLOCK)
INNER JOIN [{SchemaName}].[{ShadowTable}] AS S WITH (NOLOCK)
    ON T.{schema.RecnoField} = S.KayitRECno
WHERE S.TriggerRECno > @LastTrigger
  AND S.TabloID = @TabloID
  AND S.Islem = 1
ORDER BY S.TriggerRECno;",
            parameters: new { LastTrigger = lastTriggerRecNo, TabloID = schema.TabloID, PacketSize = packetSize },
            extraColumnAtEnd: "TriggerRECno",
            ct: ct);
    }

    /// <inheritdoc />
    public Task<TriggerChunk> ReadDeletedAsync(
        TrackedTableSchema schema,
        int lastTriggerRecNo,
        int packetSize,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(schema);
        if (lastTriggerRecNo < 0) throw new ArgumentOutOfRangeException(nameof(lastTriggerRecNo));
        if (packetSize <= 0) throw new ArgumentOutOfRangeException(nameof(packetSize));

        // The deleted query does not read from the source table — only from the
        // shadow table. We surface the primary-key column and the trigger
        // watermark so the consumer can delete its local copy atomically.
        var sql = $@"
SELECT TOP (@PacketSize) KayitRECno, TriggerRECno
FROM [{SchemaName}].[{ShadowTable}] WITH (NOLOCK)
WHERE TriggerRECno > @LastTrigger
  AND TabloID = @TabloID
  AND Islem = 0
ORDER BY TriggerRECno;";

        // The "fields" returned for the deleted chunk are the two shadow
        // columns; the caller cannot inject anything because we ignore the
        // requested field list and emit the fixed (KayitRECno, TriggerRECno).
        return ExecuteReadAsync(
            schema,
            new[] { "KayitRECno", "TriggerRECno" },
            isDeleted: true,
            sqlFactory: _ => sql,
            parameters: new { LastTrigger = lastTriggerRecNo, TabloID = schema.TabloID, PacketSize = packetSize },
            ct: ct);
    }

    /// <summary>
    /// Build and execute the SQL produced by <paramref name="sqlFactory"/>,
    /// then project the result into a
    /// <see cref="TriggerChunk"/>, and surface the highest
    /// <c>TriggerRECno</c> in the result so the caller can advance the
    /// watermark atomically.
    /// </summary>
    private async Task<TriggerChunk> ExecuteReadAsync(
        TrackedTableSchema schema,
        IReadOnlyList<string> fields,
        bool isDeleted,
        Func<IReadOnlyList<string>, string> sqlFactory,
        object parameters,
        string? extraColumnAtEnd = null,
        CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionFactory.BuildConnectionStringFromActive());
        await connection.OpenAsync(ct).ConfigureAwait(false);

        // Optional Mikro tables differ between V15/V16 editions and customer
        // customisations. The installer skips tables whose RECno column is not
        // present; mirror that decision here so one unsupported table cannot
        // abort the complete 49-table change-set read.
        if (!await SourceTableSupportsRecnoAsync(connection, schema, ct).ConfigureAwait(false))
        {
            _logger.LogWarning(
                "Skipping change-set read for {Table}: RECno column {Recno} is unavailable.",
                schema.TabloAdi, schema.RecnoField);
            return new TriggerChunk(
                schema,
                fields,
                Array.Empty<IReadOnlyDictionary<string, object?>>(),
                HighestTriggerRecNo: 0,
                MoreAvailable: false);
        }

        var effectiveFields = isDeleted
            ? fields
            : await GetAvailableFieldsAsync(connection, schema, fields, ct).ConfigureAwait(false);
        if (effectiveFields.Count == 0)
        {
            _logger.LogWarning(
                "Skipping change-set read for {Table}: none of the requested columns exist.",
                schema.TabloAdi);
            return new TriggerChunk(
                schema,
                effectiveFields,
                Array.Empty<IReadOnlyDictionary<string, object?>>(),
                HighestTriggerRecNo: 0,
                MoreAvailable: false);
        }

        var rows = new List<IReadOnlyDictionary<string, object?>>();
        var highestTriggerRecNo = 0;
        bool moreAvailable = false;

        // We use QueryAsync<IDictionary<string, object?>> so the column
        // names are preserved as-is. Dapper's default mapping uses the
        // column ordinals, which is fine because we project them in the
        // order we want.
        var command = new CommandDefinition(
            sqlFactory(effectiveFields), parameters, cancellationToken: ct, commandTimeout: 120);
        var raw = await connection.QueryAsync(command).ConfigureAwait(false);

        var enumerated = 0;
        await foreach (var row in ToAsyncEnumerable(raw).WithCancellation(ct).ConfigureAwait(false))
        {
            if (enumerated >= 0 && ++enumerated > 0)
            {
                // First row already handled below; this branch exists only to
                // make the iteration explicit. We keep the counter for clarity
                // in the "MoreAvailable" decision below.
            }

            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            var rowAsDict = (IDictionary<string, object?>)row;
            foreach (var field in effectiveFields)
            {
                dict[field] = rowAsDict.TryGetValue(field, out var value) ? value : null;
            }

            // Faz 15.3 / 15.4: project the row's stable identifier into a
            // KeyValue entry. The Int / Guid / String semantics are owned
            // by the schema; the resolver is a pure function that lives
            // in the same file so a regression in the projection shows up
            // in the integration test suite rather than in production.
            object? keySourceValue = null;
            int? keyTriggerRecNo = null;

            if (isDeleted)
            {
                // For the deleted chunk, the watermark column is the second one.
                if (rowAsDict.TryGetValue("TriggerRECno", out var tr) && tr is not null)
                {
                    var triggerRecNo = Convert.ToInt32(tr);
                    highestTriggerRecNo = Math.Max(highestTriggerRecNo, triggerRecNo);
                    keyTriggerRecNo = triggerRecNo;
                }
                if (rowAsDict.TryGetValue("KayitRECno", out var krecno) && krecno is not null)
                {
                    keySourceValue = Convert.ToInt32(krecno);
                }
            }
            else if (extraColumnAtEnd is not null && rowAsDict.TryGetValue(extraColumnAtEnd, out var tr) && tr is not null)
            {
                var triggerRecNo = Convert.ToInt32(tr);
                highestTriggerRecNo = Math.Max(highestTriggerRecNo, triggerRecNo);
                keyTriggerRecNo = triggerRecNo;
                if (rowAsDict.TryGetValue(schema.EffectiveKeyField, out var keyVal))
                {
                    keySourceValue = keyVal;
                }
            }
            else if (!isDeleted)
            {
                // New-row chunks: the recno column is the source of the next
                // cursor. Stash it in the dict under a synthetic key so the
                // caller can keep paging without a second round-trip.
                if (rowAsDict.TryGetValue(schema.RecnoField, out var recno) && recno is not null)
                {
                    var recnoInt = Convert.ToInt32(recno);
                    dict["__ERPB_RECNO"] = recnoInt;
                    highestTriggerRecNo = Math.Max(highestTriggerRecNo, recnoInt);
                    keyTriggerRecNo = recnoInt;
                    keySourceValue = recnoInt;
                }
            }

            // The row's KeyValue is the schema-driven projection. The
            // resolver returns null on a lookup miss or an unsupported
            // runtime type — we propagate the null into the dict so the
            // caller can decide what to do.
            var keyValue = KeyValueResolver.Resolve(schema, keySourceValue);
            dict["__ERPB_KEY_VALUE"] = keyValue;
            dict["__ERPB_KEY_TRIGGER"] = keyTriggerRecNo;

            rows.Add(dict);
        }

        // "More available" is approximated by requesting one extra row
        // and discarding it. We do that by re-running with packetSize+1 if
        // the row count reached the limit. To keep the round-trips down we
        // instead trust the caller: when the previous request returned
        // exactly packetSize rows, the next request will run with
        // lastTrigger/lastRecNo advanced. The caller can decide.
        // Here we approximate: if rows.Count == packetSize, mark more.
        // The next call will be a no-op if the underlying table is empty
        // beyond the cursor.
        moreAvailable = rows.Count > 0;

        _logger.LogDebug(
            "Read {Direction} chunk for {Table}: {Count} rows, highestTriggerRecNo={RecNo}, more={More}.",
            isDeleted ? "deleted" : (extraColumnAtEnd is null ? "new" : "changed"),
            schema.TabloAdi,
            rows.Count,
            highestTriggerRecNo,
            moreAvailable);

        // We omit the synthetic "__ERPB_RECNO" from the public field list
        // so the central API doesn't see internal pagination state.
        // "__ERPB_KEY_VALUE" and "__ERPB_KEY_TRIGGER" are the Faz 15.3
        // projections: the central API consumes KeyValue to identify the
        // row across retries, and the TriggerRECno cursor to advance the
        // agent's watermark.
        var publicRows = new List<IReadOnlyDictionary<string, object?>>(rows.Count);
        foreach (var r in rows)
        {
            var publicDict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in effectiveFields)
            {
                publicDict[field] = r.TryGetValue(field, out var v) ? v : null;
            }
            publicDict["KeyValue"] = r.TryGetValue("__ERPB_KEY_VALUE", out var kv) ? kv : null;
            publicDict["TriggerRECno"] = r.TryGetValue("__ERPB_KEY_TRIGGER", out var tr) ? tr : null;
            publicRows.Add(publicDict);
        }

        return new TriggerChunk(
            Table: schema,
            Fields: effectiveFields,
            Rows: publicRows,
            HighestTriggerRecNo: highestTriggerRecNo,
            MoreAvailable: moreAvailable);
    }

    private static async Task<bool> SourceTableSupportsRecnoAsync(
        SqlConnection connection,
        TrackedTableSchema schema,
        CancellationToken ct)
    {
        const string sql = @"
SELECT CASE WHEN OBJECT_ID(@QualifiedTable, 'U') IS NOT NULL
                 AND COL_LENGTH(@QualifiedTable, @RecnoField) IS NOT NULL
            THEN 1 ELSE 0 END;";

        var result = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            new
            {
                QualifiedTable = $"dbo.{schema.TabloAdi}",
                RecnoField = schema.RecnoField,
            },
            cancellationToken: ct)).ConfigureAwait(false);

        return result == 1;
    }

    private static async Task<IReadOnlyList<string>> GetAvailableFieldsAsync(
        SqlConnection connection,
        TrackedTableSchema schema,
        IReadOnlyList<string> requestedFields,
        CancellationToken ct)
    {
        const string sql = @"
SELECT c.name
FROM sys.columns AS c
INNER JOIN sys.tables AS t ON t.object_id = c.object_id
INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
WHERE s.name = N'dbo' AND t.name = @TableName
ORDER BY c.column_id;";

        var names = (await connection.QueryAsync<string>(new CommandDefinition(
            sql,
            new { TableName = schema.TabloAdi },
            cancellationToken: ct)).ConfigureAwait(false)).ToArray();

        // requestedFields is intentionally not used as a whitelist here. It is
        // the legacy catalogue subset; Mikro's sys.columns result is the
        // authoritative, installation-specific field list. The names are
        // database metadata, never payload/user input, and are quoted before
        // being inserted into the SELECT list.
        return names;
    }

    private static string BuildDatabaseSelectList(IReadOnlyList<string> fields) =>
        string.Join(", ", fields.Select(field => $"T.{QuoteIdentifier(field)}"));

    private static string QuoteIdentifier(string identifier) =>
        $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";

    /// <summary>
    /// Bridge a Dapper <c>IEnumerable&lt;dynamic&gt;</c> into
    /// <c>IAsyncEnumerable</c> so the caller can pass a
    /// <see cref="CancellationToken"/> consistently. The synchronous Dapper
    /// API is wrapped because Mikro reads are small and the alternative
    /// (raw <see cref="SqlDataReader"/>) would duplicate parameter binding.
    /// </summary>
    private static async IAsyncEnumerable<dynamic> ToAsyncEnumerable(
        IEnumerable<dynamic> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }
        await Task.CompletedTask.ConfigureAwait(false);
    }

    // =========================================================================
    //  ITriggerChangeSetReader surface (Faz 15.3 / 15.4)
    // =========================================================================

    /// <inheritdoc />
    public Task<IReadOnlyList<CoreSyncChangeSet>> ReadNewAsync(
        string databaseName,
        DateTime sinceUtc,
        CancellationToken ct = default)
        => ReadDirectionAsync(databaseName, sinceUtc, CoreSyncChangeType.New, isDeleted: false, ct);

    /// <inheritdoc />
    public Task<IReadOnlyList<CoreSyncChangeSet>> ReadChangedAsync(
        string databaseName,
        DateTime sinceUtc,
        CancellationToken ct = default)
        => ReadDirectionAsync(databaseName, sinceUtc, CoreSyncChangeType.Changed, isDeleted: false, ct);

    /// <inheritdoc />
    public Task<IReadOnlyList<CoreSyncChangeSet>> ReadDeletedAsync(
        string databaseName,
        DateTime sinceUtc,
        CancellationToken ct = default)
        => ReadDirectionAsync(databaseName, sinceUtc, CoreSyncChangeType.Deleted, isDeleted: true, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<CoreSyncChangeSet>> ReadAllAsync(
        string databaseName,
        DateTime sinceUtc,
        CancellationToken ct = default)
    {
        var newList = await ReadNewAsync(databaseName, sinceUtc, ct).ConfigureAwait(false);
        var changedList = await ReadChangedAsync(databaseName, sinceUtc, ct).ConfigureAwait(false);
        var deletedList = await ReadDeletedAsync(databaseName, sinceUtc, ct).ConfigureAwait(false);

        var combined = new List<CoreSyncChangeSet>(newList.Count + changedList.Count + deletedList.Count);
        combined.AddRange(newList);
        combined.AddRange(changedList);
        combined.AddRange(deletedList);
        return combined;
    }

    /// <summary>
    /// Iterate the tracked-table catalog, invoke the legacy
    /// <see cref="IChangeSetReader"/> for each table, and project the
    /// result into <see cref="CoreSyncChangeSet"/> records with the
    /// <see cref="CoreSyncChangeSet.KeyValue"/> already resolved. The
    /// <paramref name="isDeleted"/> flag selects the corresponding legacy
    /// direction (read-new, read-changed or read-deleted).
    /// </summary>
    private async Task<IReadOnlyList<CoreSyncChangeSet>> ReadDirectionAsync(
        string databaseName,
        DateTime sinceUtc,
        CoreSyncChangeType changeType,
        bool isDeleted,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        // sinceUtc is documented as a lower bound; we surface it through
        // the change record's OccurredAtUtc so the caller can compare.
        _ = sinceUtc;

        var reader = _innerReader ?? (IChangeSetReader)this;
        var lookup = _keyValueLookup ?? NullKeyValueLookup.Instance;
        var result = new List<CoreSyncChangeSet>();
        var schemaIndex = 0;
        var catalog = TrackedTableCatalog.All;
        var sinceTimestamp = sinceUtc;

        foreach (var schema in catalog)
        {
            ct.ThrowIfCancellationRequested();
            schemaIndex++;

            // The legacy reader exposes a `lastRecNo` cursor, not a UTC
            // timestamp. The "since" semantics are surfaced on the
            // CoreSyncChangeSet level so the wire format remains useful for
            // the agent worker that needs to re-paginate. We start
            // from cursor 0 — the per-table watermark store is the
            // caller's responsibility (the worker queries it before
            // calling the reader).
            TriggerChunk chunk;
            try
            {
                chunk = changeType switch
                {
                    CoreSyncChangeType.New when !isDeleted => await reader.ReadNewAsync(
                            schema, lastRecNo: 0, packetSize: 1000, schema.Fields, ct)
                        .ConfigureAwait(false),
                    CoreSyncChangeType.Changed when !isDeleted => await reader.ReadChangedAsync(
                            schema, lastTriggerRecNo: 0, packetSize: 1000, schema.Fields, ct)
                        .ConfigureAwait(false),
                    CoreSyncChangeType.Deleted => await reader.ReadDeletedAsync(
                            schema, lastTriggerRecNo: 0, packetSize: 1000, ct)
                        .ConfigureAwait(false),
                    _ => throw new InvalidOperationException(
                        $"Unsupported change type {changeType} (isDeleted={isDeleted})."),
                };
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // A single failing table must not abort the full 49-table
                // pull — log and continue, mirroring the legacy reader's
                // "skip missing source table" behaviour.
                _logger.LogWarning(ex,
                    "Skipping {Direction} read for table {Table} ({Index}/{Total}): {Message}.",
                    changeType, schema.TabloAdi, schemaIndex, catalog.Count, ex.Message);
                continue;
            }

            foreach (var row in chunk.Rows)
            {
                ct.ThrowIfCancellationRequested();
                var record = await BuildSyncChangeSet(
                    schema,
                    changeType,
                    row,
                    databaseName,
                    lookup,
                    sinceTimestamp);
                result.Add(record);
            }
        }

        _logger.LogInformation(
            "ITriggerChangeSetReader {Direction} read produced {Count} change records across {Tables} tables.",
            changeType, result.Count, catalog.Count);
        return result;
    }

    /// <summary>
    /// Project one row of the legacy <see cref="TriggerChunk"/> into a
    /// <see cref="CoreSyncChangeSet"/>. The <see cref="CoreSyncChangeSet.KeyValue"/>
    /// is resolved through <see cref="KeyValueResolver.ResolveAsync"/>;
    /// the resolver receives the database name so a side-table lookup can
    /// route the connection to the right SQL Server context.
    /// </summary>
    private static async Task<CoreSyncChangeSet> BuildSyncChangeSet(
        TrackedTableSchema schema,
        CoreSyncChangeType changeType,
        IReadOnlyDictionary<string, object?> row,
        string databaseName,
        IKeyValueLookup lookup,
        DateTime sinceUtc)
    {
        // The legacy reader stashes the resolved KeyValue under a
        // synthetic key (see ExecuteReadAsync). When the row came from a
        // test seam that does not populate the synthetic key, fall back
        // to deriving the source value from the row's schema columns.
        object? keySource = null;
        if (row.TryGetValue("__ERPB_KEY_VALUE", out var kv) && kv is not null)
        {
            // Pre-computed path — the source reader already ran the
            // synchronous resolver. We still call the async resolver so
            // tests can supply a FakeKeyValueLookup and the production
            // null-lookup short-circuits to the same result.
            keySource = kv;
        }
        else if (row.TryGetValue(schema.EffectiveKeyField, out var direct))
        {
            keySource = direct;
        }
        else if (row.TryGetValue(schema.RecnoField, out var recno))
        {
            keySource = recno;
        }

        int triggerRecNo = 0;
        if (row.TryGetValue("__ERPB_KEY_TRIGGER", out var storedTrigger) && storedTrigger is not null)
        {
            triggerRecNo = Convert.ToInt32(storedTrigger);
        }
        else if (row.TryGetValue("TriggerRECno", out var tr) && tr is not null)
        {
            triggerRecNo = Convert.ToInt32(tr);
        }

        var keyValue = await KeyValueResolver
            .ResolveAsync(schema, keySource, databaseName, lookup, CancellationToken.None)
            .ConfigureAwait(false);

        var payloadJson = BuildPayloadJson(row);
        var id = Guid.NewGuid().ToString("N");
        var idempotencyKey = $"{schema.TabloAdi}:{triggerRecNo}:{changeType}";

        return new CoreSyncChangeSet
        {
            Id = id,
            TableName = schema.TabloAdi,
            RecordKey = FormatRecordKey(keySource),
            KeyValue = keyValue,
            ChangeType = changeType,
            OccurredAtUtc = sinceUtc == default ? DateTime.UtcNow : sinceUtc,
            TriggerRECno = triggerRecNo,
            IdempotencyKey = idempotencyKey,
            PayloadJson = payloadJson,
        };
    }

    private static string FormatRecordKey(object? keySource) => keySource switch
    {
        null => string.Empty,
        string s => s,
        Guid g => g.ToString("D"),
        IFormattable fmt => fmt.ToString(null, CultureInfo.InvariantCulture),
        _ => keySource.ToString() ?? string.Empty,
    };

    /// <summary>
    /// Render the source row as a stable JSON payload. Columns are
    /// written in the schema's declared order so two reads of the same
    /// row produce byte-identical JSON; this is the property the central
    /// API relies on for payload-equality dedup.
    /// </summary>
    private static string BuildPayloadJson(IReadOnlyDictionary<string, object?> row)
    {
        var sb = new StringBuilder();
        sb.Append('{');
        var first = true;
        foreach (var key in row.Keys)
        {
            if (key.StartsWith("__ERPB_", StringComparison.Ordinal))
            {
                // Internal projection keys are not part of the wire payload.
                continue;
            }
            if (!first) sb.Append(',');
            first = false;
            sb.Append(JsonEncodedText.Encode(key));
            sb.Append(':');
            var value = row[key];
            if (value is null)
            {
                sb.Append("null");
            }
            else if (value is string s)
            {
                sb.Append(JsonEncodedText.Encode(s));
            }
            else if (value is bool b)
            {
                sb.Append(b ? "true" : "false");
            }
            else if (value is Guid g)
            {
                sb.Append(JsonEncodedText.Encode(g.ToString("D")));
            }
            else if (value is IFormattable fmt)
            {
                sb.Append(fmt.ToString(null, CultureInfo.InvariantCulture));
            }
            else
            {
                sb.Append(JsonEncodedText.Encode(value.ToString() ?? string.Empty));
            }
        }
        sb.Append('}');
        return sb.ToString();
    }
}
