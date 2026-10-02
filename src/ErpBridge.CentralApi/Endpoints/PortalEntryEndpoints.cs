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
using Microsoft.EntityFrameworkCore;
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
        group.MapPost("/collection/preview", PreviewCollectionAsync).WithName("PortalEntryCollectionPreview");
        group.MapPost("/collection", CreateCollectionAsync).WithName("PortalEntryCollection");
        group.MapPost("/disbursement/preview", PreviewDisbursementAsync).WithName("PortalEntryDisbursementPreview");
        group.MapPost("/disbursement", CreateDisbursementAsync).WithName("PortalEntryDisbursement");
        group.MapPost("/expense/preview", PreviewExpenseAsync).WithName("PortalEntryExpensePreview");
        group.MapPost("/expense", CreateExpenseAsync).WithName("PortalEntryExpense");
        group.MapPost("/purchase/preview", PreviewPurchaseAsync).WithName("PortalEntryPurchasePreview");
        group.MapPost("/purchase", CreatePurchaseAsync).WithName("PortalEntryPurchase");
        group.MapGet("/returnables", ReturnablesAsync).WithName("PortalEntryReturnables");
        group.MapPost("/return/preview", PreviewReturnAsync).WithName("PortalEntryReturnPreview");
        group.MapPost("/return", CreateReturnAsync).WithName("PortalEntryReturn");
        group.MapGet("/documents", DocumentsAsync).WithName("PortalEntryDocuments");
        group.MapGet("/documents/{jobId:guid}", DocumentAsync).WithName("PortalEntryDocument");
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
            var lookups = await PanelEntryLookups.AllAsync(db, tenantId, ct, "warehouse", "cash", "bank", "expense_card", "vat_rate");
            response.Warehouses = [.. lookups["warehouse"]];
            response.CashAccounts = [.. lookups["cash"]];
            response.Banks = [.. lookups["bank"]];
            response.ExpenseCards = [.. lookups["expense_card"]];
            response.VatRates = [.. lookups["vat_rate"].Where(r => r.Rate is not null)];
        }
        else
        {
            response.ExpenseCategories = [.. PanelEntryMoney.ExpenseCategories];
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

    private static Task<IResult> PreviewSaleAsync(HttpContext http, [FromBody] PortalEntrySaleRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, [FromServices] CatalogViewService views, CancellationToken ct) =>
        PreviewAsync(http, db, PanelEntryKinds.Sale, body, body?.OwnerUserId,
            (caller, owner, key) => PanelEntrySale.PlanAsync(db, cache, views, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    private static Task<IResult> CreateSaleAsync(HttpContext http, [FromBody] PortalEntrySaleRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, [FromServices] CatalogViewService views,
        [FromServices] PanelEntryWriter writer, CancellationToken ct) =>
        CreateAsync(http, db, writer, PanelEntryKinds.Sale, body, body?.OperationId, body?.OwnerUserId, body?.ExpectedTotal,
            (caller, owner, key) => PanelEntrySale.PlanAsync(db, cache, views, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    // ---- money ------------------------------------------------------------------------------

    private static Task<IResult> PreviewCollectionAsync(HttpContext http, [FromBody] PortalEntryCollectionRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct) =>
        PreviewAsync(http, db, PanelEntryKinds.Collection, body, body?.OwnerUserId,
            (caller, owner, key) => PanelEntryMoney.CollectionAsync(db, cache, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    private static Task<IResult> CreateCollectionAsync(HttpContext http, [FromBody] PortalEntryCollectionRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, [FromServices] PanelEntryWriter writer, CancellationToken ct) =>
        CreateAsync(http, db, writer, PanelEntryKinds.Collection, body, body?.OperationId, body?.OwnerUserId, body?.ExpectedTotal,
            (caller, owner, key) => PanelEntryMoney.CollectionAsync(db, cache, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    private static Task<IResult> PreviewDisbursementAsync(HttpContext http, [FromBody] PortalEntryDisbursementRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct) =>
        PreviewAsync(http, db, PanelEntryKinds.Disbursement, body, body?.OwnerUserId,
            (caller, owner, key) => PanelEntryMoney.DisbursementAsync(db, cache, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    private static Task<IResult> CreateDisbursementAsync(HttpContext http, [FromBody] PortalEntryDisbursementRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, [FromServices] PanelEntryWriter writer, CancellationToken ct) =>
        CreateAsync(http, db, writer, PanelEntryKinds.Disbursement, body, body?.OperationId, body?.OwnerUserId, body?.ExpectedTotal,
            (caller, owner, key) => PanelEntryMoney.DisbursementAsync(db, cache, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    private static Task<IResult> PreviewExpenseAsync(HttpContext http, [FromBody] PortalEntryExpenseRequest? body,
        [FromServices] CentralApiDbContext db, CancellationToken ct) =>
        PreviewAsync(http, db, PanelEntryKinds.Expense, body, body?.OwnerUserId,
            (caller, owner, key) => PanelEntryMoney.ExpenseAsync(db, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    private static Task<IResult> CreateExpenseAsync(HttpContext http, [FromBody] PortalEntryExpenseRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] PanelEntryWriter writer, CancellationToken ct) =>
        CreateAsync(http, db, writer, PanelEntryKinds.Expense, body, body?.OperationId, body?.OwnerUserId, body?.ExpectedTotal,
            (caller, owner, key) => PanelEntryMoney.ExpenseAsync(db, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    // ---- purchase and return ----------------------------------------------------------------

    private static Task<IResult> PreviewPurchaseAsync(HttpContext http, [FromBody] PortalEntryPurchaseRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct) =>
        PreviewAsync(http, db, PanelEntryKinds.Purchase, body, body?.OwnerUserId,
            (caller, owner, key) => PanelEntryLines.PurchaseAsync(db, cache, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    private static Task<IResult> CreatePurchaseAsync(HttpContext http, [FromBody] PortalEntryPurchaseRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, [FromServices] PanelEntryWriter writer, CancellationToken ct) =>
        CreateAsync(http, db, writer, PanelEntryKinds.Purchase, body, body?.OperationId, body?.OwnerUserId, body?.ExpectedTotal,
            (caller, owner, key) => PanelEntryLines.PurchaseAsync(db, cache, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    private static async Task<IResult> ReturnablesAsync(HttpContext http, string? customerCode, [FromServices] CentralApiDbContext db,
        [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var (caller, error) = await PanelEntryAccess.AuthorizeAsync(http, db, PanelEntryKinds.Return, ct);
        if (error is not null) return error;
        var (customer, unknown) = await PanelEntryMoney.CustomerAsync(db, cache, caller!, customerCode, ct);
        if (unknown is not null) return Invalid(unknown);
        return JsonResults.Ok(new PortalEntryReturnablesResponse { Items = await PanelEntryLines.ReturnablesAsync(db, cache, caller!.Tenant.Id, customer!.Code, ct) });
    }

    private static Task<IResult> PreviewReturnAsync(HttpContext http, [FromBody] PortalEntryReturnRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, [FromServices] CatalogViewService views, CancellationToken ct) =>
        PreviewAsync(http, db, PanelEntryKinds.Return, body, body?.OwnerUserId,
            (caller, owner, key) => PanelEntryLines.ReturnAsync(db, cache, views, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    private static Task<IResult> CreateReturnAsync(HttpContext http, [FromBody] PortalEntryReturnRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, [FromServices] CatalogViewService views,
        [FromServices] PanelEntryWriter writer, CancellationToken ct) =>
        CreateAsync(http, db, writer, PanelEntryKinds.Return, body, body?.OperationId, body?.OwnerUserId, body?.ExpectedTotal,
            (caller, owner, key) => PanelEntryLines.ReturnAsync(db, cache, views, caller, owner, body!, key, DateTimeOffset.UtcNow, ct), ct);

    // ---- after the save ---------------------------------------------------------------------

    /// <summary>The panel entries, newest first: the user's own, or everyone's for an administrator who asks (<c>all=true</c>).</summary>
    private static async Task<IResult> DocumentsAsync(HttpContext http, bool? all, int? page, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (caller, error) = await AuthorizeAnyAsync(http, db, ct);
        if (error is not null) return error;
        var everyone = all == true && RolePermissions.IsAdmin(caller!.User);
        return JsonResults.Ok(await PanelEntryDocuments.ListAsync(db, caller!, everyone, page ?? 1, ct));
    }

    private static async Task<IResult> DocumentAsync(HttpContext http, Guid jobId, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (caller, error) = await PanelEntryAccess.AuthorizeAsync(http, db, kind: null, ct);
        if (error is not null) return error;
        return await PanelEntryDocuments.DetailAsync(db, caller!, jobId, ct) is { } detail
            ? JsonResults.Ok(detail)
            : PanelEntryAccess.Error(StatusCodes.Status404NotFound, "ENTRY_NOT_FOUND", "Belge bulunamadı.");
    }

    // ---- shared -----------------------------------------------------------------------------

    private delegate Task<(PanelEntryPlan? Plan, PanelEntryInvalid? Invalid)> Planner(PanelEntryCaller caller, MobileUser owner, string key);

    private static async Task<IResult> PreviewAsync(HttpContext http, CentralApiDbContext db, PanelEntryKind kind, object? body, Guid? ownerUserId,
        Planner plan, CancellationToken ct)
    {
        var (caller, error) = await PanelEntryAccess.AuthorizeAsync(http, db, kind, ct);
        if (error is not null) return error;
        if (body is null) return Invalid(PanelEntryInvalid.Body("Gövde gerekli."));
        var owner = await PanelEntryAccess.OwnerAsync(db, caller!, ownerUserId, ct);
        if (owner is null) return UnknownOwner();
        var (planned, invalid) = await plan(caller!, owner, PreviewKey(kind));
        return invalid is not null ? Invalid(invalid) : JsonResults.Ok(planned!.Preview);
    }

    private static async Task<IResult> CreateAsync(HttpContext http, CentralApiDbContext db, PanelEntryWriter writer, PanelEntryKind kind,
        object? body, string? operationId, Guid? ownerUserId, decimal? expectedTotal, Planner plan, CancellationToken ct)
    {
        var (caller, error) = await PanelEntryAccess.AuthorizeAsync(http, db, kind, ct);
        if (error is not null) return error;
        if (body is null) return Invalid(PanelEntryInvalid.Body("Gövde gerekli."));
        if (PanelEntryKinds.ExternalId(kind, operationId) is not { } key) return MissingOperation();
        if (await ExistingAsync(db, caller!, key, ct) is { } existing) return existing;
        var owner = await PanelEntryAccess.OwnerAsync(db, caller!, ownerUserId, ct);
        if (owner is null) return UnknownOwner();
        var (planned, invalid) = await plan(caller!, owner, key);
        return invalid is not null ? Invalid(invalid) : await SaveAsync(http, db, writer, caller!, owner, planned!, expectedTotal, ct);
    }

    /// <summary>
    /// The jobs an earlier save of the same operation wrote (its key, or its <c>-n</c> parts): a save sent again after its
    /// answer was lost is the same entry, whatever changed since.
    /// </summary>
    private static async Task<IResult?> ExistingAsync(CentralApiDbContext db, PanelEntryCaller caller, string key, CancellationToken ct)
    {
        var parts = key + "-";
        var jobs = await db.Jobs.AsNoTracking()
            .Where(j => j.TenantId == caller.Tenant.Id && (j.ExternalId == key || j.ExternalId.StartsWith(parts)))
            .OrderBy(j => j.ExternalId)
            .ToListAsync(ct);
        return jobs.Count == 0 ? null : Saved(jobs, idempotent: true, preview: null);
    }

    private static IResult MissingOperation() => Invalid(PanelEntryInvalid.Body("operationId gerekli: kayıt başına bir GUID."));

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
