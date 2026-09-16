using System.Globalization;
using System.Text.Json;

namespace ErpBridge.Portal.Api;

public sealed record DocumentField(string Label, string Value);

/// <summary>A document line as the phone sent it; absent numbers stay null.</summary>
public sealed record DocumentLine(
    string Code, string Name, decimal? Quantity, string? Unit, decimal? UnitPrice, decimal? Total,
    decimal? Expected, decimal? Counted, string? Note,
    decimal? DiscountPercent = null, decimal? DiscountAmount = null, decimal? VatRate = null, decimal? ConditionPercent = null);

/// <summary>One document of an approval request, laid out for reading.</summary>
public sealed record DocumentView(string Title, IReadOnlyList<DocumentField> Fields, IReadOnlyList<DocumentLine> Lines, IReadOnlyList<DocumentField> Other)
{
    public bool IsCount => Lines.Any(l => l.Expected is not null || l.Counted is not null);
    public bool HasDiscount => Lines.Any(l => l.DiscountPercent is not null || l.DiscountAmount is not null);
    public bool HasVat => Lines.Any(l => l.VatRate is not null);

    /// <summary>A returned item's condition (share of its value credited back), sent by the phone's return form.</summary>
    public bool HasCondition => Lines.Any(l => l.ConditionPercent is not null);
}

