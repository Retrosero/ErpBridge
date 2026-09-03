using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Authentication;

/// <summary>
/// Authenticates the caller against a tenant-scoped API key. The raw key is
/// pulled from <c>Authorization: Bearer AK-...</c>; the tenant id from the
/// configured header (default <c>X-Tenant-Id</c>). On success the handler
/// emits a principal with <c>sub=apiKeyId</c>, <c>tenant=tenantId</c>, and
/// <c>scope=apikey</c> — the same shape the JWT handler uses, which keeps
/// downstream authorization code uniform.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    /// <summary>The scheme name used in <c>AddAuthentication().AddScheme&lt;...,&gt;("ApiKey", ...)</c>.</summary>
    public const string SchemeName = "ApiKey";

    /// <summary>Prefix every emitted API key carries (matches <see cref="Domain.ApiKey.KeyPrefix"/>).</summary>
    public const string KeyPrefix = "AK-";

    private readonly CentralApiDbContext _db;
    private readonly ApiKeyUsageTracker _usageTracker;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        CentralApiDbContext db,
        ApiKeyUsageTracker usageTracker)
        : base(options, logger, encoder)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _usageTracker = usageTracker ?? throw new ArgumentNullException(nameof(usageTracker));
    }

    /// <summary>
    /// True when the request is being authenticated under the API-key scheme.
    /// We use this from <see cref="IngestEndpoints"/> to gate the
    /// "tenant must match the token" check without re-parsing headers.
    /// </summary>
    public static bool IsApiKeyScheme(AuthenticationSchemeProvider schemes, string schemeName) =>
        string.Equals(schemeName, SchemeName, StringComparison.Ordinal);

    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Step 1: pull and validate the bearer token.
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();

        var raw = authHeader.ToString();
        if (string.IsNullOrWhiteSpace(raw) || !raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = raw.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(token) || !token.StartsWith(KeyPrefix, StringComparison.Ordinal))
            return AuthenticateResult.Fail("Invalid API key format.");

        // Step 2: require a tenant hint. It may be either a full tenant GUID
        // or its first eight hexadecimal characters; it is verified after the
        // API key resolves the authenticated tenant.
        if (!Request.Headers.TryGetValue(Options.TenantHeaderName, out var tenantHeader)
            || string.IsNullOrWhiteSpace(tenantHeader.ToString()))
        {
            return AuthenticateResult.Fail($"Missing or invalid {Options.TenantHeaderName} header.");
        }

        // Step 3: hash the raw token and look it up. Hashes use a per-row
        // salt, so they cannot be indexed directly. Every issued key carries
        // an indexed, non-secret 11-character lookup prefix; use it to bound
        // candidate rows rather than loading every active key in the system.
        // A prefix collision only adds another constant-time hash comparison.
        var lookupPrefix = token[..Math.Min(token.Length, KeyPrefix.Length + 8)];
        var saltAndHash = await _db.ApiKeys.AsNoTracking()
            .Where(k => k.IsActive && k.KeyPrefix == lookupPrefix)
            .Select(k => new
            {
                k.Id,
                k.KeySalt,
                k.KeyHash,
                k.Scopes,
                k.ExpiresAtUtc,
                k.TenantId,
                TenantIsActive = k.Tenant!.IsActive,
            })
            .ToListAsync(Context.RequestAborted);

        foreach (var row in saltAndHash)
        {
            if (row.ExpiresAtUtc is { } exp && exp <= DateTimeOffset.UtcNow)
                continue;
            var computed = ComputeHash(row.KeySalt, token);
            if (CryptographicOperations.FixedTimeEquals(computed, row.KeyHash))
            {
                if (!TenantHeaderMatches(tenantHeader.ToString(), row.TenantId))
                    return AuthenticateResult.Fail($"Missing or invalid {Options.TenantHeaderName} header.");

                // Found it — also verify the tenant itself is still active.
                // Defensive: ApiKeyAuth shouldn't be the only line of defense,
                // but mirroring the JWT path keeps the security model uniform.
                if (!row.TenantIsActive)
                    return AuthenticateResult.Fail("Tenant is inactive.");

                var claims = new[]
                {
                    new Claim(ApiKeyClaims.ApiKeyId, row.Id.ToString()),
                    new Claim(ApiKeyClaims.TenantId, row.TenantId.ToString()),
                    new Claim(ApiKeyClaims.Scope, "apikey"),
                };
                var identity = new ClaimsIdentity(claims, SchemeName);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, SchemeName);

                _usageTracker.Record(row.Id, DateTimeOffset.UtcNow);

                return AuthenticateResult.Success(ticket);
            }
        }

        return AuthenticateResult.Fail("API key not recognised.");
    }

    private static bool TenantHeaderMatches(string suppliedTenantId, Guid authenticatedTenantId)
    {
        var supplied = suppliedTenantId.Trim();
        if (Guid.TryParse(supplied, out var fullTenantId))
            return fullTenantId == authenticatedTenantId;

        return supplied.Length == 8
            && supplied.All(Uri.IsHexDigit)
            && authenticatedTenantId.ToString("N").StartsWith(supplied, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // RFC 6750 says a 401 from a Bearer scheme must include
        // WWW-Authenticate: Bearer error="...". Keep it terse on failure.
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers["WWW-Authenticate"] = $"Bearer realm=\"ErpBridge\", scheme=\"ApiKey\"";
        return Task.CompletedTask;
    }

    /// <summary>
    /// SHA-256 over <c>salt || rawKey</c>. Salt is stored per-row so a DB
    /// leak alone is insufficient to brute-force the raw value offline.
    /// </summary>
    public static byte[] ComputeHash(byte[] salt, string rawKey)
    {
        var input = new byte[salt.Length + System.Text.Encoding.UTF8.GetByteCount(rawKey)];
        Buffer.BlockCopy(salt, 0, input, 0, salt.Length);
        var written = System.Text.Encoding.UTF8.GetBytes(rawKey, 0, rawKey.Length, input, salt.Length);
        return SHA256.HashData(input.AsSpan(0, salt.Length + written));
    }
}
