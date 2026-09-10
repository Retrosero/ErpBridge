using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.RemoteApi.DependencyInjection;
using ErpBridge.RemoteApi.Http;
using ErpBridge.RemoteApi.Options;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Http;
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

    [Theory]
    // Bootstrap writes belong to BootstrapSyncService's own retry pipeline.
    // Leaving the chunked-upload routes on the HttpClient policy stacked
    // 5+15+60+300 s of transport retries under each of the service's three
    // attempts and under all nine fallback sections, so one failing /complete
    // kept the WPF UI busy for an hour.
    [InlineData("/api/v1/bootstrap", true)]
    [InlineData("/api/v1/bootstrap/", true)]
    [InlineData("/api/v1/bootstrap/upload/start", true)]
    [InlineData("/api/v1/bootstrap/upload/3f2504e0-4f89-11d3-9a0c-0305e82c3301/chunks", true)]
    [InlineData("/api/v1/bootstrap/upload/3f2504e0-4f89-11d3-9a0c-0305e82c3301/complete", true)]
    // The notify long-poll owns its reconnect loop; a transport retry stalls it.
    [InlineData("/api/v1/bootstrap/notify", true)]
    // The status probe owns NO retry: BootstrapSyncService swallows its failure
    // as "status unavailable" and downgrades the cycle to a full snapshot, so
    // it must keep the transport policy.
    [InlineData("/api/v1/bootstrap/status", false)]
    [InlineData("/api/v1/ingest/changeset", false)]
    [InlineData("/api/v1/agents/heartbeat", false)]
    public void SkipsTransportRetry_covers_bootstrap_writes_but_not_the_status_probe(string path, bool expected)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://central.test" + path));

        ServiceCollectionExtensions.SkipsTransportRetry(request).Should().Be(expected);
    }

    [Fact]
    public async Task ThrottleOnlyPolicy_waits_out_a_429()
    {
        // A full snapshot rebuild is ~210 chunk POSTs. Treating the rate
        // limiter's 429 as fatal aborted the whole upload; it is a pacing
        // signal, so the transport waits and retries.
        var calls = 0;
        var policy = ServiceCollectionExtensions.BuildThrottleOnlyPolicy(
            [TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1)]);

        var response = await policy.ExecuteAsync(() =>
        {
            calls++;
            return Task.FromResult(new HttpResponseMessage(
                calls < 3 ? HttpStatusCode.TooManyRequests : HttpStatusCode.NoContent));
        });

        calls.Should().Be(3, "two 429s should be waited out, then the third call succeeds");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ThrottleOnlyPolicy_does_not_retry_5xx()
    {
        // Stacking transport 5xx retries under BootstrapSyncService's own
        // pipeline is what once turned an unhealthy server into an hour of
        // silence in the UI. That must stay fixed.
        var calls = 0;
        var policy = ServiceCollectionExtensions.BuildThrottleOnlyPolicy(
            [TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1)]);

        var response = await policy.ExecuteAsync(() =>
        {
            calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        });

        calls.Should().Be(1, "5xx belongs to the caller's retry pipeline, not the transport");
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ThrottleOnlyPolicy_gives_up_after_the_schedule_is_exhausted()
    {
        var calls = 0;
        var policy = ServiceCollectionExtensions.BuildThrottleOnlyPolicy(
            [TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1)]);

        var response = await policy.ExecuteAsync(() =>
        {
            calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        });

        calls.Should().Be(3, "1 initial call + 2 scheduled retries");
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
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
    public async Task SendAgentTelemetryAsync_posts_to_agents_telemetry()
    {
        var (client, handler) = BuildClient(req => RespondJson(req, HttpStatusCode.NoContent, new { }));

        await client.SendAgentTelemetryAsync(new AgentTelemetryEvent
        {
            MachineName = "AGENT-01",
            Operation = "Mikro row count",
            ExceptionType = "SqlException",
            Message = "safe message",
        });

        AssertRequest(handler, HttpMethod.Post, "/api/v1/agents/telemetry", idempotencyKeyRequired: true);
    }

    [Fact]
    public async Task PushBootstrapDataAsync_posts_chunked_upload()
    {
        var (client, handler) = BuildClient(RespondBootstrapUpload);

        await client.PushBootstrapDataAsync(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB"));

        AssertRequest(handler, HttpMethod.Post, "/api/v1/bootstrap/upload/start", idempotencyKeyRequired: true);
    }

    [Fact]
    public async Task PushBootstrapDataAsync_401_returns_permanent_error_instead_of_transient_upstream()
    {
        var (client, _) = BuildClient(req => RespondJson(req, HttpStatusCode.Unauthorized, new
        {
            errorCode = "INVALID_TOKEN",
            message = "JWT expired.",
        }));

        var act = () => client.PushBootstrapDataAsync(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB"));

        var exception = await act.Should().ThrowAsync<BootstrapPermanentPushException>();
        exception.Which.ErrorCode.Should().Be("INVALID_TOKEN");
        exception.Which.Message.Should().Be("JWT expired.");
    }

    [Fact]
    public async Task PushBootstrapDataAsync_uses_legacy_endpoint_when_chunked_endpoint_is_unavailable()
    {
        var paths = new List<string>();
        var (client, _) = BuildClient(req =>
        {
            paths.Add(req.RequestUri!.AbsolutePath);
            return req.RequestUri.AbsolutePath.EndsWith("/start", StringComparison.Ordinal)
                ? RespondJson(req, HttpStatusCode.InternalServerError, new { })
                : RespondJson(req, HttpStatusCode.NoContent, new { });
        });

        await client.PushBootstrapDataAsync(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB"));

        paths.Should().Contain("/api/v1/bootstrap/upload/start");
        paths.Should().Contain("/api/v1/bootstrap");
    }

    [Fact]
    public async Task PushBootstrapDataAsync_serializes_all_planned_child_tables()
    {
        var chunkJsons = new List<string>();
        var (client, _) = BuildClient(async req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/start", StringComparison.Ordinal))
                return await RespondBootstrapUpload(req);
            if (req.RequestUri.AbsolutePath.EndsWith("/chunks", StringComparison.Ordinal))
                chunkJsons.Add(await req.Content!.ReadAsStringAsync());
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

        ChunkItems("customerAddresses").GetArrayLength().Should().Be(1);
        ChunkItems("customerContacts").GetArrayLength().Should().Be(1);
        ChunkItems("barcodes").GetArrayLength().Should().Be(1);
        ChunkItems("salesConditions").GetArrayLength().Should().Be(1);

        JsonElement ChunkItems(string section)
        {
            var json = chunkJsons.Single(body => JsonDocument.Parse(body).RootElement.GetProperty("section").GetString() == section);
            using var document = JsonDocument.Parse(json);
            return document.RootElement.GetProperty("items").Clone();
        }
    }

    [Fact]
    public async Task PushBootstrapDataAsync_partial_section_uses_chunked_merge_upload()
    {
        var paths = new List<string>();
        string? startJson = null;
        var (client, _) = BuildClient(req =>
        {
            paths.Add(req.RequestUri!.AbsolutePath);
            var body = req.Content is null ? string.Empty : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (req.RequestUri.AbsolutePath.EndsWith("/start", StringComparison.Ordinal))
            {
                startJson = body;
                return RespondJson(req, HttpStatusCode.OK, new { uploadId = Guid.NewGuid(), maxItemsPerChunk = 2 });
            }

            return RespondJson(req, HttpStatusCode.NoContent, new { });
        });

        var package = SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB") with
        {
            PartialSection = "stockTransactions",
            StockTransactions = new[]
            {
                new StockTransactionPayload("id-1", "erp-1", "Mikro", "STK-1", "STK-1", DateTime.UtcNow, 1, 1, 1, null, 1m, 0m, 1m, 10m, 10m, null, null, null, null, DateTime.UtcNow, null),
                new StockTransactionPayload("id-2", "erp-2", "Mikro", "STK-2", "STK-2", DateTime.UtcNow, 1, 1, 1, null, 2m, 0m, 2m, 20m, 40m, null, null, null, null, DateTime.UtcNow, null),
                new StockTransactionPayload("id-3", "erp-3", "Mikro", "STK-3", "STK-3", DateTime.UtcNow, 1, 1, 1, null, 3m, 0m, 3m, 30m, 90m, null, null, null, null, DateTime.UtcNow, null),
            },
        };

        await client.PushBootstrapDataAsync(package);

        using var start = JsonDocument.Parse(startJson!);
        start.RootElement.GetProperty("isIncremental").GetBoolean().Should().BeTrue();
        paths.Should().Contain(p => p.EndsWith("/bootstrap/upload/start", StringComparison.Ordinal));
        paths.Should().Contain(p => p.Contains("/bootstrap/upload/") && p.EndsWith("/chunks", StringComparison.Ordinal));
        paths.Should().Contain(p => p.EndsWith("/complete", StringComparison.Ordinal));
    }

    [Fact]
    public async Task PushBootstrapDataAsync_timeout_is_reported_as_transient_failure()
    {
        var (client, _) = BuildClient(_ => Task.FromCanceled<HttpResponseMessage>(new CancellationToken(canceled: true)));
        var package = SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB");

        var error = await Assert.ThrowsAsync<TransientPushException>(
            () => client.PushBootstrapDataAsync(package));

        error.Message.Should().Contain("timed out");
    }

    // ---- Phase X: HttpClient retry / 4xx / 5xx / network regression tests ----
    //
    // These tests lock in the contract for the Polly retry pipeline that
    // ServiceCollectionExtensions.BuildRetryPolicy attaches around the
    // HttpClient in production: 4xx (other than 429) must NOT be retried;
    // 5xx / 429 / HttpRequestException / TaskCanceledException must be
    // retried up to the configured budget, and the final outcome is
    // surfaced as either success (5xx-then-2xx) or a transient /
    // network exception (max-retries exhausted).

    [Fact]
    public async Task PushBootstrapDataAsync_401_throws_permanent_exception_without_retry()
    {
        // 401 is not in the Polly retry predicate (only 5xx / 429 / exceptions
        // are retried). The mock must be hit exactly once, and the client
        // must surface a BootstrapPermanentPushException so the caller can
        // re-register instead of looping on a doomed retry budget.
        var callCount = 0;
        var (client, _) = BuildClientWithRetryPolicy(req =>
        {
            callCount++;
            return RespondJson(req, HttpStatusCode.Unauthorized, new
            {
                errorCode = "INVALID_TOKEN",
                message = "JWT expired.",
            });
        }, retries: 3);

        var exception = await Assert.ThrowsAsync<BootstrapPermanentPushException>(
            () => client.PushBootstrapDataAsync(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB")));

        exception.ErrorCode.Should().Be("INVALID_TOKEN");
        exception.Message.Should().Be("JWT expired.");
        callCount.Should().Be(1, "401 is not in the retry predicate → no retry attempts");
    }

    [Fact]
    public async Task PushBootstrapDataAsync_403_throws_permanent_exception_without_retry()
    {
        // Symmetrical to the 401 case: 403 is a permanent failure (the agent
        // is not authorised for this tenant) and must not consume the retry
        // budget. The handler must be hit exactly once.
        var callCount = 0;
        var (client, _) = BuildClientWithRetryPolicy(req =>
        {
            callCount++;
            return RespondJson(req, HttpStatusCode.Forbidden, new
            {
                errorCode = "FORBIDDEN_TENANT",
                message = "Tenant is not allowed.",
            });
        }, retries: 3);

        var exception = await Assert.ThrowsAsync<BootstrapPermanentPushException>(
            () => client.PushBootstrapDataAsync(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB")));

        exception.ErrorCode.Should().Be("FORBIDDEN_TENANT");
        exception.Message.Should().Be("Tenant is not allowed.");
        callCount.Should().Be(1, "403 is not in the retry predicate → no retry attempts");
    }

    [Fact]
    public async Task PushBootstrapDataAsync_retries_on_500_then_succeeds()
    {
        // The /start call returns 500 on the first attempt; the Polly retry
        // fires, the second /start call returns 200 with a fresh uploadId,
        // and the subsequent chunk + complete calls all return 204. The
        // push must complete successfully and /start must be hit exactly
        // twice (one 500 + one 200).
        var startCallCount = 0;
        var (client, _) = BuildClientWithRetryPolicy(req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/start", StringComparison.Ordinal))
            {
                startCallCount++;
                return startCallCount == 1
                    ? RespondJson(req, HttpStatusCode.InternalServerError, new { errorCode = "TRANSIENT_DB" })
                    : RespondJson(req, HttpStatusCode.OK, new { uploadId = Guid.NewGuid(), maxItemsPerChunk = 500 });
            }
            return RespondJson(req, HttpStatusCode.NoContent, new { });
        }, retries: 3);

        await client.PushBootstrapDataAsync(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB"));

        startCallCount.Should().Be(2, "Polly retries once on the initial 500 → second /start call returns 200");
    }

    [Fact]
    public async Task PushBootstrapDataAsync_returns_failure_on_500_after_max_retries()
    {
        // Every call returns 500. The first call to fail is /start; with
        // retries=3, the handler is hit 1 (initial) + 3 (retries) = 4
        // times, then the final 500 propagates. The exception type is
        // BootstrapPermanentPushException because the /start call uses
        // SendAsync<T>(classifyBootstrapFailure: true), which throws the
        // permanent marker for any non-success — including 5xx. The
        // subsequent /chunks + /complete calls never get a chance to
        // run because the /start exception aborts the push immediately.
        // (A 500 on /chunks or /complete would surface as a
        // TransientPushException instead; the canonical 5xx retry path
        // for the chunked upload is exercised by BootstrapSyncService,
        // not the unit test for the HTTP client.)
        var callCount = 0;
        var (client, _) = BuildClientWithRetryPolicy(req =>
        {
            callCount++;
            return RespondJson(req, HttpStatusCode.InternalServerError, new { errorCode = "PERSISTENT_500" });
        }, retries: 3);

        var exception = await Assert.ThrowsAsync<BootstrapPermanentPushException>(
            () => client.PushBootstrapDataAsync(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB")));

        exception.ErrorCode.Should().Be("PERSISTENT_500");
        callCount.Should().Be(4, "Polly makes 1 initial + 3 retries = 4 total invocations against the handler");
    }

    [Fact]
    public async Task PushBootstrapDataAsync_returns_HttpRequestException_after_max_retries_on_network_failure()
    {
        // The mock throws HttpRequestException on every attempt (simulating
        // a persistent network failure such as DNS or TCP reset). Polly
        // retries HttpRequestException, exhausts the budget, and the final
        // exception propagates out of PushBootstrapDataAsync because
        // HttpRemoteApiClient deliberately does NOT wrap it — the
        // BootstrapSyncService orchestrator is the one that decides what
        // to do with a network failure (retry the whole section).
        var callCount = 0;
        var (client, _) = BuildClientWithRetryPolicy(req =>
        {
            callCount++;
            return Task.FromException<HttpResponseMessage>(new HttpRequestException("simulated network failure"));
        }, retries: 3);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.PushBootstrapDataAsync(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB")));

        exception.Message.Should().Be("simulated network failure");
        callCount.Should().Be(4, "Polly retries HttpRequestException 3 times → 4 total handler invocations");
    }

    [Fact]
    public async Task PushBootstrapDataAsync_returns_TransientPushException_on_timeout_TaskCanceledException()
    {
        // The mock throws TaskCanceledException on every attempt (simulating
        // the per-request timeout firing inside HttpClient). Polly retries
        // TaskCanceledException, exhausts the budget, and SendNoContentAsync
        // converts the final TaskCanceledException into a TransientPushException
        // because the caller's CancellationToken is still not cancelled.
        var callCount = 0;
        var (client, _) = BuildClientWithRetryPolicy(_ =>
        {
            callCount++;
            return Task.FromException<HttpResponseMessage>(new TaskCanceledException("simulated timeout"));
        }, retries: 3);

        var exception = await Assert.ThrowsAsync<TransientPushException>(
            () => client.PushBootstrapDataAsync(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB")));

        exception.Message.Should().Contain("timed out");
        callCount.Should().Be(4, "Polly retries TaskCanceledException 3 times → 4 total handler invocations");
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

    // ---- Phase 9: long-poll wait-for-bootstrap-update --------------------

    [Fact]
    public async Task WaitForBootstrapUpdateAsync_parses_200_with_cursor()
    {
        var cursor = DateTimeOffset.UtcNow;
        var (client, handler) = BuildClient(req => RespondJson(req, HttpStatusCode.OK, new
        {
            updated = true,
            lastPulledAtUtc = cursor,
        }));

        var signal = await client.WaitForBootstrapUpdateAsync(TimeSpan.FromSeconds(5));

        signal.Updated.Should().BeTrue();
        signal.LastPulledAtUtc.Should().BeCloseTo(cursor, TimeSpan.FromMilliseconds(50));
        // Long-poll is a GET; no Idempotency-Key (matches GET /jobs/pending).
        handler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(m =>
                m.Method == HttpMethod.Get
                && m.RequestUri!.AbsolutePath == "/api/v1/bootstrap/notify"
                && !m.Headers.Contains("Idempotency-Key")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task WaitForBootstrapUpdateAsync_parses_204_as_no_update()
    {
        var (client, _) = BuildClient(req => RespondJson(req, HttpStatusCode.NoContent, new { }));

        var signal = await client.WaitForBootstrapUpdateAsync(TimeSpan.FromSeconds(5));

        signal.Updated.Should().BeFalse();
        signal.LastPulledAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task WaitForBootstrapUpdateAsync_clamps_oversize_wait_to_60_seconds()
    {
        string? requestedPath = null;
        var (client, _) = BuildClient(async req =>
        {
            requestedPath = req.RequestUri!.AbsolutePath + req.RequestUri.Query;
            return await RespondJson(req, HttpStatusCode.NoContent, new { });
        });

        // 999 s is well above the server-side cap; the client must clamp to 60.
        await client.WaitForBootstrapUpdateAsync(TimeSpan.FromSeconds(999));

        requestedPath.Should().NotBeNull();
        requestedPath.Should().Contain("wait=60");
    }

    [Fact]
    public async Task WaitForBootstrapUpdateAsync_returns_no_update_on_5xx()
    {
        var (client, _) = BuildClient(req => RespondJson(req, HttpStatusCode.BadGateway, new
        {
            errorCode = "UPSTREAM_DOWN",
            message = "service unavailable",
        }));

        var signal = await client.WaitForBootstrapUpdateAsync(TimeSpan.FromSeconds(5));

        signal.Updated.Should().BeFalse();
    }

    // ---- helpers --------------------------------------------------------

    /// <summary>
    /// Build a client wrapped in a Polly retry policy with a 1-ms delay
    /// schedule. Mirrors the production wiring in
    /// <c>ServiceCollectionExtensions.AddErpBridgeRemoteApi</c> where the
    /// HttpClient is decorated with the canonical 5/15/60/300-second
    /// retry policy — the only difference is the back-off is shortened to
    /// 1 ms so the regression tests for 4xx / 5xx / network / timeout run
    /// in milliseconds instead of minutes.
    /// </summary>
    /// <param name="responder">Delegate the mock <see cref="HttpMessageHandler"/> invokes for each request.</param>
    /// <param name="retries">Number of Polly retries after the initial call (total invocations = retries + 1).</param>
    private static (HttpRemoteApiClient Client, Mock<HttpMessageHandler> Handler) BuildClientWithRetryPolicy(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder,
        int retries)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>((req, _) => responder(req));

        var delays = Enumerable.Range(0, retries).Select(_ => TimeSpan.FromMilliseconds(1)).ToArray();
        var policy = ServiceCollectionExtensions.BuildRetryPolicy(delays);
        var policyHandler = new PolicyHttpMessageHandler(policy)
        {
            InnerHandler = handler.Object,
        };

        var http = new HttpClient(policyHandler) { BaseAddress = new Uri(BaseUrl + "/") };
        var client = new HttpRemoteApiClient(
            http,
            StubOptions(maxAttempts: retries),
            NullLogger<HttpRemoteApiClient>.Instance);
        return (client, handler);
    }

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
        var client = new HttpRemoteApiClient(http, StubOptions(), NullLogger<HttpRemoteApiClient>.Instance);
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

    private static Task<HttpResponseMessage> RespondBootstrapUpload(HttpRequestMessage req)
        => req.RequestUri!.AbsolutePath.EndsWith("/start", StringComparison.Ordinal)
            ? RespondJson(req, HttpStatusCode.OK, new { uploadId = Guid.NewGuid(), maxItemsPerChunk = 500 })
            : RespondJson(req, HttpStatusCode.NoContent, new { });

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
