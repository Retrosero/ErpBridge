using ErpBridge.Core.Stores;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Trigger;

/// <summary>
/// Trigger-based change-set orchestrator. One
/// <see cref="RunOnceAsync"/> call = one full pull of every tracked table
/// from Mikro + one push to the central API. The implementation reuses
/// <see cref="ITriggerWatermarkStore"/> for the per-table cursor and
/// <see cref="IRemoteApiClient.PushChangeSetAsync"/> for the upload.
///
/// The service is the canonical "trigger mode" counterpart of
/// <see cref="IBootstrapSyncService"/>. The WPF toggle in
/// <c>AgentServiceOptions.UseTriggerBasedSync</c> picks which one the
/// <c>BootstrapWorker</c> drives; the two never run side by side.
/// </summary>
public interface IChangeSetSyncService
{
    /// <summary>
    /// Run a single trigger-based cycle. The result is a structured
    /// <see cref="ChangeSetSyncResult"/> so the worker can log counts and
    /// error codes without exception flow.
    /// </summary>
    Task<ChangeSetSyncResult> RunOnceAsync(CancellationToken ct = default);

    /// <summary>
    /// Wipe the per-table trigger watermarks. The next
    /// <see cref="RunOnceAsync"/> will pull the entire dataset instead of
    /// resuming from the last cursor.
    /// </summary>
    Task InvalidateAsync(CancellationToken ct = default);
}

/// <summary>
/// Outcome of a single <see cref="IChangeSetSyncService.RunOnceAsync"/>.
/// On success, the row counts sum the per-table contributions from
/// <see cref="SyncChangeSet.TotalRowCount"/>. On failure, the
/// <see cref="ErrorCode"/> and <see cref="ErrorMessage"/> fields surface the
/// same diagnostics the <c>BootstrapWorker</c> already logs.
/// </summary>
public sealed record ChangeSetSyncResult(
    bool Success,
    int TablesScanned,
    int NewRowsPushed,
    int ChangedRowsPushed,
    int DeletedRowsPushed,
    long DurationMs,
    string? ErrorCode = null,
    string? ErrorMessage = null)
{
    /// <summary>True when the cycle produced an empty bundle (no Mikro changes since the last push). Skips the watermark advance.</summary>
    public static ChangeSetSyncResult Empty(long durationMs) =>
        new(Success: true, TablesScanned: 0, NewRowsPushed: 0, ChangedRowsPushed: 0, DeletedRowsPushed: 0, DurationMs: durationMs);
}
