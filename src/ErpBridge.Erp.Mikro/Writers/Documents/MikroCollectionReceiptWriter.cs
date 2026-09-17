using System.Globalization;
using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Writers.Documents;

/// <summary>
/// Writes a phone collection as one Mikro V15 receipt (goal ERP yazım Y3g, reference §5): a
/// <c>CARI_HESAP_HAREKETLERI</c> line per payment method (<c>cha_satir_no</c> 0, 1, …) crediting the
/// customer, and for every payment but cash an <c>ODEME_EMIRLERI</c> row — a card slip or transfer
/// promise already at the bank, a cheque or note in its portfolio cash box — under Mikro's 8-digit
/// reference number. Values follow Mikro's own receipt rows, not the field writer's deviations (§6).
/// </summary>
public sealed class MikroCollectionReceiptWriter(MikroDocumentWriteRunner runner)
{
    /// <summary>Job document type the ledger records a phone collection under.</summary>
    public const string DocumentType = "collection";

    private readonly MikroDocumentWriteRunner _runner = runner ?? throw new ArgumentNullException(nameof(runner));

    public Task<ErpWriteResult> WriteAsync(CollectionCommand command, MikroConnectionSettings settings, CancellationToken ct = default)
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
    public static async Task<MikroWrittenDocument> WriteAsync(MikroWriteSession session, CollectionCommand command, CancellationToken ct)
    {
        var header = command.Header;
        if (command.Payments.Count == 0) throw new MikroWriteException(ErpWriteError.InvalidAmount());
        var total = command.Payments.Sum(p => p.Amount);
        if (Math.Abs(total - header.ExpectedTotal) > MikroPriceCalculator.TotalTolerance)
            throw new MikroWriteException(ErpWriteError.TotalMismatch(total - header.ExpectedTotal));

        var lookup = new MikroDocumentLookup(session);
        var customer = await lookup.CustomerAsync(header.CustomerCode, MikroCustomerUse.Collection, isOrder: false, ct).ConfigureAwait(false);
        await lookup.EnsureSalespersonAsync(header.SalespersonCode, ct).ConfigureAwait(false);
        foreach (var payment in command.Payments)
        {
            await (payment.Method switch
            {
                CollectionMethod.Cash => lookup.EnsureCashBoxAsync(payment.AccountCode, MikroCashBoxKind.Cash, ct),
                CollectionMethod.Cheque => lookup.EnsureCashBoxAsync(payment.AccountCode, MikroCashBoxKind.Cheque, ct),
                CollectionMethod.Note => lookup.EnsureCashBoxAsync(payment.AccountCode, MikroCashBoxKind.Note, ct),
                _ => lookup.EnsureBankAsync(payment.AccountCode, ct),
            }).ConfigureAwait(false);
        }

        var number = await session.NextNumberAsync(MikroDocumentNumbering.CollectionReceipt, header.Series, ct).ConfigureAwait(false);
        var references = new ReferenceNumbers(session, header.OccurredAt.Year);
        var headerRecno = 0;
        for (var i = 0; i < command.Payments.Count; i++)
        {
            var payment = command.Payments[i];
            var kind = Kinds[payment.Method];
            var reference = kind.Prefix is null ? null : await references.NextAsync(kind, ct).ConfigureAwait(false);
            var recno = await session.InsertAsync(MikroTables.CariHareket, LineRow(command, i, number, reference), ct).ConfigureAwait(false);
            if (i == 0) headerRecno = recno;
            if (reference is not null)
            {
                await session.InsertAsync(MikroTables.OdemeEmri, PaymentOrderRow(command, i, number, reference, customer), ct).ConfigureAwait(false);
            }
        }

        if (!string.IsNullOrWhiteSpace(header.Description))
        {
            // Fora writes the receipt's description row only when there is a description; so do Mikro's own receipts.
            var description = MikroSalesInvoiceWriter.DescriptionRow(header.Series, number, header.Description);
            description["egk_hareket_tip"] = 1;
            description["egk_evr_tip"] = MikroCodes.ChaEvrakTip.TahsilatMakbuzu;
            description["egk_evr_doksayisi"] = 0;
            await session.InsertAsync(MikroTables.EvrakAciklama, description, ct).ConfigureAwait(false);
        }

        return new MikroWrittenDocument(MikroTables.CariHareket.Name, MikroCodes.ChaEvrakTip.TahsilatMakbuzu, header.Series, number, headerRecno);
    }

