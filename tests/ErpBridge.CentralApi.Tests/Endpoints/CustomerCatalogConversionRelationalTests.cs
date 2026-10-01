using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.ErpWrite;
using ErpBridge.Core.Jobs;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static ErpBridge.CentralApi.Tests.Endpoints.CatalogCustomerTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU S10: the panel turns a request into the phone's sale. Seed as in
/// <see cref="CustomerCatalogOrdersRelationalTests"/>: A "Çay Rize" in list 1 at 100 VAT included (10 %), barcode 8690001;
/// customer C1's salesperson code P1 is Ali's, whose mapping has ERP user 7 and warehouse 3; the company's defaults are
/// user 1, warehouse 1, kind "order". C1's account has 10 % discount, so 12 × A is 1080.00.
/// </summary>
public sealed class CustomerCatalogConversionRelationalTests : IClassFixture<CatalogHostFactory>
{
    private const string Pass = "musteri123";
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    /// <summary>Every field Sipariş Cepte's <c>salesOrderPayload</c> writes for an on-account sale without a bank (OutgoingDocumentRepository.kt).</summary>
    private static readonly string[] PhoneHeaderFields =
        ["mobileDocumentId", "revision", "occurredAt", "transactionType", "counterparty", "customerCode", "amount", "currency", "paymentType", "description", "catalogOrderId", "priceListNo", "lines"];

    private static readonly string[] PhoneLineFields =
        ["barcode", "productCode", "productTitle", "quantity", "unitPrice", "lineTotal", "unitPointer", "listUnitPrice", "lineDiscountPercent", "customerDiscountPercent", "generalDiscountPercent"];

    private readonly CatalogHostFactory _factory;

    public CustomerCatalogConversionRelationalTests(CatalogHostFactory factory) => _factory = factory;

