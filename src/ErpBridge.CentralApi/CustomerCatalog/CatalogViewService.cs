using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.CustomerCatalog;

public sealed record CatalogPriceList(int No, string Name, bool IncludesVat);

/// <summary>A picture with something to show (a link, or a file with at least one size uploaded).</summary>
public sealed record CatalogPicture(Guid Id, string ThumbUrl, string FullUrl);

/// <summary>One product as the catalog shows it: the ERP card with the company's catalog settings laid over it.</summary>
/// <param name="InStock">All warehouses together, rounded as the phone rounds, is at least 1 (T9); the count itself is never shown (K6).</param>
/// <param name="ErpCartonQuantity">The ERP's carton (<see cref="CatalogViewService.ErpCartonQuantity"/>).</param>
/// <param name="CartonQuantity">The company's own carton (≥ 2), over the ERP's.</param>
/// <param name="CartonOnlySetting">What the company ticked; <see cref="CartonOnly"/> is what applies.</param>
public sealed record CatalogProduct(
    string Code, string Name, string? Unit, string? Brand, IReadOnlyList<string> Barcodes, string CategoryKey,
    decimal VatRate, IReadOnlyDictionary<int, decimal> Prices, bool InStock, int? ErpCartonQuantity,
    int? SortOrder, bool Hidden, bool NoDiscount, int? CartonQuantity, bool CartonOnlySetting, IReadOnlyList<CatalogPicture> Pictures)
{
    public int? EffectiveCartonQuantity => CartonQuantity ?? ErpCartonQuantity;

    /// <summary>Sold in whole cartons only: asked for, and there is a carton of at least 2.</summary>
    public bool CartonOnly => CartonOnlySetting && EffectiveCartonQuantity >= 2;

    public string? ThumbUrl => Pictures.Count > 0 ? Pictures[0].ThumbUrl : null;

    /// <summary>The price in one list; null when the product has none there (the customer of that list does not see it).</summary>
    public decimal? PriceIn(int? listNo) => listNo is { } no && Prices.TryGetValue(no, out var price) ? price : null;

    private static readonly CompareInfo Turkish = CultureInfo.GetCultureInfo("tr-TR").CompareInfo;

    private const CompareOptions SearchOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    /// <summary>
    /// A search hit: <paramref name="query"/> in the name, code, brand or a barcode, Turkish case and circumflexes
    /// ignored ("ışık" finds "IŞIK", "kağıt" finds "Kâğıt"); ç, ğ, ı, ö, ş, ü stay letters of their own, as in Turkish.
    /// </summary>
    public bool Matches(string query) =>
        Turkish.IndexOf(Name, query, SearchOptions) >= 0
        || Turkish.IndexOf(Code, query, SearchOptions) >= 0
        || (Brand is { } brand && Turkish.IndexOf(brand, query, SearchOptions) >= 0)
        || Barcodes.Any(b => b.Contains(query, StringComparison.OrdinalIgnoreCase));
}

/// <param name="Id">The customer API's category id: the first 12 hex of SHA-256(key).</param>
public sealed record CatalogCategory(string Key, string Id, int? SortOrder, bool Hidden, IReadOnlyList<CatalogProduct> Products);

/// <summary>
/// A company's catalog at one stock version and one layout revision: every product with a card, grouped by category, in
/// the catalog's order. Account-specific things — visibility, discount, the customer's list — are applied per request
/// over it (<see cref="CatalogVisibility"/>, <see cref="CatalogPricing"/>).
/// </summary>
public sealed record CatalogView(
    long StockVersion,
    long Revision,
    bool IsEnabled,
    int? DefaultPriceListNo,
    int? EffectiveDefaultPriceListNo,
    IReadOnlyList<CatalogPriceList> PriceLists,
    IReadOnlyList<CatalogCategory> Categories,
    IReadOnlyDictionary<string, CatalogProduct> Products)
{
    /// <summary>The stock side it was built from; a layout-only change reuses it.</summary>
    internal CatalogViewService.StockSide? Stock { get; init; }

    /// <summary>The picture revision it was built at (<see cref="CatalogSettings.ImageRevision"/>).</summary>
    public long ImageRevision { get; init; }

    private readonly Dictionary<string, CatalogCategory> _categories = Categories.ToDictionary(c => c.Key, StringComparer.Ordinal);

    public CatalogCategory? Category(string key) => _categories.GetValueOrDefault(key);

    public CatalogPriceList? PriceList(int no) => PriceLists.FirstOrDefault(l => l.No == no);

    /// <summary>The list a customer buys from: their own, else the company default (§4).</summary>
    public int? EffectivePriceListNo(int? accountPriceListNo) => accountPriceListNo ?? EffectiveDefaultPriceListNo;

    /// <summary>Shown in the main catalog: not hidden, its category not hidden, priced in the company's list.</summary>
    public bool InMainCatalog(CatalogProduct product) =>
        !product.Hidden && Category(product.CategoryKey)?.Hidden != true && product.PriceIn(EffectiveDefaultPriceListNo) is not null;
}

