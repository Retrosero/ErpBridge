using System.Text.Json;
using ErpBridge.Erp.Abstractions.Sync;
using FluentAssertions;

namespace ErpBridge.Erp.Mikro.Tests.Readers;

/// <summary>
/// The phone reads a ledger row's kasa service under Mikro's own column names
/// (<c>cha_kasa_hizmet</c>/<c>cha_kasa_hizkod</c>); on a kasa masraf fişi they name the
/// expense card the expense screen groups it under. The names are wire contract.
/// </summary>
public class CustomerTransactionPayloadJsonTests
{
    [Fact]
    public void An_expense_row_serializes_its_expense_card_under_the_phone_names()
    {
        var row = new CustomerTransactionPayload(
            "7", "7", "MIKRO", "KASA01", new DateTime(2026, 9, 20), 37, "12", 1, 500m, false, "Mazot",
            new DateTime(2026, 9, 20), 7, CashServiceKind: 5, CashServiceCode: "770.01");

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(row));

        json.RootElement.GetProperty("evrakTip").GetInt32().Should().Be(37);
        json.RootElement.GetProperty("cha_kasa_hizmet").GetInt32().Should().Be(5);
        json.RootElement.GetProperty("cha_kasa_hizkod").GetString().Should().Be("770.01");
    }

    [Fact]
    public void An_older_row_without_the_columns_still_round_trips()
    {
        const string old = """{"id":"1","erpRef":"1","erp":"MIKRO","cariKod":"C1","tarih":"2026-09-20T00:00:00","evrakTip":63,"evrakNo":"A-1","tip":0,"tutar":10,"borcMu":true,"aciklama":null,"updatedAt":"2026-09-20T00:00:00","cha_recno":1}""";

        var row = JsonSerializer.Deserialize<CustomerTransactionPayload>(old)!;

        row.CashServiceKind.Should().BeNull();
        row.CashServiceCode.Should().BeNull();
    }
}