/// <summary>
/// Turns the <c>documents</c> of an approval request — the ingest documents the phone would post,
/// <c>[{documentType, externalId, payload}]</c> — into what a person reads. Field names follow
/// <c>NativeDocumentProcessor</c> and <c>FulfillmentService.ReadOrder</c> on the server; a field this
/// class does not know is still shown under "other" rather than dropped.
/// </summary>
public static class ApprovalDocuments
{
    private static readonly Dictionary<string, string> TypeLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sales_order"] = "Satış",
        ["sales_return"] = "İade",
        ["purchase_receipt"] = "Alış",
        ["collection"] = "Tahsilat",
        ["disbursement"] = "Tediye",
        ["stock_count"] = "Sayım",
        ["stock_card"] = "Ürün kartı",
        ["stock_card_delete"] = "Ürün kartı silme",
        ["customer_card"] = "Cari kartı",
    };

    /// <summary>Header fields in reading order: payload key → label.</summary>
    private static readonly (string Key, string Label)[] HeaderFields =
    [
        ("mobileDocumentId", "Belge no"),
        ("invoiceNo", "Fatura no"),
        ("occurredAt", "Tarih"),
        ("date", "Tarih"),
        ("customerCode", "Cari kodu"),
        ("supplierCode", "Tedarikçi kodu"),
        ("counterparty", "Cari"),
        ("customerName", "Cari"),
        ("transactionType", "İşlem"),
        ("paymentType", "Ödeme şekli"),
        ("dueDate", "Vade"),
        ("warehouse", "Depo"),
        ("amount", "Tutar"),
        ("description", "Açıklama"),
        // cards
        ("stockCode", "Stok kodu"),
        ("name", "Ad"),
        ("title", "Unvan"),
        ("barcode", "Barkod"),
        ("unit", "Birim"),
        ("price", "Fiyat"),
        ("vatRate", "KDV oranı"),
        ("brand", "Marka"),
        ("category", "Kategori"),
        ("openingQuantity", "Açılış miktarı"),
        ("phone", "Telefon"),
        ("address", "Adres"),
        ("taxNumber", "Vergi no"),
        ("taxOffice", "Vergi dairesi"),
    ];

    private static readonly HashSet<string> MoneyKeys = new(StringComparer.Ordinal) { "amount", "price" };
    private static readonly HashSet<string> DateKeys = new(StringComparer.Ordinal) { "occurredAt", "date", "dueDate" };

    public static IReadOnlyList<DocumentView> Read(JsonElement documents)
    {
        if (documents.ValueKind != JsonValueKind.Array) return [];
        var views = new List<DocumentView>();
        foreach (var document in documents.EnumerateArray())
        {
            if (document.ValueKind != JsonValueKind.Object) continue;
            var type = Text(document, "documentType") ?? string.Empty;
            var payload = document.TryGetProperty("payload", out var p) && p.ValueKind == JsonValueKind.Object ? p : default;
            views.Add(ReadOne(type, payload));
        }
        return views;
    }

    public static string TypeLabel(string documentType) =>
        TypeLabels.TryGetValue(documentType, out var label) ? label : documentType;

    private static DocumentView ReadOne(string type, JsonElement payload)
    {
        var fields = new List<DocumentField>();
        var other = new List<DocumentField>();
        var lines = new List<DocumentLine>();
        if (payload.ValueKind != JsonValueKind.Object) return new DocumentView(TypeLabel(type), fields, lines, other);

        var known = new HashSet<string>(StringComparer.Ordinal) { "lines" };
        var labelsShown = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (key, label) in HeaderFields)
        {
            if (!payload.TryGetProperty(key, out var value)) continue;
            known.Add(key);
            var text = DateKeys.Contains(key) ? DateText(value) : Scalar(value, MoneyKeys.Contains(key));
            // "counterparty" and "customerName" (or "occurredAt" and "date") name the same thing.
            if (text is null || !labelsShown.Add(label)) continue;
            fields.Add(new DocumentField(label, text));
        }

        if (payload.TryGetProperty("lines", out var lineArray) && lineArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var line in lineArray.EnumerateArray())
            {
                if (line.ValueKind != JsonValueKind.Object) continue;
                var quantity = Number(line, "quantity");
                var unitPrice = Number(line, "unitPrice");
                lines.Add(new DocumentLine(
                    Text(line, "productCode") ?? Text(line, "stockCode") ?? Text(line, "barcode") ?? string.Empty,
                    Text(line, "productTitle") ?? Text(line, "name") ?? Text(line, "title") ?? string.Empty,
                    quantity,
                    Text(line, "unit"),
                    unitPrice,
                    Number(line, "lineTotal") ?? (quantity is { } q && unitPrice is { } u ? q * u : null),
                    Number(line, "expectedQuantity"),
                    Number(line, "countedQuantity"),
                    Text(line, "reason") ?? Text(line, "note"),
                    Number(line, "discountPercent") ?? Number(line, "discountRate") ?? Number(line, "iskontoOrani"),
                    Number(line, "discountAmount") ?? Number(line, "discount") ?? Number(line, "iskontoTutari"),
                    Number(line, "vatRate") ?? Number(line, "kdvOrani") ?? Number(line, "taxRate"),
                    Number(line, "conditionPercent")));
            }
        }

        foreach (var property in payload.EnumerateObject())
        {
            if (known.Contains(property.Name)) continue;
            if (Scalar(property.Value, money: false) is { } text) other.Add(new DocumentField(property.Name, text));
        }
        return new DocumentView(TypeLabel(type), fields, lines, other);
    }

    private static string? Scalar(JsonElement value, bool money) => value.ValueKind switch
    {
        JsonValueKind.String when !string.IsNullOrWhiteSpace(value.GetString()) => value.GetString()!.Trim(),
        JsonValueKind.Number when value.TryGetDecimal(out var number) => money ? Fmt.Money(number) : number.ToString("0.###", Fmt.Turkish),
        JsonValueKind.True => "Evet",
        JsonValueKind.False => "Hayır",
        _ => null,
    };

    /// <summary>The phone writes local times without an offset ("2026-09-16T14:00:00"); they are shown as written.</summary>
    private static string? DateText(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.String) return Scalar(value, money: false);
        var raw = value.GetString();
        if (!DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var at)) return Scalar(value, money: false);
        return at.TimeOfDay == TimeSpan.Zero && raw!.Length <= 10
            ? at.ToString("dd.MM.yyyy", Fmt.Turkish)
            : at.ToString("dd.MM.yyyy HH:mm", Fmt.Turkish);
    }

    private static string? Text(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()!.Trim()
            : null;

    private static decimal? Number(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number)) return number;
        return value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}
