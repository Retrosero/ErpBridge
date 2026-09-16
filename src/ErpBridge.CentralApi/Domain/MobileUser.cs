namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A person who signs in to the mobile app on behalf of a tenant. Every active,
/// non-deleted user occupies one paid seat — administrators included — so the
/// seat count is always <c>COUNT(*) WHERE IsActive AND DeletedAtUtc IS NULL</c>.
/// </summary>
public sealed class MobileUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>Lower-case sign-in name, unique per tenant among non-deleted users.</summary>
    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>BCrypt hash; the plain password is never stored or logged.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// The role an app built before multi-role accounts understands: <c>ADMIN</c>, <c>MANAGER</c> or
    /// <c>SALES</c>, derived from <see cref="Roles"/> by <see cref="MobileUserRoles.Legacy"/> on every
    /// write. Permissions are never read from this column; see <see cref="RolePermissions"/>.
    /// </summary>
    public string Role { get; set; } = MobileUserRoles.Sales;

    /// <summary>Every role the user holds (<c>mobile_user_roles</c>); at least one.</summary>
    public List<MobileUserRole> Roles { get; set; } = [];

    /// <summary>A manager the administrator allowed to approve and reject requests. Administrators always may.</summary>
    public bool CanApprove { get; set; }

    /// <summary>A manager the administrator allowed to change the approval rules. Administrators always may.</summary>
    public bool CanManageApprovalRules { get; set; }

    /// <summary>Inactive users keep their history but release their seat and cannot sign in.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Soft delete. The row stays so documents a user created remain attributable;
    /// the username becomes reusable because the unique index ignores deleted rows.
    /// </summary>
    public DateTimeOffset? DeletedAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastLoginAtUtc { get; set; }
}

/// <summary>One role of a user. A user holds any combination; permissions are their union.</summary>
public sealed class MobileUserRole
{
    public Guid UserId { get; set; }

    public MobileUser? User { get; set; }

    /// <summary>One of <see cref="MobileUserRoles.All"/>.</summary>
    public string Role { get; set; } = string.Empty;

    public DateTimeOffset GrantedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>The company administrator who granted it; null when the operator console or a migration did.</summary>
    public Guid? GrantedByUserId { get; set; }
}

/// <summary>Role names.</summary>
public static class MobileUserRoles
{
    public const string Admin = "ADMIN";

    /// <summary>A field user whose approval rights the administrator grants one by one.</summary>
    public const string Manager = "MANAGER";

    /// <summary>Office accounting: decides financial approval requests in the portal.</summary>
    public const string Accounting = "ACCOUNTING";

    /// <summary>Warehouse staff: prepares, packs and loads orders in the portal.</summary>
    public const string Warehouse = "WAREHOUSE";

    public const string Sales = "SALES";

    /// <summary>In precedence order, highest first.</summary>
    public static readonly IReadOnlyList<string> All = [Admin, Manager, Accounting, Warehouse, Sales];

    public static bool IsValid(string? role) => role is not null && All.Contains(role);

    /// <summary>
    /// Upper-cased, de-duplicated roles in precedence order, or <c>null</c> when the list is empty or
    /// names an unknown role.
    /// </summary>
    public static IReadOnlyList<string>? Normalize(IEnumerable<string?>? roles)
    {
        if (roles is null) return null;
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var raw in roles)
        {
            var role = raw?.Trim().ToUpperInvariant();
            if (!IsValid(role)) return null;
            set.Add(role!);
        }
        return set.Count == 0 ? null : All.Where(set.Contains).ToList();
    }

    /// <summary>
    /// The single role for <see cref="MobileUser.Role"/> and the session's <c>role</c> field: apps
    /// built before multi-role accounts know only ADMIN/MANAGER/SALES, and a phone treats anything else as a field user.
    /// </summary>
    public static string Legacy(IEnumerable<string> roles)
    {
        var set = roles as ICollection<string> ?? roles.ToList();
        return set.Contains(Admin) ? Admin : set.Contains(Manager) ? Manager : Sales;
    }
}

