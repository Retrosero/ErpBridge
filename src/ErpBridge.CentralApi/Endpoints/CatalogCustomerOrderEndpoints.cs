using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Storage;
using ErpBridge.CentralApi.Sync;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using static ErpBridge.CentralApi.Endpoints.CustomerCatalogManageEndpoints;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// The customer's order requests under <c>/api/v1/catalog/{code}</c> (docs/GOAL_MUSTERI_KATALOGU.md §5.2, K3): the cart
/// is priced again on the server — prices, discounts, stock, cartons and visibility as the customer's catalog has them
/// now — and becomes a request staff turn into a sale. The page's <c>requestId</c> is the request's id, so a repeated
/// submit returns the same request. The staff it is routed to and the catalog managers hear of it
/// (<see cref="UserNotificationKinds.CatalogOrderNew"/>, §5.3).
/// </summary>
internal static class CatalogCustomerOrderEndpoints
{
    /// <summary>The page shows at most this many of a customer's requests, newest first.</summary>
    public const int MaxListed = 100;

    public const int MaxNoteLength = 1000;

    /// <summary>The page's total may differ from the server's by rounding, never by more.</summary>
    public const decimal PriceTolerance = 0.05m;

    public static void Map(RouteGroupBuilder signedIn)
    {
        signedIn.MapPost("/orders", SubmitAsync).WithName("CatalogOrderSubmit");
        signedIn.MapGet("/orders", ListAsync).WithName("CatalogOrders");
        signedIn.MapGet("/orders/detail", DetailAsync).WithName("CatalogOrderDetail");
    }

