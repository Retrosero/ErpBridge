using ErpBridge.Shared;
using FluentAssertions;

namespace ErpBridge.Shared.Tests;

/// <summary>
/// Invariants the tracked-table catalog must hold for the shadow-table change
/// log to be correct.
/// </summary>
public class TrackedTableCatalogTests
{
    /// <summary>
    /// <c>TabloID</c> is the only discriminator stored in the <c>_ERPB_SYNC</c>
    /// shadow row, and the change reader filters on it before joining back to a
    /// single source table. Two tables sharing an id means their triggers write
    /// into one bucket and each reader joins the other table's rows against the
    /// wrong key column — changes vanish on an inner-join miss, or a colliding
    /// RECno matches an unrelated row.
    ///
    /// <para>
    /// This regression guard exists because the catalog shipped with two real
    /// collisions (STOK_SEKTORLERI/STOK_KATEGORILERI on 8, and
    /// BAKIM_HAREKETLERI/BEDEN_HAREKETLERI on 147), copied from the reference
    /// app's id space.
    /// </para>
    /// </summary>
    [Fact]
    public void Every_TabloID_is_unique()
    {
        var duplicates = TrackedTableCatalog.All
            .GroupBy(t => t.TabloID)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(t => t.TabloAdi))}")
            .ToList();

        duplicates.Should().BeEmpty(
            "a shared TabloID silently corrupts the shadow-table change log");
    }

    [Fact]
    public void Every_table_name_is_unique()
    {
        TrackedTableCatalog.All
            .Select(t => t.TabloAdi)
            .Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Every_table_declares_at_least_one_field()
    {
        TrackedTableCatalog.All.Should().OnlyContain(t => t.Fields.Count > 0);
    }

    [Fact]
    public void Every_table_declares_a_recno_field()
    {
        TrackedTableCatalog.All.Should().OnlyContain(t => !string.IsNullOrWhiteSpace(t.RecnoField));
    }

    [Fact]
    public void FindByTabloID_resolves_deterministically()
    {
        foreach (var table in TrackedTableCatalog.All)
        {
            TrackedTableCatalog.FindByTabloID(table.TabloID)!.TabloAdi
                .Should().Be(table.TabloAdi);
        }
    }

    [Fact]
    public void FindByTabloAdi_is_case_insensitive()
    {
        var any = TrackedTableCatalog.All[0];

        TrackedTableCatalog.FindByTabloAdi(any.TabloAdi.ToLowerInvariant())
            .Should().NotBeNull();
    }
}
