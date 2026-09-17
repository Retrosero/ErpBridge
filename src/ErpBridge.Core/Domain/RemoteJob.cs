namespace ErpBridge.Core.Domain;

/// <summary>A pending job fetched from the central API.</summary>
public sealed class RemoteJob
{
    public string JobId { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset EnqueuedAtUtc { get; set; }

    /// <summary>
    /// Which lease of the job this is (goal ERP yazım Y1e); echoed in the ack so a result from an
    /// expired lease cannot overwrite a newer one. Null from servers before the field.
    /// </summary>
    public int? Attempt { get; set; }

    /// <summary>How to write a phone document into the ERP (Y1d); null for a company without an ERP or an older server.</summary>
    public ErpBridge.Core.Jobs.ErpWriteContext? ErpContext { get; set; }

    /// <summary>
    /// Log Merkezi L3g: the trace id the server booked this job with. The agent puts it on everything it logs
    /// about the job and sends it back as <c>X-Correlation-Id</c>, so the phone's request, the ERP write and
    /// the ack are one search in the Log Centre. Null from a server before the field.
    /// </summary>
    public string? CorrelationId { get; set; }
}
