using ErpBridge.Shared;
using FluentAssertions;

namespace ErpBridge.Erp.Mikro.Tests.Trigger;

public class TrackedTableSchemaTests
{
    [Fact]
    public void Catalog_exposes_tables_with_unique_TabloAdi()
    {
        var tables = TrackedTableCatalog.All;
        // Reference app tracks 49 default tables; ErpBridge also includes a
        // few extras (STOK_AMBALAJLARI, ASORTI_TANIMLARI, ...) the reference
        // did not need. We only assert that the catalog is non-empty and
        // every entry has a unique name — the exact count is not part of
        // the contract.
        tables.Should().NotBeEmpty("the catalog must include at least one tracked table");
        tables.Select(t => t.TabloAdi).Should().OnlyHaveUniqueItems(
            "duplicate table names would corrupt the trigger DDL");
    }

    [Fact]
    public void Catalog_entries_have_non_empty_RECno_field_and_at_least_one_field()
    {
        foreach (var schema in TrackedTableCatalog.All)
        {
            schema.RecnoField.Should().NotBeNullOrWhiteSpace(
                $"table {schema.TabloAdi} must declare its primary-key column");
            schema.Fields.Should().NotBeEmpty(
                $"table {schema.TabloAdi} must declare at least one column");
        }
    }

    [Fact]
    public void FindByTabloID_returns_the_matching_table()
    {
        var stoklar = TrackedTableCatalog.FindByTabloID(13);
        stoklar.Should().NotBeNull();
        stoklar!.TabloAdi.Should().Be("STOKLAR");
        stoklar.RecnoField.Should().Be("sto_RECno");
    }

    [Fact]
    public void FindByTabloID_returns_null_for_unknown_id()
    {
        TrackedTableCatalog.FindByTabloID(999_999).Should().BeNull();
    }

    [Fact]
    public void FindByTabloAdi_is_case_insensitive()
    {
        TrackedTableCatalog.FindByTabloAdi("stoklar")!.TabloID.Should().Be(13);
        TrackedTableCatalog.FindByTabloAdi("STOKLAR")!.TabloID.Should().Be(13);
    }

    [Fact]
    public void BuildSelectList_prefixes_with_table_alias()
    {
        var schema = TrackedTableCatalog.FindByTabloAdi("STOKLAR")!;
        var select = schema.BuildSelectList(new[] { "sto_kod", "sto_isim" });
        select.Should().Be("T.sto_kod, T.sto_isim");
    }

    [Fact]
    public void BuildSelectList_rejects_a_field_that_is_not_whitelisted()
    {
        var schema = TrackedTableCatalog.FindByTabloAdi("STOKLAR")!;
        var act = () => schema.BuildSelectList(new[] { "sto_kod", "sto_drop_table" });
        act.Should().Throw<ArgumentException>()
            .WithMessage("*not part of the*STOKLAR*allowlist*");
    }

    [Fact]
    public void Soft_delete_filter_tables_have_their_iptal_field_in_the_allowlist()
    {
        // Soft-delete master-data tables must expose the iptal flag in the
        // allowlist so the consumer can mirror the soft-delete convention.
        foreach (var schema in TrackedTableCatalog.All.Where(t => t.RequiresSoftDeleteFilter))
        {
            var hasIptal = schema.Fields.Any(f => f.EndsWith("_iptal", StringComparison.OrdinalIgnoreCase));
            hasIptal.Should().BeTrue(
                $"soft-delete table {schema.TabloAdi} must expose its _iptal column in the allowlist");
        }
    }

    [Fact]
    public void Catalog_uses_the_installed_Mikro_campaign_and_customer_columns()
    {
        var customers = TrackedTableCatalog.FindByTabloAdi("CARI_HESAPLAR")!;
        customers.Fields.Should().NotContain("cari_tipi");

        var campaigns = TrackedTableCatalog.FindByTabloAdi("STOK_CARI_KAMPANYA_TANIMLARI")!;
        campaigns.RecnoField.Should().Be("kampanya_RECno");
        campaigns.Fields.Should().Contain(new[] { "kampanya_RECno", "kampanya_kod", "kampanya_ismi" });
        campaigns.Fields.Should().NotContain("kam_RECno");

        var priceLists = TrackedTableCatalog.FindByTabloAdi("STOK_SATIS_FIYAT_LISTE_TANIMLARI")!;
        priceLists.Fields.Should().Contain(new[]
        {
            "sfl_RECno", "sfl_RECid_DBCno", "sfl_RECid_RECno", "sfl_SpecRECno", "sfl_iptal",
            "sfl_fileid", "sfl_sirano", "sfl_aciklama", "sfl_fiyatuygulama",
            "sfl_fiyatformul", "sfl_odepluygulama", "sfl_odeplformul",
            "sfl_sabit_odeme_plani", "sfl_kdvdahil", "sfl_ilktarih", "sfl_sontarih",
            "sfl_yerineuygulanacakfiyat", "sfl_kurhesaplamasekli", "sfl_doviz_uygulama",
            "sfl_sabit_doviz", "sfl_iskonto_uygulama", "sfl_sabit_iskonto", "sfl_sabit_kur",
            "sfl_kampanya_uygulama", "sfl_sabit_kampanya", "sfl_kampanya_vade_gozardi",
            "sfl_kampanya_iskonto_gozardi", "sfl_otvdahil", "sfl_oivdahil",
        });
        priceLists.Fields.Should().NotContain("sfl_no");
        priceLists.Fields.Should().NotContain("sfl_adi");
        priceLists.Fields.Should().NotContain("sfl_doviz_cinsi");
    }
}
