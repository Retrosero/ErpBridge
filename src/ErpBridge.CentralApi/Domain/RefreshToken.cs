namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Persistent handle for an admin session that lets the central API issue a
/// fresh short-lived access token without re-prompting for the password.
///
/// <para>
/// Each row is the SHA-256 hash of the raw token string the client received at
/// login. The raw token is only ever sent to the client once; the server
/// stores only the hash, so a database leak does not let an attacker reuse
/// any still-live refresh token. Rotation is single-use: as soon as a row
/// participates in a successful <c>refresh</c> call, <see cref="RevokedAtUtc"/>
/// is set and the new token's id is recorded in <see cref="ReplacedByTokenId"/>.
/// </para>
///
/// <para>
/// Scoped to the admin flow on purpose. Agent tokens (scope=agent) do not use
/// this table — they ride a different rotation story through the agent
/// registration handshake.
/// </para>
/// </summary>
public sealed class RefreshToken
{
    /// <summary>Stable row id; the rotation link <see cref="ReplacedByTokenId"/> references this.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The <see cref="AdminUser.Id"/> that owns this refresh handle.</summary>
    public Guid AdminUserId { get; set; }

    /// <summary>SHA-256 hex digest of the raw token. The raw value never leaves the client.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Hard expiry; an expired row fails <c>refresh</c> with 401.</summary>
    public DateTimeOffset ExpiresAtUtc { get; set; }

    /// <summary>Set the moment a row is rotated or explicitly logged out. <c>null</c> while live.</summary>
    public DateTimeOffset? RevokedAtUtc { get; set; }

    /// <summary>Id of the <see cref="RefreshToken"/> that replaced this one. <c>null</c> for the last live row.</summary>
    public string? ReplacedByTokenId { get; set; }

    /// <summary>Remote IP recorded at issue time. Useful for audit + theft investigation.</summary>
    public string CreatedByIp { get; set; } = string.Empty;
}
