using ErpBridge.CentralApi.CustomerCatalog;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.CustomerCatalog;

/// <summary>GOAL_MUSTERI_KATALOGU §4: the visibility order, one row of the table per case, and the rule checks.</summary>
public sealed class CatalogVisibilityTests
{
    private const string Code = "S1";
    private const string Category = "İÇECEK";

    private static CatalogVisibility Of(string mode, params (string Type, string Key, string Effect)[] rules) =>
        CatalogVisibility.Create(mode, rules.Select(r => ((string?)r.Type, (string?)r.Key, (string?)r.Effect)), out var error)
        ?? throw new InvalidOperationException(error);

    [Theory]
    // 1. A product rule decides, over everything below it.
    [InlineData("all", "product", "allow", true, true, true)]
    [InlineData("only", "product", "allow", true, true, true)]
    [InlineData("all", "product", "deny", false, false, false)]
    // 2. Without one, a product the company hid stays hidden, even under an allowed category.
    [InlineData("all", "category", "allow", true, false, false)]
    [InlineData("only", "category", "allow", true, false, false)]
    // 3. A category rule decides next, over the category's own hidden flag.
    [InlineData("all", "category", "allow", false, true, true)]
    [InlineData("only", "category", "allow", false, true, true)]
    [InlineData("all", "category", "deny", false, false, false)]
    // 4. "only" shows nothing without a rule.
    [InlineData("only", "", "", false, false, false)]
    // 5. Otherwise unless the company hid its category.
    [InlineData("all", "", "", false, false, true)]
    [InlineData("all", "", "", false, true, false)]
    public void The_first_matching_step_decides(string mode, string ruleType, string effect, bool productHidden, bool categoryHidden, bool visible)
    {
        var rules = ruleType switch
        {
            "product" => new[] { ("product", Code, effect) },
            "category" => [("category", Category, effect)],
            _ => [],
        };

        Of(mode, rules).IsVisible(Code, productHidden, Category, categoryHidden).Should().Be(visible);
    }

    [Fact]
    public void The_screen_choices_map_onto_mode_and_rules()
    {
        // "Seçilenler hariç": the main catalog less what is denied.
        var except = Of("all", ("category", "DETERJAN", "deny"), ("product", "S9", "deny"));
        except.IsVisible("S1", false, Category, false).Should().BeTrue();
        except.IsVisible("S2", false, "DETERJAN", false).Should().BeFalse();
        except.IsVisible("S9", false, Category, false).Should().BeFalse();

        // "Yalnız seçilenler": only the allowed ones; a product rule reaches outside the allowed categories.
        var only = Of("only", ("category", Category, "allow"), ("product", "S9", "allow"));
        only.IsVisible("S1", false, Category, false).Should().BeTrue();
        only.IsVisible("S2", false, "DETERJAN", false).Should().BeFalse();
        only.IsVisible("S9", false, "DETERJAN", false).Should().BeTrue();

        // "Bu cariye göster": the company hid it from everyone, this customer sees it.
        Of("all", ("product", "S1", "allow")).IsVisible("s1", productHidden: true, Category, categoryHidden: true).Should().BeTrue("stock codes match case-insensitively");
        CatalogVisibility.Default.IsVisible("S1", true, Category, false).Should().BeFalse();
    }

    [Fact]
    public void Rules_are_normalized_and_survive_storage()
    {
        var created = CatalogVisibility.Create(" ONLY ", [(" Category ", "  İÇECEK ", "ALLOW"), ("product", "S1", "deny")], out var error)!;

        error.Should().BeNull();
        created.Mode.Should().Be("only");
        created.Rules.Should().Equal(new CatalogVisibility.Rule("category", "İÇECEK", "allow"), new CatalogVisibility.Rule("product", "S1", "deny"));
        var json = created.ToJson();
        json.Should().StartWith("{\"mode\":\"only\",\"rules\":[{\"type\":\"category\",\"key\":");
        var parsed = CatalogVisibility.Parse(json);
        parsed.Mode.Should().Be("only");
        parsed.Rules.Should().Equal(created.Rules);
        CatalogVisibility.Parse(Domain.CatalogAccount.DefaultVisibilityJson).Should().Match<CatalogVisibility>(v => v.Mode == "all" && v.Rules.Count == 0);
        CatalogVisibility.Create(null, null, out _)!.Mode.Should().Be("all");
    }

    [Fact]
    public void An_unreadable_row_shows_nothing_rather_than_everything()
    {
        CatalogVisibility.Parse("{not json").IsVisible("S1", false, Category, false).Should().BeFalse();
        CatalogVisibility.Parse("{\"mode\":\"some\"}").IsVisible("S1", false, Category, false).Should().BeFalse();
    }

    [Theory]
    [InlineData("most", "category", "K", "allow")]
    [InlineData("all", "brand", "K", "allow")]
    [InlineData("all", "category", "K", "show")]
    [InlineData("all", "category", " ", "allow")]
    [InlineData("all", "product", "12345678901234567890123456789012345678901234567890123456789012345", "allow")]
    public void Bad_modes_and_rules_are_refused(string mode, string type, string key, string effect)
    {
        CatalogVisibility.Create(mode, [(type, key, effect)], out var error).Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void At_most_two_thousand_rules()
    {
        var rules = Enumerable.Range(0, CatalogVisibility.MaxRules).Select(i => ((string?)"product", (string?)$"S{i}", (string?)"allow")).ToList();
        CatalogVisibility.Create("only", rules, out _).Should().NotBeNull();

        rules.Add(("product", "S-extra", "allow"));
        CatalogVisibility.Create("only", rules, out var error).Should().BeNull();
        error.Should().Contain("2000");
    }
}
