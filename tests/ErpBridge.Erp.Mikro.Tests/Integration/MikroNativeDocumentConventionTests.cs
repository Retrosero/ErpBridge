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
}
