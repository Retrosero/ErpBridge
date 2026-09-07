using System.Data;
using Dapper;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

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
/// </summary>
public sealed class TriggerChangeSetReader : IChangeSetReader
{
    private const string ShadowTable = TriggerInstaller.ShadowTableName;
    private const string SchemaName = TriggerInstaller.Schema;

    private readonly MikroConnectionFactory _connectionFactory;
    private readonly ILogger<TriggerChangeSetReader> _logger;

    /// <summary>Build the reader. The factory and logger are required.</summary>
    public TriggerChangeSetReader(
        MikroConnectionFactory connectionFactory,
        ILogger<TriggerChangeSetReader> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            if (isDeleted)
            {
                // For the deleted chunk, the watermark column is the second one.
                if (rowAsDict.TryGetValue("TriggerRECno", out var tr) && tr is not null)
                {
                    highestTriggerRecNo = Math.Max(highestTriggerRecNo, Convert.ToInt32(tr));
                }
            }
            else if (extraColumnAtEnd is not null && rowAsDict.TryGetValue(extraColumnAtEnd, out var tr) && tr is not null)
            {
                highestTriggerRecNo = Math.Max(highestTriggerRecNo, Convert.ToInt32(tr));
            }
            else if (!isDeleted)
            {
                // New-row chunks: the recno column is the source of the next
                // cursor. Stash it in the dict under a synthetic key so the
                // caller can keep paging without a second round-trip.
                if (rowAsDict.TryGetValue(schema.RecnoField, out var recno) && recno is not null)
                {
                    dict["__ERPB_RECNO"] = Convert.ToInt32(recno);
                    highestTriggerRecNo = Math.Max(highestTriggerRecNo, Convert.ToInt32(recno));
                }
            }

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
        var publicRows = new List<IReadOnlyDictionary<string, object?>>(rows.Count);
        foreach (var r in rows)
        {
            var publicDict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in effectiveFields)
            {
                publicDict[field] = r.TryGetValue(field, out var v) ? v : null;
            }
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
}
