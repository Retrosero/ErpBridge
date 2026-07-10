namespace ErpBridge.CentralApi.Options;

/// <summary>
/// Bootstrap-time admin credentials. When both <see cref="SeedEmail"/> and
/// <see cref="SeedPassword"/> are supplied (and <see cref="Jwt.SigningKey"/>
/// is configured), startup will create a single <see cref="Domain.AdminUser"/>
/// row if one with that email does not already exist. The password is stored
/// as a bcrypt hash; the plaintext value never leaves startup.
/// </summary>
public sealed class AdminSeedOptions
{
    /// <summary>Email of the bootstrap admin. Empty/null disables seeding.</summary>
    public string? SeedEmail { get; set; }

    /// <summary>Plaintext password, only used at startup to bcrypt-hash. Never logged.</summary>
    public string? SeedPassword { get; set; }

    /// <summary>Display name for the bootstrap admin.</summary>
    public string? SeedDisplayName { get; set; }
}
