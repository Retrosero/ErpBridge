using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Permissions;

/// <summary>
/// Loads the rows <see cref="PermissionResolver"/> needs and puts the result on <see cref="MobileUser.Permissions"/>.
/// Two small indexed queries (usually no rows); an administrator needs none.
/// </summary>
public static class PermissionLoader
{
    public static async Task<EffectivePermissions> LoadAsync(CentralApiDbContext db, MobileUser user, CancellationToken ct)
    {
        var roles = RolePermissions.Of(user).ToList();
        if (roles.Contains(MobileUserRoles.Admin))
            return user.Permissions = PermissionResolver.Resolve(user, [], Empty);

        var templates = await db.TenantRolePermissions.AsNoTracking()
            .Where(t => t.TenantId == user.TenantId && roles.Contains(t.Role))
            .Select(t => new RoleTemplateValue(t.Role, t.Key, t.Value))
            .ToListAsync(ct);
        var overrides = await db.MobileUserPermissionOverrides.AsNoTracking()
            .Where(o => o.UserId == user.Id)
            .ToDictionaryAsync(o => o.Key, o => o.Value, StringComparer.Ordinal, ct);
        return user.Permissions = PermissionResolver.Resolve(user, templates, overrides);
    }

    /// <summary>The same for a list of one company's users, in two queries.</summary>
    public static async Task LoadManyAsync(CentralApiDbContext db, Guid tenantId, IReadOnlyCollection<MobileUser> users, CancellationToken ct)
    {
        if (users.Count == 0) return;
        var templates = await db.TenantRolePermissions.AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .Select(t => new RoleTemplateValue(t.Role, t.Key, t.Value))
            .ToListAsync(ct);
        var ids = users.Select(u => u.Id).ToList();
        var overrides = (await db.MobileUserPermissionOverrides.AsNoTracking()
                .Where(o => ids.Contains(o.UserId))
                .ToListAsync(ct))
            .GroupBy(o => o.UserId)
            .ToDictionary(g => g.Key, g => (IReadOnlyDictionary<string, string>)g.ToDictionary(o => o.Key, o => o.Value, StringComparer.Ordinal));
        foreach (var user in users)
            user.Permissions = PermissionResolver.Resolve(user, templates, overrides.GetValueOrDefault(user.Id) ?? Empty);
    }

    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();
}
