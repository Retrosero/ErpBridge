using ErpBridge.Tools.ForaCatalog;
using FluentAssertions;

namespace ErpBridge.ForaCatalog.Tests;

/// <summary>
/// Guards the extracted editor layout: which tab a parameter sits on, what label it carries
/// and which editor the panel should offer. The panel's 63-odd tabs are generated from this
/// file, so a silent regression here would quietly reshape the whole screen.
/// </summary>
public sealed class UiCatalogTests
{
    private static readonly string Root = ForaCatalogPaths.FindRepositoryRoot(AppContext.BaseDirectory);

    private static UiCatalog Extract() =>
        UiExtractor.Extract(
            File.ReadAllText(ForaCatalogPaths.Resolve(Root, ForaCatalogPaths.AkilliUiSource)),
            ForaCatalogPaths.AkilliUiSource,
            ForaCatalogPaths.AkilliUiType);

    private static UiParameter Parameter(string name) =>
        Extract().Parameters.Single(p => p.Parameter == name);

    [Fact]
    public void The_committed_layout_matches_the_decompiled_source()
    {
        var committed = File.ReadAllText(ForaCatalogPaths.Resolve(Root, ForaCatalogPaths.AkilliUiOutput))
            .Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n') + "\n";

        CatalogJson.Serialize(Extract()).Should().Be(
            committed,
            "the committed layout is stale — run: dotnet run --project tools/ForaCatalogExtractor");
    }

    [Fact]
    public void Extraction_is_deterministic()
    {
        CatalogJson.Serialize(Extract()).Should().Be(CatalogJson.Serialize(Extract()));
    }

    [Fact]
    public void Nearly_every_akilli_parameter_is_placed_on_a_tab()
    {
        var catalog = Extract();

        catalog.TabCount.Should().Be(65);
        catalog.ParameterCount.Should().Be(1782);

        // Fora declares 1801; Sifre never reaches a control, and 18 more are declared but not
        // exposed by this editor. Nothing may be placed without a tab.
        catalog.Parameters.Should().OnlyContain(p => p.Tab != null && p.TabPath.Count > 0);
    }

    [Fact]
    public void Every_placed_parameter_exists_in_the_defaults_catalogue()
    {
        var defaults = DefaultsExtractor.Extract(
            File.ReadAllText(ForaCatalogPaths.Resolve(Root, ForaCatalogPaths.DefaultsSource)),
            ForaCatalogPaths.DefaultsSource,
            "unknown");

        var declared = defaults.Sets.Single(s => s.CatalogMethod == "MobilKullanici")
            .Parameters.Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var placed = Extract().Parameters.Select(p => p.Parameter).ToList();

        placed.Where(p => !declared.Contains(p)).Should().BeEmpty(
            "the editor must not bind a parameter the catalogue does not declare");
    }

    [Fact]
    public void Tabs_are_nested_the_way_Fora_groups_them()
    {
        var tabs = Extract().Tabs;

        tabs.Where(t => t.Path.Count == 1).Select(t => t.Title).Should().BeEquivalentTo(
            ["Parametreler", "Görünüm ve seçenekler", "Evrak girişi", "Raporlar", "Form dosyaları"],
            "these are the five outermost tabs of the editor");

        // A tab's path ends with its own title (no index-from-end here: this is an expression tree).
        tabs.Should().OnlyContain(t => t.Path.Count > 0 && t.Path[t.Path.Count - 1] == t.Title);
    }

    [Theory]
    // parameter, tab path, editor, label
    [InlineData("Goster_AnaMenu_Tahsilat", "Evrak girişi / Evrak Tipleri / Tahsilat / Tediye makbuzu",
        EditorKinds.Boolean, "Tahsilat girebilir")]
    [InlineData("CariPersonelKodu", "Parametreler / Tanımlamalar", EditorKinds.Text, "Cari personel kodu :")]
    [InlineData("EvrakSeri_SatisFaturasi", "Evrak girişi / Evrak Tipleri / Satış faturası",
        EditorKinds.Text, "Evrak seri :")]
    [InlineData("SenkronizeEt_KASALAR", "Parametreler / Senkronizasyon / Senkronize edilecek tablolar",
        EditorKinds.Boolean, "KASALAR")]
    [InlineData("Vergi4Yuzde", "Parametreler / Vergi oranları", EditorKinds.Decimal, "4 :")]
    [InlineData("MetinZyrt_Satis_Yapildi", "Parametreler / Ziyaret anket", EditorKinds.Text, "Satış yapıldı :")]
    [InlineData("StokListelemeIkinciFiyatListeNo", "Görünüm ve seçenekler / Stok / Listeleme görünümü",
        EditorKinds.Integer, "İkinci fiyat liste no : ")]
    public void Known_parameters_land_where_Fora_puts_them(string name, string path, string editor, string label)
    {
        var parameter = Parameter(name);

        string.Join(" / ", parameter.TabPath).Should().Be(path);
        parameter.Editor.Should().Be(editor);
        parameter.Label.Should().Be(label);
    }

