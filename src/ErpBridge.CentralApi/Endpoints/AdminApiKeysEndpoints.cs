using System.Security.Cryptography;
using System.Text;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/admin/api-keys</c>: list, create, revoke, rotate. Admin-only.
/// The raw key is returned exactly once at creation/rotation; subsequent
/// reads expose only the <c>KeyPrefix</c>.
/// </summary>
public static class AdminApiKeysEndpoints
{
    private const string MobileReadScope = "mobile:read";

    /// <summary>Random bytes appended to <see cref="ApiKeyAuthenticationHandler.KeyPrefix"/> for the raw value.</summary>
    private const int RawKeyRandomBytes = 24; // 48 hex chars after the AK- prefix

    /// <summary>Length of the <see cref="ApiKey.KeySalt"/> in bytes.</summary>
    private const int SaltBytes = 16;

    public static IEndpointRouteBuilder MapAdminApiKeysEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/api-keys")
            .WithTags("Admin/ApiKeys")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/", ListAsync)
            .WithName("AdminApiKeysList")
            .Produces<ApiKeyDto[]>(StatusCodes.Status200OK);

        group.MapPost("/", CreateAsync)
            .WithName("AdminApiKeysCreate")
            .Produces<ApiKeyCreatedDto>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/revoke", RevokeAsync)
            .WithName("AdminApiKeysRevoke")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/rotate", RotateAsync)
            .WithName("AdminApiKeysRotate")
            .Produces<ApiKeyCreatedDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        return routes;
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] Guid? tenantId,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var query = db.ApiKeys.AsNoTracking();
        if (tenantId.HasValue) query = query.Where(k => k.TenantId == tenantId.Value);
        var rows = await query.OrderByDescending(k => k.CreatedAtUtc).ToListAsync(ct);
        return JsonResults.Ok(rows.Select(ToDto).ToArray());
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateApiKeyRequest body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null || body.TenantId == Guid.Empty)
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_TENANT", Message = "tenantId is required." });
        if (string.IsNullOrWhiteSpace(body.Name))
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_NAME", Message = "name is required." });

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == body.TenantId, ct);
        if (tenant is null || !tenant.IsActive)
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "TENANT_NOT_FOUND", Message = "Tenant not found or inactive." });

        var (rawKey, salt, hash, prefix) = GenerateKey();
        var scopes = EnsureMobileReadScope(
            body.Scopes is { Length: > 0 } ? body.Scopes : new[] { "ingest:write" });

        var key = new ApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = body.TenantId,
            Name = body.Name.Trim(),
            KeyPrefix = prefix,
            KeyHash = hash,
            KeySalt = salt,
            Scopes = scopes,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = body.ExpiresAtUtc,
        };
        db.ApiKeys.Add(key);
        await db.SaveChangesAsync(ct);

        return JsonResults.Status(StatusCodes.Status201Created, new ApiKeyCreatedDto
        {
            Id = key.Id,
            TenantId = key.TenantId,
            Name = key.Name,
            KeyPrefix = prefix,
            RawKey = rawKey,
            Scopes = scopes,
            CreatedAtUtc = key.CreatedAtUtc,
            ExpiresAtUtc = key.ExpiresAtUtc,
        });
    }

    private static async Task<IResult> RevokeAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var key = await db.ApiKeys.FirstOrDefaultAsync(k => k.Id == id, ct);
        if (key is null)
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "API_KEY_NOT_FOUND", Message = "API key not found." });
        if (!key.IsActive) return Results.NoContent();

        key.IsActive = false;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RotateAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var key = await db.ApiKeys.FirstOrDefaultAsync(k => k.Id == id, ct);
        if (key is null)
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "API_KEY_NOT_FOUND", Message = "API key not found." });

        var (rawKey, salt, hash, prefix) = GenerateKey();
        key.KeyPrefix = prefix;
        key.KeyHash = hash;
        key.KeySalt = salt;
        key.Scopes = EnsureMobileReadScope(key.Scopes);
        key.IsActive = true;
        await db.SaveChangesAsync(ct);

        return JsonResults.Ok(new ApiKeyCreatedDto
        {
            Id = key.Id,
            TenantId = key.TenantId,
            Name = key.Name,
            KeyPrefix = prefix,
            RawKey = rawKey,
            Scopes = key.Scopes,
            CreatedAtUtc = key.CreatedAtUtc,
            ExpiresAtUtc = key.ExpiresAtUtc,
        });
    }

    /// <summary>
    /// Generates a new API key tuple: raw value (returned once), per-row salt,
    /// SHA-256(salt || raw), and a non-secret <c>AK-XXXXXXXX</c> prefix used
    /// for at-a-glance identification in the admin UI.
    /// </summary>
    private static (string RawKey, byte[] Salt, byte[] Hash, string Prefix) GenerateKey()
    {
        Span<byte> random = stackalloc byte[RawKeyRandomBytes];
        RandomNumberGenerator.Fill(random);
        var hex = Convert.ToHexString(random).ToLowerInvariant();
        var raw = ApiKeyAuthenticationHandler.KeyPrefix + hex;
        var prefix = raw.Substring(0, ApiKeyAuthenticationHandler.KeyPrefix.Length + 8);

        var salt = new byte[SaltBytes];
        RandomNumberGenerator.Fill(salt);
        var hash = ApiKeyAuthenticationHandler.ComputeHash(salt, raw);
        return (raw, salt, hash, prefix);
    }

    private static string[] EnsureMobileReadScope(IEnumerable<string>? scopes) =>
        (scopes ?? Array.Empty<string>())
            .Append(MobileReadScope)
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static ApiKeyDto ToDto(ApiKey k) => new()
    {
        Id = k.Id,
        TenantId = k.TenantId,
        Name = k.Name,
        KeyPrefix = k.KeyPrefix,
        Scopes = k.Scopes,
        IsActive = k.IsActive,
        CreatedAtUtc = k.CreatedAtUtc,
        ExpiresAtUtc = k.ExpiresAtUtc,
        LastUsedAtUtc = k.LastUsedAtUtc,
    };
}
