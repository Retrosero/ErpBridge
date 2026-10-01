using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Mobile;
using ErpBridge.CentralApi.Portal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/customer-catalog</c> (docs/GOAL_MUSTERI_KATALOGU.md §5.1): the company's web catalog — settings,
/// category and product layout, customer accounts — managed from the phone and the panel alike. Every call checks the
/// operator's module first (403 <c>MODULE_NOT_ENABLED</c>, the same answer for everyone), then the locked
/// <c>action.customer_catalog.manage</c> permission (403 <c>CATALOG_MANAGE_REQUIRED</c>). Layout writes carry the
/// revision they read and are refused with 409 <c>CATALOG_CHANGED</c> when someone wrote since. Rate limited per user.
/// Pictures: <see cref="CustomerCatalogImageEndpoints"/>.
/// </summary>
public static class CustomerCatalogManageEndpoints
{
    public const string BasePath = "/api/v1/customer-catalog";

    /// <summary>The share address when <c>CustomerCatalog:PublicBaseUrl</c> is not set (K7).</summary>
    public const string DefaultPublicBaseUrl = "https://katalog.appsgo.cloud";

    public const int MaxCategoryProducts = 5000;
    public const int MaxSearchResults = 50;
    public const int MaxLayoutEdits = 5000;
    public const int MaxCartonQuantity = 100_000;
    public const int AccountPageSize = 50;

    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private static readonly CompareInfo Turkish = CultureInfo.GetCultureInfo("tr-TR").CompareInfo;
    private static readonly StringComparer TurkishText = StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), CompareOptions.IgnoreCase);

    public static IEndpointRouteBuilder MapCustomerCatalogManageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath)
            .WithTags("CustomerCatalog")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/settings", GetSettingsAsync).WithName("CustomerCatalogSettingsGet");
        group.MapPut("/settings", PutSettingsAsync).WithName("CustomerCatalogSettingsPut");
        group.MapGet("/categories", GetCategoriesAsync).WithName("CustomerCatalogCategoriesGet");
        group.MapPut("/categories", PutCategoriesAsync).WithName("CustomerCatalogCategoriesPut");
        group.MapGet("/products", GetProductsAsync).WithName("CustomerCatalogProductsGet");
        group.MapPut("/products", PutProductsAsync).WithName("CustomerCatalogProductsPut");
        group.MapGet("/accounts", ListAccountsAsync).WithName("CustomerCatalogAccountsList");
        group.MapGet("/accounts/by-customer", AccountByCustomerAsync).WithName("CustomerCatalogAccountByCustomer");
        group.MapPost("/accounts", CreateAccountAsync).WithName("CustomerCatalogAccountCreate");
        group.MapPatch("/accounts/{id:guid}", UpdateAccountAsync).WithName("CustomerCatalogAccountUpdate");
        group.MapPut("/accounts/{id:guid}/password", SetPasswordAsync).WithName("CustomerCatalogAccountPassword");
        group.MapPost("/accounts/{id:guid}/revoke-sessions", RevokeSessionsAsync).WithName("CustomerCatalogAccountRevokeSessions");
        group.MapDelete("/accounts/{id:guid}", DeleteAccountAsync).WithName("CustomerCatalogAccountDelete");
        return routes;
    }

    /// <summary>
    /// The user, re-checked against current state (<see cref="MobileAccountEndpoints.AuthorizeAsync"/>); then the module,
    /// so a company without it gets the same answer for everyone; then, for management, the permission.
    /// </summary>
    internal static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeAsync(
        HttpContext http, CentralApiDbContext db, bool manage, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access;
        if (!await db.TenantModules.AsNoTracking().AnyAsync(m => m.TenantId == access.Tenant!.Id && m.ModuleKey == TenantModules.CustomerCatalog, ct))
            return (null, null, Error(StatusCodes.Status403Forbidden, "MODULE_NOT_ENABLED", "Müşteri kataloğu modülü bu firmada açık değil."));
        if (manage && !RolePermissions.CanManageCustomerCatalog(access.User!))
            return (null, null, Error(StatusCodes.Status403Forbidden, "CATALOG_MANAGE_REQUIRED", "Müşteri kataloğunu yalnız yetkili yöneticiler yönetebilir."));
        return access;
    }

    // ---- settings ------------------------------------------------------------------------

    private static async Task<IResult> GetSettingsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views,
        [FromServices] MobileSeatService seats, [FromServices] IOptions<CustomerCatalogOptions> options, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await SettingsAsync(db, views, seats, options.Value, access.Tenant!.Id, ct));
    }

    private static async Task<IResult> PutSettingsAsync(HttpContext http, [FromBody] CatalogSettingsRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] CatalogViewService views, [FromServices] MobileSeatService seats, [FromServices] IOptions<CustomerCatalogOptions> options, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        if (body is null) return InvalidBody("Gövde gerekli.");
        var tenantId = access.Tenant!.Id;
        var view = await views.LoadAsync(db, tenantId, forCustomer: false, ct);
        if (body.DefaultPriceListNo is { } listNo && view.PriceList(listNo) is null) return UnknownPriceList();

        var revision = await WriteLayoutAsync(db, tenantId, access.User!.Id, body.Revision, async _ =>
        {
            var settings = await db.CatalogSettings.FirstAsync(s => s.TenantId == tenantId, ct);
            settings.IsEnabled = body.IsEnabled;
            settings.DefaultPriceListNo = body.DefaultPriceListNo;
        }, ct);
        if (revision is null) return Changed();
        return JsonResults.Ok(await SettingsAsync(db, views, seats, options.Value, tenantId, ct));
    }

    private static async Task<CatalogSettingsDto> SettingsAsync(
        CentralApiDbContext db, CatalogViewService views, MobileSeatService seats, CustomerCatalogOptions options, Guid tenantId, CancellationToken ct)
    {
        // The catalog's address is the company code; a company that never had seats gets one now (§3).
        var code = await seats.EnsureTenantCodeAsync(tenantId, ct) ?? string.Empty;
        var view = await views.LoadAsync(db, tenantId, forCustomer: false, ct);
        var usedBytes = await db.CatalogImages.Where(i => i.TenantId == tenantId).SumAsync(i => (long)i.SizeBytes, ct);
        var accounts = await db.CatalogAccounts.CountAsync(a => a.TenantId == tenantId && a.DeletedAtMs == null, ct);
        var openOrders = await db.CatalogOrders.CountAsync(
            o => o.TenantId == tenantId && (o.Status == CatalogOrderStatuses.New || o.Status == CatalogOrderStatuses.Claimed), ct);
        var baseUrl = string.IsNullOrWhiteSpace(options.PublicBaseUrl) ? DefaultPublicBaseUrl : options.PublicBaseUrl.Trim().TrimEnd('/');
        return new CatalogSettingsDto
        {
            IsEnabled = view.IsEnabled,
            DefaultPriceListNo = view.DefaultPriceListNo,
            EffectiveDefaultPriceListNo = view.EffectiveDefaultPriceListNo,
            Revision = view.Revision,
            TenantCode = code,
            PublicUrl = code.Length == 0 ? string.Empty : baseUrl + "/" + code,
            PriceLists = [.. view.PriceLists.Select(l => new CatalogPriceListDto { No = l.No, Name = l.Name, IncludesVat = l.IncludesVat })],
            ImageQuota = new CatalogImageQuotaDto { UsedBytes = usedBytes, LimitBytes = options.TenantImageQuotaBytes },
            Counts = new CatalogCountsDto
            {
                Categories = view.Categories.Count,
                Products = view.Products.Count,
                VisibleProducts = view.Products.Values.Count(view.InMainCatalog),
                Accounts = accounts,
                OpenOrders = openOrders,
            },
        };
    }

    // ---- layout --------------------------------------------------------------------------

    private static async Task<IResult> GetCategoriesAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var view = await views.LoadAsync(db, access.Tenant!.Id, forCustomer: false, ct);
        return JsonResults.Ok(new CatalogCategoriesResponse
        {
            Revision = view.Revision,
            Items = [.. view.Categories.Select(c => new CatalogCategoryDto
            {
                Key = c.Key,
                Name = c.Key,
                SortOrder = c.SortOrder,
                Hidden = c.Hidden,
                ProductCount = c.Products.Count,
                HiddenCount = c.Products.Count(p => p.Hidden),
                NoDiscountCount = c.Products.Count(p => p.NoDiscount),
                CartonOnlyCount = c.Products.Count(p => p.CartonOnly),
            })],
        });
    }

    /// <summary>The whole category list in its new order; a category left out loses its place but keeps its hidden flag.</summary>
    private static async Task<IResult> PutCategoriesAsync(HttpContext http, [FromBody] CatalogCategoriesRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        if (body?.Items is not { } items) return InvalidBody("items gerekli.");
        if (items.Length > MaxLayoutEdits) return InvalidBody($"Bir seferde en çok {MaxLayoutEdits} kategori gönderilebilir.");
        var keys = new List<(string Key, bool Hidden)>(items.Length);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            var key = item.Key?.Trim();
            if (string.IsNullOrEmpty(key) || key.Length > CatalogVisibility.MaxCategoryKeyLength || !seen.Add(key))
                return InvalidBody($"Her kategori anahtarı dolu, en çok {CatalogVisibility.MaxCategoryKeyLength} karakter ve bir kez olmalı.");
            keys.Add((key, item.Hidden));
        }

        var tenantId = access.Tenant!.Id;
        var revision = await WriteLayoutAsync(db, tenantId, access.User!.Id, body.Revision, async now =>
        {
            var rows = (await db.CatalogCategorySettings.Where(s => s.TenantId == tenantId).ToListAsync(ct))
                .ToDictionary(r => r.CategoryKey, StringComparer.Ordinal);
            for (var i = 0; i < keys.Count; i++)
            {
                if (!rows.Remove(keys[i].Key, out var row))
                {
                    row = new CatalogCategorySetting { TenantId = tenantId, CategoryKey = keys[i].Key };
                    db.CatalogCategorySettings.Add(row);
                }
                row.SortOrder = i;
                row.IsHidden = keys[i].Hidden;
                row.UpdatedAtMs = now;
            }
            foreach (var unlisted in rows.Values)
            {
                if (!unlisted.IsHidden) db.CatalogCategorySettings.Remove(unlisted);
                else if (unlisted.SortOrder is not null)
                {
                    unlisted.SortOrder = null;
                    unlisted.UpdatedAtMs = now;
                }
            }
        }, ct);
        return revision is { } value ? JsonResults.Ok(new CatalogRevisionDto { Revision = value }) : Changed();
    }

    /// <summary>A whole category (at most 5000), or with <paramref name="q"/> up to 50 matches; both together search the category.</summary>
    private static async Task<IResult> GetProductsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views,
        string? category, string? q, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var view = await views.LoadAsync(db, access.Tenant!.Id, forCustomer: false, ct);
        IEnumerable<CatalogProduct> products = string.IsNullOrWhiteSpace(category)
            ? view.Categories.SelectMany(c => c.Products)
            : view.Category(category.Trim())?.Products ?? [];
        var search = q?.Trim();
        var limit = MaxCategoryProducts;
        if (!string.IsNullOrEmpty(search))
        {
            products = products.Where(p => p.Matches(search));
            limit = MaxSearchResults;
        }
        var page = products.Take(limit + 1).ToList();
        return JsonResults.Ok(new CatalogProductsResponse
        {
            Revision = view.Revision,
            Truncated = page.Count > limit,
            Items = [.. page.Take(limit).Select(p => new CatalogProductDto
            {
                StockCode = p.Code,
                Name = p.Name,
                Unit = p.Unit,
                Brand = p.Brand,
                CategoryKey = p.CategoryKey,
                SortOrder = p.SortOrder,
                Hidden = p.Hidden,
                NoDiscount = p.NoDiscount,
                CartonOnly = p.CartonOnly,
                CartonQuantity = p.CartonQuantity,
                ErpCartonQuantity = p.ErpCartonQuantity,
                ListPrice = p.PriceIn(view.EffectiveDefaultPriceListNo),
                InStock = p.InStock,
                ImageCount = p.Pictures.Count,
                ThumbUrl = p.ThumbUrl,
            })],
        });
    }

    /// <summary>The products given, each with all its settings; a product back at every default loses its row.</summary>
    private static async Task<IResult> PutProductsAsync(HttpContext http, [FromBody] CatalogProductsRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        if (body?.Items is not { } items) return InvalidBody("items gerekli.");
        if (items.Length > MaxLayoutEdits) return InvalidBody($"Bir seferde en çok {MaxLayoutEdits} ürün gönderilebilir.");
        var tenantId = access.Tenant!.Id;
        var view = await views.LoadAsync(db, tenantId, forCustomer: false, ct);
        var edits = new List<(string Code, CatalogProductEdit Edit)>(items.Length);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            var code = item.StockCode?.Trim();
            if (string.IsNullOrEmpty(code) || code.Length > CatalogVisibility.MaxStockCodeLength || !seen.Add(code))
                return InvalidBody($"Her stok kodu dolu, en çok {CatalogVisibility.MaxStockCodeLength} karakter ve bir kez olmalı.");
            if (item.CartonQuantity is { } carton && carton is < 2 or > MaxCartonQuantity)
                return Error(StatusCodes.Status400BadRequest, "INVALID_CARTON_QUANTITY", $"Koli adedi 2 ile {MaxCartonQuantity} arasında olmalı ({code}).");
            var product = view.Products.GetValueOrDefault(code);
            if (item.CartonOnly && (item.CartonQuantity ?? product?.ErpCartonQuantity) is not >= 2)
                return Error(StatusCodes.Status400BadRequest, "CARTON_QUANTITY_REQUIRED", $"Yalnız koli satışı için ürünün koli adedi (en az 2) olmalı ({code}).");
            edits.Add((product?.Code ?? code, item));
        }

        var revision = await WriteLayoutAsync(db, tenantId, access.User!.Id, body.Revision, async now =>
        {
            var rows = (await db.CatalogProductSettings.Where(s => s.TenantId == tenantId).ToListAsync(ct))
                .ToDictionary(r => r.StockCode, StringComparer.OrdinalIgnoreCase);
            foreach (var (code, edit) in edits)
            {
                var isDefault = edit.SortOrder is null && !edit.Hidden && !edit.NoDiscount && !edit.CartonOnly && edit.CartonQuantity is null;
                rows.TryGetValue(code, out var row);
                if (isDefault)
                {
                    if (row is not null) db.CatalogProductSettings.Remove(row);
                    continue;
                }
                if (row is null)
                {
                    row = new CatalogProductSetting { TenantId = tenantId, StockCode = code };
                    db.CatalogProductSettings.Add(row);
                }
                row.SortOrder = edit.SortOrder;
                row.IsHidden = edit.Hidden;
                row.NoDiscount = edit.NoDiscount;
                row.CartonOnly = edit.CartonOnly;
                row.CartonQuantity = edit.CartonQuantity;
                row.UpdatedAtMs = now;
            }
        }, ct);
        return revision is { } value ? JsonResults.Ok(new CatalogRevisionDto { Revision = value }) : Changed();
    }

    /// <summary>
    /// One layout write under the revision lock (§3 <c>Revision</c>): in one transaction the settings row's revision moves
    /// by one — only from <paramref name="expected"/> when given — and <paramref name="apply"/> runs. Null when
    /// <paramref name="expected"/> is stale; nothing is written then. Also used by picture writes, unconditionally:
    /// the catalog view keys on the revision.
    /// </summary>
    internal static async Task<long?> WriteLayoutAsync(
        CentralApiDbContext db, Guid tenantId, Guid userId, long? expected, Func<long, Task> apply, CancellationToken ct)
    {
        await EnsureSettingsAsync(db, tenantId, ct);
        var now = NowMs();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var settings = db.CatalogSettings.Where(s => s.TenantId == tenantId);
        if (expected is { } known) settings = settings.Where(s => s.Revision == known);
        // Also the row lock: a second writer waits here until this one commits, then sees the new revision.
        var moved = await settings.ExecuteUpdateAsync(s => s
            .SetProperty(x => x.Revision, x => x.Revision + 1)
            .SetProperty(x => x.UpdatedAtMs, now)
            .SetProperty(x => x.UpdatedByUserId, userId), ct);
        if (moved == 0) return null;
        await apply(now);
        await db.SaveChangesAsync(ct);
        var revision = await db.CatalogSettings.AsNoTracking().Where(s => s.TenantId == tenantId).Select(s => s.Revision).FirstAsync(ct);
        await transaction.CommitAsync(ct);
        return revision;
    }

    /// <summary>The settings row a layout write moves; made on first use (no row = not published, revision 0).</summary>
    private static async Task EnsureSettingsAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        if (await db.CatalogSettings.AnyAsync(s => s.TenantId == tenantId, ct)) return;
        var row = new CatalogSettings { TenantId = tenantId, UpdatedAtMs = NowMs() };
        db.CatalogSettings.Add(row);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // A concurrent first write made the row; that is all this wanted.
        }
        finally
        {
            db.Entry(row).State = EntityState.Detached;
        }
    }

    // ---- accounts ------------------------------------------------------------------------

    private static async Task<IResult> ListAccountsAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? q, int? page, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        // A company has at most one live account per customer; the list is read whole and searched in Turkish.
        IEnumerable<CatalogAccount> accounts = await db.CatalogAccounts.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.DeletedAtMs == null).ToListAsync(ct);
        if (q?.Trim() is { Length: > 0 } search)
            accounts = accounts.Where(a => Contains(a.CustomerCode, search) || Contains(a.CustomerName, search) || Contains(a.Username, search));
        var sorted = accounts.OrderBy(a => a.CustomerName, TurkishText).ThenBy(a => a.CustomerCode, TurkishText).ToList();
        var pageNo = Math.Max(1, page ?? 1);
        var items = sorted.Skip((pageNo - 1) * AccountPageSize).Take(AccountPageSize).ToList();
        var open = await OpenOrderCountsAsync(db, tenantId, [.. items.Select(a => a.Id)], ct);
        return JsonResults.Ok(new CatalogAccountListResponse
        {
            Total = sorted.Count,
            Items = [.. items.Select(a => new CatalogAccountSummaryDto
            {
                Id = a.Id,
                CustomerCode = a.CustomerCode,
                CustomerName = a.CustomerName,
                Username = a.Username,
                IsActive = a.IsActive,
                DiscountPercent = a.DiscountPercent,
                PriceListNo = a.PriceListNo,
                LastLoginAtMs = a.LastLoginAtMs,
                OpenOrderCount = open.GetValueOrDefault(a.Id),
            })],
        });
    }

    private static async Task<IResult> AccountByCustomerAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache,
        string? code, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        var customer = await CustomerAsync(db, cache, tenantId, code, ct);
        if (customer is null) return CustomerNotFound();
        var account = await db.CatalogAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.CustomerCode == customer.Code && a.DeletedAtMs == null, ct);
        var taken = (await db.CatalogAccounts.AsNoTracking().Where(a => a.TenantId == tenantId && a.DeletedAtMs == null)
            .Select(a => a.Username).ToListAsync(ct)).ToHashSet(StringComparer.Ordinal);
        var suggestion = CatalogAccounts.UsernameFrom(customer.Title) ?? CatalogAccounts.UsernameFrom(customer.Code) ?? "musteri";
        return JsonResults.Ok(new CatalogAccountByCustomerResponse
        {
            Account = account is null ? null : await ToDtoAsync(db, account, ct),
            CustomerName = customer.Title,
            SuggestedUsername = CatalogAccounts.FirstFree(suggestion, taken),
        });
    }

    private static async Task<IResult> CreateAccountAsync(HttpContext http, [FromBody] CatalogAccountCreateRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] IMemoryCache cache, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        if (body is null) return InvalidBody("Gövde gerekli.");
        var tenantId = access.Tenant!.Id;
        var customer = await CustomerAsync(db, cache, tenantId, body.CustomerCode, ct);
        if (customer is null) return CustomerNotFound();
        if (MobileSeatService.NormalizeUsername(body.Username) is not { } username) return InvalidUsername();
        var issued = string.IsNullOrEmpty(body.Password) ? CatalogAccounts.GeneratePassword() : null;
        var password = issued ?? body.Password!;
        if (CatalogAccounts.PasswordError(password) is { } passwordError) return Error(StatusCodes.Status400BadRequest, "INVALID_PASSWORD", passwordError);
        if (CatalogAccounts.Discount(body.DiscountPercent) is not { } discount) return InvalidDiscount();
        if (body.PriceListNo is { } listNo && (await views.LoadAsync(db, tenantId, forCustomer: false, ct)).PriceList(listNo) is null) return UnknownPriceList();
        if (VisibilityOf(body.Visibility, out var visibilityError) is not { } visibility) return InvalidVisibility(visibilityError!);
        if (body.ResponsibleUserId is { } responsible && !await IsStaffAsync(db, tenantId, responsible, ct)) return InvalidResponsible();
        if (await UsernameTakenAsync(db, tenantId, username, null, ct)) return UsernameTaken();
        if (await db.CatalogAccounts.AnyAsync(a => a.TenantId == tenantId && a.CustomerCode == customer.Code && a.DeletedAtMs == null, ct)) return AccountExists();

        var now = NowMs();
        var user = access.User!;
        var account = new CatalogAccount
        {
            TenantId = tenantId,
            CustomerCode = customer.Code,
            CustomerName = Cut(customer.Title, 200),
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = body.IsActive,
            DiscountPercent = discount,
            PriceListNo = body.PriceListNo,
            VisibilityJson = visibility.ToJson(),
            ShowStatement = body.ShowStatement,
            ShowInvoices = body.ShowInvoices,
            ShowPurchased = body.ShowPurchased,
            CanOrder = body.CanOrder,
            ResponsibleUserId = body.ResponsibleUserId,
            PasswordChangedAtMs = now,
            CreatedAtMs = now,
            UpdatedAtMs = now,
            CreatedByUserId = user.Id,
            CreatedByName = Cut(NameOf(user), 120),
            UpdatedByUserId = user.Id,
        };
        db.CatalogAccounts.Add(account);
        if (await SaveAccountAsync(db, account, ct) is { } conflict) return conflict;
        return JsonResults.Status(StatusCodes.Status201Created, new CatalogAccountSavedResponse { Account = await ToDtoAsync(db, account, ct), IssuedPassword = issued });
    }

    /// <summary>Only the fields sent change; <c>priceListNo</c> and <c>responsibleUserId</c> sent as null go back to none.</summary>
    private static async Task<IResult> UpdateAccountAsync(Guid id, HttpContext http, [FromBody] JsonElement? body, [FromServices] CentralApiDbContext db,
        [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        if (body is not { ValueKind: JsonValueKind.Object } json) return InvalidBody("Gövde bir JSON nesnesi olmalı.");
        CatalogAccountPatchRequest patch;
        try
        {
            patch = json.Deserialize<CatalogAccountPatchRequest>(Web) ?? new CatalogAccountPatchRequest();
        }
        catch (JsonException)
        {
            return InvalidBody("Gövde okunamadı.");
        }
        var tenantId = access.Tenant!.Id;
        var account = await LiveAccountAsync(db, tenantId, id, ct);
        if (account is null) return AccountNotFound();

        if (Has(json, "username"))
        {
            if (MobileSeatService.NormalizeUsername(patch.Username) is not { } username) return InvalidUsername();
            if (username != account.Username && await UsernameTakenAsync(db, tenantId, username, account.Id, ct)) return UsernameTaken();
            account.Username = username;
        }
        if (patch.DiscountPercent is { } requested)
        {
            if (CatalogAccounts.Discount(requested) is not { } discount) return InvalidDiscount();
            account.DiscountPercent = discount;
        }
        if (Has(json, "priceListNo"))
        {
            if (patch.PriceListNo is { } listNo && (await views.LoadAsync(db, tenantId, forCustomer: false, ct)).PriceList(listNo) is null) return UnknownPriceList();
            account.PriceListNo = patch.PriceListNo;
        }
        if (Has(json, "visibility"))
        {
            if (VisibilityOf(patch.Visibility, out var visibilityError) is not { } visibility) return InvalidVisibility(visibilityError!);
            account.VisibilityJson = visibility.ToJson();
        }
        if (Has(json, "responsibleUserId"))
        {
            if (patch.ResponsibleUserId is { } responsible && !await IsStaffAsync(db, tenantId, responsible, ct)) return InvalidResponsible();
            account.ResponsibleUserId = patch.ResponsibleUserId;
        }
        if (patch.IsActive is { } active)
        {
            // A deactivated account's open sessions end now, not when their token runs out.
            if (account.IsActive && !active) account.TokenVersion++;
            account.IsActive = active;
        }
        account.ShowStatement = patch.ShowStatement ?? account.ShowStatement;
        account.ShowInvoices = patch.ShowInvoices ?? account.ShowInvoices;
        account.ShowPurchased = patch.ShowPurchased ?? account.ShowPurchased;
        account.CanOrder = patch.CanOrder ?? account.CanOrder;
        account.UpdatedAtMs = NowMs();
        account.UpdatedByUserId = access.User!.Id;
        if (await SaveAccountAsync(db, account, ct) is { } conflict) return conflict;
        return JsonResults.Ok(new CatalogAccountSavedResponse { Account = await ToDtoAsync(db, account, ct) });
    }

    /// <summary>A new password (made here when none is sent, returned once); every session of the account ends.</summary>
    private static async Task<IResult> SetPasswordAsync(Guid id, HttpContext http, [FromBody] CatalogPasswordRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var account = await LiveAccountAsync(db, access.Tenant!.Id, id, ct);
        if (account is null) return AccountNotFound();
        var issued = string.IsNullOrEmpty(body?.Password) ? CatalogAccounts.GeneratePassword() : null;
        var password = issued ?? body!.Password!;
        if (CatalogAccounts.PasswordError(password) is { } passwordError) return Error(StatusCodes.Status400BadRequest, "INVALID_PASSWORD", passwordError);
        var now = NowMs();
        account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        account.PasswordChangedAtMs = now;
        account.TokenVersion++;
        account.UpdatedAtMs = now;
        account.UpdatedByUserId = access.User!.Id;
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(new CatalogPasswordResponse { IssuedPassword = issued });
    }

    private static async Task<IResult> RevokeSessionsAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var account = await LiveAccountAsync(db, access.Tenant!.Id, id, ct);
        if (account is null) return AccountNotFound();
        account.TokenVersion++;
        account.UpdatedAtMs = NowMs();
        account.UpdatedByUserId = access.User!.Id;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    /// <summary>Soft: the row stays for its order requests; its username and customer are free again.</summary>
    private static async Task<IResult> DeleteAccountAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var account = await LiveAccountAsync(db, access.Tenant!.Id, id, ct);
        if (account is null) return AccountNotFound();
        var now = NowMs();
        account.DeletedAtMs = now;
        account.IsActive = false;
        account.TokenVersion++;
        account.UpdatedAtMs = now;
        account.UpdatedByUserId = access.User!.Id;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static Task<CatalogAccount?> LiveAccountAsync(CentralApiDbContext db, Guid tenantId, Guid id, CancellationToken ct) =>
        db.CatalogAccounts.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId && a.DeletedAtMs == null, ct);

    /// <summary>The customer from the company's mirrored customers (ERP or native), by its code; the stored code is the card's.</summary>
    private static async Task<PortalLedger.Customer?> CustomerAsync(CentralApiDbContext db, IMemoryCache cache, Guid tenantId, string? code, CancellationToken ct)
    {
        var trimmed = code?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        var customers = await PortalLedger.CustomersAsync(db, cache, tenantId, ct);
        return customers.GetValueOrDefault(trimmed);
    }

    private static Task<bool> UsernameTakenAsync(CentralApiDbContext db, Guid tenantId, string username, Guid? exceptId, CancellationToken ct) =>
        db.CatalogAccounts.AnyAsync(a => a.TenantId == tenantId && a.Username == username && a.DeletedAtMs == null && a.Id != exceptId, ct);

    private static Task<bool> IsStaffAsync(CentralApiDbContext db, Guid tenantId, Guid userId, CancellationToken ct) =>
        db.MobileUsers.AnyAsync(u => u.Id == userId && u.TenantId == tenantId && u.IsActive && u.DeletedAtUtc == null, ct);

    /// <summary>Saves; a concurrent save that took the same username or customer (the live unique indexes) is a 409.</summary>
    private static async Task<IResult?> SaveAccountAsync(CentralApiDbContext db, CatalogAccount account, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
            return null;
        }
        catch (DbUpdateException)
        {
            db.Entry(account).State = EntityState.Detached;
            return await db.CatalogAccounts.AnyAsync(a => a.TenantId == account.TenantId && a.Id != account.Id && a.DeletedAtMs == null && a.Username == account.Username, ct)
                ? UsernameTaken()
                : AccountExists();
        }
    }

    private static async Task<CatalogAccountDto> ToDtoAsync(CentralApiDbContext db, CatalogAccount a, CancellationToken ct)
    {
        var visibility = CatalogVisibility.Parse(a.VisibilityJson);
        var open = await OpenOrderCountsAsync(db, a.TenantId, [a.Id], ct);
        return new CatalogAccountDto
        {
            Id = a.Id,
            CustomerCode = a.CustomerCode,
            CustomerName = a.CustomerName,
            Username = a.Username,
            IsActive = a.IsActive,
            DiscountPercent = a.DiscountPercent,
            PriceListNo = a.PriceListNo,
            Visibility = new CatalogVisibilityDto
            {
                Mode = visibility.Mode,
                Rules = [.. visibility.Rules.Select(r => new CatalogVisibilityRuleDto { Type = r.Type, Key = r.Key, Effect = r.Effect })],
            },
            ShowStatement = a.ShowStatement,
            ShowInvoices = a.ShowInvoices,
            ShowPurchased = a.ShowPurchased,
            CanOrder = a.CanOrder,
            ResponsibleUserId = a.ResponsibleUserId,
            LastLoginAtMs = a.LastLoginAtMs,
            OpenOrderCount = open.GetValueOrDefault(a.Id),
            PasswordChangedAtMs = a.PasswordChangedAtMs,
            CreatedAtMs = a.CreatedAtMs,
            CreatedByName = a.CreatedByName,
            UpdatedAtMs = a.UpdatedAtMs,
        };
    }

    private static async Task<Dictionary<Guid, int>> OpenOrderCountsAsync(CentralApiDbContext db, Guid tenantId, List<Guid> accountIds, CancellationToken ct)
    {
        if (accountIds.Count == 0) return [];
        return await db.CatalogOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId && accountIds.Contains(o.AccountId)
                        && (o.Status == CatalogOrderStatuses.New || o.Status == CatalogOrderStatuses.Claimed))
            .GroupBy(o => o.AccountId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
    }

    private static CatalogVisibility? VisibilityOf(CatalogVisibilityDto? dto, out string? error) =>
        CatalogVisibility.Create(dto?.Mode, dto?.Rules?.Select(r => (r.Type, r.Key, r.Effect)), out error);

    /// <summary>Whether the JSON body names <paramref name="name"/> at all (a null value counts), ignoring case.</summary>
    private static bool Has(JsonElement body, string name) =>
        body.EnumerateObject().Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

    private static bool Contains(string text, string query) => Turkish.IndexOf(text, query, CompareOptions.IgnoreCase) >= 0;

    private static string NameOf(MobileUser user) => string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;

    private static string Cut(string value, int max) => value.Length > max ? value[..max] : value;

    internal static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // ---- answers ---------------------------------------------------------------------------

    internal static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });

    internal static IResult InvalidBody(string message) => Error(StatusCodes.Status400BadRequest, "INVALID_BODY", message);

    private static IResult Changed() =>
        Error(StatusCodes.Status409Conflict, "CATALOG_CHANGED", "Katalog bu arada değişti; güncel hâlini okuyup yeniden kaydedin.");

    private static IResult UnknownPriceList() =>
        Error(StatusCodes.Status400BadRequest, "UNKNOWN_PRICE_LIST", "Seçilen fiyat listesi bu firmada yok.");

    private static IResult CustomerNotFound() => Error(StatusCodes.Status404NotFound, "CUSTOMER_NOT_FOUND", "Cari bulunamadı.");

    private static IResult AccountNotFound() => Error(StatusCodes.Status404NotFound, "CATALOG_ACCOUNT_NOT_FOUND", "Katalog erişimi bulunamadı.");

    private static IResult AccountExists() =>
        Error(StatusCodes.Status409Conflict, "CATALOG_ACCOUNT_EXISTS", "Bu carinin zaten bir katalog erişimi var.");

    private static IResult UsernameTaken() =>
        Error(StatusCodes.Status409Conflict, "CATALOG_USERNAME_TAKEN", "Bu kullanıcı adı başka bir katalog erişiminde kullanılıyor.");

    private static IResult InvalidUsername() =>
        Error(StatusCodes.Status400BadRequest, "INVALID_USERNAME", "Kullanıcı adı 3–64 karakter olmalı; yalnız küçük harf, rakam, nokta, alt çizgi ve tire.");

    private static IResult InvalidDiscount() =>
        Error(StatusCodes.Status400BadRequest, "INVALID_DISCOUNT", $"İskonto 0 ile {CatalogAccounts.MaxDiscountPercent} arasında olmalı.");

    private static IResult InvalidVisibility(string message) => Error(StatusCodes.Status400BadRequest, "INVALID_VISIBILITY", message);

    private static IResult InvalidResponsible() =>
        Error(StatusCodes.Status400BadRequest, "INVALID_RESPONSIBLE_USER", "Sorumlu kişi firmanın aktif bir kullanıcısı olmalı.");
}
