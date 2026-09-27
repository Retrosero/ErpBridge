using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// SKT (son kullanma tarihi) kayıtları over <c>/api/v1/android/expiry</c>. Relational because every change
/// takes its number from the tenant counter inside its transaction and a rejected operation is rolled back
/// to a savepoint.
/// </summary>
public sealed class ExpiryRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private const string Password = "parola123";
    private const string ListPath = "/api/v1/android/expiry";
    private const string OpsPath = "/api/v1/android/expiry/ops";

    private readonly SqliteCentralApiFactory _factory;

    public ExpiryRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_record_entered_on_one_phone_is_seen_by_every_user_of_the_company()
    {
        var c = await CompanyAsync();
        var id = Guid.NewGuid();
        var ops = await OpsAsync(c.Ali, Upsert(id, stockCode: "STK-1", location: "A-3", expiryDate: "2027-03-15", quantity: 12.5m, barcode: "8690001"));

        ops.Results.Single().Status.Should().Be("applied");
        ops.Results.Single().ErrorCode.Should().BeNull();
        var written = ops.Records.Single();
        written.CreatedBy.Should().Be("Ali Saha");
        written.Quantity.Should().Be(12.5m);

        var seen = (await ListAsync(c.Patron)).Records.Should().ContainSingle(r => r.Id == id).Subject;
        seen.StockCode.Should().Be("STK-1");
        seen.Barcode.Should().Be("8690001");
        seen.Location.Should().Be("A-3");
        seen.ExpiryDate.Should().Be("2027-03-15");
        seen.Quantity.Should().Be(12.5m);
        seen.Closed.Should().BeFalse();
        seen.Deleted.Should().BeFalse();
        seen.CreatedBy.Should().Be("Ali Saha");
        seen.UpdatedSeq.Should().Be(written.UpdatedSeq);
    }

    [Fact]
    public async Task An_upsert_of_a_known_id_overwrites_it_and_is_pulled_incrementally()
    {
        var c = await CompanyAsync();
        var id = Guid.NewGuid();
        var other = Guid.NewGuid();
        await OpsAsync(c.Ali, Upsert(id, quantity: 10m, note: "ön sıra"), Upsert(other));
        var cursor = (await ListAsync(c.Patron)).LatestSeq;
        (await ListAsync(c.Patron, since: cursor)).Records.Should().BeEmpty();

        var edited = await OpsAsync(c.Patron, Upsert(id, location: "B-1", expiryDate: "2027-04-01", quantity: null, note: null, closed: true));
        edited.Results.Single().Status.Should().Be("applied");

        var changed = await ListAsync(c.Ali, since: cursor);
        var record = changed.Records.Should().ContainSingle().Subject;
        record.Id.Should().Be(id);
        record.Location.Should().Be("B-1");
        record.ExpiryDate.Should().Be("2027-04-01");
        record.Quantity.Should().BeNull("an upsert carries the full state; null empties the field");
        record.Note.Should().BeNull();
        record.Closed.Should().BeTrue();
        record.CreatedBy.Should().Be("Ali Saha", "the creator does not change on an edit");
        record.UpdatedSeq.Should().BeGreaterThan(cursor);
        changed.LatestSeq.Should().Be(record.UpdatedSeq);
        changed.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task A_delete_is_a_tombstone_in_the_list_and_a_deleted_record_is_not_edited_again()
    {
        var c = await CompanyAsync();
        var id = Guid.NewGuid();
        await OpsAsync(c.Ali, Upsert(id));
        var cursor = (await ListAsync(c.Ali)).LatestSeq;

        var deleted = await OpsAsync(c.Patron, DeleteOp(id));
        deleted.Results.Single().Status.Should().Be("applied");
        deleted.Records.Single().Deleted.Should().BeTrue();

        var pulled = (await ListAsync(c.Ali, since: cursor)).Records.Should().ContainSingle().Subject;
        pulled.Id.Should().Be(id);
        pulled.Deleted.Should().BeTrue("phones learn of the delete from the same pull");
        (await CountAsync(r => r.Id == id)).Should().Be(1, "a delete is soft");

        var again = await OpsAsync(c.Ali, DeleteOp(id));
        again.Results.Single().Status.Should().Be("applied", "deleting twice is idempotent");
        (await ListAsync(c.Ali, since: pulled.UpdatedSeq)).Records.Should().BeEmpty("a repeated delete is not a change");

        var edit = await OpsAsync(c.Ali, Upsert(id, note: "geç kalan düzenleme"));
        edit.Results.Single().Status.Should().Be("rejected");
        edit.Results.Single().ErrorCode.Should().Be("EXPIRY_NOT_FOUND");

        var unknown = await OpsAsync(c.Ali, DeleteOp(Guid.NewGuid()));
        unknown.Results.Single().ErrorCode.Should().Be("EXPIRY_NOT_FOUND");
    }

    [Fact]
    public async Task A_batch_sent_twice_is_applied_once()
    {
        var c = await CompanyAsync();
        var id = Guid.NewGuid();
        var op = Upsert(id);
        var first = await OpsAsync(c.Ali, op);
        first.Results.Single().Status.Should().Be("applied");
        var seq = first.Records.Single().UpdatedSeq;

        var second = await OpsAsync(c.Ali, op);
        second.Results.Single().Status.Should().Be("duplicate");
        second.Records.Should().BeEmpty();
        (await ListAsync(c.Ali, since: seq)).Records.Should().BeEmpty("a duplicate changes nothing");

        var sameBatch = await OpsAsync(c.Ali, DeleteOp(id, opId: op.OpId));
        sameBatch.Results.Single().Status.Should().Be("duplicate", "the op id decides, not the content");
        (await CountAsync(r => r.Id == id && !r.IsDeleted)).Should().Be(1);
    }

    [Fact]
    public async Task A_rejected_operation_rolls_back_alone_and_the_rest_of_the_batch_applies()
    {
        var c = await CompanyAsync();
        var good = Guid.NewGuid();
        var bad = Guid.NewGuid();
        var response = await OpsAsync(c.Ali,
            Upsert(bad, expiryDate: "2027-13-01"),
            Upsert(good),
            Upsert(Guid.NewGuid(), location: "  "),
            new ExpiryOp { OpId = Guid.Empty, Type = "upsert", Record = Input(Guid.NewGuid()) },
            new ExpiryOp { OpId = Guid.NewGuid(), Type = "archive", Record = Input(Guid.NewGuid()) });

        response.Results.Select(r => r.Status).Should().Equal("rejected", "applied", "rejected", "rejected", "rejected");
        response.Results.Select(r => r.ErrorCode).Should().Equal("EXPIRY_INVALID", null, "EXPIRY_INVALID", "EXPIRY_OP_ID_REQUIRED", "EXPIRY_OP_UNKNOWN");
        response.Results[0].Message.Should().Be("Son kullanma tarihi okunamadı.");
        response.Results[2].Message.Should().Be("Reyon/raf boş olamaz.");
        response.Records.Should().ContainSingle(r => r.Id == good);
        (await CountAsync(r => r.Id == bad)).Should().Be(0);

        // A rejected op id is not remembered: the same op sent again, fixed, applies.
        var retry = await OpsAsync(c.Ali, Upsert(bad, opId: response.Results[0].OpId));
        retry.Results.Single().Status.Should().Be("applied");
    }

    [Fact]
    public async Task Another_company_neither_sees_nor_changes_the_records()
    {
        var a = await CompanyAsync();
        var b = await CompanyAsync();
        var id = Guid.NewGuid();
        await OpsAsync(a.Ali, Upsert(id, note: "A firması"));

        (await ListAsync(b.Patron)).Records.Should().BeEmpty();

        var overwrite = await OpsAsync(b.Patron, Upsert(id, note: "B firması"));
        overwrite.Results.Single().Status.Should().Be("rejected");
        overwrite.Results.Single().ErrorCode.Should().Be("EXPIRY_NOT_FOUND");
        overwrite.Records.Should().BeEmpty();
        var delete = await OpsAsync(b.Patron, DeleteOp(id));
        delete.Results.Single().ErrorCode.Should().Be("EXPIRY_NOT_FOUND");

        var mine = (await ListAsync(a.Patron)).Records.Single(r => r.Id == id);
        mine.Note.Should().Be("A firması");
        mine.Deleted.Should().BeFalse();
    }

    [Fact]
    public async Task The_list_pages_in_change_order()
    {
        var c = await CompanyAsync();
        var ids = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToArray();
        await OpsAsync(c.Ali, ids.Select(id => (object)Upsert(id)).ToArray());

        var collected = new List<ExpiryRecordDto>();
        long since = 0;
        var pages = 0;
        while (true)
        {
            var page = await ListAsync(c.Patron, since, take: 2);
            pages++;
            collected.AddRange(page.Records);
            page.Records.Length.Should().BeLessThanOrEqualTo(2);
            if (page.Records.Length > 0) page.LatestSeq.Should().Be(page.Records[^1].UpdatedSeq);
            since = page.LatestSeq;
            if (!page.HasMore) break;
        }

        pages.Should().Be(3);
        collected.Select(r => r.Id).Should().Equal(ids, "one batch applies in order, each op with the next number");
        collected.Select(r => r.UpdatedSeq).Should().BeInAscendingOrder().And.OnlyHaveUniqueItems();
        var last = await ListAsync(c.Patron, since);
        last.Records.Should().BeEmpty();
        last.LatestSeq.Should().Be(since, "an empty page hands the cursor back");

        (await ListAsync(c.Patron, 0, take: 0)).Records.Should().HaveCount(1, "take is clamped to at least one");
    }

    [Fact]
    public async Task A_batch_over_the_limit_is_refused_whole()
    {
        var c = await CompanyAsync();
        var ops = Enumerable.Range(0, 201).Select(_ => (object)Upsert(Guid.NewGuid())).ToArray();

        var response = await _factory.CreateClient().PostJsonAsync(OpsPath, new { ops }, c.Ali);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.ReadAsJsonAsync<ApiError>();
        error.ErrorCode.Should().Be("EXPIRY_BATCH_TOO_LARGE");
        error.Message.Should().Be("Bir seferde en çok 200 işlem gönderilebilir.");
        (await CountAsync(r => r.TenantId == c.Id)).Should().Be(0);
    }

    [Fact]
    public async Task The_endpoints_need_a_signed_in_user()
    {
        var client = _factory.CreateClient();
        (await client.GetAsync(ListPath)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.PostJsonAsync(OpsPath, new { ops = Array.Empty<object>() })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ---- helpers -----------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Ali);

    private static ExpiryRecordInput Input(
        Guid id, string stockCode = "STK-1", string location = "A-3", string expiryDate = "2027-03-15",
        decimal? quantity = 4m, string? barcode = null, string? note = null, bool closed = false) => new()
    {
        Id = id,
        StockCode = stockCode,
        Barcode = barcode,
        ProductName = "Süt 1L",
        Location = location,
        ExpiryDate = expiryDate,
        Quantity = quantity,
        Note = note,
        Closed = closed,
    };

    private static ExpiryOp Upsert(
        Guid id, string stockCode = "STK-1", string location = "A-3", string expiryDate = "2027-03-15",
        decimal? quantity = 4m, string? barcode = null, string? note = null, bool closed = false, Guid? opId = null) =>
        new()
        {
            OpId = opId ?? Guid.NewGuid(),
            Type = "upsert",
            Record = Input(id, stockCode, location, expiryDate, quantity, barcode, note, closed),
        };

    private static ExpiryOp DeleteOp(Guid id, Guid? opId = null) =>
        new() { OpId = opId ?? Guid.NewGuid(), Type = "delete", Record = new ExpiryRecordInput { Id = id } };

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"SKT-{suffix}", $"Expiry tenant {suffix}");
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
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        return new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "ali", $"DEV-A-{suffix}"));
    }

    private async Task<ExpiryOpsResponse> OpsAsync(string token, params object[] ops)
    {
        var response = await _factory.CreateClient().PostJsonAsync(OpsPath, new { ops }, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<ExpiryOpsResponse>();
    }

    private async Task<ExpiryListResponse> ListAsync(string token, long since = 0, int? take = null)
    {
        var path = $"{ListPath}?changedSinceSeq={since}" + (take is { } t ? $"&take={t}" : string.Empty);
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<ExpiryListResponse>();
    }

    private async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<StockExpiryRecord, bool>> predicate)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return await db.StockExpiryRecords.CountAsync(predicate);
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.280" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
