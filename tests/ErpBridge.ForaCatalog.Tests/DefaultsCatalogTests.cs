using ErpBridge.Tools.ForaCatalog;
using FluentAssertions;

namespace ErpBridge.ForaCatalog.Tests;

/// <summary>
/// Guards the generated parameter catalogue: it must stay in step with the decompiled
/// sources, regenerate identically every time, and keep the invariants the rest of the
/// parameter work builds on.
/// </summary>
public sealed class DefaultsCatalogTests
{
    private static readonly string Root = ForaCatalogPaths.FindRepositoryRoot(AppContext.BaseDirectory);

    internal static DefaultsCatalog Extract(string sourceBuild = "unknown") =>
        DefaultsExtractor.Extract(
            [
                new DefaultsExtractor.CatalogSource(
                    ForaCatalogPaths.DefaultsSource,
                    ForaCatalogPaths.DefaultsType,
                    File.ReadAllText(ForaCatalogPaths.Resolve(Root, ForaCatalogPaths.DefaultsSource))),
                new DefaultsExtractor.CatalogSource(
                    ForaCatalogPaths.PrinterDefaultsSource,
                    ForaCatalogPaths.PrinterDefaultsType,
                    File.ReadAllText(ForaCatalogPaths.Resolve(Root, ForaCatalogPaths.PrinterDefaultsSource))),
            ],
            sourceBuild);

    private static DefaultsCatalog Committed() =>
        CatalogJson.Deserialize<DefaultsCatalog>(
            File.ReadAllText(ForaCatalogPaths.Resolve(Root, ForaCatalogPaths.DefaultsOutput)));

    private static CatalogSet Set(string catalogMethod) =>
        Extract().Sets.Single(s => s.CatalogMethod == catalogMethod);

    /// <summary>Builds a one-file source out of an inline snippet, for the rejection tests.</summary>
    private static DefaultsExtractor.CatalogSource Snippet(string text, string typeName = "ParametrelerDefault") =>
        new("test.cs", typeName, text);

    [Fact]
    public void The_committed_catalogue_matches_the_decompiled_sources()
    {
        var committedText = File.ReadAllText(ForaCatalogPaths.Resolve(Root, ForaCatalogPaths.DefaultsOutput))
            .Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n') + "\n";

        CatalogJson.Serialize(Extract(Committed().SourceBuild)).Should().Be(
            committedText,
            "the committed catalogue is stale — run: dotnet run --project tools/ForaCatalogExtractor");
    }

    [Fact]
    public void Extraction_is_deterministic()
    {
        CatalogJson.Serialize(Extract()).Should().Be(CatalogJson.Serialize(Extract()));
    }

    [Fact]
    public void Every_declared_set_is_extracted()
    {
        var catalog = Extract();

        catalog.SetCount.Should().Be(21);
        catalog.Sets.Should().HaveCount(catalog.SetCount);
        catalog.ParameterCount.Should().Be(4713);
        catalog.Sets.Sum(s => s.Parameters.Count).Should().Be(catalog.ParameterCount);

        catalog.Sets.Select(s => s.Program).Distinct().Should().HaveCount(
            14, "Fora addresses its settings through fourteen ParametreProgram values");
    }

