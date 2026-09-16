using System.Security.Claims;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    /// <summary>
    /// Configuration key: the first phone app version (<c>versionName</c>, e.g. <c>1.5.240</c>) that
    /// shows a warehouse-only user nothing but the warehouse screen. Until it is set, such a user
    /// cannot use the phone at all — an older build would open every screen (panel goal D1).
    /// </summary>
    public const string MinWarehousePhoneVersionKey = "Mobile:MinWarehousePhoneVersion";

    public static string? MinWarehousePhoneVersion(IServiceProvider services) =>
        services.GetService<IConfiguration>()?[MinWarehousePhoneVersionKey];

    public static bool IsMobileUser(ClaimsPrincipal user) =>
        user.HasClaim(CentralApiClaims.Scope, CentralApiClaims.MobileUserScope);

    public static async Task<MobileUserAccessResult> CheckAsync(ClaimsPrincipal principal, CentralApiDbContext db, CancellationToken ct, string? minWarehousePhoneVersion = null)
    {
        if (!Guid.TryParse(principal.FindFirstValue("sub"), out var userId)
            || !principal.TryGetTenantId(out var tenantId)
            || principal.FindFirstValue(CentralApiClaims.DeviceId) is not { Length: > 0 } deviceId)
            return Deny(401, "INVALID_TOKEN", "Mobile user token is malformed.");

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        var user = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId && u.DeletedAtUtc == null, ct);
        if (tenant is null || user is null) return Deny(401, "SESSION_REVOKED", "The user no longer exists.");
        if (!tenant.IsActive) return Deny(403, "TENANT_INACTIVE", "The company account is disabled.");
        if (!user.IsActive) return Deny(403, "USER_INACTIVE", "This user is disabled.");

        var device = await db.MobileDevices.AsNoTracking()
            .Where(d => d.TenantId == tenantId && d.DeviceId == deviceId)
            .Select(d => new { d.IsActive, d.AppVersion })
            .FirstOrDefaultAsync(ct);

        // Roles decide which app a person may work in; checked on every call so removing a role applies at
        // once — also to an open phone session of an old build whose user was narrowed to WAREHOUSE.
        if (ClientDenial(principal, user, device?.AppVersion, minWarehousePhoneVersion) is { } denial) return denial;

        if (device is not { IsActive: true }) return Deny(403, "DEVICE_REVOKED", "This device has been blocked for the company.");

        var subscription = await db.TenantSubscriptions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.IsCurrent, ct);
        var status = MobileSeatService.SubscriptionStatus(subscription, DateTimeOffset.UtcNow);
        if (!MobileSeatService.AllowsWork(status))
            return Deny(403, status == "none" ? "SUBSCRIPTION_REQUIRED" : "SUBSCRIPTION_EXPIRED", "The company has no active subscription.");

        return new MobileUserAccessResult(tenant, user, 200, null, null);
    }

    /// <summary>
    /// A portal-only user on the phone app, a field-only user in the portal, or a warehouse-only user
    /// on a phone build older than <paramref name="minWarehousePhoneVersion"/> (none configured = none allowed).
    /// </summary>
    public static MobileUserAccessResult? ClientDenial(ClaimsPrincipal principal, MobileUser user, string? appVersion, string? minWarehousePhoneVersion)
    {
        if (CentralApiClaims.ClientOf(principal) == CentralApiClaims.PortalClient)
            return RolePermissions.CanUsePortal(user) ? null : Deny(403, "PORTAL_REQUIRES_MANAGER", "The web portal is not open to this user's roles.");
        if (!RolePermissions.CanUsePhone(user))
            return Deny(403, "ROLE_NOT_ALLOWED_ON_PHONE", "This user's roles work in the web portal, not the phone app.");
        if (RolePermissions.IsWarehouseOnlyOnPhone(user) && !IsAtLeast(appVersion, minWarehousePhoneVersion))
            return Deny(403, "ROLE_NOT_ALLOWED_ON_PHONE", "A warehouse-only user needs a newer phone app version, or the web portal.");
        return null;
    }

    /// <summary>Dotted numeric versions ("1.5.240"); anything unreadable is not at least anything.</summary>
    public static bool IsAtLeast(string? version, string? minimum) =>
        Version.TryParse(minimum?.Trim(), out var min)
        && Version.TryParse(version?.Trim(), out var actual)
        && actual >= min;

    private static MobileUserAccessResult Deny(int status, string code, string message) => new(null, null, status, code, message);
}
