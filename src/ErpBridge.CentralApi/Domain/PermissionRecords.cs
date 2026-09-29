namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A company's change to one role's permission template (GOAL_YETKILER). Only values that differ from the catalogue
/// default are stored; "back to default" deletes the row. ADMIN has no template: it always has everything.
/// </summary>
public sealed class TenantRolePermission
{
    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>One of <see cref="MobileUserRoles.All"/> except ADMIN.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>A <c>PermissionKeys</c> key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Storage encoding of <c>PermissionValues</c>: <c>"1"</c>/<c>"0"</c>, or a limit (<c>""</c> = unlimited).</summary>
    public string Value { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public Guid? UpdatedByUserId { get; set; }
}

/// <summary>One person's permission set apart from their roles ("izin ver" / "engelle" / a limit). No row = inherit.</summary>
public sealed class MobileUserPermissionOverride
{
    public Guid UserId { get; set; }

    public MobileUser? User { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public Guid? UpdatedByUserId { get; set; }
}

/// <summary>Who changed which permission, from where, and from what to what. Written with every change, never edited.</summary>
public sealed class PermissionChange
{
    public long Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? ActorUserId { get; set; }

    /// <summary>Snapshot: stays readable after the user is renamed or deleted.</summary>
    public string ActorName { get; set; } = string.Empty;

    /// <summary><c>android</c> or <c>portal</c>.</summary>
    public string Client { get; set; } = string.Empty;

    /// <summary><c>role</c> (a template), <c>user</c> (a personal override) or <c>roles</c> (the user's roles).</summary>
    public string Scope { get; set; } = string.Empty;

    public string? Role { get; set; }

    public Guid? TargetUserId { get; set; }

    public string? TargetUserName { get; set; }

    public string Key { get; set; } = string.Empty;

    /// <summary>Null: no row before (the default / inherited).</summary>
    public string? OldValue { get; set; }

    /// <summary>Null: the row was removed (back to the default / inherited).</summary>
    public string? NewValue { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
