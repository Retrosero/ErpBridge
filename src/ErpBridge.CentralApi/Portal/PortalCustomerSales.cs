using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Endpoints;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Portal;

/// <summary>
/// What each customer was sold, product by product, with the prices (GOAL_PANEL_GIRIS P2c): the stock movements going out
/// to a customer (<c>stockTransactions</c>, <c>tip = 1</c> — the phone's <c>observeSalesForCustomer</c>), folded into a
/// per-customer index the way <see cref="PortalStockMovements"/> folds them per product. A return may only take back what
/// was sold, at a price it was sold at (the phone's <c>salePricesFor</c>).
/// </summary>
public sealed class PortalCustomerSales
{
    private static readonly TimeSpan Idle = TimeSpan.FromMinutes(30);

    /// <summary>One sale line: <see cref="UnitPrice"/> is the price without VAT, as the movement carries it.</summary>
    public sealed record Sale(string Id, string Customer, string StockCode, DateTime Date, decimal UnitPrice);

    /// <summary>A price a product was sold to the customer at, and the last day it was.</summary>
    public sealed record PriceOption(decimal UnitPrice, DateTime LastSold);

    private readonly PortalRecordMirror<Sale> _mirror;
    private readonly Lock _sync = new();
    private readonly Dictionary<string, Dictionary<string, Sale>> _byCustomer = new(StringComparer.OrdinalIgnoreCase);

    private PortalCustomerSales(Guid tenantId) =>
        _mirror = PortalRecordMirror<Sale>.Folding(tenantId, PortalRecords.LineEntities, Parse, Apply);

    public static PortalCustomerSales For(IMemoryCache cache, Guid tenantId) =>
        cache.GetOrCreate(("portal-customer-sales", tenantId), entry =>
        {
            entry.SlidingExpiration = Idle;
            return new PortalCustomerSales(tenantId);
        })!;

    /// <summary>
    /// The products sold to <paramref name="customerCode"/> and, for each, the distinct prices (above zero) with the last
    /// day each was charged, newest first.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, IReadOnlyList<PriceOption>>> ForCustomerAsync(CentralApiDbContext db, string customerCode, CancellationToken ct)
    {
        await _mirror.ApplyAsync(db, ct);
        List<Sale> sales;
        lock (_sync) sales = _byCustomer.TryGetValue(customerCode, out var found) ? [.. found.Values] : [];
        return sales
            .GroupBy(s => s.StockCode, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<PriceOption>)[.. g.GroupBy(s => decimal.Round(s.UnitPrice, 4))
                    .Select(p => new PriceOption(p.Key, p.Max(s => s.Date)))
                    .OrderByDescending(p => p.LastSold)],
                StringComparer.Ordinal);
    }

    private static Sale? Parse(string entity, JsonElement item)
    {
        if (AndroidEndpoints.GetInt32(item, "tip") != 1) return null;
        // A cancelled sale and its reversal took nothing out for the customer.
        if (AndroidEndpoints.GetBoolean(item, "voided") == true || AndroidEndpoints.GetString(item, "voidsKey") is not null) return null;
        var customer = PortalRecords.Blank(AndroidEndpoints.GetString(item, "cariKod"));
        var code = PortalRecords.Blank(AndroidEndpoints.GetFirstString(item, "stokKod", "urunKod"));
        var id = AndroidEndpoints.GetString(item, "id") ?? AndroidEndpoints.GetString(item, "erpRef");
        if (customer is null || code is null || string.IsNullOrWhiteSpace(id) || id.EndsWith("|void", StringComparison.Ordinal)) return null;
        var price = AndroidEndpoints.GetDecimal(item, "birimFiyat") ?? 0m;
        if (price <= 0) return null;
        return new Sale(id, customer, code, PortalRecords.ReadDateTime(AndroidEndpoints.GetString(item, "tarih")) ?? DateTime.MinValue, price);
    }

    private void Apply(Sale? before, Sale? after)
    {
        if (before == after) return;
        lock (_sync)
        {
            if (before is not null && _byCustomer.TryGetValue(before.Customer, out var old))
            {
                old.Remove(before.Id);
                if (old.Count == 0) _byCustomer.Remove(before.Customer);
            }
            if (after is not null)
            {
                if (!_byCustomer.TryGetValue(after.Customer, out var sales)) _byCustomer[after.Customer] = sales = new(StringComparer.Ordinal);
                sales[after.Id] = after;
            }
        }
    }
}
