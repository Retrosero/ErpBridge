using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ErpBridge.CentralApi.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ErpBridge.CentralApi.Authentication;

/// <summary>
/// Result of a successful <see cref="IJwtIssuer.IssueAsync"/>. The token
/// string is the raw JWT; the rest are the claims it carries, for callers
/// who want to log or audit.
/// </summary>
public sealed record IssuedToken(string Token, Guid AgentId, Guid TenantId, DateTimeOffset ExpiresAtUtc);

/// <summary>Result of an admin token issuance; mirrors <see cref="IssuedToken"/> for the admin flow.</summary>
public sealed record IssuedAdminToken(string Token, Guid AdminId, DateTimeOffset ExpiresAtUtc);
public sealed record IssuedMobileToken(string Token, Guid DeviceId, Guid TenantId, DateTimeOffset ExpiresAtUtc);

/// <summary>
/// Mints HS256 JWTs for registered agents and admins. Signing/validation keys
/// live in <see cref="JwtOptions"/>. Agent tokens carry
/// <c>sub=agentId</c>, <c>tenant=tenantId</c>, and <c>scope=agent</c>.
/// Admin tokens carry <c>sub=adminId</c> and <c>scope=admin</c>.
/// </summary>
public interface IJwtIssuer
{
    /// <summary>Issue a token for a freshly registered agent.</summary>
    IssuedToken Issue(Guid agentId, Guid tenantId);

    /// <summary>Issue a token for an admin.</summary>
    IssuedAdminToken IssueForAdmin(Guid adminId);
    IssuedMobileToken IssueForMobile(Guid deviceId, Guid tenantId);

    /// <summary>Validate a token. Returns <c>null</c> when invalid/expired.</summary>
    ClaimsPrincipal? Validate(string token);
}

/// <summary>
/// Default <see cref="IJwtIssuer"/>. Pulls the signing key from
/// <see cref="IOptionsMonitor{JwtOptions}"/> so test fixtures can replace
/// options in-place without rebuilding the host.
/// </summary>
public sealed class JwtIssuer : IJwtIssuer
{
    private readonly IOptionsMonitor<JwtOptions> _options;

    public JwtIssuer(IOptionsMonitor<JwtOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public IssuedToken Issue(Guid agentId, Guid tenantId)
    {
        var opts = _options.CurrentValue;
        var keyBytes = EnsureKey(opts);
        var expires = DateTimeOffset.UtcNow.AddMinutes(opts.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, agentId.ToString()),
            new Claim("tenant", tenantId.ToString()),
            new Claim("scope", "agent"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: opts.Issuer,
            audience: opts.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires.UtcDateTime,
            signingCredentials: creds);

        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return new IssuedToken(serialized, agentId, tenantId, expires);
    }

    /// <inheritdoc />
    public IssuedAdminToken IssueForAdmin(Guid adminId)
    {
        var opts = _options.CurrentValue;
        var keyBytes = EnsureKey(opts);
        var expires = DateTimeOffset.UtcNow.AddMinutes(opts.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, adminId.ToString()),
            new Claim("scope", "admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: opts.Issuer,
            audience: opts.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires.UtcDateTime,
            signingCredentials: creds);

        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return new IssuedAdminToken(serialized, adminId, expires);
    }

    public IssuedMobileToken IssueForMobile(Guid deviceId, Guid tenantId)
    {
        var opts = _options.CurrentValue; var keyBytes = EnsureKey(opts);
        var expires = DateTimeOffset.UtcNow.AddDays(7);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, deviceId.ToString()), new Claim("tenant", tenantId.ToString()), new Claim("scope", "mobile"), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")) };
        var token = new JwtSecurityToken(opts.Issuer, opts.Audience, claims, DateTime.UtcNow, expires.UtcDateTime,
            new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256));
        return new IssuedMobileToken(new JwtSecurityTokenHandler().WriteToken(token), deviceId, tenantId, expires);
    }

    private static byte[] EnsureKey(JwtOptions opts)
    {
        var keyBytes = Encoding.UTF8.GetBytes(opts.SigningKey);
        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 bytes long for HS256.");
        }
        return keyBytes;
    }

    /// <inheritdoc />
    public ClaimsPrincipal? Validate(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var opts = _options.CurrentValue;
        var keyBytes = Encoding.UTF8.GetBytes(opts.SigningKey);
        if (keyBytes.Length < 32)
        {
            return null;
        }

        // Validation can throw for a variety of reasons (bad signature, wrong
        // issuer, wrong audience, expired, ...). Treat all of them as
        // "invalid token" and return null so the caller can produce a 401
        // without leaking which specific check failed.
        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, BuildValidationParameters(opts, keyBytes), out _);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static TokenValidationParameters BuildValidationParameters(JwtOptions opts, byte[] keyBytes) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = opts.Issuer,
        ValidateAudience = true,
        ValidAudience = opts.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
    };
}
