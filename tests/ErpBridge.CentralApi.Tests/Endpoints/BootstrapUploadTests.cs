using System.Net;
using System.Net.Http.Json;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

public sealed class BootstrapUploadTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public BootstrapUploadTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Chunked_upload_is_atomic_and_replaces_previous_snapshot()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: $"BOOT-CHUNK-{suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-CHUNK-{suffix}");
        var client = _factory.CreateClient();
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        using var start = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bootstrap/upload/start")
        {
            Content = JsonContent.Create(new { sourceDatabase = "MIKRO", pulledAtUtc = DateTimeOffset.UtcNow })
        };
        start.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var started = await client.SendAsync(start);
        started.StatusCode.Should().Be(HttpStatusCode.OK);
        var startBody = await started.Content.ReadFromJsonAsync<StartResponse>();
        startBody.Should().NotBeNull();

        using var chunk = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{startBody!.UploadId}/chunks")
        {
            Content = JsonContent.Create(new { section = "customers", chunkIndex = 0, items = new[] { new { customerCode = "C-1" } } })
        };
        chunk.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        (await client.SendAsync(chunk)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var duplicateChunk = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{startBody.UploadId}/chunks")
        {
            Content = JsonContent.Create(new { section = "customers", chunkIndex = 0, items = new[] { new { customerCode = "C-1" } } })
        };
        duplicateChunk.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        (await client.SendAsync(duplicateChunk)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var complete = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{startBody.UploadId}/complete")
        {
            Content = JsonContent.Create(new { })
        };
        complete.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        (await client.SendAsync(complete)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        (await db.BootstrapSnapshots.CountAsync(x => x.TenantId == tenant.Id && x.IsActive)).Should().Be(1);
        (await db.BootstrapSnapshotChunks.CountAsync(x => x.Section == "customers")).Should().Be(1);
    }

    private sealed class StartResponse
    {
        public Guid UploadId { get; set; }
        public int MaxItemsPerChunk { get; set; }
    }
}
