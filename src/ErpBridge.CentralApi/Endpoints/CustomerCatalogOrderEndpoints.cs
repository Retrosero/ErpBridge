using System.Globalization;
using ErpBridge.CentralApi.Approvals;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Jobs;
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
/// winner and one 409. A request becomes a sale on the phone (the sale carries <c>catalogOrderId</c>,
/// <see cref="CatalogOrderLinker"/>) or here, <c>convert</c> (S10), which builds the phone's sale itself; <c>complete</c>
/// records one entered elsewhere.
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
        group.MapGet("/counts", CountsAsync).WithName("CustomerCatalogOrderCounts");
        group.MapGet("/{id:guid}", GetAsync).WithName("CustomerCatalogOrderGet");
        group.MapPost("/{id:guid}/claim", ClaimAsync).WithName("CustomerCatalogOrderClaim");
        group.MapPost("/{id:guid}/release", ReleaseAsync).WithName("CustomerCatalogOrderRelease");
        group.MapPost("/{id:guid}/complete", CompleteAsync).WithName("CustomerCatalogOrderComplete");
        group.MapPost("/{id:guid}/reject", RejectAsync).WithName("CustomerCatalogOrderReject");
        group.MapPost("/{id:guid}/reopen", ReopenAsync).WithName("CustomerCatalogOrderReopen");
        group.MapGet("/{id:guid}/conversion", ConversionAsync).WithName("CustomerCatalogOrderConversion");
        group.MapPost("/{id:guid}/convert", ConvertAsync).WithName("CustomerCatalogOrderConvert");
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

    /// <summary>
    /// The list's <c>counts</c> alone, counted in the database: the panel menu's "new requests" badge reads it on every page
    /// change and once a minute, so it must not load the requests themselves.
    /// </summary>
    private static async Task<IResult> CountsAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: false, ct);
        if (access.Error is not null) return access.Error;
        var user = access.User!;
        var query = db.CatalogOrders.AsNoTracking().Where(o => o.TenantId == access.Tenant!.Id);
        if (!RolePermissions.CanManageCustomerCatalog(user)) query = query.Where(o => o.AssignedUserId == user.Id || o.ClaimedByUserId == user.Id);
        var byStatus = await query.GroupBy(o => o.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync(ct);
        int Of(string status) => byStatus.FirstOrDefault(s => s.Status == status)?.Count ?? 0;
        return JsonResults.Ok(new CatalogOrderCountsDto
        {
            New = Of(CatalogOrderStatuses.New),
            Claimed = Of(CatalogOrderStatuses.Claimed),
            Completed = Of(CatalogOrderStatuses.Completed),
            Rejected = Of(CatalogOrderStatuses.Rejected),
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
    /// Opens a closed request again (<c>COMPLETED</c>/<c>REJECTED</c> → <c>NEW</c>): a mistaken "entered elsewhere" or
    /// rejection, or a sale that never reached the ERP. Its document, reason, closer and taker are cleared, so the next
    /// sale made from it links as on the first day. Catalog managers only; an open request stays as it is.
    /// </summary>
    private static Task<IResult> ReopenAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, CancellationToken ct) =>
        ChangeAsync(id, http, db, views, (order, _, manage, now) =>
        {
            if (!manage)
                return Error(StatusCodes.Status403Forbidden, "CATALOG_MANAGE_REQUIRED", "Kapanmış talebi yalnız katalog yöneticisi yeniden açabilir.");
            if (CatalogOrderStatuses.IsOpen(order.Status)) return null;
            order.Status = CatalogOrderStatuses.New;
            order.DocumentRef = null;
            order.RejectReason = null;
            order.ClosedByUserId = null;
            order.ClosedByName = null;
            order.ClosedAtMs = null;
            order.ClaimedByUserId = null;
            order.ClaimedByName = null;
            order.ClaimedAtMs = null;
            order.UpdatedAtMs = now;
            return null;
        }, ct);

    /// <summary>What "Siparişe çevir" would send now (<see cref="CatalogOrderConversion"/>): any request the caller sees.</summary>
    private static async Task<IResult> ConversionAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: false, ct);
        if (access.Error is not null) return access.Error;
        var order = await db.CatalogOrders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id && o.TenantId == access.Tenant!.Id, ct);
        if (order is null || !Sees(order, access.User!)) return NotFound();
        var plan = await CatalogOrderConversion.PlanAsync(db, views, access.Tenant!, order, access.User!, warehouseNo: null, ct);
        return JsonResults.Ok(plan.Preview);
    }

    /// <summary>
    /// Turns the request into the phone's sale (S10): a catalog manager, or whoever has it — the assignee takes a new one
    /// first. Today's prices must be the ones the form showed (<c>expectedTotal</c>, else 409 <c>PRICE_CHANGED</c> with the
    /// new preview); a line no longer sold from the request's list is 422 <c>CART_INVALID</c>; an owner the agent could not
    /// write for is 409 <c>ERP_MAPPING_MISSING</c> before any job exists. A caller who decides sales writes the job (or
    /// books it, without an ERP) through <see cref="SalesJobWriter"/>; anyone else, when the company's rule or their own
    /// limits say so, sends it to the approval queue, and the request is completed when it is approved. A second document
    /// for the request is refused by the linker (409 <c>CATALOG_ORDER_ALREADY_CONVERTED</c>).
    /// </summary>
    private static async Task<IResult> ConvertAsync(Guid id, HttpContext http, [FromBody] CatalogOrderConvertRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, [FromServices] SalesJobWriter writer,
        [FromServices] ApprovalService approvals, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: false, ct);
        if (access.Error is not null) return access.Error;
        if (body?.ExpectedTotal is not { } expected) return InvalidBody("expectedTotal gerekli: önizlemedeki toplam.");
        if (body.WarehouseNo is <= 0) return InvalidBody("warehouseNo pozitif bir depo numarası olmalı.");
        var tenant = access.Tenant!;
        var user = access.User!;
        var manage = RolePermissions.CanManageCustomerCatalog(user);

        var order = await db.CatalogOrders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenant.Id, ct);
        if (order is null || !Sees(order, user)) return NotFound();
        if (order.Status == CatalogOrderStatuses.Claimed && order.ClaimedByUserId != user.Id && !manage) return Taken(order);

        // The form's warehouse counts for an ERP company only; the native ledger has no warehouse choice.
        var warehouseNo = tenant.DataSource == TenantDataSources.Erp ? body.WarehouseNo : null;
        var plan = await CatalogOrderConversion.PlanAsync(db, views, tenant, order, user, warehouseNo, ct);
        var preview = plan.Preview;
        if (preview.Lines.Any(l => l.Issue is not null))
            return ConversionError(StatusCodes.Status422UnprocessableEntity, "CART_INVALID",
                "Talepteki bazı ürünler artık bu fiyat listesinden satılamıyor; talebi düzeltin ya da reddedin.", preview);
        if (Math.Abs(preview.Total - expected) > CatalogCustomerOrderEndpoints.PriceTolerance)
            return ConversionError(StatusCodes.Status409Conflict, "PRICE_CHANGED", "Fiyatlar değişti; güncel tutarı kontrol edip yeniden onaylayın.", preview);
        if (preview.MissingMappings.Length > 0)
            return ConversionError(StatusCodes.Status409Conflict, "ERP_MAPPING_MISSING",
                $"Belge {preview.OwnerName} adına kesilecek; ERP eşlemesinde eksik: {string.Join(", ", preview.MissingMappings)}. Kullanıcının Mikro eşlemesini ya da ERP ayarlarını tamamlayın.",
                preview);

        // A new request is taken in the caller's name first, as "İşleme al" would.
        if (order.Status == CatalogOrderStatuses.New && await ClaimForConversionAsync(db, tenant.Id, id, user, manage, ct) is { } refused)
            return refused;

        var now = DateTimeOffset.UtcNow;
        var externalId = CatalogOrderConversion.DocumentPrefix + Guid.NewGuid().ToString("D");
        // Only another warehouse than the default goes in the body, as on the phone.
        var chosenWarehouse = warehouseNo is { } chosen && chosen != preview.DefaultWarehouseNo ? warehouseNo : null;
        var payloadJson = CatalogOrderConversion.PayloadJson(plan, order, externalId, chosenWarehouse, now);
        var correlationId = ErpBridge.CentralApi.LogCenter.CorrelationId.Of(http);
        var response = new CatalogOrderConvertResponse { DocumentRef = externalId };

        if (await CatalogOrderConversion.ApprovalReasonAsync(db, tenant.Id, user, payloadJson, ct) is { } reason)
        {
            var approvalPayload = CatalogOrderConversion.ApprovalPayloadJson(plan, order, externalId, payloadJson, reason);
            var submitted = await approvals.SubmitAsync(db, tenant, user, CatalogOrderConversion.ApprovalPrefix + Guid.NewGuid().ToString("D"), approvalPayload, ct, correlationId);
            if (!submitted.Succeeded) return JsonResults.Status(submitted.StatusCode, submitted.Error);
            response.Outcome = "APPROVAL";
            response.ApprovalRequestId = submitted.Value!.Id;
        }
        else
        {
            var job = new Job
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                ExternalId = externalId,
                DocumentType = CatalogOrderLinker.SalesOrderType,
                PayloadJson = payloadJson,
                Status = JobStatus.Pending,
                EnqueuedAtUtc = now,
                // The customer's salesperson's document, else the approver's (the user's decision): the agent writes it
                // with that person's mapping, and the reports count it as theirs.
                CreatedByUserId = plan.Owner.Id,
                CorrelationId = correlationId,
            };
            // The caller completes the request: they have it, or they manage the catalog, as the linker requires.
            var written = await writer.WriteAsync(db, tenant, job, user.Id, callerIsAdmin: false, ct);
            if (written.Refusal is { } linkRefusal) return Error(linkRefusal.Status, linkRefusal.Code, linkRefusal.Message);
            if (written.Job!.Status == JobStatus.Failed)
                return Error(StatusCodes.Status422UnprocessableEntity, "CATALOG_CONVERSION_FAILED",
                    "Satış deftere işlenemedi; talep açık kaldı. Cari ve ürün kartlarını kontrol edin.");
            response.Outcome = "JOB";
            response.JobId = written.Job.Id;
            response.JobStatus = written.Job.Status.ToString();
        }

        db.ChangeTracker.Clear();
        var after = await db.CatalogOrders.AsNoTracking().FirstAsync(o => o.Id == id, ct);
        response.Order = await DetailAsync(db, views, after, ct);
        return JsonResults.Status(StatusCodes.Status201Created, response);
    }

    /// <summary>Takes a new request for the caller under its row lock; null when it is theirs now (or a manager may convert it anyway).</summary>
    private static async Task<IResult?> ClaimForConversionAsync(CentralApiDbContext db, Guid tenantId, Guid id, MobileUser user, bool manage, CancellationToken ct)
    {
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        await CatalogOrders.LockAsync(db, tenantId, id, ct);
        var order = await db.CatalogOrders.FirstAsync(o => o.Id == id && o.TenantId == tenantId, ct);
        if (order.Status == CatalogOrderStatuses.Claimed && order.ClaimedByUserId != user.Id && !manage) return Taken(order);
        if (order.Status == CatalogOrderStatuses.New)
        {
            var now = NowMs();
            order.Status = CatalogOrderStatuses.Claimed;
            order.ClaimedByUserId = user.Id;
            order.ClaimedByName = NameOf(user);
            order.ClaimedAtMs = now;
            order.UpdatedAtMs = now;
            await db.SaveChangesAsync(ct);
        }
        if (transaction is not null) await transaction.CommitAsync(ct);
        // Anything else (closed meanwhile) is the linker's to answer.
        return null;
    }

    private static IResult ConversionError(int status, string code, string message, CatalogOrderConversionDto conversion) =>
        JsonResults.Status(status, new CatalogOrderConversionErrorDto { ErrorCode = code, Message = message, Conversion = conversion });

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
