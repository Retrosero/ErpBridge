using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using ErpBridge.Core.Domain;
using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.RemoteApi.DependencyInjection;
using ErpBridge.RemoteApi.Http;
using ErpBridge.RemoteApi.Options;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Polly;

namespace ErpBridge.RemoteApi.Tests.Http;

/// <summary>
/// Unit tests for <see cref="HttpRemoteApiClient"/>. The underlying
/// <see cref="HttpMessageHandler"/> is mocked so no network is touched.
/// Endpoint paths, HTTP method, and the <c>Idempotency-Key</c> header are
/// asserted on the captured request.
/// </summary>
public class HttpRemoteApiClientTests
{
    private const string BaseUrl = "https://api.erpbridge.test";

    private static IOptionsMonitor<CentralApiOptions> StubOptions(int timeoutSeconds = 30, int maxAttempts = 4, int initialDelaySeconds = 5) =>
        new TestOptionsMonitor(new CentralApiOptions
        {
            BaseUrl = BaseUrl,
            TimeoutSeconds = timeoutSeconds,
            Jwt = "test-jwt",
            Retry = new CentralApiOptions.RetryOptions
            {
                MaxAttempts = maxAttempts,
                InitialDelaySeconds = initialDelaySeconds,
            },
        });

    [Fact]
    public async Task ValidateLicenseAsync_posts_to_licenses_validate_and_returns_result()
    {
        var (client, handler) = BuildClient(req => RespondJson(req, HttpStatusCode.OK, new
        {
            valid = true,
            tenantId = "tenant-X",
            expiresAtUtc = DateTimeOffset.UtcNow.AddYears(1),
        }));

        var result = await client.ValidateLicenseAsync("LIC-001");

        result.Valid.Should().BeTrue();
        result.TenantId.Should().Be("tenant-X");
        AssertRequest(handler, HttpMethod.Post, "/api/v1/licenses/validate", idempotencyKeyRequired: true);
    }

    [Fact]
    public async Task ValidateLicenseAsync_404_returns_invalid_result()
    {
        var (client, handler) = BuildClient(req => RespondJson(req, HttpStatusCode.NotFound, new { errorCode = "NOT_FOUND" }));

        var result = await client.ValidateLicenseAsync("BAD-KEY");

        result.Valid.Should().BeFalse();
        result.ErrorCode.Should().Be("LICENSE_NOT_FOUND");
        // 404 is not an exception path; we still record the request.
        handler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Post && m.RequestUri!.AbsolutePath == "/api/v1/licenses/validate"),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetPendingJobsAsync_gets_jobs_and_deserializes()
    {
        var (client, handler) = BuildClient(req => RespondJson(req, HttpStatusCode.OK, new[]
        {
            new { jobId = "j-1", externalId = "ext-1", documentType = "sales_order", payload = "{}", enqueuedAtUtc = DateTimeOffset.UtcNow },
            new { jobId = "j-2", externalId = "ext-2", documentType = "sales_order", payload = "{\"x\":1}", enqueuedAtUtc = DateTimeOffset.UtcNow },
        }));

        var jobs = await client.GetPendingJobsAsync();

        jobs.Should().HaveCount(2);
        jobs[0].JobId.Should().Be("j-1");
        jobs[1].Payload.Should().Contain("\"x\":1");
        AssertRequest(handler, HttpMethod.Get, "/api/v1/jobs/pending", idempotencyKeyRequired: false);
    }

    [Fact]
    public async Task GetPendingJobsAsync_empty_returns_empty_list()
    {
        var (client, _) = BuildClient(req => RespondJson(req, HttpStatusCode.OK, Array.Empty<object>()));

        var jobs = await client.GetPendingJobsAsync();

        jobs.Should().BeEmpty();
    }

