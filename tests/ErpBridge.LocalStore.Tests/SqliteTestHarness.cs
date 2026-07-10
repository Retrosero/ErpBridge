using ErpBridge.LocalStore.Sqlite;
using ErpBridge.LocalStore.Sqlite.Migrations;
using Microsoft.Data.Sqlite;

namespace ErpBridge.LocalStore.Tests;

/// <summary>
/// Shared scaffolding for LocalStore tests. Each test gets its own in-memory
/// SQLite database so suites can run in parallel without sharing state. The
/// schema is applied via <see cref="InitialSchema.Script"/> AND <see cref="ProtectedConfigColumnsMigration"/>
/// so we exercise the exact DDL the agent will see at runtime, including the
/// Phase-2 Track-2 <c>protected_value</c> columns.
/// </summary>
public static class SqliteTestHarness
{
    /// <summary>
    /// Creates an isolated in-memory SQLite connection factory and applies the full
    /// schema (Phase-2 initial + Track-2 encrypted-at-rest columns). Each call returns
    /// a fresh database; the caller is responsible for disposing the underlying keep-alive
    /// connection when finished.
    /// </summary>
    /// <returns>A tuple of the factory and the keep-alive connection.</returns>
    public static (SqliteConnectionFactory Factory, SqliteConnection KeepAlive) CreateIsolatedFactory()
    {
        // Unique name per call so concurrent tests don't share a database even
        // when both happen to request "file::memory:?cache=shared".
        var name = $"erpbtest-{Guid.NewGuid():N}";
        var connectionString =
            $"Data Source=file:{name}?mode=memory&cache=shared;Pooling=True;";

        // Hold one connection open for the lifetime of this factory so the
        // shared in-memory database remains alive between connections.
        var keepAlive = new SqliteConnection(connectionString);
        keepAlive.Open();

        var factory = new SqliteConnectionFactory(connectionString);

        // Apply the schema synchronously against the keep-alive connection so
        // any subsequent connection from the factory sees the same DDL.
        Dapper.SqlMapper.Execute(keepAlive, InitialSchema.Script);

        // Initial baseline record
        Dapper.SqlMapper.Execute(keepAlive,
            "INSERT OR IGNORE INTO schema_version (version, applied_at) VALUES (@version, @appliedAt);",
            new { version = InitialSchema.Version, appliedAt = DateTime.UtcNow.ToString("O") });

        // Migration 002 — protected_value + protection_version (idempotent for tests
        // that re-open the same shared DB; in practice each test gets its own DB).
        Dapper.SqlMapper.Execute(keepAlive, ProtectedConfigColumnsMigration.Script);

        Dapper.SqlMapper.Execute(keepAlive,
            "INSERT OR REPLACE INTO schema_version (version, applied_at) VALUES (@version, @appliedAt);",
            new { version = ProtectedConfigColumnsMigration.Version, appliedAt = DateTime.UtcNow.ToString("O") });

        return (factory, keepAlive);
    }
}
