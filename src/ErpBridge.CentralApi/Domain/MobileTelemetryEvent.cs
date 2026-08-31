namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Privacy-scrubbed diagnostic event sent by a mobile client.  The source app
/// is responsible for removing credentials and customer-entered values before
/// transmission; the server bounds every field again before persistence.
/// </summary>
public sealed class MobileTelemetryEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string EventId { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public DateTimeOffset ReceivedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public string Kind { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public string AndroidVersion { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string Screen { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public string? HttpMethod { get; set; }
    public string? HttpRoute { get; set; }
    public int? HttpStatus { get; set; }
    public string? CorrelationId { get; set; }
    public string BreadcrumbsJson { get; set; } = "[]";
}