    /// <summary>
    /// Checks and prices first, outside any lock; then, under the account's row lock, the open-request limit, the request
    /// and its notifications in one save — the change counter is taken right before it (KB rule 11).
    /// </summary>
    private static async Task<IResult> SubmitAsync(
        HttpContext http,
        [FromBody] CatalogOrderRequest? body,
        [FromServices] CentralApiDbContext db,
        [FromServices] CatalogViewService views,
        [FromServices] IMemoryCache cache,
        [FromServices] ITenantEventHub events,
        [FromServices] IOptions<CustomerCatalogOptions> options,
        CancellationToken ct)
    {
        if (body is null || body.RequestId == Guid.Empty) return InvalidBody("requestId gerekli.");
        if (body.Lines is not { Length: > 0 } lines) return InvalidBody("Sepet boş.");
        if (lines.Length > options.Value.MaxOrderLines) return InvalidBody($"Bir talepte en çok {options.Value.MaxOrderLines} satır olabilir.");
        if (body.ExpectedTotal is not { } expected) return InvalidBody("expectedTotal gerekli.");
        var note = string.IsNullOrWhiteSpace(body.Note) ? null : body.Note.Trim();
        if (note is { Length: > MaxNoteLength }) return InvalidBody($"Not en çok {MaxNoteLength} karakter olabilir.");

        var session = CatalogSession.Of(http);
        var tenant = session.Tenant;
        var account = session.Account;
        if (await ReplayAsync(db, body.RequestId, session, ct) is { } replay) return replay;

        if (!account.CanOrder) return OrderingDisabled();
        var card = (await PortalLedger.CustomersAsync(db, cache, tenant.Id, tenant.DataSource, ct)).GetValueOrDefault(account.CustomerCode);
        if (card?.IsLocked == true) return OrderingDisabled();

        var customer = await CustomerCatalogPublicEndpoints.CustomerViewAsync(http, db, views, ct);
        var quote = CatalogQuote.Build(customer, lines.Select(l => (l.Key, l.Quantity)));
        if (quote.Lines.Any(l => l.Issue is not null))
            return QuoteError(StatusCodes.Status422UnprocessableEntity, "CART_INVALID", "Sepette sipariş verilemeyecek satırlar var.", quote);
        if (Math.Abs(quote.Totals.Total - expected) > PriceTolerance)
            return QuoteError(StatusCodes.Status409Conflict, "PRICE_CHANGED", "Fiyatlar değişti; güncel tutarı kontrol edip yeniden gönderin.", quote);

        var orderLines = quote.Lines.Select(l =>
        {
            var product = customer.Find(l.Key)!;
            return new CatalogOrderLine(product.Code, product.Name, product.Unit, l.Quantity, product.EffectiveCartonQuantity,
                l.Price!.List, l.Price.DiscountPercent, l.Price.Net, product.VatRate, l.Gross, l.Discount, l.Vat, l.Total);
        }).ToList();
        var assigned = (await CatalogOrders.AssigneeAsync(db, cache, tenant.Id, account.ResponsibleUserId, account.CustomerCode, card, ct)).UserId;
        var recipients = await CatalogOrders.ManagersAsync(db, tenant.Id, ct);
        if (assigned is { } assignee) recipients.Add(assignee);
        var now = NowMs();
        var order = new CatalogOrder
        {
            Id = body.RequestId,
            TenantId = tenant.Id,
            AccountId = account.Id,
            CustomerCode = account.CustomerCode,
            CustomerName = account.CustomerName,
            AccountUsername = account.Username,
            Status = CatalogOrderStatuses.New,
            Note = note,
            PriceListNo = customer.PriceListNo!.Value,
            PriceIncludesVat = customer.IncludesVat,
            DiscountPercent = account.DiscountPercent,
            Total = quote.Totals.Total,
            LineCount = orderLines.Count,
            LinesJson = CatalogOrders.LinesJson(orderLines),
            AssignedUserId = assigned,
            SubmittedAtMs = now,
            UpdatedAtMs = now,
        };

        var relational = db.Database.IsRelational();
        try
        {
            await using var transaction = relational ? await db.Database.BeginTransactionAsync(ct) : null;
            // The account's row lock: two submits of one customer take turns, so the limit below holds.
            if (relational)
                await db.CatalogAccounts.Where(a => a.Id == account.Id).ExecuteUpdateAsync(s => s.SetProperty(a => a.UpdatedAtMs, a => a.UpdatedAtMs), ct);
            if (await ReplayAsync(db, body.RequestId, session, ct) is { } raced) return raced;
            var open = await db.CatalogOrders.CountAsync(o => o.TenantId == tenant.Id && o.AccountId == account.Id
                && (o.Status == CatalogOrderStatuses.New || o.Status == CatalogOrderStatuses.Claimed), ct);
            if (open >= options.Value.MaxOpenOrders)
                return Error(StatusCodes.Status429TooManyRequests, "TOO_MANY_OPEN_ORDERS",
                    $"Bekleyen {options.Value.MaxOpenOrders} talebiniz var; yenisi için öncekilerin işlenmesini bekleyin.");
            order.No = await CatalogOrders.FreeNumberAsync(db, tenant.Id, ct);
            db.CatalogOrders.Add(order);
            var seq = relational ? await MobileRecordProjector.ReserveAsync(db, tenant.Id, 1, ct) : now * 1000;
            CatalogOrders.AddNotifications(db, order, recipients, seq);
            await db.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            // The same id raced in from elsewhere (another account's, or a lost-answer retry on another connection).
            db.ChangeTracker.Clear();
            if (await ReplayAsync(db, body.RequestId, session, ct) is { } winner) return winner;
            throw;
        }

        events.Publish(tenant.Id, TenantEventTopics.Tasks);
        return JsonResults.Status(StatusCodes.Status201Created, new CatalogOrderResponse { Order = Fill(new CatalogCustomerOrderDto(), order) });
    }

