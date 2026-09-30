namespace ErpBridge.CentralApi.Permissions;

/// <summary>Where a user's value of a permission comes from; the editors show it.</summary>
public enum PermissionSource
{
    /// <summary>The user is an administrator: everything, no limits, not editable.</summary>
    Admin,

    /// <summary>The union of the user's roles' templates.</summary>
    Role,

    /// <summary>Set for this person (or, for the approval rights, the manager's switches).</summary>
    Override,
}

/// <summary>One permission of one user, with enough context for an editor to explain it.</summary>
/// <param name="Value">The effective value in storage encoding (<see cref="PermissionValues"/>).</param>
/// <param name="RoleValue">What the roles alone give, before a personal override.</param>
/// <param name="RoleSources">The roles that produce <paramref name="RoleValue"/> (the ones granting, or holding the highest limit).</param>
/// <param name="Override">The personal value, or null when inherited.</param>
public sealed record PermissionEntry(
    string Key,
    string Value,
    PermissionSource Source,
    string RoleValue,
    IReadOnlyList<string> RoleSources,
    string? Override);

/// <summary>
/// What one user may do, resolved from their roles, the company's role templates and their personal overrides
/// (<see cref="PermissionResolver"/>). Immutable; resolved per request, never cached across requests (rule 14).
/// </summary>
public sealed class EffectivePermissions
{
    private readonly IReadOnlyDictionary<string, PermissionEntry> _entries;

    public EffectivePermissions(bool isAdmin, IReadOnlyDictionary<string, PermissionEntry> entries)
    {
        IsAdmin = isAdmin;
        _entries = entries;
    }

    public bool IsAdmin { get; }

    /// <summary>Every permission, in catalogue order.</summary>
    public IReadOnlyList<PermissionEntry> Entries => PermissionCatalog.All.Select(d => _entries[d.Key]).ToList();

    public PermissionEntry Entry(string key) =>
        _entries.TryGetValue(key, out var entry) ? entry : throw new ArgumentException($"Unknown permission '{key}'.", nameof(key));

    /// <summary>Whether a yes/no permission is granted. An unknown key is a programming error.</summary>
    public bool Can(string key) => PermissionValues.IsTrue(Entry(key).Value);

    /// <summary>The user's ceiling for a limit; <c>null</c> = unlimited.</summary>
    public decimal? Limit(string key) => PermissionValues.ParseLimit(Entry(key).Value);

    /// <summary>Yes/no permissions, for the session the phone and portal cache.</summary>
    public IReadOnlyDictionary<string, bool> Flags() =>
        Definitions(PermissionType.Bool).ToDictionary(d => d.Key, d => Can(d.Key), StringComparer.Ordinal);

    /// <summary>Limits (<c>null</c> = unlimited), for the session the phone and portal cache.</summary>
    public IReadOnlyDictionary<string, decimal?> Limits() =>
        Definitions(PermissionType.Limit).ToDictionary(d => d.Key, d => Limit(d.Key), StringComparer.Ordinal);

    /// <summary>How many permissions this person has set personally (the users list shows a badge).</summary>
    public int OverrideCount => _entries.Values.Count(e => e.Override is not null);

    private static IEnumerable<PermissionDefinition> Definitions(PermissionType type) =>
        PermissionCatalog.All.Where(d => d.Type == type);
}
