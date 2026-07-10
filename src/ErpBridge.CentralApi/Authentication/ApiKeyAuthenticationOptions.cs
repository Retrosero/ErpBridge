using Microsoft.AspNetCore.Authentication;

namespace ErpBridge.CentralApi.Authentication;

/// <summary>
/// Configuration for the API-key authentication scheme. Bound from the
/// <c>ApiKey</c> config section. Today the only tunable is the header name
/// for the tenant id; defaults match the public spec in api-contracts.md.
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>Header carrying the tenant id (defaults to <c>X-Tenant-Id</c>).</summary>
    public string TenantHeaderName { get; set; } = "X-Tenant-Id";
}

/// <summary>
/// Names and claim keys used by the API-key authentication scheme. Kept
/// separate from <see cref="CentralApiClaims"/> so a future single-sign-on
/// scheme can claim <c>tenant</c> independently.
/// </summary>
public static class ApiKeyClaims
{
    /// <summary>The API key id (stored in <c>sub</c>).</summary>
    public const string ApiKeyId = "sub";

    /// <summary>The tenant id claim, parallel to the JWT variant.</summary>
    public const string TenantId = "tenant";

    /// <summary>Scope claim; today always <c>"apikey"</c>.</summary>
    public const string Scope = "scope";

    /// <summary>Authorization policy that requires <c>scope=apikey</c>.</summary>
    public const string Policy = "ApiKey";
}