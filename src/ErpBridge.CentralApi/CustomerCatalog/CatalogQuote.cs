using ErpBridge.CentralApi.Contracts;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// Prices a customer's cart (GOAL_MUSTERI_KATALOGU §4, §5.2 <c>cart/quote</c>): each line with what the customer sees —
/// their list, their discount (none on a <c>noDiscount</c> product), the VAT rate — and an issue when it cannot be
/// ordered as it is. A product the customer does not see tells nothing about itself. The totals add up the lines
/// without an issue, so they are what an order of the cart would be.
/// </summary>
public static class CatalogQuote
{
    public const string NotAvailable = "NOT_AVAILABLE";
    public const string OutOfStock = "OUT_OF_STOCK";
    public const string CartonMultiple = "CARTON_MULTIPLE";
    public const string InvalidQuantity = "INVALID_QUANTITY";

    /// <summary>Whole units only (no decimal units such as KG in v1), at most this many on one line.</summary>
    public const decimal MaxQuantity = 100_000m;

    public static CatalogQuoteDto Build(CatalogCustomerView customer, IEnumerable<(string? Key, decimal Quantity)> lines)
    {
        var priced = new List<CatalogQuoteLineDto>();
        foreach (var (key, quantity) in lines)
        {
            var product = customer.Find(key);
            if (product is null)
            {
                priced.Add(new CatalogQuoteLineDto { Key = key, Quantity = quantity, Issue = NotAvailable });
                continue;
            }

            var line = new CatalogQuoteLineDto
            {
                Key = product.Code,
                Code = product.Code,
                Name = product.Name,
                Unit = product.Unit,
                Quantity = quantity,
                Box = Box(product),
                Price = Price(customer, product),
                VatRate = product.VatRate,
                Thumb = product.ShownThumbUrl,
            };
            if (quantity <= 0m || quantity > MaxQuantity || quantity != decimal.Truncate(quantity))
            {
                line.Issue = InvalidQuantity;
                priced.Add(line);
                continue;
            }

            var amounts = CatalogPricing.CatalogLine(customer.ListPrice(product), quantity, customer.DiscountPercent(product), product.VatRate, customer.IncludesVat);
            line.Gross = amounts.Gross;
            line.Discount = amounts.Discount;
            line.Vat = amounts.Vat;
            line.Total = amounts.Total;
            if (!product.InStock) line.Issue = OutOfStock;
            else if (product.CartonOnly && quantity % product.EffectiveCartonQuantity!.Value != 0) line.Issue = CartonMultiple;
            priced.Add(line);
        }

        var ok = priced.Where(l => l.Issue is null).ToList();
        return new CatalogQuoteDto
        {
            Lines = [.. priced],
            Totals = new CatalogQuoteTotalsDto
            {
                Gross = CatalogPricing.R2(ok.Sum(l => l.Gross)),
                Discount = CatalogPricing.R2(ok.Sum(l => l.Discount)),
                Vat = CatalogPricing.R2(ok.Sum(l => l.Vat)),
                Total = CatalogPricing.R2(ok.Sum(l => l.Total)),
            },
        };
    }

    public static CatalogCustomerPriceDto Price(CatalogCustomerView customer, CatalogProduct product)
    {
        var list = customer.ListPrice(product);
        var discount = customer.DiscountPercent(product);
        return new CatalogCustomerPriceDto
        {
            List = list,
            Net = CatalogPricing.NetUnitPrice(list, discount),
            DiscountPercent = discount,
            IncludesVat = customer.IncludesVat,
        };
    }

    /// <summary>The carton the customer sees: the company's or the ERP's, when there is one of at least 2.</summary>
    public static CatalogBoxDto? Box(CatalogProduct product) =>
        product.EffectiveCartonQuantity is int qty and >= 2 ? new CatalogBoxDto { Qty = qty, Only = product.CartonOnly } : null;
}
