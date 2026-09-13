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

    /// <summary><see cref="MobileUserRoles.Admin"/> or <see cref="MobileUserRoles.Sales"/>.</summary>
    public string Role { get; set; } = MobileUserRoles.Sales;

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

/// <summary>Role names stored in <see cref="MobileUser.Role"/>.</summary>
public static class MobileUserRoles
{
    public const string Admin = "ADMIN";
    public const string Sales = "SALES";

    public static bool IsValid(string? role) => role is Admin or Sales;
}