    /// <summary>How each method is posted (reference §5 table).</summary>
    /// <param name="Cinsi"><c>cha_cinsi</c>.</param>
    /// <param name="AccountKind"><c>cha_kasa_hizmet</c> and <c>sck_nerede_cari_cins</c>: 4 cash box, 2 bank.</param>
    /// <param name="GroupNo"><c>cha_karsidgrupno</c> and <c>sck_nerede_cari_grupno</c>.</param>
    /// <param name="Position"><c>cha_sntck_poz</c> and <c>sck_sonpoz</c>: 0 in portfolio, 2 at the bank.</param>
    /// <param name="SckTip"><c>sck_tip</c>; null for cash, which has no payment order.</param>
    /// <param name="Prefix">Reference number prefix; null for cash.</param>
    internal sealed record MethodKind(byte Cinsi, byte AccountKind, byte GroupNo, byte Position, byte? SckTip, string? Prefix);

    internal static readonly IReadOnlyDictionary<CollectionMethod, MethodKind> Kinds = new Dictionary<CollectionMethod, MethodKind>
    {
        [CollectionMethod.Cash] = new(MikroCodes.ChaCinsi.Nakit, MikroCodes.HesapCinsi.Kasamiz, 0, MikroCodes.EvrakPozisyonu.Portfoyde, null, null),
        [CollectionMethod.Card] = new(MikroCodes.ChaCinsi.MusteriKrediKarti, MikroCodes.HesapCinsi.Bankamiz, MikroCodes.BankaGrupNo.KrediKarti, MikroCodes.EvrakPozisyonu.Tahsilde, MikroCodes.SckTip.MusteriKrediKarti, "MK"),
        [CollectionMethod.Transfer] = new(MikroCodes.ChaCinsi.MusteriHavaleSozu, MikroCodes.HesapCinsi.Bankamiz, MikroCodes.BankaGrupNo.Havale, MikroCodes.EvrakPozisyonu.Tahsilde, MikroCodes.SckTip.MusteriHavaleSozu, "MH"),
        [CollectionMethod.Cheque] = new(MikroCodes.ChaCinsi.MusteriCeki, MikroCodes.HesapCinsi.Kasamiz, 0, MikroCodes.EvrakPozisyonu.Portfoyde, MikroCodes.SckTip.MusteriCeki, "MC"),
        [CollectionMethod.Note] = new(MikroCodes.ChaCinsi.MusteriSenedi, MikroCodes.HesapCinsi.Kasamiz, 0, MikroCodes.EvrakPozisyonu.Portfoyde, MikroCodes.SckTip.MusteriSenedi, "MS"),
    };

    internal static Dictionary<string, object?> LineRow(CollectionCommand command, int index, int number, string? reference)
    {
        var header = command.Header;
        var payment = command.Payments[index];
        var kind = Kinds[payment.Method];
        var day = header.OccurredAt.Date;
        return new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = MikroCodes.ChaEvrakTip.TahsilatMakbuzu,
            ["cha_evrakno_seri"] = header.Series,
            ["cha_evrakno_sira"] = number,
            ["cha_satir_no"] = index,
            ["cha_tarihi"] = day,
            ["cha_belge_tarih"] = day,
            ["cha_tip"] = MikroCodes.ChaTip.Alacak,
            ["cha_cinsi"] = kind.Cinsi,
            ["cha_normal_Iade"] = MikroCodes.NormalIade.Normal,
            ["cha_tpoz"] = MikroCodes.ChaTpoz.Acik,
            ["cha_cari_cins"] = MikroCodes.HesapCinsi.Carimiz,
            ["cha_kod"] = header.CustomerCode,
            ["cha_satici_kodu"] = header.SalespersonCode,
            ["cha_srmrkkodu"] = header.ResponsibilityCenterCode,
            ["cha_projekodu"] = header.ProjectCode,
            ["cha_aciklama"] = LineNote(payment),
            ["cha_d_kur"] = 1d,
            ["cha_altd_kur"] = 1d,
            ["cha_karsid_kur"] = 1d,
            ["cha_kasa_hizmet"] = kind.AccountKind,
            ["cha_kasa_hizkod"] = payment.AccountCode,
            ["cha_karsidgrupno"] = kind.GroupNo,
            ["cha_sntck_poz"] = kind.Position,
            ["cha_meblag"] = payment.Amount,
            ["cha_aratoplam"] = payment.Amount,
            ["cha_vade"] = int.Parse(payment.DueDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
            ["cha_trefno"] = reference,
        };
    }

