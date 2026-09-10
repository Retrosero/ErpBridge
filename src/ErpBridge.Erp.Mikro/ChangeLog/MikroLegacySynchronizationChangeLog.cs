using System.Data;
using System.Text.Json;
using Dapper;
using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Erp.Sql;
using ErpBridge.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.ChangeLog;

/// <summary>
/// Reads Mikro V15 changes from the established single-table
/// <c>dbo._ERPB_SENKRONIZASYON</c> feed. This source never creates or changes
/// ERP objects; the existing Mikro triggers remain the owner of the feed.
/// </summary>
public sealed class MikroLegacySynchronizationChangeLog : IErpChangeLogSource
{
    private readonly Func<string> _connectionStringResolver;
    private readonly ILogger<MikroLegacySynchronizationChangeLog> _logger;

    public MikroLegacySynchronizationChangeLog(
        Func<string> connectionStringResolver,
        ILogger<MikroLegacySynchronizationChangeLog>? logger = null)
    {
        _connectionStringResolver = connectionStringResolver
            ?? throw new ArgumentNullException(nameof(connectionStringResolver));
        _logger = logger
            ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<MikroLegacySynchronizationChangeLog>.Instance;
    }

    /// <inheritdoc />
    public ChangeDetectionCapability Capability => ChangeDetectionCapability.ShadowTableChangeLog;

    /// <inheritdoc />
    public IErpTrackedTableCatalog Catalog => MikroV15TrackedTableCatalog.Instance;

    /// <inheritdoc />
    public async Task<bool> IsInstalledAsync(CancellationToken ct = default)
    {
        await using var connection = await OpenAsync(ct).ConfigureAwait(false);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            MikroLegacySynchronizationSql.TableExists,
            cancellationToken: ct)).ConfigureAwait(false) == 1;
    }

    /// <inheritdoc />
    public Task InstallAsync(CancellationToken ct = default) =>
        throw new InvalidOperationException(
            "dbo._ERPB_SENKRONIZASYON is required but was not found. " +
            "ErpBridge is configured to use the existing table and will not create ERP objects.");

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

        var position = LegacySynchronizationCursor.Parse(cursor);
        var rows = new List<ErpChangeRow>(Math.Min(maxRows, 1024));
        var positions = new List<ErpTableCursorPosition>();
        var moreAvailable = false;

        await using var connection = await OpenAsync(ct).ConfigureAwait(false);
        var presentTables = (await connection.QueryAsync<string>(new CommandDefinition(
            "SELECT name FROM sys.tables", cancellationToken: ct)).ConfigureAwait(false))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var table in Catalog.Tables)
        {
            if (!presentTables.Contains(table.TableName))
            {
                continue;
            }

            if (rows.Count >= maxRows)
            {
                moreAvailable = true;
                break;
            }

            var previous = position.Get(table.TableKey);
            var budget = maxRows - rows.Count;
            var result = (await connection.QueryAsync(new CommandDefinition(
                MikroLegacySynchronizationSql.ReadChanges(table),
                new { budget, last = previous, tableId = table.TableId },
                cancellationToken: ct)).ConfigureAwait(false)).ToList();

            if (result.Count == 0)
            {
                continue;
            }

            var next = previous;
            foreach (IDictionary<string, object?> raw in result.Cast<IDictionary<string, object?>>())
            {
                var triggerRecNo = Convert.ToInt32(raw["__TriggerRECno"], System.Globalization.CultureInfo.InvariantCulture);
                var recordRecNo = Convert.ToInt32(raw["__KayitRECno"], System.Globalization.CultureInfo.InvariantCulture);
                var operation = Convert.ToByte(raw["__Islem"], System.Globalization.CultureInfo.InvariantCulture);
                next = Math.Max(next, triggerRecNo);

                var keyValue = TaggedKeyProjection.Recno.Project(table, recordRecNo);
                var sourceExists = raw.TryGetValue(table.EffectiveKeyField, out var sourceKey)
                    && sourceKey is not null and not DBNull;

                if (operation == 0 || !sourceExists)
                {
                    rows.Add(ErpChangeRow.Deleted(table.TableKey, keyValue));
                    continue;
                }

                if (operation is not (1 or 2))
                {
                    throw new InvalidDataException(
                        $"Unsupported Islem value {operation} in dbo._ERPB_SENKRONIZASYON at TriggerRECno {triggerRecNo}.");
                }

                var columns = new Dictionary<string, object?>(table.Fields.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var field in table.Fields)
                {
                    columns[field] = raw.TryGetValue(field, out var value) && value is not DBNull ? value : null;
                }

                rows.Add(new ErpChangeRow(
                    operation == 2 ? ErpChangeOp.Insert : ErpChangeOp.Update,
                    table.TableKey,
                    keyValue,
                    columns));
            }

            position.Advance(table.TableKey, next);
            positions.Add(new ErpTableCursorPosition(
                table.TableKey,
                PreviousUpsert: previous,
                NextUpsert: next,
                PreviousDelete: previous,
                NextDelete: next));
            moreAvailable |= result.Count == budget;
        }

        return new ErpChangeBatch(rows, position.ToCursor(), moreAvailable, positions);
    }

    private async Task<SqlConnection> OpenAsync(CancellationToken ct)
    {
        var connectionString = _connectionStringResolver();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No Mikro connection string is configured.");
        }

        var connection = new SqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(ct).ConfigureAwait(false);
            return connection;
        }
        catch (SqlException ex)
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            _logger.LogError(ex, "Legacy Mikro synchronization-table connection failed: {Error}",
                ConnectionStringMasker.MaskForLog(ex.Message));
            throw;
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
}

