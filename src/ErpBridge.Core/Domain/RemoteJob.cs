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
}
