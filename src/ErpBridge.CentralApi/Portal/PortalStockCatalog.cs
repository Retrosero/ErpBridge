using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
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
/// quantities and names, from the <see cref="PortalRecordMirror{T}"/> of the stock entities, with
/// each product's latest movement day from <see cref="PortalLastMovements"/>. The stock side is
/// rebuilt only when a stock row changed; a new movement line only moves one product's date
/// (G5). Warehouse (fulfillment) events share the company's sequence counter but write no such
/// rows, so they leave it alone.
///
/// <para>ERP and native companies name the same things differently (Mikro's
/// <c>mainGroupCode</c>/<c>brandCode</c>, the phone's <c>kategori</c>/<c>marka</c>); both are
/// read here and nowhere else.</para>
///
/// <para>What the data really holds (Codex review of #60): Mikro's reader sends one company-wide
/// quantity per product under the agent's configured warehouse, reserved quantity 0 and no
/// last-movement date. So warehouses are offered only as they appear in inventory rows (a
/// lookup-only warehouse holds nothing), and the last movement is the latest <c>tarih</c> of the
/// product's <c>stockTransactions</c> mirror (full STOK_HAREKETLERI history for Mikro, every
/// booked line for native companies).</para>
/// </summary>
public static class PortalStockCatalog
{

    public sealed record Product(
        string Code, string Name, string? Unit, string? MainGroup, string? SubGroup, string? Brand, string? Shelf,
        IReadOnlyList<string> Barcodes,
        IReadOnlyDictionary<int, (decimal Quantity, decimal Reserved)> Warehouses,
        IReadOnlyDictionary<int, decimal> Prices,
        DateOnly? LastMovement,
        decimal? VatRate)
    {
        public decimal TotalQuantity => Warehouses.Values.Sum(w => w.Quantity);
        public decimal TotalReserved => Warehouses.Values.Sum(w => w.Reserved);
    }

    /// <summary>
    /// One built catalogue. What every request would otherwise work out again — the default price list, the
    /// facets and the default (name) order — is worked out once per catalogue, on first use (G5).
    /// </summary>
    public sealed record Catalog(
        IReadOnlyList<Product> Products,
        IReadOnlyDictionary<int, string> WarehouseNames,
        IReadOnlyDictionary<int, string> PriceListNames,
        bool HasReserved)
    {
        private readonly Lazy<int?> _defaultPriceList = new(() =>
            Products.SelectMany(p => p.Prices.Keys).Distinct().OrderBy(n => n == 1 ? 0 : 1).ThenBy(n => n).Cast<int?>().FirstOrDefault());

        private readonly Lazy<IReadOnlyList<Product>> _byName = new(() =>
            [.. Products.OrderBy(p => p.Name, ByText).ThenBy(p => p.Code, ByText)]);

        private Lazy<PortalStockFacetsResponse>? _facets;

        public int? DefaultPriceList => _defaultPriceList.Value;

        /// <summary>The products in the page's default order: name, then code.</summary>
        internal IReadOnlyList<Product> ByName => _byName.Value;

        internal PortalStockFacetsResponse Facets =>
            LazyInitializer.EnsureInitialized(ref _facets, () => new Lazy<PortalStockFacetsResponse>(() => BuildFacets(this))).Value;
    }

