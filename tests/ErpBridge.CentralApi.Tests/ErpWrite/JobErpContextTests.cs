using System.Net.Http.Json;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.ErpWrite;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.ErpWrite;

/// <summary>
/// Goal ERP yazım Y1d: each leased job carries how to write it into the company's ERP — the company
/// settings merged with the creator's ERP counterparts, read at lease time.
/// </summary>
public sealed class JobErpContextTests : IClassFixture<CentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private readonly CentralApiFactory _factory;

    public JobErpContextTests(CentralApiFactory factory) => _factory = factory;

    /// <summary>
    /// ERP yazım 3 Y2a: alış ayarları ajana ulaşmalı. Alış deposu boşsa satış deposuna düşmek
    /// writer'ın işi; burada önemli olan firmanın söylediğinin yolda kaybolmaması.
    /// </summary>
    [Fact]
    public void The_purchase_settings_reach_the_agent()
    {
        var settings = new ErpWriteSettings
        {
            DefaultWarehouseNo = 1,
            PurchaseWarehouseNo = 7,
            PurchasePricesIncludeVat = true,
        };

        var context = ErpWriteContextBuilder.Build(settings, user: null, createdByUsername: null);

        context.WarehouseNo.Should().Be(1);
        context.PurchaseWarehouseNo.Should().Be(7, "ayrı bir alış deposu tutan firma bunu söyleyebilmeli");
        context.PurchasePricesIncludeVat.Should().BeTrue();
    }

    /// <summary>Hiç ayar yoksa varsayılan "KDV hariç"tir: sessizce KDV'li saymak faturayı şişirirdi.</summary>
    [Fact]
    public void Without_a_setting_the_supplier_price_is_taken_as_excluding_vat()
    {
        var context = ErpWriteContextBuilder.Build(settings: null, user: null, createdByUsername: null);

        context.PurchasePricesIncludeVat.Should().BeFalse();
        context.PurchaseWarehouseNo.Should().BeNull();
    }

    [Fact]
    public void A_user_value_wins_and_a_missing_one_falls_back_to_the_company()
    {
        var settings = new ErpWriteSettings
        {
            SalesDocumentKind = SalesDocumentKinds.Invoice, InvoiceSeries = "T", CollectionSeries = "M",
            DefaultWarehouseNo = 1, DefaultCashCode = "001", DefaultCardBankCode = "14", DefaultTransferBankCode = "04",
            DefaultErpUserNo = 1, DefaultSalespersonCode = "MERKEZ", DefaultPriceListNo = 2,
            ResponsibilityCenterCode = "SM1", ProjectCode = " ", DeliveryDayOffset = 2,
        };
        var user = new MobileUserErpMapping { WarehouseNo = 3, CashCode = "  ", InvoiceSeries = "", SalespersonCode = "PLS01", ErpUserNo = 4 };

        var context = ErpWriteContextBuilder.Build(settings, user, "plasiyer1");

        context.Should().BeEquivalentTo(new JobErpContextResponse
        {
            SalesDocumentKind = "invoice", OrderApprovalMode = "approved",
            Series = new JobErpSeriesResponse { Order = "", Dispatch = "", Invoice = "", Return = "", Collection = "M" },
            WarehouseNo = 3, CashCode = "001", CardBankCode = "14", TransferBankCode = "04",
            ErpUserNo = 4, SalespersonCode = "PLS01", PriceListNo = 2,
            ChequePortfolioCode = "ÇEK", NotePortfolioCode = "SENET", CreatedByUsername = "plasiyer1",
            ResponsibilityCenterCode = "SM1", ProjectCode = null, DeliveryDayOffset = 2,
        }, "every company setting travels with the job; a blank user code does not hide the company's, an empty user series means series-less on purpose");

        ErpWriteContextBuilder.Build(null, null, null).Should().Match<JobErpContextResponse>(c =>
            c.SalesDocumentKind == "order" && c.WarehouseNo == null && c.Series.Invoice == "" && c.ChequePortfolioCode == "ÇEK");
    }

    [Fact]
    public async Task A_leased_job_carries_the_context_and_a_fixed_mapping_reaches_the_retry()
    {
        var (tenant, token, user) = await SeedAsync(TenantDataSources.Erp);
        await using (var db = _factory.CreateDbContext())
        {
            db.ErpWriteSettings.Add(new ErpWriteSettings { TenantId = tenant.Id, SalesDocumentKind = SalesDocumentKinds.Invoice, DefaultWarehouseNo = 1, DefaultCashCode = "001" });
            await db.SaveChangesAsync();
        }
        var job = await SeedJobAsync(tenant.Id, user.Id);

        var first = await LeaseAsync(token, job.Id);
        first.ErpContext.Should().NotBeNull();
        first.ErpContext!.Should().Match<JobErpContextResponse>(c => c.SalesDocumentKind == "invoice" && c.CashCode == "001" && c.CreatedByUsername == user.Username);

        await using (var db = _factory.CreateDbContext())
        {
            db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = user.Id, TenantId = tenant.Id, CashCode = "002" });
            var stored = await db.Jobs.SingleAsync(j => j.Id == job.Id);
            stored.Status = JobStatus.Pending; // the operator retries after fixing the mapping
            await db.SaveChangesAsync();
        }

        (await LeaseAsync(token, job.Id)).ErpContext!.CashCode.Should().Be("002");
    }

    [Fact]
    public async Task A_company_without_an_erp_gets_no_context()
    {
        var (tenant, token, user) = await SeedAsync(TenantDataSources.Native);
        var job = await SeedJobAsync(tenant.Id, user.Id);

        (await LeaseAsync(token, job.Id)).ErpContext.Should().BeNull();
    }

    private async Task<(Tenant Tenant, string Token, MobileUser User)> SeedAsync(string dataSource)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: $"ERPCTX-{suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-ERPCTX-{suffix}");
        var user = new MobileUser { TenantId = tenant.Id, Username = $"plasiyer-{suffix}", FullName = "Plasiyer", PasswordHash = "x" };
        await using (var db = _factory.CreateDbContext())
        {
            (await db.Tenants.SingleAsync(t => t.Id == tenant.Id)).DataSource = dataSource;
            db.MobileUsers.Add(user);
            await db.SaveChangesAsync();
        }
        return (tenant, _factory.IssueTestJwt(agent.Id, tenant.Id), user);
    }

    private async Task<Job> SeedJobAsync(Guid tenantId, Guid userId)
    {
        var job = new Job { TenantId = tenantId, ExternalId = $"MOB-SO-{Guid.NewGuid():N}", DocumentType = "sales_order", PayloadJson = "{}", CreatedByUserId = userId };
        await using var db = _factory.CreateDbContext();
        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        return job;
    }

    private async Task<JobResponse> LeaseAsync(string token, Guid jobId)
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/jobs/pending", token);
        response.EnsureSuccessStatusCode();
        var jobs = await response.Content.ReadFromJsonAsync<JobResponse[]>(Web);
        return jobs!.Single(j => j.JobId == jobId);
    }
}
