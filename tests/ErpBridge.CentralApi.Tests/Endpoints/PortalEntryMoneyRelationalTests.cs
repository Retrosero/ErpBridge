using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.ErpWrite;
using ErpBridge.CentralApi.Permissions;
using ErpBridge.CentralApi.Tests.Support;
using ErpBridge.Core.Jobs;
using ErpBridge.Erp.Abstractions.Documents;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.PortalEntryTestSupport;
using K = ErpBridge.CentralApi.Permissions.PermissionKeys;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_PANEL_GIRIS P3b–d: a collection, a tediye and an expense entered in the panel are the documents the phone sends —
/// an ERP company's go to the agent (its translator takes them with the owner's context), a company without an ERP's are
/// booked at once — written straight in, refused over the user's own permissions and limits.
/// </summary>
public sealed class PortalEntryMoneyRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    private readonly SqliteCentralApiFactory _factory;

    public PortalEntryMoneyRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    // ---- collection -------------------------------------------------------------------------

    [Fact]
    public async Task An_erp_collection_is_one_receipt_with_a_line_per_method()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        var operationId = Guid.NewGuid();
        var dueDate = DateOnly.FromDateTime(DateTime.Today).AddDays(60);
        var body = new
        {
            operationId = operationId.ToString("D"),
            ownerUserId = c.AliId,
            customerCode = "C1",
            description = "Eylül tahsilatı",
            payments = new object[]
            {
                new { method = "cash", amount = 1000m },
                new { method = "card", amount = 1500m, bankCode = "13", installments = 3, surchargeAmount = 45m },
                new { method = "cheque", amount = 1200m, documentNo = "27703", dueDate = dueDate.ToString("yyyy-MM-dd"), bankName = "Ziraat" },
            },
            expectedTotal = 3700m,
        };

        var preview = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/collection/preview", c.Mudur, body));
        preview.Should().Match<PortalEntryPreviewResponse>(p => p.Refusal == null && p.Total == 3700m && p.CustomerName == "Yılmaz Market Ltd. Şti.");
        preview.Payments.Select(p => p.Label).Should().Equal("Nakit", "Kredi Kartı", "Çek");

        var saved = await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/collection", c.Mudur, body), HttpStatusCode.Created);
        var externalId = "PNL-TH-" + operationId.ToString("D");
        saved.Documents.Should().ContainSingle().Which.Should().Match<PortalEntryDocumentDto>(d => d.ExternalId == externalId && d.DocumentType == "collection" && d.Status == "Pending");

        var job = await JobAsync(c, externalId);
        job.CreatedByUserId.Should().Be(c.AliId);
        using (var payload = JsonDocument.Parse(job.PayloadJson))
        {
            payload.RootElement.GetProperty("paymentType").GetString().Should().Be("Çoklu Tahsilat");
            payload.RootElement.GetProperty("transactionType").GetString().Should().Be("Tahsilat");
        }
        var translation = new MobileDocumentTranslator().Translate("collection", externalId, job.PayloadJson, await AgentContextAsync(c.Id, c.AliId, "ali"));
        translation.Error.Should().BeNull();
        translation.Collection!.Header.Should().Match<ErpDocumentHeader>(h => h.CustomerCode == "C1" && h.ErpUserNo == 7 && h.ExpectedTotal == 3700m);
        translation.Collection!.Payments.Should().HaveCount(3);
        translation.Collection!.Payments[1].Should().Match<CollectionPayment>(p => p.Method == CollectionMethod.Card && p.AccountCode == "13" && p.Installments == 3);
        translation.Collection!.Payments[2].Should().Match<CollectionPayment>(p => p.Method == CollectionMethod.Cheque && p.DueDate == dueDate.ToDateTime(TimeOnly.MinValue) && p.Cheque!.No == "27703");
        (await ReadAsync(_factory, db => db.ApprovalRequests.CountAsync(r => r.TenantId == c.Id))).Should().Be(0);
    }

    [Fact]
    public async Task A_cheque_without_its_number_and_due_date_is_refused()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);

        await ShouldFailAsync(await PostAsync(_factory, "/collection/preview", c.Patron, new
        {
            customerCode = "C1",
            payments = new[] { new { method = "cheque", amount = 100m } },
        }), HttpStatusCode.BadRequest, "ENTRY_INVALID");
        await ShouldFailAsync(await PostAsync(_factory, "/collection/preview", c.Patron, new
        {
            customerCode = "C1",
            payments = new[] { new { method = "gold", amount = 100m } },
        }), HttpStatusCode.BadRequest, "ENTRY_INVALID");
    }

    [Fact]
    public async Task A_native_collection_books_a_cash_book_entry_per_method_all_or_none()
    {
        var c = await EntryCompanyAsync(_factory, native: true);
        await SeedNativeAsync(_factory, c);
        var operationId = Guid.NewGuid();
        var body = new
        {
            operationId = operationId.ToString("D"),
            customerCode = "C1",
            payments = new object[] { new { method = "cash", amount = 100m }, new { method = "transfer", amount = 50m, bankName = "Akbank" } },
            expectedTotal = 150m,
        };

        var saved = await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/collection", c.Patron, body), HttpStatusCode.Created);
        saved.Documents.Select(d => (d.ExternalId, d.Status)).Should().Equal(
            ($"PNL-TH-{operationId:D}-1", "Succeeded"), ($"PNL-TH-{operationId:D}-2", "Succeeded"));
        (await BalanceAsync(c, "C1")).Should().Be(-150m, "the customer was credited with both payments");
        var transfer = await JobAsync(c, $"PNL-TH-{operationId:D}-2");
        using (var payload = JsonDocument.Parse(transfer.PayloadJson))
        {
            payload.RootElement.GetProperty("paymentType").GetString().Should().Be("EFT / Havale");
            payload.RootElement.GetProperty("bankName").GetString().Should().Be("Akbank");
        }

        // Sent again after a lost answer: the same two documents, booked once.
        var again = await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/collection", c.Patron, body));
        again.Idempotent.Should().BeTrue();
        again.Documents.Should().HaveCount(2);
        (await BalanceAsync(c, "C1")).Should().Be(-150m);
    }

    [Fact]
    public async Task Without_the_collection_module_the_page_is_closed()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        await SetPermissionAsync(_factory, c.MudurId, K.ModuleCollection, PermissionValues.False);

        await ShouldFailAsync(await PostAsync(_factory, "/collection/preview", c.Mudur, new { customerCode = "C1", payments = new[] { new { method = "cash", amount = 1m } } }),
            HttpStatusCode.Forbidden, "ENTRY_MODULE_DENIED");
        (await OkAsync<PortalEntryContextResponse>(await GetAsync(_factory, "/context", c.Mudur))).Kinds.Should().NotContain("collection");
    }

    // ---- tediye -----------------------------------------------------------------------------

    [Fact]
    public async Task An_erp_transfer_tediye_carries_the_banks_code_the_agent_needs()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        var operationId = Guid.NewGuid();
        var externalId = "PNL-TD-" + operationId.ToString("D");

        await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/disbursement", c.Patron, new
        {
            operationId = operationId.ToString("D"),
            ownerUserId = c.AliId,
            customerCode = "C2",
            amount = 1500.5m,
            paymentType = "EFT / Havale",
            bankCode = "13",
            description = "Alış ödemesi",
            expectedTotal = 1500.5m,
        }), HttpStatusCode.Created);

        var job = await JobAsync(c, externalId);
        var translation = new MobileDocumentTranslator().Translate("disbursement", externalId, job.PayloadJson, await AgentContextAsync(c.Id, c.AliId, "ali"));
        translation.Error.Should().BeNull("a named bank goes with its code, so the agent does not ask for an app update");
        translation.Disbursement!.Should().Match<DisbursementCommand>(d => d.Method == DisbursementMethod.Transfer && d.AccountCode == "13" && d.Amount == 1500.5m);
        translation.Disbursement!.Header.CustomerCode.Should().Be("C2");
    }

    [Fact]
    public async Task A_tediye_over_the_users_limit_is_refused()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        await SetPermissionAsync(_factory, c.MudurId, K.LimitDisbursementAmount, "500");

        var preview = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/disbursement/preview", c.Mudur, new { customerCode = "C2", amount = 600m }));
        preview.Refusal.Should().Match<PortalEntryRefusalDto>(r => r.Code == "ENTRY_LIMIT_EXCEEDED" && r.Key == K.LimitDisbursementAmount);
        await ShouldFailAsync(await PostAsync(_factory, "/disbursement", c.Mudur, new { operationId = Guid.NewGuid(), customerCode = "C2", amount = 600m, expectedTotal = 600m }),
            HttpStatusCode.Conflict, "ENTRY_LIMIT_EXCEEDED");
        (await ReadAsync(_factory, db => db.Jobs.CountAsync(j => j.TenantId == c.Id))).Should().Be(0);
    }

    [Fact]
    public async Task A_native_tediye_raises_the_customers_balance()
    {
        var c = await EntryCompanyAsync(_factory, native: true);
        await SeedNativeAsync(_factory, c);

        await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/disbursement", c.Patron,
            new { operationId = Guid.NewGuid(), customerCode = "C1", amount = 200m, expectedTotal = 200m }), HttpStatusCode.Created);

        (await BalanceAsync(c, "C1")).Should().Be(200m);
    }

    // ---- expense ----------------------------------------------------------------------------

    [Fact]
    public async Task An_erp_expense_goes_on_its_card_with_the_vat_and_its_pointer()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        await SeedAsync(_factory, db =>
        {
            Record(db, c.Id, "lookups", "expense_card|YAKIT", new { kind = "expense_card", code = "YAKIT", name = "Yakıt giderleri" });
            Record(db, c.Id, "lookups", "vat_rate|4", new { kind = "vat_rate", code = "4", name = "%20", rate = 20m });
        });
        var operationId = Guid.NewGuid();
        var externalId = "PNL-GD-" + operationId.ToString("D");

        var context = await OkAsync<PortalEntryContextResponse>(await GetAsync(_factory, "/context", c.Patron));
        context.ExpenseCards.Should().ContainSingle().Which.Name.Should().Be("Yakıt giderleri");
        context.VatRates.Should().ContainSingle().Which.Should().Match<PortalErpLookupItem>(r => r.Code == "4" && r.Rate == 20m);

        await ShouldFailAsync(await PostAsync(_factory, "/expense/preview", c.Patron, new { amount = 100m, expenseCardCode = "YAKIT" }), HttpStatusCode.BadRequest, "ENTRY_INVALID");
        await ShouldFailAsync(await PostAsync(_factory, "/expense/preview", c.Patron, new { amount = 100m, expenseCardCode = "YAKIT", vatAmount = 16.67m, description = "x" }),
            HttpStatusCode.BadRequest, "ENTRY_INVALID");

        await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/expense", c.Patron, new
        {
            operationId = operationId.ToString("D"),
            amount = 1250.75m,
            expenseCardCode = "YAKIT",
            vatAmount = 208.46m,
            vatPointer = 4,
            paymentType = "Kredi Kartı",
            accountCode = "13",
            description = "Saha aracı yakıt",
            expectedTotal = 1250.75m,
        }), HttpStatusCode.Created);

        var job = await JobAsync(c, externalId);
        job.DocumentType.Should().Be("expense");
        var translation = new MobileDocumentTranslator().Translate("expense", externalId, job.PayloadJson, await AgentContextAsync(c.Id, c.PatronId, "patron"));
        translation.Error.Should().BeNull();
        translation.Expense!.Should().Match<ExpenseCommand>(e => e.ExpenseCardCode == "YAKIT" && e.Method == ExpensePaymentMethod.CreditCard
            && e.AccountCode == "13" && e.Amount == 1250.75m && e.VatAmount == 208.46m && e.VatPointer == 4);
    }

    [Fact]
    public async Task A_native_expense_is_kept_as_a_record_without_touching_any_balance()
    {
        var c = await EntryCompanyAsync(_factory, native: true);
        await SeedNativeAsync(_factory, c);
        var operationId = Guid.NewGuid();

        var context = await OkAsync<PortalEntryContextResponse>(await GetAsync(_factory, "/context", c.Patron));
        context.ExpenseCategories.Should().Contain("Yol/Yakıt");
        await ShouldFailAsync(await PostAsync(_factory, "/expense/preview", c.Patron, new { amount = 50m, category = "Tatil", description = "x" }), HttpStatusCode.BadRequest, "ENTRY_INVALID");

        var saved = await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/expense", c.Patron, new
        {
            operationId = operationId.ToString("D"),
            amount = 80m,
            category = "Yol/Yakıt",
            description = "Otopark",
            expectedTotal = 80m,
        }), HttpStatusCode.Created);
        saved.Documents.Single().Should().Match<PortalEntryDocumentDto>(d => d.DocumentType == "disbursement" && d.Status == "Succeeded");
        using (var payload = JsonDocument.Parse((await JobAsync(c, "PNL-GD-" + operationId.ToString("D"))).PayloadJson))
        {
            payload.RootElement.GetProperty("counterparty").GetString().Should().Be("Gider: Yol/Yakıt");
            payload.RootElement.TryGetProperty("customerCode", out _).Should().BeFalse();
        }
        (await BalanceAsync(c, "C1")).Should().Be(0m);
    }

    // ---- helpers -----------------------------------------------------------

    private Task<Job> JobAsync(EntryCompany company, string externalId) =>
        ReadAsync(_factory, db => db.Jobs.AsNoTracking().SingleAsync(j => j.TenantId == company.Id && j.ExternalId == externalId));

    private Task<decimal> BalanceAsync(EntryCompany company, string customerCode) =>
        ReadAsync(_factory, db => db.NativeCustomerBalances.Where(b => b.TenantId == company.Id && b.CustomerCode == customerCode).Select(b => b.Balance).SingleAsync());

    private async Task<ErpWriteContext> AgentContextAsync(Guid tenantId, Guid userId, string username)
    {
        var (settings, mapping) = await ReadAsync(_factory, async db => (
            await db.ErpWriteSettings.AsNoTracking().SingleAsync(s => s.TenantId == tenantId),
            await db.MobileUserErpMappings.AsNoTracking().SingleOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == userId)));
        var leased = ErpWriteContextBuilder.Build(settings, mapping, username);
        return JsonSerializer.Deserialize<ErpWriteContext>(JsonSerializer.Serialize(leased, Web), Web)!;
    }
}
