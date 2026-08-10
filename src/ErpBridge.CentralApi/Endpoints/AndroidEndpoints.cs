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
            .RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/pull", PullAsync).WithName("AndroidPull")
            .RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        MapSection(group, "/sync/cari", "customers");
        group.MapPost("/sync/urun", ProductCatalogAsync)
            .WithName("AndroidProductCatalog")
            .RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        MapSection(group, "/sync/stokSeviye", "inventory");
        group.MapPost("/sync/fiyatlar", PriceListRowsAsync)
            .WithName("AndroidPrices")
            .RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sync/stokSatisFiyatListeleri", PriceListRowsAsync)
            .WithName("AndroidPriceListRows")
            .RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sync/stokSatisFiyatListeTanimlari", PriceListDefinitionsAsync)
            .WithName("AndroidPriceListDefinitions")
            .RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        MapSection(group, "/sync/acikSiparisler", "openOrders");
        MapSection(group, "/sync/cariAdresler", "customerAddresses");
        MapSection(group, "/sync/cariYetkililer", "customerContacts");
        MapSection(group, "/sync/barkodlar", "barcodes");
        MapSection(group, "/sync/satisSartlari", "salesConditions");
        MapPagedSection(group, "/sync/cariHareketleri", "customerTransactions");
        group.MapPost("/sync/stokHareket", StockMovementsAsync)
            .WithName("AndroidStockMovements")
            .RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sync/stokHareketleri", StockMovementsAsync)
            .WithName("AndroidStockMovementsPlural")
            .RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sync/faturaHareket", InvoiceMovementsAsync)
            .WithName("AndroidInvoiceMovements")
            .RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        return routes;
    }

    private static void MapSection(RouteGroupBuilder group, string route, string propertyName) =>
        group.MapPost(route, (HttpContext http, CentralApiDbContext db, CancellationToken ct) =>
                SectionAsync(propertyName, http, db, ct))
            .WithName("Android" + propertyName)
            .RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);

    private static void MapPagedSection(RouteGroupBuilder group, string route, string propertyName) =>
        group.MapPost(route, (AndroidPageRequest request, HttpContext http, CentralApiDbContext db, CancellationToken ct) =>
                PagedSectionAsync(propertyName, request, http, db, ct))
            .WithName("AndroidPaged" + route.Replace("/", string.Empty))
            .RequireAuthorization(Program.MobilePolicy).RequireRateLimiting(Program.PerTenantRateLimitPolicy);

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

    private static async Task<IResult> SectionAsync(string propertyName, HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        var package = access.Package!;
        using var document = JsonDocument.Parse(package.PayloadJson);
        var items = document.RootElement.TryGetProperty(propertyName, out var value)
            ? value.Clone()
            : JsonDocument.Parse("[]").RootElement.Clone();
        return Results.Ok(new { sourceDatabase = package.SourceDatabase, pulledAtUtc = package.PulledAtUtc, items });
    }

    private static async Task<IResult> ProductCatalogAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        var package = access.Package!;
        var tenant = await db.Tenants.AsNoTracking()
            .Select(item => new { item.Id, item.StockDetailFieldsJson })
            .FirstOrDefaultAsync(item => item.Id == package.TenantId, ct);
        var detailFields = AdminTenantsEndpoints.ReadStockDetailFields(tenant?.StockDetailFieldsJson);
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

        var items = GetArray(root, "stocks").Select(stock =>
        {
            var mapped = stock.EnumerateObject()
                .ToDictionary(property => property.Name, property => (object?)property.Value.Clone(), StringComparer.OrdinalIgnoreCase);
            var stockCode = GetString(stock, "stockCode") ?? string.Empty;

            // Android-facing names are kept explicit so the mobile client does not
            // need to know Mikro's internal STOKLAR column names.  Preserve the
            // raw names too: this makes old and new agent payloads equivalent.
            var reyonKod = GetConfiguredStockDetailValue(stock, detailFields, "aisle", "shelfCode", "sto_yer_kod");
            var olcu = GetConfiguredStockDetailValue(stock, detailFields, "measurement", "sectorCode", "sto_sektor_kodu");
            var ambalaj = GetConfiguredStockDetailValue(stock, detailFields, "packaging", "packageCode", "sto_ambalaj_kodu");
            var marka = GetConfiguredStockDetailValue(stock, detailFields, "brand", "brandCode", "sto_marka_kodu");
            var koliAdet = GetConfiguredStockDetailValue(stock, detailFields, "cartonQuantity", "cartonCode", "sto_kalkon_kodu");

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

        return Results.Ok(new
        {
            sourceDatabase = package.SourceDatabase,
            pulledAtUtc = package.PulledAtUtc,
            stockDetailFields = detailFields,
            items,
        });
    }

    private static string? GetConfiguredStockDetailValue(
        JsonElement stock,
        IReadOnlyList<StockDetailFieldDefinition> fields,
        string key,
        params string[] fallbackFields)
    {
        var configured = fields.FirstOrDefault(field => string.Equals(field.Key, key, StringComparison.Ordinal));
        return configured is null
            ? GetFirstString(stock, fallbackFields)
            : GetFirstString(stock, new[] { configured.SourceField }.Concat(fallbackFields).ToArray());
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
                    isDeleted = false,
                };
            })
            .Where(item => item.listNo > 0)
            .OrderBy(item => item.listNo)
            .ToArray();
        return Results.Ok(new { entity = "stokSatisFiyatListeTanimlari", total = items.Length, items });
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

    private static async Task<IResult> StockMovementsAsync(
        AndroidStockMovementRequest request,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var access = await GetLatestPackageAsync(http, db, ct);
        if (access.Error is not null) return access.Error;
        using var document = JsonDocument.Parse(access.Package!.PayloadJson);

        var stockCode = request.Since?.Trim();
        var allItems = GetArray(document.RootElement, "stockTransactions")
            .Where(item =>
                string.IsNullOrWhiteSpace(stockCode)
                || string.Equals(GetString(item, "stokKod"), stockCode, StringComparison.OrdinalIgnoreCase)
                || string.Equals(GetString(item, "urunKod"), stockCode, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => GetString(item, "updatedAt") ?? GetString(item, "tarih"))
            .Select(item => item.Clone())
            .ToArray();

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 500);
        var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
        return Results.Ok(new
        {
            entity = "stokHareket",
            stokKod = stockCode,
            page,
            pageSize,
            total = allItems.Length,
            since = stockCode,
            items,
        });
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

        if (MobileLicensingEndpoints.TryDevice(http, out var deviceId, out var deviceTenantId))
        {
            if (deviceTenantId != tenantId)
                return (null, JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "INVALID_DEVICE_TOKEN", Message = "Device tenant does not match the request." }));

            var activeDevice = await db.MobileDevices.AsNoTracking().AnyAsync(
                x => x.Id == deviceId && x.TenantId == tenantId && x.IsActive && x.Tenant!.IsActive, ct);
            if (!activeDevice)
                return (null, JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError { ErrorCode = "DEVICE_REVOKED", Message = "This device is no longer licensed." }));
        }
        else
        {
            // Existing mobile installs continue to use their tenant-scoped key.
            // They can exchange it for a device session through /mobile/migrate,
            // while new installs are counted at activation time.
            var keyIdText = http.User.FindFirst(ApiKeyClaims.ApiKeyId)?.Value;
            if (!Guid.TryParse(keyIdText, out var keyId))
                return (null, JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "INVALID_API_KEY", Message = "API key identity is missing." }));

            var keyAllowed = await db.ApiKeys.AsNoTracking().AnyAsync(
                key => key.Id == keyId && key.TenantId == tenantId && key.IsActive
                    && (key.Scopes.Contains(MobileReadScope) || key.Scopes.Contains("*")), ct);
            if (!keyAllowed)
                return (null, JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError { ErrorCode = "MOBILE_READ_SCOPE_REQUIRED", Message = "API key requires the mobile:read scope." }));
        }

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
