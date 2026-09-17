namespace ErpBridge.Core.Domain;

/// <summary>Acknowledgement sent back to the central API for a processed job.</summary>
public sealed class JobAck
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErpDocumentSeries { get; set; }
    public int? ErpDocumentNumber { get; set; }
    public int? ErpRecno { get; set; }
    public string? ErpGuid { get; set; }

    /// <summary>
    /// With a failure: the same document may go through later without anyone changing anything (the ERP
    /// was unreachable), so the server queues it again with a delay (goal ERP yazım Y1e).
    /// </summary>
    public bool? Retryable { get; set; }

    /// <summary>The <see cref="RemoteJob.Attempt"/> this result belongs to.</summary>
    public int? Attempt { get; set; }
}
