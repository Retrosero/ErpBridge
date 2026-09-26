using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Mobile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/android/account</c>: mobile user sign-in and the tenant
/// administrator's user management on the phone. Seat rules live in
/// <see cref="MobileSeatService"/>; this class only authenticates and maps.
/// </summary>
public static class MobileAccountEndpoints
{
    /// <summary>
    /// Verified against when the username or tenant is unknown, so a failed sign-in
    /// costs the same BCrypt work either way and does not reveal which part was wrong.
    /// </summary>
    private static readonly string DummyPasswordHash = BCrypt.Net.BCrypt.HashPassword("erpbridge-timing-equalizer");

    public static IEndpointRouteBuilder MapMobileAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        var anonymous = routes.MapGroup("/api/v1/android/account")
            .WithTags("Android/Account")
            .AllowAnonymous()
            .RequireRateLimiting(Program.AnonymousRateLimitPolicy);
        anonymous.MapPost("/login", LoginAsync).WithName("MobileAccountLogin")
            .Produces<MobileLoginResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);

        var signedIn = routes.MapGroup("/api/v1/android/account")
            .WithTags("Android/Account")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        signedIn.MapGet("/me", MeAsync).WithName("MobileAccountMe");
        signedIn.MapGet("/users", ListUsersAsync).WithName("MobileAccountUsersList");
        signedIn.MapPost("/users", CreateUserAsync).WithName("MobileAccountUsersCreate");
        signedIn.MapPatch("/users/{id:guid}", UpdateUserAsync).WithName("MobileAccountUsersUpdate");
        signedIn.MapDelete("/users/{id:guid}", DeleteUserAsync).WithName("MobileAccountUsersDelete");
        return routes;
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] MobileLoginRequest? body,
        [FromServices] CentralApiDbContext db,
        [FromServices] MobileSeatService seats,
        [FromServices] IJwtIssuer jwt,
        [FromServices] IConfiguration configuration,
        CancellationToken ct)
    {
        var tenantCode = body?.TenantCode?.Trim().ToUpperInvariant();
        var username = MobileSeatService.NormalizeUsername(body?.Username);
        var deviceId = body?.DeviceId?.Trim();
        if (string.IsNullOrEmpty(tenantCode) || string.IsNullOrEmpty(body?.Password) || string.IsNullOrEmpty(deviceId))
            return Error(400, "INVALID_REQUEST", "tenantCode, username, password and deviceId are required.");
        if (deviceId.Length > 128)
            return Error(400, "INVALID_DEVICE_ID", "deviceId must be at most 128 characters.");
        var client = string.IsNullOrWhiteSpace(body.Client) ? CentralApiClaims.PhoneClient : body.Client.Trim().ToLowerInvariant();
        if (client is not (CentralApiClaims.PhoneClient or CentralApiClaims.PortalClient))
            return Error(400, "INVALID_CLIENT", "client must be android or portal.");

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Code == tenantCode, ct);
        var user = tenant is null || username is null
            ? null
            : await db.MobileUsers.Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Username == username && u.DeletedAtUtc == null, ct);
        var passwordOk = BCrypt.Net.BCrypt.Verify(body.Password, user?.PasswordHash ?? DummyPasswordHash);
        if (tenant is null || user is null || !passwordOk)
            return Error(401, "INVALID_CREDENTIALS", "Company code, username or password is wrong.");

        // Only past this point does the caller know the credentials, so the
        // specific reasons below are safe to disclose.
        if (!tenant.IsActive) return Error(403, "TENANT_INACTIVE", "The company account is disabled.");
        if (!user.IsActive) return Error(403, "USER_INACTIVE", "This user is disabled.");
        // Before a device row or token exists: a warehouse-only user never gets a phone session.
        var loginPrincipal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
            [new System.Security.Claims.Claim(CentralApiClaims.Client, client)]));
        if (MobileUserAccess.ClientDenial(loginPrincipal, user, body.AppVersion, configuration[MobileUserAccess.MinWarehousePhoneVersionKey]) is { } denied)
            return Error(denied.StatusCode, denied.ErrorCode!, denied.Message!);
        var subscription = await seats.GetCurrentSubscriptionAsync(tenant.Id, ct);
        var status = MobileSeatService.SubscriptionStatus(subscription, DateTimeOffset.UtcNow);
        if (!MobileSeatService.AllowsWork(status))
            return Error(403, status == "none" ? "SUBSCRIPTION_REQUIRED" : "SUBSCRIPTION_EXPIRED", "The company has no active subscription.");

        var now = DateTimeOffset.UtcNow;
        var device = await db.MobileDevices.FirstOrDefaultAsync(d => d.TenantId == tenant.Id && d.DeviceId == deviceId, ct);
        if (device is { IsActive: false })
            return Error(403, "DEVICE_REVOKED", "This device has been blocked for the company.");
        if (device is null)
        {
            device = new MobileDevice { TenantId = tenant.Id, DeviceId = deviceId, FirstSeenAtUtc = now };
            db.MobileDevices.Add(device);
        }
        device.LastUserId = user.Id;
        device.AppVersion = Truncate(body.AppVersion, 64);
        device.LastSeenAtUtc = now;
        user.LastLoginAtUtc = now;
        await db.SaveChangesAsync(ct);

        // "Remember me" is a portal choice; a phone keeps its month-long token whatever it sends.
        TimeSpan? lifetime = client == CentralApiClaims.PortalClient && body.RememberMe != true
            ? TimeSpan.FromHours(JwtIssuer.PortalSessionHours)
            : null;
        var token = jwt.IssueForMobileUser(user.Id, tenant.Id, deviceId, client, lifetime);
        return JsonResults.Ok(new MobileLoginResponse
        {
            Token = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            Session = await SessionAsync(db, seats, tenant, user, ct),
        });
    }

    private static async Task<IResult> MeAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] MobileSeatService seats, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await SessionAsync(db, seats, access.Tenant!, access.User!, ct));
    }

    private static async Task<IResult> ListUsersAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] MobileSeatService seats, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        var users = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .Where(u => u.TenantId == access.Tenant!.Id && u.DeletedAtUtc == null)
            .OrderBy(u => u.Username)
            .ToListAsync(ct);
        return JsonResults.Ok(new MobileUserListResponse
        {
            Seats = await seats.GetUsageAsync(access.Tenant!.Id, ct),
            Users = users.Select(ToDto).ToArray(),
        });
    }

    private static async Task<IResult> CreateUserAsync(HttpContext http, [FromBody] CreateMobileUserRequest? body, [FromServices] CentralApiDbContext db, [FromServices] MobileSeatService seats, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        if (body is null) return Error(400, "INVALID_BODY", "Body required.");
        var result = await seats.CreateUserAsync(access.Tenant!.Id, body, ct, access.User!.Id);
        return result.Succeeded ? JsonResults.Status(StatusCodes.Status201Created, ToDto(result.Value!)) : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> UpdateUserAsync(HttpContext http, Guid id, [FromBody] UpdateMobileUserRequest? body, [FromServices] CentralApiDbContext db, [FromServices] MobileSeatService seats, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        if (body is null) return Error(400, "INVALID_BODY", "Body required.");
        var result = await seats.UpdateUserAsync(access.Tenant!.Id, id, body, ct, access.User!.Id);
        return result.Succeeded ? JsonResults.Ok(ToDto(result.Value!)) : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> DeleteUserAsync(HttpContext http, Guid id, [FromServices] CentralApiDbContext db, [FromServices] MobileSeatService seats, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        var result = await seats.DeleteUserAsync(access.Tenant!.Id, id, ct);
        return result.Succeeded ? Results.NoContent() : JsonResults.Status(result.StatusCode, result.Error);
    }

    /// <summary>
    /// Re-validates the token against current state (see <see cref="MobileUserAccess"/>)
    /// and, for user management, requires the administrator role — read from the
    /// database, not the token, so a demotion applies immediately.
    /// </summary>
    internal static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeAsync(
        HttpContext http, CentralApiDbContext db, bool requireAdmin, CancellationToken ct)
    {
        var access = await MobileUserAccess.CheckAsync(http.User, db, ct, MobileUserAccess.MinWarehousePhoneVersion(http.RequestServices));
        if (!access.Allowed) return (null, null, Error(access.StatusCode, access.ErrorCode!, access.Message!));
        if (requireAdmin && !RolePermissions.CanManageUsers(access.User!))
            return (null, null, Error(403, "ADMIN_REQUIRED", "Only company administrators can manage users."));
        return (access.Tenant, access.User, null);
    }

    private static async Task<MobileSessionDto> SessionAsync(CentralApiDbContext db, MobileSeatService seats, Tenant tenant, MobileUser user, CancellationToken ct) => new()
    {
        User = ToDto(user),
        TenantId = tenant.Id,
        TenantName = tenant.Name,
        TenantCode = tenant.Code,
        Seats = await seats.GetUsageAsync(tenant.Id, ct),
        DataSource = tenant.DataSource,
        ApprovalRules = (await ErpBridge.CentralApi.Approvals.ApprovalService.RulesAsync(db, tenant.Id, ct)).ToMap(),
        Modules = await MobileXmlFeedEndpoints.ModulesAsync(db, tenant.Id, ct),
    };

    internal static MobileUserDto ToDto(MobileUser u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        FullName = u.FullName,
        Role = MobileUserRoles.Legacy(RolePermissions.Of(u)),
        Roles = MobileUserRoles.All.Where(RolePermissions.Of(u).Contains).ToArray(),
        CanApprove = ApprovalPermissions.CanDecide(u),
        CanManageApprovalRules = ApprovalPermissions.CanManageRules(u),
        IsActive = u.IsActive,
        CreatedAtUtc = u.CreatedAtUtc,
        LastLoginAtUtc = u.LastLoginAtUtc,
    };

    private static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });

    private static string? Truncate(string? value, int max)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed.Length <= max ? trimmed : trimmed[..max];
    }
}
