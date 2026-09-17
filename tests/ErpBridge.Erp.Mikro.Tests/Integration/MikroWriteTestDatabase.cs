using Microsoft.Data.SqlClient;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// The Mikro database write tests may touch. Writing is allowed only into a test copy whose
/// name contains <c>ERPBTEST</c> (goal: the live company database is read-only), so a
/// mistyped <c>ERPBridge_MIKRO_WRITE_DB</c> can never write to <c>MikroDB_V15_02</c>.
/// </summary>
public static class MikroWriteTestDatabase
{
    /// <summary>Database override; defaults to <c>MikroDB_V15_ERPBTEST</c>.</summary>
    public const string DatabaseEnv = "ERPBridge_MIKRO_WRITE_DB";

    /// <summary>Server override; defaults to <c>tcp:localhost</c> (shared memory drops async reads on this setup).</summary>
    public const string ServerEnv = "ERPBridge_SCHEMA_SERVER";

    public static string Database =>
        Environment.GetEnvironmentVariable(DatabaseEnv) is { Length: > 0 } d ? d : "MikroDB_V15_ERPBTEST";

    public static string Server =>
        Environment.GetEnvironmentVariable(ServerEnv) is { Length: > 0 } s ? s : "tcp:localhost";

    /// <summary>Opted in and pointed at a test copy.</summary>
    public static bool CanWrite =>
        Environment.GetEnvironmentVariable(MikroIntegrationFixture.RunIntegrationEnv) == "1"
        && Database.Contains("ERPBTEST", StringComparison.OrdinalIgnoreCase);

    public static async Task<SqlConnection> OpenAsync()
    {
        var conn = new SqlConnection(
            $"Server={Server};Database={Database};Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15");
        await conn.OpenAsync();
        return conn;
    }
}
