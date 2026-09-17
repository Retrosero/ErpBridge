using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.LogCenter;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.LogCenter;

/// <summary>Log Merkezi L1a: <c>/api/v1/admin/logs</c> list, detail and facets against SQLite.</summary>
public sealed class AdminLogEndpointsTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public AdminLogEndpointsTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Pages_newest_first_without_repeating_or_skipping_rows_on_equal_timestamps()
    {
        var operation = "page-" + Guid.NewGuid().ToString("N");
        var at = DateTimeOffset.UtcNow.AddMinutes(-5);
        // Seven events, three sharing one millisecond: the cursor must break the tie by event id.
        var inputs = Enumerable.Range(0, 7).Select(i => new LogEventInput
        {
            EventId = Guid.NewGuid().ToString(), Source = LogSources.Android, Severity = "INFO", Operation = operation,
            Message = "m" + i, OccurredAtUtc = i < 3 ? at : at.AddSeconds(-i),
        }).ToArray();
        await WriteAsync(inputs);
        var token = await AdminTokenAsync();

        var seen = new List<string>();
        string? before = null;
        do
        {
            var page = await GetAsync<AdminLogPageDto>($"/api/v1/admin/logs?operation={operation}&take=2" + (before is null ? "" : $"&before={before}"), token);
            page.Items.Should().HaveCountLessThanOrEqualTo(2);
            seen.AddRange(page.Items.Select(i => i.EventId));
            before = page.NextBefore;
        } while (before is not null);

        seen.Should().OnlyHaveUniqueItems().And.HaveCount(7).And.BeEquivalentTo(inputs.Select(i => i.EventId));
    }

    [Fact]
    public async Task Filters_combine_and_names_are_filled()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"LOGLIST-{suffix}", $"Log firması {suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"PC-{suffix}");
        var operation = "filter-" + suffix;
        await WriteAsync(
            new LogEventInput { Source = LogSources.WindowsAgent, TenantId = tenant.Id, AgentId = agent.Id, Severity = "ERROR", Operation = operation, Message = "Mikro bağlantısı koptu", AppVersion = "2.1.0" },
            new LogEventInput { Source = LogSources.WindowsAgent, TenantId = tenant.Id, AgentId = agent.Id, Severity = "INFO", Operation = operation, Message = "senkron bitti", AppVersion = "2.1.0" },
            new LogEventInput { Source = LogSources.Android, TenantId = tenant.Id, Severity = "FATAL", Operation = operation, Message = "çökme", AppVersion = "1.5.240", CorrelationId = "corr-" + suffix });
        var token = await AdminTokenAsync();

        var errors = await GetAsync<AdminLogPageDto>($"/api/v1/admin/logs?tenantId={tenant.Id}&operation={operation}&minSeverity=error&source=windows_agent", token);
        var row = errors.Items.Should().ContainSingle().Subject;
        row.Message.Should().Be("Mikro bağlantısı koptu");
        row.TenantName.Should().Be($"Log firması {suffix}");
        row.AgentName.Should().Be($"PC-{suffix}");

        (await GetAsync<AdminLogPageDto>($"/api/v1/admin/logs?operation={operation}&q=MIKRO", token)).Items
            .Should().ContainSingle(i => i.Message == "Mikro bağlantısı koptu", "search is case-insensitive for ASCII and matches the message");
        (await GetAsync<AdminLogPageDto>($"/api/v1/admin/logs?q=corr-{suffix}", token)).Items
            .Should().ContainSingle(i => i.Severity == "FATAL", "a correlation id typed into search finds its events");
        (await GetAsync<AdminLogPageDto>($"/api/v1/admin/logs?operation={operation}&appVersion=1.5.240&severity=FATAL,ERROR", token)).Items
            .Should().ContainSingle(i => i.Source == "android");
        (await GetAsync<AdminLogPageDto>($"/api/v1/admin/logs?operation={operation}&from={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddHours(1).ToString("O"))}", token)).Items
            .Should().BeEmpty();
    }

    [Fact]
    public async Task Detail_carries_stack_breadcrumbs_and_group()
    {
        var operation = "detail-" + Guid.NewGuid().ToString("N");
        await WriteAsync(new LogEventInput
        {
            EventId = Guid.NewGuid().ToString(), Source = LogSources.Android, Severity = "ERROR", Operation = operation, Message = "boom",
            StackTrace = "at com.example.Sales.save(Sales.kt:10)", BreadcrumbsJson = "[{\"category\":\"nav\",\"message\":\"sales\"}]",
            PropertiesJson = "{\"network\":\"wifi\"}",
        });
        var token = await AdminTokenAsync();
        var id = (await GetAsync<AdminLogPageDto>($"/api/v1/admin/logs?operation={operation}", token)).Items.Single().Id;

        var detail = await GetAsync<AdminLogEventDetailDto>($"/api/v1/admin/logs/{id}", token);

        detail.StackTrace.Should().Contain("Sales.save");
        detail.BreadcrumbsJson.Should().Contain("sales");
        detail.PropertiesJson.Should().Contain("wifi");
        detail.Group.Should().NotBeNull();
        detail.Group!.TotalCount.Should().Be(1);
        (await _factory.CreateClient().GetAsync($"/api/v1/admin/logs/{Guid.NewGuid()}", token)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Facets_count_the_filtered_window()
    {
        var operation = "facet-" + Guid.NewGuid().ToString("N");
        await WriteAsync(
            new LogEventInput { Source = LogSources.Android, Severity = "ERROR", Kind = "CRASH", Operation = operation, AppVersion = "1.0" },
            new LogEventInput { Source = LogSources.Android, Severity = "ERROR", Kind = "CRASH", Operation = operation, AppVersion = "1.1" },
            new LogEventInput { Source = LogSources.Portal, Severity = "WARN", Kind = "HTTP_ERROR", Operation = operation, AppVersion = "1.1" },
            new LogEventInput { Source = LogSources.Android, Severity = "INFO", Kind = "SYNC_ROUND", Operation = operation, OccurredAtUtc = DateTimeOffset.UtcNow.AddDays(-3) });
        var token = await AdminTokenAsync();

        var facets = await GetAsync<AdminLogFacetsDto>($"/api/v1/admin/logs/facets?operation={operation}", token);

        facets.Total.Should().Be(3, "the default window is the last 24 hours");
        facets.Sources.Should().BeEquivalentTo([new AdminLogFacetDto { Value = "android", Count = 2 }, new AdminLogFacetDto { Value = "portal", Count = 1 }]);
        facets.Severities.Select(f => f.Value).Should().Equal("ERROR", "WARN");
        facets.Kinds.First().Should().BeEquivalentTo(new AdminLogFacetDto { Value = "CRASH", Count = 2 });
        facets.AppVersions.Single(v => v.Value == "1.1").Count.Should().Be(2);
    }

    [Theory]
    [InlineData("source=somewhere", "INVALID_SOURCE")]
    [InlineData("severity=LOUD", "INVALID_SEVERITY")]
    [InlineData("tenantId=abc", "INVALID_ID")]
    [InlineData("from=yesterday", "INVALID_DATE")]
    [InlineData("from=2026-09-17T10:00:00Z&to=2026-09-16T10:00:00Z", "INVALID_RANGE")]
    [InlineData("before=garbage", "INVALID_CURSOR")]
    public async Task Invalid_filters_are_rejected(string query, string errorCode)
    {
        var response = await _factory.CreateClient().GetAsync($"/api/v1/admin/logs?{query}", await AdminTokenAsync());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be(errorCode);
    }

    [Fact]
    public async Task Requires_an_admin_token()
    {
        (await _factory.CreateClient().GetAsync("/api/v1/admin/logs")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task WriteAsync(params LogEventInput[] inputs)
    {
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ILogEventWriter>().WriteAsync(inputs, CancellationToken.None);
    }

    private async Task<string> AdminTokenAsync()
    {
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        return _factory.IssueAdminJwt(admin.Id);
    }

    private async Task<T> GetAsync<T>(string path, string token)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }
}