    [Fact]
    public void The_mobile_user_set_carries_the_whole_akilli_catalogue()
    {
        var akilli = Set("MobilKullanici");

        akilli.Program.Should().Be("akilli");
        akilli.ScopeKind.Should().Be(ScopeKinds.MobileUser);
        akilli.Scopes.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Field = ScopeFields.User, Source = "User" },
                "Fora addresses a mobile user through ParametreUser");
        akilli.AnaGrubu.Should().BeEmpty();
        akilli.AltGrubu.Should().BeEmpty();
        akilli.Parameters.Should().HaveCount(1801);
    }

    [Fact]
    public void Import_templates_are_scoped_by_alt_grubu_not_by_user()
    {
        // The import sets leave ParametreUser empty and address a template through
        // ParametreAltGrubu, with ParametreAnaGrubu acting as a fixed discriminator.
        var sql = Set("GenelAktarimSqlSablon");

        sql.Program.Should().Be("GenelAktarim");
        sql.ScopeKind.Should().Be(ScopeKinds.ImportTemplate);
        sql.Scopes.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Field = ScopeFields.AltGrubu, Source = "SablonAdi" });
        sql.User.Should().BeEmpty();
        sql.AnaGrubu.Should().Be("SqlAktarimSablon");
    }

    [Fact]
    public void A_printer_template_field_is_addressed_by_two_columns()
    {
        // Printer templates are declared imperatively in YaziciAyarlari, not in
        // ParametrelerDefault, and need both the template name and the field name.
        var fields = Set("alanekle");

        fields.Program.Should().Be("YaziciAyarlari");
        fields.ScopeKind.Should().Be(ScopeKinds.PrinterTemplate);
        fields.AnaGrubu.Should().Be("Alan");
        fields.Parameters.Should().HaveCount(16);

        fields.Scopes.Should().BeEquivalentTo(
            [
                new { Field = ScopeFields.User, Source = "_sablonadi" },
                new { Field = ScopeFields.AltGrubu, Source = "alanismi" },
            ],
            options => options.WithStrictOrdering());

        var page = Set("genelayarlaritanimla");
        page.AnaGrubu.Should().Be("GenelAyarlar");
        page.Parameters.Should().HaveCount(9);
        page.Parameters.Single(p => p.Name == "SayfaKolonSayisi").Default.Should().Be("120");
    }

    [Fact]
    public void A_default_supplied_at_runtime_records_where_it_comes_from()
    {
        // A printer field's caption defaults to the field's own name, so there is no constant
        // to record — only the identifier it is taken from.
        var isim = Set("alanekle").Parameters.Single(p => p.Name == "Isim");

        isim.Default.Should().BeEmpty();
        isim.DefaultSource.Should().Be("alanismi");

        Extract().Sets.SelectMany(s => s.Parameters).Count(p => p.DefaultSource is not null)
            .Should().Be(1, "only the printer field caption is filled in at runtime");
    }

    [Fact]
    public void Global_sets_have_no_scope()
    {
        var firmWide = Set("ForaMikro");

        firmWide.Scopes.Should().BeEmpty();
        firmWide.ScopeKind.Should().Be(ScopeKinds.None);
    }

    [Fact]
    public void Only_one_set_reuses_a_parameter_id_and_the_collisions_are_recorded()
    {
        var catalog = Extract();

        // Fora's own defect, not ours: TahsilatAktarimTxtCsvSablon gives the year, month and
        // day fields one shared id where GenelAktarimTxtCsvSablon correctly uses 16, 18 and 20.
        // _GetParametre(int) returns the first match, so the later entries are unreachable in
        // Fora too. The catalogue keeps them and flags them rather than hiding the problem.
        catalog.Sets.Where(s => s.DuplicateIds.Count > 0)
            .Select(s => s.CatalogMethod)
            .Should().Equal("TahsilatAktarimTxtCsvSablon");

        catalog.ShadowedCount.Should().Be(5);

        var broken = Set("TahsilatAktarimTxtCsvSablon");
        broken.DuplicateIds.Should().Equal(16, 17, 604);
        broken.Parameters.Where(p => p.Shadowed).Select(p => p.Name).Should().Equal(
            "belge_tarihi_ay_baslangic",
            "belge_tarihi_ay_uzunluk",
            "belge_tarihi_gun_baslangic",
            "belge_tarihi_gun_uzunluk",
            "cari_kod2_uzunluk");

        foreach (var set in catalog.Sets.Where(s => s.CatalogMethod != "TahsilatAktarimTxtCsvSablon"))
        {
            set.Parameters.Select(p => p.Id).Should().OnlyHaveUniqueItems(
                "{0} addresses its parameters by ParametreID", set.CatalogMethod);
            set.Parameters.Should().OnlyContain(p => !p.Shadowed);
        }
    }

    [Fact]
    public void Names_declared_twice_are_recorded_too()
    {
        var catalog = Extract();

        // _GetParametre(string) also returns the first match, so a name declared under two ids
        // leaves the second copy unreachable: the editor binds the first and the second sits at
        // its default forever. This is what makes 1,801 akilli entries only 1,796 usable names.
        catalog.Sets.Where(s => s.DuplicateNames.Count > 0).Select(s => s.CatalogMethod)
            .Should().Equal("MobilKullanici");

        var akilli = Extract().Sets.Single(s => s.CatalogMethod == "MobilKullanici");

        akilli.DuplicateNames.Should().Equal(
            "YazdirmaAlinanSiparisBluetoothAygitIsmi",
            "YazdirmaAlinanSiparisSablonAdi",
            "YazdirmaAlinanSiparisSadeceAktarilanlariYazdirabilir",
            "YazdirmaAlinanSiparisSatirlarArasiBeklemeSuresi",
            "YazdirmaAlinanSiparisSayfaSatirSayisi");

        akilli.Parameters.Select(p => p.Name).Distinct(StringComparer.Ordinal).Should().HaveCount(1796);

        // Both copies carry the same default, so the unreachable one is inert rather than wrong.
        foreach (var name in akilli.DuplicateNames)
        {
            akilli.Parameters.Where(p => p.Name == name).Select(p => p.Default)
                .Distinct(StringComparer.Ordinal).Should().ContainSingle();
        }
    }

    [Fact]
    public void The_first_entry_of_a_colliding_id_is_the_one_that_wins()
    {
        // Mirrors Parametreler._GetParametre(int): first match wins.
        var sixteens = Set("TahsilatAktarimTxtCsvSablon").Parameters.Where(p => p.Id == 16).ToList();

        sixteens.Should().HaveCount(3);
        sixteens[0].Shadowed.Should().BeFalse();
        sixteens[0].Name.Should().Be("belge_tarihi_yil_baslangic");
        sixteens.Skip(1).Should().OnlyContain(p => p.Shadowed);
    }

    [Fact]
    public void Escaped_quotes_in_label_templates_survive_extraction()
    {
        // These four ZPL templates end in an escaped quote. A line-based regex keeps the
        // backslash or truncates the value; Roslyn hands back the decoded literal.
        var template = Set("MobilKullanici").Parameters
            .Single(p => p.Name == "KoliEtiketiStokListesiBaslangicMetniStokKodu");

        template.Default.Should().Be("A[degisken],105,1,3,2,1,N,\"");
        template.Default.Should().NotContain("\\");
    }

    [Fact]
    public void Turkish_defaults_are_preserved()
    {
        Set("MobilKullanici").Parameters.Single(p => p.Name == "MetinZyrt_Satis_Yapildi")
            .Default.Should().Be("Satış yapıldı");
    }

    [Fact]
    public void Every_set_has_a_program_a_source_file_and_a_known_scope_kind()
    {
        string[] knownKinds =
        [
            ScopeKinds.None, ScopeKinds.MobileUser, ScopeKinds.DesktopUser, ScopeKinds.ReportCode,
            ScopeKinds.ImportTemplate, ScopeKinds.CriteriaName, ScopeKinds.EdiRelation, ScopeKinds.PrinterTemplate,
        ];

        string[] knownFields = [ScopeFields.User, ScopeFields.AnaGrubu, ScopeFields.AltGrubu];

        foreach (var set in Extract().Sets)
        {
            set.Program.Should().NotBeNullOrWhiteSpace();
            set.SourceFile.Should().NotBeNullOrWhiteSpace();
            set.ScopeKind.Should().BeOneOf(knownKinds);

            // A global set has no scope columns at all, so assert over the items rather than
            // the collection (OnlyContain treats an empty collection as a failure).
            set.Scopes.Select(s => s.Field).Should().BeSubsetOf(knownFields).And.OnlyHaveUniqueItems();
            set.Scopes.Where(s => string.IsNullOrWhiteSpace(s.Source)).Should().BeEmpty();
        }
    }

    [Fact]
    public void A_set_whose_addressing_columns_disagree_is_rejected()
    {
        const string source = """
            namespace Fora.Mikro.ParametreTanimlari;
            public static class ParametrelerDefault
            {
                public static Parametreler MobilKullanici(string User)
                {
                    return new Parametreler
                    {
                        ParametreListesi =
                        {
                            new Parametre("akilli", User, "", "", 1, "Bir", ""),
                            new Parametre("akilli", User, "Baska", "", 2, "Iki", "")
                        }
                    };
                }
            }
            """;

        var extract = () => DefaultsExtractor.Extract([Snippet(source)], "unknown");

        extract.Should().Throw<CatalogExtractionException>().WithMessage("*anaGrubu*not uniform*");
    }

    [Fact]
    public void An_unmapped_factory_method_is_rejected_rather_than_guessed()
    {
        const string source = """
            namespace Fora.Mikro.ParametreTanimlari;
            public static class ParametrelerDefault
            {
                public static Parametreler YeniBirSet(string Kod)
                {
                    return new Parametreler
                    {
                        ParametreListesi =
                        {
                            new Parametre("yeni", Kod, "", "", 1, "Bir", "")
                        }
                    };
                }
            }
            """;

        var extract = () => DefaultsExtractor.Extract([Snippet(source)], "unknown");

        extract.Should().Throw<CatalogExtractionException>().WithMessage("*no scope kind mapped*");
    }

    [Fact]
    public void A_source_that_declares_nothing_is_rejected()
    {
        var extract = () => DefaultsExtractor.Extract(
            [Snippet("public static class ParametrelerDefault { }")], "unknown");

        extract.Should().Throw<CatalogExtractionException>().WithMessage("*declares no Parametre entries*");
    }
}