/// <summary>
/// Builds and caches each company's <see cref="CatalogView"/> (GOAL_MUSTERI_KATALOGU §4). The stock comes from the
/// shared stock mirror (<see cref="PortalRecordMirror{T}"/> "stock", the stock page's), the layout — settings, category
/// and product rows, picture rows (never their bytes) — is read again only when <see cref="CatalogSettings.Revision"/>
/// or <see cref="CatalogSettings.ImageRevision"/> moved; every layout and picture write moves one of them. One build at
/// a time per company. Staff requests bring the mirror up to date every time; customer requests at most every
/// <see cref="CustomerRefreshInterval"/>, so a busy catalog does not poll <c>mobile_records</c> on every page — and
/// while the cached view is that fresh and its revisions are the stored ones, a customer request takes it without
/// waiting for the company's build lock (one row read).
/// </summary>
public sealed class CatalogViewService(IMemoryCache cache, TimeProvider time)
{
    public static readonly TimeSpan CustomerRefreshInterval = TimeSpan.FromSeconds(5);

    /// <summary>The category of a product with none: the phone's own fallback.</summary>
    public const string OtherCategory = "Diğer";

    private static readonly TimeSpan Idle = TimeSpan.FromMinutes(30);
    private static readonly StringComparer ByText = StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), CompareOptions.IgnoreCase);

    // Not the memory cache: its GetOrCreate may run the factory twice for two cold requests (PortalStockCatalog).
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _gates = new();
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _refreshedAt = new();

    /// <summary>The card side of the catalog, from the stock entities alone.</summary>
    internal sealed record StockSide(long Version, IReadOnlyList<StockProduct> Products, IReadOnlyList<CatalogPriceList> PriceLists, int? DefaultPriceListNo);

    internal sealed record StockProduct(
        string Code, string Name, string? Unit, string? Brand, IReadOnlyList<string> Barcodes, string CategoryKey,
        decimal VatRate, IReadOnlyDictionary<int, decimal> Prices, bool InStock, int? ErpCartonQuantity);

    /// <param name="forCustomer">A customer's request: the mirror is brought up to date at most every <see cref="CustomerRefreshInterval"/>.</param>
    public async Task<CatalogView> LoadAsync(CentralApiDbContext db, Guid tenantId, bool forCustomer, CancellationToken ct)
    {
        // The fast path: a customer page within the refresh interval of a view whose revisions are still the stored ones
        // does not queue behind another company request's build.
        if (forCustomer
            && cache.TryGetValue(CacheKey(tenantId), out CatalogView? fresh) && fresh is not null
            && _refreshedAt.TryGetValue(tenantId, out var refreshed) && time.GetUtcNow() - refreshed < CustomerRefreshInterval)
        {
            var marks = await db.CatalogSettings.AsNoTracking().Where(s => s.TenantId == tenantId)
                .Select(s => new { s.Revision, s.ImageRevision }).FirstOrDefaultAsync(ct);
            if (fresh.Revision == (marks?.Revision ?? 0) && fresh.ImageRevision == (marks?.ImageRevision ?? 0)) return fresh;
        }

        var gate = GateOf(tenantId);
        await gate.WaitAsync(ct);
        try
        {
            return await BuildAsync(db, tenantId, forCustomer, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<CatalogView> BuildAsync(CentralApiDbContext db, Guid tenantId, bool forCustomer, CancellationToken ct)
    {
        var key = CacheKey(tenantId);
        cache.TryGetValue(key, out CatalogView? cached);
        var mirror = PortalRecordMirror<PortalRecords.StockPart>.For(cache, "stock", tenantId, PortalRecords.StockEntities, PortalRecords.ParseStock);

        var now = time.GetUtcNow();
        IReadOnlyList<PortalRecords.StockPart>? parts = null;
        long stockVersion;
        if (forCustomer && cached is not null && _refreshedAt.TryGetValue(tenantId, out var refreshed) && now - refreshed < CustomerRefreshInterval)
        {
            stockVersion = cached.StockVersion;
        }
        else
        {
            parts = await mirror.RefreshAsync(db, cached?.Stock?.Version, ct);
            stockVersion = cached?.Stock is { } known && parts is null ? known.Version : mirror.Version;
            _refreshedAt[tenantId] = now;
        }

        var settings = await db.CatalogSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        var revision = settings?.Revision ?? 0;
        var imageRevision = settings?.ImageRevision ?? 0;
        if (cached is not null && cached.StockVersion == stockVersion && cached.Revision == revision && cached.ImageRevision == imageRevision) return cached;

        var stock = parts is not null ? BuildStock(stockVersion, parts)
            : cached?.Stock is { } reused && reused.Version == stockVersion ? reused
            : BuildStock(mirror.Version, (await mirror.RefreshAsync(db, null, ct))!);

        var categoryRows = await db.CatalogCategorySettings.AsNoTracking().Where(s => s.TenantId == tenantId).ToListAsync(ct);
        var productRows = await db.CatalogProductSettings.AsNoTracking().Where(s => s.TenantId == tenantId).ToListAsync(ct);
        // Banner pictures (and any other reserved "~" key) are not a product's.
        var images = await db.CatalogImages.AsNoTracking()
            .Where(i => i.TenantId == tenantId && !i.StockCode.StartsWith(CatalogBanners.ReservedPrefix)).ToListAsync(ct);
        var view = Compose(stock, settings, categoryRows, productRows, images);
        cache.Set(key, view, new MemoryCacheEntryOptions { SlidingExpiration = Idle });
        return view;
    }

    internal static StockSide BuildStock(long version, IReadOnlyList<PortalRecords.StockPart> parts)
    {
        var listNames = new Dictionary<int, (string Name, bool IncludesVat)>();
        var brandNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var categories = new StockCategories();
        var cards = new Dictionary<string, PortalRecords.CardPart>(StringComparer.OrdinalIgnoreCase);
        var barcodes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var quantities = new Dictionary<string, Dictionary<int, decimal>>(StringComparer.OrdinalIgnoreCase);
        var prices = new Dictionary<string, Dictionary<int, decimal>>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in parts)
        {
            switch (part)
            {
                case PortalRecords.LookupPart lookup when lookup.Kind == "price_list":
                    listNames[lookup.Number] = (lookup.Name, lookup.IncludesVat);
                    break;
                case PortalRecords.NamePart name when name.Kind == "stock_brand":
                    brandNames[name.Code] = name.Name;
                    break;
                case PortalRecords.SubGroupPart subGroup:
                    categories.Add(subGroup.Code, subGroup.Name);
                    break;
                case PortalRecords.CardPart card:
                    cards[card.Code] = card;
                    break;
                case PortalRecords.BarcodePart barcode:
                    if (!barcodes.TryGetValue(barcode.Code, out var list)) barcodes[barcode.Code] = list = [];
                    if (!list.Contains(barcode.Barcode)) list.Add(barcode.Barcode);
                    break;
                case PortalRecords.InventoryPart inventory:
                    if (!quantities.TryGetValue(inventory.Code, out var byWarehouse)) quantities[inventory.Code] = byWarehouse = [];
                    byWarehouse[inventory.WarehouseNo] = byWarehouse.GetValueOrDefault(inventory.WarehouseNo) + inventory.Quantity;
                    break;
                case PortalRecords.PricePart price:
                    if (!prices.TryGetValue(price.Code, out var byList)) prices[price.Code] = byList = [];
                    byList.TryAdd(price.ListNumber, price.Price);
                    break;
            }
        }

        var products = new List<StockProduct>(cards.Count);
        foreach (var (code, card) in cards)
        {
            products.Add(new StockProduct(
                code,
                card.Name,
                card.Unit,
                card.Brand is { } brand ? brandNames.GetValueOrDefault(brand) ?? brand : null,
                barcodes.GetValueOrDefault(code) ?? [],
                CategoryKey(categories.Resolve(card.MainGroup, card.SubGroup) ?? card.Category),
                card.VatRate ?? 0m,
                prices.GetValueOrDefault(code) ?? [],
                IsInStock(quantities.GetValueOrDefault(code)),
                ErpCartonQuantity(card.CartonCode)));
        }

        var priced = products.SelectMany(p => p.Prices.Keys).ToHashSet();
        var priceLists = listNames.Keys.Union(priced).Order()
            .Select(no => listNames.TryGetValue(no, out var named) ? new CatalogPriceList(no, named.Name, named.IncludesVat) : new CatalogPriceList(no, $"Liste {no}", false))
            .ToList();
        int? defaultList = priced.Count == 0 ? null : priced.Contains(1) ? 1 : priced.Min();
        return new StockSide(version, products, priceLists, defaultList);
    }

    private static CatalogView Compose(
        StockSide stock, CatalogSettings? settings, List<CatalogCategorySetting> categoryRows, List<CatalogProductSetting> productRows, List<CatalogImage> images)
    {
        var categorySettings = categoryRows.ToDictionary(r => r.CategoryKey, StringComparer.Ordinal);
        var productSettings = productRows.ToDictionary(r => r.StockCode, StringComparer.OrdinalIgnoreCase);
        var pictures = images
            .Select(i => (Image: i, Thumb: CatalogImages.ThumbUrl(i), Full: CatalogImages.FullUrl(i)))
            .Where(x => x.Thumb is not null && x.Full is not null)
            .GroupBy(x => x.Image.StockCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<CatalogPicture>)[.. g.OrderBy(x => x.Image.SortOrder).ThenBy(x => x.Image.CreatedAtMs).ThenBy(x => x.Image.Id)
                    .Select(x => new CatalogPicture(x.Image.Id, x.Thumb!, x.Full!))],
                StringComparer.OrdinalIgnoreCase);

        var products = new Dictionary<string, CatalogProduct>(stock.Products.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var p in stock.Products)
        {
            var setting = productSettings.GetValueOrDefault(p.Code);
            products[p.Code] = new CatalogProduct(
                p.Code, p.Name, p.Unit, p.Brand, p.Barcodes, p.CategoryKey, p.VatRate, p.Prices, p.InStock, p.ErpCartonQuantity,
                setting?.SortOrder, setting?.IsHidden ?? false, setting?.NoDiscount ?? false, setting?.CartonQuantity, setting?.CartonOnly ?? false,
                pictures.GetValueOrDefault(p.Code) ?? []);
        }

        var categories = products.Values
            .GroupBy(p => p.CategoryKey, StringComparer.Ordinal)
            .Select(g =>
            {
                var setting = categorySettings.GetValueOrDefault(g.Key);
                return new CatalogCategory(g.Key, CategoryId(g.Key), setting?.SortOrder, setting?.IsHidden ?? false,
                    [.. g.OrderBy(p => p.SortOrder is null ? 1 : 0).ThenBy(p => p.SortOrder ?? 0).ThenBy(p => p.Name, ByText).ThenBy(p => p.Code, ByText)]);
            })
            .OrderBy(c => c.SortOrder is null ? 1 : 0).ThenBy(c => c.SortOrder ?? 0).ThenBy(c => c.Key, ByText)
            .ToList();

        return new CatalogView(
            stock.Version,
            settings?.Revision ?? 0,
            settings?.IsEnabled ?? false,
            settings?.DefaultPriceListNo,
            settings?.DefaultPriceListNo ?? stock.DefaultPriceListNo,
            stock.PriceLists,
            categories,
            products)
        {
            Stock = stock,
            ImageRevision = settings?.ImageRevision ?? 0,
        };
    }

    private static (string, Guid) CacheKey(Guid tenantId) => ("customer-catalog-view", tenantId);

    /// <summary>The company's build lock (tests hold it to show a customer page does not wait for it).</summary>
    internal SemaphoreSlim GateOf(Guid tenantId) => _gates.GetOrAdd(tenantId, _ => new SemaphoreSlim(1, 1));

    /// <summary>The category the phone shows, trimmed, or "Diğer"; cut to the settings column's 160 characters.</summary>
    public static string CategoryKey(string? name)
    {
        var key = string.IsNullOrWhiteSpace(name) ? OtherCategory : name.Trim();
        return key.Length > CatalogVisibility.MaxCategoryKeyLength ? key[..CatalogVisibility.MaxCategoryKeyLength] : key;
    }

    /// <summary>The customer API's category id: the first 12 hex digits of SHA-256(key), safe in a query string.</summary>
    public static string CategoryId(string key) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..12];

    /// <summary>
    /// The ERP's carton (the phone's <c>quickBoxQuantity</c>): the text trimmed, a comma read as the decimal point, a
    /// whole number above 1. "12" and "12,0" are 12; "1", "12,5" and "abc" are none.
    /// </summary>
    public static int? ErpCartonQuantity(string? cartonCode)
    {
        var text = cartonCode?.Trim().Replace(',', '.');
        if (string.IsNullOrEmpty(text) || !double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)) return null;
        if (!double.IsFinite(value) || value > int.MaxValue) return null;
        var whole = (int)value;
        return whole > 1 && whole == value ? whole : null;
    }

    /// <summary>The phone's stock: each warehouse rounded half away from zero, added up; in stock from 1.</summary>
    public static bool IsInStock(IReadOnlyDictionary<int, decimal>? byWarehouse) =>
        byWarehouse is not null && byWarehouse.Values.Sum(q => Math.Round(q, MidpointRounding.AwayFromZero)) >= 1m;
}
