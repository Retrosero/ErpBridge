using ErpBridge.Erp.Mikro.Connection;
using Microsoft.Extensions.Configuration;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// Shared helper for live-DB integration tests in the Mikro test project.
/// Reads environment variables, builds a <see cref="MikroConnectionSettings"/>,
/// and returns an in-memory <see cref="IConfiguration"/> bound to the
/// <c>Mikro</c> section so adapter code paths that read from configuration
/// (e.g. <c>MikroConnectionSettings.FromConfiguration</c>) work unchanged.
///
/// If <c>ERPBridge_RUN_INTEGRATION</c> is not set, <see cref="ShouldRun"/>
/// returns <c>false</c> and individual tests should early-return so the
/// hermetic CI pipeline does not need a real SQL Server.
/// </summary>
public static class MikroIntegrationFixture
{
    /// <summary>Env var gate. Set to "1" to opt-in to live SQL Server execution.</summary>
    public const string RunIntegrationEnv = "ERPBridge_RUN_INTEGRATION";

    /// <summary>Server + port env var for V16 fixture (port 14330 in docker-compose).</summary>
    public const string Server16Env = "ERPBridge_SQL_SERVER_16";

    /// <summary>Server + port env var for V15 fixture (port 14331 in docker-compose).</summary>
    public const string Server15Env = "ERPBridge_SQL_SERVER_15";

    /// <summary>SQL login env var (defaults to "sa").</summary>
    public const string UserEnv = "ERPBridge_SQL_USER";

    /// <summary>SQL password env var. Test-only.</summary>
    public const string PasswordEnv = "ERPBridge_SQL_PASSWORD";

    /// <summary>Hardcoded fallback password for the docker-compose fixture.</summary>
    public const string TestPasswordFallback = "ErpBridge_Test_2026!";

    /// <summary>
    /// True only when the user explicitly opts in. We require the env var
    /// to be "1" — any other value (missing, empty, "true", "yes") leaves
    /// the fixture disabled so a stray local env does not silently turn
    /// the hermetic unit test pipeline into a live DB test.
    /// </summary>
    public static bool ShouldRun =>
        Environment.GetEnvironmentVariable(RunIntegrationEnv) == "1";

    /// <summary>
    /// Build a <see cref="MikroConnectionSettings"/> for the requested Mikro
    /// version fixture ("15" or "16"). Returns <c>null</c> when
    /// <see cref="ShouldRun"/> is false so test methods can early-return.
    /// </summary>
    /// <param name="databaseSuffix">"15" → V15 fixture, "16" → V16 fixture.</param>
    /// <remarks>
    /// Seed expectations (mirrored in <c>mikro15-init.sql</c> and
    /// <c>mikro16-init.sql</c>):
    /// <list type="bullet">
    ///   <item>STOK001, STOK002 — two stock codes for multi-line tests.</item>
    ///   <item>CARI 120.01.0001, DEPO 1.</item>
    /// </list>
    /// STK002 was added in F6.4 so the integration tests can exercise a
    /// payload with two distinct stock lines.
    /// </remarks>
    public static MikroConnectionSettings? GetSettings(string databaseSuffix = "16")
    {
        if (!ShouldRun) return null;

        var isV15 = databaseSuffix == "15";
        var port = isV15 ? "14331" : "14330";
        var serverEnv = isV15 ? Server15Env : Server16Env;
        var databaseName = isV15 ? "MIKRO15_FAZ3" : "MIKRO16_FAZ3";

        var server = Environment.GetEnvironmentVariable(serverEnv);
        if (string.IsNullOrWhiteSpace(server))
        {
            // Fallback to localhost on the docker-compose port so a developer
            // who only set ERPBridge_RUN_INTEGRATION=1 can still run the suite.
            server = $"localhost,{port}";
        }

        return new MikroConnectionSettings(
            Server: server,
            UserId: Environment.GetEnvironmentVariable(UserEnv) ?? "sa",
            Password: Environment.GetEnvironmentVariable(PasswordEnv) ?? TestPasswordFallback,
            DatabaseName: databaseName);
    }

    /// <summary>
    /// Build an in-memory <see cref="IConfiguration"/> with the Mikro section
    /// populated. Adapter code that reads via <see cref="IConfiguration"/>
    /// gets the same values the runtime would see.
    /// </summary>
    public static IConfiguration BuildConfiguration(MikroConnectionSettings settings)
    {
        var dict = new Dictionary<string, string?>
        {
            ["Mikro:Server"] = settings.Server,
            ["Mikro:UserId"] = settings.UserId,
            ["Mikro:Password"] = settings.Password,
            ["Mikro:DatabaseName"] = settings.DatabaseName,
        };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }
}
