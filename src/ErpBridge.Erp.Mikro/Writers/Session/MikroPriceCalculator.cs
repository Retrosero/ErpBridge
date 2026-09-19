using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Writers.Session;

/// <summary>
/// One priced stock line as Mikro stores it (reference §1 "İskonto zinciri", goal D8/D9): the gross
/// amount before discounts and VAT, each discount as an amount taken from what the previous one left,
/// and the VAT on the discounted net. Every amount is rounded to 2 decimals, midpoint away from zero.
/// </summary>
/// <param name="UnitPrice">Unit price without VAT (the list price converted when the list includes VAT).</param>
/// <param name="Quantity">Quantity in the line's unit.</param>
/// <param name="Gross"><c>sth_tutar</c>: unit price × quantity.</param>
/// <param name="Discount1"><c>sth_iskonto1</c>.</param>
/// <param name="Discount2"><c>sth_iskonto2</c>.</param>
/// <param name="Discount3"><c>sth_iskonto3</c>.</param>
/// <param name="VatPointer"><c>sth_vergi_pntr</c>.</param>
/// <param name="VatRate">The pointer's rate in percent (<c>fn_VergiYuzde</c>).</param>
/// <param name="Vat"><c>sth_vergi</c>.</param>
public sealed record MikroPricedLine(
    decimal UnitPrice,
    decimal Quantity,
    decimal Gross,
    decimal Discount1,
    decimal Discount2,
    decimal Discount3,
    byte VatPointer,
    decimal VatRate,
    decimal Vat)
{
    public decimal Net => Gross - Discount1 - Discount2 - Discount3;

    public decimal Total => Net + Vat;
}

/// <summary>A document's priced lines and the header figures Mikro keeps as their sums (D9: no header rounding).</summary>
public sealed record MikroPricedDocument(IReadOnlyList<MikroPricedLine> Lines)
{
    /// <summary><c>cha_aratoplam</c>.</summary>
    public decimal Gross => Lines.Sum(l => l.Gross);

    public decimal Discount1 => Lines.Sum(l => l.Discount1);
    public decimal Discount2 => Lines.Sum(l => l.Discount2);
    public decimal Discount3 => Lines.Sum(l => l.Discount3);
    public decimal Vat => Lines.Sum(l => l.Vat);

    /// <summary><c>cha_meblag</c>: gross − discounts + VAT.</summary>
    public decimal Total => Lines.Sum(l => l.Total);

    /// <summary>KDV'siz toplam — tedarikçi fiyatı KDV hariçse telefonun gösterdiği rakam budur.</summary>
    public decimal Net => Lines.Sum(l => l.Net);

    /// <summary>
    /// VAT per <c>cha_vergi1..5</c> bucket, as Fora files it: pointer 2 → bucket 2, 3 → 3, 4 → 4, 5 → 5,
    /// anything else → bucket 1 (reference §2).
    /// </summary>
    public decimal VatBucket(int bucket) => Lines.Where(l => Bucket(l.VatPointer) == bucket).Sum(l => l.Vat);

    private static int Bucket(byte pointer) => pointer is >= 2 and <= 5 ? pointer : 1;
}

/// <summary>Mikro's line arithmetic for phone documents (goal ERP yazım Y3b). Pure: no database.</summary>
public static class MikroPriceCalculator
{
    /// <summary>The phone's total may differ from Mikro's by at most this much (D8).</summary>
    public const decimal TotalTolerance = 0.05m;

    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    /// <summary>A sale line: list price, quantity and the three chained discount percentages.</summary>
    /// <param name="listUnitPrice">The price on the phone, taken from the ERP price list.</param>
    /// <param name="quantity">Quantity.</param>
    /// <param name="lineDiscountPercent">Line discount (<c>sth_iskonto1</c>).</param>
    /// <param name="customerDiscountPercent">Customer discount (<c>sth_iskonto2</c>).</param>
    /// <param name="generalDiscountPercent">Basket discount (<c>sth_iskonto3</c>).</param>
    /// <param name="vatPointer">The stock card's VAT pointer.</param>
    /// <param name="vatRate">That pointer's rate in percent.</param>
    /// <param name="listIncludesVat">The price list's <c>sfl_kdvdahil</c>.</param>
    public static MikroPricedLine SaleLine(
        decimal listUnitPrice, decimal quantity, decimal lineDiscountPercent, decimal customerDiscountPercent, decimal generalDiscountPercent,
        byte vatPointer, decimal vatRate, bool listIncludesVat)
    {
        var unit = UnitWithoutVat(listUnitPrice, vatRate, listIncludesVat);
        var gross = Round(unit * quantity);
        var d1 = Round(gross * lineDiscountPercent / 100m);
        var d2 = Round((gross - d1) * customerDiscountPercent / 100m);
        var d3 = Round((gross - d1 - d2) * generalDiscountPercent / 100m);
        var vat = Round((gross - d1 - d2 - d3) * vatRate / 100m);
        return new MikroPricedLine(unit, quantity, gross, d1, d2, d3, vatPointer, vatRate, vat);
    }

