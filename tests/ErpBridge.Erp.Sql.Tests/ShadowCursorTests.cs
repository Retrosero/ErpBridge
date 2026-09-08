using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Erp.Sql;
using FluentAssertions;

namespace ErpBridge.Erp.Sql.Tests;

/// <summary>
/// The cursor is the safety-critical part of the change-log protocol: it decides
/// what gets re-sent after a crash and what gets skipped. These tests pin the
/// three properties the sync service relies on — round-trip fidelity,
/// monotonicity, and tolerance of a corrupted token.
/// </summary>
public class ShadowCursorTests
{
    [Fact]
    public void Empty_cursor_reports_zero_for_every_table()
    {
        var cursor = new ShadowCursor();

        cursor.Upsert("STOKLAR").Should().Be(0);
        cursor.Delete("STOKLAR").Should().Be(0);
        cursor.ToCursor().Should().Be(ErpSyncCursor.Start);
    }

    [Fact]
    public void Advancing_then_encoding_and_decoding_preserves_positions()
    {
        var cursor = new ShadowCursor();
        cursor.AdvanceUpsert("STOKLAR", 1204);
        cursor.AdvanceUpsert("CARI_HESAPLAR", 88);
        cursor.AdvanceDelete("STOKLAR", 17);

        var round = ShadowCursor.Parse(cursor.ToCursor());

        round.Upsert("STOKLAR").Should().Be(1204);
        round.Upsert("CARI_HESAPLAR").Should().Be(88);
        round.Delete("STOKLAR").Should().Be(17);
        round.Delete("CARI_HESAPLAR").Should().Be(0);
    }

    [Fact]
    public void Table_keys_round_trip_case_insensitively()
    {
        var cursor = new ShadowCursor();
        cursor.AdvanceUpsert("STOKLAR", 42);

        ShadowCursor.Parse(cursor.ToCursor()).Upsert("stoklar").Should().Be(42);
    }

    [Fact]
    public void Advance_never_rewinds_the_position()
    {
        var cursor = new ShadowCursor();
        cursor.AdvanceUpsert("STOKLAR", 500);

        // A buggy retry reporting an older batch must not rewind the cursor —
        // that would re-send rows the central API already accepted.
        cursor.AdvanceUpsert("STOKLAR", 100);

        cursor.Upsert("STOKLAR").Should().Be(500);
    }

    [Fact]
    public void Delete_and_upsert_positions_are_tracked_independently()
    {
        var cursor = new ShadowCursor();
        cursor.AdvanceUpsert("STOKLAR", 900);
        cursor.AdvanceDelete("STOKLAR", 3);

        cursor.Upsert("STOKLAR").Should().Be(900);
        cursor.Delete("STOKLAR").Should().Be(3);
    }

    [Theory]
    [InlineData("not json at all")]
    [InlineData("{ \"u\": ")]
    [InlineData("[]")]
    public void Corrupted_token_decodes_to_start_instead_of_throwing(string raw)
    {
        // A corrupted cursor should cost a re-sync, never a crash loop.
        var cursor = ShadowCursor.Parse(new ErpSyncCursor(raw));

        cursor.Upsert("STOKLAR").Should().Be(0);
    }

    [Fact]
    public void Null_cursor_decodes_to_start()
    {
        ShadowCursor.Parse(null).Upsert("STOKLAR").Should().Be(0);
    }

    [Fact]
    public void ErpSyncCursor_ToString_truncates_long_tokens_for_logs()
    {
        var longToken = new ErpSyncCursor(new string('x', 200));

        longToken.ToString().Should().HaveLength(65).And.EndWith("…");
        ErpSyncCursor.Start.ToString().Should().Be("<start>");
    }
}
