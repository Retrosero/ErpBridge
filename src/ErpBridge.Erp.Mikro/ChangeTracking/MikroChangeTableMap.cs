namespace ErpBridge.Erp.Mikro.ChangeTracking;

/// <summary>
/// Stable mapping between Mikro base tables and Android bootstrap sections.
/// Table names are constants owned by the adapter and are never sourced from user input.
/// </summary>
public static class MikroChangeTableMap
{
    public static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Tables =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["CARI_HESAPLAR"] = Set("customers"),
            ["CARI_HESAP_ADRESLERI"] = Set("customers"),
            ["CARI_HESAP_YETKILILERI"] = Set("customers"),
            ["STOKLAR"] = Set("stocks"),
            ["BARKOD_TANIMLARI"] = Set("stocks"),
            ["SIPARISLER"] = Set("openOrders"),
            ["KASALAR"] = Set("cashAndBank"),
            ["BANKALAR"] = Set("cashAndBank"),
            ["DEPOLAR"] = Set("lookups"),
            ["CARI_PERSONEL_TANIMLARI"] = Set("lookups"),
            ["ODEME_PLANLARI"] = Set("lookups"),
            ["PROJELER"] = Set("lookups"),
            ["STOK_SATIS_FIYAT_LISTE_TANIMLARI"] = Set("prices"),
            ["STOK_SATIS_FIYAT_LISTELERI"] = Set("prices"),
            ["SATIS_SARTLARI"] = Set("prices"),
            ["CARI_HESAP_HAREKETLERI"] = Set("customerTransactions"),
            ["STOK_HAREKETLERI"] = Set("stockTransactions", "inventory"),
        };

    private static IReadOnlySet<string> Set(params string[] values) =>
        new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
}
