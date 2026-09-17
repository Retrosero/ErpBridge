using ErpBridge.Core.Stores;
using ErpBridge.Core.Sync;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ErpBridge.Core.Authentication;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Log Merkezi L3f: the heartbeat's "last sync" has to mean something. Before this, the field was filled with
/// <c>now</c> on every beat, so an agent that had not synced since Tuesday looked perfectly healthy.
/// </summary>
public sealed class AgentRunStatusTests
{
    [Fact]
    public void Only_a_round_that_worked_moves_the_clock()
    {
        var status = new AgentRunStatus();
        var monday = new DateTimeOffset(2026, 9, 14, 8, 0, 0, TimeSpan.Zero);

        status.Read().LastSyncAtUtc.Should().BeNull("an agent that never synced must not claim it did");

        status.RecordSync(success: true, monday);
        status.RecordSync(success: false, monday.AddDays(1), "ERP_UNREACHABLE", "Mikro yanıt vermiyor");

        var snapshot = status.Read();
        snapshot.LastSyncAtUtc.Should().Be(monday, "a failed round is not a sync");
        snapshot.LastSyncResult.Should().Be("failed");
        snapshot.LastErrorCode.Should().Be("ERP_UNREACHABLE");
        snapshot.LastError.Should().Be("Mikro yanıt vermiyor");

        // A round that works again clears the failure.
        status.RecordSync(success: true, monday.AddDays(2));
        var healed = status.Read();
        healed.LastSyncAtUtc.Should().Be(monday.AddDays(2));
        healed.LastSyncResult.Should().Be("ok");
        healed.LastErrorCode.Should().BeNull();
        healed.LastError.Should().BeNull();
    }

    [Fact]
    public void A_connection_string_in_an_error_never_leaves_the_machine()
    {
        var status = new AgentRunStatus();

        status.RecordError("SQL_LOGIN", "Login failed: Server=GURBUZ;User Id=sa;Password=Cok-Gizli;");

        var snapshot = status.Read();
        snapshot.LastError.Should().NotContain("Cok-Gizli").And.Contain("Server=GURBUZ");
        snapshot.LastErrorCode.Should().Be("SQL_LOGIN");

        // The heartbeat clears it once sent, so the panel shows the newest failure, not the oldest.
        status.ClearError();
        status.Read().LastError.Should().BeNull();
    }

    [Fact]
    public void The_erp_version_is_probed_again_only_when_it_has_aged()
    {
        var status = new AgentRunStatus();
        var now = DateTimeOffset.UtcNow;

        status.NeedsErpVersion(now, TimeSpan.FromHours(6)).Should().BeTrue("nothing has been probed yet");
        status.RecordErpVersion("V15", now);
        status.Read().ErpVersion.Should().Be("V15");
        status.NeedsErpVersion(now.AddHours(1), TimeSpan.FromHours(6)).Should().BeFalse();
        status.NeedsErpVersion(now.AddHours(7), TimeSpan.FromHours(6)).Should().BeTrue();
    }

    [Fact]
    public async Task The_sync_loop_writes_the_status_the_heartbeat_reads()
    {
        var bootstrap = new Mock<IBootstrapSyncService>();
        bootstrap.Setup(s => s.RunOnceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BootstrapSyncResult(false, 0, 0, 0, 0, 0, 0, 0, 12, "UPSTREAM_4XX", "rejected"));
        var changeLog = new Mock<IErpChangeLogSyncService>();
        changeLog.Setup(s => s.RunOnceAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ErpChangeLogSyncResult.Empty(3));
        var tokens = new Mock<IAgentTokenService>();
        tokens.Setup(t => t.EnsureValidAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var status = new AgentRunStatus();
        var services = new ServiceCollection();
        services.AddSingleton(bootstrap.Object);
        services.AddSingleton(changeLog.Object);
        services.AddSingleton(tokens.Object);
        services.AddSingleton(status);
        using var provider = services.BuildServiceProvider();

        var loop = new AgentSyncLoop(provider, new AgentSyncLoopOptions(), NullLogger<AgentSyncLoop>.Instance);
        await loop.RunSingleIterationAsync(CancellationToken.None);

        var snapshot = status.Read();
        snapshot.LastSyncResult.Should().Be("failed", "the snapshot round ran last and it failed");
        snapshot.LastErrorCode.Should().Be("UPSTREAM_4XX");
        snapshot.LastSyncAtUtc.Should().NotBeNull("the change-log round before it succeeded");
    }
}
