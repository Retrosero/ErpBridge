using ErpBridge.Erp.Abstractions.Sync;

namespace ErpBridge.Erp.Mikro.Readers;

/// <summary>
/// Reads the bootstrap master-data snapshot directly from the Mikro SQL Server
/// database. Every method is fully <c>async</c>, uses Dapper with named
/// parameters (no string concatenation), and returns a non-null empty list
/// when the underlying table is empty so callers don't need to special-case
/// "no rows" vs "reader failure".
/// </summary>
/// <remarks>
/// All readers take <c>firmNo</c> because Mikro multi-firm installations share
/// a single database where every master table carries <c>*_firmano</c> or
/// <c>*_firma_no</c>. The warehouse-specific inventory reader additionally
/// takes <c>warehouseNo</c>. Readers must never open a connection themselves —
/// the injected <see cref="Connection.MikroConnectionFactory"/> owns the
/// connection-string formatting.
/// </remarks>
public interface IMikroDbReader
{
    /// <summary>
    /// Read all caris (customers and vendors) from <c>CARI_HESAPLAR</c> for the
    /// given <paramref name="firmNo"/>. Optional child collections (addresses,
    /// contacts) are filled from the dedicated tables downstream — Phase 5 keeps
    /// them empty.
    /// </summary>
    Task<IReadOnlyList<CustomerPayload>> ReadCustomersAsync(int firmNo, CancellationToken ct = default);

    /// <summary>
    /// Read all active stoks (items) from <c>STOKLAR</c> for the given
    /// <paramref name="firmNo"/> where <c>sto_pasif_fl = 0</c>. Passive rows are
    /// excluded so the central API doesn't see archived SKUs.
    /// </summary>
    Task<IReadOnlyList<StockPayload>> ReadStocksAsync(int firmNo, CancellationToken ct = default);

    /// <summary>
    /// Read all open sales-order lines from <c>SIPARISLER</c> for the given
    /// <paramref name="firmNo"/> where <c>sip_kapat_fl = 0</c>. The
    /// <c>RemainingQuantity</c> column on <see cref="OpenOrderPayload"/> is
    /// denormalised as <c>Quantity - DeliveredQuantity</c>.
    /// </summary>
    Task<IReadOnlyList<OpenOrderPayload>> ReadOpenOrdersAsync(int firmNo, CancellationToken ct = default);

    /// <summary>
    /// Read kasa + banka records (cash and bank accounts) as a single typed
    /// stream where <see cref="CashAndBankPayload.Kind"/> is either
    /// <c>"cash"</c> or <c>"bank"</c>.
    /// </summary>
    Task<IReadOnlyList<CashAndBankPayload>> ReadCashAndBankAsync(int firmNo, CancellationToken ct = default);

    /// <summary>
    /// Read the closed-set of lookup dimensions (warehouse, salesperson,
    /// payment-plan, project, currency) as a single uniform stream where
    /// <see cref="LookupPayload.Kind"/> disambiguates the source master.
    /// </summary>
    Task<IReadOnlyList<LookupPayload>> ReadLookupsAsync(int firmNo, CancellationToken ct = default);

    /// <summary>
    /// Read all aktif price-list rows from <c>STOK_SATIS_FIYAT_LISTELERI</c>
    /// for the given <paramref name="firmNo"/>. Rows with null price are kept
    /// (mirroring what Mikro actually stores).
    /// </summary>
    Task<IReadOnlyList<PricePayload>> ReadPricesAsync(int firmNo, CancellationToken ct = default);

    /// <summary>
    /// Read the on-hand inventory for a single warehouse by aggregating
    /// <c>STOK_HAREKETLERI</c> over the supplied <paramref name="firmNo"/> and
    /// <paramref name="warehouseNo"/>. Tip values &lt; 4 count as inflow,
    /// tips ≥ 4 as outflow so net quantity is sign-correct.
    /// </summary>
    Task<IReadOnlyList<InventoryPayload>> ReadInventoryAsync(int firmNo, int warehouseNo, CancellationToken ct = default);
}