    [Fact]
    public async Task SendAckAsync_posts_to_jobs_ack_with_idempotency_key()
    {
        var (client, handler) = BuildClient(req => RespondJson(req, HttpStatusCode.NoContent, new { }));

        await client.SendAckAsync(new JobAck
        {
            JobId = "j-ack-42",
            Status = "succeeded",
        });

        // Idempotency-Key must be derived from the job id (ack:{JobId}).
        AssertRequest(handler, HttpMethod.Post, "/api/v1/jobs/ack", idempotencyKeyRequired: true,
            expectedIdempotencyKey: "ack:j-ack-42");
    }

    [Fact]
    public async Task SendAckAsync_5xx_throws_without_retrying_when_no_polly_policy_attached()
    {
        // The HttpRemoteApiClient does not own the retry policy — it lives in
        // the DI registration as an HttpClient message handler. When the client
        // is constructed standalone (as in this test), a 5xx surfaces as an
        // exception immediately. The retry behaviour is verified separately
        // by BuildRetryPolicy_retries_4_times_after_5xx.
        var callCount = 0;
        var (client, _) = BuildClient(_ =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            });
        });

        var act = () => client.SendAckAsync(new JobAck { JobId = "j-1", Status = "failed" });

        await act.Should().ThrowAsync<HttpRequestException>();
        callCount.Should().Be(1, "no Polly policy attached → no retries");
    }

    [Fact]
    public async Task BuildRetryPolicy_retries_4_times_after_5xx()
    {
        // Verifies the canonical retry policy from DI extension: 1 initial call
        // + 4 retries = 5 total invocations against a handler that always
        // returns 500. Delays are forced to ~1ms so the test runs in milliseconds.
        var callCount = 0;
        var policy = ServiceCollectionExtensions.BuildRetryPolicy(new[]
            {
                TimeSpan.FromMilliseconds(1),
                TimeSpan.FromMilliseconds(1),
                TimeSpan.FromMilliseconds(1),
                TimeSpan.FromMilliseconds(1),
            });

        var handler = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };

        try
        {
            await policy.ExecuteAsync(
                (Func<Context, Task<HttpResponseMessage>>)(_ =>
                {
                    callCount++;
                    return Task.FromResult(handler);
                }),
                new Context());
        }
        catch (HttpRequestException)
        {
            // Polly v7 still returns the last 5xx response without throwing;
            // we only get an exception if the policy's Handle<HttpRequestException>()
            // matches. For this verification we just count invocations.
        }

        callCount.Should().Be(5, "Polly retries 4 times after the initial 5xx → 1 + 4 = 5 invocations");
    }

    [Fact]
    public async Task SendHeartbeatAsync_posts_to_agents_heartbeat()
    {
        var (client, handler) = BuildClient(req => RespondJson(req, HttpStatusCode.NoContent, new { }));

        await client.SendHeartbeatAsync(new AgentHeartbeat
        {
            AgentId = "agent-1",
            TenantId = "tenant-X",
            Status = "running",
            LastSyncAtUtc = DateTimeOffset.UtcNow,
            QueueDepth = 3,
        });

        AssertRequest(handler, HttpMethod.Post, "/api/v1/agents/heartbeat", idempotencyKeyRequired: true);
    }

    [Fact]
    public async Task PushBootstrapDataAsync_posts_to_bootstrap()
    {
        var (client, handler) = BuildClient(req => RespondJson(req, HttpStatusCode.NoContent, new { }));

        await client.PushBootstrapDataAsync(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB"));

        AssertRequest(handler, HttpMethod.Post, "/api/v1/bootstrap", idempotencyKeyRequired: true);
    }

    [Fact]
    public async Task PushBootstrapDataAsync_serializes_all_planned_child_tables()
    {
        string? requestJson = null;
        var (client, _) = BuildClient(async req =>
        {
            requestJson = await req.Content!.ReadAsStringAsync();
            return await RespondJson(req, HttpStatusCode.NoContent, new { });
        });
        var package = SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB") with
        {
            CustomerAddresses = new[] { new CustomerAddressPayload("C001", 1, "Istanbul", null, null, null, null, null, null) },
            CustomerContacts = new[] { new CustomerContactPayload("C001", "Ada", null, null, null, null, null) },
            Barcodes = new[] { new BarcodePayload("8690000000001", "S001", null, null, null, 1) },
            SalesConditions = new[]
            {
                new SalesConditionPayload("S001", "C001", 1, null, null, null, 10m, "0", new[] { 5m }),
            },
        };

        await client.PushBootstrapDataAsync(package);

        using var json = JsonDocument.Parse(requestJson!);
        var payload = json.RootElement.GetProperty("payload");
        payload.GetProperty("customerAddresses").GetArrayLength().Should().Be(1);
        payload.GetProperty("customerContacts").GetArrayLength().Should().Be(1);
        payload.GetProperty("barcodes").GetArrayLength().Should().Be(1);
        payload.GetProperty("salesConditions").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task SendAckAsync_uses_bearer_authorization_header_when_jwt_is_set()
    {
        var (client, handler) = BuildClient(req => RespondJson(req, HttpStatusCode.NoContent, new { }));

        await client.SendAckAsync(new JobAck { JobId = "j-1", Status = "succeeded" });

        handler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(m =>
                m.Headers.Authorization != null
                && m.Headers.Authorization.Scheme == "Bearer"
                && m.Headers.Authorization.Parameter == "test-jwt"),
            ItExpr.IsAny<CancellationToken>());
    }

    // ---- helpers --------------------------------------------------------

    private static (HttpRemoteApiClient Client, Mock<HttpMessageHandler> Handler) BuildClient(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>((req, _) => responder(req));

        var http = new HttpClient(handler.Object) { BaseAddress = new Uri(BaseUrl + "/") };
        var client = new HttpRemoteApiClient(http, StubOptions(), new ConfigurationBuilder().Build(), NullLogger<HttpRemoteApiClient>.Instance);
        return (client, handler);
    }

    private static Task<HttpResponseMessage> RespondJson(HttpRequestMessage req, HttpStatusCode status, object body)
    {
        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
    }

    private static void AssertRequest(
        Mock<HttpMessageHandler> handler,
        HttpMethod expectedMethod,
        string expectedPath,
        bool idempotencyKeyRequired,
        string? expectedIdempotencyKey = null)
    {
        handler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(m => m.Method == expectedMethod && m.RequestUri!.AbsolutePath == expectedPath),
            ItExpr.IsAny<CancellationToken>());

        if (idempotencyKeyRequired)
        {
            handler.Protected().Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(m => HeadersContainIdempotencyKey(m, expectedIdempotencyKey)),
                ItExpr.IsAny<CancellationToken>());
        }
        else
        {
            // GET /jobs/pending should NOT carry an idempotency key (it's a safe read).
            handler.Protected().Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(m => !m.Headers.Contains("Idempotency-Key")),
                ItExpr.IsAny<CancellationToken>());
        }
    }

    private static bool HeadersContainIdempotencyKey(HttpRequestMessage m, string? expected)
    {
        if (!m.Headers.TryGetValues("Idempotency-Key", out var values))
        {
            return false;
        }
        var actual = values.FirstOrDefault();
        if (actual is null) return false;
        if (expected is not null) return actual == expected;
        return !string.IsNullOrEmpty(actual);
    }

    /// <summary>Minimal in-memory <see cref="IOptionsMonitor{T}"/> stub for tests.</summary>
    private sealed class TestOptionsMonitor : IOptionsMonitor<CentralApiOptions>
    {
        public TestOptionsMonitor(CentralApiOptions currentValue)
        {
            CurrentValue = currentValue;
        }

        public CentralApiOptions CurrentValue { get; }
        public CentralApiOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<CentralApiOptions, string?> listener) => null;
    }
}
