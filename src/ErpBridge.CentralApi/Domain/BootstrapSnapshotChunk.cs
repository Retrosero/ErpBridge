namespace ErpBridge.CentralApi.Domain;

/// <summary>A bounded JSON array chunk belonging to a staged or active snapshot.</summary>
public sealed class BootstrapSnapshotChunk
{
    public Guid Id { get; set; }
    public Guid SnapshotId { get; set; }
    public string Section { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public int ItemCount { get; set; }
    public string PayloadJson { get; set; } = "[]";
    public DateTimeOffset ReceivedAtUtc { get; set; }

    public BootstrapSnapshot? Snapshot { get; set; }
}
