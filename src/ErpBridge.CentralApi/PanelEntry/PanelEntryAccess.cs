using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Permissions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.PanelEntry;

/// <summary>The signed-in panel user making an entry, with their resolved permissions.</summary>
public sealed record PanelEntryCaller(Tenant Tenant, MobileUser User, EffectivePermissions Permissions)
{
    public bool IsNative => Tenant.DataSource == TenantDataSources.Native;
}

/// <summary>
/// Who may enter documents from the panel (GOAL_PANEL_GIRIS K3): a panel session — never the phone's, which must go
/// through ingest and its approval rules — of a user whose phone module for the kind is on. The company's approval
/// rules are not consulted (K2); the user's limits are, by the caller of <see cref="DocumentPermissionCheck.Check"/>.
/// </summary>
public static class PanelEntryAccess
{
    public static async Task<(PanelEntryCaller? Caller, IResult? Error)> AuthorizeAsync(
        HttpContext http, CentralApiDbContext db, PanelEntryKind? kind, CancellationToken ct)
    {
        // Without this a phone token could post here and skip the approval rules ingest holds it to.
        if (CentralApiClaims.ClientOf(http.User) != CentralApiClaims.PortalClient)
            return (null, Error(StatusCodes.Status403Forbidden, "ENTRY_REQUIRES_PORTAL", "Bu giriş yalnız yönetim panelinden yapılır."));
        var (tenant, user, error) = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (error is not null) return (null, error);
        var permissions = user!.Permissions ?? await PermissionLoader.LoadAsync(db, user, ct);
        var caller = new PanelEntryCaller(tenant!, user, permissions);
        if (kind is not null && !permissions.Can(kind.ModuleKey))
            return (null, Error(StatusCodes.Status403Forbidden, "ENTRY_MODULE_DENIED",
                $"Bu işlem için yetkiniz yok ({PermissionCatalog.Find(kind.ModuleKey)?.Label ?? kind.Label})."));
        return (caller, null);
    }

    /// <summary>The kinds the user may enter, in the menu's order.</summary>
    public static IReadOnlyList<PanelEntryKind> Allowed(EffectivePermissions permissions) =>
        [.. PanelEntryKinds.All.Where(k => permissions.Can(k.ModuleKey))];

    /// <summary>
    /// The document's owner: the caller, or another active user of the company who works the phone (a salesperson whose
    /// ERP mapping the agent will write it with). Null when <paramref name="ownerUserId"/> names nobody of the kind.
    /// </summary>
    public static async Task<MobileUser?> OwnerAsync(CentralApiDbContext db, PanelEntryCaller caller, Guid? ownerUserId, CancellationToken ct)
    {
        if (ownerUserId is not { } id || id == caller.User.Id) return caller.User;
        var user = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == caller.Tenant.Id && u.IsActive && u.DeletedAtUtc == null, ct);
        return user is not null && RolePermissions.CanUsePhone(user) ? user : null;
    }

    /// <summary>The owners the form offers: the caller first, then the company's active phone users by name.</summary>
    public static async Task<List<PortalEntryOwnerDto>> OwnersAsync(CentralApiDbContext db, PanelEntryCaller caller, CancellationToken ct)
    {
        var users = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .Where(u => u.TenantId == caller.Tenant.Id && u.IsActive && u.DeletedAtUtc == null && u.Id != caller.User.Id)
            .ToListAsync(ct);
        var owners = new List<PortalEntryOwnerDto> { Owner(caller.User, self: true) };
        owners.AddRange(users.Where(RolePermissions.CanUsePhone)
            .Select(u => Owner(u, self: false))
            .OrderBy(o => o.Name, StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"), ignoreCase: true)));
        return owners;
    }

    public static string NameOf(MobileUser user) => string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;

    public static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });

    private static PortalEntryOwnerDto Owner(MobileUser user, bool self) =>
        new() { UserId = user.Id, Name = NameOf(user), Username = user.Username, IsSelf = self };
}
