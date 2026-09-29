using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Bekleyen siparişler over <c>/api/v1/android/suspended-sales</c>. Relational because every change takes its
/// number from the tenant counter inside its transaction and a rejected operation is rolled back to a savepoint.
/// </summary>
public sealed class SuspendedSaleRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private const string Password = "parola123";
    private const string ListPath = "/api/v1/android/suspended-sales";
    private const string OpsPath = "/api/v1/android/suspended-sales/ops";

    private readonly SqliteCentralApiFactory _factory;

    public SuspendedSaleRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_sale_parked_on_one_phone_is_seen_by_every_user_of_the_company()
    {
        var c = await CompanyAsync();
        var id = Guid.NewGuid();
        var ops = await OpsAsync(c.Ali, Park(id));

        ops.Results.Single().Status.Should().Be("applied");
        var written = ops.Sales.Single();
        written.CreatedBy.Should().Be("Ali Saha");

        var seen = (await ListAsync(c.Veli)).Sales.Should().ContainSingle(s => s.Id == id).Subject;
        seen.DocNo.Should().Be("BS-1001");
        seen.CustomerId.Should().Be("C-7");
        seen.CustomerName.Should().Be("Yıldız Market");
        seen.Warehouse.Should().Be("Merkez");
        seen.Note.Should().Be("öğlen teslim");
        seen.TotalAmount.Should().Be(190m);
        seen.Deleted.Should().BeFalse();
        seen.CreatedByUserId.Should().Be(written.CreatedByUserId);
        var line = seen.Lines.Should().ContainSingle().Subject;
        line.Barcode.Should().Be("8690001");
        line.StockCode.Should().Be("STK-1");
        line.ProductName.Should().Be("Çay 1 kg");
        line.Quantity.Should().Be(2m);
        line.Price.Should().Be(100m);
        line.LineDiscountPercent.Should().Be(5m);
    }

    [Fact]
    public async Task Opening_a_sale_claims_it_for_one_phone_and_a_second_claim_is_told_who_took_it()
    {
        var c = await CompanyAsync();
        var id = Guid.NewGuid();
        await OpsAsync(c.Ali, Park(id));
        var cursor = (await ListAsync(c.Ali)).LatestSeq;

        var first = await OpsAsync(c.Veli, ClaimOp(id));
        first.Results.Single().Status.Should().Be("applied");

        var second = await OpsAsync(c.Patron, ClaimOp(id));
        var result = second.Results.Single();
        result.Status.Should().Be("rejected");
        result.ErrorCode.Should().Be("SUSPENDED_SALE_TAKEN");
        result.Message.Should().Contain("Veli Satış");
        second.Sales.Should().ContainSingle(s => s.Id == id && s.Deleted, "the phone learns to drop it");

        var tombstone = (await ListAsync(c.Ali, since: cursor)).Sales.Should().ContainSingle().Subject;
        tombstone.Deleted.Should().BeTrue();
        tombstone.ClosedReason.Should().Be("claimed");
        tombstone.ClosedBy.Should().Be("Veli Satış");
        tombstone.Lines.Should().BeEmpty();
    }

    [Fact]
    public async Task Only_the_creator_or_a_manager_deletes_a_sale()
    {
        var c = await CompanyAsync();
        var alis = Guid.NewGuid();
        var other = Guid.NewGuid();
        await OpsAsync(c.Ali, Park(alis), Park(other));

        var refused = (await OpsAsync(c.Veli, DeleteOp(alis))).Results.Single();
        refused.Status.Should().Be("rejected");
        refused.ErrorCode.Should().Be("SUSPENDED_SALE_FORBIDDEN");

        (await OpsAsync(c.Ali, DeleteOp(alis))).Results.Single().Status.Should().Be("applied");
        (await OpsAsync(c.Ali, DeleteOp(alis))).Results.Single().Status.Should().Be("applied", "deleting it again is a no-op");
        (await OpsAsync(c.Patron, DeleteOp(other))).Results.Single().Status.Should().Be("applied", "an admin may delete anyone's");

        var sales = (await ListAsync(c.Veli)).Sales;
        sales.Should().OnlyContain(s => s.Deleted && s.ClosedReason == "deleted");
    }

    [Fact]
    public async Task Only_the_creator_or_a_manager_overwrites_a_sale_and_a_claimed_one_is_not_parked_again()
    {
        var c = await CompanyAsync();
        var id = Guid.NewGuid();
        await OpsAsync(c.Ali, Park(id));

        (await OpsAsync(c.Veli, Park(id, note: "değişti"))).Results.Single().ErrorCode.Should().Be("SUSPENDED_SALE_FORBIDDEN");
        (await OpsAsync(c.Ali, Park(id, note: "değişti"))).Results.Single().Status.Should().Be("applied");
        (await ListAsync(c.Veli)).Sales.Single(s => s.Id == id).Note.Should().Be("değişti");

        await OpsAsync(c.Veli, ClaimOp(id));
        (await OpsAsync(c.Ali, Park(id))).Results.Single().ErrorCode.Should().Be("SUSPENDED_SALE_TAKEN");
    }

    [Fact]
    public async Task A_batch_sent_twice_is_applied_once_and_a_rejected_op_rolls_back_alone()
    {
        var c = await CompanyAsync();
        var good = Guid.NewGuid();
        var bad = Guid.NewGuid();
        var opId = Guid.NewGuid();
        var batch = new object[] { Park(good, opId: opId), Park(bad, lines: []) };

        var first = await OpsAsync(c.Ali, batch);
        first.Results.Select(r => r.Status).Should().Equal("applied", "rejected");
        first.Results[1].ErrorCode.Should().Be("SUSPENDED_SALE_INVALID");

        var again = await OpsAsync(c.Ali, Park(good, opId: opId));
        again.Results.Single().Status.Should().Be("duplicate");

        (await CountAsync(c.TenantId)).Should().Be(1);
    }

    [Fact]
    public async Task Another_company_neither_sees_nor_claims_the_sale()
    {
        var c = await CompanyAsync();
        var other = await CompanyAsync();
        var id = Guid.NewGuid();
        await OpsAsync(c.Ali, Park(id));

        (await ListAsync(other.Ali)).Sales.Should().BeEmpty();
        (await OpsAsync(other.Ali, ClaimOp(id))).Results.Single().ErrorCode.Should().Be("SUSPENDED_SALE_NOT_FOUND");
        (await OpsAsync(other.Ali, Park(id))).Results.Single().ErrorCode.Should().Be("SUSPENDED_SALE_NOT_FOUND");
        (await ListAsync(c.Veli)).Sales.Single(s => s.Id == id).Deleted.Should().BeFalse();
    }

    [Fact]
    public async Task An_oversized_batch_is_refused_whole_and_a_call_without_a_token_is_401()
    {
        var c = await CompanyAsync();
        var ops = Enumerable.Range(0, 201).Select(_ => (object)Park(Guid.NewGuid())).ToArray();
        var response = await _factory.CreateClient().PostJsonAsync(OpsPath, new { ops }, c.Ali);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("SUSPENDED_SALE_BATCH_TOO_LARGE");

        (await _factory.CreateClient().GetAsync(ListPath)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record Company(Guid TenantId, string Patron, string Ali, string Veli);

    private static SuspendedSaleLineDto Line() => new()
    {
        Barcode = "8690001",
        StockCode = "STK-1",
        ProductName = "Çay 1 kg",
        Quantity = 2m,
        Price = 100m,
        LineDiscountPercent = 5m,
    };

    private static SuspendedSaleOp Park(Guid id, string note = "öğlen teslim", SuspendedSaleLineDto[]? lines = null, Guid? opId = null) =>
        new()
        {
            OpId = opId ?? Guid.NewGuid(),
            Type = "upsert",
            Sale = new SuspendedSaleInput
            {
                Id = id,
                DocNo = "BS-1001",
                CustomerId = "C-7",
                CustomerName = "Yıldız Market",
                Warehouse = "Merkez",
                Note = note,
                TotalAmount = 190m,
                Lines = lines ?? [Line()],
            },
        };

    private static SuspendedSaleOp ClaimOp(Guid id) =>
        new() { OpId = Guid.NewGuid(), Type = "claim", Sale = new SuspendedSaleInput { Id = id } };

    private static SuspendedSaleOp DeleteOp(Guid id) =>
        new() { OpId = Guid.NewGuid(), Type = "delete", Sale = new SuspendedSaleInput { Id = id } };

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"BS-{suffix}", $"Suspended tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        (await client.PutJsonAsync($"{basePath}/subscription", new { seats = 4, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        async Task Create(string username, string fullName, string role) =>
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        await Create("patron", "Patron", "ADMIN");
        await Create("ali", "Ali Saha", "SALES");
        await Create("veli", "Veli Satış", "SALES");
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        return new Company(
            tenant.Id,
            await LoginAsync(code, "patron", $"DEV-P-{suffix}"),
            await LoginAsync(code, "ali", $"DEV-A-{suffix}"),
            await LoginAsync(code, "veli", $"DEV-V-{suffix}"));
    }

    private async Task<SuspendedSaleOpsResponse> OpsAsync(string token, params object[] ops)
    {
        var response = await _factory.CreateClient().PostJsonAsync(OpsPath, new { ops }, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<SuspendedSaleOpsResponse>();
    }

    private async Task<SuspendedSaleListResponse> ListAsync(string token, long since = 0)
    {
        var response = await _factory.CreateClient().GetAsync($"{ListPath}?changedSinceSeq={since}", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<SuspendedSaleListResponse>();
    }

    private async Task<int> CountAsync(Guid tenantId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return await db.SuspendedSales.CountAsync(s => s.TenantId == tenantId);
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.284" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
