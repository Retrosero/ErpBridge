namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// The one computer a Go license is bound to. A Go key activates on a single machine; moving it
/// to another computer means an operator releases this row in the Admin console first.
/// </summary>
public sealed class GoInstallation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Unique: one installation per license.</summary>
    public Guid LicenseId { get; set; }

    public License? License { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>Opaque machine fingerprint the app computes (a hash; never the raw hardware id).</summary>
    public const int MachineIdMaxLength = 128;
    public string MachineId { get; set; } = string.Empty;

    /// <summary>Windows computer name, for the operator to recognise the machine.</summary>
    public const int MachineNameMaxLength = 128;
    public string? MachineName { get; set; }

    public const int AppVersionMaxLength = 32;
    public string? AppVersion { get; set; }

    public DateTimeOffset ActivatedAtUtc { get; set; }

    /// <summary>Last successful activation or renewal from this machine.</summary>
    public DateTimeOffset LastSeenAtUtc { get; set; }
}
