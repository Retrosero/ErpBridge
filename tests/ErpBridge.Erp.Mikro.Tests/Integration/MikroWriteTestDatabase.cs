using Microsoft.Data.SqlClient;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// The Mikro database live tests may write to. Writing needs <c>ERPBridge_RUN_INTEGRATION=1</c>
/// <b>and</b> an explicit <c>ERPBridge_MIKRO_WRITE_DB</c> naming one of
/// <see cref="AllowedDatabases"/> — test copies of the company database. The live company
/// database (<c>MikroDB_V15_02</c>) is read-only for this goal, and the docker fixture's shared
/// gate alone never turns these tests on.
/// </summary>
public static class MikroWriteTestDatabase
{
    public const string DatabaseEnv = "ERPBridge_MIKRO_WRITE_DB";

    /// <summary>
    /// xUnit collection of every test that writes to the test copy: they share <c>_ERPB_EVRAK_ESLESME</c>
    /// (one test drops and recreates it), so they never run in parallel.
    /// </summary>
    public const string Collection = "Mikro write test database";

    /// <summary>Server override; defaults to <c>tcp:localhost</c> (shared memory drops async reads on this setup).</summary>
    public const string ServerEnv = "ERPBridge_SCHEMA_SERVER";

    /// <summary>
    /// <c>MikroDB_V15_DEMO</c>: the test company the user opened in Mikro, restored from the
    /// 2026-09-17 backup. The first copy (<c>MikroDB_V15_ERPBTEST</c>) was dropped on 2026-09-17.
    /// </summary>
    public static readonly IReadOnlySet<string> AllowedDatabases =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "MikroDB_V15_DEMO" };

    public static string? Database => Environment.GetEnvironmentVariable(DatabaseEnv) is { Length: > 0 } d ? d : null;

    public static string Server =>
        Environment.GetEnvironmentVariable(ServerEnv) is { Length: > 0 } s ? s : "tcp:localhost";

    /// <summary>Opted in and pointed at an allowed test copy.</summary>
    public static bool CanWrite =>
        Environment.GetEnvironmentVariable(MikroIntegrationFixture.RunIntegrationEnv) == "1"
        && Database is { } database && AllowedDatabases.Contains(database);

    public static async Task<SqlConnection> OpenAsync()
    {
        if (!CanWrite)
        {
            throw new InvalidOperationException($"{DatabaseEnv} must name one of: {string.Join(", ", AllowedDatabases)}.");
        }

        var conn = new SqlConnection(
            $"Server={Server};Database={Database};Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15");
        await conn.OpenAsync();
        return conn;
    }
}
