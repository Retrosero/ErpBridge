namespace ErpBridge.Erp.Abstractions.Sync;

/// <summary>
/// Bootstrap snapshot read by the adapter from the ERP and pushed to the central API.
/// All collections are independent — empty means "no rows" rather than "not yet read".
/// Phase 5 fills every collection with the canonical Mikro bootstrap subset.
/// </summary>
/// <param name="PulledAtUtc">When the snapshot was pulled from the ERP.</param>
/// <param name="SourceDatabase">Database the snapshot was read from.</param>
/// <param name="Customers">Cari kart snapshot (CARI_HESAPLAR).</param>
/// <param name="CustomerAddresses">Cari adres snapshot (CARI_HESAP_ADRESLERI).</param>
/// <param name="CustomerContacts">Cari yetkili snapshot (CARI_HESAP_YETKILILERI).</param>
/// <param name="Stocks">Aktif stok kartları (STOKLAR, sto_pasif_fl=0).</param>
/// <param name="Barcodes">Stok barkodları (BARKOD_TANIMLARI).</param>
/// <param name="Prices">Satış fiyat listeleri (STOK_SATIS_FIYAT_LISTELERI).</param>
/// <param name="Inventory">Depo bazlı stok envanteri (STOK_HAREKETLERI agregatı).</param>
/// <param name="OpenOrders">Açık sipariş satırları (SIPARISLER, kapat_fl=0).</param>
/// <param name="CashAndBank">Kasa + banka snapshotu (KASALAR + BANKALAR).</param>
/// <param name="Lookups">Depo / plasiyer / ödeme planı / proje / SM / döviz lookup snapshot.</param>
/// <param name="SalesConditions">Customer/stock sales conditions (SATIS_SARTLARI).</param>
/// <param name="CustomerTransactions">Cari ledger rows (CARI_HESAP_HAREKETLERI).</param>
/// <param name="StockTransactions">Raw stock ledger rows (STOK_HAREKETLERI).</param>
/// <param name="PartialSection">Section key for a manual partial push; null for a complete snapshot.</param>
public sealed record SyncPackage(
    DateTime PulledAtUtc,
    string SourceDatabase,
    IReadOnlyList<CustomerPayload> Customers,
    IReadOnlyList<CustomerAddressPayload> CustomerAddresses,
    IReadOnlyList<CustomerContactPayload> CustomerContacts,
    IReadOnlyList<StockPayload> Stocks,
    IReadOnlyList<BarcodePayload> Barcodes,
    IReadOnlyList<PricePayload> Prices,
    IReadOnlyList<SalesConditionPayload> SalesConditions,
    IReadOnlyList<InventoryPayload> Inventory,
    IReadOnlyList<OpenOrderPayload> OpenOrders,
    IReadOnlyList<CashAndBankPayload> CashAndBank,
    IReadOnlyList<LookupPayload> Lookups,
    IReadOnlyList<CustomerTransactionPayload> CustomerTransactions,
    IReadOnlyList<StockTransactionPayload> StockTransactions,
    string? PartialSection = null)
{
    /// <summary>
    /// Build a metadata-only <see cref="SyncPackage"/> with empty row collections.
    /// Used by adapters (and tests) that need a placeholder snapshot — the
    /// central API rejects a non-null but completely-empty payload the same
    /// way it rejects a populated one, so this is the safe MVP default.
    /// </summary>
    /// <param name="pulledAtUtc">UTC instant the snapshot was taken (e.g. <c>DateTimeOffset.UtcNow</c>).</param>
    /// <param name="sourceDatabase">Source database name (e.g. <c>MikroDatabaseName</c>).</param>
    public static SyncPackage Empty(DateTimeOffset pulledAtUtc, string sourceDatabase) =>
        new(
            PulledAtUtc: pulledAtUtc.UtcDateTime,
            SourceDatabase: sourceDatabase,
            Customers: Array.Empty<CustomerPayload>(),
            CustomerAddresses: Array.Empty<CustomerAddressPayload>(),
            CustomerContacts: Array.Empty<CustomerContactPayload>(),
            Stocks: Array.Empty<StockPayload>(),
            Barcodes: Array.Empty<BarcodePayload>(),
            Prices: Array.Empty<PricePayload>(),
            SalesConditions: Array.Empty<SalesConditionPayload>(),
            Inventory: Array.Empty<InventoryPayload>(),
            OpenOrders: Array.Empty<OpenOrderPayload>(),
            CashAndBank: Array.Empty<CashAndBankPayload>(),
            Lookups: Array.Empty<LookupPayload>(),
            CustomerTransactions: Array.Empty<CustomerTransactionPayload>(),
            StockTransactions: Array.Empty<StockTransactionPayload>());
}

/// <summary>One changed ERP row. The key is the stable Mikro RECno (or a composite
/// key for derived lookup rows); payload is the original Android-compatible JSON.
/// </summary>
public sealed record BootstrapDeltaRow(string Key, string PayloadJson);

/// <summary>Incremental bootstrap envelope. The central API merges it into the
/// existing snapshot without changing the Android snapshot wire contract.</summary>
public sealed record BootstrapDelta(
    DateTime PulledAtUtc,
    string SourceDatabase,
    IReadOnlyDictionary<string, IReadOnlyList<BootstrapDeltaRow>> Upserts,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Deletes);

/// <summary>Server-side bootstrap presence/revision information.</summary>
public sealed record BootstrapState(bool Exists, DateTimeOffset? ReceivedAtUtc, string? Revision);
