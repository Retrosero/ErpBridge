using System.Globalization;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;

namespace ErpBridge.Erp.Mikro.Connection;

/// <summary>
/// Maps <see cref="AgentConfig"/> to a Mikro-bound
/// <see cref="MikroConnectionSettings"/>. Lives in <c>ErpBridge.Erp.Mikro</c>
/// because the Core layer must not reference Mikro types (architectural rule,
/// see <c>SKILL.md</c> section 3 rule 1). The mapper returns
/// <c><see cref="object"/></c> from the public entry-point to respect the
/// <see cref="IAgentConfigToErpSettingsMapper"/> contract — typed access is
/// offered through the convenience <c>FromAgentConfig</c> helper below.
/// </summary>
public sealed class AgentConfigMapper : IAgentConfigToErpSettingsMapper
{
    /// <inheritdoc />
    /// <remarks>
    /// The interface contract returns <c>object?</c> so <c>Core</c> stays Mikro-free.
    /// Concrete callers inside the Mikro package can use the typed
    /// <see cref="FromAgentConfig(AgentConfig)"/> helper.
    /// </remarks>
    public object? ToErpSettings(AgentConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        // Try-parse int fields up-front so callers learn about validation
        // failures via the null return (or via the typed helper below).
        if (!int.TryParse(config.CompanyNo.ToString(CultureInfo.InvariantCulture),
                NumberStyles.Integer, CultureInfo.InvariantCulture, out var companyNo))
        {
            return null;
        }
        if (!int.TryParse(config.BranchNo.ToString(CultureInfo.InvariantCulture),
                NumberStyles.Integer, CultureInfo.InvariantCulture, out var branchNo))
        {
            return null;
        }

        return FromAgentConfig(config, companyNo, branchNo);
    }

    /// <summary>
    /// Public surface area intentionally exposes two <c>FromAgentConfig</c> overloads
    /// so callers can either reuse parsed values or let the mapper read
    /// <c>CompanyNo</c> / <c>BranchNo</c> directly from the config.
    /// </summary>

    /// <summary>
    /// Strongly-typed mapping. Performs strict null/empty checks on the fields
    /// the Mikro adapter actually needs (<c>SqlServer</c>, <c>SqlUserName</c>,
    /// <c>MikroDatabaseName</c>). Returns <c>null</c> when any required field
    /// is missing — <c>Password</c> may legitimately be empty (trusted auth /
    /// integrated security on the customer's network).
    /// </summary>
    /// <param name="config">Source agent config.</param>
    public static MikroConnectionSettings? FromAgentConfig(AgentConfig config)
        => FromAgentConfig(config, config.CompanyNo, config.BranchNo);

    /// <summary>
    /// Strongly-typed mapping with explicit company / branch numbers (so
    /// callers that parse the values up-front can reuse them and still hit
    /// the canonical validation path).
    /// </summary>
    public static MikroConnectionSettings? FromAgentConfig(AgentConfig config, int companyNo, int branchNo)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (string.IsNullOrWhiteSpace(config.SqlServer)) return null;
        if (string.IsNullOrWhiteSpace(config.MikroDatabaseName)) return null;

        // SQL-auth path requires a user name; Windows-auth path does not.
        if (!config.UseWindowsAuth && string.IsNullOrWhiteSpace(config.SqlUserName)) return null;

        // CompanyNo / BranchNo are validated at this seam — out-of-range values
        // would also be caught downstream by the lookup SQL, but we surface
        // a clear "missing field" here.
        if (companyNo <= 0) return null;
        if (branchNo < 0) return null;

        return new MikroConnectionSettings(
            Server: config.SqlServer.Trim(),
            UserId: config.UseWindowsAuth ? string.Empty : (config.SqlUserName ?? string.Empty).Trim(),
            Password: config.UseWindowsAuth ? string.Empty : (config.SqlPassword ?? string.Empty),
            DatabaseName: config.MikroDatabaseName.Trim(),
            IntegratedSecurity: config.UseWindowsAuth);
    }
}
