using ErpBridge.Erp.Mikro.ChangeTracking;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.ChangeTracking;

public sealed class MikroChangeTableMapTests
{
    [Fact]
    public void Stock_movements_refresh_transactions_and_inventory()
    {
        MikroChangeTableMap.Tables["STOK_HAREKETLERI"]
            .Should().BeEquivalentTo("stockTransactions", "inventory");
    }

    [Theory]
    [InlineData("CARI_HESAPLAR", "customers")]
    [InlineData("STOKLAR", "stocks")]
    [InlineData("SIPARISLER", "openOrders")]
    [InlineData("KASALAR", "cashAndBank")]
    [InlineData("BANKALAR", "cashAndBank")]
    [InlineData("STOK_SATIS_FIYAT_LISTELERI", "prices")]
    public void Source_table_maps_to_expected_section(string table, string section)
    {
        MikroChangeTableMap.Tables[table].Should().Contain(section);
    }
}
