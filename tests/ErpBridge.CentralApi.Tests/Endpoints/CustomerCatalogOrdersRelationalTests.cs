using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static ErpBridge.CentralApi.Tests.Endpoints.CatalogCustomerTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU §5.1–§5.3 / S8: a customer's cart becomes an order request priced again by the server; the
/// staff it is routed to (responsible user or salesperson mapping) and the catalog managers are told; staff take it,
/// give it back, reject or complete it; a sale carrying <c>catalogOrderId</c> completes it at ingest or approval, and a
/// second sale for the same request is refused before any job is written. Seed as in
/// <see cref="CustomerCatalogBrowseRelationalTests"/>: A "Çay Rize" in stock, 100 VAT included (10 %), carton 12; B
/// out of stock. Customer C1's salesperson code is P1.
/// </summary>
public sealed class CustomerCatalogOrdersRelationalTests : IClassFixture<CatalogHostFactory>
{
    private const string Pass = "musteri123";

    private readonly CatalogHostFactory _factory;

    public CustomerCatalogOrdersRelationalTests(CatalogHostFactory factory) => _factory = factory;

    [Fact]
    public async Task The_server_prices_the_request_again_and_refuses_a_changed_price_or_an_invalid_cart()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass, discountPercent: 10m);
        var browser = await SignedInAsync(c, "yilmaz");

        // 12 × 100 VAT included at 10 % less 10 % is 1080.00 (ErpSalePricing); the page saw 6 kuruş more.
        var changed = await SubmitAsync(browser, c, Guid.NewGuid(), 1080.06m, ("A", 12m));
        changed.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var priceError = await changed.ReadAsJsonAsync<CatalogQuoteErrorDto>();
        priceError.ErrorCode.Should().Be("PRICE_CHANGED");
        priceError.Quote.Totals.Total.Should().Be(1080.00m);
        (await ReadAsync(_factory, db => db.CatalogOrders.CountAsync(o => o.TenantId == c.Id))).Should().Be(0);

        var created = await OkAsync<CatalogOrderResponse>(await SubmitAsync(browser, c, Guid.NewGuid(), 1080.05m, ("a", 12m)), HttpStatusCode.Created);
        created.Order.Should().Match<CatalogCustomerOrderDto>(o => o.Status == "NEW" && o.Total == 1080.00m && o.LineCount == 1 && o.RejectReason == null);
        created.Order.No.Should().MatchRegex("^KT-[A-HJ-NP-Z2-9]{6}$");
        var row = await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == created.Order.Id));
        row.Should().Match<CatalogOrder>(o => o.CustomerCode == "C1" && o.AccountUsername == "yilmaz" && o.PriceListNo == 1 && o.PriceIncludesVat
            && o.DiscountPercent == 10m && o.Note == "kapıya bırakın");
        CatalogOrders.Lines(row).Should().ContainSingle().Which.Should().Be(
            new CatalogOrderLine("A", "Çay Rize", "KG", 12m, 12, 100m, 10m, 90m, 10m, 1090.91m, 109.09m, 98.18m, 1080.00m));

        await EditProductsAsync(c, new { stockCode = "A", sortOrder = (int?)null, hidden = false, noDiscount = false, cartonOnly = true, cartonQuantity = (int?)null });
        var notCartons = await SubmitAsync(browser, c, Guid.NewGuid(), 450m, ("A", 5m));
        notCartons.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var cartError = await notCartons.ReadAsJsonAsync<CatalogQuoteErrorDto>();
        cartError.ErrorCode.Should().Be("CART_INVALID");
        cartError.Quote.Lines.Single().Issue.Should().Be("CARTON_MULTIPLE");
        (await SubmitAsync(browser, c, Guid.NewGuid(), 45m, ("B", 1m))).StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity, "out of stock");

        await ShouldFailAsync(await SendAsync(browser, HttpMethod.Post, Api(c) + "/orders", new { requestId = Guid.NewGuid(), lines = Array.Empty<object>(), expectedTotal = 0 }),
            HttpStatusCode.BadRequest, "INVALID_BODY");
        await ShouldFailAsync(await SendAsync(browser, HttpMethod.Post, Api(c) + "/orders", new
        {
            requestId = Guid.NewGuid(),
            lines = Enumerable.Range(0, 201).Select(_ => new { key = "A", quantity = 12m }).ToArray(),
            expectedTotal = 0,
        }), HttpStatusCode.BadRequest, "INVALID_BODY");
        (await ReadAsync(_factory, db => db.CatalogOrders.CountAsync(o => o.TenantId == c.Id))).Should().Be(1);
    }

    [Fact]
    public async Task The_same_request_id_makes_one_request_and_is_nobody_elses()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        await AccountAsync(_factory, c, "C2", "akgida", Pass);
        var browser = await SignedInAsync(c, "yilmaz");
        var requestId = Guid.NewGuid();

        var responses = await Task.WhenAll(SubmitAsync(browser, c, requestId, 100m, ("A", 1m)), SubmitAsync(browser, c, requestId, 100m, ("A", 1m)));
        var first = await OkAsync<CatalogOrderResponse>(responses[0], HttpStatusCode.Created);
        var second = await OkAsync<CatalogOrderResponse>(responses[1], HttpStatusCode.Created);
        second.Order.Should().BeEquivalentTo(first.Order);
        (await OkAsync<CatalogOrderResponse>(await SubmitAsync(browser, c, requestId, 100m, ("A", 1m)), HttpStatusCode.Created)).Order.No.Should().Be(first.Order.No);
        (await ReadAsync(_factory, db => db.CatalogOrders.CountAsync(o => o.TenantId == c.Id))).Should().Be(1);
        (await ReadAsync(_factory, db => db.UserNotifications.CountAsync(n => n.TenantId == c.Id))).Should().Be(2, "patron and mudur, once");

        var other = await SignedInAsync(c, "akgida");
        var taken = await SubmitAsync(other, c, requestId, 100m, ("A", 1m));
        taken.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await taken.Content.ReadAsStringAsync();
        body.Should().Contain("REQUEST_ID_CONFLICT").And.NotContain(first.Order.No);
        await ShouldFailAsync(await GetAsync(other, $"{Api(c)}/orders/detail?id={requestId}"), HttpStatusCode.NotFound, "NOT_FOUND");
        (await OkAsync<CatalogCustomerOrdersResponse>(await GetAsync(other, Api(c) + "/orders"))).Items.Should().BeEmpty();

        var mine = await OkAsync<CatalogCustomerOrdersResponse>(await GetAsync(browser, Api(c) + "/orders"));
        mine.Items.Should().ContainSingle().Which.Id.Should().Be(requestId);
        var detail = await OkAsync<CatalogCustomerOrderDetailDto>(await GetAsync(browser, $"{Api(c)}/orders/detail?id={requestId}"));
        detail.Lines.Should().ContainSingle().Which.Should().BeEquivalentTo(new CatalogCustomerOrderLineDto { Key = "A", Code = "A", Name = "Çay Rize", Quantity = 1m, Net = 100m, Total = 100m });
    }

    [Fact]
    public async Task A_request_goes_to_the_customers_salesperson_and_the_managers()
    {
        var c = await OpenCatalogAsync(_factory);
        await SeedAsync(_factory, db => db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = c.AliId, TenantId = c.Id, SalespersonCode = " p1 " }));
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        await AccountAsync(_factory, c, "C2", "akgida", Pass);
        var users = await ReadAsync(_factory, db => db.MobileUsers.AsNoTracking().Where(u => u.TenantId == c.Id).ToDictionaryAsync(u => u.Username, u => u.Id));
        var hub = _factory.Services.GetRequiredService<ITenantEventHub>();
        var tasksBefore = hub.Version(c.Id, TenantEventTopics.Tasks);

        var order = (await OkAsync<CatalogOrderResponse>(await SubmitAsync(await SignedInAsync(c, "yilmaz"), c, Guid.NewGuid(), 1200m, ("A", 12m)), HttpStatusCode.Created)).Order;

        hub.Version(c.Id, TenantEventTopics.Tasks).Should().BeGreaterThan(tasksBefore, "the phones are woken");
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == order.Id))).AssignedUserId.Should().Be(c.AliId);
        var notices = await ReadAsync(_factory, db => db.UserNotifications.AsNoTracking().Where(n => n.TenantId == c.Id).ToListAsync());
        notices.Select(n => n.UserId).Should().BeEquivalentTo(new[] { c.AliId, users["patron"], users["mudur"] });
        notices.Should().AllSatisfy(n =>
        {
            n.Kind.Should().Be("CATALOG_ORDER_NEW");
            n.TaskId.Should().BeNull();
            n.Title.Should().Be("Yeni müşteri siparişi: Yılmaz Market Ltd. Şti.");
            n.Body.Should().Be($"{order.No} · 1 kalem · 1.200,00 TL");
            n.Seq.Should().BeGreaterThan(0);
        });

        // The phone's notification feed carries it as it is: no task, its own kind.
        var feed = await OkAsync<UserNotificationListResponse>(await SendAsync(_factory, HttpMethod.Get, "/api/v1/android/notifications/?changedSinceSeq=0", c.Ali));
        feed.Notifications.Should().ContainSingle().Which.Should().Match<UserNotificationDto>(n => n.Kind == "CATALOG_ORDER_NEW" && n.TaskId == null);
        feed.UnreadCount.Should().Be(1);

        // Without a salesperson the request goes to the managers alone.
        var second = (await OkAsync<CatalogOrderResponse>(await SubmitAsync(await SignedInAsync(c, "akgida"), c, Guid.NewGuid(), 100m, ("A", 1m)), HttpStatusCode.Created)).Order;
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == second.Id))).AssignedUserId.Should().BeNull();
        (await ReadAsync(_factory, db => db.UserNotifications.CountAsync(n => n.TenantId == c.Id))).Should().Be(5);

        // The salesperson sees their own, the managers all, accounting none.
        (await OkAsync<CatalogOrderListResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/orders", c.Ali))).Items.Select(o => o.Id).Should().Equal(order.Id);
        var all = await OkAsync<CatalogOrderListResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/orders", c.Mudur));
        all.Items.Select(o => o.Id).Should().Equal(second.Id, order.Id);
        all.Counts.Should().BeEquivalentTo(new CatalogOrderCountsDto { New = 2 });
        all.Items[1].Should().Match<CatalogOrderSummaryDto>(o => o.CustomerCode == "C1" && o.AssignedUserName == "ali bey" && o.Total == 1200m && o.Status == "NEW");
        (await OkAsync<CatalogOrderListResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/orders?q=ak gıda", c.Mudur))).Items.Select(o => o.Id).Should().Equal(second.Id);
        (await OkAsync<CatalogOrderListResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/orders", c.Muhasebe))).Total.Should().Be(0);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, $"{Base}/orders/{second.Id}", c.Ali), HttpStatusCode.NotFound, "CATALOG_ORDER_NOT_FOUND");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Base + "/orders?status=OPEN", c.Mudur), HttpStatusCode.BadRequest, "INVALID_BODY");

        var detail = await OkAsync<CatalogOrderDetailDto>(await SendAsync(_factory, HttpMethod.Get, $"{Base}/orders/{order.Id}", c.Ali));
        detail.Should().Match<CatalogOrderDetailDto>(d => d.PriceListNo == 1 && d.PriceListName == "Perakende" && d.PriceIncludesVat && d.DiscountPercent == 0m);
        detail.Lines.Should().ContainSingle().Which.Should().Match<CatalogOrderLineDto>(l => l.StockCode == "A" && l.Quantity == 12m && l.CartonQuantity == 12
            && l.ListPrice == 100m && l.VatRate == 10m && l.Total == 1200m && l.InStockNow);

        var company = await CompanyAsync(_factory, withModule: false);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Base + "/orders", company.Patron), HttpStatusCode.Forbidden, "MODULE_NOT_ENABLED");
    }

    [Fact]
    public async Task One_person_takes_a_request_and_only_a_manager_takes_it_over()
    {
        var c = await OpenCatalogAsync(_factory);
        await SeedAsync(_factory, db => db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = c.AliId, TenantId = c.Id, SalespersonCode = "P1" }));
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var browser = await SignedInAsync(c, "yilmaz");
        var id = (await OkAsync<CatalogOrderResponse>(await SubmitAsync(browser, c, Guid.NewGuid(), 100m, ("A", 1m)), HttpStatusCode.Created)).Order.Id;

        var claims = await Task.WhenAll(ClaimAsync(c, c.Ali, id), ClaimAsync(c, c.Mudur, id));
        claims.Count(r => r.StatusCode == HttpStatusCode.OK).Should().Be(1);
        claims.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(1);
        // Whoever lost asks again: the winner keeps it, the other is told who has it.
        var winner = claims[0].StatusCode == HttpStatusCode.OK ? c.Ali : c.Mudur;
        var loser = winner == c.Ali ? c.Mudur : c.Ali;
        (await ClaimAsync(c, winner, id)).StatusCode.Should().Be(HttpStatusCode.OK, "taking one's own again changes nothing");
        await ShouldFailAsync(await ClaimAsync(c, loser, id), HttpStatusCode.Conflict, "CATALOG_ORDER_TAKEN");
        if (winner == c.Mudur) (await OkAsync<CatalogOrderDetailDto>(await PostAsync(c, c.Mudur, id, "release"))).Status.Should().Be("NEW");

        if (winner == c.Mudur) (await ClaimAsync(c, c.Ali, id)).StatusCode.Should().Be(HttpStatusCode.OK);
        var mine = await OkAsync<CatalogOrderDetailDto>(await SendAsync(_factory, HttpMethod.Get, $"{Base}/orders/{id}", c.Ali));
        mine.Should().Match<CatalogOrderDetailDto>(d => d.Status == "CLAIMED" && d.ClaimedByName == "ali bey" && d.ClaimedAtMs != null);
        (await OkAsync<CatalogCustomerOrdersResponse>(await GetAsync(browser, Api(c) + "/orders"))).Items.Single().Status.Should().Be("CLAIMED");

        await ShouldFailAsync(await ClaimAsync(c, c.Mudur, id), HttpStatusCode.Conflict, "CATALOG_ORDER_TAKEN");
        await ShouldFailAsync(await ClaimAsync(c, c.Ali, id, force: true), HttpStatusCode.Forbidden, "CATALOG_MANAGE_REQUIRED");
        (await OkAsync<CatalogOrderDetailDto>(await ClaimAsync(c, c.Mudur, id, force: true))).ClaimedByName.Should().Be("mudur bey");
        await ShouldFailAsync(await PostAsync(c, c.Ali, id, "release"), HttpStatusCode.Conflict, "CATALOG_ORDER_TAKEN");
        (await OkAsync<CatalogOrderDetailDto>(await PostAsync(c, c.Mudur, id, "release"))).Should().Match<CatalogOrderDetailDto>(d => d.Status == "NEW" && d.ClaimedByUserId == null);

        await ShouldFailAsync(await PostAsync(c, c.Ali, id, "reject", new { reason = "  " }), HttpStatusCode.BadRequest, "INVALID_BODY");
        (await OkAsync<CatalogOrderDetailDto>(await PostAsync(c, c.Ali, id, "reject", new { reason = "Stok yetersiz" })))
            .Should().Match<CatalogOrderDetailDto>(d => d.Status == "REJECTED" && d.RejectReason == "Stok yetersiz" && d.ClosedByName == "ali bey" && d.ClosedAtMs != null);
        await ShouldFailAsync(await ClaimAsync(c, c.Mudur, id), HttpStatusCode.Conflict, "CATALOG_ORDER_CLOSED");
        await ShouldFailAsync(await PostAsync(c, c.Mudur, id, "complete", new { documentRef = "X" }), HttpStatusCode.Conflict, "CATALOG_ORDER_CLOSED");
        (await OkAsync<CatalogCustomerOrdersResponse>(await GetAsync(browser, Api(c) + "/orders"))).Items.Single()
            .Should().Match<CatalogCustomerOrderDto>(o => o.Status == "REJECTED" && o.RejectReason == "Stok yetersiz");

        var other = (await OkAsync<CatalogOrderResponse>(await SubmitAsync(browser, c, Guid.NewGuid(), 100m, ("A", 1m)), HttpStatusCode.Created)).Order.Id;
        (await OkAsync<CatalogOrderDetailDto>(await PostAsync(c, c.Mudur, other, "complete", new { documentRef = "MOB-SO-9" })))
            .Should().Match<CatalogOrderDetailDto>(d => d.Status == "COMPLETED" && d.DocumentRef == "MOB-SO-9" && d.ClosedByName == "mudur bey");
        (await PostAsync(c, c.Ali, other, "complete", new { documentRef = "MOB-SO-9" })).StatusCode.Should().Be(HttpStatusCode.OK, "the same document again");
        await ShouldFailAsync(await PostAsync(c, c.Mudur, other, "complete", new { documentRef = "MOB-SO-10" }), HttpStatusCode.Conflict, "CATALOG_ORDER_ALREADY_CONVERTED");
        await ShouldFailAsync(await PostAsync(c, c.Mudur, other, "reject", new { reason = "geç" }), HttpStatusCode.Conflict, "CATALOG_ORDER_CLOSED");
        await ShouldFailAsync(await ClaimAsync(c, c.Mudur, Guid.NewGuid()), HttpStatusCode.NotFound, "CATALOG_ORDER_NOT_FOUND");
    }

    [Fact]
    public async Task A_sale_carrying_the_request_completes_it_and_a_second_sale_for_it_is_refused_without_a_job()
    {
        var c = await OpenCatalogAsync(_factory);
        await SalesWithoutApprovalAsync(c);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var browser = await SignedInAsync(c, "yilmaz");
        var id = (await OkAsync<CatalogOrderResponse>(await SubmitAsync(browser, c, Guid.NewGuid(), 100m, ("A", 1m)), HttpStatusCode.Created)).Order.Id;
        (await ClaimAsync(c, c.Mudur, id)).StatusCode.Should().Be(HttpStatusCode.OK);

        await OkAsync<IngestJobResponse>(await IngestAsync(c, c.Ali, "MOB-SO-K1", id), HttpStatusCode.Created);
        var row = await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == id));
        row.Should().Match<CatalogOrder>(o => o.Status == "COMPLETED" && o.DocumentRef == "MOB-SO-K1" && o.ClosedByUserId == c.AliId && o.ClosedByName == "ali bey");

        (await IngestAsync(c, c.Ali, "MOB-SO-K1", id)).StatusCode.Should().Be(HttpStatusCode.OK, "the same sale again is the same job");
        await ShouldFailAsync(await IngestAsync(c, c.Mudur, "MOB-SO-K2", id), HttpStatusCode.Conflict, "CATALOG_ORDER_ALREADY_CONVERTED");
        (await JobsAsync(c, "MOB-SO-K2")).Should().Be(0);
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == id))).DocumentRef.Should().Be("MOB-SO-K1");

        await ShouldFailAsync(await IngestAsync(c, c.Ali, "MOB-SO-K3", Guid.NewGuid()), HttpStatusCode.Conflict, "CATALOG_ORDER_NOT_FOUND");
        await ShouldFailAsync(await IngestAsync(c, c.Ali, "MOB-SO-K4", "not-an-id"), HttpStatusCode.Conflict, "CATALOG_ORDER_NOT_FOUND");
        var rejected = (await OkAsync<CatalogOrderResponse>(await SubmitAsync(browser, c, Guid.NewGuid(), 100m, ("A", 1m)), HttpStatusCode.Created)).Order.Id;
        (await PostAsync(c, c.Mudur, rejected, "reject", new { reason = "yok" })).StatusCode.Should().Be(HttpStatusCode.OK);
        await ShouldFailAsync(await IngestAsync(c, c.Ali, "MOB-SO-K5", rejected), HttpStatusCode.Conflict, "CATALOG_ORDER_CLOSED");
        (await JobsAsync(c, "MOB-SO-K3", "MOB-SO-K4", "MOB-SO-K5")).Should().Be(0);

        // Another company's request is not found from here.
        var elsewhere = await OpenCatalogAsync(_factory);
        await SalesWithoutApprovalAsync(elsewhere);
        await ShouldFailAsync(await IngestAsync(elsewhere, elsewhere.Ali, "MOB-SO-K6", rejected), HttpStatusCode.Conflict, "CATALOG_ORDER_NOT_FOUND");

        // Without the field (or with null) a sale goes in exactly as before.
        (await SendAsync(_factory, HttpMethod.Post, "/api/v1/ingest/jobs", c.Ali, new { externalId = "MOB-SO-K7", documentType = "sales_order", payload = Sale("C1") }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await IngestAsync(c, c.Ali, "MOB-SO-K8", null)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await JobsAsync(c, "MOB-SO-K7", "MOB-SO-K8")).Should().Be(2);
    }

    [Fact]
    public async Task Without_an_ERP_the_booking_completes_the_request_and_a_refused_booking_leaves_it_open()
    {
        var c = await OpenCatalogAsync(_factory);
        await SeedAsync(_factory, db => db.Tenants.Single(t => t.Id == c.Id).DataSource = TenantDataSources.Native);
        await SalesWithoutApprovalAsync(c);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var id = (await OkAsync<CatalogOrderResponse>(await SubmitAsync(await SignedInAsync(c, "yilmaz"), c, Guid.NewGuid(), 100m, ("A", 1m)), HttpStatusCode.Created)).Order.Id;

        // The ledger refuses the sale (no such customer): the job is kept as failed, the request stays open for the corrected one.
        var refused = await OkAsync<IngestJobResponse>(await NativeSaleAsync(c, "MOB-SO-N1", "YOK", id), HttpStatusCode.Created);
        refused.Status.Should().Be("Failed");
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == id)))
            .Should().Match<CatalogOrder>(o => o.Status == "NEW" && o.DocumentRef == null && o.ClosedAtMs == null);

        (await OkAsync<IngestJobResponse>(await NativeSaleAsync(c, "MOB-SO-N2", "C1", id), HttpStatusCode.Created)).Status.Should().Be("Succeeded");
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == id)))
            .Should().Match<CatalogOrder>(o => o.Status == "COMPLETED" && o.DocumentRef == "MOB-SO-N2");
        await ShouldFailAsync(await NativeSaleAsync(c, "MOB-SO-N3", "C1", id), HttpStatusCode.Conflict, "CATALOG_ORDER_ALREADY_CONVERTED");
        (await JobsAsync(c, "MOB-SO-N3")).Should().Be(0);
    }

    [Fact]
    public async Task An_approved_sale_completes_the_request_and_a_rejected_one_leaves_it_open()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var browser = await SignedInAsync(c, "yilmaz");
        var approved = (await OkAsync<CatalogOrderResponse>(await SubmitAsync(browser, c, Guid.NewGuid(), 100m, ("A", 1m)), HttpStatusCode.Created)).Order.Id;
        var refused = (await OkAsync<CatalogOrderResponse>(await SubmitAsync(browser, c, Guid.NewGuid(), 100m, ("A", 1m)), HttpStatusCode.Created)).Order.Id;

        var first = await ApprovalAsync(c, "APR-K1", "MOB-SO-AP1", approved);
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == approved))).Status.Should().Be("NEW", "nothing happens before approval");
        (await DecideAsync(c, c.Patron, first, "approve")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == approved)))
            .Should().Match<CatalogOrder>(o => o.Status == "COMPLETED" && o.DocumentRef == "MOB-SO-AP1" && o.ClosedByUserId == c.AliId);
        (await JobsAsync(c, "MOB-SO-AP1")).Should().Be(1);

        var second = await ApprovalAsync(c, "APR-K2", "MOB-SO-AP2", refused);
        (await DecideAsync(c, c.Patron, second, "reject")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == refused))).Status.Should().Be("NEW");

        // An approval for a request another sale already completed is refused and stays pending; no job.
        var late = await ApprovalAsync(c, "APR-K3", "MOB-SO-AP3", approved);
        await ShouldFailAsync(await DecideAsync(c, c.Patron, late, "approve"), HttpStatusCode.Conflict, "CATALOG_ORDER_ALREADY_CONVERTED");
        (await JobsAsync(c, "MOB-SO-AP3")).Should().Be(0);
        (await ReadAsync(_factory, db => db.ApprovalRequests.AsNoTracking().SingleAsync(r => r.Id == late))).Status.Should().Be(ApprovalStatuses.Pending);
    }

    [Fact]
    public async Task Ordering_needs_the_accounts_right_an_unlocked_customer_and_room_for_another_open_request()
    {
        var c = await OpenCatalogAsync(_factory);
        var accountId = await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var browser = await SignedInAsync(c, "yilmaz");

        (await SendAsync(_factory, HttpMethod.Patch, $"{Base}/accounts/{accountId}", c.Mudur, new { canOrder = false })).StatusCode.Should().Be(HttpStatusCode.OK);
        await ShouldFailAsync(await SubmitAsync(browser, c, Guid.NewGuid(), 100m, ("A", 1m)), HttpStatusCode.Forbidden, "ORDERING_DISABLED");
        (await SendAsync(_factory, HttpMethod.Patch, $"{Base}/accounts/{accountId}", c.Mudur, new { canOrder = true })).StatusCode.Should().Be(HttpStatusCode.OK);

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await SeedAsync(_factory, db =>
        {
            for (var i = 0; i < 20; i++)
            {
                db.CatalogOrders.Add(new CatalogOrder
                {
                    Id = Guid.NewGuid(), TenantId = c.Id, AccountId = accountId, CustomerCode = "C1", CustomerName = "Yılmaz", AccountUsername = "yilmaz",
                    No = $"KT-OPEN{i:D2}", Status = i % 2 == 0 ? CatalogOrderStatuses.New : CatalogOrderStatuses.Claimed, SubmittedAtMs = now, UpdatedAtMs = now,
                });
            }
        });
        await ShouldFailAsync(await SubmitAsync(browser, c, Guid.NewGuid(), 100m, ("A", 1m)), HttpStatusCode.TooManyRequests, "TOO_MANY_OPEN_ORDERS");
        await SeedAsync(_factory, db => db.CatalogOrders.Single(o => o.No == "KT-OPEN00" && o.TenantId == c.Id).Status = CatalogOrderStatuses.Completed);
        (await SubmitAsync(browser, c, Guid.NewGuid(), 100m, ("A", 1m))).StatusCode.Should().Be(HttpStatusCode.Created);

        // A customer the ERP locked cannot send a request.
        await SeedAsync(_factory, db => Record(db, c.Id, "customers", "C3", new { customerCode = "C3", title1 = "Kilitli Ticaret", isLocked = true }));
        await AccountAsync(_factory, c, "C3", "kilitli", Pass);
        await ShouldFailAsync(await SubmitAsync(await SignedInAsync(c, "kilitli"), c, Guid.NewGuid(), 100m, ("A", 1m)), HttpStatusCode.Forbidden, "ORDERING_DISABLED");
    }

    private async Task<HttpClient> SignedInAsync(CatalogCompany company, string username)
    {
        var browser = Browser(_factory);
        var login = await LoginAsync(browser, company, username, Pass);
        login.StatusCode.Should().Be(HttpStatusCode.OK, await login.Content.ReadAsStringAsync());
        return browser;
    }

    private static Task<HttpResponseMessage> SubmitAsync(HttpClient browser, CatalogCompany company, Guid requestId, decimal expectedTotal, params (string Key, decimal Quantity)[] lines) =>
        SendAsync(browser, HttpMethod.Post, Api(company) + "/orders", new
        {
            requestId,
            lines = lines.Select(l => new { key = l.Key, quantity = l.Quantity }).ToArray(),
            note = " kapıya bırakın ",
            expectedTotal,
        });

    private Task<HttpResponseMessage> ClaimAsync(CatalogCompany company, string token, Guid id, bool force = false) =>
        PostAsync(company, token, id, "claim", new { force });

    private Task<HttpResponseMessage> PostAsync(CatalogCompany company, string token, Guid id, string action, object? body = null) =>
        SendAsync(_factory, HttpMethod.Post, $"{Base}/orders/{id}/{action}", token, body ?? new { });

    /// <summary>A cash sale: a salesperson may send it without approval (an on-account one needs the right).</summary>
    private static object Sale(string customerCode, object? catalogOrderId = null) => new
    {
        customerCode,
        paymentType = "Nakit",
        catalogOrderId,
        lines = new[] { new { stockCode = "A", quantity = 1, unitPrice = 100 } },
    };

    private Task<HttpResponseMessage> IngestAsync(CatalogCompany company, string token, string externalId, object? catalogOrderId) =>
        SendAsync(_factory, HttpMethod.Post, "/api/v1/ingest/jobs", token, new { externalId, documentType = "sales_order", payload = Sale("C1", catalogOrderId) });

    private async Task<Guid> ApprovalAsync(CatalogCompany company, string externalId, string saleExternalId, Guid catalogOrderId)
    {
        var response = await SendAsync(_factory, HttpMethod.Post, "/api/v1/ingest/jobs", company.Ali, new
        {
            externalId,
            documentType = "approval_request",
            payload = new
            {
                kind = "sale",
                counterpartyName = "Yılmaz Market",
                amount = 100,
                documents = new object[] { new { documentType = "sales_order", externalId = saleExternalId, payload = Sale("C1", catalogOrderId) } },
            },
        });
        return (await OkAsync<IngestJobResponse>(response, HttpStatusCode.Created)).JobId;
    }

    /// <summary>The company lets sales in without approval (its rule is on by default).</summary>
    private async Task SalesWithoutApprovalAsync(CatalogCompany company) =>
        (await SendAsync(_factory, HttpMethod.Put, "/api/v1/android/approvals/rules", company.Patron, new { rules = new { sale = false } }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

    private Task<HttpResponseMessage> DecideAsync(CatalogCompany company, string token, Guid requestId, string action) =>
        SendAsync(_factory, HttpMethod.Post, $"/api/v1/android/approvals/{requestId}/{action}", token, new { note = (string?)null });

    private Task<HttpResponseMessage> NativeSaleAsync(CatalogCompany company, string externalId, string customerCode, Guid catalogOrderId) =>
        SendAsync(_factory, HttpMethod.Post, "/api/v1/ingest/jobs", company.Ali, new
        {
            externalId,
            documentType = "sales_order",
            payload = new { customerCode, paymentType = "Nakit", catalogOrderId, lines = new[] { new { productCode = "A", quantity = 1, unitPrice = 100 } } },
        });

    private Task<int> JobsAsync(CatalogCompany company, params string[] externalIds) =>
        ReadAsync(_factory, db => db.Jobs.CountAsync(j => j.TenantId == company.Id && externalIds.Contains(j.ExternalId)));

    private async Task EditProductsAsync(CatalogCompany company, params object[] items)
    {
        var revision = (await OkAsync<CatalogSettingsDto>(await SendAsync(_factory, HttpMethod.Get, Base + "/settings", company.Mudur))).Revision;
        var saved = await SendAsync(_factory, HttpMethod.Put, Base + "/products", company.Mudur, new { revision, items });
        saved.StatusCode.Should().Be(HttpStatusCode.OK, await saved.Content.ReadAsStringAsync());
    }
}
