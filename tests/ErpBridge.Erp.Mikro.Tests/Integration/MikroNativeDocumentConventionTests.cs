using Dapper;
using ErpBridge.Erp.Mikro.Writers;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// Read-only checks that the write conventions in <c>docs/mikro-yazim-referansi.md</c>
/// and <see cref="MikroCodes"/> match documents <b>Mikro itself</b> entered.
///
/// <para>
/// Mikro-entered documents are recognised by an empty series plus a filled
/// <c>cha_uuid</c> on invoices (field apps in the same database leave <c>cha_uuid</c>
/// NULL and write some columns differently — see reference §6). The writers copy
/// Mikro's own shape, so these tests are what keep the codes honest.
/// </para>
///
/// <para>
/// Runs only with <c>ERPBridge_RUN_INTEGRATION=1</c> <b>and</b> an explicit
/// <c>ERPBridge_MIKRO_WRITE_DB</c> naming a copy of a real company database (e.g.
/// <c>MikroDB_V15_DEMO</c>, Windows authentication, server <c>ERPBridge_SCHEMA_SERVER</c> or
/// <c>tcp:localhost</c>). The docker fixture in <c>tests/README-integration.md</c> has no
/// Mikro-entered documents, so the shared gate alone leaves these tests off. Nothing here writes.
/// </para>
/// </summary>
public class MikroNativeDocumentConventionTests
{
    /// <summary>Database the write tests target; this class only reads it.</summary>
    public const string WriteDatabaseEnv = "ERPBridge_MIKRO_WRITE_DB";

    private readonly ITestOutputHelper _output;

    public MikroNativeDocumentConventionTests(ITestOutputHelper output) => _output = output;

    private static bool GateOpen =>
        Environment.GetEnvironmentVariable(MikroIntegrationFixture.RunIntegrationEnv) == "1"
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(WriteDatabaseEnv));

    private static string Database => Environment.GetEnvironmentVariable(WriteDatabaseEnv)!;

    private static string Server =>
        Environment.GetEnvironmentVariable(MikroSchemaContractTests.ServerEnv) is { Length: > 0 } s ? s : "tcp:localhost";

    private static async Task<SqlConnection> OpenAsync()
    {
        var conn = new SqlConnection(
            $"Server={Server};Database={Database};Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15");
        await conn.OpenAsync();
        return conn;
    }

    /// <summary>Mikro-entered cari movements: no series, and invoices carry a uuid.</summary>
    private const string NativeCariRows = @"