/// <summary>
/// What a user may do, as the union of their roles. Read from the current database row on every
/// request (rule 14), never from the token, so revoking a role applies at once.
/// </summary>
public static class RolePermissions
{
    /// <summary>
    /// The user's roles. <see cref="MobileUser.Roles"/> must be loaded; a row read without them
    /// falls back to the legacy column, which never grants more than the user had before multi-role.
    /// </summary>
    public static IReadOnlyCollection<string> Of(MobileUser user) =>
        user.Roles.Count > 0 ? user.Roles.Select(r => r.Role).ToHashSet(StringComparer.Ordinal) : [user.Role];

    public static bool Has(MobileUser user, string role) => Of(user).Contains(role);

    public static bool IsAdmin(MobileUser user) => Has(user, MobileUserRoles.Admin);

    /// <summary>Signing in to the phone app: selling, collecting, route work.</summary>
    public static bool CanUsePhone(MobileUser user) =>
        Of(user).Any(r => r is MobileUserRoles.Admin or MobileUserRoles.Manager or MobileUserRoles.Sales);

    /// <summary>Signing in to the web portal.</summary>
    public static bool CanUsePortal(MobileUser user) =>
        Of(user).Any(r => r is MobileUserRoles.Admin or MobileUserRoles.Manager or MobileUserRoles.Accounting or MobileUserRoles.Warehouse);

    public static bool CanManageUsers(MobileUser user) => IsAdmin(user);

    /// <summary>Company-wide sales, collection and route reports.</summary>
    public static bool CanViewReports(MobileUser user) =>
        Of(user).Any(r => r is MobileUserRoles.Admin or MobileUserRoles.Manager);

    /// <summary>Customer balances and stock.</summary>
    public static bool CanViewLedger(MobileUser user) =>
        Of(user).Any(r => r is MobileUserRoles.Admin or MobileUserRoles.Manager or MobileUserRoles.Accounting);

    public static bool CanPlanRoutes(MobileUser user) =>
        Of(user).Any(r => r is MobileUserRoles.Admin or MobileUserRoles.Manager);

    public static bool CanOperateWarehouse(MobileUser user) =>
        Of(user).Any(r => r is MobileUserRoles.Admin or MobileUserRoles.Manager or MobileUserRoles.Warehouse);

    /// <summary>Cancelling and reassigning orders, undoing anyone's step, the warehouse settings.</summary>
    public static bool CanManageWarehouse(MobileUser user) =>
        Of(user).Any(r => r is MobileUserRoles.Admin or MobileUserRoles.Manager);
}

/// <summary>What a user may do in the approval centre, from their current row.</summary>
public static class ApprovalPermissions
{
    /// <summary>The money documents accounting decides; cards and stock counts stay with managers.</summary>
    public static readonly IReadOnlySet<string> AccountingKinds = new HashSet<string>(StringComparer.Ordinal)
    {
        ApprovalKinds.Sale, ApprovalKinds.Return, ApprovalKinds.Collection, ApprovalKinds.Disbursement, ApprovalKinds.Purchase,
    };

    private static bool DecidesEverything(MobileUser user) =>
        RolePermissions.IsAdmin(user) || (RolePermissions.Has(user, MobileUserRoles.Manager) && user.CanApprove);

    /// <summary>Whether the user may approve, reject or reopen requests of at least one kind.</summary>
    public static bool CanDecide(MobileUser user) =>
        DecidesEverything(user) || RolePermissions.Has(user, MobileUserRoles.Accounting);

    /// <summary>Whether the user may decide a request of <paramref name="kind"/>.</summary>
    public static bool CanDecide(MobileUser user, string kind) =>
        DecidesEverything(user) || (RolePermissions.Has(user, MobileUserRoles.Accounting) && AccountingKinds.Contains(kind));

    /// <summary>The kinds the user decides; <c>null</c> means every kind, an empty set none.</summary>
    public static IReadOnlySet<string>? DecidableKinds(MobileUser user) =>
        DecidesEverything(user) ? null
        : RolePermissions.Has(user, MobileUserRoles.Accounting) ? AccountingKinds
        : new HashSet<string>();

    public static bool CanManageRules(MobileUser user) =>
        RolePermissions.IsAdmin(user) || (RolePermissions.Has(user, MobileUserRoles.Manager) && user.CanManageApprovalRules);
}
