using ErpBridge.Erp.Mikro.Connection;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// Live-SQL Server settings helper for the production "TULPAR" Mikro V15 instance.
/// <para>
/// TULPAR is the customer-side SQL Server (server <c>TULPAR</c>, database
/// <c>MikroDB_V15_02</c>, user <c>mikro_sync_user</c>). This helper lets an
/// operator point the live integration tests at the real ERP without baking the
/// credentials into source — every value is read from an environment variable at
/// test startup. The default behaviour (no env var set) is identical to
/// <see cref="MikroIntegrationFixture"/>: tests are skipped and the hermetic CI
/// pipeline remains green.
/// </para>
/// <para>
/// Env-var surface (all four required to opt in):
/// </para>
/// <list type="bullet">
///   <item><c>ERPBridge_TULPAR_SERVER</c> — e.g. <c>TULPAR</c> or <c>TULPAR,1433</c>.</item>
///   <item><c>ERPBridge_TULPAR_DATABASE</c> — e.g. <c>MikroDB_V15_02</c>.</item>
///   <item><c>ERPBridge_TULPAR_USER</c> — e.g. <c>mikro_sync_user</c>.</item>
///   <item><c>ERPBridge_TULPAR_PASSWORD</c> — never logged, never asserted on.</item>
/// </list>
/// <para>
/// In addition the operator must set <see cref="MikroIntegrationFixture.RunIntegrationEnv"/>
/// to <c>1</c> so the env-var gate opens. The password value is <b>not</b>
/// compared to a hard-coded literal — that would defeat the purpose of the
/// env-var surface and pin a credential in source. Tests assert the connection
/// either succeeds or surfaces a clean SqlException, which is enough to detect
/// "password expired" / "user locked" / "database offline" without baking any
/// production secret into the test project.
/// </para>
/// </summary>
/// <remarks>
/// Why a separate helper from <see cref="MikroIntegrationFixture"/>?
/// <list type="bullet">
///   <item>The docker-compose fixture is hermetic, ephemeral, and seeded with a
///         tiny test database. TULPAR is the real production instance, with
///         arbitrarily many rows in every master table, and an expired password
///         at the time of writing. Mixing the two would make the docker test
///         paths flaky when TULPAR-specific rows leak into shared assertions.</item>
///   <item>Connection-string build behaviour is identical — both helpers return
///         the canonical <see cref="MikroConnectionSettings"/> record consumed
///         by <see cref="MikroConnectionFactory"/>.</item>
/// </list>
/// </remarks>
public static class TulparLiveSettings
{
    /// <summary>Server (host or <c>host,port</c>) env var for the TULPAR fixture.</summary>
    public const string ServerEnv = "ERPBridge_TULPAR_SERVER";

    /// <summary>Database name env var for the TULPAR fixture.</summary>
    public const string DatabaseEnv = "ERPBridge_TULPAR_DATABASE";

    /// <summary>SQL login env var for the TULPAR fixture.</summary>
    public const string UserEnv = "ERPBridge_TULPAR_USER";

    /// <summary>SQL password env var for the TULPAR fixture. Never logged.</summary>
    public const string PasswordEnv = "ERPBridge_TULPAR_PASSWORD";

    /// <summary>
    /// True only when the operator opted in by setting every required env var.
    /// <para>
    /// Note: the TULPAR fixture is decoupled from
    /// <see cref="MikroIntegrationFixture.RunIntegrationEnv"/> on purpose.
    /// The docker-compose integration gate covers the hermetic V15/V16 SQL
    /// fixture; TULPAR is a separate, real customer database. An operator
    /// running only the TULPAR test suite should not be forced to also stand
    /// up the docker-compose SQL container just to flip the global gate.
    /// </para>
    /// <para>
    /// If a stray local env is the worry, the
    /// <c>ERPBridge_TULPAR_PASSWORD</c> check (length &gt; 0) already keeps
    /// the fixture disabled until the operator explicitly writes a password
    /// into <c>.env</c>.
    /// </para>
    /// </summary>
    public static bool IsConfigured
    {
        get
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ServerEnv))
                && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(DatabaseEnv))
                && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(UserEnv))
                && Environment.GetEnvironmentVariable(PasswordEnv) is { Length: > 0 };
        }
    }

    /// <summary>
    /// Read the four env vars and build a <see cref="MikroConnectionSettings"/>.
    /// Returns <c>null</c> when <see cref="IsConfigured"/> is <c>false</c>;
    /// individual test methods should early-return in that case so the
    /// "no-env" run is a quiet pass.
    /// </summary>
    /// <remarks>
    /// The returned object is fed directly to
    /// <see cref="MikroConnectionFactory.SetActiveSettings"/> so the
    /// <c>MikroDbReader</c> path (no explicit settings parameter) resolves to
    /// the TULPAR connection string.
    /// </remarks>
    public static MikroConnectionSettings? GetSettings()
    {
        if (!IsConfigured)
        {
            return null;
        }

        return new MikroConnectionSettings(
            Server: Environment.GetEnvironmentVariable(ServerEnv)!.Trim(),
            UserId: Environment.GetEnvironmentVariable(UserEnv)!.Trim(),
            Password: Environment.GetEnvironmentVariable(PasswordEnv)!,
            DatabaseName: Environment.GetEnvironmentVariable(DatabaseEnv)!.Trim());
    }

    /// <summary>
    /// Sanitised description of the active configuration suitable for
    /// <c>ITestOutputHelper</c> log lines. Never includes the password.
    /// </summary>
    public static string Describe(MikroConnectionSettings settings)
        => $"server={settings.Server}; database={settings.DatabaseName}; user={settings.UserId}; password=***REDACTED***";
}
