using System.Data;
using Dapper;
using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Sql;

/// <summary>
/// <see cref="IErpChangeLogSource"/> for any SQL Server-backed ERP, built on a
/// pair of shadow tables fed by per-table AFTER triggers.
///
/// <para>
/// This is the reusable heart of Faz 18. It carries <b>no vendor knowledge</b>:
/// the table list arrives as an <see cref="IErpTrackedTableCatalog"/> and the row
/// identity as an <see cref="IErpKeyProjection"/>. Mikro supplies its 49-table
/// catalog plus a recno/guid projection; a Logo adapter supplies its
/// <c>LG_*</c> catalog plus a <c>logicalref</c> projection, and gets the same
/// proven INSERT/UPDATE/DELETE capture for free.
/// </para>
///
/// <para>
/// <b>Read protocol.</b> For each tracked table the reader pulls rows whose
/// shadow <c>TriggerRECno</c> is greater than the per-table high-water mark in
/// the cursor, joining the shadow back to the source table to fetch the
/// whitelisted columns. Deletes come from the delete shadow and carry the key
/// only — the source row is gone. The batch stops as soon as the caller's row
/// budget is reached and reports <see cref="ErpChangeBatch.MoreAvailable"/> so
/// the caller keeps paging.
/// </para>
///
/// <para>
/// <b>Safety.</b> Every value is a Dapper parameter. The only interpolated
/// fragments are identifiers taken from the compiled-in catalog, each passed
/// through <see cref="SqlIdentifier.Validate"/> first.
/// </para>
/// </summary>
public sealed class SqlServerShadowTableChangeLog : IErpChangeLogSource
{
    private readonly Func<string> _connectionStringResolver;
    private readonly ShadowTableOptions _options;
    private readonly KeyKindProjection _projection;
    private readonly ILogger<SqlServerShadowTableChangeLog> _logger;

    /// <summary>
    /// Build the change log.
    /// </summary>
    /// <param name="catalog">Tables to track — supplied by the vendor adapter.</param>
    /// <param name="connectionStringResolver">
    /// Resolves the connection string on every call so a credential change in the
    /// operator UI takes effect without rebuilding the object graph.
    /// </param>
    /// <param name="projection">How row keys are tagged (recno / guid / logicalref).</param>
    /// <param name="options">Shadow-table naming; defaults to <see cref="ShadowTableOptions.Default"/>.</param>
    /// <param name="logger">Diagnostics sink.</param>
    public SqlServerShadowTableChangeLog(
        IErpTrackedTableCatalog catalog,
        Func<string> connectionStringResolver,
        KeyKindProjection? projection = null,
        ShadowTableOptions? options = null,
        ILogger<SqlServerShadowTableChangeLog>? logger = null)
    {
        Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _connectionStringResolver = connectionStringResolver ?? throw new ArgumentNullException(nameof(connectionStringResolver));
        _projection = projection ?? KeyKindProjection.RecnoOrGuid;
        _options = options ?? ShadowTableOptions.Default;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<SqlServerShadowTableChangeLog>.Instance;
    }

    /// <inheritdoc />
    public ChangeDetectionCapability Capability => ChangeDetectionCapability.ShadowTableChangeLog;

    /// <inheritdoc />
    public IErpTrackedTableCatalog Catalog { get; }

    /// <summary>Shadow-table naming this instance installs against.</summary>
    public ShadowTableOptions Options => _options;

    /// <inheritdoc />
    public async Task<bool> IsInstalledAsync(CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct).ConfigureAwait(false);

