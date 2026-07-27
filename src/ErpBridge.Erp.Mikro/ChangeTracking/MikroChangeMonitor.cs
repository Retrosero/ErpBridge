using System.Data;
using Dapper;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Mikro.Connection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.ChangeTracking;

/// <summary>
/// Uses SQL Server Change Tracking when ALTER permission is available. If the
/// customer supplied a read-only login, falls back to lightweight table
/// modification signatures without failing agent startup.
/// </summary>
public sealed class MikroChangeMonitor : IMikroChangeMonitor
{
    private readonly IAgentConfigStore _configStore;
    private readonly MikroConnectionFactory _connectionFactory;
    private readonly ILogger<MikroChangeMonitor> _logger;
    private readonly Dictionary<string, string> _compatibilitySignatures =
        new(StringComparer.OrdinalIgnoreCase);
    private string? _initialisedDatabase;
    private MikroChangeMonitorMode _mode;
    private string? _warning;

    public MikroChangeMonitor(
        IAgentConfigStore configStore,
        MikroConnectionFactory connectionFactory,
        ILogger<MikroChangeMonitor> logger)
    {
        _configStore = configStore;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<MikroChangeBatch> PollAsync(long? lastVersion, CancellationToken ct = default)
    {
        var config = await _configStore.LoadAsync(ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Agent configuration is not available.");
        var settings = AgentConfigMapper.FromAgentConfig(config)
            ?? throw new InvalidOperationException("Mikro connection settings are incomplete.");
        _connectionFactory.SetActiveSettings(settings);

        await using var connection = new SqlConnection(_connectionFactory.BuildConnectionString(settings));
        await connection.OpenAsync(ct).ConfigureAwait(false);

        if (!string.Equals(_initialisedDatabase, settings.DatabaseName, StringComparison.OrdinalIgnoreCase))
        {
            await InitialiseAsync(connection, settings.DatabaseName, ct).ConfigureAwait(false);
            _initialisedDatabase = settings.DatabaseName;
        }

        return _mode == MikroChangeMonitorMode.ChangeTracking
            ? await PollChangeTrackingAsync(connection, lastVersion, ct).ConfigureAwait(false)
            : await PollCompatibilityAsync(connection, ct).ConfigureAwait(false);
    }

    private async Task InitialiseAsync(SqlConnection connection, string databaseName, CancellationToken ct)
    {
        try
        {
            var enabled = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                "SELECT COUNT(*) FROM sys.change_tracking_databases WHERE database_id = DB_ID();",
                cancellationToken: ct)).ConfigureAwait(false) > 0;
            if (!enabled)
            {
                var quotedDatabase = QuoteIdentifier(databaseName);
                await connection.ExecuteAsync(new CommandDefinition(
                    $"ALTER DATABASE {quotedDatabase} SET CHANGE_TRACKING = ON (CHANGE_RETENTION = 7 DAYS, AUTO_CLEANUP = ON);",
                    cancellationToken: ct)).ConfigureAwait(false);
            }

            foreach (var table in MikroChangeTableMap.Tables.Keys)
            {
                if (!await TableExistsAsync(connection, table, ct).ConfigureAwait(false))
                    continue;
                var isTracked = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                    "SELECT COUNT(*) FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID(@tableName);",
                    new { tableName = "dbo." + table }, cancellationToken: ct)).ConfigureAwait(false) > 0;
                if (!isTracked)
                {
                    await connection.ExecuteAsync(new CommandDefinition(
                        $"ALTER TABLE dbo.{QuoteIdentifier(table)} ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = OFF);",
                        cancellationToken: ct)).ConfigureAwait(false);
                }
            }

