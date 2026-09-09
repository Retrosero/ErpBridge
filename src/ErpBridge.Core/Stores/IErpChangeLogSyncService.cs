namespace ErpBridge.Core.Stores;

/// <summary>
/// Drives one change-log sync cycle: read what changed in the ERP, push it to
/// the central API, then advance the resume cursor.
///
/// <para>
/// This is the ERP-agnostic successor to the Mikro-bound
/// <c>IChangeSetSyncService</c>. It depends only on
/// <see cref="ErpBridge.Erp.Abstractions.IErpAdapterFactory"/>,
/// <see cref="ErpBridge.Erp.Abstractions.ChangeLog.IErpSyncCursorStore"/> and
/// <see cref="IRemoteApiClient"/>, so a Logo or Netsis adapter reaching the same
/// <see cref="ErpBridge.Erp.Abstractions.ChangeLog.IErpChangeLogSource"/>
/// contract is driven by this service unchanged.
/// </para>
/// </summary>
public interface IErpChangeLogSyncService
{
    /// <summary>
    /// Run a single cycle. Failures are returned as a structured result rather
    /// than thrown, so the hosting worker never has to catch for expected
    /// business outcomes.
    /// </summary>
    Task<ErpChangeLogSyncResult> RunOnceAsync(CancellationToken ct = default);

    /// <summary>
    /// Clear the resume cursor so the next cycle re-reads everything the change
    /// log still holds. Used by the operator's "reset sync" action.
    /// </summary>
    Task InvalidateAsync(CancellationToken ct = default);
}

/// <summary>
/// Outcome of one <see cref="IErpChangeLogSyncService.RunOnceAsync"/>.
/// </summary>
/// <param name="Success">False when the cycle failed; see the error fields.</param>
/// <param name="TablesTouched">How many tables reported at least one change.</param>
/// <param name="UpsertRowsPushed">Insert/update events pushed.</param>
/// <param name="DeleteRowsPushed">Delete events pushed.</param>
/// <param name="MoreAvailable">
/// True when the ERP still had more pages ready. The worker may run again
/// immediately instead of waiting for the next tick.
/// </param>
/// <param name="DurationMs">Wall-clock duration.</param>
/// <param name="ErrorCode">Stable error code when <paramref name="Success"/> is false.</param>
/// <param name="ErrorMessage">Human-readable diagnostic.</param>
public sealed record ErpChangeLogSyncResult(
    bool Success,
    int TablesTouched,
    int UpsertRowsPushed,
    int DeleteRowsPushed,
    bool MoreAvailable,
    long DurationMs,
    string? ErrorCode = null,
    string? ErrorMessage = null)
{
    /// <summary>Nothing had changed since the last cycle.</summary>
    public static ErpChangeLogSyncResult Empty(long durationMs) =>
        new(Success: true, 0, 0, 0, MoreAvailable: false, durationMs);

    /// <summary>A failed cycle. The cursor is deliberately left where it was.</summary>
    public static ErpChangeLogSyncResult Failed(long durationMs, string code, string? message) =>
        new(Success: false, 0, 0, 0, MoreAvailable: false, durationMs, code, message);

    /// <summary>Total events pushed in this cycle.</summary>
    public int TotalRowsPushed => UpsertRowsPushed + DeleteRowsPushed;
}
