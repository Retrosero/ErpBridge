using System.Security.Cryptography;
using System.Text;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

public static class MobileLicensingEndpoints
{
    public static IEndpointRouteBuilder MapMobileLicensingEndpoints(this IEndpointRouteBuilder routes)
    {
        var mobile = routes.MapGroup("/api/v1/mobile").WithTags("Mobile Licensing").RequireRateLimiting(Program.AnonymousRateLimitPolicy);
        mobile.MapPost("/activate", ActivateAsync);
        mobile.MapPost("/migrate", MigrateAsync).RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        mobile.MapPost("/renew", RenewAsync).RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        var admin = routes.MapGroup("/api/v1/admin/mobile-devices").WithTags("Admin/Mobile Devices").RequireAuthorization(Program.AdminPolicy).RequireRateLimiting(Program.PerAdminRateLimitPolicy);
        admin.MapGet("/{tenantId:guid}", ListAsync);
        admin.MapPost("/activation-codes", CreateCodeAsync);
        admin.MapPost("/{id:guid}/revoke", RevokeAsync);
        return routes;
    }

    private static async Task<IResult> ActivateAsync(MobileActivateRequest request, CentralApiDbContext db, IJwtIssuer jwt, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.InstallationId)) return Error("INVALID_ACTIVATION", "Activation code and device identity are required.");
        var hash = Hash(request.Code.Trim()); var now = DateTimeOffset.UtcNow;
        var code = await db.DeviceActivationCodes.Include(x => x.Tenant).FirstOrDefaultAsync(x => x.CodeHash == hash, ct);
        if (code is null || code.ConsumedAtUtc is not null || code.ExpiresAtUtc <= now || code.Tenant is not { IsActive: true }) return Error("INVALID_ACTIVATION", "Activation code is invalid or expired.");
        var existing = await db.MobileDevices.FirstOrDefaultAsync(x => x.TenantId == code.TenantId && x.InstallationId == request.InstallationId.Trim(), ct);
        if (existing is null)
        {
            var activeCount = await db.MobileDevices.CountAsync(x => x.TenantId == code.TenantId && x.IsActive, ct);
            if (activeCount >= code.Tenant.DeviceSeatLimit) return Error("DEVICE_LIMIT_REACHED", "The company device limit has been reached. Remove an existing device first.", StatusCodes.Status409Conflict);
            existing = new MobileDevice { TenantId = code.TenantId, InstallationId = request.InstallationId.Trim(), DisplayName = string.IsNullOrWhiteSpace(request.DeviceName) ? "Android cihaz" : request.DeviceName.Trim(), Platform = "android", AppVersion = request.AppVersion, ActivatedAtUtc = now, LastSeenAtUtc = now };
            db.MobileDevices.Add(existing);
        }
        else { existing.IsActive = true; existing.RevokedAtUtc = null; existing.LastSeenAtUtc = now; existing.AppVersion = request.AppVersion ?? existing.AppVersion; }
        code.ConsumedAtUtc = now; code.DeviceId = existing.Id; await db.SaveChangesAsync(ct);
        var token = jwt.IssueForMobile(existing.Id, code.TenantId);
        return JsonResults.Ok(new MobileSessionDto { Token = token.Token, TenantId = token.TenantId, DeviceId = token.DeviceId, ExpiresAtUtc = token.ExpiresAtUtc });
    }

    private static async Task<IResult> RenewAsync(HttpContext http, CentralApiDbContext db, IJwtIssuer jwt, CancellationToken ct)
    {
        if (!TryDevice(http, out var deviceId, out var tenantId)) return Error("INVALID_DEVICE_TOKEN", "Device token is invalid.", StatusCodes.Status401Unauthorized);
        var device = await db.MobileDevices.Include(x => x.Tenant).FirstOrDefaultAsync(x => x.Id == deviceId && x.TenantId == tenantId, ct);
        if (device is null || !device.IsActive || device.Tenant is not { IsActive: true }) return Error("DEVICE_REVOKED", "This device is no longer licensed.", StatusCodes.Status403Forbidden);
        device.LastSeenAtUtc = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); var token = jwt.IssueForMobile(device.Id, tenantId);
        return JsonResults.Ok(new MobileSessionDto { Token = token.Token, TenantId = tenantId, DeviceId = device.Id, ExpiresAtUtc = token.ExpiresAtUtc });
    }

    /// <summary>One-time bridge for installations that already possess a tenant mobile API key.</summary>
    private static async Task<IResult> MigrateAsync(MobileActivateRequest request, HttpContext http, CentralApiDbContext db, IJwtIssuer jwt, CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId) || string.IsNullOrWhiteSpace(request.InstallationId)) return Error("INVALID_MIGRATION", "Device identity is required.", StatusCodes.Status401Unauthorized);
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId && x.IsActive, ct);
        if (tenant is null) return Error("TENANT_NOT_FOUND", "Tenant not found.", StatusCodes.Status404NotFound);
        var now = DateTimeOffset.UtcNow;
        var device = await db.MobileDevices.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.InstallationId == request.InstallationId.Trim(), ct);
        if (device is null)
        {
            var active = await db.MobileDevices.CountAsync(x => x.TenantId == tenantId && x.IsActive, ct);
            if (active >= tenant.DeviceSeatLimit) return Error("DEVICE_LIMIT_REACHED", "The company device limit has been reached. Remove an existing device first.", StatusCodes.Status409Conflict);
            device = new MobileDevice { TenantId = tenantId, InstallationId = request.InstallationId.Trim(), DisplayName = string.IsNullOrWhiteSpace(request.DeviceName) ? "Android cihaz" : request.DeviceName.Trim(), Platform = "android", AppVersion = request.AppVersion, ActivatedAtUtc = now, LastSeenAtUtc = now };
            db.MobileDevices.Add(device);
        }
        else { device.IsActive = true; device.RevokedAtUtc = null; device.LastSeenAtUtc = now; device.AppVersion = request.AppVersion ?? device.AppVersion; }
        await db.SaveChangesAsync(ct); var token = jwt.IssueForMobile(device.Id, tenantId);
        return JsonResults.Ok(new MobileSessionDto { Token = token.Token, TenantId = tenantId, DeviceId = device.Id, ExpiresAtUtc = token.ExpiresAtUtc });
    }

    private static async Task<IResult> ListAsync(Guid tenantId, CentralApiDbContext db, CancellationToken ct) => JsonResults.Ok((await db.MobileDevices.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.LastSeenAtUtc).ToListAsync(ct)).Select(ToDto).ToArray());
    private static async Task<IResult> CreateCodeAsync(CreateDeviceActivationCodeRequest request, CentralApiDbContext db, CancellationToken ct)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == request.TenantId && x.IsActive, ct); if (tenant is null) return Error("TENANT_NOT_FOUND", "Tenant not found.", StatusCodes.Status404NotFound);
        var raw = "ERP-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(8)); var expires = DateTimeOffset.UtcNow.AddMinutes(15);
        db.DeviceActivationCodes.Add(new DeviceActivationCode { TenantId = tenant.Id, CodeHash = Hash(raw), ExpiresAtUtc = expires }); await db.SaveChangesAsync(ct);
        return JsonResults.Status(StatusCodes.Status201Created, new DeviceActivationCodeDto { Code = raw, ExpiresAtUtc = expires });
    }
    private static async Task<IResult> RevokeAsync(Guid id, CentralApiDbContext db, CancellationToken ct) { var d = await db.MobileDevices.FindAsync([id], ct); if (d is null) return Error("DEVICE_NOT_FOUND", "Device not found.", 404); d.IsActive = false; d.RevokedAtUtc = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return Results.NoContent(); }
    private static MobileDeviceDto ToDto(MobileDevice x) => new() { Id = x.Id, TenantId = x.TenantId, DisplayName = x.DisplayName, InstallationId = x.InstallationId, AppVersion = x.AppVersion, ActivatedAtUtc = x.ActivatedAtUtc, LastSeenAtUtc = x.LastSeenAtUtc, IsActive = x.IsActive };
    internal static bool TryDevice(HttpContext c, out Guid id, out Guid tenant) { id = Guid.Empty; tenant = Guid.Empty; return c.User.HasClaim("scope", "mobile") && Guid.TryParse(c.User.FindFirst("sub")?.Value, out id) && Guid.TryParse(c.User.FindFirst("tenant")?.Value, out tenant); }
    private static string Hash(string v) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(v))).ToLowerInvariant();
    private static IResult Error(string code, string message, int status = 400) => JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}
