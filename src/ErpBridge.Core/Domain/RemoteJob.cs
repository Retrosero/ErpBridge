namespace ErpBridge.Core.Domain;

/// <summary>A pending job fetched from the central API.</summary>
public sealed class RemoteJob
{
    public string JobId { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset EnqueuedAtUtc { get; set; }

    /// <summary>Log Merkezi L3g: the id of the request that created the job (phone upload, approval); may be null.</summary>
    public string? CorrelationId { get; set; }
}
