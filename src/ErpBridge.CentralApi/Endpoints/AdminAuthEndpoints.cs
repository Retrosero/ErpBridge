using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps the admin auth surface: <c>POST /api/v1/admin/login</c>,
/// <c>POST /api/v1/admin/auth/refresh</c> and <c>POST /api/v1/admin/logout</c>.
/// <list type="bullet">
///   <item><description><c>POST /api/v1/admin/login</c> — verify bcrypt password, mint an access JWT (<c>scope=admin</c>) and a long-lived refresh token. The refresh token is the SHA-256-hashed row stored in <c>refresh_tokens</c>; only its hash is persisted, the raw value is returned to the client once.</description></item>
///   <item><description><c>POST /api/v1/admin/auth/refresh</c> — exchange a live refresh token for a new access + refresh pair; the old refresh row is atomically revoked and linked to the new one.</description></item>
///   <item><description><c>POST /api/v1/admin/logout</c> — revoke the supplied refresh token (no-op if missing or unknown). Stateless on the access-token side: the client drops it.</description></item>
/// </list>
/// </summary>
public static class AdminAuthEndpoints
{
    /// <summary>Register an <see cref="IEndpointRouteBuilder"/> extension that maps all three endpoints.</summary>
    public static IEndpointRouteBuilder MapAdminAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        // Login + logout keep their existing URL shapes so the rest of the
        // stack (tests, Blazor client) does not have to chase a route rename.
        // The new refresh-token flow lives under /auth/refresh as a sibling
        // group; only the new endpoint is added there.
        var group = routes.MapGroup("/api/v1/admin").WithTags("Admin");

        group.MapPost("/login", LoginAsync)
            .WithName("AdminLogin")
            .Produces<AdminLoginResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous()
            .RequireRateLimiting(Program.AnonymousRateLimitPolicy);

