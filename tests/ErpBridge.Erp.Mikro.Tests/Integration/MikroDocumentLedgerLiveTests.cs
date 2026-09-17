using Dapper;
using ErpBridge.Erp.Mikro.Writers;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// <c>dbo._ERPB_EVRAK_ESLESME</c> against a real Mikro test copy (Y0d). Writes only into a
/// test copy (<see cref="MikroWriteTestDatabase.AllowedDatabases"/>); see <see cref="MikroWriteTestDatabase"/>.
/// </summary>
public class MikroDocumentLedgerLiveTests
{
    private readonly MikroDocumentLedger _ledger = new();

    private static MikroLedgerEntry Entry(string externalId) =>
        new("sales_order", externalId, "CARI_HESAP_HAREKETLERI", 63 /* satış faturası */, "ERPBT", 1, 42);

    private static string NewExternalId() => $"ERPBT-TEST-{Guid.NewGuid():N}";

    [Fact]
    public async Task Ensure_creates_the_table_once_and_leaves_an_existing_one_alone()
    {
        if (!MikroWriteTestDatabase.CanWrite)
        {
            return;
        }

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        await _ledger.EnsureTableAsync(conn);
        var createdAt = await conn.ExecuteScalarAsync<DateTime>(
            "SELECT create_date FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[_ERPB_EVRAK_ESLESME]')");

        await _ledger.EnsureTableAsync(conn);

        (await conn.ExecuteScalarAsync<DateTime>(
            "SELECT create_date FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[_ERPB_EVRAK_ESLESME]')"))
            .Should().Be(createdAt);
        (await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[_ERPB_EVRAK_ESLESME]') AND is_unique = 1 AND name = 'UX_ERPB_EVRAK_ESLESME_Document'"))
            .Should().Be(1);
    }

    [Fact]
    public async Task A_record_exists_only_when_its_transaction_commits()
    {
        if (!MikroWriteTestDatabase.CanWrite)
        {
            return;
        }

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        await _ledger.EnsureTableAsync(conn);

        var rolledBack = NewExternalId();
        await using (var tx = conn.BeginTransaction())
        {
            (await _ledger.TryRecordAsync(conn, tx, Entry(rolledBack))).Should().BeTrue();
            (await _ledger.FindAsync(conn, tx, "sales_order", rolledBack)).Should().NotBeNull();
            tx.Rollback();
        }
        (await _ledger.FindAsync(conn, null, "sales_order", rolledBack)).Should().BeNull("a rolled-back document leaves no record");

        var committed = NewExternalId();
        try
        {
            await using (var tx = conn.BeginTransaction())
            {
                (await _ledger.TryRecordAsync(conn, tx, Entry(committed))).Should().BeTrue();
                tx.Commit();
            }

            (await _ledger.FindAsync(conn, null, "sales_order", committed)).Should().Be(Entry(committed));
            (await _ledger.FindAsync(conn, null, "collection", committed)).Should().BeNull("the key includes the document type");

            await using (var tx = conn.BeginTransaction())
            {
                (await _ledger.TryRecordAsync(conn, tx, Entry(committed) with { EvrakSira = 2 }))
                    .Should().BeFalse("the same mobile document is already written");
                tx.Rollback();
            }
        }
        finally
        {
            // Only this test's own ledger row; no Mikro document was written.
            await conn.ExecuteAsync("DELETE FROM [dbo].[_ERPB_EVRAK_ESLESME] WHERE [ExternalId] = @committed", new { committed });
        }
    }
}
