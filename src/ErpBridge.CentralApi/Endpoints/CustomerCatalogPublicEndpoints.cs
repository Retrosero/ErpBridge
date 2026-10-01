using System.Globalization;
using System.Security.Claims;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Mobile;
using ErpBridge.CentralApi.Options;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using static ErpBridge.CentralApi.Endpoints.CustomerCatalogManageEndpoints;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/catalog/{code}</c> (docs/GOAL_MUSTERI_KATALOGU.md §5.2): the web catalog of the company with that
/// code, for its customers. <c>info</c> and <c>login</c> are anonymous; everything else needs the customer's session
/// cookie (<see cref="CatalogCookies"/>) and passes <see cref="Program.CatalogCustomerPolicy"/>, which re-checks the
/// account, the company, its module, its published switch and its subscription on every request. Every changing
/// request — sign-in included — must carry <c>X-Katalog: 1</c> and, when the browser sends one, the catalog's own
/// <c>Origin</c> (403 <c>CSRF_REJECTED</c>). Answers are never cached (<c>private, no-store</c>). Prices, discounts,
/// visibility and stock are the server's (<see cref="CatalogCustomerView"/>); the browser sends only keys and
/// quantities.
/// </summary>
public static class CustomerCatalogPublicEndpoints
{
    public const string BasePath = CatalogCookies.ApiPrefix + "/{code:regex(^[A-Za-z0-9]{{4,16}}$)}";
    public const string CsrfHeader = "X-Katalog";
    public const int DefaultPageSize = 48;
    public const int MaxPageSize = 60;
    public const int MinQueryLength = 2;

