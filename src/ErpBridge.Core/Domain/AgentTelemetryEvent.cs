namespace ErpBridge.Core.Domain;

/// <summary>
/// Privacy-scrubbed diagnostic event emitted by a Windows agent. Credentials,
/// connection strings and business payloads must never be placed in this type.
/// </summary>
public sealed class AgentTelemetryEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public string Kind { get; init; } = "desktop_exception";
    public string Severity { get; init; } = "ERROR";
    public string AppVersion { get; init; } = string.Empty;
    public string WindowsVersion { get; init; } = string.Empty;
    public string MachineName { get; init; } = string.Empty;
    public string Operation { get; init; } = string.Empty;
    public string ExceptionType { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string StackTrace { get; init; } = string.Empty;
}
