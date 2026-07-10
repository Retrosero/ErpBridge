using ErpBridge.Core.Domain;

namespace ErpBridge.Core.Stores;

/// <summary>
/// Per-tenant resume-cursor store. Used by bootstrap/incremental readers to remember
/// where they left off across agent restarts.
/// </summary>
public interface ICheckpointStore
{
    Task<CheckpointRecord?> LoadAsync(
        string tenantId,
        string syncScope,
        CancellationToken ct = default);

    Task SaveAsync(CheckpointRecord checkpoint, CancellationToken ct = default);
}
