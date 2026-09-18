using ErpBridge.Admin.Api;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Admin.Tests.Api;

/// <summary>
/// The tab tree and the in-tab grouping (P2b). Both are derived from the catalogue rather than
/// written out, so these tests pin the derivation, not a layout someone typed.
/// </summary>
public sealed class ParameterLayoutTests
{
    private static ParameterValueDto Field(
        string name, string? tabPath, string? label = null, bool overridden = false) => new()
    {
        CatalogEntryId = Guid.NewGuid(),
        Name = name,
        Label = label ?? name,
        TabPath = tabPath,
        Editor = "text",
        Value = overridden ? "x" : "",
        DefaultValue = "",
        IsOverridden = overridden,
    };

    [Fact]
    public void The_tree_follows_the_tab_path_the_catalogue_carries()
    {
        var tabs = ParameterLayout.BuildTabs(
        [
            Field("A", "Evrak girişi / Cari risk takibi"),
            Field("B", "Evrak girişi / Miktar girişi"),
            Field("C", "Görünüm ve seçenekler"),
        ]);

        tabs.Should().HaveCount(2);
        tabs[0].Title.Should().Be("Evrak girişi");
        tabs[0].Children.Select(c => c.Title).Should().Equal("Cari risk takibi", "Miktar girişi");
        tabs[0].Children[0].Path.Should().Be("Evrak girişi / Cari risk takibi");
    }

    [Fact]
    public void Tabs_keep_the_order_the_parameters_arrive_in()
    {
        // The server returns them in Fora's own editor order, so first appearance is that order.
        var tabs = ParameterLayout.BuildTabs(
        [
            Field("A", "Raporlar"),
            Field("B", "Evrak girişi"),
            Field("C", "Raporlar"),
        ]);

        tabs.Select(t => t.Title).Should().Equal("Raporlar", "Evrak girişi");
    }

    [Fact]
    public void A_parent_counts_everything_underneath_it()
    {
        var tabs = ParameterLayout.BuildTabs(
        [
            Field("A", "Evrak girişi", overridden: true),
            Field("B", "Evrak girişi / Diğer"),
            Field("C", "Evrak girişi / Diğer / Alt", overridden: true),
        ]);

        // The count is what makes a 63-tab tree navigable: it says where the work is.
        tabs[0].DirectCount.Should().Be(1);
        tabs[0].TotalCount.Should().Be(3);
        tabs[0].OverriddenCount.Should().Be(2);
    }

    [Fact]
    public void A_parameter_with_no_editor_in_Fora_is_collected_rather_than_hidden()
    {
        var tabs = ParameterLayout.BuildTabs([Field("Vergi0Orani", tabPath: null)]);

        // Thirteen akilli parameters have no editor in Fora at all. Dropping them here would
        // repeat Fora's own defect in a panel meant to fix it.
        tabs.Should().ContainSingle();
        tabs[0].Title.Should().Be(ParameterLayout.NoTabTitle);
    }

    [Fact]
    public void Fields_of_a_tab_are_the_ones_on_exactly_that_tab()
    {
        var values = new[]
        {
            Field("A", "Evrak girişi"),
            Field("B", "Evrak girişi / Diğer"),
        };

        ParameterLayout.FieldsOf(values, "Evrak girişi").Should().ContainSingle()
            .Which.Name.Should().Be("A", "a child tab's fields belong to the child");
    }

    [Fact]
    public void A_family_that_repeats_by_index_becomes_one_group()
    {
        var fields = Enumerable.Range(1, 100)
            .Select(i => Field($"GosterZyrt_Temsilci_Ozel_{i}_Derece", "Parametreler / Ziyaret anket", "Göster"))
            .ToArray();

        var groups = ParameterLayout.GroupFields(fields);

        // 830 of akilli's 1,782 fields sit on the survey tab and 800 of those are four kinds over
        // 100 slots. Drawn flat that tab is unusable, and the repetition is in the names.
        groups.Should().ContainSingle();
        groups[0].IsRepeating.Should().BeTrue();
        groups[0].Pattern.Should().Be("GosterZyrt_Temsilci_Ozel_#_Derece");
        groups[0].Items.Should().HaveCount(100);
        groups[0].Title.Should().Contain("100 adet");
    }

    [Fact]
    public void Different_families_on_the_same_tab_stay_apart()
    {
        var fields = Enumerable.Range(1, 4)
            .SelectMany(i => new[]
            {
                Field($"GosterZyrt_Temsilci_Ozel_{i}_Derece", "Anket"),
                Field($"MetinZyrt_Temsilci_Ozel_{i}_Derece", "Anket"),
            })
            .ToArray();

        var groups = ParameterLayout.GroupFields(fields);

        groups.Should().HaveCount(2);
        groups.Select(g => g.Pattern).Should().BeEquivalentTo(
            "GosterZyrt_Temsilci_Ozel_#_Derece", "MetinZyrt_Temsilci_Ozel_#_Derece");
    }

    [Fact]
    public void A_group_takes_the_place_of_its_first_member()
    {
        var fields = new[]
        {
            Field("Tekil", "Anket"),
            Field("Aile_1", "Anket"),
            Field("Aile_2", "Anket"),
            Field("Aile_3", "Anket"),
            Field("Sonraki", "Anket"),
        };

        var groups = ParameterLayout.GroupFields(fields);

        // Keeping Fora's order matters: the operator is looking for a field where Fora put it.
        groups.Select(g => g.Items[0].Name).Should().Equal("Tekil", "Aile_1", "Sonraki");
    }

    [Fact]
    public void Two_of_a_kind_is_not_a_family()
    {
        var fields = new[] { Field("Vergi_1", "V"), Field("Vergi_2", "V") };

        // Collapsing a pair hides more than it saves.
        ParameterLayout.GroupFields(fields).Should().HaveCount(2);
        ParameterLayout.GroupFields(fields).Should().OnlyContain(g => !g.IsRepeating);
    }
}
