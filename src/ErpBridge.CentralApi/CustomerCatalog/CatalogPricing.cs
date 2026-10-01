namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// The sale arithmetic of Sipariş Cepte's <c>ErpSalePricing</c> (and so of the ERP writer's <c>MikroPriceCalculator</c>,
/// which CentralApi does not reference), so the catalog's request total is the total the phone books when staff turn it
/// into a sale (GOAL_MUSTERI_KATALOGU §4). Per line: gross = list price × quantity; the line, customer and basket
/// discounts each take a share of what the previous one left, as amounts; VAT is on the discounted net. Every amount is
/// rounded to 2 decimals, half away from zero; a VAT-inclusive list price is first divided by (1 + rate), unrounded.
/// The catalog uses only the customer discount: the account's percent, or none for a <c>noDiscount</c> product.
/// </summary>
public static class CatalogPricing
{
    public sealed record Line(
        decimal ListUnitPrice,
        decimal Quantity,
        decimal LineDiscountPercent = 0m,
        decimal CustomerDiscountPercent = 0m,
        decimal GeneralDiscountPercent = 0m,
        decimal VatPercent = 0m,
        bool ListIncludesVat = false);

    public sealed record PricedLine(decimal Gross, decimal LineDiscount, decimal CustomerDiscount, decimal GeneralDiscount, decimal Vat)
    {
        public decimal Discount => LineDiscount + CustomerDiscount + GeneralDiscount;

        public decimal Net => R2(Gross - Discount);

        public decimal Total => R2(Net + Vat);
    }

    public sealed record Totals(IReadOnlyList<PricedLine> Lines)
    {
        public decimal Gross => R2(Lines.Sum(l => l.Gross));

        public decimal Discount => R2(Lines.Sum(l => l.Discount));

        public decimal Vat => R2(Lines.Sum(l => l.Vat));

        public decimal Total => R2(Lines.Sum(l => l.Total));
    }

    public static PricedLine Price(Line line)
    {
        var gross = R2(UnitWithoutVat(line.ListUnitPrice, line.VatPercent, line.ListIncludesVat) * line.Quantity);
        var d1 = R2(gross * line.LineDiscountPercent / 100m);
        var d2 = R2((gross - d1) * line.CustomerDiscountPercent / 100m);
        var d3 = R2((gross - d1 - d2) * line.GeneralDiscountPercent / 100m);
        var vat = R2((gross - d1 - d2 - d3) * line.VatPercent / 100m);
        return new PricedLine(gross, d1, d2, d3, vat);
    }

    public static Totals Total(IEnumerable<Line> lines) => new([.. lines.Select(Price)]);

    /// <summary>A catalog line: the customer's discount only (<paramref name="discountPercent"/> 0 for a <c>noDiscount</c> product).</summary>
    public static PricedLine CatalogLine(decimal listPrice, decimal quantity, decimal discountPercent, decimal vatPercent, bool listIncludesVat) =>
        Price(new Line(listPrice, quantity, CustomerDiscountPercent: discountPercent, VatPercent: vatPercent, ListIncludesVat: listIncludesVat));

    /// <summary>The unit price the customer sees: the list price less their discount, before any VAT split.</summary>
    public static decimal NetUnitPrice(decimal listPrice, decimal discountPercent) => R2(listPrice * (1m - discountPercent / 100m));

    /// <summary>The discount that applies to a product: none when the company marked it <c>noDiscount</c>.</summary>
    public static decimal DiscountFor(bool noDiscount, decimal accountDiscountPercent) => noDiscount ? 0m : accountDiscountPercent;

    /// <summary>Two decimals, half away from zero (Kotlin <c>HALF_UP</c> on BigDecimal).</summary>
    public static decimal R2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal UnitWithoutVat(decimal listUnitPrice, decimal vatPercent, bool listIncludesVat) =>
        listIncludesVat && vatPercent != 0m ? listUnitPrice / (1m + vatPercent / 100m) : listUnitPrice;
}