internal static class MikroLegacySynchronizationSql
{
    internal const string TableExists = """
SELECT CASE WHEN OBJECT_ID(N'[dbo].[_ERPB_SENKRONIZASYON]', 'U') IS NOT NULL
THEN 1 ELSE 0 END;
""";

    internal static string ReadChanges(ErpTrackedTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        var schema = SqlIdentifier.Validate(table.SchemaName);
        var tableName = SqlIdentifier.Validate(table.TableName);
        var keyField = SqlIdentifier.Validate(table.EffectiveKeyField);
        var fields = string.Join(", ", table.Fields.Select(field =>
            $"t.[{SqlIdentifier.Validate(field)}]"));

        return $@"
SELECT TOP (@budget)
       s.[TriggerRECno] AS [__TriggerRECno],
       s.[KayitRECno]   AS [__KayitRECno],
       s.[Islem]        AS [__Islem],
       {fields}
FROM [dbo].[_ERPB_SENKRONIZASYON] s WITH (NOLOCK)
LEFT JOIN [{schema}].[{tableName}] t WITH (NOLOCK)
       ON t.[{keyField}] = s.[KayitRECno]
WHERE s.[TriggerRECno] > @last
  AND s.[TabloID] = @tableId
ORDER BY s.[TriggerRECno];";
    }
}

internal sealed class LegacySynchronizationCursor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<string, int> _positions;

    private LegacySynchronizationCursor(Dictionary<string, int>? positions = null)
    {
        _positions = new Dictionary<string, int>(
            positions ?? new Dictionary<string, int>(),
            StringComparer.OrdinalIgnoreCase);
    }

    internal int Get(string tableKey) => _positions.GetValueOrDefault(tableKey, 0);

    internal void Advance(string tableKey, int triggerRecNo)
    {
        if (triggerRecNo > Get(tableKey))
        {
            _positions[tableKey] = triggerRecNo;
        }
    }

    internal ErpSyncCursor ToCursor() => _positions.Count == 0
        ? ErpSyncCursor.Start
        : new ErpSyncCursor(JsonSerializer.Serialize(new CursorDto { Legacy = _positions }, JsonOptions));

    internal static LegacySynchronizationCursor Parse(ErpSyncCursor? cursor)
    {
        if (cursor is null || cursor.IsStart)
        {
            return new LegacySynchronizationCursor();
        }

        try
        {
            var dto = JsonSerializer.Deserialize<CursorDto>(cursor.Value, JsonOptions);
            return new LegacySynchronizationCursor(dto?.Legacy);
        }
        catch (JsonException)
        {
            return new LegacySynchronizationCursor();
        }
    }

    private sealed class CursorDto
    {
        public Dictionary<string, int>? Legacy { get; set; }
    }
}
