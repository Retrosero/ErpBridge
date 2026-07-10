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
}
