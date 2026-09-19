using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Writers.Documents;

/// <summary>
/// Writes a phone stock count as one Mikro V15 sayım fişi (ERP yazım 3, reference §14): a row per counted
/// line in <c>SAYIM_SONUCLARI</c>.
///
/// <para>This is the one document here that is <b>not</b> a movement. Writing these rows does not change any
/// stock figure — applying the count is Mikro's own "sayım sonuçlarını uygula" step, which the user runs when
/// they are satisfied with the count (K9). That is also why the table has no column recording whether a count
/// was applied: until somebody applies it, a sayım fişi is only a record of what was counted.</para>
///
/// <para>The count has no series. Mikro numbers <c>sym_evrakno</c> straight as an integer within a warehouse,
/// so the number scope is built per warehouse and the next number is that warehouse's highest plus one (K11).</para>
/// </summary>
public sealed class MikroStockCountWriter(MikroDocumentWriteRunner runner)
{
    /// <summary>Job document type the ledger records a phone stock count under.</summary>
    public const string DocumentType = "stock_count";

    /// <summary>Mikro's main unit; the phone counts in it.</summary>
    internal const byte MainUnit = 1;

    private readonly MikroDocumentWriteRunner _runner = runner ?? throw new ArgumentNullException(nameof(runner));

    public Task<ErpWriteResult> WriteAsync(StockCountCommand command, MikroConnectionSettings settings, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(settings);
        var header = command.Header;
        return _runner.RunAsync(
            settings,
            new MikroWriteRequest(DocumentType, header.ExternalId, header.ErpUserNo, settings.DatabaseName),
            (session, token) => WriteAsync(session, command, token),
            ct);
    }

    /// <summary>Writes the count inside an open session; the caller commits.</summary>
    public static async Task<MikroWrittenDocument> WriteAsync(MikroWriteSession session, StockCountCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Lines.Count == 0) throw new MikroWriteException(ErpWriteError.InvalidDocument());

        var lookup = new MikroDocumentLookup(session);
        await lookup.EnsureWarehouseAsync(command.WarehouseNo, ct).ConfigureAwait(false);
        // Every line is checked before a single row exists. A count with one unknown code is not a count
        // that is nearly right — it is a count somebody will apply to stock believing it complete (K10).
        foreach (var line in command.Lines)
        {
            if (line.CountedQuantity < 0) throw new MikroWriteException(ErpWriteError.NegativeCountedQuantity(line.StockCode));
            await lookup.StockAsync(line.StockCode, forSale: false, ct).ConfigureAwait(false);
        }

        var number = await session.NextNumberAsync(MikroDocumentNumbering.StockCount(command.WarehouseNo), series: string.Empty, ct)
            .ConfigureAwait(false);

        var firstRecNo = 0;
        for (var i = 0; i < command.Lines.Count; i++)
        {
            var recno = await session.InsertAsync(MikroTables.SayimSonuclari, LineRow(command, command.Lines[i], number, i), ct).ConfigureAwait(false);
            if (i == 0) firstRecNo = recno;
        }

        // A count has no cari document type; the ledger records the table it landed in and its number.
        return new MikroWrittenDocument(MikroTables.SayimSonuclari.Name, EvrakTip: 0, Series: string.Empty, Number: number, HeaderRecNo: firstRecNo);
    }

    internal static Dictionary<string, object?> LineRow(StockCountCommand command, StockCountLine line, int number, int index) => new()
    {
        ["sym_tarihi"] = command.Header.OccurredAt.Date,
        ["sym_depono"] = command.WarehouseNo,
        ["sym_evrakno"] = number,
        ["sym_satirno"] = index,
        ["sym_Stokkodu"] = line.StockCode,
        // Mikro fills the barcode column even when the operator typed the code; the live rows carry the
        // stock code there when no barcode was scanned, so an empty barcode is never left behind.
        ["sym_barkod"] = string.IsNullOrWhiteSpace(line.Barcode) ? line.StockCode : line.Barcode,
        ["sym_miktar1"] = line.CountedQuantity,
        ["sym_birim_pntr"] = MainUnit,
    };
}
