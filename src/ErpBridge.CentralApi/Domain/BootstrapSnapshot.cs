namespace ErpBridge.CentralApi.Domain;

/// <summary>Metadata for the one active bootstrap snapshot of a tenant.</summary>
public sealed class BootstrapSnapshot
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string SourceDatabase { get; set; } = string.Empty;
    public DateTimeOffset PulledAtUtc { get; set; }
    public DateTimeOffset ReceivedAtUtc { get; set; }
    public DateTimeOffset ActivatedAtUtc { get; set; }
    public bool IsActive { get; set; }
    public bool IsIncremental { get; set; }

    public Tenant? Tenant { get; set; }
    public ICollection<BootstrapSnapshotChunk> Chunks { get; set; } = new List<BootstrapSnapshotChunk>();
}
