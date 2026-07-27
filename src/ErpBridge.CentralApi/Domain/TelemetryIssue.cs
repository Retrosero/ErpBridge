namespace ErpBridge.CentralApi.Domain;

public enum TelemetryIssueStatus
{
    Open = 0,
    Resolved = 1,
    Ignored = 2,
}

/// <summary>A tenant-scoped group of Android telemetry events sharing one fingerprint.</summary>
public sealed class TelemetryIssue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Fingerprint { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public TelemetryIssueStatus Status { get; set; }
    public DateTimeOffset FirstSeenAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastSeenAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAtUtc { get; set; }
    public int OccurrenceCount { get; set; }
    public string? LastAppVersion { get; set; }
    public Guid? LastDeviceId { get; set; }
    public Tenant? Tenant { get; set; }
    public ICollection<TelemetryEvent> Events { get; set; } = new List<TelemetryEvent>();
}
