using ErpBridge.CentralApi.Sync;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Sync;

/// <summary>
/// How a cursor position is handed out.
///
/// <para>The whole feed rests on one property: sequence order is commit order.
/// A PostgreSQL identity column does not give that — the value is assigned at
/// INSERT and the transaction commits later, so a transaction that takes 100 can
/// commit after one that took 101, and a device reading at 99 would step over
/// 100 and never be offered it again.</para>
///
/// <para>Reserving from a locked counter row closes the hole, but only while a
/// transaction holds the lock. Proving the concurrency behaviour itself needs
/// two real PostgreSQL connections (see <c>tests/docker-compose.test.yml</c>);
/// what is enforceable everywhere, and checked here, is that no writer can
/// reserve a position outside a transaction in the first place.</para>
/// </summary>
public sealed class SyncCursorAllocationTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public SyncCursorAllocationTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Reserving_outside_a_transaction_is_refused()
    {
        var (tenant, _) = await _factory.SeedTenantAsync(
            $"SEQ-NOTX-{Guid.NewGuid():N}"[..20], "Cursor without a transaction");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();

        var act = async () => await MobileRecordProjector.ReserveAsync(db, tenant.Id, 1, default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*open transaction*",
                "a position released before its records are committed can be overtaken");
    }

    [Fact]
    public async Task Consecutive_reservations_never_overlap_and_never_go_backwards()
    {
        var (tenant, _) = await _factory.SeedTenantAsync(
            $"SEQ-BLOCK-{Guid.NewGuid():N}"[..20], "Cursor blocks");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();

        var blocks = new List<(long First, int Count)>();
        foreach (var size in new[] { 3, 1, 10, 2 })
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            blocks.Add((await MobileRecordProjector.ReserveAsync(db, tenant.Id, size, default), size));
            await transaction.CommitAsync();
        }

        blocks[0].First.Should().Be(1, "the first position a tenant ever hands out is 1");

        var taken = blocks.SelectMany(b => Enumerable.Range(0, b.Count).Select(i => b.First + i)).ToList();
        taken.Should().OnlyHaveUniqueItems();
        taken.Should().BeInAscendingOrder();
        taken.Should().BeEquivalentTo(Enumerable.Range(1, 16).Select(x => (long)x));

        var counter = await db.TenantSyncCounters.AsNoTracking().SingleAsync(x => x.TenantId == tenant.Id);
        counter.LastSeq.Should().Be(16);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(long.MaxValue)]
    public void A_position_survives_the_round_trip_through_a_token(long sequence)
    {
        SyncCursor.TryDecode(SyncCursor.Encode(sequence), out var decoded).Should().BeTrue();
        decoded.Should().Be(sequence);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void An_absent_token_means_a_device_that_holds_nothing(string? token)
    {
        SyncCursor.TryDecode(token, out var decoded).Should().BeTrue();
        decoded.Should().Be(SyncCursor.Start);
    }

    [Theory]
    [InlineData("not-base64!!")]
    [InlineData("bm90LWpzb24=")]                     // base64 of "not-json"
    [InlineData("eyJ2Ijo5OTksInMiOjF9")]             // {"v":999,"s":1} — a format this build does not know
    [InlineData("eyJ2IjoxLCJzIjotNX0=")]             // {"v":1,"s":-5} — no such position
    public void A_token_that_cannot_be_trusted_is_refused_rather_than_guessed_at(string token)
    {
        SyncCursor.TryDecode(token, out _).Should()
            .BeFalse("the caller answers with a resync; inventing a position would silently skip records");
    }
}