    internal static Dictionary<string, object?> PaymentOrderRow(CollectionCommand command, int index, int number, string reference, MikroCustomer customer)
    {
        var header = command.Header;
        var payment = command.Payments[index];
        var kind = Kinds[payment.Method];
        var day = header.OccurredAt.Date;
        var borclu = payment.Method switch
        {
            CollectionMethod.Cheque when !string.IsNullOrWhiteSpace(payment.Cheque?.Drawer) => payment.Cheque!.Drawer,
            CollectionMethod.Note when !string.IsNullOrWhiteSpace(payment.Note?.Debtor) => payment.Note!.Debtor,
            _ => customer.Title,
        };
        return new Dictionary<string, object?>
        {
            ["sck_tip"] = kind.SckTip,
            ["sck_refno"] = reference,
            ["sck_borclu"] = MikroSalesInvoiceWriter.Cut(borclu, 50),
            ["sck_vdaire_no"] = MikroSalesInvoiceWriter.Cut($"{customer.TaxOffice} {customer.TaxNumber}", 40),
            ["sck_vade"] = payment.DueDate.Date,
            ["sck_tutar"] = payment.Amount,
            ["sck_no"] = payment.Cheque?.No ?? payment.Note?.No,
            ["sck_banka_adres1"] = MikroSalesInvoiceWriter.Cut(payment.Cheque?.BankName, 50),
            ["sck_sube_adres2"] = MikroSalesInvoiceWriter.Cut(payment.Cheque?.Branch, 50),
            ["sck_hesapno_sehir"] = MikroSalesInvoiceWriter.Cut(payment.Cheque?.AccountNo, 30),
            ["sck_duzen_tarih"] = payment.Method == CollectionMethod.Note ? day : null,
            ["sck_sahip_cari_cins"] = MikroCodes.HesapCinsi.Carimiz,
            ["sck_sahip_cari_kodu"] = header.CustomerCode,
            ["sck_nerede_cari_cins"] = kind.AccountKind,
            ["sck_nerede_cari_kodu"] = payment.AccountCode,
            ["sck_nerede_cari_grupno"] = kind.GroupNo,
            ["sck_ilk_hareket_tarihi"] = day,
            ["sck_son_hareket_tarihi"] = day,
            ["sck_ilk_evrak_seri"] = header.Series,
            ["sck_ilk_evrak_sira_no"] = number,
            ["sck_ilk_evrak_satir_no"] = index,
            ["sck_doviz_kur"] = 1d,
            ["sck_sonpoz"] = kind.Position,
            ["sck_srmmrk"] = header.ResponsibilityCenterCode,
            ["sck_projekodu"] = header.ProjectCode,
        };
    }

    /// <summary>
    /// The line note Mikro's screens show: a cheque's <c>/no/bank/branch/account</c>, a note's
    /// <c>/debtor/</c>; a card payment's instalments and surcharge, which are not posted (K11).
    /// </summary>
    internal static string? LineNote(CollectionPayment payment)
    {
        var note = payment.Method switch
        {
            CollectionMethod.Cheque when payment.Cheque is { } c => $"/{c.No}/{c.BankName}/{c.Branch}/{c.AccountNo}",
            CollectionMethod.Note when payment.Note is { } n => $"/{n.Debtor}/",
            CollectionMethod.Card when payment.Installments is > 1 || payment.SurchargeAmount is > 0 => string.Join(", ", new[]
            {
                payment.Installments is > 1 ? $"{payment.Installments} taksit" : null,
                payment.SurchargeAmount is > 0 ? $"vade farkı {payment.SurchargeAmount.Value.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))} TL" : null,
            }.Where(p => p is not null)),
            _ => null,
        };
        return MikroSalesInvoiceWriter.Cut(note, MikroSalesInvoiceWriter.HeaderNoteLength);
    }

    /// <summary>
    /// Mikro's reference numbers: <c>MK/MH/MC/MS-fff-sss-yyyy-nnnnnnnn</c>, the next 8-digit number within the
    /// prefix, firm, branch and year (Fora <c>TahsilatSonRefNoBul</c>). Read under an update range lock so a
    /// second writer waits; numbers taken earlier in the same receipt are counted on.
    /// </summary>
    internal sealed class ReferenceNumbers(MikroWriteSession session, int year)
    {
        private readonly Dictionary<string, int> _last = new(StringComparer.Ordinal);

        public string Prefix(MethodKind kind) =>
            string.Create(CultureInfo.InvariantCulture, $"{kind.Prefix}-{session.CompanyNo:000}-{session.BranchNo:000}-{year}-");

        public async Task<string> NextAsync(MethodKind kind, CancellationToken ct)
        {
            var prefix = Prefix(kind);
            if (!_last.TryGetValue(prefix, out var last))
            {
                last = await session.Connection.ExecuteScalarAsync<int>(new CommandDefinition(@"
SELECT ISNULL(MAX(TRY_CAST(RIGHT(sck_refno, 8) AS int)), 0)
FROM ODEME_EMIRLERI WITH (UPDLOCK, HOLDLOCK)
WHERE sck_tip = @tip AND sck_refno LIKE @pattern",
                    new { tip = kind.SckTip, pattern = prefix + "________" }, session.Transaction, cancellationToken: ct)).ConfigureAwait(false);
            }

            _last[prefix] = ++last;
            return prefix + last.ToString("00000000", CultureInfo.InvariantCulture);
        }
    }
}
