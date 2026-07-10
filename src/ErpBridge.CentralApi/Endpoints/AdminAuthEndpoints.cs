using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/admin/auth</c>: login + logout. Login verifies the bcrypt
/// password hash and mints a JWT carrying <c>scope=admin</c>. Logout is a
/// stateless no-op — the client drops the token. A future server-side blocklist
/// would slot in here without changing the call surface.
/// </summary>
public static class AdminAuthEndpoints
{
    /// <summary>Register an <see cref="IEndpointRouteBuilder"/> extension that maps both endpoints.</summary>
    public static IEndpointRouteBuilder MapAdminAuthEndpoints(this IEndpointRouteBuilder routes)
    {
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
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] AdminLoginRequest body,
        [FromServices] CentralApiDbContext db,
        [FromServices] Authentication.IJwtIssuer jwt,
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
        return JsonResults.Ok(new AdminLoginResponse
        {
            Token = issued.Token,
            AdminId = admin.Id,
            Email = admin.Email,
            DisplayName = admin.DisplayName,
            ExpiresAtUtc = issued.ExpiresAtUtc,
        });
    }

    /// <summary>Stateless logout — client-side token drop is the contract.</summary>
    private static IResult LogoutAsync() => Results.NoContent();
}