        var tablesExist = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(ShadowTableDdl.ShadowTablesExist(_options), cancellationToken: ct))
            .ConfigureAwait(false);

        if (tablesExist == 0)
        {
            return false;
        }

        var installed = (await conn.QueryAsync<string>(
            new CommandDefinition(ShadowTableDdl.ListInstalledTriggers(_options), cancellationToken: ct))
            .ConfigureAwait(false))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = Catalog.Tables.Count(t =>
            !installed.Contains(_options.SyncTriggerName(t.TableName)) ||
            !installed.Contains(_options.SyncDelTriggerName(t.TableName)));

        if (missing > 0)
        {
            _logger.LogInformation(
                "Change-log install incomplete for {Erp}: {Missing} of {Total} tracked tables are missing a trigger.",
                Catalog.Erp, missing, Catalog.Tables.Count);
        }

        return missing == 0;
    }

    /// <inheritdoc />
    public async Task InstallAsync(CancellationToken ct = default)
    {
        await using var conn = await OpenAsync(ct).ConfigureAwait(false);

        await conn.ExecuteAsync(new CommandDefinition(
            ShadowTableDdl.CreateSyncTable(_options), cancellationToken: ct)).ConfigureAwait(false);
        await conn.ExecuteAsync(new CommandDefinition(
            ShadowTableDdl.CreateSyncDelTable(_options), cancellationToken: ct)).ConfigureAwait(false);

        var installed = (await conn.QueryAsync<string>(
            new CommandDefinition(ShadowTableDdl.ListInstalledTriggers(_options), cancellationToken: ct))
            .ConfigureAwait(false))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var present = await ReadPresentTablesAsync(conn, ct).ConfigureAwait(false);

        var created = 0;
        var skipped = 0;
        foreach (var table in Catalog.Tables)
        {
            // A Mikro installation ships only the modules the customer licensed,
            // and a few catalog entries live in the master database. Creating a
            // trigger on a table that is not here would abort the whole install,
            // so skip it and report the count instead.
            if (!present.Contains(table.TableName))
            {
                skipped++;
                continue;
            }

            if (!installed.Contains(_options.SyncTriggerName(table.TableName)))
            {
                await conn.ExecuteAsync(new CommandDefinition(
                    ShadowTableDdl.CreateSyncTrigger(_options, table),
                    cancellationToken: ct)).ConfigureAwait(false);
                created++;
            }

            if (!installed.Contains(_options.SyncDelTriggerName(table.TableName)))
            {
                await conn.ExecuteAsync(new CommandDefinition(
                    ShadowTableDdl.CreateSyncDelTrigger(_options, table),
                    cancellationToken: ct)).ConfigureAwait(false);
                created++;
            }
        }

        _logger.LogInformation(
            "Change-log install for {Erp} complete: {Created} trigger(s) created across {Total} tracked tables; {Skipped} table(s) absent from this installation.",
            Catalog.Erp, created, Catalog.Tables.Count, skipped);
    }

    /// <inheritdoc />
    public async Task<ErpChangeBatch> ReadChangesAsync(
        ErpSyncCursor cursor,
        int maxRows,
        CancellationToken ct = default)
    {
        if (maxRows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRows), maxRows, "maxRows must be positive.");
        }

        var position = ShadowCursor.Parse(cursor);
        var before = ShadowCursor.Parse(cursor);
        var rows = new List<ErpChangeRow>(Math.Min(maxRows, 1024));
        var touched = new List<ErpTrackedTable>();
        var moreAvailable = false;

        await using var conn = await OpenAsync(ct).ConfigureAwait(false);

        foreach (var table in Catalog.Tables)
        {
            if (rows.Count >= maxRows)
            {
                moreAvailable = true;
                break;
            }

            var budget = maxRows - rows.Count;
            touched.Add(table);

            var deleted = await ReadDeletesAsync(conn, table, position, budget, ct).ConfigureAwait(false);
            rows.AddRange(deleted.Rows);
            moreAvailable |= deleted.More;

            if (rows.Count >= maxRows)
            {
                moreAvailable = true;
                break;
            }

            var upserts = await ReadUpsertsAsync(conn, table, position, maxRows - rows.Count, ct).ConfigureAwait(false);
            rows.AddRange(upserts.Rows);
            moreAvailable |= upserts.More;
        }

        // Report where each visited table started and ended so the wire
        // protocol can carry a numeric high-water mark without the caller
        // having to decode the opaque cursor.
        var positions = touched
            .Select(t => new ErpTableCursorPosition(
                t.TableKey,
                before.Upsert(t.TableKey), position.Upsert(t.TableKey),
                before.Delete(t.TableKey), position.Delete(t.TableKey)))
            .Where(p => p.Advanced)
            .ToList();

        return new ErpChangeBatch(rows, position.ToCursor(), moreAvailable, positions);
    }

    private async Task<(List<ErpChangeRow> Rows, bool More)> ReadUpsertsAsync(
        SqlConnection conn,
        ErpTrackedTable table,
        ShadowCursor position,
        int budget,
        CancellationToken ct)
    {
        var tableName = SqlIdentifier.Validate(table.TableName);
        var schemaName = SqlIdentifier.Validate(table.SchemaName);
        var keyField = SqlIdentifier.Validate(table.EffectiveKeyField);
        var columns = BuildColumnList(table);
        var keyColumn = ShadowTableDdl.KeyColumn(table);
        var last = position.Upsert(table.TableKey);

        // Join natively on the typed shadow key so SQL Server can seek the
        // source table's primary-key index; a string conversion here would make
        // the predicate non-sargable and scan the whole table on every poll.
        var sql = $@"
SELECT TOP (@budget)
       s.[TriggerRECno] AS __TriggerRECno,
       {columns}
FROM {_options.QualifiedSyncTable} s
INNER JOIN [{schemaName}].[{tableName}] t
        ON t.[{keyField}] = s.[{keyColumn}]
WHERE s.[TabloID] = @tableId
  AND s.[TriggerRECno] > @last
ORDER BY s.[TriggerRECno];";

        var read = await conn.QueryAsync(new CommandDefinition(
            sql,
            new { budget, tableId = table.TableId, last },
            cancellationToken: ct)).ConfigureAwait(false);

        var rows = new List<ErpChangeRow>();
        var count = 0;

        foreach (IDictionary<string, object?> raw in read.Cast<IDictionary<string, object?>>())
        {
            count++;
            var triggerRecNo = Convert.ToInt32(raw["__TriggerRECno"], System.Globalization.CultureInfo.InvariantCulture);
            var keyValue = _projection.Project(table, raw.TryGetValue(table.EffectiveKeyField, out var rawKey) ? rawKey : null);

            var payload = new Dictionary<string, object?>(raw.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var kv in raw)
            {
                if (kv.Key is "__TriggerRECno")
                {
                    continue;
                }

                payload[kv.Key] = kv.Value is DBNull ? null : kv.Value;
            }

            // The shadow row records an "upsert"; the consumer treats both the
            // same way (idempotent upsert), so Update is the honest label — the
            // shadow table does not distinguish first insert from later edit.
            rows.Add(new ErpChangeRow(ErpChangeOp.Update, table.TableKey, keyValue, payload));
            position.AdvanceUpsert(table.TableKey, triggerRecNo);
        }

        return (rows, count == budget);
    }

    private async Task<(List<ErpChangeRow> Rows, bool More)> ReadDeletesAsync(
        SqlConnection conn,
        ErpTrackedTable table,
        ShadowCursor position,
        int budget,
        CancellationToken ct)
    {
        var last = position.Delete(table.TableKey);

        var keyColumn = ShadowTableDdl.KeyColumn(table);

        var sql = $@"
SELECT TOP (@budget)
       s.[TriggerRECno] AS TriggerRecNo,
       s.[{keyColumn}]  AS KeyValue
FROM {_options.QualifiedSyncDelTable} s
WHERE s.[TabloID] = @tableId
  AND s.[TriggerRECno] > @last
ORDER BY s.[TriggerRECno];";

        var read = (await conn.QueryAsync(new CommandDefinition(
            sql,
            new { budget, tableId = table.TableId, last },
            cancellationToken: ct)).ConfigureAwait(false)).ToList();

        var rows = new List<ErpChangeRow>(read.Count);
        foreach (IDictionary<string, object?> row in read.Cast<IDictionary<string, object?>>())
        {
            var triggerRecNo = Convert.ToInt32(row["TriggerRecNo"], System.Globalization.CultureInfo.InvariantCulture);
            rows.Add(ErpChangeRow.Deleted(table.TableKey, _projection.Project(table, row["KeyValue"])));
            position.AdvanceDelete(table.TableKey, triggerRecNo);
        }

        return (rows, read.Count == budget);
    }

    /// <summary>
    /// Build the bracket-quoted <c>t.[col]</c> list for a tracked table. Every
    /// name comes from the catalog whitelist and is validated first.
    /// </summary>
    private static string BuildColumnList(ErpTrackedTable table)
    {
        if (table.Fields.Count == 0)
        {
            throw new InvalidOperationException(
                $"Tracked table '{table.TableKey}' declares no readable fields.");
        }

        return string.Join(", ", table.Fields.Select(f => $"t.[{SqlIdentifier.Validate(f)}]"));
    }

    /// <summary>
    /// Table names that actually exist in the target database. A Mikro
    /// installation ships only the licensed modules, so the catalog is a superset
    /// of any one install.
    /// </summary>
    private static async Task<HashSet<string>> ReadPresentTablesAsync(SqlConnection conn, CancellationToken ct)
    {
        var names = await conn.QueryAsync<string>(new CommandDefinition(
            "SELECT name FROM sys.tables", cancellationToken: ct)).ConfigureAwait(false);
        return names.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<SqlConnection> OpenAsync(CancellationToken ct)
    {
        var connectionString = _connectionStringResolver();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No ERP connection string is configured; the change log cannot open a connection.");
        }

        var conn = new SqlConnection(connectionString);
        try
        {
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return conn;
        }
        catch (SqlException ex)
        {
            await conn.DisposeAsync().ConfigureAwait(false);
            // SqlException messages can echo fragments of the connection string.
            _logger.LogError(ex, "Change-log connection failed: {Error}",
                ConnectionStringMasker.MaskForLog(ex.Message));
            throw;
        }
        catch
        {
            await conn.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
}
