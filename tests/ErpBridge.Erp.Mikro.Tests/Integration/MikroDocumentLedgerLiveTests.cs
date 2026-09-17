using Dapper;
using ErpBridge.Erp.Mikro.Writers;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// <c>dbo._ERPB_EVRAK_ESLESME</c> against a real Mikro test copy (Y0d). Writes only into a
/// test copy (<see cref="MikroWriteTestDatabase.AllowedDatabases"/>); see <see cref="MikroWriteTestDatabase"/>.
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
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
        var widths = (await conn.QueryAsync<(string Column, int Length)>(
            "SELECT COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '_ERPB_EVRAK_ESLESME' AND COLUMN_NAME IN ('DocumentType', 'ExternalId')"))
            .ToDictionary(x => x.Column, x => x.Length);
        widths.Should().Equal(new Dictionary<string, int>
        {
            ["DocumentType"] = MikroDocumentLedger.DocumentTypeMaxLength,
            ["ExternalId"] = MikroDocumentLedger.ExternalIdMaxLength,
        }, "every job the central API accepts must fit");
    }

    [Fact]
    public async Task Agents_creating_the_table_at_the_same_time_all_succeed()
    {
        if (!MikroWriteTestDatabase.CanWrite)
        {
            return;
        }

        await using (var conn = await MikroWriteTestDatabase.OpenAsync())
        {
            // Only ErpBridge's own table in a test copy, and only when no test left a row in it.
            var rows = await conn.ExecuteScalarAsync<int?>(
                "IF OBJECT_ID(N'[dbo].[_ERPB_EVRAK_ESLESME]', N'U') IS NOT NULL SELECT COUNT(*) FROM [dbo].[_ERPB_EVRAK_ESLESME]");
            if (rows is > 0)
            {
                return;
            }

            await conn.ExecuteAsync("IF OBJECT_ID(N'[dbo].[_ERPB_EVRAK_ESLESME]', N'U') IS NOT NULL DROP TABLE [dbo].[_ERPB_EVRAK_ESLESME]");
        }

        var creators = Enumerable.Range(0, 8).Select(async _ =>
        {
            await using var conn = await MikroWriteTestDatabase.OpenAsync();
            await _ledger.EnsureTableAsync(conn);
        });
        await Task.WhenAll(creators);

        await using var check = await MikroWriteTestDatabase.OpenAsync();
        (await check.ExecuteScalarAsync<int?>("SELECT OBJECT_ID(N'[dbo].[_ERPB_EVRAK_ESLESME]', N'U')")).Should().NotBeNull();
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

        // As long as the central API's jobs.ExternalId allows.
        var committed = NewExternalId().PadRight(MikroDocumentLedger.ExternalIdMaxLength, 'x');
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

            // Mikro's collation is case-insensitive; the central API's keys are not.
            await using (var tx = conn.BeginTransaction())
            {
                (await _ledger.TryRecordAsync(conn, tx, Entry(committed.ToUpperInvariant())))
                    .Should().BeTrue("a key differing only in case is another document");
                (await _ledger.FindAsync(conn, tx, "SALES_ORDER", committed)).Should().BeNull();
                tx.Rollback();
            }
            await _ledger.Invoking(l => l.FindAsync(conn, null, "sales_order", committed + " "))
                .Should().ThrowAsync<ArgumentException>("SQL Server ignores trailing spaces, so they would match another key");
        }
        finally
        {
            // Only this test's own ledger row; no Mikro document was written.
            await conn.ExecuteAsync("DELETE FROM [dbo].[_ERPB_EVRAK_ESLESME] WHERE [ExternalId] = @committed", new { committed });
        }
    }
}
