using System.Text.Json;

namespace ErpBridge.CentralApi.Sync;

/// <summary>
/// Decides what identifies a mobile record, for every writer.
///
/// <para>This logic used to live as a private method inside
/// <c>BootstrapUploadEndpoints</c>, which made the snapshot merge the only code
/// in the system that knew a price row is identified by stock code <i>and</i>
/// list number, or a sales condition by four fields together. Every other path —
/// the delete queue above all — guessed, and guessed differently, which is why
/// an ERP deletion never matched the row it was supposed to remove. One key
/// definition, shared by all writers, is what makes the delete path line up with
/// the upsert path at all.</para>
/// </summary>
public static class MobileRecordKey
{
    /// <summary>Separator between the parts of a composite key.</summary>
    public const char Separator = '|';

    /// <summary>
    /// Every snapshot section that becomes a mobile entity. Sections absent from
    /// this list have no stable identity and are not projected.
    /// </summary>
    public static readonly string[] Entities =
    [
        "customers", "customerAddresses", "customerContacts",
        "stocks", "barcodes", "prices", "salesConditions", "inventory",
        "openOrders", "cashAndBank", "lookups",
        "customerTransactions", "stockTransactions",
    ];

    /// <summary>
    /// Business key for a row of <paramref name="section"/>, or null when the
    /// section has no stable identity — such a row can only be replaced
    /// wholesale, never addressed individually.
    /// </summary>
    public static string? For(string section, JsonElement item)
    {
        string? Value(string name) =>
            item.ValueKind == JsonValueKind.Object
            && item.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
                ? value.ToString()
                : null;

        string? Join(params string[] names)
        {
            var values = names.Select(Value).ToArray();
            return values.Any(string.IsNullOrWhiteSpace) ? null : string.Join(Separator, values);
        }

        return section.ToLowerInvariant() switch
        {
            "customers" => Join("customerCode"),
            "customeraddresses" => Join("customerCode", "addressNo"),
            "customercontacts" => Join("customerCode", "email", "mobile"),
            "stocks" => Join("stockCode"),
            "barcodes" => Join("barcode"),
            "prices" => Join("stockCode", "listNumber"),
            "salesconditions" => Join("stockCode", "customerCode", "warehouseNo", "paymentPlanNo"),
            "inventory" => Join("stockCode", "warehouseNo"),
            "openorders" => Join("series", "number", "lineNo"),
            "cashandbank" or "lookups" => Join("kind", "code"),
            "customertransactions" or "stocktransactions" => Join("id"),
            _ => null,
        };
    }

    /// <summary>
    /// The ERP's physical identity the upload carried for this row (RECno or
    /// Guid), normalised for lookup; null when the section does not carry one.
    /// </summary>
    public static string? SourceKey(string section, JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object) return null;
        if (!item.TryGetProperty("recordKey", out var value) || value.ValueKind == JsonValueKind.Null) return null;
        return NormalizeSourceKey(value.ToString());
    }

    /// <summary>
    /// One spelling for an ERP identity on both sides of the lookup: the change
    /// log renders a Guid lower-case, SQL Server renders it upper-case.
    /// </summary>
    public static string? NormalizeSourceKey(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    /// <summary>
    /// The stock card and customer this row hangs off, so a parent's deletion can
    /// cascade with an indexed update. Either part is null when the entity has no
    /// such parent.
    ///
    /// <para>Ledger movements return <c>(null, null)</c> on purpose. Deleting a
    /// stock card in Mikro does not delete its movements, so evicting them here
    /// would hide documents the ERP still has.</para>
    /// </summary>
    public static (string? StockKey, string? CustomerKey) Parents(string section, JsonElement item)
    {
        string? Value(string name) =>
            item.ValueKind == JsonValueKind.Object
            && item.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
                ? Trimmed(value.ToString())
                : null;

        return section.ToLowerInvariant() switch
        {
            "customers" or "customeraddresses" or "customercontacts" => (null, Value("customerCode")),
            "stocks" or "barcodes" or "prices" or "inventory" => (Value("stockCode"), null),
            "salesconditions" => (Value("stockCode"), Value("customerCode")),
            _ => (null, null),
        };
    }

    /// <summary>
    /// Entity a deletion in <paramref name="tableName"/> lands on, paired with how
    /// the change log identifies the row.
    ///
    /// <para><see cref="DeleteTarget.ByBusinessKey"/> means the shadow log's RECno
    /// has to be translated into the code the snapshot is keyed by
    /// (<c>sto_kod</c>, <c>cari_kod</c>) before anything can be matched; the
    /// deletion then cascades to the record's children. Movement tables are keyed
    /// by the physical record number in both worlds, so no translation happens and
    /// nothing cascades.</para>
    /// </summary>
    public static DeleteTarget? DeleteTargetFor(string tableName) => tableName.ToUpperInvariant() switch
    {
        "STOKLAR" => new DeleteTarget("stocks", "sto_kod", Cascade.Stock),
        "CARI_HESAPLAR" => new DeleteTarget("customers", "cari_kod", Cascade.Customer),
        "CARI_HESAP_HAREKETLERI" => new DeleteTarget("customerTransactions", null, Cascade.None),
        "STOK_HAREKETLERI" => new DeleteTarget("stockTransactions", null, Cascade.None),
        _ => null,
    };

    /// <summary>Where a deletion in one ERP table reaches.</summary>
    /// <param name="Entity">Entity holding the deleted row itself.</param>
    /// <param name="BusinessKeyColumn">ERP column carrying the business code, or null when the record key is already it.</param>
    /// <param name="Cascade">Which family of child records goes with it.</param>
    public readonly record struct DeleteTarget(string Entity, string? BusinessKeyColumn, Cascade Cascade);

    /// <summary>Which parent column a deletion cascades through.</summary>
    public enum Cascade
    {
        /// <summary>Only the named record is removed.</summary>
        None = 0,

        /// <summary>Everything hanging off the stock card: barcodes, prices, inventory, sales conditions.</summary>
        Stock = 1,

        /// <summary>Everything hanging off the customer: addresses, contacts, sales conditions.</summary>
        Customer = 2,
    }

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
