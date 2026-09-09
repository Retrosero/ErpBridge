namespace ErpBridge.Erp.Abstractions.ChangeLog;

/// <summary>
/// A source of INSERT / UPDATE / DELETE events for one ERP database.
///
/// <para>
/// This is the seam that lets Mikro, Logo and Netsis share one working
/// mechanism while leaving room for back-ends that cannot support it. The
/// SQL Server implementations install shadow tables + triggers and page by a
/// monotonic identity; a REST back-end would poll <c>updated_at</c> or drain a
/// webhook queue behind the same three methods.
/// </para>
///
/// <para>
/// The caller never inspects <see cref="ErpSyncCursor"/>: it reads a batch,
/// pushes it downstream, and only then persists
/// <see cref="ErpChangeBatch.CursorAfter"/>. That ordering makes a crash
/// mid-push replay the batch rather than lose it — consumers must therefore
/// treat events as idempotent upserts.
/// </para>
/// </summary>
public interface IErpChangeLogSource
{
    /// <summary>What this source can actually offer. Callers gate their strategy on it.</summary>
    ChangeDetectionCapability Capability { get; }

    /// <summary>Catalog of the tables this source reports changes for.</summary>
    IErpTrackedTableCatalog Catalog { get; }

    /// <summary>
    /// True when the change-capture machinery is present and complete in the
    /// target database (all shadow tables and triggers installed). Sources that
    /// need no installation return <c>true</c>.
    /// </summary>
    Task<bool> IsInstalledAsync(CancellationToken ct = default);

    /// <summary>
    /// Create or repair the change-capture machinery. Must be idempotent — the
    /// agent calls it on every start-up. Sources that need no installation are
    /// a no-op.
    /// </summary>
    Task InstallAsync(CancellationToken ct = default);

    /// <summary>
    /// Read the next page of changes after <paramref name="cursor"/>.
    /// </summary>
    /// <param name="cursor">Resume token; <see cref="ErpSyncCursor.Start"/> on first run.</param>
    /// <param name="maxRows">Upper bound on events in the returned page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ErpChangeBatch> ReadChangesAsync(
        ErpSyncCursor cursor,
        int maxRows,
        CancellationToken ct = default);
}

/// <summary>
/// Durable store for the per-tenant, per-ERP change-log resume cursor. The
/// stored value is opaque — see <see cref="ErpSyncCursor"/>.
/// </summary>
public interface IErpSyncCursorStore
{
    /// <summary>Load the cursor; returns <see cref="ErpSyncCursor.Start"/> when nothing is stored yet.</summary>
    Task<ErpSyncCursor> GetAsync(string tenantId, ErpType erp, CancellationToken ct = default);

    /// <summary>Persist the cursor after a batch has been durably accepted downstream.</summary>
    Task SetAsync(string tenantId, ErpType erp, ErpSyncCursor cursor, CancellationToken ct = default);

    /// <summary>Clear the cursor so the next sync starts from scratch. Returns rows removed.</summary>
    Task<int> ResetAsync(string tenantId, ErpType erp, CancellationToken ct = default);
}
