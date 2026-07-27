namespace ErpBridge.CentralApi.Domain;

/// <summary>One sanitized Android crash, handled error, network failure, or health event.</summary>
public sealed class TelemetryEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public Guid TenantId { get; set; }
    public Guid MobileDeviceId { get; set; }
    public Guid TelemetryIssueId { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public DateTimeOffset ReceivedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public string Kind { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? AppVersion { get; set; }
    public string? AndroidVersion { get; set; }
    public string? DeviceModel { get; set; }
    public string? Screen { get; set; }
    public string? Operation { get; set; }
    public string? ExceptionType { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
    public string? HttpMethod { get; set; }
    public string? HttpRoute { get; set; }
    public int? HttpStatus { get; set; }
    public string? CorrelationId { get; set; }
    public string BreadcrumbsJson { get; set; } = "[]";
    public Tenant? Tenant { get; set; }
    public MobileDevice? MobileDevice { get; set; }
    public TelemetryIssue? Issue { get; set; }
}