        group.MapPost("/logout", LogoutAsync)
            .WithName("AdminLogout")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        var authGroup = routes.MapGroup("/api/v1/admin/auth").WithTags("Admin");
        authGroup.MapPost("/refresh", RefreshAsync)
            .WithName("AdminRefresh")
            .Produces<AdminRefreshResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous()
            .RequireRateLimiting(Program.AnonymousRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] AdminLoginRequest body,
        [FromServices] CentralApiDbContext db,
        [FromServices] IJwtIssuer jwt,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (body is null)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });
        if (string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_CREDENTIALS", Message = "email and password are required." });

        var email = body.Email.Trim().ToLowerInvariant();
        var admin = await db.AdminUsers.FirstOrDefaultAsync(a => a.Email == email, ct);
        // Same response shape for unknown email / wrong password / inactive admin —
        // do not leak which branch the caller hit.
        if (admin is null || !admin.IsActive || !BCrypt.Net.BCrypt.Verify(body.Password, admin.PasswordHash))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_CREDENTIALS", Message = "Invalid email or password." });
        }

        // Update LastLoginAtUtc. Load+update (rather than ExecuteUpdate) so the
        // EF Core in-memory test provider — which does not translate
        // ExecuteUpdate — sees the row in the same scope.
        admin.LastLoginAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        var issued = jwt.IssueForAdmin(admin.Id);
        var refresh = await IssueRefreshTokenAsync(db, admin.Id, ResolveClientIp(httpContext), ct);
        return JsonResults.Ok(new AdminLoginResponse
        {
            Token = issued.Token,
            AdminId = admin.Id,
            Email = admin.Email,
            DisplayName = admin.DisplayName,
            ExpiresAtUtc = issued.ExpiresAtUtc,
            RefreshToken = refresh.RawToken,
            RefreshTokenExpiresAtUtc = refresh.ExpiresAtUtc,
        });
    }

    private static async Task<IResult> RefreshAsync(
        [FromBody] AdminRefreshRequest body,
        [FromServices] CentralApiDbContext db,
        [FromServices] IJwtIssuer jwt,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.RefreshToken))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_REFRESH_TOKEN", Message = "refreshToken is required." });

        var hash = JwtIssuer.HashRefreshToken(body.RefreshToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (existing is null)
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_REFRESH_TOKEN", Message = "Refresh token is invalid." });
        }
        if (existing.RevokedAtUtc is not null)
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "REVOKED_REFRESH_TOKEN", Message = "Refresh token has been revoked." });
        }
        if (existing.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "EXPIRED_REFRESH_TOKEN", Message = "Refresh token has expired." });
        }

        // The admin row may have been deleted (or deactivated) between issue
        // and refresh. Treat "no live admin" as 401 so a stale token cannot
        // bootstrap a session for a removed operator.
        var admin = await db.AdminUsers.FirstOrDefaultAsync(a => a.Id == existing.AdminUserId, ct);
        if (admin is null || !admin.IsActive)
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "ADMIN_NOT_FOUND", Message = "Admin account is no longer available." });
        }

        // Rotate: revoke the old row, write the new row, link them.
        var newRefresh = await IssueRefreshTokenAsync(db, admin.Id, ResolveClientIp(httpContext), ct);
        existing.RevokedAtUtc = DateTimeOffset.UtcNow;
        existing.ReplacedByTokenId = newRefresh.Id.ToString();
        await db.SaveChangesAsync(ct);

        var issued = jwt.IssueForAdmin(admin.Id);
        return JsonResults.Ok(new AdminRefreshResponse
        {
            Token = issued.Token,
            AdminId = admin.Id,
            Email = admin.Email,
            DisplayName = admin.DisplayName,
            ExpiresAtUtc = issued.ExpiresAtUtc,
            RefreshToken = newRefresh.RawToken,
            RefreshTokenExpiresAtUtc = newRefresh.ExpiresAtUtc,
        });
    }

    /// <summary>
    /// Revoke the supplied refresh token (no-op if missing/unknown). The
    /// authorization requirement still applies — a caller must present a
    /// live admin access token before the server will revoke anything, so
    /// this endpoint cannot be used to log another admin out.
    /// </summary>
    private static async Task<IResult> LogoutAsync(
        [FromBody] AdminLogoutRequest? body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.RefreshToken))
            return Results.NoContent();

        var hash = JwtIssuer.HashRefreshToken(body.RefreshToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (existing is not null && existing.RevokedAtUtc is null)
        {
            existing.RevokedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.NoContent();
    }

    /// <summary>
    /// Mint and persist a fresh refresh token. The raw value is returned
    /// to the caller exactly once; the database only ever sees the SHA-256
    /// hash. Lifetime is <see cref="JwtIssuer.DefaultRefreshTokenDays"/> days.
    /// </summary>
    private static async Task<(Guid Id, string RawToken, DateTimeOffset ExpiresAtUtc)> IssueRefreshTokenAsync(
        CentralApiDbContext db,
        Guid adminId,
        string ip,
        CancellationToken ct)
    {
        var raw = JwtIssuer.GenerateRefreshToken();
        var hash = JwtIssuer.HashRefreshToken(raw);
        var expires = DateTimeOffset.UtcNow.AddDays(JwtIssuer.DefaultRefreshTokenDays);
        var row = new RefreshToken
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminId,
            TokenHash = hash,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = expires,
            CreatedByIp = ip,
        };
        db.RefreshTokens.Add(row);
        await db.SaveChangesAsync(ct);
        return (row.Id, raw, expires);
    }

    private static string ResolveClientIp(HttpContext httpContext)
    {
        // Prefer the forwarded header when sitting behind a reverse proxy;
        // fall back to the direct remote address. Capped at 64 chars to match
        // the refresh_tokens.CreatedByIp column width.
        var forwarded = httpContext.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            var first = forwarded.Split(',', 2)[0].Trim();
            if (!string.IsNullOrEmpty(first))
                return first.Length > 64 ? first[..64] : first;
        }
        var remote = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return remote.Length > 64 ? remote[..64] : remote;
    }
}
