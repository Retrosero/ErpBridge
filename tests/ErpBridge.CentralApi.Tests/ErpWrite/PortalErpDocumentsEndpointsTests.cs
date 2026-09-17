using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.ErpWrite;

/// <summary>
/// Goal ERP yazım Y5a: the portal lists documents sent to the ERP with their state, ERP number and reason, and an
/// administrator sends a failed one to the agent again.
/// </summary>
public sealed class PortalErpDocumentsEndpointsTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private readonly SqliteCentralApiFactory _factory;

    public PortalErpDocumentsEndpointsTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task The_list_shows_state_number_reason_sender_and_customer_newest_first()
    {
        var c = await CompanyAsync(native: false);
        var ali = await UserIdAsync(c, "ali");
        var now = DateTimeOffset.UtcNow;
        var written = await SeedJobAsync(c, "MOB-SO-1", "sales_order", JobStatus.Succeeded, ali, now.AddMinutes(-30), "120.001", "Bakkal Ali", 820.8m,
            new JobAckRecord { Status = "succeeded", ErpDocumentSeries = "T", ErpDocumentNumber = 1234, AckedAtUtc = now });
        var failed = await SeedJobAsync(c, "MOB-TH-1", "collection", JobStatus.Failed, ali, now.AddMinutes(-10), "120.002", "Market Veli", 500m,
            new JobAckRecord { Status = "failed", ErrorCode = "ERP_MAPPING_MISSING", ErrorMessage = "Portal'da kasa kodu eşlemesi eksik.", AckedAtUtc = now });
        await SeedJobAsync(c, "MOB-SO-OLD", "sales_order", JobStatus.Succeeded, ali, now.AddDays(-20), "120.001", "Bakkal Ali", 1m);

        var page = await ListAsync(c.Patron, "");

        page.Total.Should().Be(2, "the default window is the last seven days");
        page.Items.Select(i => i.ExternalId).Should().Equal("MOB-TH-1", "MOB-SO-1");
        var receipt = page.Items[0];
        receipt.Should().Match<PortalErpDocumentDto>(i => i.JobId == failed && i.State == "failed" && i.ErrorCode == "ERP_MAPPING_MISSING"
            && i.Message == "Portal'da kasa kodu eşlemesi eksik." && i.UserName == "ali" && i.CustomerCode == "120.002" && i.Amount == 500m && i.CanRetry);
        page.Items[1].Should().Match<PortalErpDocumentDto>(i => i.JobId == written && i.State == "written" && i.ErpDocumentNo == "T-1234" && i.Message == null && !i.CanRetry);

        (await ListAsync(c.Patron, "?state=failed")).Items.Should().ContainSingle(i => i.ExternalId == "MOB-TH-1");
        (await ListAsync(c.Patron, "?customer=bakkal")).Items.Should().ContainSingle(i => i.ExternalId == "MOB-SO-1");
        (await ListAsync(c.Patron, "?documentType=collection")).Total.Should().Be(1);
        (await ListAsync(c.Patron, $"?from={Day(now.AddDays(-25))}&to={Day(now)}")).Total.Should().Be(3);
        (await ListAsync(c.Patron, "?pageSize=1&page=2")).Items.Should().ContainSingle(i => i.ExternalId == "MOB-SO-1");
        (await ListAsync(c.Sef, "")).Items.Should().OnlyContain(i => !i.CanRetry, "only an administrator retries");
    }

    [Fact]
    public async Task An_administrator_sends_a_failed_document_again_with_fresh_attempts()
    {
        var c = await CompanyAsync(native: false);
        var job = await SeedJobAsync(c, "MOB-SR-1", "sales_return", JobStatus.Failed, null, DateTimeOffset.UtcNow, "120.001", "Bakkal Ali", 144m);

        (await SendAsync(HttpMethod.Post, c.Sef, $"/api/v1/portal/erp-documents/{job}/retry")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var retried = await SendAsync(HttpMethod.Post, c.Patron, $"/api/v1/portal/erp-documents/{job}/retry");
        retried.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonSerializer.Deserialize<PortalErpRetryResponse>(await retried.Content.ReadAsStringAsync(), Web)!.State.Should().Be("pending");

        await using (var db = _factory.CreateDbContext())
        {
            (await db.Jobs.SingleAsync(j => j.Id == job)).Should().Match<Job>(j =>
                j.Status == JobStatus.Pending && j.RetryCount == 0 && j.NextAttemptAtMs == null && j.CompletedAtUtc == null && j.LastError == null);
        }
        var again = await SendAsync(HttpMethod.Post, c.Patron, $"/api/v1/portal/erp-documents/{job}/retry");
        again.StatusCode.Should().Be(HttpStatusCode.Conflict, "a document already waiting is not sent twice");
        (await again.Content.ReadAsStringAsync()).Should().Contain("JOB_NOT_RETRYABLE");
        (await SendAsync(HttpMethod.Post, c.Patron, $"/api/v1/portal/erp-documents/{Guid.NewGuid()}/retry")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Salespeople_companies_without_an_erp_and_bad_queries_are_refused()
    {
        var c = await CompanyAsync(native: false);
        (await SendAsync(HttpMethod.Get, c.Ali, "/api/v1/portal/erp-documents")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        foreach (var query in new[] { "?state=lost", "?pageSize=101", "?from=2026-01-01&to=2026-03-01", "?from=17.09.2026" })
            (await SendAsync(HttpMethod.Get, c.Patron, "/api/v1/portal/erp-documents" + query)).StatusCode.Should().Be(HttpStatusCode.BadRequest, query);

        var native = await CompanyAsync(native: true);
        (await SendAsync(HttpMethod.Get, native.Patron, "/api/v1/portal/erp-documents")).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private static string Day(DateTimeOffset instant) =>
        ErpBridge.CentralApi.Portal.PortalReports.IstanbulDay(instant).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    private async Task<PortalErpDocumentsResponse> ListAsync(string token, string query)
    {
        var response = await SendAsync(HttpMethod.Get, token, "/api/v1/portal/erp-documents" + query);
        response.StatusCode.Should().Be(HttpStatusCode.OK, query);
        return JsonSerializer.Deserialize<PortalErpDocumentsResponse>(await response.Content.ReadAsStringAsync(), Web)!;
    }

    private async Task<Guid> SeedJobAsync(Company c, string externalId, string type, JobStatus status, Guid? userId, DateTimeOffset enqueuedAt,
        string customerCode, string customerName, decimal amount, params JobAckRecord[] acks)
    {
        await using var db = _factory.CreateDbContext();
        var job = new Job
        {
            TenantId = c.Id, ExternalId = externalId, DocumentType = type, Status = status, CreatedByUserId = userId, EnqueuedAtUtc = enqueuedAt,
            RetryCount = 1, LastError = status == JobStatus.Failed ? "failed" : null,
            PayloadJson = JsonSerializer.Serialize(new { mobileDocumentId = externalId, customerCode, counterparty = customerName, amount }),
        };
        db.Jobs.Add(job);
        foreach (var ack in acks)
        {
            ack.JobId = job.Id;
            db.JobAcks.Add(ack);
        }
        await db.SaveChangesAsync();
        return job.Id;
    }

    private sealed record Company(Guid Id, string Code, string Patron, string Sef, string Ali);

    private async Task<Company> CompanyAsync(bool native)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ERPDOC-{suffix}", $"Erp documents tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        if (native)
            (await AdminPutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await AdminPutAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("sef", "MANAGER"), ("ali", "SALES") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        return new Company(tenant.Id, code,
            await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "sef", $"DEV-S-{suffix}"), await LoginAsync(code, "ali", $"DEV-A-{suffix}"));
    }

    private async Task<Guid> UserIdAsync(Company c, string username)
    {
        var response = await SendAsync(HttpMethod.Get, c.Patron, "/api/v1/android/account/users");
        var users = JsonSerializer.Deserialize<MobileUserListResponse>(await response.Content.ReadAsStringAsync(), Web)!;
        return users.Users.Single(u => u.Username == username).Id;
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "portal-test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private async Task<HttpResponseMessage> AdminPutAsync(string path, object value, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string token, string path)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }
}
