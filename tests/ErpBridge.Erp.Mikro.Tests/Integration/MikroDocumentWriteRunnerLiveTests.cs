using Dapper;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// <see cref="MikroWriteSession"/> and <see cref="MikroDocumentWriteRunner"/> against a Mikro test copy
/// (goal ERP yazım Y3a). The "document" is a single <c>EVRAK_ACIKLAMALARI</c> row in the test series
/// <see cref="TestSeries"/>, which no Mikro screen lists on its own; each test deletes the rows and
/// ledger entries it wrote. Runs only against <see cref="MikroWriteTestDatabase.AllowedDatabases"/>.
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
public class MikroDocumentWriteRunnerLiveTests
{
    private const string TestSeries = "ERPBT3";
    private const string DocumentType = "erpbt_session_test";

    /// <summary>The description rows of tahsilat makbuzları, in a series of their own.</summary>
    private static readonly MikroNumberScope TestScope = new("test", [MikroDocumentNumbering.CollectionReceipt.Sources[1]]);

    private static MikroConnectionSettings Settings() => new(
        Server: MikroWriteTestDatabase.Server, UserId: string.Empty, Password: string.Empty,
        DatabaseName: MikroWriteTestDatabase.Database!, IntegratedSecurity: true, CompanyNo: 0, BranchNo: 0);

    private static MikroDocumentWriteRunner Runner() =>
        new(new MikroConnectionFactory(), new MikroDocumentLedger(), cache: null, NullLogger<MikroDocumentWriteRunner>.Instance);

    private static async Task<MikroWrittenDocument> WriteDescriptionAsync(MikroWriteSession session, string text, CancellationToken ct)
    {
        var number = await session.NextNumberAsync(TestScope, TestSeries, ct);
        var recno = await session.InsertAsync(MikroTables.EvrakAciklama, new Dictionary<string, object?>
        {
            ["egk_dosyano"] = MikroCodes.FileId.CariHesapHareketleri,
            ["egk_hareket_tip"] = 1,
            ["egk_evr_tip"] = MikroCodes.ChaEvrakTip.TahsilatMakbuzu,
            ["egk_evr_seri"] = TestSeries,
            ["egk_evr_sira"] = number,
            ["egk_evracik1"] = text,
            ["egk_tesaltarihi"] = new DateTime(1900, 1, 1),
        }, ct);
        return new MikroWrittenDocument(MikroTables.EvrakAciklama.Name, MikroCodes.ChaEvrakTip.TahsilatMakbuzu, TestSeries, number, recno);
    }

    private static async Task CleanUpAsync(params string[] externalIds)
    {
        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        // Only this test's own rows: the test series and this test's ledger keys.
        await conn.ExecuteAsync(
            "DELETE FROM EVRAK_ACIKLAMALARI WHERE egk_evr_seri = @TestSeries AND egk_evracik1 IN @externalIds",
            new { TestSeries, externalIds });
        await conn.ExecuteAsync(
            "IF OBJECT_ID(N'[dbo].[_ERPB_EVRAK_ESLESME]', N'U') IS NOT NULL DELETE FROM [dbo].[_ERPB_EVRAK_ESLESME] WHERE DocumentType = @DocumentType AND ExternalId IN @externalIds",
            new { DocumentType, externalIds });
    }

    private static async Task<int> RowsAsync(string externalId)
    {
        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM EVRAK_ACIKLAMALARI WHERE egk_evr_seri = @TestSeries AND egk_evracik1 = @externalId", new { TestSeries, externalId });
    }

    private static string NewId() => $"ERPBT-{Guid.NewGuid():N}"[..30];

