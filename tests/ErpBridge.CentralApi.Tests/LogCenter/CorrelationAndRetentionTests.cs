using System.Net;
using System.Net.Http.Headers;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.LogCenter;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ErpBridge.CentralApi.Tests.LogCenter;

public sealed class CorrelationIdTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public CorrelationIdTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_safe_caller_id_is_echoed()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add(CorrelationId.HeaderName, "phone-7f3a:sale.42");

        var response = await _factory.CreateClient().SendAsync(request);

        response.Headers.GetValues(CorrelationId.HeaderName).Should().Equal("phone-7f3a:sale.42");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("has spaces and <script>")]
    public async Task A_missing_or_unsafe_id_is_replaced_with_a_new_one(string? header)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        if (header is not null) request.Headers.TryAddWithoutValidation(CorrelationId.HeaderName, header);

        var response = await _factory.CreateClient().SendAsync(request);

        var id = response.Headers.GetValues(CorrelationId.HeaderName).Single();
        Guid.TryParse(id, out _).Should().BeTrue();
    }

    [Theory]
    [InlineData("abc-123", "abc-123")]
    [InlineData("  abc  ", "abc")]
    [InlineData("", null)]
    [InlineData("a/b", null)]
    public void Sanitize_accepts_only_short_safe_ids(string raw, string? expected)
    {
        CorrelationId.Sanitize(raw).Should().Be(expected);
        CorrelationId.Sanitize(new string('a', CorrelationId.MaxLength + 1)).Should().BeNull();
    }
}

public sealed class UnhandledExceptionTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public UnhandledExceptionTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Unhandled_exception_returns_a_bare_500_and_is_recorded_with_context()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"LOG-500-{suffix}");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, $"AK-LOG500-{suffix}", scopes: new[] { "mobile:read" });
        var correlation = "test-" + suffix;
        using var faulty = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IBootstrapNotificationHub>();
            services.AddSingleton<IBootstrapNotificationHub, ThrowingHub>();
        }));
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/android/notify?wait=1");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        request.Headers.Add("X-Tenant-Id", tenant.Id.ToString());
        request.Headers.Add(CorrelationId.HeaderName, correlation);

        var response = await faulty.CreateClient().SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain(ThrowingHub.Secret).And.NotContain("ThrowingHub");
        (await response.ReadAsJsonAsync<ApiError>()).Should().BeEquivalentTo(new ApiError { ErrorCode = "INTERNAL_ERROR", Message = "An unexpected error occurred.", TraceId = correlation });
        response.Headers.GetValues(CorrelationId.HeaderName).Should().Equal(correlation);

        using var db = _factory.CreateDbContext();
        var row = await db.LogEvents.AsNoTracking().SingleAsync(e => e.CorrelationId == correlation);
        row.Source.Should().Be(LogSources.CentralApi);
        row.Kind.Should().Be(UnhandledExceptionHandler.Kind);
        row.Severity.Should().Be("ERROR");
        row.TenantId.Should().Be(tenant.Id);
        row.HttpRoute.Should().Be("/api/v1/android/notify");
        row.HttpStatus.Should().Be(500);
        row.ExceptionType.Should().Be(typeof(InvalidOperationException).FullName);
        row.Message.Should().NotContain(ThrowingHub.Secret, "the scrubber masks the password");
        row.FingerprintId.Should().NotBeNull();
    }

    private sealed class ThrowingHub : IBootstrapNotificationHub
    {
        public const string Secret = "Sup3rS3cret";

        public Task<DateTimeOffset> WaitAsync(Guid tenantId, TimeSpan timeout, CancellationToken ct) =>
            throw new InvalidOperationException($"hub exploded, Password={Secret}");

        public void Publish(Guid tenantId, DateTimeOffset newPulledAtUtc) { }
    }
}

public sealed class LogRetentionTests : IClassFixture<SqliteCentralApiFactory>
{
    private const long DayMs = 86_400_000L;
    private readonly SqliteCentralApiFactory _factory;

