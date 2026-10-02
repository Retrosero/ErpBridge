using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.PanelEntry;
using ErpBridge.CentralApi.Permissions;
using ErpBridge.CentralApi.Portal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal/entry/*</c> (GOAL_PANEL_GIRIS): the panel enters the documents Sipariş Cepte enters, for an ERP
/// company and a company without one alike. The server builds the phone's own body and writes it the way ingest writes
/// the phone's (<see cref="PanelEntryWriter"/>); the panel itself still never posts to <c>/ingest</c>
/// (<c>PORTAL_CANNOT_SUBMIT_DOCUMENTS</c>). An entry is written straight in — no approval request (K2) — unless the
/// user's own permissions or limits refuse it (K4). Every write is preceded by a preview the form shows; the save sends
/// the total the user saw and is refused when it moved.
/// </summary>
public static class PortalEntryEndpoints
{
    public const int MaxCustomers = 50;
    public const int MaxProducts = 50;

    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");
    private const CompareOptions SearchOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    public static IEndpointRouteBuilder MapPortalEntryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/entry")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            // Previews follow the form as it is typed: they must not use up the company's shared budget.
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/context", ContextAsync).WithName("PortalEntryContext");
        group.MapGet("/customers", CustomersAsync).WithName("PortalEntryCustomers");
        group.MapGet("/products", ProductsAsync).WithName("PortalEntryProducts");
        group.MapPost("/sale/preview", PreviewSaleAsync).WithName("PortalEntrySalePreview");
        group.MapPost("/sale", CreateSaleAsync).WithName("PortalEntrySale");
        return routes;
    }

    // ---- lookups ----------------------------------------------------------------------------

    private static async Task<IResult> ContextAsync(HttpContext http, [FromServices] CentralApiDbContext db,
        [FromServices] IMemoryCache cache, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var (caller, error) = await PanelEntryAccess.AuthorizeAsync(http, db, kind: null, ct);
        if (error is not null) return error;
        var tenantId = caller!.Tenant.Id;
        var view = await views.LoadAsync(db, tenantId, forCustomer: false, ct);
        var response = new PortalEntryContextResponse
        {
            DataSource = caller.Tenant.DataSource,
            Today = PanelEntryDates.Today(DateTimeOffset.UtcNow).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Kinds = [.. PanelEntryAccess.Allowed(caller.Permissions).Select(k => k.Name)],
            CanSellOnAccount = caller.Permissions.Can(PermissionKeys.SaleOpenAccount),
            CanSellBelowStock = caller.Permissions.Can(PermissionKeys.SaleNegativeStock),
            Owners = await PanelEntryAccess.OwnersAsync(db, caller, ct),
            PriceLists = [.. view.PriceLists.Select(l => new PortalEntryPriceListDto { No = l.No, Name = l.Name, IncludesVat = l.IncludesVat })],
        };
        if (!caller.IsNative)
        {
            var lookups = await PanelEntryLookups.AllAsync(db, tenantId, ct, "warehouse", "cash", "bank");
            response.Warehouses = [.. lookups["warehouse"]];
            response.CashAccounts = [.. lookups["cash"]];
            response.Banks = [.. lookups["bank"]];
        }
        return JsonResults.Ok(response);
    }

    private static async Task<IResult> CustomersAsync(HttpContext http, string? q, int? take, [FromServices] CentralApiDbContext db,
        [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var (caller, error) = await AuthorizeAnyAsync(http, db, ct);
        if (error is not null) return error;
        var customers = await PortalLedger.CustomersAsync(db, cache, caller!.Tenant.Id, caller.Tenant.DataSource, ct);
        IEnumerable<PortalLedger.Customer> matches = customers.Values;
        if (q?.Trim() is { Length: > 0 } search)
            matches = matches.Where(c => Contains(c.Code, search) || Contains(c.Title, search) || (c.Phone?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        var list = matches.ToList();
        var showBalance = caller.Permissions.Can(PermissionKeys.ViewCustomerBalance);
        var byText = StringComparer.Create(Turkish, CompareOptions.IgnoreCase);
        return JsonResults.Ok(new PortalEntryCustomersResponse
        {
            Total = list.Count,
            Items = [.. list.OrderBy(c => c.Title, byText).ThenBy(c => c.Code, byText).Take(Math.Clamp(take ?? 20, 1, MaxCustomers))
                .Select(c => new PortalEntryCustomerDto
                {
                    Code = c.Code,
                    Title = c.Title,
                    City = c.City,
                    Phone = c.Phone,
                    Balance = showBalance ? c.Balance : null,
                })],
        });
    }

    private static async Task<IResult> ProductsAsync(HttpContext http, string? q, int? take, [FromServices] CentralApiDbContext db,
        [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var (caller, error) = await AuthorizeAnyAsync(http, db, ct);
        if (error is not null) return error;
        var catalog = await PortalStockCatalog.LoadAsync(db, cache, caller!.Tenant.Id, ct);
        IEnumerable<PortalStockCatalog.Product> matches = catalog.Products;
        if (q?.Trim() is { Length: > 0 } search)
            matches = matches.Where(p => Contains(p.Name, search) || Contains(p.Code, search) || p.Barcodes.Any(b => b.Contains(search, StringComparison.OrdinalIgnoreCase)));
        var list = matches.ToList();
        var byText = StringComparer.Create(Turkish, CompareOptions.IgnoreCase);
        return JsonResults.Ok(new PortalEntryProductsResponse
        {
            Total = list.Count,
            Items = [.. list.OrderBy(p => p.Name, byText).ThenBy(p => p.Code, byText).Take(Math.Clamp(take ?? 20, 1, MaxProducts))
                .Select(p => new PortalEntryProductDto
                {
                    Code = p.Code,
                    Name = p.Name,
                    Unit = p.Unit,
                    Barcode = p.Barcodes.FirstOrDefault(b => !string.IsNullOrWhiteSpace(b)),
                    VatRate = p.VatRate ?? PanelEntrySale.DefaultVatRate,
                    Prices = new Dictionary<int, decimal>(p.Prices),
                    DefaultPriceListNo = PanelEntrySale.HeadlineList(p),
                    Stock = p.TotalQuantity,
                })],
        });
    }

    // ---- sale -------------------------------------------------------------------------------

    private static async Task<IResult> PreviewSaleAsync(HttpContext http, [FromBody] PortalEntrySaleRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var (caller, error) = await PanelEntryAccess.AuthorizeAsync(http, db, PanelEntryKinds.Sale, ct);
        if (error is not null) return error;
        if (body is null) return Invalid(PanelEntryInvalid.Body("Gövde gerekli."));
        var owner = await PanelEntryAccess.OwnerAsync(db, caller!, body.OwnerUserId, ct);
        if (owner is null) return UnknownOwner();
        var (plan, invalid) = await PanelEntrySale.PlanAsync(db, cache, views, caller!, owner, body, PreviewKey(PanelEntryKinds.Sale), DateTimeOffset.UtcNow, ct);
        return invalid is not null ? Invalid(invalid) : JsonResults.Ok(plan!.Preview);
    }

    private static async Task<IResult> CreateSaleAsync(HttpContext http, [FromBody] PortalEntrySaleRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, [FromServices] CatalogViewService views,
        [FromServices] PanelEntryWriter writer, CancellationToken ct)
    {
        var (caller, error) = await PanelEntryAccess.AuthorizeAsync(http, db, PanelEntryKinds.Sale, ct);
        if (error is not null) return error;
        if (body is null) return Invalid(PanelEntryInvalid.Body("Gövde gerekli."));
        if (PanelEntryKinds.ExternalId(PanelEntryKinds.Sale, body.OperationId) is not { } externalId)
            return Invalid(PanelEntryInvalid.Body("operationId gerekli: kayıt başına bir GUID."));
        // A save sent again after its answer was lost is the same document, whatever changed since.
        if (await Jobs.SalesJobWriter.ExistingAsync(db, caller!.Tenant.Id, PanelEntrySale.DocumentType, externalId, ct) is { } existing)
            return Saved([existing], idempotent: true, preview: null);
        var owner = await PanelEntryAccess.OwnerAsync(db, caller, body.OwnerUserId, ct);
        if (owner is null) return UnknownOwner();
        var (plan, invalid) = await PanelEntrySale.PlanAsync(db, cache, views, caller, owner, body, externalId, DateTimeOffset.UtcNow, ct);
        return invalid is not null ? Invalid(invalid) : await SaveAsync(http, db, writer, caller, owner, plan!, body.ExpectedTotal, ct);
    }

    // ---- shared -----------------------------------------------------------------------------

    /// <summary>Writes a planned entry: refused when something stands in its way or its total moved since the preview.</summary>
    private static async Task<IResult> SaveAsync(HttpContext http, CentralApiDbContext db, PanelEntryWriter writer, PanelEntryCaller caller,
        MobileUser owner, PanelEntryPlan plan, decimal? expectedTotal, CancellationToken ct)
    {
        if (plan.Preview.Refusal is { } refusal) return PanelEntryAccess.Error(PanelEntryChecks.StatusOf(refusal.Code), refusal.Code, refusal.Message);
        if (expectedTotal is not { } expected) return Invalid(PanelEntryInvalid.Body("expectedTotal gerekli: önizlemedeki toplam."));
        if (Math.Abs(plan.Preview.Total - expected) > CatalogCustomerOrderEndpoints.PriceTolerance)
            return PanelEntryAccess.Error(StatusCodes.Status409Conflict, "PRICE_CHANGED",
                $"Tutar değişti ({plan.Preview.Total.ToString("#,0.00", Turkish)} TL); güncel tutarı kontrol edip yeniden kaydedin.");

        var written = await writer.WriteAsync(db, caller, owner, plan.Documents, plan.Summary, LogCenter.CorrelationId.Of(http), ct);
        if (written.Refusal is { } refused)
            return PanelEntryAccess.Error(StatusCodes.Status422UnprocessableEntity, "ENTRY_BOOKING_FAILED", refused);
        return Saved(written.Jobs, written.Idempotent, plan.Preview);
    }

    private static IResult Saved(IReadOnlyList<Job> jobs, bool idempotent, PortalEntryPreviewResponse? preview) =>
        JsonResults.Status(idempotent ? StatusCodes.Status200OK : StatusCodes.Status201Created, new PortalEntryCreateResponse
        {
            Idempotent = idempotent,
            Preview = preview,
            Documents = [.. jobs.Select(j => new PortalEntryDocumentDto { JobId = j.Id, ExternalId = j.ExternalId, DocumentType = j.DocumentType, Status = j.Status.ToString() })],
        });

    /// <summary>The key a preview's document is built (and dry-run) under; never written.</summary>
    private static string PreviewKey(PanelEntryKind kind) => kind.Prefix + Guid.Empty.ToString("D");

    /// <summary>The lookups serve every entry page: any entry module will do.</summary>
    private static async Task<(PanelEntryCaller? Caller, IResult? Error)> AuthorizeAnyAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var (caller, error) = await PanelEntryAccess.AuthorizeAsync(http, db, kind: null, ct);
        if (error is not null) return (null, error);
        return PanelEntryAccess.Allowed(caller!.Permissions).Count == 0
            ? (null, PanelEntryAccess.Error(StatusCodes.Status403Forbidden, PanelEntryChecks.ModuleDenied, "Panelden belge girme yetkiniz yok."))
            : (caller, null);
    }

    private static IResult Invalid(PanelEntryInvalid invalid) =>
        PanelEntryAccess.Error(StatusCodes.Status400BadRequest, invalid.Code, invalid.Message);

    private static IResult UnknownOwner() =>
        PanelEntryAccess.Error(StatusCodes.Status400BadRequest, "UNKNOWN_OWNER", "Belgenin kimin adına yazılacağı bulunamadı: firmanın etkin bir saha kullanıcısını seçin.");

    private static bool Contains(string text, string query) => Turkish.CompareInfo.IndexOf(text, query, SearchOptions) >= 0;
}
