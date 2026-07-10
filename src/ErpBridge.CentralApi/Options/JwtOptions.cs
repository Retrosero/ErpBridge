namespace ErpBridge.CentralApi.Options;

/// <summary>
/// Configuration bound from <c>Jwt</c> section of appsettings. Used by the
/// central API to mint and validate agent JWTs. <see cref="SigningKey"/> must
/// be at least 32 bytes long; HS256 mandates it.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>JWT <c>iss</c> claim — typically <c>ErpBridge.CentralApi</c>.</summary>
    public string Issuer { get; set; } = "ErpBridge.CentralApi";

    /// <summary>JWT <c>aud</c> claim — typically <c>ErpBridge.Agents</c>.</summary>
    public string Audience { get; set; } = "ErpBridge.Agents";

    /// <summary>HMAC-SHA256 signing key. Must be at least 32 bytes long.</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Lifetime of the issued access token in minutes.</summary>
    public int AccessTokenMinutes { get; set; } = 60;
}
