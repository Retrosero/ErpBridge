using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Read-only mobile API over the latest bootstrap package for the authenticated
/// tenant. A mobile key must be explicitly created with the <c>mobile:read</c>
/// scope; an ingest-only key cannot read ERP reference data.
/// </summary>
public static class AndroidEndpoints
{
    private const string MobileReadScope = "mobile:read";

    public static IEndpointRouteBuilder MapAndroidEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/android").WithTags("Android");

        group.MapPost("/bootstrap", BootstrapAsync).WithName("AndroidBootstrap")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/pull", PullAsync).WithName("AndroidPull")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        group.MapPost("/sync/cari", CustomersAsync)
            .WithName("AndroidCustomers")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sync/urun", ProductCatalogAsync)
            .WithName("AndroidProductCatalog")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        MapSection(group, "/sync/stokSeviye", "inventory");
        group.MapPost("/sync/fiyatlar", PriceListRowsAsync)
            .WithName("AndroidPrices")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sync/stokSatisFiyatListeleri", PriceListRowsAsync)
            .WithName("AndroidPriceListRows")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sync/stokSatisFiyatListeTanimlari", PriceListDefinitionsAsync)
            .WithName("AndroidPriceListDefinitions")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        MapSection(group, "/sync/acikSiparisler", "openOrders");
        group.MapPost("/sync/cariAdresler", CustomerAddressesAsync)
            .WithName("AndroidCustomerAddresses")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        MapSection(group, "/sync/cariYetkililer", "customerContacts");
        MapSection(group, "/sync/barkodlar", "barcodes");
        MapSection(group, "/sync/satisSartlari", "salesConditions");
        group.MapPost("/sync/bankalar", (AndroidPageRequest? request, HttpContext http, CentralApiDbContext db, CancellationToken ct) =>
                CashAndBankSectionAsync("bank", "bankalar", request ?? new AndroidPageRequest(), http, db, ct))
            .WithName("AndroidBanks")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sync/kasalar", (AndroidPageRequest? request, HttpContext http, CentralApiDbContext db, CancellationToken ct) =>
                CashAndBankSectionAsync("cash", "kasalar", request ?? new AndroidPageRequest(), http, db, ct))
            .WithName("AndroidCashRegisters")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sync/kasaYonetim", (AndroidPageRequest? request, HttpContext http, CentralApiDbContext db, CancellationToken ct) =>
                CashAndBankSectionAsync("cash", "kasaYonetim", request ?? new AndroidPageRequest(), http, db, ct))
            .WithName("AndroidCashManagement")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        MapPagedSection(group, "/sync/cariHareketleri", "customerTransactions");
        group.MapPost("/sync/stokHareket", StockMovementsAsync)
            .WithName("AndroidStockMovements")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sync/stokHareketleri", StockMovementsAsync)
            .WithName("AndroidStockMovementsPlural")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sync/faturaHareket", InvoiceMovementsAsync)
            .WithName("AndroidInvoiceMovements")
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        return routes;
    }

    private static void MapSection(RouteGroupBuilder group, string route, string propertyName) =>
        group.MapPost(route, (AndroidPageRequest? request, HttpContext http, CentralApiDbContext db, CancellationToken ct) =>
                SectionAsync(propertyName, request ?? new AndroidPageRequest(), http, db, ct))
            .WithName("Android" + propertyName)
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);

    private static void MapPagedSection(RouteGroupBuilder group, string route, string propertyName) =>
        group.MapPost(route, (AndroidPageRequest request, HttpContext http, CentralApiDbContext db, CancellationToken ct) =>
                PagedSectionAsync(propertyName, request, http, db, ct))
            .WithName("AndroidPaged" + route.Replace("/", string.Empty))
            .RequireAuthorization(Program.ApiKeyPolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);

    private static async Task<IResult> BootstrapAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        var package = access.Package!;
        return Results.Ok(new { tenantId = package.TenantId, sourceDatabase = package.SourceDatabase, pulledAtUtc = package.PulledAtUtc, receivedAtUtc = package.ReceivedAtUtc });
    }

    private static async Task<IResult> PullAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        var package = access.Package!;
        using var document = JsonDocument.Parse(package.PayloadJson);
        return Results.Ok(new { sourceDatabase = package.SourceDatabase, pulledAtUtc = package.PulledAtUtc, receivedAtUtc = package.ReceivedAtUtc, data = document.RootElement.Clone() });
    }

    private static async Task<IResult> SectionAsync(
        string propertyName,
        AndroidPageRequest request,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        var package = access.Package!;
        using var document = JsonDocument.Parse(package.PayloadJson);
        var allItems = GetArray(document.RootElement, propertyName).Select(item => item.Clone()).ToArray();
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 500);
        var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
        return Results.Ok(new
        {
            entity = propertyName,
            sourceDatabase = package.SourceDatabase,
            pulledAtUtc = package.PulledAtUtc,
            page,
            pageSize,
            total = allItems.Length,
            items,
        });
    }

    private static async Task<IResult> ProductCatalogAsync(
        AndroidPageRequest? request,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        var package = access.Package!;
        using var document = JsonDocument.Parse(package.PayloadJson);
        var root = document.RootElement;

        var barcodesByStock = GetArray(root, "barcodes")
            .Where(item => !string.IsNullOrWhiteSpace(GetString(item, "stockCode")))
            .GroupBy(item => GetString(item, "stockCode")!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Select(item => item.Clone()).ToArray(), StringComparer.OrdinalIgnoreCase);

        var pricesByStock = GetArray(root, "prices")
            .Where(item => !string.IsNullOrWhiteSpace(GetString(item, "stockCode")) && GetDecimal(item, "price") is > 0)
            .GroupBy(item => GetString(item, "stockCode")!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(item => GetInt32(item, "listNumber") == 1 ? 0 : 1)
                    .ThenBy(item => GetInt32(item, "listNumber") ?? int.MaxValue)
                    .Select(item => GetDecimal(item, "price"))
                    .FirstOrDefault(price => price is > 0),
                StringComparer.OrdinalIgnoreCase);

        var priceListNames = GetArray(root, "lookups")
            .Where(item => string.Equals(GetString(item, "kind"), "price_list", StringComparison.OrdinalIgnoreCase))
            .Select(item => new
            {
                Number = int.TryParse(GetString(item, "code"), out var number) ? number : 0,
                Name = GetString(item, "name"),
            })
            .Where(item => item.Number > 0 && !string.IsNullOrWhiteSpace(item.Name))
            .GroupBy(item => item.Number)
            .ToDictionary(group => group.Key, group => group.First().Name!);

        var customPricesByStock = GetArray(root, "prices")
            .Where(item => !string.IsNullOrWhiteSpace(GetString(item, "stockCode")) && GetDecimal(item, "price") is > 0)
            .GroupBy(item => GetString(item, "stockCode")!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .GroupBy(item => GetInt32(item, "listNumber") ?? 0)
                    .Where(list => list.Key > 0)
                    .ToDictionary(
                        list => priceListNames.TryGetValue(list.Key, out var name) ? name : $"Liste {list.Key}",
                        list => list.Select(item => GetDecimal(item, "price")).First(price => price is > 0)!.Value),
                StringComparer.OrdinalIgnoreCase);

        var inventoryByStock = GetArray(root, "inventory")
            .Where(item => !string.IsNullOrWhiteSpace(GetString(item, "stockCode")))
            .GroupBy(item => GetString(item, "stockCode")!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.GroupBy(item => GetInt32(item, "warehouseNo") ?? 0)
                    .ToDictionary(
                        warehouse => $"Depo {warehouse.Key}",
                        warehouse => (int)Math.Round(warehouse.Sum(item => GetDecimal(item, "quantity") ?? 0m),
                            MidpointRounding.AwayFromZero)),
                StringComparer.OrdinalIgnoreCase);

        var allItems = GetArray(root, "stocks").Select(stock =>
        {
            var mapped = stock.EnumerateObject()
                .ToDictionary(property => property.Name, property => (object?)property.Value.Clone(), StringComparer.OrdinalIgnoreCase);
            var stockCode = GetString(stock, "stockCode") ?? string.Empty;

            var reyonKod = GetFirstString(stock, "shelfCode", "sto_yer_kod");
            var olcu = GetFirstString(stock, "sectorCode", "sto_sektor_kodu");
            var ambalaj = GetFirstString(stock, "packageCode", "sto_ambalaj_kodu");
            var marka = GetFirstString(stock, "brandCode", "sto_marka_kodu");
            var koliAdet = GetFirstString(stock, "cartonCode", "sto_kalkon_kodu");

            mapped["reyonKod"] = reyonKod;
            mapped["olcu"] = olcu;
            mapped["ambalaj"] = ambalaj;
            mapped["marka"] = marka;
            mapped["koliAdet"] = koliAdet;
            mapped["sto_yer_kod"] = reyonKod;
            mapped["sto_sektor_kodu"] = olcu;
            mapped["sto_ambalaj_kodu"] = ambalaj;
            mapped["sto_marka_kodu"] = marka;
            mapped["sto_kalkon_kodu"] = koliAdet;

            if (barcodesByStock.TryGetValue(stockCode, out var barcodes) && barcodes.Length > 0)
            {
                mapped["barcodes"] = barcodes;
                mapped["barkod"] = GetString(barcodes[0], "barcode") ?? string.Empty;
            }

            if (pricesByStock.TryGetValue(stockCode, out var price) && price is > 0)
            {
                mapped["satis_fiyati"] = price.Value;
                mapped["price"] = price.Value;
            }
            mapped["customPrices"] = customPricesByStock.TryGetValue(stockCode, out var customPrices)
                ? customPrices
                : new Dictionary<string, decimal>();

            if (inventoryByStock.TryGetValue(stockCode, out var warehouses))
            {
                mapped["stockByWarehouse"] = warehouses;
                mapped["stok"] = warehouses.Values.Sum();
            }
            else
            {
                mapped["stockByWarehouse"] = new Dictionary<string, int>();
                mapped["stok"] = 0;
            }

            return mapped;
        }).ToArray();

        var page = Math.Max(1, request?.Page ?? 1);
        var pageSize = Math.Clamp(request?.PageSize ?? 200, 1, 500);
        var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

        return Results.Ok(new
        {
            sourceDatabase = package.SourceDatabase,
            pulledAtUtc = package.PulledAtUtc,
            page,
            pageSize,
            total = allItems.Length,
            items,
        });
    }

    /// <summary>
    /// Projects the canonical customer payload to the legacy Android names.
    /// The mobile DTO intentionally uses Turkish field names, whereas the
    /// bridge package uses English canonical names. Returning the raw payload
    /// silently discarded tax, phone and region values on Android.
    /// </summary>
    private static async Task<IResult> CustomersAsync(
        AndroidPageRequest? request,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        var package = access.Package!;
        using var document = JsonDocument.Parse(package.PayloadJson);
        var page = Math.Max(1, request?.Page ?? 1);
        var pageSize = Math.Clamp(request?.PageSize ?? 200, 1, 500);
        var all = GetArray(document.RootElement, "customers").ToArray();
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).Select(customer => new
        {
            id = GetString(customer, "customerCode"),
            erpRef = GetString(customer, "customerCode"),
            cariKod = GetString(customer, "customerCode"),
            unvan = JoinAddressLine(GetString(customer, "title1"), GetString(customer, "title2")),
            cariUnvan = GetString(customer, "title1"),
            vergiNo = GetString(customer, "taxNo"),
            vergiDairesi = GetString(customer, "taxOffice"),
            telefon = GetString(customer, "phone"),
            email = GetString(customer, "email"),
            cariBolgeKodu = GetString(customer, "regionCode"),
            paraBirimi = GetString(customer, "currency"),
            bakiye = GetDecimal(customer, "balance") ?? 0m,
            updatedAt = package.PulledAtUtc,
            isDeleted = false,
        }).ToArray();

        return Results.Ok(new
        {
            entity = "cari",
            sourceDatabase = package.SourceDatabase,
            pulledAtUtc = package.PulledAtUtc,
            page,
            pageSize,
            total = all.Length,
            items,
        });
    }

    private static async Task<IResult> CashAndBankSectionAsync(
        string kind,
        string entity,
        AndroidPageRequest? request,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        using var document = JsonDocument.Parse(access.Package!.PayloadJson);
        var allItems = GetArray(document.RootElement, "cashAndBank")
            .Where(item => string.Equals(GetString(item, "kind"), kind, StringComparison.OrdinalIgnoreCase))
            .Select(item => new
            {
                erpRef = GetString(item, "code") ?? string.Empty,
                erp = "MIKRO",
                kod = GetString(item, "code") ?? string.Empty,
                isim = GetString(item, "name") ?? string.Empty,
                sube = GetString(item, "branch"),
                hesapNumarasi = GetString(item, "accountNo"),
                tCMBKodu = GetString(item, "tcmbCode"),
                dovizCinsi = GetInt32(item, "currency"),
                muhasebeKod = (string?)null,
                tip = kind == "cash" ? 0 : 1,
            })
            .ToArray();
        var page = Math.Max(1, request?.Page ?? 1);
        var pageSize = Math.Clamp(request?.PageSize ?? 200, 1, 500);
        var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
        return Results.Ok(new { entity, page, pageSize, total = allItems.Length, items });
    }

    /// <summary>
    /// Translates the canonical bootstrap address payload to the Android contract.
    /// The mobile client deliberately uses Turkish field names because these values
    /// are also stored verbatim in its offline Room database.  Returning the raw
    /// <c>CustomerAddressPayload</c> here used to expose <c>customerCode</c>,
    /// <c>city</c> and <c>street</c>, silently leaving cariKod/il/sokak empty.
    /// </summary>
    private static async Task<IResult> CustomerAddressesAsync(
        AndroidPageRequest? request,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        var package = access.Package!;
        using var document = JsonDocument.Parse(package.PayloadJson);
        var page = Math.Max(1, request?.Page ?? 1);
        var pageSize = Math.Clamp(request?.PageSize ?? 200, 1, 500);

        var allItems = GetArray(document.RootElement, "customerAddresses")
            .Select(address =>
            {
                var cariKod = GetString(address, "customerCode") ?? string.Empty;
                var adresNo = GetInt32(address, "addressNo") ?? 0;
                var telUlkeKodu = GetString(address, "phoneCountryCode");
                var telBolgeKodu = GetString(address, "phoneAreaCode");
                var telNo = GetString(address, "phoneNo");
                var phone = string.Join(" ", new[] { telUlkeKodu, telBolgeKodu, telNo }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
                return new
                {
                    erpRef = $"ADR-{cariKod}-{adresNo}",
                    erp = "MIKRO",
                    cariKod,
                    adresNo,
                    yazdirilabilir = GetBoolean(address, "isPrintable") ?? false,
                    cadde = GetString(address, "avenue"),
                    mahalle = GetString(address, "neighborhood"),
                    sokak = GetString(address, "streetName") ?? GetString(address, "street"),
                    semt = GetString(address, "quarter"),
                    ilce = GetString(address, "district"),
                    il = GetString(address, "city"),
                    ulke = GetString(address, "country"),
                    postaKodu = GetString(address, "postalCode"),
                    telUlkeKodu,
                    telBolgeKodu,
                    telNo1 = telNo,
                    telefon = phone,
                    gpsEnlem = GetDouble(address, "latitude"),
                    gpsBoylam = GetDouble(address, "longitude"),
                    ziyaretPeriyodu = GetInt32(address, "visitPeriod"),
                    ziyaretGunu = GetInt32(address, "visitDay"),
                    eFaturaAlias = GetString(address, "eInvoiceAlias"),
                    adresSatir1 = JoinAddressLine(GetString(address, "avenue"), GetString(address, "neighborhood"), GetString(address, "streetName")),
                    adresSatir2 = JoinAddressLine(GetString(address, "quarter"), GetString(address, "apartmentNo"), GetString(address, "flatNo")),
                    updatedAt = GetString(address, "updatedAt"),
                };
            })
            .OrderBy(item => item.cariKod, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.adresNo)
            .ToArray();

        var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
        return Results.Ok(new { entity = "cariAdresler", page, pageSize, total = allItems.Length, items });
    }

    private static async Task<IResult> PriceListDefinitionsAsync(
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        using var document = JsonDocument.Parse(access.Package!.PayloadJson);
        var items = GetArray(document.RootElement, "lookups")
            .Where(item => string.Equals(GetString(item, "kind"), "price_list", StringComparison.OrdinalIgnoreCase))
            .Select(item =>
            {
                var code = GetString(item, "code") ?? string.Empty;
                return new
                {
                    id = code,
                    erpRef = code,
                    listNo = int.TryParse(code, out var number) ? number : 0,
                    aciklama = GetString(item, "name"),
                    sfl_sirano = int.TryParse(code, out var sflSirano) ? sflSirano : 0,
                    sfl_aciklama = GetString(item, "name"),
                    sfl_fiyatformul = GetString(item, "parentCode"),
                    isDeleted = false,
                };
            })
            .Where(item => item.listNo > 0)
            .OrderBy(item => item.listNo)
            .ToArray();
        return Results.Ok(new { entity = "stokSatisFiyatListeTanimlari", total = items.Length, items });
    }

    private static async Task<IResult> StockMovementsAsync(
        AndroidStockMovementRequest request,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        using var document = JsonDocument.Parse(access.Package!.PayloadJson);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 500);
        // `since` was historically (and incorrectly) overloaded as a stock
        // code.  Treat a timestamp as the incremental watermark while keeping
        // the old stock-code request usable for the stock-card screen.
        var rawSince = request.Since?.Trim();
        var hasWatermark = DateTimeOffset.TryParse(rawSince, out var watermark);
        var stockCode = hasWatermark ? null : rawSince;
        var offset = (page - 1) * pageSize;
        var total = 0;
        string? latestWatermark = rawSince;
        var items = new List<JsonElement>(pageSize);
        foreach (var item in GetArray(document.RootElement, "stockTransactions"))
        {
            var updatedAt = GetString(item, "updatedAt") ?? GetString(item, "sth_lastup_date") ?? GetString(item, "tarih");
            if (hasWatermark && (!DateTimeOffset.TryParse(updatedAt, out var updated) || updated <= watermark)) continue;
            if (!string.IsNullOrWhiteSpace(stockCode)
                && !string.Equals(GetString(item, "stokKod"), stockCode, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(GetString(item, "urunKod"), stockCode, StringComparison.OrdinalIgnoreCase)) continue;

            if (items.Count == 0 || total >= offset)
            {
                if (total >= offset && items.Count < pageSize) items.Add(item.Clone());
            }
            total++;
            if (DateTimeOffset.TryParse(updatedAt, out var candidate)
                && (!DateTimeOffset.TryParse(latestWatermark, out var latest) || candidate > latest))
                latestWatermark = candidate.ToUniversalTime().ToString("O");
        }
        return Results.Ok(new
        {
            entity = "stokHareket",
            stokKod = stockCode,
            page,
            pageSize,
            total,
            since = rawSince,
            watermark = latestWatermark,
            items,
        });
    }

    private static async Task<IResult> PriceListRowsAsync(
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        var package = access.Package!;
        using var document = JsonDocument.Parse(package.PayloadJson);
        var root = document.RootElement;
        var namesByListNo = GetArray(root, "lookups")
            .Where(item => string.Equals(GetString(item, "kind"), "price_list", StringComparison.OrdinalIgnoreCase))
            .Select(item => new { Number = GetInt32(item, "code") ?? 0, Name = GetString(item, "name") })
            .Where(item => item.Number > 0 && !string.IsNullOrWhiteSpace(item.Name))
            .GroupBy(item => item.Number)
            .ToDictionary(group => group.Key, group => group.First().Name!);

        var items = GetArray(root, "prices").Select(price =>
        {
            var mapped = price.EnumerateObject()
                .ToDictionary(property => property.Name, property => (object?)property.Value.Clone(), StringComparer.OrdinalIgnoreCase);
            var listNo = GetInt32(price, "listNumber") ?? GetInt32(price, "sfiyat_listeno") ?? 0;
            var listName = namesByListNo.TryGetValue(listNo, out var name) ? name : string.Empty;
            mapped["listName"] = listName;
            mapped["aciklama"] = listName;
            return mapped;
        }).ToArray();

        return Results.Ok(new { entity = "stokSatisFiyatListeleri", sourceDatabase = package.SourceDatabase, pulledAtUtc = package.PulledAtUtc, total = items.Length, items });
    }

    private static async Task<IResult> PagedSectionAsync(
        string propertyName,
        AndroidPageRequest request,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        var package = access.Package!;
        using var document = JsonDocument.Parse(package.PayloadJson);
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 500);
        var allItems = document.RootElement.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Select(item => item.Clone()).ToArray()
            : Array.Empty<JsonElement>();
        var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

        return Results.Ok(new
        {
            entity = propertyName,
            sourceDatabase = package.SourceDatabase,
            pulledAtUtc = package.PulledAtUtc,
            page,
            pageSize,
            total = allItems.Length,
            since = package.PulledAtUtc,
            items,
        });
    }

    private static async Task<IResult> InvoiceMovementsAsync(
        AndroidInvoiceRequest request,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        var package = access.Package!;
        using var document = JsonDocument.Parse(package.PayloadJson);
        var root = document.RootElement;

        var stockNames = GetArray(root, "stocks")
            .Select(stock => new
            {
                Code = GetString(stock, "stockCode"),
                Name = GetString(stock, "name"),
            })
            .Where(stock => !string.IsNullOrWhiteSpace(stock.Code))
            .GroupBy(stock => stock.Code!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Name, StringComparer.OrdinalIgnoreCase);

        var linesByInvoiceRecNo = GetArray(root, "stockTransactions")
            .Select(line => new { Line = line, InvoiceRecNo = GetInt32(line, "faturaRecno") })
            .Where(item => item.InvoiceRecNo is > 0)
            .GroupBy(item => item.InvoiceRecNo!.Value)
            .ToDictionary(group => group.Key, group => group.Select(item => item.Line).ToArray());

        var customerCode = request.Since?.Trim();
        var invoices = GetArray(root, "customerTransactions")
            .Where(transaction =>
                string.IsNullOrWhiteSpace(customerCode)
                || string.Equals(GetString(transaction, "cariKod"), customerCode, StringComparison.OrdinalIgnoreCase))
            .Select(transaction => new
            {
                Transaction = transaction,
                RecNo = GetInt32(transaction, "cha_recno"),
            })
            .Where(item => item.RecNo is > 0 && linesByInvoiceRecNo.ContainsKey(item.RecNo.Value))
            .ToArray();

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 500);
        var items = invoices
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(invoice =>
            {
                var transaction = invoice.Transaction;
                var lines = linesByInvoiceRecNo[invoice.RecNo!.Value]
                    .Select(line =>
                    {
                        var stockCode = GetString(line, "stokKod");
                        return new
                        {
                            erpRef = GetString(line, "erpRef") ?? GetString(line, "id") ?? string.Empty,
                            stokKod = stockCode,
                            stokAd = stockCode is not null && stockNames.TryGetValue(stockCode, out var stockName) ? stockName : null,
                            tarih = GetString(line, "tarih"),
                            tip = GetInt32(line, "tip"),
                            cins = GetInt32(line, "cins"),
                            girisMiktar = GetDecimal(line, "girisMiktar"),
                            cikisMiktar = GetDecimal(line, "cikisMiktar"),
                            miktar = GetDecimal(line, "miktar"),
                            birimFiyat = GetDecimal(line, "birimFiyat"),
                            tutar = GetDecimal(line, "tutar"),
                            vergi = GetDecimal(line, "vergi"),
                            girisDepoNo = GetInt32(line, "girisDepoNo"),
                            cikisDepoNo = GetInt32(line, "cikisDepoNo"),
                            aciklama = GetString(line, "aciklama"),
                            updatedAt = GetString(line, "updatedAt"),
                            sth_fat_recid_recno = invoice.RecNo,
                        };
                    })
                    .ToArray();

                return new
                {
                    erpRef = GetString(transaction, "erpRef") ?? invoice.RecNo.Value.ToString(),
                    erp = GetString(transaction, "erp"),
                    cariKod = GetString(transaction, "cariKod"),
                    tarih = GetString(transaction, "tarih"),
                    evrakTip = GetInt32(transaction, "evrakTip"),
                    evrakNo = GetString(transaction, "evrakNo"),
                    tip = GetInt32(transaction, "tip"),
                    tutar = GetDecimal(transaction, "tutar"),
                    updatedAt = GetString(transaction, "updatedAt"),
                    satirlar = lines,
                };
            })
            .ToArray();

        return Results.Ok(new
        {
            entity = "faturaHareket",
            cariKod = customerCode,
            page,
            pageSize,
            total = invoices.Length,
            since = customerCode,
            items,
        });
    }

    private static IEnumerable<JsonElement> GetArray(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
            : [];

    private static string? GetString(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static string GetFirstString(JsonElement item, params string[] propertyNames) =>
        propertyNames
            .Select(propertyName => GetString(item, propertyName)?.Trim())
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
        ?? string.Empty;

    private static int? GetInt32(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
            return number;
        return int.TryParse(value.ToString(), out number) ? number : null;
    }

    private static bool? GetBoolean(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;
        if (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
            return value.GetBoolean();
        return bool.TryParse(value.ToString(), out var result)
            ? result
            : int.TryParse(value.ToString(), out var numeric) ? numeric != 0 : null;
    }

    private static double? GetDouble(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var result))
            return result;
        return double.TryParse(value.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result)
            ? result
            : null;
    }

    private static string? JoinAddressLine(params string?[] values)
    {
        var result = string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!.Trim()));
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    private static decimal? GetDecimal(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
            return number;
        return decimal.TryParse(value.ToString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out number)
            ? number
            : null;
    }

    private static async Task<(BootstrapPackage? Package, IResult? Error)> GetLatestPackageAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
            return (null, JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." }));

        var keyIdText = http.User.FindFirst(ApiKeyClaims.ApiKeyId)?.Value;
        if (!Guid.TryParse(keyIdText, out var keyId))
            return (null, JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "INVALID_API_KEY", Message = "API key identity is missing." }));

        var allowed = await db.ApiKeys.AsNoTracking().AnyAsync(key => key.Id == keyId && key.TenantId == tenantId && key.IsActive && (key.Scopes.Contains(MobileReadScope) || key.Scopes.Contains("*")), ct);
        if (!allowed)
            return (null, JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError { ErrorCode = "MOBILE_READ_SCOPE_REQUIRED", Message = "API key requires the mobile:read scope." }));

        var package = await db.BootstrapPackages.AsNoTracking().Where(item => item.TenantId == tenantId)
            .OrderByDescending(item => item.PulledAtUtc).ThenByDescending(item => item.ReceivedAtUtc).FirstOrDefaultAsync(ct);
        if (package is null)
            return (null, JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "BOOTSTRAP_NOT_FOUND", Message = "No ERP data has been received for this tenant yet." }));

        return (package, null);
    }

    private sealed record AndroidPageRequest(int Page = 1, int PageSize = 200, DateTimeOffset? Since = null);
    private sealed record AndroidStockMovementRequest(int Page = 1, int PageSize = 50, string? Since = null);
    private sealed record AndroidInvoiceRequest(int Page = 1, int PageSize = 200, string? Since = null);
}
