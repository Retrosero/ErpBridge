using ErpBridge.Tools.ForaCatalog;
using FluentAssertions;

namespace ErpBridge.ForaCatalog.Tests;

/// <summary>
/// Guards the extracted editor layouts: which tab a parameter sits on, what label it carries and
/// which editor the panel should offer. The panel's tabs are generated from these files, so a
/// silent regression here would quietly reshape the screens.
/// </summary>
public sealed class UiCatalogTests
{
    private static readonly string Root = ForaCatalogPaths.FindRepositoryRoot(AppContext.BaseDirectory);

    private static ForaCatalogPaths.UiSource Source(string typeName) =>
        ForaCatalogPaths.UiSources.Single(s => s.TypeName == typeName);

    private static UiCatalog Extract(ForaCatalogPaths.UiSource source, string sourceBuild = "unknown")
    {
        var binding = UiSourceBinding.Describe(DefaultsCatalogTests.Extract(), source);

        return UiExtractor.Extract(
            File.ReadAllText(ForaCatalogPaths.Resolve(Root, source.File)),
            source.File,
            source.TypeName,
            sourceBuild,
            binding.Declared,
            binding.Required);
    }

    private static UiCatalog Akilli() => Extract(Source("ForaAndroidKullaniciDuzenleme"));

    private static UiParameter Parameter(string name) =>
        Akilli().Parameters.Single(p => p.Parameter == name);

    public static TheoryData<string> EveryEditor()
    {
        var data = new TheoryData<string>();
        foreach (var source in ForaCatalogPaths.UiSources)
        {
            data.Add(source.TypeName);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(EveryEditor))]
    public void The_committed_layout_matches_the_decompiled_source(string typeName)
    {
        var source = Source(typeName);

        var committed = File.ReadAllText(ForaCatalogPaths.Resolve(Root, source.Output))
            .Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n') + "\n";

        var build = CatalogJson.Deserialize<UiCatalog>(committed).SourceBuild;

        CatalogJson.Serialize(Extract(source, build)).Should().Be(
            committed,
            "the committed layout is stale — run: dotnet run --project tools/ForaCatalogExtractor");
    }

