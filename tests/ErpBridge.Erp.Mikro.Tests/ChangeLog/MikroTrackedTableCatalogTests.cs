using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Erp.Mikro.ChangeLog;
using ErpBridge.Shared;
using FluentAssertions;

namespace ErpBridge.Erp.Mikro.Tests.ChangeLog;

/// <summary>
/// Faz 18 moves change capture behind an ERP-neutral engine driven by
/// <see cref="IErpTrackedTableCatalog"/>. Mikro's catalog is a projection of the
/// legacy <see cref="TrackedTableCatalog"/>, and it must be <b>lossless</b>:
/// <c>TabloID</c> values are written into shadow rows and echoed to the Android
/// client, so a renumbering would silently corrupt existing installs.
/// </summary>
public class MikroTrackedTableCatalogTests
{
    [Fact]
    public void Catalog_reports_Mikro_as_its_erp()
    {
        MikroTrackedTableCatalog.Instance.Erp.Should().Be(ErpType.Mikro);
    }

    [Fact]
    public void Projection_covers_every_legacy_tracked_table()
    {
        MikroTrackedTableCatalog.Instance.Tables
            .Should().HaveCount(TrackedTableCatalog.All.Count);
    }

    [Fact]
    public void Table_ids_and_names_survive_the_projection_unchanged()
    {
        foreach (var legacy in TrackedTableCatalog.All)
        {
            var projected = MikroTrackedTableCatalog.Instance.FindById(legacy.TabloID);

            projected.Should().NotBeNull($"TabloID {legacy.TabloID} ({legacy.TabloAdi}) must survive");
            projected!.TableKey.Should().Be(legacy.TabloAdi);
            projected.TableName.Should().Be(legacy.TabloAdi);
            projected.PrimaryKeyField.Should().Be(legacy.RecnoField);
            projected.Fields.Should().BeEquivalentTo(legacy.Fields);
            projected.RequiresSoftDeleteFilter.Should().Be(legacy.RequiresSoftDeleteFilter);
        }
    }

    [Fact]
    public void Key_field_and_kind_survive_the_projection()
    {
        foreach (var legacy in TrackedTableCatalog.All)
        {
            var projected = MikroTrackedTableCatalog.Instance.FindById(legacy.TabloID)!;

            projected.EffectiveKeyField.Should().Be(legacy.EffectiveKeyField);
            projected.KeyKind.Should().Be(
                legacy.KeyKind == RowKeyKind.Guid ? ErpRowKeyKind.Guid : ErpRowKeyKind.Int);
        }
    }

    [Fact]
    public void Table_ids_are_unique()
    {
        MikroTrackedTableCatalog.Instance.Tables
            .Select(t => t.TableId)
            .Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Find_is_case_insensitive_on_the_table_key()
    {
        var any = MikroTrackedTableCatalog.Instance.Tables[0];

        MikroTrackedTableCatalog.Instance.Find(any.TableKey.ToLowerInvariant())
            .Should().BeSameAs(any);
    }

    [Fact]
    public void Find_returns_null_for_an_unknown_table()
    {
        MikroTrackedTableCatalog.Instance.Find("NOT_A_MIKRO_TABLE").Should().BeNull();
        MikroTrackedTableCatalog.Instance.FindById(-1).Should().BeNull();
    }

    [Fact]
    public void Every_table_declares_at_least_one_readable_field()
    {
        // The change-log reader builds its SELECT list from Fields; an empty
        // whitelist would produce invalid SQL at runtime.
        MikroTrackedTableCatalog.Instance.Tables
            .Should().OnlyContain(t => t.Fields.Count > 0);
    }

    [Fact]
    public void Every_table_uses_the_dbo_schema()
    {
        MikroTrackedTableCatalog.Instance.Tables
            .Should().OnlyContain(t => t.SchemaName == "dbo");
    }
}
