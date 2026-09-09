using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.Erp.Logo.ChangeLog;
using ErpBridge.Erp.Sql;

namespace ErpBridge.Erp.Logo;

/// <summary>
/// Logo (Tiger / GO / WINGS) adapter — <b>skeleton</b>.
///
/// <para>
/// Faz 21 builds this to answer one question: can a second ERP satisfy
/// <see cref="IErpAdapter"/> without changing anything shared? The answer this
/// file demonstrates is yes for the parts that are genuinely reusable — change
/// capture comes free from <see cref="SqlServerShadowTableChangeLog"/> given
/// only a catalog and a key projection — and the write path is where the real
/// work lives, because Logo's document model differs from Mikro's throughout.
/// </para>
///
/// <para>
/// Every write throws <see cref="NotImplementedException"/> on purpose. A
/// half-written Logo document is worse than a refusal: it would land in a
/// customer's ledger. The bootstrap read is likewise unimplemented rather than
/// returning an empty snapshot, which the central API would happily accept as
/// "this customer has no data".
/// </para>
/// </summary>
public sealed class LogoAdapter : IErpAdapter
{
    private readonly Func<string> _connectionStringResolver;
    private readonly Lazy<IErpChangeLogSource> _changeLog;

    /// <summary>
    /// Shadow-table naming for Logo. Deliberately distinct from Mikro's so both
    /// can be installed on the same SQL Server without colliding.
    /// </summary>
    public static ShadowTableOptions ShadowOptions { get; } = new(
        SyncTable: "_ERPB_LOGO_SYNC",
        SyncDelTable: "_ERPB_LOGO_SYNC_DEL",
        TriggerSuffix: "_ERPB_LOGO_SYNC",
        DeleteTriggerSuffix: "_ERPB_LOGO_SYNC_DEL");

    /// <summary>Build the adapter for one firm/period.</summary>
    public LogoAdapter(Func<string> connectionStringResolver, int firmNumber = 1, int periodNumber = 1)
    {
        _connectionStringResolver = connectionStringResolver
            ?? throw new ArgumentNullException(nameof(connectionStringResolver));

        Catalog = new LogoTrackedTableCatalog(firmNumber, periodNumber);
        _changeLog = new Lazy<IErpChangeLogSource>(() => new SqlServerShadowTableChangeLog(
            catalog: Catalog,
            connectionStringResolver: _connectionStringResolver,
            // Logo keys every row by an int LOGICALREF, so the recno projection
            // applies unchanged — only the tag differs.
            projection: new KeyKindProjection(TaggedKeyProjection.LogicalRef, TaggedKeyProjection.Guid),
            options: ShadowOptions));
    }

    /// <summary>The firm/period-bound table catalog this adapter tracks.</summary>
    public LogoTrackedTableCatalog Catalog { get; }

    /// <inheritdoc />
    public ChangeDetectionCapability ChangeDetection => ChangeDetectionCapability.ShadowTableChangeLog;

    /// <inheritdoc />
    public IErpChangeLogSource? ChangeLog => _changeLog.Value;

    /// <inheritdoc />
    public Task<ErpConnectionTestResult> TestConnectionAsync(CancellationToken ct = default) =>
        throw new NotImplementedException("Logo adapter is a Faz 21 skeleton; connection test is not implemented.");

    /// <inheritdoc />
    public Task<ErpVersionInfo> DetectVersionAsync(CancellationToken ct = default) =>
        throw new NotImplementedException("Logo adapter is a Faz 21 skeleton; version detection is not implemented.");

    /// <inheritdoc />
    public Task<SyncPackage> ReadBootstrapDataAsync(CancellationToken ct = default) =>
        throw new NotImplementedException(
            "Logo adapter is a Faz 21 skeleton. Returning an empty snapshot would be read by the " +
            "central API as 'this customer has no data', so it refuses instead.");

    /// <inheritdoc />
    public Task<SyncPackage> ReadBootstrapSectionAsync(string sectionName, CancellationToken ct = default) =>
        throw new NotImplementedException("Logo adapter is a Faz 21 skeleton; sectioned bootstrap read is not implemented.");

    /// <inheritdoc />
    public Task<ErpWriteResult> WriteSalesOrderAsync(SalesOrderPayload payload, CancellationToken ct = default) =>
        throw new NotImplementedException(Unwritable("sales order"));

    /// <inheritdoc />
    public Task<ErpWriteResult> WriteInvoiceAsync(InvoicePayload payload, CancellationToken ct = default) =>
        throw new NotImplementedException(Unwritable("invoice"));

    /// <inheritdoc />
    public Task<ErpWriteResult> WriteCollectionAsync(CollectionPayload payload, CancellationToken ct = default) =>
        throw new NotImplementedException(Unwritable("collection"));

    /// <inheritdoc />
    public Task<ErpWriteResult> WriteDispatchNoteAsync(DispatchNotePayload payload, CancellationToken ct = default) =>
        throw new NotImplementedException(Unwritable("dispatch note"));

    /// <inheritdoc />
    public Task<ErpWriteResult> WritePaymentOrderAsync(PaymentOrderPayload payload, CancellationToken ct = default) =>
        throw new NotImplementedException(Unwritable("payment order"));

    /// <inheritdoc />
    public Task<ErpWriteResult> WriteCustomerCardAsync(CreateCustomerRequest request, CancellationToken ct = default) =>
        throw new NotImplementedException(Unwritable("customer card"));

    /// <inheritdoc />
    public Task<ErpWriteResult> WriteStockCardAsync(CreateStockRequest request, CancellationToken ct = default) =>
        throw new NotImplementedException(Unwritable("stock card"));

    private static string Unwritable(string document) =>
        $"Logo adapter is a Faz 21 skeleton and cannot write a {document}. " +
        "Refusing is deliberate: a partially-written document would land in the customer's ledger.";
}