    [Theory]
    [MemberData(nameof(EveryEditor))]
    public void Every_declared_parameter_is_either_placed_or_reported(string typeName)
    {
        var source = Source(typeName);
        var catalog = Extract(source);
        var required = UiSourceBinding.Describe(DefaultsCatalogTests.Extract(), source).Required;

        // The whole point of the gap list: nothing the catalogue declares may vanish without a
        // trace. Every owned parameter is either placed on the screen or listed as a gap.
        var accountedFor = catalog.Parameters.Select(p => p.Parameter)
            .Concat(catalog.Gaps.Where(g => g.Kind is "notInEditor" or "unboundParameter").Select(g => g.Subject))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        required.Where(r => !accountedFor.Contains(r, StringComparer.Ordinal)).Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(EveryEditor))]
    public void Every_parameter_is_attributed_to_a_set_the_editor_maintains(string typeName)
    {
        var source = Source(typeName);
        var catalog = Extract(source);

        var allowed = source.CatalogMethods.Concat(source.AlsoEdits ?? []).ToList();

        catalog.Parameters.Select(p => p.CatalogMethod).Distinct()
            .Should().BeSubsetOf(allowed, "a name alone does not identify a parameter");
        catalog.Parameters.Should().NotContain(p => p.Editor == EditorKinds.Unknown);
        catalog.Parameters.Select(p => p.Order).Should().Equal(Enumerable.Range(0, catalog.ParameterCount));
    }

    [Fact]
    public void Extraction_is_deterministic()
    {
        var source = Source("ForaAndroidKullaniciDuzenleme");

        CatalogJson.Serialize(Extract(source)).Should().Be(CatalogJson.Serialize(Extract(source)));
    }

    [Fact]
    public void Every_editor_has_its_own_layout_file()
    {
        ForaCatalogPaths.UiSources.Select(s => s.Output).Should().OnlyHaveUniqueItems();
        ForaCatalogPaths.UiSources.Select(s => s.TypeName).Should().OnlyHaveUniqueItems();

        // The bulk-import screens are deliberately left to P6; everything else a person can edit
        // in Fora has a layout here.
        ForaCatalogPaths.UiSources.Should().HaveCount(11);
    }

    [Fact]
    public void The_mobile_user_editor_places_nearly_everything()
    {
        var catalog = Akilli();

        catalog.TabCount.Should().Be(65);
        catalog.ParameterCount.Should().Be(1782);
        catalog.UnlabelledCount.Should().Be(0);
        catalog.Parameters.Should().OnlyContain(p => p.Tab != null && p.TabPath.Count > 0);
    }

    [Fact]
    public void Parameters_the_mobile_editor_never_mentions_are_listed()
    {
        var absent = Akilli().Gaps.Where(g => g.Kind == "notInEditor").Select(g => g.Subject).ToList();

        absent.Should().BeEquivalentTo(
            [
                "EvrakSeri_KonsinyeIrsaliyesi",
                "EvrakSeri_KonsinyedenIadeIrsaliyesi",
                "GormeCariUnvan",
                "Goster_AnaMenu_Konsinye_Irsaliyesi",
                "Goster_AnaMenu_Konsinyeden_Iade_Irsaliyesi",
                "HakGormeCariAdresler",
                "KullaniciEtkinlikleriniKayitEt",
                "SiparisKarsilamaFotoGenislik",
                "SiparisKarsilamaFotoYukseklik",
                "StokEklemeArtiEksiButonlariGoster",
                "Vergi0KisaAdi",
                "Vergi0UzunAdi",
                "Vergi0Yuzde",
            ],
            "Fora declares these but its own editor offers no field for them");
    }

    [Fact]
    public void The_firm_wide_editor_hides_the_same_zero_rate_row()
    {
        // Consistent with the mobile editor: Fora never exposes VAT row 0 ("Tanımsız") anywhere.
        var catalog = Extract(Source("GenelParametrelerForm"));

        catalog.ParameterCount.Should().Be(32);
        catalog.Gaps.Where(g => g.Kind == "notInEditor").Select(g => g.Subject)
            .Should().BeEquivalentTo(["Vergi0KisaAdi", "Vergi0UzunAdi", "Vergi0Yuzde"]);

        catalog.Parameters.Single(p => p.Parameter == "Vergi4Yuzde").Editor.Should().Be(EditorKinds.Decimal);
    }

    [Fact]
    public void A_screen_without_tabs_is_read_without_complaint()
    {
        // ComarchEdiGenelParametrelerForm drops its controls straight onto the form with
        // base.Controls.Add(...) and uses no tab control at all.
        var catalog = Extract(Source("ComarchEdiGenelParametrelerForm"));

        catalog.TabCount.Should().Be(0);
        catalog.ParameterCount.Should().Be(14);
        catalog.UnlabelledCount.Should().Be(0);
        catalog.Gaps.Should().BeEmpty();
    }

    [Fact]
    public void A_screen_may_show_parameters_of_a_set_it_does_not_own()
    {
        // The relation screen owns the relation's seven parameters and additionally reads three
        // connection settings from the general EDI set, which has its own editor.
        var source = Source("ComarchEdiIliskiYonetimi");
        var catalog = Extract(source);

        source.CatalogMethods.Should().Equal("ComarchEdiIliskiParametreleri");
        source.AlsoEdits.Should().Equal("ComarchEdiGenelParametreler");

        catalog.ParameterCount.Should().Be(7);
        catalog.Parameters.Should().OnlyContain(p => p.CatalogMethod == "ComarchEdiIliskiParametreleri");

        // The three it only reads are reported, and the rest of that set is not blamed on it.
        catalog.Gaps.Where(g => g.Kind == "unboundParameter").Select(g => g.Subject)
            .Should().BeEquivalentTo(["KullaniciAdi", "Sifre", "ZamanAsimi"]);
        catalog.Gaps.Should().NotContain(g => g.Kind == "notInEditor");
    }

    [Fact]
    public void Foras_bespoke_pickers_become_reference_fields()
    {
        // These four are codes chosen from an ERP list, not free text — exactly the fields D12
        // says the panel must offer a picker for rather than a text box.
        var pickers = Extract(Source("KullaniciDuzenleme")).Parameters
            .Where(p => p.Editor == EditorKinds.Reference)
            .ToList();

        pickers.Select(p => p.ReferenceKind).Should().BeEquivalentTo(["cari", "depo", "kargo", "ekipKodu"]);
        pickers.Should().OnlyContain(p => p.ControlType.EndsWith("Secimi", StringComparison.Ordinal));
    }

    [Fact]
    public void A_value_only_a_purpose_built_screen_can_edit_is_marked_composite()
    {
        // The report definition is a JSON blob behind a bespoke Fora screen; the panel will need
        // its own editor for it, and saying so beats pretending it is a text field.
        var json = Extract(Source("MobilRaporStokSatisDuzenleme")).Parameters
            .Single(p => p.Parameter == "RaporJson");

        json.Editor.Should().Be(EditorKinds.Composite);
        json.ReferenceKind.Should().BeNull();
    }

    [Fact]
    public void The_printer_designer_covers_both_of_its_sets()
    {
        var catalog = Extract(Source("YaziciAyarlariForm"));

        catalog.CatalogMethods.Should().BeEquivalentTo(["genelayarlaritanimla", "alanekle"]);
        catalog.Tabs.Select(t => t.Title).Should().Contain("GENEL");

        var column = catalog.Parameters.Single(p => p.Parameter == "Kolon");
        column.CatalogMethod.Should().Be("alanekle");
        column.Editor.Should().Be(EditorKinds.Integer);
        column.Label.Should().Be("Kolon :");
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
    public void Tabs_are_nested_the_way_Fora_groups_them()
    {
        var tabs = Akilli().Tabs;

        tabs.Where(t => t.Path.Count == 1).Select(t => t.Title).Should().BeEquivalentTo(
            ["Parametreler", "Görünüm ve seçenekler", "Evrak girişi", "Raporlar", "Form dosyaları"],
            "these are the five outermost tabs of the editor");

        // A tab's path ends with its own title (no index-from-end here: this is an expression tree).
        tabs.Should().OnlyContain(t => t.Path.Count > 0 && t.Path[t.Path.Count - 1] == t.Title);
    }

    [Fact]
    public void Check_boxes_take_their_own_caption_as_the_label()
    {
        var parameter = Parameter("Goster_AnaMenu_Tahsilat");

        parameter.ControlType.Should().Be("CheckEdit");
        parameter.LabelSource.Should().Be(LabelSources.Caption);
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
    public void A_control_that_shows_one_parameter_and_saves_another_is_reported()
    {
        // Fora's defect: the check boxes for GosterZyrt_Temsilci_Ozel_24..94_Var_Yok display the
        // 12..19 parameters instead. Opening and saving that screen overwrites each of the former
        // with the latter's value. The subject is the parameter that gets overwritten.
        var mismatches = Akilli().Gaps.Where(g => g.Kind == "bindingMismatch").Select(g => g.Subject).ToList();

        mismatches.Should().BeEquivalentTo(
            new[] { 24, 34, 44, 54, 64, 74, 84, 94 }.Select(i => $"GosterZyrt_Temsilci_Ozel_{i}_Var_Yok"));
    }

    [Fact]
    public void One_parameter_shown_by_several_controls_is_not_a_defect()
    {
        // A printer field's value is edited in one of three controls depending on its data type,
        // and a colour is edited by one picker that feeds three parameters. Neither is a mismatch.
        Extract(Source("YaziciAyarlariForm")).Gaps.Should().NotContain(g => g.Kind == "bindingMismatch");

        Akilli().Parameters.GroupBy(p => p.Control).Where(g => g.Count() > 1).Select(g => g.Key)
            .Should().BeEquivalentTo(["SiparisKarsilamaCariColor", "SiparisKarsilamaStokColor"]);
    }

    [Fact]
    public void The_password_parameter_is_not_bound_to_any_control()
    {
        // Fora decrypts Sifre through a local before showing it, so it never flows straight into a
        // control. It is excluded from our model anyway — see D6 in the goal document.
        var catalog = Akilli();

        catalog.Parameters.Should().NotContain(p => p.Parameter == "Sifre");
        catalog.Gaps.Should().ContainSingle(g => g.Kind == "unboundParameter" && g.Subject == "Sifre");
    }

    [Fact]
    public void The_layout_records_which_Fora_build_it_came_from()
    {
        Extract(Source("ForaAndroidKullaniciDuzenleme"), "17.9.9.9").SourceBuild.Should().Be(
            "17.9.9.9", "a layout and a set of defaults from different releases cannot be mixed");
    }

    [Fact]
    public void A_missing_form_type_is_rejected()
    {
        var extract = () => UiExtractor.Extract(
            "class Baska { }", "test.cs", "Yok", "unknown", new Dictionary<string, string>(), []);

        extract.Should().Throw<CatalogExtractionException>().WithMessage("*not found*");
    }

    [Fact]
    public void A_form_bound_to_the_wrong_catalogue_set_is_rejected()
    {
        // The mapping table is ours, so a wrong entry has to fail loudly rather than produce a
        // layout for parameters the set never declared.
        const string form = """
            public class Ekran
            {
                private TextEdit Kod;
                private void Yukle()
                {
                    Kod.Text = _p._GetParametre("BilinmeyenParametre")._GetString;
                }
            }
            """;

        var extract = () => UiExtractor.Extract(
            form, "test.cs", "Ekran", "unknown", new Dictionary<string, string>(), []);

        extract.Should().Throw<CatalogExtractionException>().WithMessage("*none of its catalogue sets declare*");
    }
}
