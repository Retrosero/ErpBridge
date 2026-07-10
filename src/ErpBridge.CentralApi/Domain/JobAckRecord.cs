namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Acknowledgement from an agent for a processed job. Records both failure
/// (error code/message) and success (ErpDocumentSeries/Number + ErpRecno/
/// ErpGuid). One job can in theory have multiple ack rows (the agent may
/// retry); the latest row wins.
/// </summary>
public sealed class JobAckRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid JobId { get; set; }

    public Job? Job { get; set; }

    /// <summary>"succeeded" or "failed" — matches the wire format in api-contracts.md.</summary>
    public string Status { get; set; } = string.Empty;

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErpDocumentSeries { get; set; }

    public int? ErpDocumentNumber { get; set; }

    /// <summary>Mikro V15 identity (RECno).</summary>
    public int? ErpRecno { get; set; }

    /// <summary>Mikro V16 identity (Guid).</summary>
    public Guid? ErpGuid { get; set; }

    public DateTimeOffset AckedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}