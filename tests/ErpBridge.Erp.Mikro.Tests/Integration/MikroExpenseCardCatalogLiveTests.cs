using Dapper;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Readers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// The expense cards the phone chooses from (ERP yazım 3 Y2b, referans §13), read from a Mikro test copy.
/// They ride in the snapshot's <c>lookups</c> section, so the thing worth proving against a real database
/// is that the query finds them and that their codes are what the writer puts in <c>cha_kasa_hizkod</c>.
/// Read-only; opt in with <c>ERPBridge_RUN_INTEGRATION=1</c> and <c>ERPBridge_MIKRO_WRITE_DB</c>.
/// </summary>
public class MikroExpenseCardCatalogLiveTests
{
    private static MikroDbReader CreateReader()
    {
        var factory = new MikroConnectionFactory();
        factory.SetActiveSettings(new MikroConnectionSettings(
            MikroWriteTestDatabase.Server, string.Empty, string.Empty, MikroWriteTestDatabase.Database!, IntegratedSecurity: true));
        return new MikroDbReader(factory, NullLogger<MikroDbReader>.Instance);
    }

    [Fact]
    public async Task The_lookups_carry_every_usable_expense_card()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        var lookups = await CreateReader().ReadLookupsAsync(firmNo: 0);
        await using var conn = await MikroWriteTestDatabase.OpenAsync();

        var expected = (await conn.QueryAsync<(string Code, string Name)>(
            "SELECT his_kod, ISNULL(his_isim, '') FROM MASRAF_HESAPLARI WHERE ISNULL(his_iptal, 0) = 0 AND ISNULL(his_hidden, 0) = 0"))
            .ToList();
        if (expected.Count == 0) return; // Bu kopyada gider kartı tanımlı değil.

        var cards = lookups.Where(l => l.Kind == "expense_card").ToList();

        cards.Select(c => c.Code).Should().BeEquivalentTo(expected.Select(e => e.Code));
        cards.Should().OnlyContain(c => c.Name.Length > 0, "kart adı olmadan kullanıcı hangisini seçtiğini bilemez");
        // Kod, giderin Mikro'da yazıldığı cha_kasa_hizkod'dur (§13): boşluk ya da kırpılma affetmez.
        cards.Should().OnlyContain(c => c.Code == c.Code.Trim() && c.Code.Length > 0);
    }

    /// <summary>İptal ya da gizlenmiş kart telefona hiç gitmez; kullanıcı seçemediği bir karta gider yazamaz.</summary>
    [Fact]
    public async Task A_cancelled_or_hidden_card_is_left_out()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var excluded = (await conn.QueryAsync<string>(
            "SELECT his_kod FROM MASRAF_HESAPLARI WHERE ISNULL(his_iptal, 0) <> 0 OR ISNULL(his_hidden, 0) <> 0")).ToList();

        var codes = (await CreateReader().ReadLookupsAsync(firmNo: 0))
            .Where(l => l.Kind == "expense_card").Select(l => l.Code).ToList();

        codes.Should().NotIntersectWith(excluded);
    }
}
