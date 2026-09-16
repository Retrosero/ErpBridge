namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A warehouse TV paired to a company (plan step 7, Faz 49). It signs in with a pairing code instead of a
/// user, so it takes no seat; it can only read the warehouse board. Revoking it ends its token at the next call.
/// </summary>
public sealed class DisplayDevice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>"Depo girişi TV".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 of the pairing secret the TV proved when it took its token; kept so the row shows the
    /// device was paired with a secret. The token itself is a signed JWT and is never stored.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    public Guid? CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Updated at most once a minute while the board polls.</summary>
    public DateTimeOffset? LastSeenAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }
}

/// <summary>
/// A six-digit code a TV shows until a manager enters it in the portal. Valid for ten minutes; the TV polls
/// with the secret only it knows, and the row is deleted once the TV has taken its token.
/// </summary>
public sealed class DisplayPairingCode
{
    /// <summary>Six digits; unique among the rows that exist (expired rows are removed before a new code is made).</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>SHA-256 of the TV's polling secret.</summary>
    public string PairingSecretHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    /// <summary>Set when a manager claims the code.</summary>
    public Guid? DisplayDeviceId { get; set; }

    public DateTimeOffset? ClaimedAtUtc { get; set; }
}
