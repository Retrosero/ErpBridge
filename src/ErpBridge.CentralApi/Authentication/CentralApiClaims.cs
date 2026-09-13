using System.Security.Claims;

namespace ErpBridge.CentralApi.Authentication;

/// <summary>
/// Constants for JWT claims used by the central API. The
/// <see cref="Issuer"/> writes these claims at registration time; the
/// controllers read them back to enforce tenant isolation.
/// </summary>
public static class CentralApiClaims
{
    /// <summary>The agent id claim (Jwt <c>sub</c>).</summary>
    public const string AgentId = "sub";

    /// <summary>The tenant id claim. Used to scope every authenticated query.</summary>
    public const string TenantId = "tenant";

    /// <summary>The scope claim: <c>agent</c>, <c>admin</c>, <c>apikey</c> or <c>mobile-user</c>.</summary>
    public const string Scope = "scope";

    /// <summary>Scope value of a mobile app user token.</summary>
    public const string MobileUserScope = "mobile-user";

    /// <summary>The installation id a mobile user token was issued to.</summary>
    public const string DeviceId = "device";

    /// <summary>Read the agent id claim from <paramref name="user"/>. Returns <c>false</c> when missing/malformed.</summary>
    public static bool TryGetAgentId(this ClaimsPrincipal user, out Guid agentId) =>
        Guid.TryParse(user.FindFirstValue(AgentId), out agentId);

    /// <summary>Read the tenant id claim from <paramref name="user"/>. Returns <c>false</c> when missing/malformed.</summary>
    public static bool TryGetTenantId(this ClaimsPrincipal user, out Guid tenantId) =>
        Guid.TryParse(user.FindFirstValue(TenantId), out tenantId);
}
