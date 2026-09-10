using ErpBridge.Core.Authentication;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ErpBridge.Core.Tests;

/// <summary>
/// The central API issues 60-minute agent tokens and gives agents no refresh
/// endpoint — registering again is the only way to get a new one. Every "am I
/// registered?" check in the agent used to ask whether a token <i>string</i>
/// existed, never whether it still worked, so an agent left running was
/// permanently dead after an hour: uploads, status probes and the notify
/// long-poll all returned 401 and no UI button could recover it.
/// </summary>
public class AgentTokenServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 12, 0, 0, TimeSpan.Zero);

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan by) => _now += by;
    }

    private static Mock<IAgentConfigStore> ConfigStore(string? licenseKey = "LIC-1", string? tenantId = null)
    {
        var store = new Mock<IAgentConfigStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AgentConfig { LicenseKey = licenseKey, TenantId = tenantId });
        return store;
    }

    private static Mock<IRemoteApiClient> RemoteApi(DateTimeOffset? expiresAt, string jwt = "jwt-1")
    {
        var api = new Mock<IRemoteApiClient>();
        api.Setup(a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AgentRegistrationResult
            {
                Success = true,
                Jwt = jwt,
                TenantId = Guid.NewGuid(),
                ExpiresAtUtc = expiresAt,
            });
        return api;
    }

    private static AgentTokenService Build(
        Mock<IAgentConfigStore> configStore, Mock<IRemoteApiClient> api, IAgentTokenSource source, TimeProvider clock)
        => new(configStore.Object, api.Object, source, NullLogger<AgentTokenService>.Instance, clock);

    [Fact]
    public async Task Registers_when_no_token_is_held_yet()
    {
        var source = new InMemoryAgentTokenSource();
        var api = RemoteApi(Now.AddHours(1));
        var sut = Build(ConfigStore(), api, source, new FixedClock(Now));

        var ok = await sut.EnsureValidAsync();

        ok.Should().BeTrue();
        source.CurrentJwt.Should().Be("jwt-1");
    }

    [Fact]
    public async Task Does_not_hit_the_network_while_the_token_is_still_fresh()
    {
        var source = new InMemoryAgentTokenSource();
        var api = RemoteApi(Now.AddHours(1));
        var clock = new FixedClock(Now);
        var sut = Build(ConfigStore(), api, source, clock);

        await sut.EnsureValidAsync();
        await sut.EnsureValidAsync();
        await sut.EnsureValidAsync();

        api.Verify(
            a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "a valid token must not be re-registered on every cycle");
    }

    [Fact]
    public async Task Renews_before_the_token_actually_expires()
    {
        // The whole point: renew inside the window, while the current token is
        // still being accepted, so no request is ever answered with 401.
        var source = new InMemoryAgentTokenSource();
        var api = RemoteApi(Now.AddHours(1));
        var clock = new FixedClock(Now);
        var sut = Build(ConfigStore(), api, source, clock);

        await sut.EnsureValidAsync();

        // 57 minutes in: 3 minutes of life left, inside the 5-minute window.
        clock.Advance(TimeSpan.FromMinutes(57));
        await sut.EnsureValidAsync();

        api.Verify(
            a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Renews_an_already_expired_token()
    {
        // The regression this fixes: a non-empty but dead token used to be
        // treated as proof of registration forever.
        var source = new InMemoryAgentTokenSource();
        var api = RemoteApi(Now.AddHours(1));
        var clock = new FixedClock(Now);
        var sut = Build(ConfigStore(), api, source, clock);

        await sut.EnsureValidAsync();
        clock.Advance(TimeSpan.FromHours(2));

        var ok = await sut.EnsureValidAsync();

        ok.Should().BeTrue();
        api.Verify(
            a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Refresh_replaces_the_token_even_when_it_looks_valid()
    {
        // For the expiry no schedule can predict: the server restarted with a
        // different signing key, so a token with hours left is dead anyway.
        var source = new InMemoryAgentTokenSource();
        var api = new Mock<IRemoteApiClient>();
        var issued = 0;
        api.Setup(a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new AgentRegistrationResult
            {
                Success = true,
                Jwt = $"jwt-{++issued}",
                TenantId = Guid.NewGuid(),
                ExpiresAtUtc = Now.AddHours(1),
            });
        var clock = new FixedClock(Now);
        var sut = Build(ConfigStore(), api, source, clock);

        await sut.EnsureValidAsync();
        source.CurrentJwt.Should().Be("jwt-1");

        // Past the cooldown that keeps a rejected token from turning every
        // failing request into another registration.
        clock.Advance(AgentTokenService.RefreshCooldown + TimeSpan.FromSeconds(1));
        await sut.RefreshAsync();

        source.CurrentJwt.Should().Be("jwt-2");
    }

    [Fact]
    public async Task Repeated_refreshes_are_throttled_into_one_registration()
    {
        // Production regression: every 401 triggered a refresh, and the notify
        // long-poll reconnects the instant it fails. That reached ~100
        // registrations a minute until the central API answered with 429.
        var source = new InMemoryAgentTokenSource();
        var api = RemoteApi(Now.AddHours(1));
        var clock = new FixedClock(Now);
        var sut = Build(ConfigStore(), api, source, clock);

        await sut.EnsureValidAsync();
        for (var i = 0; i < 50; i++) await sut.RefreshAsync();

        api.Verify(
            a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "a burst of rejected requests must not become a burst of registrations");
    }

    [Fact]
    public async Task A_refresh_is_allowed_again_once_the_cooldown_passes()
    {
        var source = new InMemoryAgentTokenSource();
        var api = RemoteApi(Now.AddHours(1));
        var clock = new FixedClock(Now);
        var sut = Build(ConfigStore(), api, source, clock);

        await sut.EnsureValidAsync();
        await sut.RefreshAsync();
        clock.Advance(AgentTokenService.RefreshCooldown + TimeSpan.FromSeconds(1));
        await sut.RefreshAsync();

        api.Verify(
            a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2),
            "throttling must delay recovery, not prevent it");
    }

    [Fact]
    public async Task A_failed_refresh_keeps_the_existing_token_rather_than_going_unauthenticated()
    {
        // Clearing before registering made IJwtTokenProvider fall back to the
        // statically configured token — the very one being rejected — for
        // every request in flight during the attempt.
        var source = new InMemoryAgentTokenSource();
        var api = new Mock<IRemoteApiClient>();
        var calls = 0;
        api.Setup(a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ++calls == 1
                ? new AgentRegistrationResult { Success = true, Jwt = "jwt-1", TenantId = Guid.NewGuid(), ExpiresAtUtc = Now.AddHours(1) }
                : new AgentRegistrationResult { Success = false, ErrorCode = "NETWORK" });
        var clock = new FixedClock(Now);
        var sut = Build(ConfigStore(), api, source, clock);

        await sut.EnsureValidAsync();
        clock.Advance(AgentTokenService.RefreshCooldown + TimeSpan.FromSeconds(1));
        var ok = await sut.RefreshAsync();

        ok.Should().BeFalse();
        source.CurrentJwt.Should().Be("jwt-1", "a failed renewal must not leave the agent with no token at all");
    }

    [Fact]
    public async Task A_rejected_registration_leaves_no_token_and_reports_failure()
    {
        var source = new InMemoryAgentTokenSource();
        var api = new Mock<IRemoteApiClient>();
        api.Setup(a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AgentRegistrationResult { Success = false, ErrorCode = "LICENSE_EXPIRED" });
        var sut = Build(ConfigStore(), api, source, new FixedClock(Now));

        var ok = await sut.EnsureValidAsync();

        ok.Should().BeFalse();
        source.CurrentJwt.Should().BeNull();
    }

    [Fact]
    public async Task Without_a_licence_key_it_fails_without_calling_the_api()
    {
        var source = new InMemoryAgentTokenSource();
        var api = RemoteApi(Now.AddHours(1));
        var sut = Build(ConfigStore(licenseKey: null), api, source, new FixedClock(Now));

        var ok = await sut.EnsureValidAsync();

        ok.Should().BeFalse();
        api.Verify(
            a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task A_missing_expiry_is_treated_as_short_lived_rather_than_trusted()
    {
        // A server that reports no expiry must not be read as "never expires";
        // that is how a dead token becomes permanent.
        var source = new InMemoryAgentTokenSource();
        var api = RemoteApi(expiresAt: null);
        var clock = new FixedClock(Now);
        var sut = Build(ConfigStore(), api, source, clock);

        await sut.EnsureValidAsync();
        clock.Advance(TimeSpan.FromMinutes(20));
        await sut.EnsureValidAsync();

        api.Verify(
            a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Concurrent_callers_register_once()
    {
        var source = new InMemoryAgentTokenSource();
        var api = new Mock<IRemoteApiClient>();
        api.Setup(a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(30);
                return new AgentRegistrationResult
                {
                    Success = true, Jwt = "jwt-1", TenantId = Guid.NewGuid(), ExpiresAtUtc = Now.AddHours(1),
                };
            });
        var sut = Build(ConfigStore(), api, source, new FixedClock(Now));

        await Task.WhenAll(sut.EnsureValidAsync(), sut.EnsureValidAsync(), sut.EnsureValidAsync());

        api.Verify(
            a => a.RegisterAgentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "the sync loop and the heartbeat can ask at the same moment");
    }
}
