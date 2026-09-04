using Microsoft.Data.SqlClient;

namespace ErpBridge.Erp.Mikro.Connection;

/// <summary>
/// Builds <see cref="SqlConnection"/> instances for the Mikro database. The factory
/// itself never stores credentials — it is stateless w.r.t. secrets — but it does
/// expose a single "active settings" slot so collaborators that are constructed
/// without a direct settings reference (e.g. <see cref="Readers.MikroDbReader"/>)
/// can still resolve a connection string at read time. The active slot is updated
/// by <c>MikroAdapter</c> via <see cref="SetActiveSettings(MikroConnectionSettings)"/>
/// whenever the user saves new credentials.
/// </summary>
public sealed class MikroConnectionFactory
{
    private MikroConnectionSettings _activeSettings = new(
        Server: string.Empty,
        UserId: string.Empty,
        Password: string.Empty,
        DatabaseName: string.Empty,
        CompanyNo: 1,
        WarehouseNo: 1);

    /// <summary>
    /// Overwrite the "active" settings used by readers that don't carry
    /// settings of their own. Safe to call from any thread — assignment of a
    /// reference type is atomic on .NET.
    /// </summary>
    public void SetActiveSettings(MikroConnectionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _activeSettings = settings;
    }

    /// <summary>
    /// Build a SQL Server connection string for the supplied Mikro settings. The
    /// format is the canonical
    /// <c>Server=...;User ID=...;Password=...;Initial Catalog=...;MultipleActiveResultSets=True</c>
    /// — <c>MultipleActiveResultSets</c> is enabled so future writers (Phase 6) can
    /// batch header + lines on the same connection without juggling MARS quirks.
    /// </summary>
    /// <remarks>
    /// When <see cref="MikroConnectionSettings.IntegratedSecurity"/> is true the
    /// SQL user/password pair is omitted and <c>Integrated Security=true</c> is
    /// emitted — the connection then authenticates as the process identity
    /// (Windows Service: <c>NETWORK SERVICE</c> / <c>LOCAL SYSTEM</c>; WPF: the
    /// signed-in Windows user). Trusted_Connection / SSPI flow is supported
    /// for environments where Mikro rejects SQL-logins.
    /// </remarks>
    /// <exception cref="ArgumentException">Any required field is missing.</exception>
    public string BuildConnectionString(MikroConnectionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.Server))
            throw new ArgumentException("Server is required.", nameof(settings));
        if (string.IsNullOrWhiteSpace(settings.DatabaseName))
            throw new ArgumentException("DatabaseName is required.", nameof(settings));
        if (!settings.IntegratedSecurity && string.IsNullOrWhiteSpace(settings.UserId))
            throw new ArgumentException("UserId is required when IntegratedSecurity is false.", nameof(settings));

        // Canonical keys with explicit casing — SqlConnectionStringBuilder otherwise
        // emits "Multiple Active Result Sets" with spaces, which the test suite
        // matches as "MultipleActiveResultSets=True". Keep the format stable.
        var builder = new List<string>
        {
            $"Data Source={settings.Server}",
            $"Initial Catalog={settings.DatabaseName}",
            "MultipleActiveResultSets=True",
            "Encrypt=True",
            "TrustServerCertificate=True",
            "Connect Timeout=30",
        };

        if (settings.IntegratedSecurity)
        {
            // Windows authentication — driver picks up the process identity.
            // No User ID / Password fragments in the connection string.
            builder.Add("Integrated Security=true");
        }
        else
        {
            // SQL-auth path — the canonical SqlClient shape.
            // Password may legitimately be empty for some local-trust setups, but
            // never null/whitespace-only beyond this guard — the .NET SqlClient
            // tolerates an empty password.
            builder.Add($"User ID={settings.UserId}");
            builder.Add($"Password={settings.Password ?? string.Empty}");
        }

        return string.Join(';', builder);
    }

    /// <summary>
    /// Build the connection string from the currently "active" settings set by
    /// the adapter via <see cref="SetActiveSettings(MikroConnectionSettings)"/>.
    /// Used by collaborators that don't have a settings reference of their own
    /// (notably <see cref="Readers.MikroDbReader"/>).
    /// </summary>
    public string BuildConnectionStringFromActive()
        => BuildConnectionString(_activeSettings);

    /// <summary>
    /// Convenience helper — builds the connection string and wraps it in a fresh,
    /// non-open <see cref="SqlConnection"/>. The caller owns the lifetime and is
    /// expected to dispose it.
    /// </summary>
    public SqlConnection CreateConnection(MikroConnectionSettings settings)
        => new(BuildConnectionString(settings));
}