FROM CARI_HESAP_HAREKETLERI
WHERE cha_evrakno_seri = ''
  AND (cha_evrak_tip NOT IN (0, 63) OR (cha_uuid IS NOT NULL AND cha_uuid <> ''))";

    [Fact]
    public async Task Mikro_entered_rows_have_no_null_columns()
    {
        if (!GateOpen)
        {
            return; // hermetic default: opt in with ERPBridge_RUN_INTEGRATION=1
        }

        await using var conn = await OpenAsync();
        var samples = new (string Table, string Where)[]
        {
            ("CARI_HESAP_HAREKETLERI", "cha_RECno IN (SELECT TOP 300 cha_RECno " + NativeCariRows + " ORDER BY cha_RECno DESC)"),
            ("STOK_HAREKETLERI", "sth_fat_recid_recno IN (SELECT TOP 100 cha_RECno " + NativeCariRows + " AND cha_evrak_tip IN (0, 63) ORDER BY cha_RECno DESC)"),
            ("ODEME_EMIRLERI", "sck_refno IN (SELECT TOP 100 cha_trefno " + NativeCariRows + " AND cha_trefno <> '' ORDER BY cha_RECno DESC)"),
        };

        var problems = new List<string>();
        foreach (var (table, where) in samples)
        {
            // Column names come from INFORMATION_SCHEMA of a fixed table list, never from input.
            var columns = (await conn.QueryAsync<string>(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table",
                new { table })).ToList();
            var nullCounts = string.Join(", ", columns.Select(c => $"SUM(CASE WHEN [{c}] IS NULL THEN 1 ELSE 0 END) AS [{c}]"));
            var row = (IDictionary<string, object>)await conn.QuerySingleAsync($"SELECT COUNT(*) AS __rows, {nullCounts} FROM {table} WHERE {where}");

            Convert.ToInt32(row["__rows"]).Should().BeGreaterThan(0, $"{table}: the sample must contain Mikro-entered rows");
            problems.AddRange(columns
                .Where(c => row[c] is not null && row[c] is not DBNull && Convert.ToInt32(row[c]) > 0)
                .Select(c => $"{table}.{c}"));
        }

        foreach (var p in problems)
        {
            _output.WriteLine(p);
        }

        problems.Should().BeEmpty("Mikro writes a value into every column, so the writers must not leave NULLs");
    }

    [Fact]
    public async Task Invoice_and_return_headers_use_the_documented_codes()
    {
        if (!GateOpen)
        {
            return;
        }

        await using var conn = await OpenAsync();
        var shapes = (await conn.QueryAsync<(int EvrakTip, int Tip, int Cinsi, int Iade, int CariCins, int Tpoz, int GrupNo, int Count)>(
            "SELECT cha_evrak_tip, cha_tip, cha_cinsi, cha_normal_Iade, cha_cari_cins, cha_tpoz, cha_grupno, COUNT(*) " + NativeCariRows +
            " AND cha_evrak_tip IN (0, 63) GROUP BY cha_evrak_tip, cha_tip, cha_cinsi, cha_normal_Iade, cha_cari_cins, cha_tpoz, cha_grupno")).ToList();
        foreach (var s in shapes)
        {
            _output.WriteLine(s.ToString());
        }

        var c = MikroCodes.ChaEvrakTip.SatisFaturasi;
        var fatura = MikroCodes.ChaCinsi.ToptanFatura;
        // Açık hesap satış faturası.
        shapes.Should().Contain(s => s.EvrakTip == c && s.Tip == MikroCodes.ChaTip.Borc && s.Cinsi == fatura
            && s.Iade == MikroCodes.NormalIade.Normal && s.CariCins == MikroCodes.HesapCinsi.Carimiz && s.Tpoz == MikroCodes.ChaTpoz.Acik);
        // Peşin satış: kasaya kapalı (grup 0) ve bankaya kapalı (grup 1).
        shapes.Should().Contain(s => s.EvrakTip == c && s.CariCins == MikroCodes.HesapCinsi.Kasamiz && s.Tpoz == MikroCodes.ChaTpoz.Kapali && s.GrupNo == 0);
        shapes.Should().Contain(s => s.EvrakTip == c && s.CariCins == MikroCodes.HesapCinsi.Bankamiz && s.Tpoz == MikroCodes.ChaTpoz.Kapali && s.GrupNo == 1);
        // Satıştan iade: alış faturası kodu, alacak, iade bayrağı.
        shapes.Should().Contain(s => s.EvrakTip == MikroCodes.ChaEvrakTip.AlisFaturasi && s.Tip == MikroCodes.ChaTip.Alacak
            && s.Cinsi == fatura && s.Iade == MikroCodes.NormalIade.Iade);

        // A closed invoice keeps the customer in cha_ciro_cari_kodu.
        var closedWithoutCustomer = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) " + NativeCariRows + " AND cha_evrak_tip = 63 AND cha_tpoz = 1 AND cha_ciro_cari_kodu = ''");
        closedWithoutCustomer.Should().Be(0);
    }

    [Fact]
    public async Task Invoice_and_return_lines_use_the_documented_codes()
    {
        if (!GateOpen)
        {
            return;
        }

        await using var conn = await OpenAsync();
        var shapes = (await conn.QueryAsync<(int HeaderTip, int HeaderIade, int EvrakTip, int Tip, int Iade, int IskMas1, int IskMas2, int IskMas10, int Count)>(@"
SELECT h.cha_evrak_tip, h.cha_normal_Iade, s.sth_evraktip, s.sth_tip, s.sth_normal_iade, s.sth_isk_mas1, s.sth_isk_mas2, s.sth_isk_mas10, COUNT(*)
FROM STOK_HAREKETLERI s
JOIN CARI_HESAP_HAREKETLERI h ON h.cha_RECno = s.sth_fat_recid_recno
WHERE h.cha_RECno IN (SELECT TOP 500 cha_RECno " + NativeCariRows + @" AND cha_evrak_tip IN (0, 63) ORDER BY cha_RECno DESC)
GROUP BY h.cha_evrak_tip, h.cha_normal_Iade, s.sth_evraktip, s.sth_tip, s.sth_normal_iade, s.sth_isk_mas1, s.sth_isk_mas2, s.sth_isk_mas10")).ToList();
        foreach (var s in shapes)
        {
            _output.WriteLine(s.ToString());
        }

        shapes.Should().Contain(s => s.HeaderTip == MikroCodes.ChaEvrakTip.SatisFaturasi && s.HeaderIade == 0
            && s.EvrakTip == MikroCodes.SthEvrakTip.CikisFaturasi && s.Tip == MikroCodes.SthTip.Cikis && s.Iade == 0);
        shapes.Should().Contain(s => s.HeaderTip == MikroCodes.ChaEvrakTip.AlisFaturasi && s.HeaderIade == 1
            && s.EvrakTip == MikroCodes.SthEvrakTip.GirisFaturasi && s.Tip == MikroCodes.SthTip.Giris && s.Iade == 1);
        // Satış lines never carry another stock document kind.
        shapes.Where(s => s.HeaderTip == MikroCodes.ChaEvrakTip.SatisFaturasi && s.HeaderIade == 0)
            .Should().OnlyContain(s => s.EvrakTip == MikroCodes.SthEvrakTip.CikisFaturasi);

        // Discount chain: slot 1 applies to the gross amount, later slots to what is left.
        var dominant = shapes.OrderByDescending(s => s.Count).First();
        dominant.IskMas1.Should().Be(0);
        dominant.IskMas2.Should().Be(1);
        dominant.IskMas10.Should().Be(1);
    }

    [Fact]
    public async Task Receipt_lines_and_payment_orders_use_the_documented_codes()
    {
        if (!GateOpen)
        {
            return;
        }

        await using var conn = await OpenAsync();
        var lines = (await conn.QueryAsync<(int Cinsi, int KasaHizmet, int SntckPoz, int KarsidGrupNo, string RefPrefix, int? SckTip, int? SckSonPoz, int? NeredeCins, int? NeredeGrup, int Count)>(@"
SELECT h.cha_cinsi, h.cha_kasa_hizmet, h.cha_sntck_poz, h.cha_karsidgrupno, LEFT(h.cha_trefno, 2),
       o.sck_tip, o.sck_sonpoz, o.sck_nerede_cari_cins, o.sck_nerede_cari_grupno, COUNT(*)
FROM CARI_HESAP_HAREKETLERI h
LEFT JOIN ODEME_EMIRLERI o ON o.sck_refno = h.cha_trefno AND h.cha_trefno <> ''
WHERE h.cha_evrak_tip = 1 AND h.cha_evrakno_seri = '' AND h.cha_tip = 1
GROUP BY h.cha_cinsi, h.cha_kasa_hizmet, h.cha_sntck_poz, h.cha_karsidgrupno, LEFT(h.cha_trefno, 2),
         o.sck_tip, o.sck_sonpoz, o.sck_nerede_cari_cins, o.sck_nerede_cari_grupno")).ToList();
        foreach (var l in lines)
        {
            _output.WriteLine(l.ToString());
        }

        var kasa = MikroCodes.HesapCinsi.Kasamiz;
        var banka = MikroCodes.HesapCinsi.Bankamiz;
        lines.Should().Contain(l => l.Cinsi == MikroCodes.ChaCinsi.Nakit && l.KasaHizmet == kasa && l.RefPrefix == "" && l.SckTip == null);
        lines.Should().Contain(l => l.Cinsi == MikroCodes.ChaCinsi.MusteriKrediKarti && l.KasaHizmet == banka
            && l.SntckPoz == MikroCodes.EvrakPozisyonu.Tahsilde && l.KarsidGrupNo == MikroCodes.BankaGrupNo.KrediKarti && l.RefPrefix == "MK"
            && l.SckTip == MikroCodes.SckTip.MusteriKrediKarti && l.SckSonPoz == MikroCodes.EvrakPozisyonu.Tahsilde
            && l.NeredeCins == banka && l.NeredeGrup == MikroCodes.BankaGrupNo.KrediKarti);
        lines.Should().Contain(l => l.Cinsi == MikroCodes.ChaCinsi.MusteriHavaleSozu && l.KasaHizmet == banka
            && l.KarsidGrupNo == MikroCodes.BankaGrupNo.Havale && l.RefPrefix == "MH"
            && l.SckTip == MikroCodes.SckTip.MusteriHavaleSozu && l.NeredeGrup == MikroCodes.BankaGrupNo.Havale);
        lines.Should().Contain(l => l.Cinsi == MikroCodes.ChaCinsi.MusteriCeki && l.KasaHizmet == kasa && l.RefPrefix == "MC"
            && l.SckTip == MikroCodes.SckTip.MusteriCeki && l.SckSonPoz == MikroCodes.EvrakPozisyonu.Portfoyde && l.NeredeCins == kasa);
        lines.Should().Contain(l => l.Cinsi == MikroCodes.ChaCinsi.MusteriSenedi && l.KasaHizmet == kasa && l.RefPrefix == "MS"
            && l.SckTip == MikroCodes.SckTip.MusteriSenedi && l.SckSonPoz == MikroCodes.EvrakPozisyonu.Portfoyde && l.NeredeCins == kasa);

        // Mikro's reference numbers are MK-fff-sss-yyyy-nnnnnnnn (24 characters, 8-digit counter).
        var badRefs = await conn.ExecuteScalarAsync<int>(@"
SELECT COUNT(*) FROM CARI_HESAP_HAREKETLERI
WHERE cha_evrak_tip = 1 AND cha_evrakno_seri = '' AND cha_trefno <> ''
  AND cha_trefno NOT LIKE '[A-Z][A-Z]-[0-9][0-9][0-9]-[0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
        badRefs.Should().Be(0);
    }

    /// <summary>
    /// ERP yazım 2 (Z0b): the purchase invoice, read from documents Mikro entered itself. Fora — the application
    /// that did this successfully before — writes exactly these codes, and the live company database agrees.
    /// </summary>
    [Fact]
    public async Task Purchase_invoice_headers_and_lines_use_the_documented_codes()
    {
        if (!GateOpen)
        {
            return;
        }

        await using var conn = await OpenAsync();
        var headers = (await conn.QueryAsync<(int Tip, int Cinsi, int Iade, int CariCins, int Tpoz, int Ticaret, int Count)>(
            "SELECT cha_tip, cha_cinsi, cha_normal_Iade, cha_cari_cins, cha_tpoz, cha_ticaret_turu, COUNT(*) " + NativeCariRows +
            " AND cha_evrak_tip = 0 AND cha_normal_Iade = 0" +
            " GROUP BY cha_tip, cha_cinsi, cha_normal_Iade, cha_cari_cins, cha_tpoz, cha_ticaret_turu")).ToList();
        foreach (var h in headers)
        {
            _output.WriteLine(h.ToString());
        }

        // Açık alış: tedarikçiye alacak, toptan fatura, cari satırı.
        headers.Should().Contain(h => h.Tip == MikroCodes.ChaTip.Alacak && h.Cinsi == MikroCodes.ChaCinsi.ToptanFatura
            && h.CariCins == MikroCodes.HesapCinsi.Carimiz && h.Tpoz == MikroCodes.ChaTpoz.Acik);
        // Peşin alış = kapalı fatura, satışın aynası (§10).
        headers.Should().Contain(h => h.Tpoz == MikroCodes.ChaTpoz.Kapali
            && (h.CariCins == MikroCodes.HesapCinsi.Kasamiz || h.CariCins == MikroCodes.HesapCinsi.Bankamiz));

        // Kapalı alış faturası tedarikçiyi cha_ciro_cari_kodu'nda tutar — satıştaki kuralın aynısı.
        var closedWithoutSupplier = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) " + NativeCariRows + " AND cha_evrak_tip = 0 AND cha_normal_Iade = 0 AND cha_tpoz = 1 AND cha_ciro_cari_kodu = ''");
        closedWithoutSupplier.Should().Be(0);

        var lines = (await conn.QueryAsync<(int Tip, int Cins, int Iade, int CariCinsi, int Count)>(@"
SELECT sth_tip, sth_cins, sth_normal_iade, sth_cari_cinsi, COUNT(*)
FROM STOK_HAREKETLERI
WHERE sth_evraktip = 3 AND sth_evrakno_seri = '' AND sth_normal_iade = 0
GROUP BY sth_tip, sth_cins, sth_normal_iade, sth_cari_cinsi")).ToList();
        foreach (var l in lines)
        {
            _output.WriteLine(l.ToString());
        }

        // Alış kalemi: giriş faturası, stok girer.
        lines.Should().Contain(l => l.Tip == MikroCodes.SthTip.Giris && l.Cins == MikroCodes.SthCins.Toptan
            && l.CariCinsi == MikroCodes.HesapCinsi.Carimiz);
    }

    /// <summary>
    /// ERP yazım 2 (Z0b): the purchase invoice and the sales return **share** <c>cha_evrak_tip = 0</c>, separated only
    /// by the return flag — so the next document number has to be taken across both. A writer that filtered on
    /// <c>cha_normal_Iade</c> would hand out a number the other kind already used, and Mikro's unique index
    /// (<c>evrak tip, seri, sıra, satır</c>) would refuse the document.
    /// </summary>
    [Fact]
    public async Task Purchase_invoices_and_sales_returns_share_one_number_space()
    {
        if (!GateOpen)
        {
            return;
        }

        await using var conn = await OpenAsync();
        var purchases = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) " + NativeCariRows + " AND cha_evrak_tip = 0 AND cha_normal_Iade = 0");
        var returns = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) " + NativeCariRows + " AND cha_evrak_tip = 0 AND cha_normal_Iade = 1");
        _output.WriteLine($"alış={purchases} satış iadesi={returns}");
        purchases.Should().BeGreaterThan(0, "the company buys");
        returns.Should().BeGreaterThan(0, "and takes returns");

        // Aynı (seri, sıra) hem alışta hem iadede kullanılmamış: tek numara uzayı.
        var shared = await conn.ExecuteScalarAsync<int>(@"
SELECT COUNT(*) FROM (
    SELECT cha_evrakno_seri, cha_evrakno_sira
    FROM CARI_HESAP_HAREKETLERI
    WHERE cha_evrak_tip = 0 AND cha_satir_no = 0
    GROUP BY cha_evrakno_seri, cha_evrakno_sira
    HAVING COUNT(DISTINCT cha_normal_Iade) > 1
) AS çakışan");
        shared.Should().Be(0, "one number belongs to one document, whichever kind it is");
    }

    /// <summary>ERP yazım 2 (Z0c): the disbursement receipt — the collection's mirror.</summary>
    [Fact]
    public async Task Disbursement_headers_use_the_documented_codes()
    {
        if (!GateOpen)
        {
            return;
        }

        await using var conn = await OpenAsync();
        var shapes = (await conn.QueryAsync<(int Tip, int Cinsi, int CariCins, int KasaHizmet, int Count)>(
            "SELECT cha_tip, cha_cinsi, cha_cari_cins, cha_kasa_hizmet, COUNT(*) " + NativeCariRows +
            " AND cha_evrak_tip = 64 GROUP BY cha_tip, cha_cinsi, cha_cari_cins, cha_kasa_hizmet")).ToList();
        foreach (var s in shapes)
        {
            _output.WriteLine(s.ToString());
        }

        shapes.Should().NotBeEmpty("the company pays money out");
        // Tediye borçtur (tahsilatın tersi) ve nakitte kasa hesabını taşır.
        shapes.Should().Contain(s => s.Tip == MikroCodes.ChaTip.Borc && s.Cinsi == MikroCodes.ChaCinsi.Nakit
            && s.CariCins == MikroCodes.HesapCinsi.Carimiz && s.KasaHizmet == MikroCodes.HesapCinsi.Kasamiz);
        // Havale/EFT firma tarafıdır: FirmaHavaleEmri (20), banka hesabı. Tahsilatın 17'si gelen havaledir.
        shapes.Should().Contain(s => s.Cinsi == FirmaHavaleEmri && s.KasaHizmet == MikroCodes.HesapCinsi.Bankamiz);

        // Ayrı kasa/banka satırı yok: her satır cariye ait, hesap aynı satırda kasa_hizmet/kasa_hizkod'da.
        shapes.Should().OnlyContain(s => s.CariCins == MikroCodes.HesapCinsi.Carimiz && s.Tip == MikroCodes.ChaTip.Borc,
            "a disbursement is one row per payment method, always on the account's side");
        var withoutAccount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) " + NativeCariRows + " AND cha_evrak_tip = 64 AND cha_kasa_hizkod = ''");
        withoutAccount.Should().Be(0, "every disbursement names the cash box or bank it left");
    }

    /// <summary>
    /// Mikro's outgoing payment kinds, the mirror of the collection's customer kinds (reference §11). Named here
    /// because <see cref="MikroCodes"/> only carries what the writers use so far.
    /// </summary>
    private const int FirmaHavaleEmri = 20;

    /// <summary>
    /// ERP yazım 2 (Z0d): what a customer card carries in this company. The writer copies the constants every one
    /// of the 524 cards shares and leaves the fields the company never uses empty — guessing either way would
    /// produce a card the accountant has to fix by hand.
    /// </summary>
    [Fact]
    public async Task Customer_cards_share_the_documented_constants()
    {
        if (!GateOpen)
        {
            return;
        }

        await using var conn = await OpenAsync();
        var row = await conn.QuerySingleAsync<(int Total, int FileId, int HareketTipi, int Doviz, int Doviz1, int Doviz1Recent,
            int VadeFark, int KurHesap, int FaturaAdres, int SevkAdres, int Eft, int TeminatA, int TeminatB, int DepozitoV,
            int DepozitoA)>(@"
SELECT COUNT(*),
       SUM(CASE WHEN cari_fileid = 31 THEN 1 ELSE 0 END),
       SUM(CASE WHEN cari_hareket_tipi = 0 THEN 1 ELSE 0 END),
       SUM(CASE WHEN cari_doviz_cinsi = 0 THEN 1 ELSE 0 END),
       SUM(CASE WHEN cari_doviz_cinsi1 = 255 AND cari_doviz_cinsi2 = 255 THEN 1 ELSE 0 END),
       (SELECT COUNT(*) FROM (SELECT TOP 100 cari_doviz_cinsi1, cari_doviz_cinsi2 FROM CARI_HESAPLAR
            WHERE ISNULL(cari_iptal, 0) = 0 ORDER BY cari_create_date DESC) AS son
        WHERE son.cari_doviz_cinsi1 = 255 AND son.cari_doviz_cinsi2 = 255),
       SUM(CASE WHEN cari_vade_fark_yuz = 25 THEN 1 ELSE 0 END),
       SUM(CASE WHEN cari_KurHesapSekli = 1 THEN 1 ELSE 0 END),
       SUM(CASE WHEN cari_fatura_adres_no = 1 THEN 1 ELSE 0 END),
       SUM(CASE WHEN cari_sevk_adres_no = 1 THEN 1 ELSE 0 END),
       SUM(CASE WHEN cari_EftHesapNum = 1 THEN 1 ELSE 0 END),
       SUM(CASE WHEN cari_TeminatMekAlacakMuhKodu = '910' THEN 1 ELSE 0 END),
       SUM(CASE WHEN cari_TeminatMekBorcMuhKodu = '912' THEN 1 ELSE 0 END),
       SUM(CASE WHEN cari_VerilenDepozitoTeminatMuhKodu = '226' THEN 1 ELSE 0 END),
       SUM(CASE WHEN cari_AlinanDepozitoTeminatMuhKodu = '326' THEN 1 ELSE 0 END)
FROM CARI_HESAPLAR WHERE ISNULL(cari_iptal, 0) = 0");

        _output.WriteLine(row.ToString());
        row.Total.Should().BeGreaterThan(0);
        // Reference §12: these are the same on every card, so the writer may hard-code them.
        row.FileId.Should().Be(row.Total);
        row.HareketTipi.Should().Be(row.Total);
        row.Doviz.Should().Be(row.Total);
        // Currency slots: 255 on the 100 most recent cards. Two old cards carry 127, so this is "what Mikro
        // writes today" rather than "what every row has ever had" — and the writer copies today's value.
        row.Doviz1Recent.Should().Be(100);
        row.Doviz1.Should().BeGreaterThan(row.Total - 10, "the exception is a couple of legacy cards");
        row.VadeFark.Should().Be(row.Total);
        row.KurHesap.Should().Be(row.Total);
        row.FaturaAdres.Should().Be(row.Total);
        row.SevkAdres.Should().Be(row.Total);
        row.Eft.Should().Be(row.Total);
        row.TeminatA.Should().Be(row.Total);
        row.TeminatB.Should().Be(row.Total);
        row.DepozitoV.Should().Be(row.Total);
        row.DepozitoA.Should().Be(row.Total);

        // V15 identity: DBCno 0 and the row points at itself (reference §1, §12). Measured on the recent cards
        // for the same reason as the currency slots — two legacy cards were written before this settled.
        var selfLink = await conn.ExecuteScalarAsync<int>(@"
SELECT COUNT(*) FROM (SELECT TOP 100 cari_RECno, cari_RECid_DBCno, cari_RECid_RECno FROM CARI_HESAPLAR
    WHERE ISNULL(cari_iptal, 0) = 0 ORDER BY cari_create_date DESC) AS son
WHERE son.cari_RECid_DBCno = 0 AND son.cari_RECid_RECno = son.cari_RECno");
        selfLink.Should().Be(100, "a V15 customer card points at itself, like every other V15 row");

        // The code is unique on its own, so a duplicate is the database's refusal, not ours to discover late.
        var uniqueOnCode = await conn.ExecuteScalarAsync<int>(@"
SELECT COUNT(*) FROM sys.indexes i
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE i.object_id = OBJECT_ID('CARI_HESAPLAR') AND i.is_unique = 1 AND c.name = 'cari_kod'
  AND (SELECT COUNT(*) FROM sys.index_columns k WHERE k.object_id = i.object_id AND k.index_id = i.index_id) = 1");
        uniqueOnCode.Should().Be(1, "cari_kod carries a unique index of its own");
    }
}