    [Fact]
    public void Check_boxes_take_their_own_caption_as_the_label()
    {
        var parameter = Parameter("Goster_AnaMenu_Tahsilat");

        parameter.ControlType.Should().Be("CheckEdit");
        parameter.LabelSource.Should().Be(LabelSources.Caption);
    }

    [Fact]
    public void Every_parameter_gets_a_label()
    {
        var catalog = Extract();

        catalog.UnlabelledCount.Should().Be(0);
        catalog.Parameters.Should().OnlyContain(p => p.Label != null && p.LabelSource != LabelSources.None);
    }

    [Fact]
    public void A_tall_field_takes_the_label_aligned_with_its_first_line()
    {
        // CariEkstreMesaj is a 162px MemoEdit; "Mesaj :" sits beside its top edge, not its centre.
        var parameter = Parameter("CariEkstreMesaj");

        parameter.Label.Should().Be("Mesaj :");
        parameter.LabelSource.Should().Be(LabelSources.LabelLeft);
    }

    [Fact]
    public void A_field_that_only_qualifies_a_check_box_borrows_its_caption()
    {
        // This combo has no label of its own; it narrows the check box on the same row.
        var parameter = Parameter("SiparisKarsilamaToplamlariGosterBirimi");

        parameter.Label.Should().Be("Karşılanan miktar / Toplam miktar bilgisini göster");
        parameter.LabelSource.Should().Be(LabelSources.CheckBoxLeft);
    }

    [Fact]
    public void Foras_load_save_mismatches_are_reported_not_inherited()
    {
        // Fora loads GosterZyrt_Temsilci_Ozel_12..19_Var_Yok into the wrong check boxes but saves
        // each from its own. Opening and saving that screen in Fora overwrites one parameter with
        // another's value. The extractor follows the save side and records the disagreement.
        var catalog = Extract();

        var mismatches = catalog.Gaps.Where(g => g.Kind == "bindingMismatch").Select(g => g.Subject).ToList();

        mismatches.Should().BeEquivalentTo(Enumerable.Range(12, 8)
            .Select(i => $"GosterZyrt_Temsilci_Ozel_{i}_Var_Yok"));

        Parameter("GosterZyrt_Temsilci_Ozel_12_Var_Yok").Control
            .Should().Be("GosterZyrt_Temsilci_Ozel_12_Var_Yok", "the save side names the owning control");
    }

    [Fact]
    public void The_password_parameter_is_not_bound_to_any_control()
    {
        // Fora decrypts Sifre through a local before showing it, so it never flows straight into a
        // control. It is excluded from our model anyway — see D6 in the goal document.
        var catalog = Extract();

        catalog.Parameters.Should().NotContain(p => p.Parameter == "Sifre");
        catalog.Gaps.Should().ContainSingle(g => g.Kind == "unboundParameter" && g.Subject == "Sifre");
    }

    [Fact]
    public void One_colour_picker_may_feed_several_parameters()
    {
        // Fora stores a colour as three integers and edits them with a single picker. That is a
        // legitimate many-to-one binding, unlike the mismatches above.
        var catalog = Extract();

        var red = catalog.Parameters.Single(p => p.Parameter == "SiparisKarsilamaCariColorRed");
        var green = catalog.Parameters.Single(p => p.Parameter == "SiparisKarsilamaCariColorGreen");
        var blue = catalog.Parameters.Single(p => p.Parameter == "SiparisKarsilamaCariColorBlue");

        new[] { red, green, blue }.Should().OnlyContain(p =>
            p.Control == "SiparisKarsilamaCariColor" && p.Editor == EditorKinds.Color);

        // Every other control owns exactly one parameter.
        catalog.Parameters.GroupBy(p => p.Control).Where(g => g.Count() > 1).Select(g => g.Key)
            .Should().BeEquivalentTo(["SiparisKarsilamaCariColor", "SiparisKarsilamaStokColor"]);
    }

    [Fact]
    public void Every_parameter_gets_a_concrete_editor()
    {
        Extract().Parameters.Should().NotContain(p => p.Editor == EditorKinds.Unknown);
    }

    [Fact]
    public void Reading_order_is_stable_and_gapless()
    {
        var parameters = Extract().Parameters;

        parameters.Select(p => p.Order).Should().Equal(Enumerable.Range(0, parameters.Count));
    }

    [Fact]
    public void A_missing_form_type_is_rejected()
    {
        var extract = () => UiExtractor.Extract("class Baska { }", "test.cs", "Yok");

        extract.Should().Throw<CatalogExtractionException>().WithMessage("*not found*");
    }
}