    /// <summary>
    /// A return line: the refunded share of the list price. The part not refunded (a damaged item's
    /// condition difference) is <c>sth_iskonto1</c>; a return line has no other discount (D11).
    /// </summary>
    /// <param name="listUnitPrice">The price on the phone, taken from the ERP price list.</param>
    /// <param name="quantity">Quantity.</param>
    /// <param name="conditionRatio">Refunded share, 0..1.</param>
    /// <param name="vatPointer">The stock card's VAT pointer.</param>
    /// <param name="vatRate">That pointer's rate in percent.</param>
    /// <param name="listIncludesVat">The price list's <c>sfl_kdvdahil</c>.</param>
    public static MikroPricedLine ReturnLine(
        decimal listUnitPrice, decimal quantity, decimal conditionRatio, byte vatPointer, decimal vatRate, bool listIncludesVat)
    {
        var unit = UnitWithoutVat(listUnitPrice, vatRate, listIncludesVat);
        var gross = Round(unit * quantity);
        var condition = Round(gross * (1m - conditionRatio));
        var vat = Round((gross - condition) * vatRate / 100m);
        return new MikroPricedLine(unit, quantity, gross, condition, 0m, 0m, vatPointer, vatRate, vat);
    }

    /// <summary>
    /// An alış line: the supplier's own unit price, with VAT taken from the stock card (K6). A purchase
    /// carries no discount chain — what the supplier charged is what goes in; a discount the supplier gave
    /// is already inside the price they invoiced.
    /// </summary>
    /// <param name="unitPrice">The supplier's unit price as the phone recorded it.</param>
    /// <param name="quantity">Quantity.</param>
    /// <param name="vatPointer">The stock card's VAT pointer.</param>
    /// <param name="vatRate">That pointer's rate in percent.</param>
    /// <param name="priceIncludesVat">Whether the supplier's price already contains VAT.</param>
    public static MikroPricedLine PurchaseLine(
        decimal unitPrice, decimal quantity, byte vatPointer, decimal vatRate, bool priceIncludesVat)
    {
        var unit = UnitWithoutVat(unitPrice, vatRate, priceIncludesVat);
        var gross = Round(unit * quantity);
        var vat = Round(gross * vatRate / 100m);
        return new MikroPricedLine(unit, quantity, gross, 0m, 0m, 0m, vatPointer, vatRate, vat);
    }

    /// <summary>
    /// Refuses a purchase whose Mikro figure differs from what the phone showed. Which figure to compare
    /// depends on the prices: when the supplier's prices exclude VAT the phone's total is the net, and
    /// comparing it against Mikro's VAT-inclusive total would reject every correct document.
    /// </summary>
    /// <exception cref="MikroWriteException">The difference is more than <see cref="TotalTolerance"/>.</exception>
    public static void EnsurePurchaseTotal(MikroPricedDocument document, decimal expectedTotal, bool pricesIncludeVat)
    {
        ArgumentNullException.ThrowIfNull(document);
        var mikro = pricesIncludeVat ? document.Total : document.Net;
        var difference = mikro - expectedTotal;
        if (Math.Abs(difference) > TotalTolerance)
        {
            throw new MikroWriteException(ErpWriteError.TotalMismatch(difference));
        }
    }

    /// <summary>Refuses a document whose Mikro total differs from what the phone showed (<c>TOTAL_MISMATCH</c>, D8).</summary>
    /// <exception cref="MikroWriteException">The difference is more than <see cref="TotalTolerance"/>.</exception>
    public static void EnsureTotal(MikroPricedDocument document, decimal expectedTotal)
    {
        ArgumentNullException.ThrowIfNull(document);
        var difference = document.Total - expectedTotal;
        if (Math.Abs(difference) > TotalTolerance)
        {
            throw new MikroWriteException(ErpWriteError.TotalMismatch(difference));
        }
    }

    /// <summary>A price from a list that includes VAT is stored without it; the unit price itself is not rounded.</summary>
    private static decimal UnitWithoutVat(decimal listUnitPrice, decimal vatRate, bool listIncludesVat) =>
        listIncludesVat && vatRate != 0m ? listUnitPrice / (1m + vatRate / 100m) : listUnitPrice;
}