    [Fact]
    public async Task The_panel_builds_the_phones_sale_in_the_salespersons_name_and_the_agent_can_write_it()
    {
        var c = await CompanyWithMappingsAsync();
        var patronId = await UserIdAsync(c, "patron");
        var id = await OrderAsync(c, "C1", "yilmaz", 10m, 1080m);

        var preview = await OkAsync<CatalogOrderConversionDto>(await SendAsync(_factory, HttpMethod.Get, $"{Base}/orders/{id}/conversion", c.Patron));
        preview.Should().Match<CatalogOrderConversionDto>(p => p.OwnerUserId == c.AliId && p.OwnerIsAssignee && p.OwnerName == "ali bey"
            && p.Total == 1080.00m && p.OrderedTotal == 1080.00m && !p.PriceChanged && p.Erp && p.DefaultWarehouseNo == 3
            && p.MissingMappings.Length == 0 && !p.RequiresApproval && p.PriceListNo == 1 && p.PriceIncludesVat);
        preview.Warehouses.Select(w => w.Code).Should().Equal("1", "3");
        preview.Lines.Should().ContainSingle().Which.Should().Match<CatalogOrderConversionLineDto>(l =>
            l.StockCode == "A" && l.ListPrice == 100m && l.OrderedListPrice == 100m && l.DiscountPercent == 10m && l.Total == 1080.00m && l.Issue == null && !l.PriceChanged);

        var converted = await OkAsync<CatalogOrderConvertResponse>(await ConvertAsync(c, c.Patron, id, 1080m), HttpStatusCode.Created);
        converted.Should().Match<CatalogOrderConvertResponse>(r => r.Outcome == "JOB" && r.JobStatus == "Pending" && r.ApprovalRequestId == null);
        converted.DocumentRef.Should().MatchRegex("^CAT-SO-[0-9a-f-]{36}$");
        converted.Order.Should().Match<CatalogOrderDetailDto>(o => o.Status == "COMPLETED" && o.DocumentRef == converted.DocumentRef && o.ClosedByName == "patron bey");

        var job = await ReadAsync(_factory, db => db.Jobs.AsNoTracking().SingleAsync(j => j.TenantId == c.Id && j.ExternalId == converted.DocumentRef));
        job.Should().Match<Job>(j => j.Id == converted.JobId && j.DocumentType == "sales_order" && j.Status == JobStatus.Pending);
        job.CreatedByUserId.Should().Be(c.AliId, "the customer's salesperson's document, not the converter's");
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == id)))
            .Should().Match<CatalogOrder>(o => o.ClosedByUserId == patronId && o.ClaimedByUserId == patronId);

        // Field for field the phone's body.
        using var body = JsonDocument.Parse(job.PayloadJson);
        var root = body.RootElement;
        root.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo(PhoneHeaderFields, "the default warehouse is not sent, as on the phone");
        root.GetProperty("mobileDocumentId").GetString().Should().Be(converted.DocumentRef);
        root.GetProperty("revision").GetInt32().Should().Be(1);
        root.GetProperty("occurredAt").GetString().Should().MatchRegex(@"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}$");
        root.GetProperty("transactionType").GetString().Should().Be("Satış");
        root.GetProperty("counterparty").GetString().Should().Be("Yılmaz Market Ltd. Şti.");
        root.GetProperty("customerCode").GetString().Should().Be("C1");
        root.GetProperty("amount").GetDecimal().Should().Be(1080.00m);
        root.GetProperty("currency").GetString().Should().Be("TL");
        root.GetProperty("paymentType").GetString().Should().Be("Cari Borç");
        root.GetProperty("description").GetString().Should().Be($"Katalog siparişi {converted.Order.No}\n[Notlar: kapıya bırakın]");
        root.GetProperty("catalogOrderId").GetString().Should().Be(id.ToString());
        root.GetProperty("priceListNo").GetInt32().Should().Be(1);
        var line = root.GetProperty("lines").EnumerateArray().Should().ContainSingle().Subject;
        line.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo(PhoneLineFields);
        line.GetProperty("barcode").GetString().Should().Be("8690001");
        line.GetProperty("productCode").GetString().Should().Be("A");
        line.GetProperty("productTitle").GetString().Should().Be("Çay Rize");
        line.GetProperty("quantity").GetDecimal().Should().Be(12m);
        line.GetProperty("unitPointer").GetInt32().Should().Be(1);
        line.GetProperty("listUnitPrice").GetDecimal().Should().Be(100m);
        line.GetProperty("lineDiscountPercent").GetDecimal().Should().Be(0m);
        line.GetProperty("customerDiscountPercent").GetDecimal().Should().Be(10m);
        line.GetProperty("generalDiscountPercent").GetDecimal().Should().Be(0m);
        // 1090.91 gross (VAT taken out of the list price) less 109.09 discount: the net before VAT, as the phone's lineTotal.
        line.GetProperty("lineTotal").GetDecimal().Should().Be(981.82m);
        line.GetProperty("unitPrice").GetDecimal().Should().Be(81.818333m);

        // The agent reads it with the owner's mapping, as the lease builds it.
        var translation = new MobileDocumentTranslator().Translate("sales_order", converted.DocumentRef, job.PayloadJson, await AgentContextAsync(c, c.AliId, "ali"));
        translation.Error.Should().BeNull();
        var sale = translation.Sale!;
        sale.Should().Match<SalesDocumentCommand>(s => s.Kind == SalesDocumentKind.Order && s.WarehouseNo == 3 && s.PriceListNo == 1 && s.Settlement == SalesSettlement.Open);
        sale.Header.Should().Match<ErpDocumentHeader>(h => h.CustomerCode == "C1" && h.ErpUserNo == 7 && h.SalespersonCode == "P1" && h.ExpectedTotal == 1080.00m);
        sale.Lines.Should().ContainSingle().Which.Should().Be(new SalesDocumentLine("A", 12m, 1, 100m, 0m, 10m, 0m));
    }

    [Fact]
    public async Task Without_a_salesperson_the_document_is_the_converters_and_only_another_warehouse_is_sent()
    {
        var c = await CompanyWithMappingsAsync();
        var patronId = await UserIdAsync(c, "patron");
        var first = await OrderAsync(c, "C2", "akgida", 0m, 1200m);

        var preview = await OkAsync<CatalogOrderConversionDto>(await SendAsync(_factory, HttpMethod.Get, $"{Base}/orders/{first}/conversion", c.Patron));
        preview.Should().Match<CatalogOrderConversionDto>(p => p.OwnerUserId == patronId && !p.OwnerIsAssignee && p.DefaultWarehouseNo == 1);

        var chosen = await OkAsync<CatalogOrderConvertResponse>(await ConvertAsync(c, c.Patron, first, 1200m, warehouseNo: 3), HttpStatusCode.Created);
        var job = await ReadAsync(_factory, db => db.Jobs.AsNoTracking().SingleAsync(j => j.TenantId == c.Id && j.ExternalId == chosen.DocumentRef));
        job.CreatedByUserId.Should().Be(patronId, "nobody else to write it for");
        using (var body = JsonDocument.Parse(job.PayloadJson)) body.RootElement.GetProperty("warehouseNo").GetInt32().Should().Be(3);

        var second = await OrderAsync(c, "C2", "akgida", 0m, 1200m, newAccount: false);
        var kept = await OkAsync<CatalogOrderConvertResponse>(await ConvertAsync(c, c.Patron, second, 1200m, warehouseNo: 1), HttpStatusCode.Created);
        var payload = await ReadAsync(_factory, db => db.Jobs.AsNoTracking().Where(j => j.ExternalId == kept.DocumentRef).Select(j => j.PayloadJson).SingleAsync());
        using (var body = JsonDocument.Parse(payload)) body.RootElement.TryGetProperty("warehouseNo", out _).Should().BeFalse("the default warehouse stays the agent's");
    }

    [Fact]
    public async Task A_changed_price_or_a_product_no_longer_sold_stops_the_conversion_with_the_new_preview()
    {
        var c = await CompanyWithMappingsAsync();
        var id = await OrderAsync(c, "C1", "yilmaz", 10m, 1080m);
        await SetPriceAsync(c, "A|1", 110m);

        var preview = await OkAsync<CatalogOrderConversionDto>(await SendAsync(_factory, HttpMethod.Get, $"{Base}/orders/{id}/conversion", c.Patron));
        preview.Should().Match<CatalogOrderConversionDto>(p => p.PriceChanged && p.OrderedTotal == 1080.00m && p.Total == 1188.00m);
        preview.Lines.Single().Should().Match<CatalogOrderConversionLineDto>(l => l.OrderedListPrice == 100m && l.ListPrice == 110m && l.PriceChanged);

        var changed = await ConvertAsync(c, c.Patron, id, 1080m);
        changed.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await changed.ReadAsJsonAsync<CatalogOrderConversionErrorDto>();
        error.ErrorCode.Should().Be("PRICE_CHANGED");
        error.Conversion.Total.Should().Be(1188.00m);
        (await JobCountAsync(c)).Should().Be(0);
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == id))).Status.Should().Be("NEW", "nothing happens before the new prices are accepted");

        await EditProductsAsync(c, new { stockCode = "A", sortOrder = (int?)null, hidden = true, noDiscount = false, cartonOnly = false, cartonQuantity = (int?)null });
        var hidden = await ConvertAsync(c, c.Patron, id, 1188m);
        hidden.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var invalid = await hidden.ReadAsJsonAsync<CatalogOrderConversionErrorDto>();
        invalid.ErrorCode.Should().Be("CART_INVALID");
        invalid.Conversion.Lines.Single().Issue.Should().Be("NOT_AVAILABLE");
        (await JobCountAsync(c)).Should().Be(0);
    }

    [Fact]
    public async Task A_request_becomes_one_document_only()
    {
        var c = await CompanyWithMappingsAsync();
        var id = await OrderAsync(c, "C1", "yilmaz", 10m, 1080m);

        await OkAsync<CatalogOrderConvertResponse>(await ConvertAsync(c, c.Patron, id, 1080m), HttpStatusCode.Created);
        await ShouldFailAsync(await ConvertAsync(c, c.Patron, id, 1080m), HttpStatusCode.Conflict, "CATALOG_ORDER_ALREADY_CONVERTED");
        (await JobCountAsync(c)).Should().Be(1);

        // The phone's sale for the same request is refused too (the company lets sales in without approval for it).
        (await SendAsync(_factory, HttpMethod.Put, "/api/v1/android/approvals/rules", c.Patron, new { rules = new { sale = false } }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var phone = await SendAsync(_factory, HttpMethod.Post, "/api/v1/ingest/jobs", c.Ali, new
        {
            externalId = "MOB-SO-CEV1",
            documentType = "sales_order",
            payload = new { customerCode = "C1", paymentType = "Nakit", catalogOrderId = id, lines = new[] { new { stockCode = "A", quantity = 1, unitPrice = 100 } } },
        });
        await ShouldFailAsync(phone, HttpStatusCode.Conflict, "CATALOG_ORDER_ALREADY_CONVERTED");
    }

    [Fact]
    public async Task Someone_who_does_not_decide_sales_sends_it_for_approval_and_the_approval_completes_the_request()
    {
        var c = await CompanyWithMappingsAsync();
        var id = await OrderAsync(c, "C1", "yilmaz", 10m, 1080m);

        // Accounting does not see the request; Ali, its salesperson, does.
        await ShouldFailAsync(await ConvertAsync(c, c.Muhasebe, id, 1080m), HttpStatusCode.NotFound, "CATALOG_ORDER_NOT_FOUND");
        (await OkAsync<CatalogOrderConversionDto>(await SendAsync(_factory, HttpMethod.Get, $"{Base}/orders/{id}/conversion", c.Ali))).RequiresApproval
            .Should().BeTrue("the company's rule (on by default) puts sales through approval");

        var sent = await OkAsync<CatalogOrderConvertResponse>(await ConvertAsync(c, c.Ali, id, 1080m), HttpStatusCode.Created);
        sent.Should().Match<CatalogOrderConvertResponse>(r => r.Outcome == "APPROVAL" && r.JobId == null && r.ApprovalRequestId != null);
        sent.Order.Should().Match<CatalogOrderDetailDto>(o => o.Status == "CLAIMED" && o.ClaimedByName == "ali bey", "the new request was taken in Ali's name first");
        (await JobCountAsync(c)).Should().Be(0);
        var request = await ReadAsync(_factory, db => db.ApprovalRequests.AsNoTracking().SingleAsync(r => r.Id == sent.ApprovalRequestId));
        request.Should().Match<ApprovalRequest>(r => r.Kind == "sale" && r.RequestedByUserId == c.AliId && r.Amount == 1080.00m && r.Status == ApprovalStatuses.Pending);
        using (var documents = JsonDocument.Parse(request.DocumentsJson))
        {
            var document = documents.RootElement.EnumerateArray().Should().ContainSingle().Subject;
            document.GetProperty("externalId").GetString().Should().Be(sent.DocumentRef);
            document.GetProperty("payload").GetProperty("catalogOrderId").GetString().Should().Be(id.ToString());
        }

        (await SendAsync(_factory, HttpMethod.Post, $"/api/v1/android/approvals/{request.Id}/approve", c.Patron, new { note = (string?)null }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == id)))
            .Should().Match<CatalogOrder>(o => o.Status == "COMPLETED" && o.DocumentRef == sent.DocumentRef && o.ClosedByUserId == c.AliId);
        (await ReadAsync(_factory, db => db.Jobs.AsNoTracking().SingleAsync(j => j.TenantId == c.Id))).ExternalId.Should().Be(sent.DocumentRef);
    }

    [Fact]
    public async Task Only_whoever_has_the_request_or_a_manager_converts_it()
    {
        var c = await CompanyWithMappingsAsync();
        var id = await OrderAsync(c, "C1", "yilmaz", 10m, 1080m);
        (await SendAsync(_factory, HttpMethod.Post, $"{Base}/orders/{id}/claim", c.Mudur, new { force = false })).StatusCode.Should().Be(HttpStatusCode.OK);

        await ShouldFailAsync(await ConvertAsync(c, c.Ali, id, 1080m), HttpStatusCode.Conflict, "CATALOG_ORDER_TAKEN");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, $"{Base}/orders/{id}/convert", c.Patron, new { }), HttpStatusCode.BadRequest, "INVALID_BODY");

        var converted = await OkAsync<CatalogOrderConvertResponse>(await ConvertAsync(c, c.Patron, id, 1080m), HttpStatusCode.Created);
        converted.Order.Should().Match<CatalogOrderDetailDto>(o => o.Status == "COMPLETED" && o.ClaimedByName == "mudur bey", "a manager converts another's request without taking it");
    }

    [Fact]
    public async Task A_missing_erp_mapping_is_told_before_any_job_exists()
    {
        var c = await OpenCatalogAsync(_factory);
        await SeedAsync(_factory, db => db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = c.AliId, TenantId = c.Id, SalespersonCode = "P1" }));
        var id = await OrderAsync(c, "C1", "yilmaz", 10m, 1080m);

        var preview = await OkAsync<CatalogOrderConversionDto>(await SendAsync(_factory, HttpMethod.Get, $"{Base}/orders/{id}/conversion", c.Patron));
        preview.MissingMappings.Should().Equal(CatalogOrderConversion.MissingErpUserNo, CatalogOrderConversion.MissingWarehouse);

        var refused = await ConvertAsync(c, c.Patron, id, 1080m, warehouseNo: 2);
        refused.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await refused.ReadAsJsonAsync<CatalogOrderConversionErrorDto>();
        error.ErrorCode.Should().Be("ERP_MAPPING_MISSING");
        error.Message.Should().Contain("ali bey").And.Contain("ERP kullanıcı numarası").And.NotContain("depo", "the form chose a warehouse");
        (await JobCountAsync(c)).Should().Be(0);
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == id))).Status.Should().Be("NEW");
    }

    [Fact]
    public async Task Without_an_erp_the_sale_is_booked_in_the_ledger_at_once()
    {
        var c = await OpenCatalogAsync(_factory);
        await SeedAsync(_factory, db => db.Tenants.Single(t => t.Id == c.Id).DataSource = TenantDataSources.Native);
        var id = await OrderAsync(c, "C1", "yilmaz", 10m, 1080m);

        var preview = await OkAsync<CatalogOrderConversionDto>(await SendAsync(_factory, HttpMethod.Get, $"{Base}/orders/{id}/conversion", c.Patron));
        preview.Should().Match<CatalogOrderConversionDto>(p => !p.Erp && p.MissingMappings.Length == 0 && p.Warehouses.Length == 0 && p.DefaultWarehouseNo == null);

        var converted = await OkAsync<CatalogOrderConvertResponse>(await ConvertAsync(c, c.Patron, id, 1080m, warehouseNo: 9), HttpStatusCode.Created);
        converted.Should().Match<CatalogOrderConvertResponse>(r => r.Outcome == "JOB" && r.JobStatus == "Succeeded");
        converted.Order.Status.Should().Be("COMPLETED");
        var payload = await ReadAsync(_factory, db => db.Jobs.AsNoTracking().Where(j => j.ExternalId == converted.DocumentRef).Select(j => j.PayloadJson).SingleAsync());
        using (var body = JsonDocument.Parse(payload)) body.RootElement.TryGetProperty("warehouseNo", out _).Should().BeFalse("the ledger has no warehouse choice");
        var entries = await ReadAsync(_factory, db => db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == c.Id && r.Entity == "customerTransactions" && r.PayloadJson!.Contains(converted.DocumentRef)).Select(r => r.PayloadJson!).ToListAsync());
        entries.Should().ContainSingle();
        using var entry = JsonDocument.Parse(entries[0]);
        entry.RootElement.GetProperty("customerCode").GetString().Should().Be("C1");
        entry.RootElement.GetProperty("amount").GetDecimal().Should().Be(1080.00m);
    }

    // ---- helpers ------------------------------------------------------------------------------

    /// <summary>A published catalog with warehouses 1 and 3, the company's ERP defaults and Ali's mapping (P1, user 7, warehouse 3).</summary>
    private async Task<CatalogCompany> CompanyWithMappingsAsync()
    {
        var c = await OpenCatalogAsync(_factory);
        await SeedAsync(_factory, db =>
        {
            db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = c.AliId, TenantId = c.Id, SalespersonCode = "P1", ErpUserNo = 7, WarehouseNo = 3 });
            db.ErpWriteSettings.Add(new ErpWriteSettings { TenantId = c.Id, SalesDocumentKind = SalesDocumentKinds.Order, DefaultWarehouseNo = 1, DefaultErpUserNo = 1 });
            Record(db, c.Id, "lookups", "warehouse|3", new { kind = "warehouse", code = "3", name = "Şube" });
            Record(db, c.Id, "lookups", "warehouse|1", new { kind = "warehouse", code = "1", name = "Merkez" });
        });
        return c;
    }

    /// <summary>A customer's request for 12 × A (the account made first), its id.</summary>
    private async Task<Guid> OrderAsync(CatalogCompany company, string customerCode, string username, decimal discountPercent, decimal expectedTotal, bool newAccount = true)
    {
        if (newAccount) await AccountAsync(_factory, company, customerCode, username, Pass, discountPercent: discountPercent);
        var browser = Browser(_factory);
        (await LoginAsync(browser, company, username, Pass)).StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await CatalogCustomerTestSupport.SendAsync(browser, HttpMethod.Post, Api(company) + "/orders", new
        {
            requestId = Guid.NewGuid(),
            lines = new[] { new { key = "A", quantity = 12m } },
            note = " kapıya bırakın ",
            expectedTotal,
        });
        return (await OkAsync<CatalogOrderResponse>(created, HttpStatusCode.Created)).Order.Id;
    }

    private Task<HttpResponseMessage> ConvertAsync(CatalogCompany company, string token, Guid id, decimal expectedTotal, int? warehouseNo = null) =>
        SendAsync(_factory, HttpMethod.Post, $"{Base}/orders/{id}/convert", token, new { expectedTotal, warehouseNo });

    private Task<Guid> UserIdAsync(CatalogCompany company, string username) =>
        ReadAsync(_factory, db => db.MobileUsers.AsNoTracking().Where(u => u.TenantId == company.Id && u.Username == username).Select(u => u.Id).SingleAsync());

    private Task<int> JobCountAsync(CatalogCompany company) => ReadAsync(_factory, db => db.Jobs.CountAsync(j => j.TenantId == company.Id));

    /// <summary>The context the lease sends the agent for a job of <paramref name="userId"/>, as the agent reads it.</summary>
    private async Task<ErpWriteContext> AgentContextAsync(CatalogCompany company, Guid userId, string username)
    {
        var (settings, mapping) = await ReadAsync(_factory, async db => (
            await db.ErpWriteSettings.AsNoTracking().SingleAsync(s => s.TenantId == company.Id),
            await db.MobileUserErpMappings.AsNoTracking().SingleAsync(m => m.TenantId == company.Id && m.UserId == userId)));
        var leased = ErpWriteContextBuilder.Build(settings, mapping, username);
        return JsonSerializer.Deserialize<ErpWriteContext>(JsonSerializer.Serialize(leased, Web), Web)!;
    }

    /// <summary>The ERP changed a price: the stock mirror picks the row up by its newer sequence.</summary>
    private Task SetPriceAsync(CatalogCompany company, string key, decimal price) => SeedAsync(_factory, db =>
    {
        var next = db.MobileRecords.Where(r => r.TenantId == company.Id).Max(r => r.UpdatedSeq) + 1;
        var row = db.MobileRecords.Single(r => r.TenantId == company.Id && r.Entity == "prices" && r.RecordKey == key);
        var parts = key.Split('|');
        row.PayloadJson = JsonSerializer.Serialize(new { stockCode = parts[0], listNumber = int.Parse(parts[1]), price }, Web);
        row.UpdatedSeq = next;
    });

    private async Task EditProductsAsync(CatalogCompany company, params object[] items)
    {
        var revision = (await OkAsync<CatalogSettingsDto>(await SendAsync(_factory, HttpMethod.Get, Base + "/settings", company.Mudur))).Revision;
        var saved = await SendAsync(_factory, HttpMethod.Put, Base + "/products", company.Mudur, new { revision, items });
        saved.StatusCode.Should().Be(HttpStatusCode.OK, await saved.Content.ReadAsStringAsync());
    }
}
