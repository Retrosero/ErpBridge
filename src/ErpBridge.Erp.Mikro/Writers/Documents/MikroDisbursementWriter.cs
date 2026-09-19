using System.Globalization;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Writers.Documents;

/// <summary>
/// Writes a phone disbursement as one Mikro V15 tediye receipt (ERP yazım 2, reference §11): a single
/// <c>CARI_HESAP_HAREKETLERI</c> row that debits the account and names the cash box or bank it left.
///
/// <para>It is the collection receipt's mirror with three differences, all of them evidence from Mikro's own
/// rows: the document type is 64 (<c>TediyeMakbuzu</c>) instead of 1, the row is a debit instead of a credit,
/// and the payment kind comes from Mikro's <b>company</b> family — cash is still 0, but a transfer is
/// <c>FirmaHavaleEmri</c> (20), not the collection's <c>MusteriHavaleSozu</c> (17), because the money is
/// leaving on the company's own order. Cash and transfer need no <c>ODEME_EMIRLERI</c> row: in the live
/// company database those tediye rows carry no reference number.</para>
/// </summary>
public sealed class MikroDisbursementWriter(MikroDocumentWriteRunner runner)
{
    /// <summary>Job document type the ledger records a phone disbursement under.</summary>
    public const string DocumentType = "disbursement";

    private readonly MikroDocumentWriteRunner _runner = runner ?? throw new ArgumentNullException(nameof(runner));

    public Task<ErpWriteResult> WriteAsync(DisbursementCommand command, MikroConnectionSettings settings, CancellationToken ct = default)
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

    /// <summary>Writes the receipt inside an open session; the caller commits.</summary>
    public static async Task<MikroWrittenDocument> WriteAsync(MikroWriteSession session, DisbursementCommand command, CancellationToken ct)
    {
        var header = command.Header;
        if (command.Amount <= 0) throw new MikroWriteException(ErpWriteError.InvalidAmount());
        if (Math.Abs(command.Amount - header.ExpectedTotal) > MikroPriceCalculator.TotalTolerance)
            throw new MikroWriteException(ErpWriteError.TotalMismatch(command.Amount - header.ExpectedTotal));

        var lookup = new MikroDocumentLookup(session);
        // The account must exist in Mikro before the row is written: a tediye to a cash box nobody has is
        // the kind of mistake that is cheap to refuse now and expensive to unpick from the ledger later.
        await lookup.CustomerAsync(header.CustomerCode, MikroCustomerUse.Collection, isOrder: false, ct).ConfigureAwait(false);
        await lookup.EnsureSalespersonAsync(header.SalespersonCode, ct).ConfigureAwait(false);
        await (command.Method == DisbursementMethod.Cash
            ? lookup.EnsureCashBoxAsync(command.AccountCode, MikroCashBoxKind.Cash, ct)
            : lookup.EnsureBankAsync(command.AccountCode, ct)).ConfigureAwait(false);

        var number = await session.NextNumberAsync(MikroDocumentNumbering.DisbursementReceipt, header.Series, ct).ConfigureAwait(false);
        var recno = await session.InsertAsync(MikroTables.CariHareket, LineRow(command, number), ct).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(header.Description))
        {
            var description = MikroSalesInvoiceWriter.DescriptionRow(header.Series, number, header.Description);
            // Reference §11: the disbursement's description row is (51, 0, 64) — the collection's is (51, 1, 1).
            description["egk_hareket_tip"] = 0;
            description["egk_evr_tip"] = MikroCodes.ChaEvrakTip.TediyeMakbuzu;
            description["egk_evr_doksayisi"] = 0;
            await session.InsertAsync(MikroTables.EvrakAciklama, description, ct).ConfigureAwait(false);
        }

        return new MikroWrittenDocument(MikroTables.CariHareket.Name, MikroCodes.ChaEvrakTip.TediyeMakbuzu, header.Series, number, recno);
    }

    /// <summary>What the money left as: cash from a cash box, or the company's own transfer order.</summary>
    internal static (byte Cinsi, byte AccountKind) Kind(DisbursementMethod method) => method switch
    {
        DisbursementMethod.Cash => (MikroCodes.ChaCinsi.Nakit, MikroCodes.HesapCinsi.Kasamiz),
        _ => (MikroCodes.ChaCinsi.FirmaHavaleEmri, MikroCodes.HesapCinsi.Bankamiz),
    };

    internal static Dictionary<string, object?> LineRow(DisbursementCommand command, int number)
    {
        var header = command.Header;
        var (cinsi, accountKind) = Kind(command.Method);
        var day = header.OccurredAt.Date;
        return new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = MikroCodes.ChaEvrakTip.TediyeMakbuzu,
            ["cha_evrakno_seri"] = header.Series,
            ["cha_evrakno_sira"] = number,
            ["cha_satir_no"] = 0,
            ["cha_tarihi"] = day,
            ["cha_belge_tarih"] = day,
            // The one line that makes this a payment out rather than in.
            ["cha_tip"] = MikroCodes.ChaTip.Borc,
            ["cha_cinsi"] = cinsi,
            ["cha_normal_Iade"] = MikroCodes.NormalIade.Normal,
            ["cha_tpoz"] = MikroCodes.ChaTpoz.Acik,
            ["cha_cari_cins"] = MikroCodes.HesapCinsi.Carimiz,
            ["cha_kod"] = header.CustomerCode,
            ["cha_satici_kodu"] = header.SalespersonCode,
            ["cha_srmrkkodu"] = header.ResponsibilityCenterCode,
            ["cha_projekodu"] = header.ProjectCode,
            ["cha_aciklama"] = null,
            ["cha_d_kur"] = 1d,
            ["cha_altd_kur"] = 1d,
            ["cha_karsid_kur"] = 1d,
            ["cha_kasa_hizmet"] = accountKind,
            ["cha_kasa_hizkod"] = command.AccountCode,
            ["cha_meblag"] = command.Amount,
            ["cha_aratoplam"] = command.Amount,
            ["cha_vade"] = int.Parse(day.ToString("yyyyMMdd", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
        };
    }
}
