using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Permissions;

/// <summary>A company's change to one role's template (only differences from the catalogue default are stored).</summary>
public sealed record RoleTemplateValue(string Role, string Key, string Value);

/// <summary>
/// Resolves a user's permissions (GOAL_YETKILER). Pure: the caller loads the rows.
///
/// <list type="number">
/// <item>ADMIN: every permission, no limits, source <see cref="PermissionSource.Admin"/>. Nothing can take an
/// administrator's rights away, so the company can never lock itself out (the last-admin rule stays with users).</item>
/// <item>Per role: the company's template value, or the catalogue default.</item>
/// <item>Across the user's roles: a yes/no permission is granted if any role grants it; a limit is the highest,
/// and unlimited beats any number — roles only add.</item>
/// <item>A manager's two approval switches (<see cref="MobileUser.CanApprove"/>,
/// <see cref="MobileUser.CanManageApprovalRules"/>) act as that person's override, exactly as before.</item>
/// <item>A personal override replaces what the roles give.</item>
/// </list>
/// </summary>
public static class PermissionResolver
{
    public static EffectivePermissions Resolve(
        IReadOnlyCollection<string> roles,
        IEnumerable<RoleTemplateValue> templates,
        IReadOnlyDictionary<string, string> overrides,
        bool managerCanApprove = false,
        bool managerCanManageApprovalRules = false)
    {
        var entries = new Dictionary<string, PermissionEntry>(StringComparer.Ordinal);
        if (roles.Contains(MobileUserRoles.Admin))
        {
            foreach (var definition in PermissionCatalog.All)
            {
                var all = definition.Type == PermissionType.Bool ? PermissionValues.True : PermissionValues.Unlimited;
                entries[definition.Key] = new PermissionEntry(definition.Key, all, PermissionSource.Admin, all, [MobileUserRoles.Admin], null);
            }
            return new EffectivePermissions(isAdmin: true, entries);
        }

        var templateValues = templates
            .GroupBy(t => (t.Role, t.Key))
            .ToDictionary(g => g.Key, g => g.Last().Value);
        var validRoles = roles.Where(MobileUserRoles.IsValid).Distinct(StringComparer.Ordinal).ToList();
        var isManager = validRoles.Contains(MobileUserRoles.Manager);

        foreach (var definition in PermissionCatalog.All)
        {
            var perRole = validRoles
                .Select(role => (Role: role, Value: templateValues.GetValueOrDefault((role, definition.Key)) ?? definition.Defaults[role]))
                .ToList();
            var (roleValue, sources) = Combine(definition, perRole);

            var personal = overrides.GetValueOrDefault(definition.Key);
            if (personal is null && isManager)
            {
                if (definition.Key == PermissionKeys.ApprovalsDecide && managerCanApprove) personal = PermissionValues.True;
                if (definition.Key == PermissionKeys.ApprovalsManageRules && managerCanManageApprovalRules) personal = PermissionValues.True;
            }

            entries[definition.Key] = personal is null
                ? new PermissionEntry(definition.Key, roleValue, PermissionSource.Role, roleValue, sources, null)
                : new PermissionEntry(definition.Key, personal, PermissionSource.Override, roleValue, sources, personal);
        }
        return new EffectivePermissions(isAdmin: false, entries);
    }

    /// <summary>A user's permissions from the rows already loaded for them.</summary>
    public static EffectivePermissions Resolve(MobileUser user, IEnumerable<RoleTemplateValue> templates, IReadOnlyDictionary<string, string> overrides) =>
        Resolve(RolePermissions.Of(user), templates, overrides, user.CanApprove, user.CanManageApprovalRules);

    private static (string Value, IReadOnlyList<string> Sources) Combine(PermissionDefinition definition, List<(string Role, string Value)> perRole)
    {
        if (perRole.Count == 0)
            return (definition.Type == PermissionType.Bool ? PermissionValues.False : PermissionValues.FormatLimit(0), []);

        if (definition.Type == PermissionType.Bool)
        {
            var granting = perRole.Where(r => PermissionValues.IsTrue(r.Value)).Select(r => r.Role).ToList();
            return granting.Count > 0 ? (PermissionValues.True, granting) : (PermissionValues.False, perRole.Select(r => r.Role).ToList());
        }

        var unlimited = perRole.Where(r => PermissionValues.ParseLimit(r.Value) is null).Select(r => r.Role).ToList();
        if (unlimited.Count > 0) return (PermissionValues.Unlimited, unlimited);
        var highest = perRole.Max(r => PermissionValues.ParseLimit(r.Value)!.Value);
        return (PermissionValues.FormatLimit(highest),
            perRole.Where(r => PermissionValues.ParseLimit(r.Value) == highest).Select(r => r.Role).ToList());
    }
}