    public static IEndpointRouteBuilder MapCustomerCatalogPublicEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath)
            .WithTags("CatalogCustomer")
            .AddEndpointFilter(GuardAsync);
        group.MapGet("/info", InfoAsync).WithName("CatalogInfo")
            .AllowAnonymous()
            .RequireRateLimiting(Program.CatalogPublicRateLimitPolicy);
        group.MapPost("/login", LoginAsync).WithName("CatalogLogin")
            .AllowAnonymous()
            .RequireRateLimiting(Program.CatalogLoginRateLimitPolicy);

        var signedIn = group.MapGroup(string.Empty)
            .RequireAuthorization(Program.CatalogCustomerPolicy)
            .RequireRateLimiting(Program.PerCatalogAccountRateLimitPolicy);
        signedIn.MapPost("/logout", Logout).WithName("CatalogLogout");
        signedIn.MapGet("/me", MeAsync).WithName("CatalogMe");
        signedIn.MapPost("/password", ChangePasswordAsync).WithName("CatalogPassword");
        signedIn.MapGet("/categories", CategoriesAsync).WithName("CatalogCategories");
        signedIn.MapGet("/products", ProductsAsync).WithName("CatalogProducts");
        signedIn.MapGet("/products/detail", ProductDetailAsync).WithName("CatalogProductDetail");
        signedIn.MapPost("/cart/quote", QuoteAsync).WithName("CatalogCartQuote");
        return routes;
    }

    /// <summary>
    /// A changing request must come from the catalog page itself: a header no form or image can send, and the
    /// catalog's origin when the browser names one (SameSite=Strict counts every <c>*.appsgo.cloud</c> host as the
    /// same site). Without a configured host (tests, a server that does not serve the catalog) only the header is checked.
    /// </summary>
    private static async ValueTask<object?> GuardAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var request = http.Request;
        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
        {
            var host = http.RequestServices.GetRequiredService<IOptions<CustomerCatalogOptions>>().Value.PublicHost.Trim();
            var originOk = host.Length == 0
                || request.Headers.Origin.Count == 0
                || string.Equals(request.Headers.Origin.ToString(), "https://" + host.ToLowerInvariant(), StringComparison.Ordinal);
            if (request.Headers[CsrfHeader] != "1" || !originOk)
                return Error(StatusCodes.Status403Forbidden, "CSRF_REJECTED", "İstek katalog sayfasından gelmedi.");
        }
        http.Response.Headers.CacheControl = "private, no-store";
        return await next(context);
    }

    // ---- session ---------------------------------------------------------------------------

    private static async Task<IResult> InfoAsync(string code, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var tenant = await CatalogCustomerAccess.OpenCompanyAsync(db, code, ct);
        return tenant is null
            ? Error(StatusCodes.Status404NotFound, "CATALOG_NOT_FOUND", "Katalog bulunamadı.")
            : JsonResults.Ok(new CatalogInfoResponse { CompanyName = tenant.Name, Code = tenant.Code! });
    }

    /// <summary>
    /// Unknown company, unknown name and wrong password are one answer, at the same BCrypt cost. A name that failed
    /// too often waits without its password being checked (<see cref="LoginThrottle"/>), unless the browser carries
    /// the device cookie of that account. Only with the right password are the specific refusals told.
    /// </summary>
    private static async Task<IResult> LoginAsync(
        string code,
        HttpContext http,
        [FromBody] CatalogLoginRequest? body,
        [FromServices] CentralApiDbContext db,
        [FromServices] MobileSeatService seats,
        [FromServices] IJwtIssuer jwt,
        [FromServices] LoginThrottle throttle,
        [FromServices] CatalogLoginGate gate,
        [FromServices] IOptionsMonitor<JwtOptions> jwtOptions,
        [FromServices] CatalogViewService views,
        [FromServices] IMemoryCache cache,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(body?.Username) || string.IsNullOrEmpty(body.Password))
            return Error(StatusCodes.Status400BadRequest, "INVALID_REQUEST", "Kullanıcı adı ve şifre gerekli.");
        var tenantCode = code.ToUpperInvariant();
        if (gate.Enter(tenantCode) is { } busy) return RateLimitedResponse.Result(busy);

        var username = MobileSeatService.NormalizeUsername(body.Username);
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Code == tenantCode, ct);
        var account = tenant is null || username is null
            ? null
            : await db.CatalogAccounts.FirstOrDefaultAsync(a => a.TenantId == tenant.Id && a.Username == username && a.DeletedAtMs == null, ct);

        // A browser this account signed in on before keeps its own count: others' failures do not lock it out.
        var signingKey = jwtOptions.CurrentValue.SigningKey;
        var trusted = account is not null && CatalogCookies.DeviceAccount(http.Request, signingKey) == account.Id;
        var throttleName = trusted ? "#" + account!.Id.ToString("N") : body.Username;
        if (throttle.RetryAfter(LoginThrottle.CatalogArea, tenantCode, throttleName) is { } wait)
            return RateLimitedResponse.Result(wait);

        var passwordOk = await gate.VerifyAsync(body.Password, account?.PasswordHash ?? PasswordHashing.Dummy, ct);
        if (tenant is null || account is null || !passwordOk)
        {
            throttle.RecordFailure(LoginThrottle.CatalogArea, tenantCode, throttleName);
            return Error(StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", "Kullanıcı adı ya da şifre hatalı.");
        }
        throttle.RecordSuccess(LoginThrottle.CatalogArea, tenantCode, throttleName);

        if (!account.IsActive)
            return Error(StatusCodes.Status403Forbidden, CatalogAccountStateHandler.AccountInactive, CatalogAccountStateHandler.MessageOf(CatalogAccountStateHandler.AccountInactive));
        if (await CatalogAccountStateHandler.CompanyDenialAsync(db, seats, tenant, ct) is { } denied)
            return Error(StatusCodes.Status403Forbidden, denied, CatalogAccountStateHandler.MessageOf(denied));

        account.LastLoginAtMs = NowMs();
        await db.SaveChangesAsync(ct);
        CatalogCookies.SetSession(http.Response, tenantCode, jwt.IssueForCatalogAccount(account.Id, tenant.Id, account.TokenVersion, body.Remember));
        CatalogCookies.SetDevice(http.Response, account.Id, signingKey);
        return JsonResults.Ok(new CatalogLoginResponse { Me = await MeOfAsync(db, views, cache, tenant, account, ct) });
    }

    /// <summary>Ends this browser's session only; the device cookie stays.</summary>
    private static IResult Logout(string code, HttpContext http)
    {
        CatalogCookies.DeleteSession(http.Response, code);
        return Results.NoContent();
    }

    private static async Task<IResult> MeAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views,
        [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var session = CatalogSession.Of(http);
        return JsonResults.Ok(await MeOfAsync(db, views, cache, session.Tenant, session.Account, ct));
    }

    /// <summary>
    /// The customer's own password, current one first (a wrong one counts against the name like a failed sign-in).
    /// Every other session of the account ends; this browser gets a new cookie of the same kind.
    /// </summary>
    private static async Task<IResult> ChangePasswordAsync(
        string code,
        HttpContext http,
        [FromBody] CatalogChangePasswordRequest? body,
        [FromServices] CentralApiDbContext db,
        [FromServices] IJwtIssuer jwt,
        [FromServices] LoginThrottle throttle,
        [FromServices] CatalogLoginGate gate,
        [FromServices] IOptions<CustomerCatalogOptions> options,
        CancellationToken ct)
    {
        var session = CatalogSession.Of(http);
        var account = session.Account;
        var tenantCode = session.Tenant.Code!;
        if (string.IsNullOrEmpty(body?.Current) || body.Next is null)
            return Error(StatusCodes.Status400BadRequest, "INVALID_REQUEST", "Mevcut ve yeni şifre gerekli.");
        if (throttle.RetryAfter(LoginThrottle.CatalogArea, tenantCode, account.Username) is { } wait)
            return RateLimitedResponse.Result(wait);
        if (!await gate.VerifyAsync(body.Current, account.PasswordHash, ct))
        {
            throttle.RecordFailure(LoginThrottle.CatalogArea, tenantCode, account.Username);
            return Error(StatusCodes.Status400BadRequest, "INVALID_CREDENTIALS", "Mevcut şifre hatalı.");
        }
        if (CatalogAccounts.PasswordError(body.Next) is { } weak)
            return Error(StatusCodes.Status400BadRequest, "INVALID_PASSWORD", weak);

        var now = NowMs();
        account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.Next);
        account.PasswordChangedAtMs = now;
        account.TokenVersion++;
        account.UpdatedAtMs = now;
        await db.SaveChangesAsync(ct);
        CatalogCookies.SetSession(http.Response, code, jwt.IssueForCatalogAccount(account.Id, session.Tenant.Id, account.TokenVersion, Remembered(http.User, options.Value)));
        return Results.NoContent();
    }

    // ---- browsing --------------------------------------------------------------------------

    private static async Task<IResult> CategoriesAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var customer = await CustomerViewAsync(http, db, views, ct);
        var items = new List<CatalogCustomerCategoryDto>();
        foreach (var category in customer.Catalog.Categories)
        {
            var count = category.Products.Count(customer.Sees);
            if (count > 0) items.Add(new CatalogCustomerCategoryDto { Id = category.Id, Name = category.Key, Count = count });
        }
        return JsonResults.Ok(new CatalogCustomerCategoriesResponse { Items = [.. items] });
    }

    /// <summary>
    /// A page of what the customer sees, in the catalog's order: one category (by its id) or all; with <c>q</c>
    /// (at least 2 characters; a shorter one is ignored) the name, code, brand or a barcode must contain it.
    /// </summary>
    private static async Task<IResult> ProductsAsync(HttpContext http, string? category, string? q, int? page, int? pageSize,
        [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var customer = await CustomerViewAsync(http, db, views, ct);
        var size = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);
        var number = Math.Max(1, page ?? 1);
        var products = string.IsNullOrWhiteSpace(category)
            ? customer.Products
            : customer.Catalog.Categories.FirstOrDefault(c => c.Id == category.Trim().ToLowerInvariant())?.Products.Where(customer.Sees) ?? [];
        var search = q?.Trim();
        if (search is { Length: >= MinQueryLength }) products = products.Where(p => p.Matches(search));
        var matched = products.ToList();
        return JsonResults.Ok(new CatalogCustomerProductsResponse
        {
            Items = [.. matched.Skip((int)Math.Min((long)(number - 1) * size, int.MaxValue)).Take(size).Select(p => ProductOf<CatalogCustomerProductDto>(customer, p))],
            Total = matched.Count,
            Page = number,
            PageSize = size,
        });
    }

    private static async Task<IResult> ProductDetailAsync(HttpContext http, string? key, [FromServices] CentralApiDbContext db,
        [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var customer = await CustomerViewAsync(http, db, views, ct);
        if (customer.Find(key) is not { } product) return Error(StatusCodes.Status404NotFound, "NOT_FOUND", "Ürün bulunamadı.");
        var detail = ProductOf<CatalogCustomerProductDetailDto>(customer, product);
        detail.Images = [.. product.Pictures.Select(p => new CatalogCustomerImageDto { Thumb = p.ThumbUrl, Full = p.FullUrl })];
        return JsonResults.Ok(detail);
    }

    private static async Task<IResult> QuoteAsync(HttpContext http, [FromBody] CatalogQuoteRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] CatalogViewService views, [FromServices] IOptions<CustomerCatalogOptions> options, CancellationToken ct)
    {
        if (body?.Lines is not { } lines) return InvalidBody("lines gerekli.");
        if (lines.Length > options.Value.MaxOrderLines) return InvalidBody($"Sepette en çok {options.Value.MaxOrderLines} satır olabilir.");
        var customer = await CustomerViewAsync(http, db, views, ct);
        return JsonResults.Ok(CatalogQuote.Build(customer, lines.Select(l => (l.Key, l.Quantity))));
    }

    // ---- helpers ---------------------------------------------------------------------------

    private static async Task<CatalogCustomerView> CustomerViewAsync(HttpContext http, CentralApiDbContext db, CatalogViewService views, CancellationToken ct)
    {
        var session = CatalogSession.Of(http);
        return new CatalogCustomerView(await views.LoadAsync(db, session.Tenant.Id, forCustomer: true, ct), session.Account);
    }

    private static T ProductOf<T>(CatalogCustomerView customer, CatalogProduct product) where T : CatalogCustomerProductDto, new() => new()
    {
        Key = product.Code,
        Code = product.Code,
        Name = product.Name,
        Unit = product.Unit,
        Brand = product.Brand,
        CategoryId = CatalogViewService.CategoryId(product.CategoryKey),
        Price = CatalogQuote.Price(customer, product),
        Box = CatalogQuote.Box(product),
        InStock = product.InStock,
        Thumb = product.ThumbUrl,
    };

    private static async Task<CatalogMeDto> MeOfAsync(CentralApiDbContext db, CatalogViewService views, IMemoryCache cache, Tenant tenant, CatalogAccount account, CancellationToken ct)
    {
        var customer = new CatalogCustomerView(await views.LoadAsync(db, tenant.Id, forCustomer: true, ct), account);
        CatalogBalanceDto? balance = null;
        if (account.ShowStatement)
        {
            // The balance the panel and the phone show (the ledger's for an ERP company); only with the statement.
            var customers = await PortalLedger.CustomersAsync(db, cache, tenant.Id, tenant.DataSource, ct);
            if (customers.GetValueOrDefault(account.CustomerCode) is { } card) balance = new CatalogBalanceDto { Amount = card.Balance };
        }
        return new CatalogMeDto
        {
            CompanyName = tenant.Name,
            Code = tenant.Code ?? string.Empty,
            Customer = new CatalogCustomerDto { Code = account.CustomerCode, Name = account.CustomerName },
            Username = account.Username,
            DiscountPercent = account.DiscountPercent,
            PriceList = customer.PriceList is { } list ? new CatalogPriceListDto { No = list.No, Name = list.Name, IncludesVat = list.IncludesVat } : null,
            Features = new CatalogFeaturesDto
            {
                Order = account.CanOrder,
                Statement = account.ShowStatement,
                Invoices = account.ShowInvoices,
                Purchased = account.ShowPurchased,
            },
            Balance = balance,
        };
    }

    /// <summary>Whether the session was a "remember me" one: its token outlives a browser-session token.</summary>
    private static bool Remembered(ClaimsPrincipal user, CustomerCatalogOptions options) =>
        long.TryParse(user.FindFirstValue("exp"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var expires)
        && long.TryParse(user.FindFirstValue("nbf"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var notBefore)
        && TimeSpan.FromSeconds(expires - notBefore) > TimeSpan.FromHours(options.SessionHours) + TimeSpan.FromMinutes(1);
}
