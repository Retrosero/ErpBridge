using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Erp.Mikro.ChangeLog;
using FluentAssertions;

namespace ErpBridge.Erp.Mikro.Tests.ChangeLog;

public class MikroLegacySynchronizationChangeLogTests
{
    private static ErpTrackedTable Stoklar =>
        MikroV15TrackedTableCatalog.Instance.Find("STOKLAR")!;

    [Fact]
    public void Installation_check_only_checks_the_existing_legacy_table()
    {
        MikroLegacySynchronizationSql.TableExists
            .Should().Contain("[dbo].[_ERPB_SENKRONIZASYON]")
            .And.NotContain("_ERPB_SYNC]")
            .And.NotContain("CREATE TABLE");
    }

    [Fact]
    public void Change_query_reads_operation_and_recno_only_from_the_legacy_feed()
    {
        var sql = MikroLegacySynchronizationSql.ReadChanges(Stoklar);

        sql.Should().Contain("FROM [dbo].[_ERPB_SENKRONIZASYON]")
            .And.Contain("s.[Islem]")
            .And.Contain("s.[KayitRECno]")
            .And.Contain("s.[TabloID] = @tableId")
            .And.Contain("s.[TriggerRECno] > @last")
            .And.Contain("LEFT JOIN [dbo].[STOKLAR]")
            .And.NotContain("_ERPB_SYNC]")
            .And.NotContain("_ERPB_SYNC_DEL");
    }

    [Fact]
    public void Legacy_cursor_round_trips_a_per_table_watermark()
    {
        var cursor = LegacySynchronizationCursor.Parse(ErpSyncCursor.Start);
        cursor.Advance("STOKLAR", 1058);

        var restored = LegacySynchronizationCursor.Parse(cursor.ToCursor());

        restored.Get("STOKLAR").Should().Be(1058);
    }

    [Fact]
    public void Previous_two_table_cursor_starts_the_legacy_feed_from_zero()
    {
        var previousSourceCursor = new ErpSyncCursor("{\"u\":{\"STOKLAR\":99},\"d\":{\"STOKLAR\":12}}");

        var cursor = LegacySynchronizationCursor.Parse(previousSourceCursor);

        cursor.Get("STOKLAR").Should().Be(0);
    }
}
