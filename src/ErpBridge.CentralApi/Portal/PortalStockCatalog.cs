using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Portal;

/// <summary>The filters, sort and page of a stock search, already validated.</summary>
public sealed record StockQuery(
    string? Search,
    IReadOnlyCollection<string> MainGroups,
    IReadOnlyCollection<string> SubGroups,
    IReadOnlyCollection<string> Brands,
    IReadOnlyCollection<string> Shelves,
    int? WarehouseNo,
    int? PriceList,
    decimal? MinQuantity,
    decimal? MaxQuantity,
    decimal? MinPrice,
    decimal? MaxPrice,
    string Status,
    decimal? Below,
    int? IdleDays,
    string Sort,
    bool Descending,
    int Page,
    int PageSize)
{
    public static readonly string[] Statuses = ["all", "in", "out", "negative", "below"];
    public static readonly string[] Sorts = ["name", "code", "qty", "price", "group", "brand", "shelf", "lastMovement"];
    public const int MaxPageSize = 250;
}

/// <summary>
/// The company's products as the stock page reads them: cards, barcodes, prices, warehouse
/// quantities and names, folded out of <c>mobile_records</c> once and kept in memory until a
/// stock record changes. The key is the highest <c>UpdatedSeq</c> and count of the stock
/// entities only — warehouse (fulfillment) events share the company's sequence counter but
/// do not write these rows, so they do not throw the catalogue away.
///
/// <para>ERP and native companies name the same things differently (Mikro's
/// <c>mainGroupCode</c>/<c>brandCode</c>, the phone's <c>kategori</c>/<c>marka</c>); both are
/// read here and nowhere else.</para>
/// </summary>
public static class PortalStockCatalog
{
    private static readonly string[] Entities = ["stocks", "inventory", "prices", "barcodes", "lookups"];
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(10);

    public sealed record Product(
        string Code, string Name, string? Unit, string? MainGroup, string? SubGroup, string? Brand, string? Shelf,
        IReadOnlyList<string> Barcodes,
        IReadOnlyDictionary<int, (decimal Quantity, decimal Reserved)> Warehouses,
        IReadOnlyDictionary<int, decimal> Prices,
        DateOnly? LastMovement)
    {
        public decimal TotalQuantity => Warehouses.Values.Sum(w => w.Quantity);
        public decimal TotalReserved => Warehouses.Values.Sum(w => w.Reserved);
    }

    public sealed record Catalog(
        IReadOnlyList<Product> Products,
        IReadOnlyDictionary<int, string> WarehouseNames,
        IReadOnlyDictionary<int, string> PriceListNames,
        bool HasReserved)
    {
        public int? DefaultPriceList =>
            Products.SelectMany(p => p.Prices.Keys).Distinct().OrderBy(n => n == 1 ? 0 : 1).ThenBy(n => n).Cast<int?>().FirstOrDefault();
    }

    private sealed record Cached(long MaxSeq, int Count, Catalog Catalog);

    public static async Task<Catalog> LoadAsync(CentralApiDbContext db, IMemoryCache cache, Guid tenantId, CancellationToken ct)
    {
        var stamp = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == tenantId && Entities.Contains(r.Entity))
            .GroupBy(_ => 1)
            .Select(g => new { MaxSeq = g.Max(r => r.UpdatedSeq), Count = g.Count(r => !r.IsDeleted) })
            .FirstOrDefaultAsync(ct);
        var maxSeq = stamp?.MaxSeq ?? 0;
        var count = stamp?.Count ?? 0;
        var key = ("portal-stock-catalog", tenantId);
        if (cache.TryGetValue(key, out Cached? cached) && cached!.MaxSeq == maxSeq && cached.Count == count)
            return cached.Catalog;

