using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Customers in the panel (panel goal P3): the paged list, the card, the statement and an
/// invoice's lines — from the ledger and line mirrors the phones receive, for Mikro and
/// native companies alike.
/// </summary>
public sealed class PortalCustomerLedgerRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private static readonly DateOnly Today = PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);

    private readonly SqliteCentralApiFactory _factory;

    public PortalCustomerLedgerRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task An_erp_company_lists_customers_and_opens_a_statement_and_an_invoice()
    {
        var c = await CompanyAsync(native: false);
        await SeedErpLedgerAsync(c.Id);

        var all = await GetJsonAsync<PortalCustomersResponse>(c.Patron, "/api/v1/portal/customers");
        all.Total.Should().Be(3);
        all.Items.Select(i => i.CustomerCode).Should().Equal("C/1", "C-3", "C-2"); // by title: Bakkal Ali, Büfe Can, Market Veli
        all.TotalReceivable.Should().Be(700m);
        all.TotalPayable.Should().Be(200m);
        all.Items[0].City.Should().Be("İzmir");
        (await GetJsonAsync<PortalCustomersResponse>(c.Patron, "/api/v1/portal/customers?balance=receivable")).Items.Select(i => i.CustomerCode).Should().Equal("C/1");
        (await GetJsonAsync<PortalCustomersResponse>(c.Patron, "/api/v1/portal/customers?balance=payable")).Items.Select(i => i.CustomerCode).Should().Equal("C-2");
        (await GetJsonAsync<PortalCustomersResponse>(c.Patron, "/api/v1/portal/customers?balance=nonzero")).Total.Should().Be(2);
        (await GetJsonAsync<PortalCustomersResponse>(c.Patron, "/api/v1/portal/customers?sort=balance&dir=desc")).Items.Select(i => i.CustomerCode).Should().Equal("C/1", "C-3", "C-2");
        (await GetJsonAsync<PortalCustomersResponse>(c.Patron, "/api/v1/portal/customers?q=0532")).Items.Select(i => i.CustomerCode).Should().Equal("C/1");
        (await GetJsonAsync<PortalCustomersResponse>(c.Patron, "/api/v1/portal/customers?pageSize=2&page=2")).Items.Select(i => i.CustomerCode).Should().Equal("C-2");

        var card = await GetJsonAsync<PortalCustomerCard>(c.Patron, "/api/v1/portal/customers/card?code=C%2F1");
        card.Should().Match<PortalCustomerCard>(x => x.Title == "Bakkal Ali" && x.Balance == 700m && x.Phone == "0532 000 00 00" && x.TaxNo == "123" && x.DataSource == c.DataSource);
        card.Address.Should().Contain("İzmir");

        var ledger = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C%2F1");
        ledger.Items.Select(i => (i.Id, i.Kind, i.Balance)).Should().Equal(("m3", "sale_return", 700m), ("m2", "collection", 800m), ("m1", "sale", 1300m), ("m4", "sale", 300m));
        ledger.Opening.Should().Be(0m);
        ledger.Closing.Should().Be(700m, "the statement ends on the card balance");
        ledger.Items.Single(i => i.Id == "m1").Should().Match<PortalLedgerRow>(i => i.DocumentKey == "r11" && i.Debit == 1000m && i.DocumentNo == "A-1" && i.Date == "2026-01-05");
        ledger.Items.Single(i => i.Id == "m3").DocumentKey.Should().Be("r12", "an invoice opens even when no lines are mirrored");
        ledger.Items.Single(i => i.Id == "m2").DocumentKey.Should().BeNull("a collection has no lines");

        var thisYear = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C%2F1&from=2026-01-01");
        thisYear.Opening.Should().Be(300m);
        thisYear.Total.Should().Be(3);

        var january = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C%2F1&to=2026-01-31");
        january.Items.Select(i => i.Id).Should().Equal("m1", "m4");
        january.Closing.Should().Be(1300m);

        var collections = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C%2F1&kind=collection");
        collections.Items.Should().ContainSingle().Which.Balance.Should().Be(800m, "the balance column still counts every movement");
        collections.TotalCredit.Should().Be(500m);
        collections.TotalDebit.Should().Be(0m);

        var paged = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C%2F1&pageSize=3&page=2");
        paged.Items.Select(i => i.Id).Should().Equal("m4");

        var invoice = await GetJsonAsync<PortalDocumentResponse>(c.Patron, "/api/v1/portal/customers/document?code=C%2F1&key=r11");
        invoice.Should().Match<PortalDocumentResponse>(d => d.LinesAvailable && d.Amount == 1000m && d.Kind == "sale" && d.DocumentNo == "A-1");
        invoice.Lines.Should().ContainSingle().Which.Should().Match<PortalDocumentLine>(l =>
            l.StockCode == "CAY-1" && l.Name == "Çay 1 kg" && l.Quantity == 5m && l.UnitPrice == 200m && l.Amount == 1000m && l.Tax == 180m && l.WarehouseNo == 1);

        var noLines = await GetJsonAsync<PortalDocumentResponse>(c.Patron, "/api/v1/portal/customers/document?code=C%2F1&key=r12");
        noLines.Should().Match<PortalDocumentResponse>(d => !d.LinesAvailable && d.Lines.Count == 0 && d.Kind == "sale_return" && d.Amount == 100m);
        await ExpectAsync(c.Patron, "/api/v1/portal/customers/document?code=C%2F1&key=r20", HttpStatusCode.NotFound, "DOCUMENT_NOT_FOUND");

        // Same-day Mikro movements follow their record number as a number: 9 before 10.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var seq = 2_100_000L;
            foreach (var (id, recNo, amount) in new[] { ("10", 10, 40m), ("9", 9, 30m) })
                db.MobileRecords.Add(new MobileRecord
                {
                    TenantId = c.Id, Entity = "customerTransactions", RecordKey = id, UpdatedSeq = ++seq,
                    PayloadJson = JsonSerializer.Serialize(new { id, erp = "MIKRO", cariKod = "C-3", tarih = "2026-04-01T00:00:00", tutar = amount, borcMu = true, type = "SATIS", cha_recno = recNo }, Web),
                });
            await db.SaveChangesAsync();
        }
        var sameDay = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-3");
        // C-3's card still says 0 (the agent re-sends a card only when the card changes); the list and the
        // statement follow the ledger, like Sipariş Cepte.
        sameDay.Items.Select(i => (i.Id, i.Balance)).Should().Equal(("10", 70m), ("9", 30m));
        sameDay.Opening.Should().Be(0m);
        await ExpectAsync(c.Patron, "/api/v1/portal/customers/document?code=C-2&key=r11", HttpStatusCode.NotFound, "DOCUMENT_NOT_FOUND");
        await ExpectAsync(c.Patron, "/api/v1/portal/customers/card?code=NOPE", HttpStatusCode.NotFound, "CUSTOMER_NOT_FOUND");
        await ExpectAsync(c.Patron, "/api/v1/portal/customers/ledger?code=C%2F1&kind=gift", HttpStatusCode.BadRequest, "INVALID_QUERY");
        await ExpectAsync(c.Patron, "/api/v1/portal/customers/ledger?code=C%2F1&from=2026-02-01&to=2026-01-01", HttpStatusCode.BadRequest, "INVALID_RANGE");
        await ExpectAsync(c.Patron, "/api/v1/portal/customers?balance=some", HttpStatusCode.BadRequest, "INVALID_QUERY");
        (await _factory.CreateClient().GetAsync("/api/v1/portal/customers", c.Ali)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_closed_mikro_invoice_belongs_to_its_customer_but_stays_out_of_the_statement()
    {
        var c = await CompanyAsync(native: false);
        await SeedErpLedgerAsync(c.Id);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            // Peşin sale closed to kasa 001 (cariKod) for customer C-3 (ciroCariKod): Mikro never moved C-3's balance.
            db.MobileRecords.Add(new MobileRecord
            {
                TenantId = c.Id, Entity = "customerTransactions", RecordKey = "k1", UpdatedSeq = 2_200_001,
                PayloadJson = JsonSerializer.Serialize(new { id = "k1", erp = "MIKRO", cariKod = "001", ciroCariKod = "C-3", kapali = true, tarih = "2026-05-01T00:00:00", evrakTip = 63, tutar = 780m, borcMu = true, type = "SATIS", cha_recno = 40 }, Web),
            });
            db.MobileRecords.Add(new MobileRecord
            {
                TenantId = c.Id, Entity = "customerTransactions", RecordKey = "k2", UpdatedSeq = 2_200_002,
                PayloadJson = JsonSerializer.Serialize(new { id = "k2", erp = "MIKRO", cariKod = "C-3", kapali = false, tarih = "2026-05-02T00:00:00", evrakTip = 0, normalIade = true, tutar = 50m, borcMu = false, type = "SATIS_IADE", cha_recno = 41 }, Web),
            });
            await db.SaveChangesAsync();
        }

        var ledger = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-3");
        ledger.Items.Select(i => (i.Id, i.Kind)).Should().Equal(("k2", "sale_return"));
        ledger.Closing.Should().Be(-50m, "the return moved C-3's balance; the closed peşin sale did not");
        (await GetJsonAsync<PortalCustomerCard>(c.Patron, "/api/v1/portal/customers/card?code=C-3")).Balance.Should().Be(-50m);
        await ExpectAsync(c.Patron, "/api/v1/portal/customers/card?code=001", HttpStatusCode.NotFound, "CUSTOMER_NOT_FOUND");
    }

    [Fact]
    public async Task An_erp_company_balance_follows_the_ledger_when_the_agent_card_balance_is_stale()
    {
        var c = await CompanyAsync(native: false);
        await SeedErpLedgerAsync(c.Id);
        // Warm the panel's caches first: the new movements below must reach an already-built view.
        (await GetJsonAsync<PortalCustomersResponse>(c.Patron, "/api/v1/portal/customers")).TotalReceivable.Should().Be(700m);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var seq = 2_300_000L;
            void Add(string entity, string key, object payload) => db.MobileRecords.Add(new MobileRecord
            {
                TenantId = c.Id, Entity = entity, RecordKey = key, UpdatedSeq = ++seq, PayloadJson = JsonSerializer.Serialize(payload, Web),
            });
            // A new sale and collection for C/1; its card (balance 700) is not re-sent.
            Add("customerTransactions", "n1", new { id = "n1", erp = "MIKRO", cariKod = "C/1", tarih = "2026-06-01T00:00:00", tutar = 400m, borcMu = true, type = "SATIS", cha_recno = 50 });
            Add("customerTransactions", "n2", new { id = "n2", erp = "MIKRO", cariKod = " c/1 ", tarih = "2026-06-02T00:00:00", tutar = 150m, borcMu = false, type = "TAHSILAT", cha_recno = 51 });
            // The agent mirrors the whole ledger: a customer without rows is at zero, whatever the card says.
            Add("customers", "C-4", new { customerCode = "C-4", title1 = "Devir Ltd", balance = 90 });
            // Kasa/banka-side rows under a code equal to a customer's never move that customer (cha_cari_cins <> 0):
            // told by cariCins, or — from an agent before G4 — by the ciro code the reader fills only on that side.
            Add("customerTransactions", "k1", new { id = "k1", erp = "MIKRO", cariKod = "C-3", cariCins = 4, tarih = "2026-06-03T00:00:00", tutar = 75m, borcMu = true, type = "HAREKET", cha_recno = 52 });
            Add("customerTransactions", "k2", new { id = "k2", erp = "MIKRO", cariKod = "C-3", ciroCariKod = "C/1", tarih = "2026-06-04T00:00:00", tutar = 25m, borcMu = false, type = "HAREKET", cha_recno = 53 });
            await db.SaveChangesAsync();
        }

        var list = await GetJsonAsync<PortalCustomersResponse>(c.Patron, "/api/v1/portal/customers?sort=code");
        list.Items.Select(i => (i.CustomerCode, i.Balance)).Should().BeEquivalentTo(new[] { ("C/1", 950m), ("C-2", -200m), ("C-3", 0m), ("C-4", 0m) });
        list.TotalReceivable.Should().Be(950m);
        list.TotalPayable.Should().Be(200m);
        (await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-3")).Items.Should().BeEmpty();
        (await GetJsonAsync<PortalCustomerCard>(c.Patron, "/api/v1/portal/customers/card?code=C%2F1")).Balance.Should().Be(950m);

        var ledger = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C%2F1");
        ledger.Opening.Should().Be(0m);
        ledger.Closing.Should().Be(950m, "the statement ends on the balance the list shows");
        ledger.Items[0].Should().Match<PortalLedgerRow>(i => i.Id == "n2" && i.Balance == 950m);

        var desk = await GetJsonAsync<PortalBalancesResponse>(c.Patron, "/api/v1/portal/balances");
        desk.Rows.Select(r => (r.CustomerCode, r.Balance)).Should().BeEquivalentTo(new[] { ("C/1", 950m), ("C-2", -200m) });
        desk.TotalReceivable.Should().Be(950m);
        (await GetJsonAsync<PortalBalancesResponse>(c.Patron, "/api/v1/portal/balances?search=market")).Rows.Should().ContainSingle().Which.Balance.Should().Be(-200m);

        // C-2's only invoice is deleted in Mikro; its card (-200) is not re-sent. The panel shows zero, not the card.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var row = db.MobileRecords.Single(r => r.TenantId == c.Id && r.Entity == "customerTransactions" && r.RecordKey == "x1");
            row.IsDeleted = true;
            row.UpdatedSeq = 2_300_100;
            await db.SaveChangesAsync();
        }
        (await GetJsonAsync<PortalCustomerCard>(c.Patron, "/api/v1/portal/customers/card?code=C-2")).Balance.Should().Be(0m);
    }

    [Fact]
    public async Task An_erp_company_whose_agent_sends_no_ledger_keeps_the_card_balances()
    {
        var c = await CompanyAsync(native: false);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            db.MobileRecords.Add(new MobileRecord
            {
                TenantId = c.Id, Entity = "customers", RecordKey = "C-9", CustomerKey = "C-9", UpdatedSeq = 2_400_001,
                PayloadJson = JsonSerializer.Serialize(new { customerCode = "C-9", title1 = "Kart Bakiyeli", balance = 125 }, Web),
            });
            await db.SaveChangesAsync();
        }

        (await GetJsonAsync<PortalCustomersResponse>(c.Patron, "/api/v1/portal/customers")).Items.Should().ContainSingle()
            .Which.Balance.Should().Be(125m);
    }

    [Fact]
    public async Task A_native_company_statement_joins_sale_lines_by_document_number()
    {
        var c = await CompanyAsync(native: true);
        await PostAsync(c, c.Patron, "stock_card", "CARD-S1", new { stockCode = "CAY-1", name = "Çay 1 kg", price = 150, openingQuantity = 40 });
        await PostAsync(c, c.Patron, "customer_card", "CARD-C1", new { customerCode = "C-001", title = "Bakkal Ali", phone = "0555", openingBalance = 50 });
        var day = Today.ToString("yyyy-MM-dd");
        await PostAsync(c, c.Ali, "sales_order", "SO-1", new
        {
            mobileDocumentId = "SO-1", occurredAt = day + "T10:00:00", counterparty = "Bakkal Ali", customerCode = "C-001", amount = 450, paymentType = "Cari Borç",
            lines = new[] { new { productCode = "CAY-1", productTitle = "Çay 1 kg", quantity = 3, unitPrice = 150, lineTotal = 450 } },
        });
        await PostAsync(c, c.Ali, "sales_order", "SO-2", new
        {
            mobileDocumentId = "SO-2", occurredAt = day + "T11:00:00", counterparty = "Bakkal Ali", customerCode = "C-001", amount = 150, paymentType = "Nakit",
            lines = new[] { new { productCode = "CAY-1", productTitle = "Çay 1 kg", quantity = 1, unitPrice = 150, lineTotal = 150 } },
        });
        await PostAsync(c, c.Ali, "collection", "TAH-1", new { mobileDocumentId = "TAH-1", occurredAt = Today.ToString("dd.MM.yyyy") + " 12:00", counterparty = "Bakkal Ali", customerCode = "C-001", amount = 100, paymentType = "Nakit" });

        var card = await GetJsonAsync<PortalCustomerCard>(c.Patron, "/api/v1/portal/customers/card?code=C-001");
        card.Balance.Should().Be(400m, "opening 50 + 450 sale − 100 collection; the cash sale nets to zero");

        var ledger = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-001");
        ledger.Closing.Should().Be(400m);
        ledger.Opening.Should().Be(50m, "the opening balance was set on the card, not booked as a movement");
        ledger.Items.Should().HaveCount(4);
        ledger.Items.Count(i => i.Kind == "sale").Should().Be(2);
        ledger.Items.Count(i => i.Kind == "collection").Should().Be(2);
        ledger.Items.Where(i => i.Kind == "sale").Should().OnlyContain(i => i.DocumentKey != null);
        ledger.Items.Where(i => i.Kind == "collection").Should().OnlyContain(i => i.DocumentKey == null);
        ledger.Items[0].Date.Should().Be(day, "the cash-book date format is read too");

        var saleKey = ledger.Items.Single(i => i.DocumentNo == "SO-1" && i.Kind == "sale").DocumentKey!;
        var invoice = await GetJsonAsync<PortalDocumentResponse>(c.Patron, "/api/v1/portal/customers/document?code=C-001&key=" + Uri.EscapeDataString(saleKey));
        invoice.Lines.Should().ContainSingle().Which.Should().Match<PortalDocumentLine>(l => l.StockCode == "CAY-1" && l.Name == "Çay 1 kg" && l.Quantity == 3m && l.UnitPrice == 150m && l.Amount == 450m);

        var customers = await GetJsonAsync<PortalCustomersResponse>(c.Patron, "/api/v1/portal/customers");
        customers.Items.Should().ContainSingle().Which.Balance.Should().Be(400m);

        // GOAL_PANEL_ERPSIZ E4d: only the standalone collection (TAH-1, booked as its own Collection
        // job) is editable — a sale's own row and its immediate-payment leg (SO-2's cash payment,
        // booked by the sales_order job) are excluded, exactly like E4a's own void check.
        ledger.Items.Single(i => i.DocumentNo == "TAH-1").Editable.Should().BeTrue("a standalone collection is voidable/editable (E4a)");
        ledger.Items.Where(i => i.Kind == "sale").Should().OnlyContain(i => !i.Editable, "a sale's own row is never editable here (D11)");
        ledger.Items.Single(i => i.DocumentNo == "SO-2" && i.Kind == "collection").Editable.Should().BeFalse("a sale's immediate-payment leg belongs to E5's document_void, not here");
    }

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Ali, string DataSource);

    private async Task SeedErpLedgerAsync(Guid tenantId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var seq = 2_000_000L;
        void Add(string entity, string key, object payload, string? customerKey = null, string? stockKey = null) => db.MobileRecords.Add(new MobileRecord
        {
            TenantId = tenantId,
            Entity = entity,
            RecordKey = key,
            CustomerKey = customerKey,
            StockKey = stockKey,
            PayloadJson = JsonSerializer.Serialize(payload, Web),
            UpdatedSeq = ++seq,
        });

        Add("customers", "C/1", new { customerCode = "C/1", title1 = "Bakkal", title2 = "Ali", balance = 700, phone = "0532 000 00 00", taxNo = "123", taxOffice = "Konak" }, "C/1");
        Add("customers", "C-2", new { customerCode = "C-2", title1 = "Market Veli", balance = -200 }, "C-2");
        Add("customers", "C-3", new { customerCode = "C-3", title1 = "Büfe Can", balance = 0 }, "C-3");
        Add("customerAddresses", "C/1|1", new { customerCode = "C/1", addressNo = 1, city = "İzmir", district = "Konak", street = "1. Sok." }, "C/1");
        Add("stocks", "CAY-1", new { stockCode = "CAY-1", name = "Çay 1 kg" }, stockKey: "CAY-1");
        object Move(string id, string date, string type, decimal amount, bool debit, int? recNo, string? evrakNo) =>
            new { id, erpRef = id, erp = "MIKRO", cariKod = "C/1", tarih = date + "T00:00:00", evrakTip = 63, evrakNo, tip = debit ? 0 : 1, tutar = amount, borcMu = debit, type, cha_recno = recNo };
        Add("customerTransactions", "m1", Move("m1", "2026-01-05", "SATIS", 1000, true, 11, "A-1"));
        Add("customerTransactions", "m2", Move("m2", "2026-02-01", "TAHSILAT", 500, false, 20, null));
        Add("customerTransactions", "m3", Move("m3", "2026-03-10", "SATIS_IADE", 100, false, 12, "A-2"));
        Add("customerTransactions", "m4", Move("m4", "2025-12-20", "SATIS", 300, true, 10, "A-0"));
        Add("customerTransactions", "x1", new { id = "x1", erp = "MIKRO", cariKod = "C-2", tarih = "2026-01-01T00:00:00", tutar = 200, borcMu = false, type = "ALIS", cha_recno = 30 });
        Add("stockTransactions", "s1", new { id = "s1", erp = "MIKRO", stokKod = "CAY-1", tarih = "2026-01-05T00:00:00", cikisMiktar = 5, girisMiktar = 0, miktar = -5, birimFiyat = 200, tutar = 1000, vergi = 180, cariKod = "C/1", cikisDepoNo = 1, faturaRecno = 11 });
        Add("stockTransactions", "s0", new { id = "s0", erp = "MIKRO", stokKod = "CAY-1", tarih = "2025-12-20T00:00:00", cikisMiktar = 2, miktar = -2, birimFiyat = 150, tutar = 300, cariKod = "C/1", faturaRecno = 10 });
        await db.SaveChangesAsync();
    }

    private async Task ExpectAsync(string token, string path, HttpStatusCode status, string errorCode)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(status, path);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be(errorCode, path);
    }

    private async Task<Company> CompanyAsync(bool native)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"LEDGER-{suffix}", $"Ledger tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        if (native)
            (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("ali", "SALES") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var overview = await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        var company = new Company(tenant.Id, await LoginAsync(overview.TenantCode!, "patron", $"DEV-P-{suffix}"), await LoginAsync(overview.TenantCode!, "ali", $"DEV-A-{suffix}"),
            native ? TenantDataSources.Native : TenantDataSources.Erp);

        var rules = ApprovalKinds.All.ToDictionary(kind => kind, _ => false);
        (await SendAsync(HttpMethod.Put, company.Patron, tenant.Id, "/api/v1/android/approvals/rules", new { rules })).StatusCode.Should().Be(HttpStatusCode.OK);
        return company;
    }

    private async Task PostAsync(Company c, string token, string documentType, string externalId, object payload)
    {
        var response = await SendAsync(HttpMethod.Post, token, c.Id, "/api/v1/ingest/jobs", new { externalId, documentType, payload });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");
    }

    private async Task<T> GetJsonAsync<T>(string token, string path)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "portal-test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string token, Guid tenantId, string path, object body)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return await _factory.CreateClient().SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutAsync(string path, object value, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }
}