    private static async Task<IResult> ListAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var session = CatalogSession.Of(http);
        var orders = await db.CatalogOrders.AsNoTracking()
            .Where(o => o.TenantId == session.Tenant.Id && o.AccountId == session.Account.Id)
            .OrderByDescending(o => o.SubmittedAtMs)
            .Take(MaxListed)
            .Select(o => new CatalogOrder
            {
                Id = o.Id,
                No = o.No,
                Status = o.Status,
                Total = o.Total,
                LineCount = o.LineCount,
                SubmittedAtMs = o.SubmittedAtMs,
                RejectReason = o.RejectReason,
            })
            .ToListAsync(ct);
        return JsonResults.Ok(new CatalogCustomerOrdersResponse { Items = [.. orders.Select(o => Fill(new CatalogCustomerOrderDto(), o))] });
    }

    private static async Task<IResult> DetailAsync(HttpContext http, string? id, [FromServices] CentralApiDbContext db, [FromServices] IOptions<StorageOptions> storage,
        CancellationToken ct)
    {
        var session = CatalogSession.Of(http);
        var order = Guid.TryParse(id, out var orderId)
            ? await db.CatalogOrders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId && o.TenantId == session.Tenant.Id && o.AccountId == session.Account.Id, ct)
            : null;
        if (order is null) return Error(StatusCodes.Status404NotFound, "NOT_FOUND", "Talep bulunamadı.");
        var detail = Fill(new CatalogCustomerOrderDetailDto(), order);
        detail.Note = order.Note;
        var lines = CatalogOrders.Lines(order);
        var codes = lines.Select(l => l.StockCode).ToArray();
        // Only pictures of this account's own order, within its tenant; no blob bytes or per-line queries.
        var pictures = await db.CatalogImages.AsNoTracking()
            .Where(i => i.TenantId == session.Tenant.Id && codes.Contains(i.StockCode))
            .OrderBy(i => i.SortOrder).ThenBy(i => i.CreatedAtMs).ThenBy(i => i.Id).ToListAsync(ct);
        var urls = await CatalogFileUrls.LoadAsync(db, storage.Value, session.Tenant.Id, pictures, ct);
        var thumbs = pictures.GroupBy(i => i.StockCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(i => CatalogImages.ThumbUrl(i, urls)).FirstOrDefault(url => url is not null), StringComparer.OrdinalIgnoreCase);
        detail.Lines = [.. lines.Select(l => new CatalogCustomerOrderLineDto
        {
            Thumb = thumbs.GetValueOrDefault(l.StockCode),
            Key = l.StockCode,
            Code = l.StockCode,
            Name = l.Name,
            Quantity = l.Quantity,
            Net = l.Net,
            Total = l.Total,
        })];
        return JsonResults.Ok(detail);
    }

    /// <summary>
    /// The request already made with this id: the same 201 for its own account; another account's (or company's) is a
    /// 409 that says nothing about it. Null when the id is new.
    /// </summary>
    private static async Task<IResult?> ReplayAsync(CentralApiDbContext db, Guid requestId, CatalogSession session, CancellationToken ct)
    {
        var existing = await db.CatalogOrders.AsNoTracking()
            .Where(o => o.Id == requestId)
            .Select(o => new CatalogOrder
            {
                Id = o.Id,
                TenantId = o.TenantId,
                AccountId = o.AccountId,
                No = o.No,
                Status = o.Status,
                Total = o.Total,
                LineCount = o.LineCount,
                SubmittedAtMs = o.SubmittedAtMs,
                RejectReason = o.RejectReason,
            })
            .FirstOrDefaultAsync(ct);
        if (existing is null) return null;
        if (existing.TenantId != session.Tenant.Id || existing.AccountId != session.Account.Id)
            return Error(StatusCodes.Status409Conflict, "REQUEST_ID_CONFLICT", "Bu talep kimliği kullanılamaz; sayfayı yenileyip yeniden gönderin.");
        return JsonResults.Status(StatusCodes.Status201Created, new CatalogOrderResponse { Order = Fill(new CatalogCustomerOrderDto(), existing) });
    }

    private static T Fill<T>(T dto, CatalogOrder o) where T : CatalogCustomerOrderDto
    {
        dto.Id = o.Id;
        dto.No = o.No;
        dto.Status = o.Status;
        dto.Total = o.Total;
        dto.LineCount = o.LineCount;
        dto.SubmittedAtMs = o.SubmittedAtMs;
        dto.RejectReason = o.RejectReason;
        return dto;
    }

    private static IResult OrderingDisabled() =>
        Error(StatusCodes.Status403Forbidden, "ORDERING_DISABLED", "Bu hesaptan sipariş talebi gönderilemiyor; firmanızla görüşün.");

    private static IResult QuoteError(int status, string code, string message, CatalogQuoteDto quote) =>
        JsonResults.Status(status, new CatalogQuoteErrorDto { ErrorCode = code, Message = message, Quote = quote });
}