            _mode = MikroChangeMonitorMode.ChangeTracking;
            _warning = null;
            _logger.LogInformation("SQL Server Change Tracking is active for Mikro database {Database}.", databaseName);
        }
        catch (SqlException ex) when (ex.Number is 229 or 262 or 297 or 5011 or 5069)
        {
            _mode = MikroChangeMonitorMode.Compatibility;
            _warning = "SQL Change Tracking yetkisi yok; salt-okunur uyumluluk modu kullanılıyor.";
            _logger.LogWarning("Change Tracking could not be enabled; compatibility mode is active ({Number}).", ex.Number);
        }
    }

    private static async Task<bool> TableExistsAsync(SqlConnection connection, string table, CancellationToken ct) =>
        await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(*) FROM sys.tables WHERE object_id = OBJECT_ID(@tableName);",
            new { tableName = "dbo." + table }, cancellationToken: ct)).ConfigureAwait(false) > 0;

    private async Task<MikroChangeBatch> PollChangeTrackingAsync(
        SqlConnection connection,
        long? lastVersion,
        CancellationToken ct)
    {
        var current = await connection.ExecuteScalarAsync<long?>(new CommandDefinition(
            "SELECT CHANGE_TRACKING_CURRENT_VERSION();", cancellationToken: ct)).ConfigureAwait(false) ?? 0;
        if (!lastVersion.HasValue)
            return new(MikroChangeMonitorMode.ChangeTracking, current,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase), true);

        var sections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (table, mappedSections) in MikroChangeTableMap.Tables)
        {
            if (!await TableExistsAsync(connection, table, ct).ConfigureAwait(false))
                continue;
            var minValid = await connection.ExecuteScalarAsync<long?>(new CommandDefinition(
                "SELECT CHANGE_TRACKING_MIN_VALID_VERSION(OBJECT_ID(@tableName));",
                new { tableName = "dbo." + table }, cancellationToken: ct)).ConfigureAwait(false);
            if (minValid.HasValue && lastVersion.Value < minValid.Value)
                return new(MikroChangeMonitorMode.ChangeTracking, current, sections, true,
                    $"Change Tracking checkpoint for {table} is no longer valid.");

            var changed = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(
                $"SELECT TOP (1) 1 FROM CHANGETABLE(CHANGES dbo.{QuoteIdentifier(table)}, @version) AS CT;",
                new { version = lastVersion.Value }, cancellationToken: ct)).ConfigureAwait(false);
            if (changed == 1)
                sections.UnionWith(mappedSections);
        }

        return new(MikroChangeMonitorMode.ChangeTracking, current, sections, false, _warning);
    }

    private async Task<MikroChangeBatch> PollCompatibilityAsync(SqlConnection connection, CancellationToken ct)
    {
        const string sql = """
SELECT t.name AS TableName,
       CONCAT(COUNT_BIG(p.rows), ':',
              COALESCE(CONVERT(varchar(33), MAX(ius.last_user_update), 126), '')) AS Signature
FROM sys.tables t
LEFT JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
LEFT JOIN sys.dm_db_index_usage_stats ius
       ON ius.database_id = DB_ID() AND ius.object_id = t.object_id AND ius.index_id = 1
WHERE t.name IN @tables
GROUP BY t.name;
""";
        var rows = await connection.QueryAsync<SignatureRow>(new CommandDefinition(
            sql, new { tables = MikroChangeTableMap.Tables.Keys.ToArray() }, cancellationToken: ct))
            .ConfigureAwait(false);
        var sections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            if (_compatibilitySignatures.TryGetValue(row.TableName, out var previous)
                && !StringComparer.Ordinal.Equals(previous, row.Signature)
                && MikroChangeTableMap.Tables.TryGetValue(row.TableName, out var mapped))
                sections.UnionWith(mapped);
            _compatibilitySignatures[row.TableName] = row.Signature;
        }

        return new(MikroChangeMonitorMode.Compatibility, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            sections, false, _warning);
    }

    private static string QuoteIdentifier(string value) =>
        "[" + value.Replace("]", "]]", StringComparison.Ordinal) + "]";

    private sealed class SignatureRow
    {
        public string TableName { get; init; } = string.Empty;
        public string Signature { get; init; } = string.Empty;
    }
}
