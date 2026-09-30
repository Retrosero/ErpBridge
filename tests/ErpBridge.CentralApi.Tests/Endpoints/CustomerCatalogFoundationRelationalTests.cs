using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Mobile;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU S2: the catalog tables, the operator's module, the company code the catalog address is built
/// from and the configuration defaults. SQLite enforces the filtered unique indexes and the cascades PostgreSQL has.
/// </summary>
public sealed class CustomerCatalogFoundationRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public CustomerCatalogFoundationRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Live_accounts_have_unique_usernames_and_customers_and_a_deleted_one_frees_both()
    {
        var tenantId = await NewTenantAsync();
        var otherTenantId = await NewTenantAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var first = Account(tenantId, "bayi", "C1");
        db.CatalogAccounts.Add(first);
        await db.SaveChangesAsync();

        async Task ShouldConflictAsync(CatalogAccount account)
        {
            db.CatalogAccounts.Add(account);
            await db.Invoking(d => d.SaveChangesAsync()).Should().ThrowAsync<DbUpdateException>();
            db.Entry(account).State = EntityState.Detached;
        }
        await ShouldConflictAsync(Account(tenantId, "bayi", "C2"));
        await ShouldConflictAsync(Account(tenantId, "bayi2", "C1"));

        db.CatalogAccounts.Add(Account(otherTenantId, "bayi", "C1"));
        await db.SaveChangesAsync();
        // Deleted first, then reused: within one batch the index would still see both rows live.
        first.DeletedAtMs = Now();
        await db.SaveChangesAsync();
        db.CatalogAccounts.Add(Account(tenantId, "bayi", "C1"));
        await db.SaveChangesAsync();

        (await db.CatalogAccounts.CountAsync(a => a.TenantId == tenantId)).Should().Be(2, "the deleted account stays for its history");
    }

    [Fact]
    public async Task Every_catalog_row_goes_with_its_company_and_a_picture_takes_its_files_along()
    {
        var tenantId = await NewTenantAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var now = Now();
        var account = Account(tenantId, "bayi", "C1");
        var kept = new CatalogImage { TenantId = tenantId, StockCode = "S1", SourceHash = "h1", CreatedAtMs = now, HasLarge = true, SizeBytes = 3 };
        var removed = new CatalogImage { TenantId = tenantId, StockCode = "S1", SourceHash = "h2", CreatedAtMs = now, HasSmall = true, HasLarge = true, SizeBytes = 4 };
        db.AddRange(
            new CatalogSettings { TenantId = tenantId, IsEnabled = true, DefaultPriceListNo = 2, Revision = 1, UpdatedAtMs = now },
            new CatalogCategorySetting { TenantId = tenantId, CategoryKey = "İÇECEK", SortOrder = 1, UpdatedAtMs = now },
            new CatalogProductSetting { TenantId = tenantId, StockCode = "S1", NoDiscount = true, CartonOnly = true, CartonQuantity = 12, UpdatedAtMs = now },
            account,
            kept,
            removed,
            new CatalogImageBlob { ImageId = kept.Id, Variant = CatalogImageVariants.Large, Data = [1, 2, 3] },
            new CatalogImageBlob { ImageId = removed.Id, Variant = CatalogImageVariants.Small, Data = [1] },
            new CatalogImageBlob { ImageId = removed.Id, Variant = CatalogImageVariants.Large, Data = [1, 2, 3] },
            new CatalogOrder
            {
                Id = Guid.NewGuid(), TenantId = tenantId, AccountId = account.Id, CustomerCode = "C1", CustomerName = "Bayi",
                AccountUsername = "bayi", No = "KT-ABC234", PriceListNo = 2, DiscountPercent = 7.5m, Total = 118.8m, LineCount = 1,
                LinesJson = "[{\"stockCode\":\"S1\",\"quantity\":12}]", SubmittedAtMs = now, UpdatedAtMs = now,
            });
        await db.SaveChangesAsync();

        await db.CatalogImages.Where(i => i.Id == removed.Id).ExecuteDeleteAsync();
        (await db.CatalogImageBlobs.Select(b => b.ImageId).ToListAsync()).Should().NotContain(removed.Id);
        (await db.CatalogImageBlobs.CountAsync(b => b.ImageId == kept.Id)).Should().Be(1);

        await db.Tenants.Where(t => t.Id == tenantId).ExecuteDeleteAsync();
        (await db.CatalogSettings.AnyAsync(x => x.TenantId == tenantId)).Should().BeFalse();
        (await db.CatalogCategorySettings.AnyAsync(x => x.TenantId == tenantId)).Should().BeFalse();
        (await db.CatalogProductSettings.AnyAsync(x => x.TenantId == tenantId)).Should().BeFalse();
        (await db.CatalogAccounts.AnyAsync(x => x.TenantId == tenantId)).Should().BeFalse();
        (await db.CatalogImages.AnyAsync(x => x.TenantId == tenantId)).Should().BeFalse();
        (await db.CatalogImageBlobs.AnyAsync(b => b.ImageId == kept.Id)).Should().BeFalse();
        (await db.CatalogOrders.AnyAsync(x => x.TenantId == tenantId)).Should().BeFalse();
    }

    [Fact]
    public async Task A_second_upload_of_the_same_original_and_a_repeated_request_number_are_refused()
    {
        var tenantId = await NewTenantAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        db.CatalogImages.Add(new CatalogImage { TenantId = tenantId, StockCode = "S1", SourceHash = "same", CreatedAtMs = Now() });
        await db.SaveChangesAsync();

        db.CatalogImages.Add(new CatalogImage { TenantId = tenantId, StockCode = "S1", SourceHash = "same", CreatedAtMs = Now() });
        await db.Invoking(d => d.SaveChangesAsync()).Should().ThrowAsync<DbUpdateException>("the server finds the existing picture by its source hash");
        db.ChangeTracker.Clear();

        db.CatalogOrders.Add(Order(tenantId, "KT-SAME22"));
        await db.SaveChangesAsync();
        db.CatalogOrders.Add(Order(tenantId, "KT-SAME22"));
        await db.Invoking(d => d.SaveChangesAsync()).Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task A_company_without_a_code_gets_one_the_first_time_it_is_asked_and_keeps_it()
    {
        var tenantId = await NewTenantAsync();
        using var scope = _factory.Services.CreateScope();
        var seats = scope.ServiceProvider.GetRequiredService<MobileSeatService>();

        var code = await seats.EnsureTenantCodeAsync(tenantId, CancellationToken.None);

        code.Should().MatchRegex("^[A-Z2-9]{8}$");
        (await seats.EnsureTenantCodeAsync(tenantId, CancellationToken.None)).Should().Be(code);
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.Tenants.AsNoTracking().SingleAsync(t => t.Id == tenantId)).Code.Should().Be(code);
        (await seats.EnsureTenantCodeAsync(Guid.NewGuid(), CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task An_existing_code_is_never_replaced_and_the_in_memory_host_makes_one_too()
    {
        var tenantId = await NewTenantAsync(code: "KODU2345");
        using (var scope = _factory.Services.CreateScope())
            (await scope.ServiceProvider.GetRequiredService<MobileSeatService>().EnsureTenantCodeAsync(tenantId, CancellationToken.None))
                .Should().Be("KODU2345");

        using var memory = new CentralApiFactory();
        var (tenant, _) = await memory.SeedTenantAsync();
        using var memoryScope = memory.Services.CreateScope();
        var made = await memoryScope.ServiceProvider.GetRequiredService<MobileSeatService>().EnsureTenantCodeAsync(tenant.Id, CancellationToken.None);
        made.Should().MatchRegex("^[A-Z2-9]{8}$");
        (await memoryScope.ServiceProvider.GetRequiredService<MobileSeatService>().EnsureTenantCodeAsync(tenant.Id, CancellationToken.None)).Should().Be(made);
    }

    [Fact]
    public async Task The_operator_switches_the_catalog_module_on_and_the_session_lists_it()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"KT-{suffix}", $"Catalog tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        (await client.PutJsonAsync($"{basePath}/subscription", new { seats = 2, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostJsonAsync($"{basePath}/users", new { username = "patron", fullName = "Patron", password = Password, role = "ADMIN" }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        (await client.PutJsonAsync($"{basePath}/modules", new { modules = new[] { "CUSTOMER_CATALOG", TenantModules.XmlImport } }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        var login = await client.PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username = "patron", password = Password, deviceId = $"DEV-{suffix}", appVersion = "1.5.300" });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = (await login.ReadAsJsonAsync<MobileLoginResponse>()).Session;
        session.Modules.Should().Equal(TenantModules.CustomerCatalog, TenantModules.XmlImport);
        session.Permissions[ErpBridge.CentralApi.Permissions.PermissionKeys.CustomerCatalogManage].Should().BeTrue();
    }

    [Fact]
    public void The_configuration_defaults_match_the_contract()
    {
        var options = _factory.Services.GetRequiredService<IOptions<CustomerCatalogOptions>>().Value;

        options.PublicHost.Should().BeEmpty("the catalog is served nowhere until the operator sets the host");
        options.TokenDays.Should().Be(30);
        options.SessionHours.Should().Be(12);
        options.MaxImageBytesLarge.Should().Be(1_048_576);
        options.MaxImageBytesSmall.Should().Be(204_800);
        options.MaxImagesPerProduct.Should().Be(8);
        options.TenantImageQuotaBytes.Should().Be(1_073_741_824);
        options.MaxOpenOrders.Should().Be(20);
        options.MaxOrderLines.Should().Be(200);
        options.WebRoot.Should().Be("wwwroot/katalog");
    }

    private async Task<Guid> NewTenantAsync(string? code = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var tenant = new Tenant { Name = $"Katalog {Guid.NewGuid():N}", Code = code };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        return tenant.Id;
    }

    private static CatalogAccount Account(Guid tenantId, string username, string customerCode) => new()
    {
        TenantId = tenantId,
        CustomerCode = customerCode,
        CustomerName = "Bayi " + customerCode,
        Username = username,
        PasswordHash = "hash",
        DiscountPercent = 7.5m,
        CreatedAtMs = Now(),
        UpdatedAtMs = Now(),
        PasswordChangedAtMs = Now(),
        CreatedByName = "Patron",
    };

    private static CatalogOrder Order(Guid tenantId, string no) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        AccountId = Guid.NewGuid(),
        CustomerCode = "C1",
        CustomerName = "Bayi",
        AccountUsername = "bayi",
        No = no,
        SubmittedAtMs = Now(),
        UpdatedAtMs = Now(),
    };

    private static long Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
