using System.Globalization;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Writers.Documents;

/// <summary>
/// Writes a phone expense as one Mikro V15 <b>kasa masraf fişi</b> (ERP yazım 3, reference §13): a single
/// <c>CARI_HESAP_HAREKETLERI</c> row with <c>cha_evrak_tip = 37</c>.
///
/// <para>This is deliberately <b>not</b> the tediye writer with a different code. In a tediye the account the
/// money left is in <c>cha_kasa_hizmet</c>/<c>cha_kasa_hizkod</c> and the counterparty is a customer. An expense
/// turns that around: <c>cha_kasa_hizmet</c> holds <c>Giderimiz</c> (5) with the <b>expense card</b>, and the
/// paying cash box or bank moves to <c>cha_cari_cins</c>/<c>cha_kod</c>. There is no customer at all.</para>
///
/// <para>Fora writes expenses as a service invoice (<c>AlisFaturasi</c> + <c>HizmetFaturasi</c>) and never writes
/// type 37, but the company's own Mikro screen does — 151 rows of it. Reference §6 settles that disagreement in
/// favour of Mikro's own record, so 37 is what we write (K1).</para>
///
/// <para>Unlike every other document here, an expense carries <b>no</b> <c>EVRAK_ACIKLAMALARI</c> row: the live
/// data has none for type 37, and the note the user typed goes in <c>cha_aciklama</c> instead.</para>
/// </summary>
public sealed class MikroExpenseWriter(MikroDocumentWriteRunner runner)
{
    /// <summary>Job document type the ledger records a phone expense under.</summary>
    public const string DocumentType = "expense";

    /// <summary><c>cha_aciklama</c> is nvarchar(50) in Mikro V15.</summary>
    internal const int DescriptionMaxLength = 50;

    private readonly MikroDocumentWriteRunner _runner = runner ?? throw new ArgumentNullException(nameof(runner));

    public Task<ErpWriteResult> WriteAsync(ExpenseCommand command, MikroConnectionSettings settings, CancellationToken ct = default)
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
    public static async Task<MikroWrittenDocument> WriteAsync(MikroWriteSession session, ExpenseCommand command, CancellationToken ct)
    {
        var header = command.Header;
        if (command.Amount <= 0) throw new MikroWriteException(ErpWriteError.InvalidAmount());
        if (command.VatAmount < 0) throw new MikroWriteException(ErpWriteError.InvalidAmount());
        // The phone shows one figure — what left the till. VAT is part of it, not added on top (K4).
        if (command.VatAmount > command.Amount) throw new MikroWriteException(ErpWriteError.InvalidAmount());
        if (Math.Abs(command.Amount - header.ExpectedTotal) > MikroPriceCalculator.TotalTolerance)
            throw new MikroWriteException(ErpWriteError.TotalMismatch(command.Amount - header.ExpectedTotal));
        if (string.IsNullOrWhiteSpace(command.ExpenseCardCode))
            throw new MikroWriteException(ErpWriteError.ExpenseCardNotFound(command.ExpenseCardCode ?? string.Empty));

        var lookup = new MikroDocumentLookup(session);
        // Both sides are checked before a row exists: an expense on a card or an account Mikro does not
        // have is cheap to refuse now and expensive to unpick from the ledger later.
        await lookup.EnsureExpenseCardAsync(command.ExpenseCardCode, ct).ConfigureAwait(false);
        await lookup.EnsureSalespersonAsync(header.SalespersonCode, ct).ConfigureAwait(false);
        await (command.Method == ExpensePaymentMethod.Cash
            ? lookup.EnsureCashBoxAsync(command.AccountCode, MikroCashBoxKind.Cash, ct)
            : lookup.EnsureBankAsync(command.AccountCode, ct)).ConfigureAwait(false);

        var number = await session.NextNumberAsync(MikroDocumentNumbering.ExpenseReceipt, header.Series, ct).ConfigureAwait(false);
        var recno = await session.InsertAsync(MikroTables.CariHareket, LineRow(command, number), ct).ConfigureAwait(false);

        return new MikroWrittenDocument(MikroTables.CariHareket.Name, MikroCodes.ChaEvrakTip.KasaMasrafFisi, header.Series, number, recno);
    }

    /// <summary>
    /// How the expense was paid: the kind Mikro stamps on the row, and what the paying account is.
    /// A credit card is a <b>bank</b> account in Mikro — the live tediye rows with <c>cha_cinsi=22</c>
    /// carry a <c>BANKALAR.ban_kod</c>, and <c>FIRMA_KREDI_KARTI_TANIMLARI</c> is empty (§13).
    /// </summary>
    internal static (byte Cinsi, byte AccountKind) Kind(ExpensePaymentMethod method) => method switch
    {
        ExpensePaymentMethod.Cash => (MikroCodes.ChaCinsi.Nakit, MikroCodes.HesapCinsi.Kasamiz),
        ExpensePaymentMethod.Transfer => (MikroCodes.ChaCinsi.FirmaHavaleEmri, MikroCodes.HesapCinsi.Bankamiz),
        _ => (MikroCodes.ChaCinsi.FirmaKrediKarti, MikroCodes.HesapCinsi.Bankamiz),
    };

    internal static Dictionary<string, object?> LineRow(ExpenseCommand command, int number)
    {
        var header = command.Header;
        var (cinsi, accountKind) = Kind(command.Method);
        var day = header.OccurredAt.Date;
        var note = header.Description is { Length: > 0 } text
            ? text[..Math.Min(text.Length, DescriptionMaxLength)]
            : string.Empty;
        return new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = MikroCodes.ChaEvrakTip.KasaMasrafFisi,
            ["cha_evrakno_seri"] = header.Series,
            ["cha_evrakno_sira"] = number,
            ["cha_satir_no"] = 0,
            ["cha_tarihi"] = day,
            ["cha_belge_tarih"] = day,
            // An expense leaves the till, so it credits the paying account.
            ["cha_tip"] = MikroCodes.ChaTip.Alacak,
            ["cha_cinsi"] = cinsi,
            ["cha_normal_Iade"] = MikroCodes.NormalIade.Normal,
            ["cha_tpoz"] = MikroCodes.ChaTpoz.Acik,
            // The turned-around pair: paying account here, expense card below.
            ["cha_cari_cins"] = accountKind,
            ["cha_kod"] = command.AccountCode,
            ["cha_kasa_hizmet"] = MikroCodes.HesapCinsi.Giderimiz,
            ["cha_kasa_hizkod"] = command.ExpenseCardCode,
            ["cha_satici_kodu"] = header.SalespersonCode,
            ["cha_srmrkkodu"] = header.ResponsibilityCenterCode,
            ["cha_projekodu"] = header.ProjectCode,
            // No description row for type 37 (§13), so the note lives on the movement itself.
            ["cha_aciklama"] = note,
            ["cha_d_kur"] = 1d,
            ["cha_altd_kur"] = 1d,
            ["cha_karsid_kur"] = 1d,
            ["cha_meblag"] = command.Amount,
            ["cha_aratoplam"] = command.Amount,
            ["cha_vergipntr"] = command.VatPointer,
            ["cha_vergi1"] = command.VatAmount,
            ["cha_vade"] = int.Parse(day.ToString("yyyyMMdd", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
        };
    }
}
