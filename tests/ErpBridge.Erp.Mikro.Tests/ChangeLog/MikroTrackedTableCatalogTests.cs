using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Erp.Mikro.ChangeLog;
using FluentAssertions;

namespace ErpBridge.Erp.Mikro.Tests.ChangeLog;

/// <summary>
/// Invariants the Mikro tracked-table catalog must hold for the shadow-table
/// change log to be correct.
///
/// <para>
/// The catalog is transcribed from the Fora Mikro reference application's
/// <c>TabloHelper.GetMikroV14DefaultTablolar()</c>, which has run against
/// production Mikro installs for years. The hand-written predecessor guessed at
/// column names and was wrong for 34 of its 63 tables — hence the live-schema
/// contract test in <c>MikroSchemaContractTests</c> alongside these structural
/// checks.
/// </para>
/// </summary>
public class MikroTrackedTableCatalogTests
{
    private static IReadOnlyList<ErpTrackedTable> Tables => MikroTrackedTableCatalog.V15.Tables;

    [Fact]
    public void Catalog_reports_Mikro_as_its_erp()
    {
        MikroTrackedTableCatalog.V15.Erp.Should().Be(ErpType.Mikro);
    }

    [Fact]
    public void Catalog_carries_the_reference_applications_default_table_set()
    {
        // The reference syncs 49 tables by default. A change to this number is a
        // deliberate scope decision, not an accident.
        Tables.Should().HaveCount(49);
    }

    /// <summary>
    /// <c>TableId</c> is the only discriminator stored in a shadow row, and the
    /// reader filters on it before joining back to one source table. Two tables
    /// sharing an id means each reader pulls the other's rows and the inner join
    /// drops them — the change is lost silently.
    ///
    /// <para>
    /// The reference application ships STOK_KATEGORILERI and STOK_SEKTORLERI both
    /// on id 8; this catalog deliberately renumbers one of them.
    /// </para>
    /// </summary>
    [Fact]
    public void Every_TableId_is_unique()
    {
        var duplicates = Tables
            .GroupBy(t => t.TableId)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(t => t.TableKey))}")
            .ToList();

        duplicates.Should().BeEmpty(
            "a shared TableId makes the change log drop rows for both tables");
    }

    [Fact]
    public void Every_table_key_is_unique()
    {
        Tables.Select(t => t.TableKey).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Every_table_declares_at_least_one_readable_field()
    {
        // The reader builds its SELECT list from Fields; an empty whitelist would
        // produce invalid SQL at runtime.
        Tables.Should().OnlyContain(t => t.Fields.Count > 0);
    }

    [Fact]
    public void Every_tables_key_field_is_in_its_own_field_list()
    {
        // The reader joins on the key column and projects it back out as the
        // event's KeyValue, so it has to be selected.
        foreach (var table in Tables)
        {
            table.Fields.Should().Contain(
                f => string.Equals(f, table.EffectiveKeyField, StringComparison.OrdinalIgnoreCase),
                $"{table.TableKey} joins on {table.EffectiveKeyField}");
        }
    }

    [Fact]
    public void Every_table_uses_the_dbo_schema()
    {
        Tables.Should().OnlyContain(t => t.SchemaName == "dbo");
    }

    [Fact]
    public void Field_lists_contain_no_duplicates()
    {
        foreach (var table in Tables)
        {
            table.Fields.Should().OnlyHaveUniqueItems($"{table.TableKey} would SELECT a column twice");
        }
    }

    [Fact]
    public void Core_business_tables_are_tracked()
    {
        // These carry the field-sales dataset; losing one silently breaks the
        // mobile client's view of stock, customers, prices or ledger.
        foreach (var expected in new[]
                 {
                     "STOKLAR", "CARI_HESAPLAR", "CARI_HESAP_HAREKETLERI", "STOK_HAREKETLERI",
                     "SIPARISLER", "BARKOD_TANIMLARI", "DEPOLAR", "STOK_SATIS_FIYAT_LISTELERI",
                 })
        {
            MikroTrackedTableCatalog.V15.Find(expected)
                .Should().NotBeNull($"{expected} is part of the field-sales dataset");
        }
    }

    [Fact]
    public void Find_is_case_insensitive_on_the_table_key()
    {
        var any = Tables[0];

        MikroTrackedTableCatalog.V15.Find(any.TableKey.ToLowerInvariant())
            .Should().BeSameAs(any);
    }

    [Fact]
    public void Find_returns_null_for_an_unknown_table()
    {
        MikroTrackedTableCatalog.V15.Find("NOT_A_MIKRO_TABLE").Should().BeNull();
        MikroTrackedTableCatalog.V15.FindById(-1).Should().BeNull();
    }

    [Fact]
    public void FindById_resolves_every_declared_table()
    {
        foreach (var table in Tables)
        {
            MikroTrackedTableCatalog.V15.FindById(table.TableId)!.TableKey
                .Should().Be(table.TableKey);
        }
    }
}
