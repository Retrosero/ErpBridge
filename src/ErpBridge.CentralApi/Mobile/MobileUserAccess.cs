using System.Security.Claims;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Mobile;

/// <summary>Result of re-validating a mobile user token against current state.</summary>
public sealed record MobileUserAccessResult(Tenant? Tenant, MobileUser? User, int StatusCode, string? ErrorCode, string? Message)
{
    public bool Allowed => ErrorCode is null;
}

/// <summary>
/// A mobile user token only proves who signed in and when. Whether that person
/// may still work is decided here, from the database, on every call: a disabled
/// or deleted user, a blocked device, a disabled tenant or an expired
/// subscription loses access at once instead of when the 30-day token expires.
/// Used by the account endpoints (for precise error codes) and by
/// <see cref="MobileUserStateHandler"/> (for every data and document endpoint).
/// </summary>
public static class MobileUserAccess
{
    public static bool IsMobileUser(ClaimsPrincipal user) =>
        user.HasClaim(CentralApiClaims.Scope, CentralApiClaims.MobileUserScope);

    public static async Task<MobileUserAccessResult> CheckAsync(ClaimsPrincipal principal, CentralApiDbContext db, CancellationToken ct)
    {
        if (!Guid.TryParse(principal.FindFirstValue("sub"), out var userId)
            || !principal.TryGetTenantId(out var tenantId)
            || principal.FindFirstValue(CentralApiClaims.DeviceId) is not { Length: > 0 } deviceId)
            return Deny(401, "INVALID_TOKEN", "Mobile user token is malformed.");

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        var user = await db.MobileUsers.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId && u.DeletedAtUtc == null, ct);
        if (tenant is null || user is null) return Deny(401, "SESSION_REVOKED", "The user no longer exists.");
        if (!tenant.IsActive) return Deny(403, "TENANT_INACTIVE", "The company account is disabled.");
        if (!user.IsActive) return Deny(403, "USER_INACTIVE", "This user is disabled.");

        var deviceActive = await db.MobileDevices.AsNoTracking()
            .AnyAsync(d => d.TenantId == tenantId && d.DeviceId == deviceId && d.IsActive, ct);
        if (!deviceActive) return Deny(403, "DEVICE_REVOKED", "This device has been blocked for the company.");

        var subscription = await db.TenantSubscriptions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.IsCurrent, ct);
        var status = MobileSeatService.SubscriptionStatus(subscription, DateTimeOffset.UtcNow);
        if (!MobileSeatService.AllowsWork(status))
            return Deny(403, status == "none" ? "SUBSCRIPTION_REQUIRED" : "SUBSCRIPTION_EXPIRED", "The company has no active subscription.");

        return new MobileUserAccessResult(tenant, user, 200, null, null);
    }

    private static MobileUserAccessResult Deny(int status, string code, string message) => new(null, null, status, code, message);
}