        var records = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == tenantId && !r.IsDeleted && Entities.Contains(r.Entity))
            .Select(r => new { r.Entity, r.StockKey, r.PayloadJson })
            .ToListAsync(ct);

        var warehouseNames = new Dictionary<int, string>();
        var priceListNames = new Dictionary<int, string>();
        var cards = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        var barcodes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var quantities = new Dictionary<string, Dictionary<int, (decimal Quantity, decimal Reserved)>>(StringComparer.OrdinalIgnoreCase);
        var prices = new Dictionary<string, Dictionary<int, decimal>>(StringComparer.OrdinalIgnoreCase);
        var movements = new Dictionary<string, DateOnly>(StringComparer.OrdinalIgnoreCase);
        var hasReserved = false;

        foreach (var record in records)
        {
            if (string.IsNullOrWhiteSpace(record.PayloadJson)) continue;
            using var document = JsonDocument.Parse(record.PayloadJson);
            var item = document.RootElement;
            if (item.ValueKind != JsonValueKind.Object) continue;
            var code = record.StockKey ?? AndroidEndpoints.GetString(item, "stockCode");

            switch (record.Entity)
            {
                case "lookups":
                    var kind = AndroidEndpoints.GetString(item, "kind");
                    var number = AndroidEndpoints.GetInt32(item, "code");
                    var name = AndroidEndpoints.GetString(item, "name")?.Trim();
                    if (number is not { } n || string.IsNullOrEmpty(name)) break;
                    if (string.Equals(kind, "warehouse", StringComparison.OrdinalIgnoreCase)) warehouseNames[n] = name;
                    else if (string.Equals(kind, "price_list", StringComparison.OrdinalIgnoreCase)) priceListNames[n] = name;
                    break;
                case "stocks" when code is not null:
                    cards[code] = item.Clone();
                    break;
                case "barcodes" when code is not null:
                    if (AndroidEndpoints.GetString(item, "barcode")?.Trim() is { Length: > 0 } barcode)
                    {
                        if (!barcodes.TryGetValue(code, out var list)) barcodes[code] = list = [];
                        if (!list.Contains(barcode)) list.Add(barcode);
                    }
                    break;
                case "inventory" when code is not null:
                    var warehouse = AndroidEndpoints.GetInt32(item, "warehouseNo") ?? 0;
                    var reserved = AndroidEndpoints.GetDecimal(item, "reservedQuantity") ?? 0m;
                    hasReserved |= reserved != 0m;
                    if (!quantities.TryGetValue(code, out var byWarehouse)) quantities[code] = byWarehouse = [];
                    var current = byWarehouse.GetValueOrDefault(warehouse);
                    byWarehouse[warehouse] = (current.Quantity + (AndroidEndpoints.GetDecimal(item, "quantity") ?? 0m), current.Reserved + reserved);
                    if (ReadDate(AndroidEndpoints.GetString(item, "lastMovementDate")) is { } moved
                        && (!movements.TryGetValue(code, out var latest) || moved > latest))
                        movements[code] = moved;
                    break;
                case "prices" when code is not null:
                    if (AndroidEndpoints.GetDecimal(item, "price") is not { } price || price <= 0) break;
                    var listNumber = AndroidEndpoints.GetInt32(item, "listNumber") ?? 0;
                    if (listNumber <= 0) break;
                    if (!prices.TryGetValue(code, out var byList)) prices[code] = byList = [];
                    byList.TryAdd(listNumber, price);
                    break;
            }
        }

        var products = new List<Product>(cards.Count);
        foreach (var (code, card) in cards)
        {
            products.Add(new Product(
                code,
                AndroidEndpoints.GetFirstString(card, "name", "urunAd"),
                Blank(AndroidEndpoints.GetFirstString(card, "unit1", "birim", "unit")),
                Blank(AndroidEndpoints.GetFirstString(card, "mainGroupCode", "kategori", "category")),
                Blank(AndroidEndpoints.GetFirstString(card, "subGroupCode")),
                Blank(AndroidEndpoints.GetFirstString(card, "brandCode", "marka")),
                Blank(AndroidEndpoints.GetFirstString(card, "shelfCode", "sto_yer_kod")),
                barcodes.GetValueOrDefault(code) ?? [],
                quantities.GetValueOrDefault(code) ?? [],
                prices.GetValueOrDefault(code) ?? [],
                movements.TryGetValue(code, out var last) ? last : null));
        }

        var catalog = new Catalog(products, warehouseNames, priceListNames, hasReserved);
        cache.Set(key, new Cached(maxSeq, count, catalog), CacheLifetime);
        return catalog;
    }

    public static PortalStockSearchResponse Search(Catalog catalog, StockQuery query, DateOnly today)
    {
        var priceList = query.PriceList ?? catalog.DefaultPriceList;
        decimal QuantityOf(Product p) => query.WarehouseNo is { } w ? p.Warehouses.GetValueOrDefault(w).Quantity : p.TotalQuantity;
        decimal? PriceOf(Product p) => priceList is { } l && p.Prices.TryGetValue(l, out var price) ? price : null;

        IEnumerable<Product> matches = catalog.Products;
        if (query.Search?.Trim() is { Length: > 0 } search)
            matches = matches.Where(p => p.Code.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                                         || p.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                                         || p.Barcodes.Any(b => b.Contains(search, StringComparison.OrdinalIgnoreCase)));
        matches = In(matches, query.MainGroups, p => p.MainGroup);
        matches = In(matches, query.SubGroups, p => p.SubGroup);
        matches = In(matches, query.Brands, p => p.Brand);
        matches = In(matches, query.Shelves, p => p.Shelf);
        if (query.WarehouseNo is { } warehouseNo) matches = matches.Where(p => p.Warehouses.ContainsKey(warehouseNo));
        if (query.MinQuantity is { } minQuantity) matches = matches.Where(p => QuantityOf(p) >= minQuantity);
        if (query.MaxQuantity is { } maxQuantity) matches = matches.Where(p => QuantityOf(p) <= maxQuantity);
        if (query.MinPrice is { } minPrice) matches = matches.Where(p => PriceOf(p) >= minPrice);
        if (query.MaxPrice is { } maxPrice) matches = matches.Where(p => PriceOf(p) <= maxPrice);
        if (query.IdleDays is { } idleDays)
        {
            var since = today.AddDays(-idleDays);
            matches = matches.Where(p => p.LastMovement is { } moved && moved < since);
        }
        matches = query.Status switch
        {
            "in" => matches.Where(p => QuantityOf(p) > 0),
            "out" => matches.Where(p => QuantityOf(p) <= 0),
            "negative" => matches.Where(p => QuantityOf(p) < 0),
            "below" => matches.Where(p => QuantityOf(p) <= (query.Below ?? 0)),
            _ => matches,
        };
        var filtered = matches.ToList();

        var culture = CultureInfo.GetCultureInfo("tr-TR");
        var byText = StringComparer.Create(culture, CompareOptions.IgnoreCase);
        IOrderedEnumerable<Product> ordered = query.Sort switch
        {
            "code" => Order(filtered, p => p.Code, byText, query.Descending),
            "qty" => Order(filtered, QuantityOf, Comparer<decimal>.Default, query.Descending),
            // A product without a price sorts after every priced one, either way.
            "price" => filtered.OrderBy(p => PriceOf(p) is null ? 1 : 0).ThenBy(p => PriceOf(p) ?? 0, Direction(Comparer<decimal>.Default, query.Descending)),
            "group" => NullsLast(filtered, p => p.MainGroup, byText, query.Descending),
            "brand" => NullsLast(filtered, p => p.Brand, byText, query.Descending),
            "shelf" => NullsLast(filtered, p => p.Shelf, byText, query.Descending),
            "lastMovement" => filtered.OrderBy(p => p.LastMovement is null ? 1 : 0).ThenBy(p => p.LastMovement ?? DateOnly.MinValue, Direction(Comparer<DateOnly>.Default, query.Descending)),
            _ => Order(filtered, p => p.Name, byText, query.Descending),
        };
        var page = ordered.ThenBy(p => p.Code, byText)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => ToItem(p, catalog, QuantityOf(p), query.WarehouseNo, PriceOf(p)))
            .ToList();

        return new PortalStockSearchResponse
        {
            Items = page,
            Total = filtered.Count,
            Page = query.Page,
            PageSize = query.PageSize,
            PriceList = priceList,
            WarehouseNo = query.WarehouseNo,
            Summary = new PortalStockSummary
            {
                Products = filtered.Count,
                InStock = filtered.Count(p => QuantityOf(p) > 0),
                OutOfStock = filtered.Count(p => QuantityOf(p) <= 0),
                Negative = filtered.Count(p => QuantityOf(p) < 0),
            },
        };
    }

    public static PortalStockFacetsResponse Facets(Catalog catalog)
    {
        static List<PortalFacetValue> Count(IEnumerable<string?> values) => values
            .Where(v => v is not null)
            .GroupBy(v => v!, StringComparer.OrdinalIgnoreCase)
            .Select(g => new PortalFacetValue { Code = g.Key, Count = g.Count() })
            .OrderBy(v => v.Code, StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), CompareOptions.IgnoreCase))
            .ToList();

        var warehouseNumbers = catalog.Products.SelectMany(p => p.Warehouses.Keys).Concat(catalog.WarehouseNames.Keys).Distinct().Order();
        var priceLists = catalog.Products.SelectMany(p => p.Prices.Keys).Distinct().Order();
        return new PortalStockFacetsResponse
        {
            MainGroups = Count(catalog.Products.Select(p => p.MainGroup)),
            SubGroups = catalog.Products.Where(p => p.SubGroup is not null)
                .GroupBy(p => (Sub: p.SubGroup!.ToUpperInvariant(), Main: p.MainGroup?.ToUpperInvariant()))
                .Select(g => new PortalFacetValue { Code = g.First().SubGroup!, Parent = g.First().MainGroup, Count = g.Count() })
                .OrderBy(v => v.Parent).ThenBy(v => v.Code)
                .ToList(),
            Brands = Count(catalog.Products.Select(p => p.Brand)),
            Shelves = Count(catalog.Products.Select(p => p.Shelf)),
            Warehouses = warehouseNumbers.Select(n => new PortalNamedNumber { Number = n, Name = WarehouseName(catalog, n) }).ToList(),
            PriceLists = priceLists.Select(n => new PortalNamedNumber { Number = n, Name = PriceListName(catalog, n) }).ToList(),
            HasMovementDates = catalog.Products.Any(p => p.LastMovement is not null),
            HasReserved = catalog.HasReserved,
        };
    }

    private static PortalStockItem ToItem(Product p, Catalog catalog, decimal quantity, int? warehouseNo, decimal? price) => new()
    {
        StockCode = p.Code,
        Name = p.Name,
        Unit = p.Unit,
        MainGroup = p.MainGroup,
        SubGroup = p.SubGroup,
        Brand = p.Brand,
        Shelf = p.Shelf,
        Barcodes = [.. p.Barcodes],
        Quantity = quantity,
        Reserved = warehouseNo is { } w ? p.Warehouses.GetValueOrDefault(w).Reserved : p.TotalReserved,
        Price = price,
        LastMovementDate = p.LastMovement?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        Warehouses = p.Warehouses.OrderBy(x => x.Key)
            .Select(x => new PortalStockWarehouseQuantity { WarehouseNo = x.Key, Name = WarehouseName(catalog, x.Key), Quantity = x.Value.Quantity, Reserved = x.Value.Reserved })
            .ToList(),
        Prices = p.Prices.OrderBy(x => x.Key)
            .Select(x => new PortalStockPrice { ListNumber = x.Key, Name = PriceListName(catalog, x.Key), Price = x.Value })
            .ToList(),
    };

    private static string WarehouseName(Catalog catalog, int number) =>
        catalog.WarehouseNames.TryGetValue(number, out var name) ? name : $"Depo {number}";

    private static string PriceListName(Catalog catalog, int number) =>
        catalog.PriceListNames.TryGetValue(number, out var name) ? name : $"Liste {number}";

    private static IEnumerable<Product> In(IEnumerable<Product> products, IReadOnlyCollection<string> wanted, Func<Product, string?> value)
    {
        if (wanted.Count == 0) return products;
        var set = new HashSet<string>(wanted, StringComparer.OrdinalIgnoreCase);
        return products.Where(p => value(p) is { } v && set.Contains(v));
    }

    private static IComparer<T> Direction<T>(IComparer<T> comparer, bool descending) =>
        descending ? Comparer<T>.Create((a, b) => comparer.Compare(b, a)) : comparer;

    private static IOrderedEnumerable<Product> Order<T>(IEnumerable<Product> products, Func<Product, T> key, IComparer<T> comparer, bool descending) =>
        products.OrderBy(key, Direction(comparer, descending));

    private static IOrderedEnumerable<Product> NullsLast(IEnumerable<Product> products, Func<Product, string?> key, IComparer<string> comparer, bool descending) =>
        products.OrderBy(p => key(p) is null ? 1 : 0).ThenBy(p => key(p) ?? string.Empty, Direction(comparer, descending));

    private static string? Blank(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static DateOnly? ReadDate(string? value) =>
        value is not null && DateOnly.TryParse(value.Length >= 10 ? value[..10] : value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
}