    public LogRetentionTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Info_goes_after_14_days_warnings_after_90_and_only_open_stale_groups_are_removed()
    {
        var now = DateTimeOffset.UtcNow;
        var nowMs = now.ToUnixTimeMilliseconds();
        var tag = "ret-" + Guid.NewGuid().ToString("N");
        using (var db = _factory.CreateDbContext())
        {
            db.LogSettings.RemoveRange(db.LogSettings);
            db.LogEvents.AddRange(
                Row(tag, "INFO", nowMs - 15 * DayMs, "old-info"),
                Row(tag, "INFO", nowMs - 13 * DayMs, "recent-info"),
                Row(tag, "ERROR", nowMs - 30 * DayMs, "month-old-error"),
                Row(tag, "ERROR", nowMs - 91 * DayMs, "old-error"));
            db.LogErrorGroups.AddRange(
                Group(tag + "-open", LogErrorGroup.Open, nowMs - 91 * DayMs),
                Group(tag + "-resolved", LogErrorGroup.Resolved, nowMs - 91 * DayMs),
                Group(tag + "-fresh", LogErrorGroup.Open, nowMs - DayMs));
            await db.SaveChangesAsync();
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var result = await scope.ServiceProvider.GetRequiredService<LogRetention>().RunOnceAsync(now, CancellationToken.None);
            result.InfoDeleted.Should().BeGreaterThanOrEqualTo(1);
            result.WarnDeleted.Should().BeGreaterThanOrEqualTo(1);
        }

        using var check = _factory.CreateDbContext();
        (await check.LogEvents.Where(e => e.Operation == tag).Select(e => e.Message).ToListAsync())
            .Should().BeEquivalentTo("recent-info", "month-old-error");
        (await check.LogErrorGroups.Where(g => g.Operation == tag).Select(g => g.Fingerprint).ToListAsync())
            .Should().BeEquivalentTo(tag + "-resolved", tag + "-fresh");
    }

    [Fact]
    public async Task Stored_settings_are_clamped_to_the_allowed_range()
    {
        using (var db = _factory.CreateDbContext())
        {
            db.LogSettings.RemoveRange(db.LogSettings);
            db.LogSettings.Add(new LogSettings { InfoRetentionDays = 0, WarnRetentionDays = 5000 });
            await db.SaveChangesAsync();
        }

        using var scope = _factory.Services.CreateScope();
        var settings = await scope.ServiceProvider.GetRequiredService<LogRetention>().CurrentSettingsAsync(CancellationToken.None);

        settings.InfoRetentionDays.Should().Be(LogSettings.MinRetentionDays);
        settings.WarnRetentionDays.Should().Be(LogSettings.MaxRetentionDays);

        using var cleanup = _factory.CreateDbContext();
        cleanup.LogSettings.RemoveRange(cleanup.LogSettings);
        await cleanup.SaveChangesAsync();
    }

    private static LogEvent Row(string tag, string severity, long receivedMs, string message) => new()
    {
        EventId = Guid.NewGuid().ToString(),
        Source = LogSources.CentralApi,
        Severity = severity,
        Kind = "TEST",
        Operation = tag,
        Message = message,
        OccurredAtMs = receivedMs,
        OccurredAtUtc = DateTimeOffset.FromUnixTimeMilliseconds(receivedMs),
        ReceivedAtMs = receivedMs,
        ReceivedAtUtc = DateTimeOffset.FromUnixTimeMilliseconds(receivedMs),
    };

    private static LogErrorGroup Group(string fingerprint, string status, long lastSeenMs) => new()
    {
        Fingerprint = fingerprint,
        Source = LogSources.CentralApi,
        Kind = "TEST",
        Operation = fingerprint[..fingerprint.LastIndexOf('-')],
        Severity = "ERROR",
        Status = status,
        LastSeenMs = lastSeenMs,
        FirstSeenMs = lastSeenMs,
    };
}
