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

        MapSection(group, "/sync/cari", "customers");
        MapSection(group, "/sync/urun", "stocks");
        MapSection(group, "/sync/stokSeviye", "inventory");
        MapSection(group, "/sync/fiyatlar", "prices");
        MapSection(group, "/sync/acikSiparisler", "openOrders");
        MapSection(group, "/sync/cariAdresler", "customerAddresses");
        MapSection(group, "/sync/cariYetkililer", "customerContacts");
        MapSection(group, "/sync/barkodlar", "barcodes");
        MapSection(group, "/sync/satisSartlari", "salesConditions");
        MapPagedSection(group, "/sync/cariHareketleri", "customerTransactions");
        MapPagedSection(group, "/sync/stokHareket", "stockTransactions");
        MapPagedSection(group, "/sync/stokHareketleri", "stockTransactions");
        return routes;
    }

    private static void MapSection(RouteGroupBuilder group, string route, string propertyName) =>
        group.MapPost(route, (HttpContext http, CentralApiDbContext db, CancellationToken ct) =>
                SectionAsync(propertyName, http, db, ct))
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
}
