using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace ErpBridge.Portal.Api;

/// <summary>
/// What the stock page shows: filters, sort and page. It travels two ways — to the central API
/// (<c>/api/v1/portal/stock/search</c>, English names) and in the page address (Turkish names),
/// so a refresh, the back button and a shared link keep the list as it was.
/// </summary>
public sealed class StockFilter
{
    public static readonly int[] PageSizes = [25, 50, 100, 250];

    /// <summary>Server status → address word.</summary>
    private static readonly (string Server, string Address)[] StatusWords =
        [("all", "tumu"), ("in", "stokta"), ("out", "tukenen"), ("negative", "eksi"), ("below", "esik")];

    public static readonly string[] Sorts = ["name", "code", "qty", "price", "group", "brand", "shelf", "lastMovement"];

    public string? Search { get; set; }
    public List<string> MainGroups { get; set; } = [];
    public List<string> SubGroups { get; set; } = [];
    public List<string> Brands { get; set; } = [];
    public List<string> Shelves { get; set; } = [];
    public int? Warehouse { get; set; }
    public int? PriceList { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Status { get; set; } = "all";
    public decimal? Below { get; set; }
    public int? IdleDays { get; set; }
    public string Sort { get; set; } = "name";
    public bool Descending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    /// <summary>Filters chosen in the filter panel (not search, status, sort or page).</summary>
    public int PanelFilterCount =>
        MainGroups.Count + SubGroups.Count + Brands.Count + Shelves.Count
        + (Warehouse is null ? 0 : 1) + (PriceList is null ? 0 : 1)
        + (MinQuantity is null && MaxQuantity is null ? 0 : 1)
        + (MinPrice is null && MaxPrice is null ? 0 : 1)
        + (IdleDays is null ? 0 : 1);

    public StockFilter Clone() => new()
    {
        Search = Search,
        MainGroups = [.. MainGroups],
        SubGroups = [.. SubGroups],
        Brands = [.. Brands],
        Shelves = [.. Shelves],
        Warehouse = Warehouse,
        PriceList = PriceList,
        MinQuantity = MinQuantity,
        MaxQuantity = MaxQuantity,
        MinPrice = MinPrice,
        MaxPrice = MaxPrice,
        Status = Status,
        Below = Below,
        IdleDays = IdleDays,
        Sort = Sort,
        Descending = Descending,
        Page = Page,
        PageSize = PageSize,
    };

    /// <summary>The query string of the central API call, starting with "?".</summary>
    public string ToApiQuery() => Build(
    [
        ("q", Search), .. Many("mainGroup", MainGroups), .. Many("subGroup", SubGroups), .. Many("brand", Brands), .. Many("shelf", Shelves),
        ("warehouse", Number(Warehouse)), ("priceList", Number(PriceList)),
        ("minQty", Number(MinQuantity)), ("maxQty", Number(MaxQuantity)), ("minPrice", Number(MinPrice)), ("maxPrice", Number(MaxPrice)),
        ("status", Status == "all" ? null : Status), ("below", Status == "below" ? Number(Below ?? 0) : null), ("idleDays", Number(IdleDays)),
        ("sort", Sort), ("dir", Descending ? "desc" : "asc"),
        ("page", Number(Page)), ("pageSize", Number(PageSize)),
    ]);

    /// <summary>The page address query; defaults are left out so a plain list has a plain address.</summary>
    public string ToAddressQuery() => Build(
    [
        ("q", Search), .. Many("grup", MainGroups), .. Many("altgrup", SubGroups), .. Many("marka", Brands), .. Many("reyon", Shelves),
        ("depo", Number(Warehouse)), ("liste", Number(PriceList)),
        ("minMiktar", Number(MinQuantity)), ("maxMiktar", Number(MaxQuantity)), ("minFiyat", Number(MinPrice)), ("maxFiyat", Number(MaxPrice)),
        ("durum", Status == "all" ? null : StatusWords.First(w => w.Server == Status).Address),
        ("esik", Status == "below" ? Number(Below) : null), ("hareketsiz", Number(IdleDays)),
        ("sirala", Sort == "name" ? null : Sort), ("yon", Descending ? "azalan" : null),
        ("sayfa", Page == 1 ? null : Number(Page)), ("boyut", PageSize == 50 ? null : Number(PageSize)),
    ]);

    public static StockFilter FromAddress(string query)
    {
        var values = QueryHelpers.ParseQuery(query);
        string? One(string name) => values.TryGetValue(name, out var v) && !StringValues.IsNullOrEmpty(v) ? v[^1]!.Trim() : null;
        List<string> All(string name) => values.TryGetValue(name, out var v)
            ? v.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            : [];
        static int? Int(string? s) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
        static decimal? Dec(string? s) => decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var n) ? n : null;

        var status = StatusWords.FirstOrDefault(w => w.Address == One("durum")).Server ?? "all";
        var sort = One("sirala");
        var pageSize = Int(One("boyut"));
        return new StockFilter
        {
            Search = One("q"),
            MainGroups = All("grup"),
            SubGroups = All("altgrup"),
            Brands = All("marka"),
            Shelves = All("reyon"),
            Warehouse = Int(One("depo")),
            PriceList = Int(One("liste")),
            MinQuantity = Dec(One("minMiktar")),
            MaxQuantity = Dec(One("maxMiktar")),
            MinPrice = Dec(One("minFiyat")),
            MaxPrice = Dec(One("maxFiyat")),
            Status = status,
            Below = Dec(One("esik")),
            IdleDays = Int(One("hareketsiz")) is { } idle and >= 0 ? idle : null,
            Sort = sort is not null && Sorts.Contains(sort) ? sort : "name",
            Descending = One("yon") == "azalan",
            Page = Math.Max(1, Int(One("sayfa")) ?? 1),
            PageSize = pageSize is { } size && PageSizes.Contains(size) ? size : 50,
        };
    }

    private static IEnumerable<(string, string?)> Many(string name, IEnumerable<string> values) => values.Select(v => (name, (string?)v));

    private static string? Number(int? value) => value?.ToString(CultureInfo.InvariantCulture);

    private static string? Number(decimal? value) => value?.ToString(CultureInfo.InvariantCulture);

    private static string Build(IEnumerable<(string Name, string? Value)> parts)
    {
        var present = parts.Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .Select(p => $"{p.Name}={Uri.EscapeDataString(p.Value!.Trim())}")
            .ToList();
        return present.Count == 0 ? string.Empty : "?" + string.Join("&", present);
    }
}
