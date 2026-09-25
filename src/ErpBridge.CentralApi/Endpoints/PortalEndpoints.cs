using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Native;
using ErpBridge.CentralApi.Portal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal</c>: read-only figures for the company manager portal (Faz 41).
///
/// <para>The caller signs in with the same company account as the phone
/// (<c>/api/v1/android/account/login</c>). The company comes from the token, never from
/// the request, and every call re-checks the user, device and subscription. Only
/// administrators and managers may read; a salesperson gets 403
/// <c>PORTAL_REQUIRES_MANAGER</c>. Approvals and users are served by the existing
/// <c>/api/v1/android/approvals</c> and <c>/api/v1/android/account/users</c> endpoints.</para>
/// </summary>
public static class PortalEndpoints
{
    public static IEndpointRouteBuilder MapPortalEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/summary", SummaryAsync).WithName("PortalSummary");
        group.MapGet("/activity", ActivityAsync).WithName("PortalActivity");
        group.MapGet("/visits", VisitsAsync).WithName("PortalVisits");
        group.MapGet("/balances", BalancesAsync).WithName("PortalBalances");
        group.MapGet("/stock", StockAsync).WithName("PortalStock");
        group.MapGet("/stock/search", StockSearchAsync).WithName("PortalStockSearch");
        group.MapGet("/stock/facets", StockFacetsAsync).WithName("PortalStockFacets");
        group.MapGet("/customers", CustomersAsync).WithName("PortalCustomers");
        group.MapGet("/customers/card", CustomerCardAsync).WithName("PortalCustomerCard");
        group.MapGet("/customers/ledger", CustomerLedgerAsync).WithName("PortalCustomerLedger");
        group.MapGet("/customers/document", CustomerDocumentAsync).WithName("PortalCustomerDocument");
        group.MapGet("/payments", PaymentsAsync).WithName("PortalPayments");
        group.MapGet("/movements", CompanyMovementsAsync).WithName("PortalCompanyMovements");
        return routes;
    }

    private static async Task<IResult> SummaryAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? date, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeManagerAsync(http, db, ct);
        if (error is not null) return error;
        if (!TryDay(date, out var day)) return BadDate("date");

        var response = new PortalSummaryResponse
        {
            Date = Format(day),
            DataSource = tenant!.DataSource,
        };
        foreach (var document in await PortalReports.MoneyDocumentsAsync(db, tenant, day, day, ct))
            PortalReports.Add(PortalReports.LineFor(document.DocumentType, response.Sales, response.Collections, response.Disbursements, response.Returns), document.Amount);

        var visits = await PortalReports.VisitsAsync(db, tenant.Id, day, ct);
        response.VisitsPlanned = visits.Count(v => v.Planned);
        response.VisitsCompleted = visits.Count(v => v.Status == "COMPLETED");
        response.VisitsSkipped = visits.Count(v => v.Status == "SKIPPED");
        response.PendingApprovals = await db.ApprovalRequests.AsNoTracking()
            .CountAsync(r => r.TenantId == tenant.Id && r.Status == ApprovalStatuses.Pending, ct);
        return JsonResults.Ok(response);
    }

    private static async Task<IResult> ActivityAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? from, string? to, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeManagerAsync(http, db, ct);
        if (error is not null) return error;
        if (!TryDay(from, out var start)) return BadDate("from");
        if (!TryDay(to, out var end)) return BadDate("to");
        if (end < start) return JsonResults.Status(400, new ApiError { ErrorCode = "INVALID_RANGE", Message = "to is before from." });
        if (end.DayNumber - start.DayNumber + 1 > PortalReports.MaxRangeDays)
            return JsonResults.Status(400, new ApiError { ErrorCode = "RANGE_TOO_LONG", Message = $"At most {PortalReports.MaxRangeDays} days." });

        var users = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenant!.Id)
            .Select(u => new { u.Id, u.Username, u.FullName, u.Role, u.DeletedAtUtc })
            .ToListAsync(ct);
        var byId = new Dictionary<Guid, PortalUserActivity>();
        var byName = new Dictionary<string, PortalUserActivity>(StringComparer.OrdinalIgnoreCase);
        foreach (var user in users)
        {
            var activity = new PortalUserActivity { UserId = user.Id, Username = user.Username, FullName = user.FullName, Role = user.Role };
            byId[user.Id] = activity;
            // A deleted user's name can be reused; visits then count for the current holder.
            if (user.DeletedAtUtc is null || !byName.ContainsKey(user.Username)) byName[user.Username] = activity;
        }
        PortalUserActivity? unattributed = null;

        foreach (var document in await PortalReports.MoneyDocumentsAsync(db, tenant!, start, end, ct))
        {
            PortalUserActivity target;
            if (document.CreatedByUserId is { } id && byId.TryGetValue(id, out var known)) target = known;
            else target = unattributed ??= new PortalUserActivity { Username = string.Empty, FullName = "Belirsiz (hesapsız veya eski kayıt)" };
            PortalReports.Add(PortalReports.LineFor(document.DocumentType, target.Sales, target.Collections, target.Disbursements, target.Returns), document.Amount);
        }

        var routeRecords = await PortalReports.RouteRecordsAsync(db, tenant!.Id, ct);
        for (var day = start; day <= end; day = day.AddDays(1))
        {
            foreach (var visit in PortalReports.BuildVisits(routeRecords, day))
            {
                if (!byName.TryGetValue(visit.Username, out var user)) continue;
                if (visit.Status == "COMPLETED") user.VisitsCompleted++;
                else if (visit.Status == "SKIPPED") user.VisitsSkipped++;
            }
        }

        var rows = byId.Values
            .Where(u => u.Sales.Count + u.Collections.Count + u.Disbursements.Count + u.Returns.Count + u.VisitsCompleted + u.VisitsSkipped > 0
                        || users.Any(x => x.Id == u.UserId && x.DeletedAtUtc is null))
            .OrderByDescending(u => u.Sales.Amount)
            .ThenBy(u => u.Username, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (unattributed is not null) rows.Add(unattributed);

        return JsonResults.Ok(new PortalActivityResponse { From = Format(start), To = Format(end), Users = rows });
    }

    private static async Task<IResult> VisitsAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? date, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeManagerAsync(http, db, ct);
        if (error is not null) return error;
        if (!TryDay(date, out var day)) return BadDate("date");
        return JsonResults.Ok(new PortalVisitsResponse { Date = Format(day), Rows = await PortalReports.VisitsAsync(db, tenant!.Id, day, ct) });
    }

    private static async Task<IResult> BalancesAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, string? search, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeLedgerAsync(http, db, ct);
        if (error is not null) return error;
        return JsonResults.Ok(await PortalReports.BalancesAsync(db, cache, tenant!, search, ct));
    }

    private static async Task<IResult> StockAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? search, bool? outOfStock, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeLedgerAsync(http, db, ct);
        if (error is not null) return error;
        return JsonResults.Ok(await PortalReports.StockAsync(db, tenant!.Id, search, outOfStock == true, ct));
    }

    /// <summary>
    /// The stock page: every product, filtered, sorted and paged on the server. Multi-value
    /// filters repeat their parameter (<c>?brand=A&amp;brand=B</c>).
    /// </summary>
    private static async Task<IResult> StockSearchAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache,
        [AsParameters] StockSearchParameters p, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeLedgerAsync(http, db, ct);
        if (error is not null) return error;

        var status = string.IsNullOrWhiteSpace(p.Status) ? "all" : p.Status.Trim();
        var sort = string.IsNullOrWhiteSpace(p.Sort) ? "name" : p.Sort.Trim();
        if (!StockQuery.Statuses.Contains(status, StringComparer.Ordinal))
            return BadQuery("status must be one of: " + string.Join(", ", StockQuery.Statuses) + ".");
        if (!StockQuery.Sorts.Contains(sort, StringComparer.Ordinal))
            return BadQuery("sort must be one of: " + string.Join(", ", StockQuery.Sorts) + ".");
        if (p.Dir is not (null or "asc" or "desc")) return BadQuery("dir must be asc or desc.");
        if (p.IdleDays is < 0) return BadQuery("idleDays cannot be negative.");

        var query = new StockQuery(
            p.Q, Values(p.MainGroup), Values(p.SubGroup), Values(p.Brand), Values(p.Shelf),
            p.Warehouse, p.PriceList, p.MinQty, p.MaxQty, p.MinPrice, p.MaxPrice,
            status, p.Below, p.IdleDays, sort, p.Dir == "desc",
            Math.Max(1, p.Page ?? 1), Math.Clamp(p.PageSize ?? 50, 1, StockQuery.MaxPageSize));
        var catalog = await PortalStockCatalog.LoadAsync(db, cache, tenant!.Id, ct);
        return JsonResults.Ok(PortalStockCatalog.Search(catalog, query, PortalReports.BusinessDate(null, DateTimeOffset.UtcNow)));
    }

    private static async Task<IResult> StockFacetsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeLedgerAsync(http, db, ct);
        if (error is not null) return error;
        return JsonResults.Ok(PortalStockCatalog.Facets(await PortalStockCatalog.LoadAsync(db, cache, tenant!.Id, ct)));
    }

    /// <summary>Every customer, paged. Codes travel in the query, not the path: Mikro codes may hold a slash.</summary>
    private static async Task<IResult> CustomersAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache,
        string? q, string? balance, string? sort, string? dir, int? page, int? pageSize, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeLedgerAsync(http, db, ct);
        if (error is not null) return error;
        balance = string.IsNullOrWhiteSpace(balance) ? "all" : balance.Trim();
        sort = string.IsNullOrWhiteSpace(sort) ? "title" : sort.Trim();
        if (balance is not ("all" or "receivable" or "payable" or "nonzero")) return BadQuery("balance must be all, receivable, payable or nonzero.");
        if (sort is not ("title" or "code" or "balance" or "absBalance")) return BadQuery("sort must be title, code, balance or absBalance.");
        if (dir is not (null or "asc" or "desc")) return BadQuery("dir must be asc or desc.");

        var customers = await PortalLedger.CustomersAsync(db, cache, tenant!.Id, tenant.DataSource, ct);
        return JsonResults.Ok(PortalLedger.Search(customers, q, balance, sort, dir == "desc",
            Math.Max(1, page ?? 1), Math.Clamp(pageSize ?? 50, 1, PortalLedger.MaxPageSize)));
    }

    private static async Task<IResult> CustomerCardAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache,
        string? code, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeLedgerAsync(http, db, ct);
        if (error is not null) return error;
        var customer = await FindCustomerAsync(db, cache, tenant!, code, ct);
        return customer is null ? CustomerNotFound() : JsonResults.Ok(PortalLedger.Card(customer, tenant!.DataSource));
    }

    /// <summary>
    /// A customer's statement: <c>from</c>/<c>to</c> (yyyy-MM-dd, both optional), repeated <c>kind</c>,
    /// newest first. <c>includeVoided</c> (GOAL_PANEL_ERPSIZ E4d) defaults to false, so a plain call
    /// keeps today's contract: a voided original stays out of the list, only its reversal shows.
    /// </summary>
    private static async Task<IResult> CustomerLedgerAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache,
        string? code, string? from, string? to, string[]? kind, bool? includeVoided, int? page, int? pageSize, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeLedgerAsync(http, db, ct);
        if (error is not null) return error;
        DateOnly? start = null, end = null;
        if (!string.IsNullOrWhiteSpace(from))
        {
            if (!TryDay(from, out var day)) return BadDate("from");
            start = day;
        }
        if (!string.IsNullOrWhiteSpace(to))
        {
            if (!TryDay(to, out var day)) return BadDate("to");
            end = day;
        }
        if (start is not null && end is not null && end < start)
            return JsonResults.Status(400, new ApiError { ErrorCode = "INVALID_RANGE", Message = "to is before from." });
        var kinds = Values(kind);
        if (kinds.FirstOrDefault(k => !PortalLedger.Kinds.Contains(k, StringComparer.Ordinal)) is { } unknown)
            return BadQuery($"kind '{unknown}' is not one of: " + string.Join(", ", PortalLedger.Kinds) + ".");

        var customer = await FindCustomerAsync(db, cache, tenant!, code, ct);
        if (customer is null) return CustomerNotFound();
        var movements = await PortalLedger.MovementsAsync(db, cache, tenant!.Id, ct);
        var statement = PortalLedger.Statement(customer, movements, start, end, kinds, includeVoided ?? false,
            Math.Max(1, page ?? 1), Math.Clamp(pageSize ?? 50, 1, PortalLedger.MaxPageSize));
        await AttachEditableAndVoidedByAsync(db, tenant.Id, statement.Items, ct);
        return JsonResults.Ok(statement);
    }

    /// <summary>
    /// Fills in the two fields <see cref="PortalLedger.Statement"/> cannot: <see cref="PortalLedgerRow.Editable"/>
    /// (needs the row's own <c>Jobs.DocumentType</c> — the same check E4a/E4c's endpoints make before
    /// booking a void/edit) and <see cref="PortalLedgerRow.VoidedBy"/> (a name for <see cref="PortalLedgerRow.VoidedByUserId"/>).
    /// Scoped to the page shown, like E3c's own creator lookup.
    /// </summary>
    private static async Task AttachEditableAndVoidedByAsync(CentralApiDbContext db, Guid tenantId, List<PortalLedgerRow> items, CancellationToken ct)
    {
        if (items.Count == 0) return;
        var externalIds = items.Select(i => PortalLedger.ExternalIdOf(i.Id)).Distinct(StringComparer.Ordinal).ToList();
        var jobTypes = await db.Jobs.AsNoTracking()
            .Where(j => j.TenantId == tenantId && externalIds.Contains(j.ExternalId))
            .Select(j => new { j.ExternalId, j.DocumentType })
            .ToListAsync(ct);
        var typeByExternalId = jobTypes.GroupBy(j => j.ExternalId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().DocumentType, StringComparer.Ordinal);

        var voidedByIds = items.Select(i => i.VoidedByUserId).OfType<Guid>().Distinct().ToList();
        var names = voidedByIds.Count == 0 ? [] : await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenantId && voidedByIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName, ct);

        foreach (var item in items)
        {
            var documentType = typeByExternalId.TryGetValue(PortalLedger.ExternalIdOf(item.Id), out var type) ? type : null;
            item.Editable = !item.Voided && documentType is not null && NativeDocumentProcessor.VoidableLedgerJobTypes.Contains(documentType);
            if (item.VoidedByUserId is { } userId && names.TryGetValue(userId, out var name)) item.VoidedBy = name;
        }
    }

    private static async Task<IResult> CustomerDocumentAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache,
        string? code, string? key, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeLedgerAsync(http, db, ct);
        if (error is not null) return error;
        var customer = await FindCustomerAsync(db, cache, tenant!, code, ct);
        if (customer is null) return CustomerNotFound();
        var document = string.IsNullOrWhiteSpace(key) ? null : PortalLedger.Document(customer, await PortalLedger.MovementsAsync(db, cache, tenant!.Id, ct), key);
        return document is null
            ? JsonResults.Status(404, new ApiError { ErrorCode = "DOCUMENT_NOT_FOUND", Message = "No document with lines under that key for this customer." })
            : JsonResults.Ok(document);
    }

    /// <summary>
    /// Every collection/payment across the company (GOAL_PANEL_ERPSIZ E3c). <c>from</c> defaults to the
    /// first of this month, <c>to</c> to today — unlike the per-customer statement, which defaults to
    /// no bound, because scanning every payment ever made would grow unbounded for an old tenant.
    /// </summary>
    private static async Task<IResult> PaymentsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache,
        string? from, string? to, string? customer, string[]? kind, Guid? userId, int? page, int? pageSize, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeLedgerAsync(http, db, ct);
        if (error is not null) return error;

        DateOnly start;
        if (string.IsNullOrWhiteSpace(from))
        {
            var today = PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);
            start = new DateOnly(today.Year, today.Month, 1);
        }
        else if (!TryDay(from, out start)) return BadDate("from");
        if (!TryDay(to, out var end)) return BadDate("to");
        if (end < start) return JsonResults.Status(400, new ApiError { ErrorCode = "INVALID_RANGE", Message = "to is before from." });

        var kinds = Values(kind);
        if (kinds.FirstOrDefault(k => k is not ("collection" or "payment")) is { } unknown)
            return BadQuery($"kind '{unknown}' is not one of: collection, payment.");

        var customers = await PortalLedger.CustomersAsync(db, cache, tenant!.Id, ct);
        var movements = await PortalLedger.MovementsAsync(db, cache, tenant.Id, ct);

        // Bounded by the payment-kind rows the tenant has ever had, not by the requested date range:
        // cheap for a native tenant (Jobs is its whole write history) and avoids a second round trip
        // once the range is known to Payments().
        var paymentMovementIds = movements.ByCustomer.Values.SelectMany(list => list)
            .Where(m => m.Kind is "collection" or "payment")
            .Select(m => PortalLedger.ExternalIdOf(m.Id))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var creatorLookup = (await db.Jobs.AsNoTracking()
                .Where(j => j.TenantId == tenant.Id && paymentMovementIds.Contains(j.ExternalId))
                .Select(j => new { j.ExternalId, j.CreatedByUserId })
                .ToListAsync(ct))
            .GroupBy(j => j.ExternalId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().CreatedByUserId, StringComparer.Ordinal);
        var creatorIds = creatorLookup.Values.OfType<Guid>().Distinct().ToList();
        var userNames = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenant.Id && creatorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName, ct);

        return JsonResults.Ok(PortalLedger.Payments(customers, movements, start, end, kinds, customer, userId,
            externalId => creatorLookup.TryGetValue(externalId, out var uid) ? uid : null, userNames,
            Math.Max(1, page ?? 1), Math.Clamp(pageSize ?? 50, 1, PortalLedger.MaxPageSize)));
    }

    /// <summary>
    /// GOAL_PANEL_ERPSIZ E4e: every customer-side movement of the company (<c>/hareketler</c>) — date range (default this month),
    /// kinds, customer, user, amount range, cancelled rows on demand, paged. Read-only, the statement's own gate.
    /// </summary>
    private static async Task<IResult> CompanyMovementsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache,
        string? from, string? to, string? customer, string[]? kind, Guid? userId, decimal? minAmount, decimal? maxAmount, bool? includeVoided,
        int? page, int? pageSize, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeLedgerAsync(http, db, ct);
        if (error is not null) return error;

        DateOnly start;
        if (string.IsNullOrWhiteSpace(from))
        {
            var today = PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);
            start = new DateOnly(today.Year, today.Month, 1);
        }
        else if (!TryDay(from, out start)) return BadDate("from");
        if (!TryDay(to, out var end)) return BadDate("to");
        if (end < start) return JsonResults.Status(400, new ApiError { ErrorCode = "INVALID_RANGE", Message = "to is before from." });
        if (minAmount is { } min && maxAmount is { } max && max < min) return BadQuery("maxAmount is below minAmount.");

        var kinds = Values(kind);
        if (kinds.FirstOrDefault(k => !PortalLedger.Kinds.Contains(k)) is { } unknown)
            return BadQuery($"kind '{unknown}' is not one of: {string.Join(", ", PortalLedger.Kinds)}.");

        var customers = await PortalLedger.CustomersAsync(db, cache, tenant!.Id, tenant.DataSource, ct);
        var movements = await PortalLedger.MovementsAsync(db, cache, tenant.Id, ct);
        var matching = PortalLedger.CompanyMovements(movements, start, end, kinds, customer, minAmount, maxAmount, includeVoided == true);

        var size = Math.Clamp(pageSize ?? 50, 1, PortalLedger.MaxPageSize);
        var number = Math.Max(1, page ?? 1);
        // Who wrote a row comes from its job. Without a user filter only the rows on the page need one; with it, every
        // matching row does (bounded by the filter's own date range).
        var reversalAuthors = PortalLedger.ReversalAuthors(movements);
        var needed = (userId is null ? matching.Skip((number - 1) * size).Take(size) : matching)
            .Where(m => !reversalAuthors.ContainsKey(m.Id))
            .Select(m => PortalLedger.ExternalIdOf(m.Id)).Distinct(StringComparer.Ordinal).ToList();
        var creators = (await db.Jobs.AsNoTracking()
                .Where(j => j.TenantId == tenant.Id && needed.Contains(j.ExternalId))
                .Select(j => new { j.ExternalId, j.CreatedByUserId })
                .ToListAsync(ct))
            .GroupBy(j => j.ExternalId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().CreatedByUserId, StringComparer.Ordinal);
        Guid? CreatorOf(PortalLedger.Movement m) =>
            reversalAuthors.TryGetValue(m.Id, out var voider) ? voider
            : creators.TryGetValue(PortalLedger.ExternalIdOf(m.Id), out var id) ? id : null;
        if (userId is { } wanted) matching = matching.Where(m => CreatorOf(m) == wanted).ToList();

        var creatorIds = creators.Values.OfType<Guid>().Concat(reversalAuthors.Values).Distinct().ToList();
        var userNames = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenant.Id && creatorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName, ct);
        return JsonResults.Ok(PortalLedger.MovementsPage(customers, matching, start, end, CreatorOf, userNames, number, size));
    }

    private static async Task<PortalLedger.Customer?> FindCustomerAsync(CentralApiDbContext db, IMemoryCache cache, Tenant tenant, string? code, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var customers = await PortalLedger.CustomersAsync(db, cache, tenant.Id, tenant.DataSource, ct);
        return customers.TryGetValue(code.Trim(), out var customer) ? customer : null;
    }

    private static IResult CustomerNotFound() =>
        JsonResults.Status(404, new ApiError { ErrorCode = "CUSTOMER_NOT_FOUND", Message = "No customer with that code." });

    public sealed class StockSearchParameters
    {
        [FromQuery] public string? Q { get; set; }
        [FromQuery] public string[]? MainGroup { get; set; }
        [FromQuery] public string[]? SubGroup { get; set; }
        [FromQuery] public string[]? Brand { get; set; }
        [FromQuery] public string[]? Shelf { get; set; }
        [FromQuery] public int? Warehouse { get; set; }
        [FromQuery] public int? PriceList { get; set; }
        [FromQuery] public decimal? MinQty { get; set; }
        [FromQuery] public decimal? MaxQty { get; set; }
        [FromQuery] public decimal? MinPrice { get; set; }
        [FromQuery] public decimal? MaxPrice { get; set; }
        [FromQuery] public string? Status { get; set; }
        [FromQuery] public decimal? Below { get; set; }
        [FromQuery] public int? IdleDays { get; set; }
        [FromQuery] public string? Sort { get; set; }
        [FromQuery] public string? Dir { get; set; }
        [FromQuery] public int? Page { get; set; }
        [FromQuery] public int? PageSize { get; set; }
    }

    // ---- helpers --------------------------------------------------------------

    private static string[] Values(string[]? values) =>
        values is null ? [] : values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static IResult BadQuery(string message) =>
        JsonResults.Status(400, new ApiError { ErrorCode = "INVALID_QUERY", Message = message });

    private static Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeManagerAsync(
        HttpContext http, CentralApiDbContext db, CancellationToken ct) =>
        AuthorizeAsync(http, db, RolePermissions.CanViewReports, ct);

    /// <summary>Balances and stock: managers and accounting.</summary>
    private static Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeLedgerAsync(
        HttpContext http, CentralApiDbContext db, CancellationToken ct) =>
        AuthorizeAsync(http, db, RolePermissions.CanViewLedger, ct);

    private static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeAsync(
        HttpContext http, CentralApiDbContext db, Func<MobileUser, bool> allowed, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access;
        if (!allowed(access.User!))
            return (null, null, JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
            {
                ErrorCode = "PORTAL_REQUIRES_MANAGER",
                Message = "The manager portal is for company administrators and managers.",
            }));
        return access;
    }

    /// <summary>An absent day means today in Istanbul.</summary>
    private static bool TryDay(string? value, out DateOnly day)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            day = PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);
            return true;
        }
        return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day);
    }

    private static string Format(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static IResult BadDate(string name) =>
        JsonResults.Status(400, new ApiError { ErrorCode = "INVALID_DATE", Message = $"{name} must be yyyy-MM-dd." });
}
