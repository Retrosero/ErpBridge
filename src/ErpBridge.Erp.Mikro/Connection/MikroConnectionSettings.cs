using Microsoft.Extensions.Configuration;

namespace ErpBridge.Erp.Mikro.Connection;

/// <summary>
/// Plain-data carrier for Mikro SQL connection parameters. All fields are
/// required for the default SQL-auth path; when <see cref="IntegratedSecurity"/>
/// is set the SQL <see cref="UserId"/> and <see cref="Password"/> are ignored
/// at the connection-string layer and Windows authentication is used instead.
/// </summary>
/// <param name="Server">SQL Server host (e.g. "MIKROSQL\\MIKRO" or "192.168.1.10").</param>
/// <param name="UserId">SQL login (ignored when <see cref="IntegratedSecurity"/> is true).</param>
/// <param name="Password">SQL password (ignored when <see cref="IntegratedSecurity"/> is true).</param>
/// <param name="DatabaseName">Mikro database (e.g. "MIKRO16").</param>
/// <param name="IntegratedSecurity">True for Windows authentication (Trusted_Connection / SSPI).</param>
public sealed record MikroConnectionSettings(
    string Server,
    string UserId,
    string Password,
    string DatabaseName,
    bool IntegratedSecurity = false)
{
    /// <summary>
    /// Configuration section that holds Mikro SQL connection parameters. The WPF
    /// settings window writes to this section on every "Kaydet" click so the
    /// adapter always reads the latest values without restarting the process.
    /// </summary>
    public const string ConfigurationSection = "Mikro";

    /// <summary>
    /// Build a <see cref="MikroConnectionSettings"/> from the
    /// <see cref="ConfigurationSection"/> of <paramref name="configuration"/>.
    /// Returns <c>null</c> when the section is missing or any required field is
    /// blank — the caller is expected to surface a clear "configuration missing"
    /// error rather than fall back to defaults.
    /// </summary>
    /// <remarks>
    /// Passwords may legitimately be empty (trusted auth / integrated security),
    /// so an empty password is allowed and the call returns a populated settings
    /// object — but Server and DatabaseName are mandatory. UserId is required
    /// only when <see cref="IntegratedSecurity"/> is false.
    /// </remarks>
    public static MikroConnectionSettings? FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(ConfigurationSection);
        if (!section.Exists())
        {
            return null;
        }

        var server = section["Server"];
        var userId = section["UserId"];
        var databaseName = section["DatabaseName"];
        var integratedSecurity = ParseBool(section["IntegratedSecurity"]);

        if (string.IsNullOrWhiteSpace(server)
            || string.IsNullOrWhiteSpace(databaseName))
        {
            return null;
        }

        if (!integratedSecurity && string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        // Password is read as-is (may be empty for trusted-auth setups). The
        // adapter's logger never logs this value verbatim — it sees the masked
        // form via ConnectionStringMasker.
        var password = section["Password"] ?? string.Empty;

        return new MikroConnectionSettings(
            Server: server.Trim(),
            UserId: integratedSecurity ? (userId?.Trim() ?? string.Empty) : userId!.Trim(),
            Password: integratedSecurity ? string.Empty : password,
            DatabaseName: databaseName.Trim(),
            IntegratedSecurity: integratedSecurity);
    }

    private static bool ParseBool(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return false;
        return raw.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)
            || raw.Trim().Equals("1", StringComparison.OrdinalIgnoreCase)
            || raw.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase);
    }
}