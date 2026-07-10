namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A human operator with a credential row. Admin accounts are global (not
/// tenant-scoped) and authenticate against <c>/api/v1/admin/login</c> using a
/// bcrypt-hashed password. Tokens carry <c>scope=admin</c> which the
/// <c>Admin</c> authorization policy requires.
/// </summary>
public sealed class AdminUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Email/login id, unique.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>bcrypt hash of the password. Never returned by any endpoint.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastLoginAtUtc { get; set; }

    public bool IsActive { get; set; } = true;
}
