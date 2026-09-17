namespace ErpBridge.CentralApi.LogCenter;

/// <summary>
/// What a producer hands to <see cref="ILogEventWriter"/>. Everything is raw and untrusted: the writer
/// normalizes, bounds and scrubs every field. Leave <see cref="EventId"/> empty for server-side events —
/// the writer then assigns one and skips the duplicate check.
/// </summary>
public sealed class LogEventInput
{
    public string? EventId { get; init; }
    public required string Source { get; init; }
    public Guid? TenantId { get; init; }
    public DateTimeOffset? OccurredAtUtc { get; init; }
    public string? Severity { get; init; }
    public string? Kind { get; init; }
    public string? Operation { get; init; }
    public string? Screen { get; init; }
    public string? Message { get; init; }
    public string? ExceptionType { get; init; }
    public string? StackTrace { get; init; }
    public string? AppVersion { get; init; }
    public string? OsVersion { get; init; }
    public string? DeviceModel { get; init; }
    public string? DeviceId { get; init; }
    public Guid? UserId { get; init; }
    public Guid? AgentId { get; init; }
    public string? SessionId { get; init; }
    public string? CorrelationId { get; init; }
    public string? HttpMethod { get; init; }
    public string? HttpRoute { get; init; }
    public int? HttpStatus { get; init; }
    public int? DurationMs { get; init; }
    public int RepeatCount { get; init; } = 1;

    /// <summary>Raw JSON object text; anything else is stored as <c>{}</c>.</summary>
    public string? PropertiesJson { get; init; }

    /// <summary>Raw JSON array text; anything else is stored as <c>[]</c>.</summary>
    public string? BreadcrumbsJson { get; init; }
}

public sealed record LogWriteResult(int Accepted, int Duplicate);