    [Fact]
    public async Task An_inserted_row_has_no_null_column_and_points_at_itself()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), companyNo: 0, branchNo: 0, erpUserNo: 4);
        var written = await WriteDescriptionAsync(session, NewId(), CancellationToken.None);

        var row = (IDictionary<string, object?>)await conn.QuerySingleAsync(
            "SELECT * FROM EVRAK_ACIKLAMALARI WHERE egk_RECno = @HeaderRecNo", written, session.Transaction);
        row.Where(c => c.Value is null).Select(c => c.Key).Should().BeEmpty("Mikro's own rows have no NULLs");
        row["egk_RECid_RECno"].Should().Be(written.HeaderRecNo);
        row["egk_RECid_DBCno"].Should().Be((short)0);
        row["egk_create_user"].Should().Be((short)4);
        row["egk_fileid"].Should().Be((short)66);
        row["egk_create_date"].Should().Be(session.ServerNow);
        row["egk_kargokodu"].Should().Be(string.Empty);
        // Not committed: disposing the session rolls the row back.
    }

    [Fact]
    public async Task Running_the_same_document_twice_writes_one_document()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        var id = NewId();
        try
        {
            var runner = Runner();
            var first = await runner.RunAsync(Settings(), new MikroWriteRequest(DocumentType, id, 1), (s, ct) => WriteDescriptionAsync(s, id, ct));
            var second = await runner.RunAsync(Settings(), new MikroWriteRequest(DocumentType, id, 1), (s, ct) => WriteDescriptionAsync(s, id, ct));

            first.Ok.Should().BeTrue(first.ErrorMessage);
            second.Should().Be(first, "the second run answers with the document already written");
            (await RowsAsync(id)).Should().Be(1);
        }
        finally
        {
            await CleanUpAsync(id);
        }
    }

    [Fact]
    public async Task A_crash_right_after_the_commit_still_leaves_one_document()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        var id = NewId();
        try
        {
            var crashing = Runner();
            crashing.AfterCommit = _ => throw new IOException("agent killed after commit");
            await crashing.Invoking(r => r.RunAsync(Settings(), new MikroWriteRequest(DocumentType, id, 1), (s, ct) => WriteDescriptionAsync(s, id, ct)))
                .Should().ThrowAsync<IOException>();

            var retry = await Runner().RunAsync(Settings(), new MikroWriteRequest(DocumentType, id, 1), (s, ct) => WriteDescriptionAsync(s, id, ct));

            retry.Ok.Should().BeTrue(retry.ErrorMessage);
            (await RowsAsync(id)).Should().Be(1, "the ledger committed with the document, not with the ack");
        }
        finally
        {
            await CleanUpAsync(id);
        }
    }

    [Fact]
    public async Task A_failed_write_leaves_neither_the_row_nor_the_ledger_entry()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        var id = NewId();
        try
        {
            var result = await Runner().RunAsync(Settings(), new MikroWriteRequest(DocumentType, id, 1), async (s, ct) =>
            {
                await WriteDescriptionAsync(s, id, ct);
                throw new MikroWriteException(ErpBridge.Shared.ErpWriteError.StockNotFound("YOK-1"));
            });

            result.Should().Match<ErpBridge.Erp.Abstractions.SalesOrder.ErpWriteResult>(r => !r.Ok && r.ErrorCode == ErpBridge.Shared.ErpWriteError.StockNotFoundCode);
            (await RowsAsync(id)).Should().Be(0);
            await using var conn = await MikroWriteTestDatabase.OpenAsync();
            (await new MikroDocumentLedger().FindAsync(conn, null, DocumentType, id)).Should().BeNull();
        }
        finally
        {
            await CleanUpAsync(id);
        }
    }

    [Fact]
    public async Task Concurrent_documents_in_one_series_get_different_numbers()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        var ids = Enumerable.Range(0, 6).Select(_ => NewId()).ToArray();
        try
        {
            var results = await Task.WhenAll(ids.Select(id => Task.Run(() =>
                Runner().RunAsync(Settings(), new MikroWriteRequest(DocumentType, id, 1), (s, ct) => WriteDescriptionAsync(s, id, ct)))));

            results.Should().OnlyContain(r => r.Ok);
            results.Select(r => r.DocumentNumber).Should().OnlyHaveUniqueItems();
            results.Select(r => r.DocumentNumber!.Value).Max().Should().Be(results.Select(r => r.DocumentNumber!.Value).Min() + ids.Length - 1, "numbers are consecutive");
        }
        finally
        {
            await CleanUpAsync(ids);
        }
    }
}
