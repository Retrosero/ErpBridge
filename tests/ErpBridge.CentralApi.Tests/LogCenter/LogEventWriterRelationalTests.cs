using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.LogCenter;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.LogCenter;

/// <summary>
/// Log Merkezi L0: the writer and the two legacy telemetry endpoints against a relational store, so the unique
/// index on (Source, EventId) and the single-statement group counter run for real.
/// </summary>
public sealed class LogEventWriterRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private readonly SqliteCentralApiFactory _factory;

    public LogEventWriterRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Writer_scrubs_bounds_and_normalizes_every_field()
    {
        var eventId = Guid.NewGuid().ToString();
        var result = await WriteAsync(new LogEventInput
        {
            EventId = eventId,
            Source = LogSources.Android,
            Severity = "warning",
            Kind = "sync round",
            Message = "login failed for Password=hunter2; " + new string('x', 5000),
            HttpMethod = "post",
            HttpRoute = "/api/v1/android/sync/pull?cursor=abc",
            HttpStatus = 42,
            PropertiesJson = "not json",
            BreadcrumbsJson = "{\"not\":\"an array\"}",
            OccurredAtUtc = DateTimeOffset.UtcNow.AddYears(5),
        });

        result.Should().Be(new LogWriteResult(1, 0));
        var row = await SingleAsync(eventId);
        row.Severity.Should().Be("WARN");
        row.Kind.Should().Be("SYNC_ROUND");
        row.Message.Should().NotContain("hunter2").And.HaveLength(LogEventWriter.MessageMax);
        row.HttpMethod.Should().Be("POST");
        row.HttpRoute.Should().Be("/api/v1/android/sync/pull");
        row.HttpStatus.Should().BeNull();
        row.PropertiesJson.Should().Be("{}");
        row.BreadcrumbsJson.Should().Be("[]");
        row.OccurredAtMs.Should().Be(row.ReceivedAtMs, "a timestamp far in the future is a wrong device clock");
    }

    [Fact]
    public async Task Repeated_event_id_is_ignored_even_without_a_tenant()
    {
        var eventId = Guid.NewGuid().ToString();
        var input = new LogEventInput { EventId = eventId, Source = LogSources.Portal, Severity = "INFO", Message = "retry" };

        (await WriteAsync(input, input)).Should().Be(new LogWriteResult(1, 1));
        (await WriteAsync(input)).Should().Be(new LogWriteResult(0, 1));

        using var db = _factory.CreateDbContext();
        (await db.LogEvents.CountAsync(e => e.EventId == eventId)).Should().Be(1);
    }

    [Fact]
    public async Task Warn_and_above_are_grouped_and_counted_info_is_not()
    {
        var operation = "op-" + Guid.NewGuid().ToString("N");
        LogEventInput Error(string message, int repeat = 1) => new()
        {
            Source = LogSources.WindowsAgent, Severity = "ERROR", Kind = "SYNC_FAILED", Operation = operation,
            ExceptionType = "SqlException", Message = message, AppVersion = "2.0." + repeat, RepeatCount = repeat,
        };

        await WriteAsync(Error("Timeout after 30000 ms"), Error("Timeout after 15000 ms", repeat: 3));
        await WriteAsync(Error("Timeout after 99 ms", repeat: 2));
        await WriteAsync(new LogEventInput { Source = LogSources.WindowsAgent, Severity = "INFO", Operation = operation, Message = "Timeout after 1 ms" });

        using var db = _factory.CreateDbContext();
        var group = await db.LogErrorGroups.SingleAsync(g => g.Operation == operation);
        group.TotalCount.Should().Be(6);
        group.Severity.Should().Be("ERROR");
        group.Status.Should().Be(LogErrorGroup.Open);
        var rows = await db.LogEvents.Where(e => e.Operation == operation).ToListAsync();
        rows.Where(r => r.Severity == "ERROR").Should().OnlyContain(r => r.FingerprintId == group.Id).And.HaveCount(3);
        rows.Single(r => r.Severity == "INFO").FingerprintId.Should().BeNull();
    }

    [Fact]
    public async Task A_new_event_reopens_a_resolved_group()
    {
        var operation = "op-" + Guid.NewGuid().ToString("N");
        var error = new LogEventInput { Source = LogSources.CentralApi, Severity = "ERROR", Operation = operation, Message = "boom" };
        await WriteAsync(error);
        using (var db = _factory.CreateDbContext())
        {
            var group = await db.LogErrorGroups.SingleAsync(g => g.Operation == operation);
            group.Status = LogErrorGroup.Resolved;
            await db.SaveChangesAsync();
        }

        await WriteAsync(new LogEventInput { Source = LogSources.CentralApi, Severity = "FATAL", Operation = operation, Message = "boom" });

        using var check = _factory.CreateDbContext();
        var reopened = await check.LogErrorGroups.SingleAsync(g => g.Operation == operation);
        reopened.Status.Should().Be(LogErrorGroup.Open);
        reopened.ReopenedAtMs.Should().NotBeNull();
        reopened.TotalCount.Should().Be(2);
        reopened.Severity.Should().Be("FATAL");
    }

    [Fact]
    public async Task A_lower_severity_never_lowers_the_group()
    {
        var operation = "op-" + Guid.NewGuid().ToString("N");
        LogEventInput Event(string severity) => new() { Source = LogSources.Android, Severity = severity, Operation = operation, Message = "boom" };

        await WriteAsync(Event("WARN"));
        await WriteAsync(Event("FATAL"));
        await WriteAsync(Event("ERROR"));
        await WriteAsync(Event("WARN"));

        using var db = _factory.CreateDbContext();
        var group = await db.LogErrorGroups.SingleAsync(g => g.Operation == operation);
        group.Severity.Should().Be("FATAL");
        group.TotalCount.Should().Be(4);
    }

    [Fact]
    public async Task A_batch_mixing_a_known_event_and_new_groups_counts_only_what_was_stored()
    {
        var operation = "op-" + Guid.NewGuid().ToString("N");
        var repeated = new LogEventInput { EventId = Guid.NewGuid().ToString(), Source = LogSources.Android, Severity = "ERROR", Operation = operation, Message = "first" };
        await WriteAsync(repeated);

        var result = await WriteAsync(repeated,
            new LogEventInput { Source = LogSources.Android, Severity = "ERROR", Operation = operation, Message = "second", ExceptionType = "A" },
            new LogEventInput { Source = LogSources.Android, Severity = "ERROR", Operation = operation, Message = "third", ExceptionType = "B" });

        result.Should().Be(new LogWriteResult(2, 1));
        using var db = _factory.CreateDbContext();
        var groups = await db.LogErrorGroups.Where(g => g.Operation == operation).ToListAsync();
        groups.Should().HaveCount(3).And.OnlyContain(g => g.TotalCount == 1);
        (await db.LogEvents.CountAsync(e => e.Operation == operation && e.FingerprintId == null)).Should().Be(0);
    }

    [Fact]
    public async Task Backfill_groups_copied_rows_without_reopening_resolved_groups()
    {
        var operation = "op-" + Guid.NewGuid().ToString("N");
        await WriteAsync(new LogEventInput { Source = LogSources.Android, Severity = "ERROR", Kind = "CRASH", Operation = operation, Message = "resolved one" });
        using (var db = _factory.CreateDbContext())
        {
            var group = await db.LogErrorGroups.SingleAsync(g => g.Operation == operation);
            group.Status = LogErrorGroup.Resolved;
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            LogEvent Copied(string severity, string message, long ageMs) => new()
            {
                EventId = Guid.NewGuid().ToString(), Source = LogSources.Android, Severity = severity, Kind = "CRASH",
                Operation = operation, Message = message, OccurredAtMs = nowMs - ageMs, ReceivedAtMs = nowMs - ageMs,
                OccurredAtUtc = DateTimeOffset.FromUnixTimeMilliseconds(nowMs - ageMs), ReceivedAtUtc = DateTimeOffset.FromUnixTimeMilliseconds(nowMs - ageMs),
            };
            db.LogEvents.AddRange(
                Copied("ERROR", "resolved one", 3_000),
                Copied("ERROR", "legacy crash", 2_000),
                Copied("FATAL", "legacy crash", 1_000),
                Copied("INFO", "screen view", 500));
            await db.SaveChangesAsync();
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var writer = scope.ServiceProvider.GetRequiredService<ILogEventWriter>();
            var grouped = 0;
            for (var n = await writer.BackfillGroupsAsync(1, CancellationToken.None); n > 0; n = await writer.BackfillGroupsAsync(1, CancellationToken.None))
                grouped += n;
            grouped.Should().BeGreaterThanOrEqualTo(3);
        }

        using var check = _factory.CreateDbContext();
        var groups = await check.LogErrorGroups.Where(g => g.Operation == operation).ToListAsync();
        var resolved = groups.Single(g => g.SampleMessage == "resolved one");
        resolved.Status.Should().Be(LogErrorGroup.Resolved, "history must not reopen a decision");
        resolved.TotalCount.Should().Be(2);
        var legacy = groups.Single(g => g.SampleMessage == "legacy crash");
        legacy.TotalCount.Should().Be(2);
        legacy.Severity.Should().Be("FATAL");
        (await check.LogEvents.Where(e => e.Operation == operation && e.FingerprintId == null).Select(e => e.Severity).ToListAsync())
            .Should().Equal("INFO");
    }

    [Fact]
    public async Task Unknown_source_is_a_programming_error()
    {
        var act = () => WriteAsync(new LogEventInput { Source = "somewhere" });
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Phone_telemetry_is_copied_with_user_and_device_from_the_token()
    {
        var (tenantId, token, deviceId) = await PhoneSessionAsync();
        var eventId = Guid.NewGuid().ToString();
        var correlationId = Guid.NewGuid().ToString();
        var body = new
        {
            events = new object[]
            {
                new
                {
                    eventId, occurredAtUtc = DateTimeOffset.UtcNow, kind = "HTTP_ERROR", severity = "ERROR", appVersion = "1.5.240",
                    androidVersion = "15", deviceModel = "Pixel 8", screen = "sales", operation = "api_request",
                    exceptionType = "HttpException", message = "HTTP 500", stackTrace = "", httpMethod = "POST",
                    httpRoute = "/api/v1/ingest/jobs", httpStatus = 500, correlationId,
                    deviceId = "BODY-DEVICE-IGNORED", sessionId = "session-1", properties = new { network = "wifi" },
                    breadcrumbs = new[] { new { timestampUtc = DateTimeOffset.UtcNow, category = "nav", message = "sales" } },
                },
            },
        };

        var response = await PostAsync("/api/v1/mobile/telemetry/batch", body, token, tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var row = await SingleAsync(eventId);
        row.Source.Should().Be(LogSources.Android);
        row.TenantId.Should().Be(tenantId);
        row.UserId.Should().NotBeNull();
        row.DeviceId.Should().Be(deviceId);
        row.SessionId.Should().Be("session-1");
        row.CorrelationId.Should().Be(correlationId);
        row.OsVersion.Should().Be("15");
        row.PropertiesJson.Should().Contain("wifi");
        row.BreadcrumbsJson.Should().Contain("sales");
        row.FingerprintId.Should().NotBeNull();

        using var db = _factory.CreateDbContext();
        (await db.MobileTelemetryEvents.CountAsync(e => e.EventId == eventId)).Should().Be(0, "since the L1c cutover only log_events is written");
    }

    [Fact]
    public async Task Agent_telemetry_is_copied_with_agent_identity()
    {
        var (tenant, _) = await _factory.SeedTenantAsync("LOG-AGENT-" + Guid.NewGuid().ToString("N")[..8]);
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-LOG-" + Guid.NewGuid().ToString("N")[..6]);
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);
        var eventId = Guid.NewGuid();

        var response = await PostAsync("/api/v1/agents/telemetry", new
        {
            eventId, occurredAtUtc = DateTimeOffset.UtcNow, kind = "desktop_exception", severity = "ERROR", appVersion = "1.0.0",
            windowsVersion = "Windows 11", operation = "MikroDB row count", exceptionType = "SqlException",
            message = "Login failed; Password=Gizli123", stackTrace = "safe",
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var row = await SingleAsync(eventId.ToString());
        row.Source.Should().Be(LogSources.WindowsAgent);
        row.AgentId.Should().Be(agent.Id);
        row.DeviceModel.Should().Be(agent.MachineId);
        row.Kind.Should().Be("DESKTOP_EXCEPTION");
        row.Message.Should().NotContain("Gizli123");
    }

    // ---- helpers -----------------------------------------------------------

    private async Task<LogWriteResult> WriteAsync(params LogEventInput[] inputs)
    {
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ILogEventWriter>().WriteAsync(inputs, CancellationToken.None);
    }

    private async Task<LogEvent> SingleAsync(string eventId)
    {
        using var db = _factory.CreateDbContext();
        return await db.LogEvents.AsNoTracking().SingleAsync(e => e.EventId == eventId);
    }

    private async Task<(Guid TenantId, string Token, string DeviceId)> PhoneSessionAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"LOG-{suffix}", $"Log tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{suffix}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        (await SendAsync(HttpMethod.Put, $"{basePath}/subscription", new { seats = 2, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostJsonAsync($"{basePath}/users", new { username = "plasiyer", fullName = "Plasiyer", password = "parola123", role = "ADMIN" }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var overview = await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        var deviceId = $"DEV-LOG-{suffix}";
        var login = await client.PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = overview.TenantCode, username = "plasiyer", password = "parola123", deviceId, appVersion = "1.5.240" });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        return (tenant.Id, (await login.ReadAsJsonAsync<MobileLoginResponse>()).Token, deviceId);
    }

    private Task<HttpResponseMessage> PostAsync(string path, object body, string token, Guid? tenantId = null) =>
        SendAsync(HttpMethod.Post, path, body, token, tenantId);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object body, string token, Guid? tenantId = null)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (tenantId is { } id) request.Headers.Add("X-Tenant-Id", id.ToString());
        return await _factory.CreateClient().SendAsync(request);
    }
}
