using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Endpoints;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Portal;

/// <summary>
/// The compact forms the panel keeps of <c>mobile_records</c> rows (see
/// <see cref="PortalRecordMirror{T}"/>), and the one place that reads their payloads. Mikro and
/// the native booking spell the same things differently; both spellings are handled here.
/// </summary>
public static class PortalRecords
{
    public const string NativeErp = "NATIVE";

    // ---- stock ------------------------------------------------------------------

    public static readonly string[] StockEntities = ["stocks", "inventory", "prices", "barcodes", "lookups"];

    public abstract record StockPart;

    /// <summary>A stock card: Mikro <c>mainGroupCode/subGroupCode/brandCode/shelfCode/unit1</c>, native <c>kategori/marka/shelfCode/birim</c>.</summary>
    public sealed record CardPart(string Code, string Name, string? Unit, string? MainGroup, string? SubGroup, string? Brand, string? Shelf) : StockPart;

    /// <summary>Mikro sends one company-wide quantity under the configured warehouse, reserved 0 and no date.</summary>
    public sealed record InventoryPart(string Code, int WarehouseNo, decimal Quantity, decimal Reserved, DateOnly? LastMovement) : StockPart;

    public sealed record PricePart(string Code, int ListNumber, decimal Price) : StockPart;

    public sealed record BarcodePart(string Code, string Barcode) : StockPart;

    public sealed record LookupPart(string Kind, int Number, string Name) : StockPart;

    public static StockPart? ParseStock(string entity, JsonElement item)
    {
        var code = Blank(AndroidEndpoints.GetString(item, "stockCode"));
        switch (entity)
        {
            case "stocks" when code is not null:
                return new CardPart(
                    code,
                    AndroidEndpoints.GetFirstString(item, "name", "urunAd"),
                    Blank(AndroidEndpoints.GetFirstString(item, "unit1", "birim", "unit")),
                    Blank(AndroidEndpoints.GetFirstString(item, "mainGroupCode", "kategori", "category")),
                    Blank(AndroidEndpoints.GetFirstString(item, "subGroupCode")),
                    Blank(AndroidEndpoints.GetFirstString(item, "brandCode", "marka")),
                    Blank(AndroidEndpoints.GetFirstString(item, "shelfCode", "sto_yer_kod")));
            case "inventory" when code is not null:
                return new InventoryPart(
                    code,
                    AndroidEndpoints.GetInt32(item, "warehouseNo") ?? 0,
                    AndroidEndpoints.GetDecimal(item, "quantity") ?? 0m,
                    AndroidEndpoints.GetDecimal(item, "reservedQuantity") ?? 0m,
                    ReadDay(AndroidEndpoints.GetString(item, "lastMovementDate")));
            case "prices" when code is not null:
                return AndroidEndpoints.GetDecimal(item, "price") is { } price and > 0 && AndroidEndpoints.GetInt32(item, "listNumber") is { } list and > 0
                    ? new PricePart(code, list, price)
                    : null;
            case "barcodes" when code is not null:
                return Blank(AndroidEndpoints.GetString(item, "barcode")) is { } barcode ? new BarcodePart(code, barcode) : null;
            case "lookups":
                var kind = AndroidEndpoints.GetString(item, "kind")?.Trim().ToLowerInvariant();
                return kind is "warehouse" or "price_list"
                       && AndroidEndpoints.GetInt32(item, "code") is { } number
                       && Blank(AndroidEndpoints.GetString(item, "name")) is { } name
                    ? new LookupPart(kind, number, name)
                    : null;
            default:
                return null;
        }
    }

    // ---- stock movement lines (STOK_HAREKETLERI mirror) -----------------------------

    public static readonly string[] LineEntities = ["stockTransactions"];

    /// <summary>
    /// A stock movement line. <see cref="DocumentKey"/> names the document it belongs to: Mikro
    /// by invoice record number (<c>faturaRecno</c> = <c>cha_recno</c>), native lines by customer
    /// and <c>evrakNo</c>; null when the line belongs to no document the ledger can open.
    /// </summary>
    public sealed record LinePart(
        string StockCode, DateOnly? Day, string? DocumentKey, decimal Quantity, decimal UnitPrice, decimal Amount,
        decimal? Tax, int? WarehouseNo, string? Description);

    public static PortalRecordMirror<LinePart> Lines(IMemoryCache cache, Guid tenantId) =>
        PortalRecordMirror<LinePart>.For(cache, "lines", tenantId, LineEntities, ParseLine);

    public static LinePart? ParseLine(string entity, JsonElement item)
    {
        var stockCode = Blank(AndroidEndpoints.GetFirstString(item, "stokKod", "urunKod"));
        if (stockCode is null) return null;
        var quantity = AndroidEndpoints.GetDecimal(item, "cikisMiktar") is { } outQty and not 0 ? outQty
            : AndroidEndpoints.GetDecimal(item, "girisMiktar") is { } inQty and not 0 ? inQty
            : Math.Abs(AndroidEndpoints.GetDecimal(item, "miktar") ?? 0m);
        string? documentKey = null;
        if (AndroidEndpoints.GetInt32(item, "faturaRecno") is { } recNo and > 0) documentKey = ErpDocumentKey(recNo);
        else if (string.Equals(AndroidEndpoints.GetString(item, "erp"), NativeErp, StringComparison.OrdinalIgnoreCase)
                 && Blank(AndroidEndpoints.GetString(item, "cariKod")) is { } customer
                 && Blank(AndroidEndpoints.GetString(item, "evrakNo")) is { } documentNo)
            documentKey = NativeDocumentKey(customer, documentNo);
        return new LinePart(
            stockCode,
            ReadDay(AndroidEndpoints.GetString(item, "tarih")),
            documentKey,
            quantity,
            AndroidEndpoints.GetDecimal(item, "birimFiyat") ?? 0m,
            AndroidEndpoints.GetDecimal(item, "tutar") ?? 0m,
            AndroidEndpoints.GetDecimal(item, "vergi"),
            AndroidEndpoints.GetInt32(item, "cikisDepoNo") ?? AndroidEndpoints.GetInt32(item, "girisDepoNo"),
            Blank(AndroidEndpoints.GetString(item, "aciklama")));
    }

    public static string ErpDocumentKey(int recNo) => "r" + recNo.ToString(CultureInfo.InvariantCulture);

    public static string NativeDocumentKey(string customer, string documentNo) => "d" + customer.ToUpperInvariant() + "|" + documentNo;

    // ---- values -----------------------------------------------------------------------

    private static readonly string[] DateTimeFormats =
        ["yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFFFFFF", "yyyy-MM-dd", "dd.MM.yyyy HH:mm", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy"];

    /// <summary>Wall-clock dates as written: Mikro <c>tarih</c>, the phone's ISO or cash-book <c>dd.MM.yyyy HH:mm</c>.</summary>
    public static DateTime? ReadDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = value.Trim();
        if (DateTime.TryParseExact(text, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var wall)) return wall;
        return DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var instant) ? instant.DateTime : null;
    }

    public static DateOnly? ReadDay(string? value) => ReadDateTime(value) is { } at ? DateOnly.FromDateTime(at) : null;

    public static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
