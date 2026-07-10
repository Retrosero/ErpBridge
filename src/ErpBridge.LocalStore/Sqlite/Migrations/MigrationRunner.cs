using System.Data.Common;
using Dapper;
using Microsoft.Data.Sqlite;

namespace ErpBridge.LocalStore.Sqlite.Migrations;

/// <summary>
/// Embedded SQL strings for the Phase-2 initial migration. The DDL is idempotent
/// (<c>CREATE TABLE IF NOT EXISTS</c>) so calling <see cref="MigrationRunner.EnsureSchemaAsync"/>
/// repeatedly is safe.
/// </summary>
public static class InitialSchema
{
    /// <summary>Schema version written into <c>schema_version</c> after a successful run.</summary>
    public const int Version = 1;

    /// <summary>
    /// Aggregate script executed by <see cref="MigrationRunner.EnsureSchemaAsync"/>.
    /// Matches <c>SKILL.md</c> section 7 verbatim.
    /// </summary>
    public const string Script = @"
CREATE TABLE IF NOT EXISTS mappings (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    tenant_id TEXT NOT NULL,
    entity_type TEXT NOT NULL,
    document_type TEXT NOT NULL,
    external_id TEXT NOT NULL,
    erp_type TEXT NOT NULL,
    erp_version TEXT NULL,
    erp_database_name TEXT NULL,
    document_series TEXT NULL,
    document_number INTEGER NULL,
    recno INTEGER NULL,
    guid TEXT NULL,
    checksum TEXT NOT NULL,
    created_at TEXT NOT NULL,
    UNIQUE (tenant_id, entity_type, document_type, external_id)
);

CREATE TABLE IF NOT EXISTS local_jobs (
    id TEXT PRIMARY KEY,
    tenant_id TEXT NOT NULL,
    job_type TEXT NOT NULL,
    external_id TEXT NOT NULL,
    payload_json TEXT NOT NULL,
    status TEXT NOT NULL,
    retry_count INTEGER NOT NULL DEFAULT 0,
    last_error TEXT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS checkpoints (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    tenant_id TEXT NOT NULL,
    sync_scope TEXT NOT NULL,
    last_success_at TEXT NULL,
    last_token TEXT NULL,
    updated_at TEXT NOT NULL,
    UNIQUE (tenant_id, sync_scope)
);

CREATE TABLE IF NOT EXISTS agent_config (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    is_secret INTEGER NOT NULL DEFAULT 0,
    updated_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS schema_version (
    version INTEGER PRIMARY KEY,
    applied_at TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_mappings_tenant ON mappings(tenant_id, external_id);
CREATE INDEX IF NOT EXISTS idx_local_jobs_status ON local_jobs(status);
";
}

/// <summary>
/// Phase-2 Track-2 migration: extend the <c>agent_config</c> table with the columns
/// needed for encrypted-at-rest secret values. The migration is additive — the existing
/// <c>value</c> column keeps plaintext rows readable by <see cref="SqliteAgentConfigStore"/>;
/// <c>protected_value</c> will be filled by the store on the next <c>SaveAsync</c> after
/// a real <see cref="ErpBridge.Core.Stores.IProtectedConfigProvider"/> is wired up.
/// </summary>
public static class ProtectedConfigColumnsMigration
{
    /// <summary>Schema version written into <c>schema_version</c> once this migration succeeds.</summary>
    public const int Version = 2;

    /// <summary>
    /// DDL applied by the migration runner. Both statements use the
    /// <c>PRAGMA table_info</c> guard because SQLite has no <c>ALTER TABLE … ADD COLUMN IF NOT EXISTS</c>.
    /// </summary>
    public const string Script = @"
-- 002_add_protected_value.sql
-- Adds the columns needed for encrypt-at-rest storage of secret values.
-- Both statements are NO-OP when the columns already exist.
ALTER TABLE agent_config ADD COLUMN protected_value TEXT NULL;
ALTER TABLE agent_config ADD COLUMN protection_version INTEGER NULL DEFAULT 0;
";
}

/// <summary>
/// Bootstraps the local SQLite schema. Idempotent — re-running just no-ops on an
/// already-migrated database.
/// </summary>
public sealed class MigrationRunner
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public MigrationRunner(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// Apply pending schema changes. Safe to call on every agent startup.
    /// </summary>
    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        await connection.ExecuteAsync(InitialSchema.Script).ConfigureAwait(false);

        // Initial baseline (version 1)
        const string recordScriptV1 = @"
INSERT OR IGNORE INTO schema_version (version, applied_at)
VALUES (@version, @appliedAt);";

        await connection.ExecuteAsync(recordScriptV1, new
        {
            version = InitialSchema.Version,
            appliedAt = DateTime.UtcNow.ToString("O"),
        }).ConfigureAwait(false);

        // Migration 002: protected_value + protection_version columns.
        // The runner checks both the schema_version table (to skip entirely on already-migrated
        // databases) AND PRAGMA table_info (to handle the legacy case where version 1 was
        // written but the new columns are missing — e.g. on an in-flight upgrade).
        await ApplyProtectedColumnsIfNeededAsync(connection).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the highest applied schema version, or <c>0</c> when the database is
    /// pristine or the table does not yet exist.
    /// </summary>
    public async Task<int> GetCurrentVersionAsync(CancellationToken ct = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        try
        {
            var version = await connection.ExecuteScalarAsync<int?>(
                "SELECT MAX(version) FROM schema_version;").ConfigureAwait(false);
            return version ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private static async Task ApplyProtectedColumnsIfNeededAsync(DbConnection connection)
    {
        var current = await connection.ExecuteScalarAsync<int?>(
            "SELECT MAX(version) FROM schema_version;").ConfigureAwait(false);

        if (current is not null && current.Value >= ProtectedConfigColumnsMigration.Version)
        {
            return;
        }

        var agentConfigColumns = await GetAgentConfigColumnsAsync(connection).ConfigureAwait(false);
        var hasProtectedValue = agentConfigColumns.Contains("protected_value");
        var hasProtectionVersion = agentConfigColumns.Contains("protection_version");

        if (!hasProtectedValue)
        {
            await TryAlterAddColumnAsync(connection, "agent_config", "protected_value", "TEXT").ConfigureAwait(false);
        }

        if (!hasProtectionVersion)
        {
            await TryAlterAddColumnAsync(connection, "agent_config", "protection_version", "INTEGER", defaultValue: "0").ConfigureAwait(false);
        }

        const string recordScriptV2 = @"
INSERT OR REPLACE INTO schema_version (version, applied_at)
VALUES (@version, @appliedAt);";

        await connection.ExecuteAsync(recordScriptV2, new
        {
            version = ProtectedConfigColumnsMigration.Version,
            appliedAt = DateTime.UtcNow.ToString("O"),
        }).ConfigureAwait(false);
    }

    private static async Task<HashSet<string>> GetAgentConfigColumnsAsync(DbConnection connection)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA table_info(agent_config);";

        if (cmd is SqliteCommand sqliteCmd)
        {
            await using var reader = await sqliteCmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var name = reader.GetString(1); // column 'name' is index 1 in pragma result
                columns.Add(name);
            }
        }
        else
        {
            // Generic DbConnection fallback for non-Sqlite drivers (kept for forward compatibility).
            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                columns.Add(reader.GetString(1));
            }
        }

        return columns;
    }

    private static async Task TryAlterAddColumnAsync(
        DbConnection connection,
        string tableName,
        string columnName,
        string sqlType,
        string? defaultValue = null)
    {
        var stmt = string.IsNullOrEmpty(defaultValue)
            ? $"ALTER TABLE {tableName} ADD COLUMN {columnName} {sqlType} NULL;"
            : $"ALTER TABLE {tableName} ADD COLUMN {columnName} {sqlType} NULL DEFAULT {defaultValue};";

        try
        {
            await connection.ExecuteAsync(stmt).ConfigureAwait(false);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
        {
            // Swallow duplicate-column errors — happens when schema_version was never
            // recorded (e.g. pre-existing database) but the column already exists.
        }
    }
}
