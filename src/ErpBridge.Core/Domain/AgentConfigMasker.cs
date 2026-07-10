using ErpBridge.Core.Domain;
using ErpBridge.Shared;

namespace ErpBridge.Core.Domain;

/// <summary>
/// Log-safe projection helpers for <see cref="AgentConfig"/>. The masker lives
/// in <c>Core</c> because <c>AgentConfig</c> is a Core type — <c>Shared</c>
/// cannot take a dependency on <c>Core</c>, and pulling the masker up would
/// break the architectural rule.
/// </summary>
public static class AgentConfigMasker
{
    /// <summary>
    /// Produce a deep-ish clone of <paramref name="config"/> with the SQL
    /// password replaced by the canonical redacted marker. The original
    /// instance is untouched — this method is meant for diagnostic dumps
    /// and structured log scopes.
    /// </summary>
    public static AgentConfig Mask(AgentConfig? config)
    {
        if (config is null)
        {
            return new AgentConfig();
        }

        return new AgentConfig
        {
            LicenseKey = config.LicenseKey,
            TenantId = config.TenantId,
            ErpType = config.ErpType,
            SqlServer = config.SqlServer,
            SqlUserName = config.UseWindowsAuth ? "(Integrated Security)" : config.SqlUserName,
            SqlPassword = ConnectionStringMasker.RedactedMarker,
            MikroDatabaseName = config.MikroDatabaseName,
            CompanyNo = config.CompanyNo,
            BranchNo = config.BranchNo,
            ApiBaseUrl = config.ApiBaseUrl,
            UseWindowsAuth = config.UseWindowsAuth,
        };
    }
}