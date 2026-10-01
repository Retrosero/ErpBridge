using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ErpBridge.CentralApi.Endpoints.CustomerCatalogManageEndpoints;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/customer-catalog/orders</c> (docs/GOAL_MUSTERI_KATALOGU.md §5.1, T1, T5): the customers' order requests
/// for staff on the phone and the panel. The module is checked as for the rest of the catalog, the management
/// permission is not: catalog managers see every request, everyone else only those routed to them or that they took.
/// A request is taken (<c>claim</c>) before it is turned into a sale; another's request is taken over only with
/// <c>force</c>, by a manager. Each change runs under the request's row lock, so two people pressing at once get one
/// winner and one 409. Turning it into a sale is the phone's (the sale carries <c>catalogOrderId</c>,
/// <see cref="CatalogOrderLinker"/>); <c>complete</c> records one entered elsewhere.
/// </summary>
public static class CustomerCatalogOrderEndpoints
{
    public const int PageSize = 50;
    public const int MaxRejectReasonLength = 500;
    public const int MaxDocumentRefLength = 128;

    private static readonly CompareInfo Turkish = CultureInfo.GetCultureInfo("tr-TR").CompareInfo;

    public static IEndpointRouteBuilder MapCustomerCatalogOrderEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath + "/orders")
            .WithTags("CustomerCatalog")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet(string.Empty, ListAsync).WithName("CustomerCatalogOrdersList");
        group.MapGet("/{id:guid}", GetAsync).WithName("CustomerCatalogOrderGet");
        group.MapPost("/{id:guid}/claim", ClaimAsync).WithName("CustomerCatalogOrderClaim");
        group.MapPost("/{id:guid}/release", ReleaseAsync).WithName("CustomerCatalogOrderRelease");
        group.MapPost("/{id:guid}/complete", CompleteAsync).WithName("CustomerCatalogOrderComplete");
        group.MapPost("/{id:guid}/reject", RejectAsync).WithName("CustomerCatalogOrderReject");
        return routes;
    }

    private static async Task<IResult> ListAsync(HttpContext http, string? status, string? q, int? page,
        [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: false, ct);
        if (access.Error is not null) return access.Error;
        var wanted = status?.Trim().ToUpperInvariant();
        if (!string.IsNullOrEmpty(wanted) && !CatalogOrderStatuses.All.Contains(wanted))
            return InvalidBody("status NEW, CLAIMED, COMPLETED ya da REJECTED olmalı.");

        var tenantId = access.Tenant!.Id;
        var user = access.User!;
        var query = db.CatalogOrders.AsNoTracking().Where(o => o.TenantId == tenantId);
        if (!RolePermissions.CanManageCustomerCatalog(user)) query = query.Where(o => o.AssignedUserId == user.Id || o.ClaimedByUserId == user.Id);
        // The lines stay in the database: a list needs only the summary.
        var visible = await query.Select(o => new CatalogOrder
        {
            Id = o.Id,
            No = o.No,
            CustomerCode = o.CustomerCode,
            CustomerName = o.CustomerName,
            Status = o.Status,
            Total = o.Total,
            LineCount = o.LineCount,
            SubmittedAtMs = o.SubmittedAtMs,
            AssignedUserId = o.AssignedUserId,
            ClaimedByUserId = o.ClaimedByUserId,
            ClaimedByName = o.ClaimedByName,
            ClaimedAtMs = o.ClaimedAtMs,
        }).ToListAsync(ct);

        IEnumerable<CatalogOrder> matching = visible;
        if (!string.IsNullOrEmpty(wanted)) matching = matching.Where(o => o.Status == wanted);
        if (q?.Trim() is { Length: > 0 } search)
            matching = matching.Where(o => Contains(o.No, search) || Contains(o.CustomerCode, search) || Contains(o.CustomerName, search));
        var sorted = matching.OrderByDescending(o => o.SubmittedAtMs).ThenBy(o => o.No, StringComparer.Ordinal).ToList();
        var pageNo = Math.Max(1, page ?? 1);
        var items = sorted.Skip((int)Math.Min((long)(pageNo - 1) * PageSize, int.MaxValue)).Take(PageSize).ToList();
        var names = await NamesAsync(db, tenantId, items.Select(o => o.AssignedUserId), ct);
        return JsonResults.Ok(new CatalogOrderListResponse
        {
            Items = [.. items.Select(o => Fill(new CatalogOrderSummaryDto(), o, names))],
            Total = sorted.Count,
            Counts = new CatalogOrderCountsDto
            {
                New = visible.Count(o => o.Status == CatalogOrderStatuses.New),
                Claimed = visible.Count(o => o.Status == CatalogOrderStatuses.Claimed),
                Completed = visible.Count(o => o.Status == CatalogOrderStatuses.Completed),
                Rejected = visible.Count(o => o.Status == CatalogOrderStatuses.Rejected),
            },
        });
    }

    private static async Task<IResult> GetAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: false, ct);
        if (access.Error is not null) return access.Error;
        var order = await db.CatalogOrders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id && o.TenantId == access.Tenant!.Id, ct);
        return order is null || !Sees(order, access.User!)
            ? NotFound()
            : JsonResults.Ok(await DetailAsync(db, views, order, ct));
    }

    /// <summary>Takes the request: <c>NEW</c> → <c>CLAIMED</c> by the caller. Taking one's own again changes nothing.</summary>
    private static Task<IResult> ClaimAsync(Guid id, HttpContext http, [FromBody] CatalogOrderClaimRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var force = body?.Force == true;
        return ChangeAsync(id, http, db, views, (order, user, manage, now) =>
        {
            if (force && !manage)
                return Error(StatusCodes.Status403Forbidden, "CATALOG_MANAGE_REQUIRED", "Başkasının aldığı talebi yalnız katalog yöneticisi devralabilir.");
            if (!CatalogOrderStatuses.IsOpen(order.Status)) return Closed(order);
            if (order.Status == CatalogOrderStatuses.Claimed && order.ClaimedByUserId == user.Id) return null;
            if (order.Status == CatalogOrderStatuses.Claimed && !force) return Taken(order);
            order.Status = CatalogOrderStatuses.Claimed;
            order.ClaimedByUserId = user.Id;
            order.ClaimedByName = NameOf(user);
            order.ClaimedAtMs = now;
            order.UpdatedAtMs = now;
            return null;
        }, ct);
    }

    /// <summary>Gives the request back: <c>CLAIMED</c> → <c>NEW</c>; by whoever has it, or a manager.</summary>
    private static Task<IResult> ReleaseAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, CancellationToken ct) =>
        ChangeAsync(id, http, db, views, (order, user, manage, now) =>
        {
            if (!CatalogOrderStatuses.IsOpen(order.Status)) return Closed(order);
            if (order.Status == CatalogOrderStatuses.New) return null;
            if (order.ClaimedByUserId != user.Id && !manage) return Taken(order);
            order.Status = CatalogOrderStatuses.New;
            order.ClaimedByUserId = null;
            order.ClaimedByName = null;
            order.ClaimedAtMs = null;
            order.UpdatedAtMs = now;
            return null;
        }, ct);

    /// <summary>
    /// Marks the request turned into a sale (<c>documentRef</c> = the sale's <c>externalId</c>, or none when it was entered
    /// elsewhere). Repeating it with the same document, or with none, changes nothing; another document is 409.
    /// </summary>
    private static Task<IResult> CompleteAsync(Guid id, HttpContext http, [FromBody] CatalogOrderCompleteRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var documentRef = string.IsNullOrWhiteSpace(body?.DocumentRef) ? null : body.DocumentRef.Trim();
        if (documentRef is { Length: > MaxDocumentRefLength })
            return Task.FromResult(InvalidBody($"documentRef en çok {MaxDocumentRefLength} karakter olabilir."));
        return ChangeAsync(id, http, db, views, (order, user, manage, now) =>
        {
            if (order.Status == CatalogOrderStatuses.Rejected) return Closed(order);
            if (order.Status == CatalogOrderStatuses.Completed)
                return documentRef is null || string.Equals(order.DocumentRef, documentRef, StringComparison.Ordinal) ? null : AlreadyConverted(order);
            if (order.Status == CatalogOrderStatuses.Claimed && order.ClaimedByUserId != user.Id && !manage) return Taken(order);
            order.Status = CatalogOrderStatuses.Completed;
            order.DocumentRef = documentRef;
            Close(order, user, now);
            return null;
        }, ct);
    }

    /// <summary>Refuses the request with a reason the customer reads; rejecting it again changes nothing.</summary>
    private static Task<IResult> RejectAsync(Guid id, HttpContext http, [FromBody] CatalogOrderRejectRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var reason = body?.Reason?.Trim();
        if (string.IsNullOrEmpty(reason)) return Task.FromResult(InvalidBody("Reddetme gerekçesi gerekli."));
        if (reason.Length > MaxRejectReasonLength) reason = reason[..MaxRejectReasonLength];
        return ChangeAsync(id, http, db, views, (order, user, manage, now) =>
        {
            if (order.Status == CatalogOrderStatuses.Completed) return Closed(order);
            if (order.Status == CatalogOrderStatuses.Rejected) return null;
            if (order.Status == CatalogOrderStatuses.Claimed && order.ClaimedByUserId != user.Id && !manage) return Taken(order);
            order.Status = CatalogOrderStatuses.Rejected;
            order.RejectReason = reason;
            Close(order, user, now);
            return null;
        }, ct);
    }

    /// <summary>
    /// One change of a request the caller may see, under its row lock: <paramref name="apply"/> changes the tracked row
    /// (or nothing) and returns null, or the refusal. Answers with the request as it is afterwards.
    /// </summary>
    private static async Task<IResult> ChangeAsync(Guid id, HttpContext http, CentralApiDbContext db, CatalogViewService views,
        Func<CatalogOrder, MobileUser, bool, long, IResult?> apply, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: false, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        var user = access.User!;
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        await CatalogOrders.LockAsync(db, tenantId, id, ct);
        var order = await db.CatalogOrders.FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId, ct);
        if (order is null || !Sees(order, user)) return NotFound();
        if (apply(order, user, RolePermissions.CanManageCustomerCatalog(user), NowMs()) is { } refused) return refused;
        if (db.ChangeTracker.HasChanges()) await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return JsonResults.Ok(await DetailAsync(db, views, order, ct));
    }

    /// <summary>Managers see every request; anyone else those routed to them or that they took.</summary>
    private static bool Sees(CatalogOrder order, MobileUser user) =>
        RolePermissions.CanManageCustomerCatalog(user) || order.AssignedUserId == user.Id || order.ClaimedByUserId == user.Id;

    private static void Close(CatalogOrder order, MobileUser user, long now)
    {
        order.ClosedByUserId = user.Id;
        order.ClosedByName = NameOf(user);
        order.ClosedAtMs = now;
        order.UpdatedAtMs = now;
    }

    private static async Task<CatalogOrderDetailDto> DetailAsync(CentralApiDbContext db, CatalogViewService views, CatalogOrder order, CancellationToken ct)
    {
        var view = await views.LoadAsync(db, order.TenantId, forCustomer: false, ct);
        var names = await NamesAsync(db, order.TenantId, [order.AssignedUserId], ct);
        var detail = Fill(new CatalogOrderDetailDto(), order, names);
        detail.Note = order.Note;
        detail.PriceListNo = order.PriceListNo;
        detail.PriceListName = view.PriceList(order.PriceListNo)?.Name;
        detail.PriceIncludesVat = order.PriceIncludesVat;
        detail.DiscountPercent = order.DiscountPercent;
        detail.RejectReason = order.RejectReason;
        detail.DocumentRef = order.DocumentRef;
        detail.ClosedByName = order.ClosedByName;
        detail.ClosedAtMs = order.ClosedAtMs;
        detail.Lines = [.. CatalogOrders.Lines(order).Select(l => new CatalogOrderLineDto
        {
            StockCode = l.StockCode,
            Name = l.Name,
            Unit = l.Unit,
            Quantity = l.Quantity,
            CartonQuantity = l.CartonQuantity,
            ListPrice = l.ListPrice,
            DiscountPercent = l.DiscountPercent,
            VatRate = l.VatRate,
            Gross = l.Gross,
            Discount = l.Discount,
            Vat = l.Vat,
            Total = l.Total,
            InStockNow = view.Products.GetValueOrDefault(l.StockCode)?.InStock == true,
        })];
        return detail;
    }

    private static T Fill<T>(T dto, CatalogOrder o, IReadOnlyDictionary<Guid, string> names) where T : CatalogOrderSummaryDto
    {
        dto.Id = o.Id;
        dto.No = o.No;
        dto.CustomerCode = o.CustomerCode;
        dto.CustomerName = o.CustomerName;
        dto.Status = o.Status;
        dto.Total = o.Total;
        dto.LineCount = o.LineCount;
        dto.SubmittedAtMs = o.SubmittedAtMs;
        dto.AssignedUserName = o.AssignedUserId is { } assigned ? names.GetValueOrDefault(assigned) : null;
        dto.ClaimedByUserId = o.ClaimedByUserId;
        dto.ClaimedByName = o.ClaimedByName;
        dto.ClaimedAtMs = o.ClaimedAtMs;
        return dto;
    }

    private static async Task<Dictionary<Guid, string>> NamesAsync(CentralApiDbContext db, Guid tenantId, IEnumerable<Guid?> ids, CancellationToken ct)
    {
        var wanted = ids.OfType<Guid>().Distinct().ToList();
        if (wanted.Count == 0) return [];
        return await db.MobileUsers.AsNoTracking().Where(u => u.TenantId == tenantId && wanted.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName, ct);
    }

    private static string NameOf(MobileUser user)
    {
        var name = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;
        return name.Length > 120 ? name[..120] : name;
    }

    private static bool Contains(string text, string query) => Turkish.IndexOf(text, query, CompareOptions.IgnoreCase) >= 0;

    private static IResult NotFound() => Error(StatusCodes.Status404NotFound, "CATALOG_ORDER_NOT_FOUND", "Müşteri talebi bulunamadı.");

    private static IResult Closed(CatalogOrder order) =>
        Error(StatusCodes.Status409Conflict, "CATALOG_ORDER_CLOSED",
            order.Status == CatalogOrderStatuses.Completed ? "Bu talep zaten siparişe çevrildi." : "Bu talep reddedildi.");

    private static IResult Taken(CatalogOrder order) =>
        Error(StatusCodes.Status409Conflict, "CATALOG_ORDER_TAKEN", $"Bu talep {order.ClaimedByName ?? "başka bir kullanıcı"} tarafından işleniyor.");

    private static IResult AlreadyConverted(CatalogOrder order) =>
        Error(StatusCodes.Status409Conflict, "CATALOG_ORDER_ALREADY_CONVERTED", $"Bu talep ({order.No}) zaten başka bir belgeye çevrildi.");
}