    private static readonly StringComparer ByText = StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), CompareOptions.IgnoreCase);

    /// <param name="Stock">The catalogue from the stock entities alone; line dates are laid over it.</param>
    private sealed record CachedCatalog(long StockVersion, long DaysVersion, Catalog Stock, Catalog Catalog);

    /// <summary>
    /// The company's catalogue. One build at a time per company: the page asks for search and facets at once,
    /// and a cold or changed catalogue must not be built twice side by side (G5).
    /// </summary>
    public static async Task<Catalog> LoadAsync(CentralApiDbContext db, IMemoryCache cache, Guid tenantId, CancellationToken ct)
    {
        var gate = cache.GetOrCreate(("portal-stock-build", tenantId), entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);
            return new SemaphoreSlim(1, 1);
        })!;
        await gate.WaitAsync(ct);
        try
        {
            return await BuildAsync(db, cache, tenantId, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    private static async Task<Catalog> BuildAsync(CentralApiDbContext db, IMemoryCache cache, Guid tenantId, CancellationToken ct)
    {
        var stockMirror = PortalRecordMirror<PortalRecords.StockPart>.For(cache, "stock", tenantId, PortalRecords.StockEntities, PortalRecords.ParseStock);
        var lastMovements = PortalLastMovements.For(cache, tenantId);
        var viewKey = ("portal-stock-catalog", tenantId);
        cache.TryGetValue(viewKey, out CachedCatalog? cached);

        var parts = await stockMirror.RefreshAsync(db, cached?.StockVersion, ct);
        var (daysVersion, latest) = await lastMovements.RefreshAsync(db, cached?.DaysVersion, ct);
        if (cached is not null && parts is null && latest is null) return cached.Catalog;

        // A sale changes only line dates: the stock side is reused, not rebuilt.
        var stock = parts is null && cached is not null ? cached.Stock : StockCatalog(parts ?? []);
        latest ??= (await lastMovements.RefreshAsync(db, null, ct)).Latest!;
        var products = new List<Product>(stock.Products.Count);
        foreach (var product in stock.Products)
        {
            products.Add(latest.TryGetValue(product.Code, out var day) && (product.LastMovement is not { } known || day > known)
                ? product with { LastMovement = day }
                : product);
        }

        var catalog = new Catalog(products, stock.WarehouseNames, stock.PriceListNames, stock.HasReserved);
        cache.Set(viewKey, new CachedCatalog(stockMirror.Version, daysVersion, stock, catalog), TimeSpan.FromMinutes(30));
        return catalog;
    }

    private static Catalog StockCatalog(IReadOnlyList<PortalRecords.StockPart> parts)
    {
        var warehouseNames = new Dictionary<int, string>();
        var priceListNames = new Dictionary<int, string>();
        var cards = new Dictionary<string, PortalRecords.CardPart>(StringComparer.OrdinalIgnoreCase);
        var barcodes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var quantities = new Dictionary<string, Dictionary<int, (decimal Quantity, decimal Reserved)>>(StringComparer.OrdinalIgnoreCase);
        var prices = new Dictionary<string, Dictionary<int, decimal>>(StringComparer.OrdinalIgnoreCase);
        var movements = new Dictionary<string, DateOnly>(StringComparer.OrdinalIgnoreCase);
        var hasReserved = false;

        foreach (var part in parts)
        {
            switch (part)
            {
                case PortalRecords.LookupPart lookup when lookup.Kind == "warehouse":
                    warehouseNames[lookup.Number] = lookup.Name;
                    break;
                case PortalRecords.LookupPart lookup when lookup.Kind == "price_list":
                    priceListNames[lookup.Number] = lookup.Name;
                    break;
                case PortalRecords.CardPart card:
                    cards[card.Code] = card;
                    break;
                case PortalRecords.BarcodePart barcode:
                    if (!barcodes.TryGetValue(barcode.Code, out var list)) barcodes[barcode.Code] = list = [];
                    if (!list.Contains(barcode.Barcode)) list.Add(barcode.Barcode);
                    break;
                case PortalRecords.InventoryPart inventory:
                    hasReserved |= inventory.Reserved != 0m;
                    if (!quantities.TryGetValue(inventory.Code, out var byWarehouse)) quantities[inventory.Code] = byWarehouse = [];
                    var current = byWarehouse.GetValueOrDefault(inventory.WarehouseNo);
                    byWarehouse[inventory.WarehouseNo] = (current.Quantity + inventory.Quantity, current.Reserved + inventory.Reserved);
                    if (inventory.LastMovement is { } day && (!movements.TryGetValue(inventory.Code, out var latest) || day > latest))
                        movements[inventory.Code] = day;
                    break;
                case PortalRecords.PricePart price:
                    if (!prices.TryGetValue(price.Code, out var byList)) prices[price.Code] = byList = [];
                    byList.TryAdd(price.ListNumber, price.Price);
                    break;
            }
        }

        var products = new List<Product>(cards.Count);
        foreach (var (code, card) in cards)
        {
            products.Add(new Product(
                code, card.Name, card.Unit, card.MainGroup, card.SubGroup, card.Brand, card.Shelf,
                barcodes.GetValueOrDefault(code) ?? [],
                quantities.GetValueOrDefault(code) ?? [],
                prices.GetValueOrDefault(code) ?? [],
                movements.TryGetValue(code, out var last) ? last : null,
                card.VatRate));
        }
        return new Catalog(products, warehouseNames, priceListNames, hasReserved);
    }

    public static PortalStockSearchResponse Search(Catalog catalog, StockQuery query, DateOnly today)
    {
        var priceList = query.PriceList ?? catalog.DefaultPriceList;
        decimal QuantityOf(Product p) => query.WarehouseNo is { } w ? p.Warehouses.GetValueOrDefault(w).Quantity : p.TotalQuantity;
        decimal? PriceOf(Product p) => priceList is { } l && p.Prices.TryGetValue(l, out var price) ? price : null;

        // The default order is kept with the catalogue; filtering keeps it, so that page skips the sort.
        var presorted = query.Sort == "name" && !query.Descending;
        IEnumerable<Product> matches = presorted ? catalog.ByName : catalog.Products;
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

        var byText = ByText;
        IEnumerable<Product> sorted = presorted ? filtered : (query.Sort switch
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
        }).ThenBy(p => p.Code, byText);
        var page = sorted
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

    public static PortalStockFacetsResponse Facets(Catalog catalog) => catalog.Facets;

    private static PortalStockFacetsResponse BuildFacets(Catalog catalog)
    {
        static List<PortalFacetValue> Count(IEnumerable<string?> values) => values
            .Where(v => v is not null)
            .GroupBy(v => v!, StringComparer.OrdinalIgnoreCase)
            .Select(g => new PortalFacetValue { Code = g.Key, Count = g.Count() })
            .OrderBy(v => v.Code, StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), CompareOptions.IgnoreCase))
            .ToList();

        // Only warehouses that hold rows: Mikro's reader sends one company-wide figure, so a
        // warehouse known only from the lookups would always filter to nothing.
        var warehouseNumbers = catalog.Products.SelectMany(p => p.Warehouses.Keys).Distinct().Order();
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

    internal static string PriceListName(Catalog catalog, int number) =>
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
}
