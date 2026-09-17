using System.Net;
using System.Text;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// A job's keys travel verbatim to the agent's ERP idempotency record, where SQL Server ignores
/// trailing spaces and the columns are 64/128 wide. A key that could not be recorded exactly is
/// refused at the door instead of failing on every write attempt (goal ERP yazım, PR #77 Codex).
/// </summary>
public sealed class IngestKeyValidationTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public IngestKeyValidationTests(CentralApiFactory factory) => _factory = factory;

    [Theory]
    [InlineData("order-1 ", "sales_order", "INVALID_EXTERNAL_ID")]
    [InlineData(" order-1", "sales_order", "INVALID_EXTERNAL_ID")]
    [InlineData("order-1", "sales_order ", "INVALID_DOCUMENT_TYPE")]
    public async Task Keys_with_surrounding_whitespace_are_refused(string externalId, string documentType, string code)
    {
        var (status, error) = await IngestAsync(externalId, documentType);

        status.Should().Be(HttpStatusCode.BadRequest);
        error.Should().Be(code);
    }

    [Fact]
    public async Task Keys_wider_than_the_erp_record_are_refused()
    {
        (await IngestAsync(new string('x', 129), "sales_order")).Error.Should().Be("INVALID_EXTERNAL_ID");
        (await IngestAsync("order-2", new string('x', 65))).Error.Should().Be("INVALID_DOCUMENT_TYPE");
    }

    private async Task<(HttpStatusCode Status, string? Error)> IngestAsync(string externalId, string documentType)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"INGEST-KEY-{suffix}", "Ingest key tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);
        var client = _factory.CreateClient();
        var body = System.Text.Json.JsonSerializer.Serialize(new { externalId, documentType, payload = new { } });
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ingest/jobs") { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", raw);
        request.Headers.Add("X-Tenant-Id", tenant.Id.ToString());
        var response = await client.SendAsync(request);
        var error = response.StatusCode == HttpStatusCode.BadRequest
            ? System.Text.Json.JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))?.ErrorCode
            : null;
        return (response.StatusCode, error);
    }
}
