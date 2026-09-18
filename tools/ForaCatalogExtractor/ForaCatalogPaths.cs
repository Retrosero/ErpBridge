namespace ErpBridge.Tools.ForaCatalog;

/// <summary>
/// Repository-relative locations the extractor reads from and writes to. Both the
/// console entry point and the golden test resolve paths through here so they can
/// never drift apart.
/// </summary>
public static class ForaCatalogPaths
{
    /// <summary>Decompiled Fora source that declares the default catalogue.</summary>
    public const string DefaultsSource =
        "Fora_Mikro/.decompiled/Core/Fora.Mikro.ParametreTanimlari/ParametrelerDefault.cs";

    /// <summary>Generated catalogue, committed to the repository.</summary>
    public const string DefaultsOutput = "catalog/parameters/defaults.json";

    /// <summary>Printer templates: declared imperatively, not in <c>ParametrelerDefault</c>.</summary>
    public const string PrinterDefaultsSource =
        "Fora_Mikro/.decompiled/Core/Fora.Mikro.Yazdirma/YaziciAyarlari.cs";

    /// <summary>Type declared in <see cref="PrinterDefaultsSource"/>.</summary>
    public const string PrinterDefaultsType = "YaziciAyarlari";

    /// <summary>Type declared in <see cref="DefaultsSource"/>.</summary>
    public const string DefaultsType = "ParametrelerDefault";

    /// <summary>
    /// One Fora editor screen and the catalogue sets it maintains. The mapping is ours: a form
    /// cannot be matched to a set by parameter names alone, because 862 of the 3,365 distinct
    /// names appear in more than one set (all four <c>MobilRapor*</c> screens share the same two
    /// names, and the two <c>KriterDuzenleme</c> screens share all seven).
    /// </summary>
    /// <param name="File">Repository-relative path of the decompiled form.</param>
    /// <param name="TypeName">Class that declares the form.</param>
    /// <param name="CatalogMethods">Catalogue sets this form is the editor for.</param>
    /// <param name="Output">Repository-relative path of the generated layout.</param>
    /// <param name="AlsoEdits">
    /// Sets the form touches without owning. Their parameters are attributed correctly, but the
    /// rest of those sets is not reported as missing from this screen.
    /// </param>
    public sealed record UiSource(
        string File,
        string TypeName,
        IReadOnlyList<string> CatalogMethods,
        string Output,
        IReadOnlyList<string>? AlsoEdits = null);

    private const string ExeRoot = "Fora_Mikro/.decompiled/Exe/";

    /// <summary>
    /// The editors extracted today. The bulk-import screens (<c>GenelAktarim</c>,
    /// <c>TahsilatAktarim</c>, <c>BankaAktarim</c> — roughly 3,500 fields) are left to P6, where
    /// their panel screens are actually built; their defaults are already in the catalogue.
    /// </summary>
    public static readonly IReadOnlyList<UiSource> UiSources =
    [
        new($"{ExeRoot}Fora.App.Win.Mikro.ForaAndroid/ForaAndroidKullaniciDuzenleme.cs",
            "ForaAndroidKullaniciDuzenleme", ["MobilKullanici"], "catalog/parameters/ui/akilli.json"),

        new($"{ExeRoot}Fora.App.Win.Mikro.Ayarlar/GenelParametrelerForm.cs",
            "GenelParametrelerForm", ["ForaMikro"], "catalog/parameters/ui/foramikro-genel.json"),

        new($"{ExeRoot}Fora.App.Win.Mikro.Ayarlar/KullaniciDuzenleme.cs",
            "KullaniciDuzenleme", ["ForaMikroKullanici"], "catalog/parameters/ui/foramikro-kullanici.json"),

        new($"{ExeRoot}Fora.App.Win.Mikro.Ayarlar/YaziciAyarlariForm.cs",
            "YaziciAyarlariForm", ["genelayarlaritanimla", "alanekle"],
            "catalog/parameters/ui/yaziciayarlari.json"),

        new($"{ExeRoot}Fora.App.Win.Mikro.B2B/B2BPArametreleriForm.cs",
            "B2BPArametreleriForm", ["B2B"], "catalog/parameters/ui/b2b.json"),

        new($"{ExeRoot}Fora.App.Win.Mikro.Aktarimlar.ComarchEdi/ComarchEdiGenelParametrelerForm.cs",
            "ComarchEdiGenelParametrelerForm", ["ComarchEdiGenelParametreler"],
            "catalog/parameters/ui/comarchedi-genel.json"),

        // This screen owns the relation's seven parameters and additionally shows three
        // connection settings that belong to the general EDI set, which has its own editor.
        new($"{ExeRoot}Fora.App.Win.Mikro.Aktarimlar.ComarchEdi/ComarchEdiIliskiYonetimi.cs",
            "ComarchEdiIliskiYonetimi", ["ComarchEdiIliskiParametreleri"],
            "catalog/parameters/ui/comarchedi-iliski.json",
            AlsoEdits: ["ComarchEdiGenelParametreler"]),

        new($"{ExeRoot}Fora.App.Win.Mikro.ForaAndroid/MobilRaporStokSatisDuzenleme.cs",
            "MobilRaporStokSatisDuzenleme", ["MobilRaporStokSatis"],
            "catalog/parameters/ui/mobilrapor-stoksatis.json"),

        new($"{ExeRoot}Fora.App.Win.Mikro.ForaAndroid/MobilRaporStokEnvanterDuzenleme.cs",
            "MobilRaporStokEnvanterDuzenleme", ["MobilRaporStokEnvanter"],
            "catalog/parameters/ui/mobilrapor-stokenvanter.json"),

        new($"{ExeRoot}Fora.App.Win.Mikro.ForaAndroid/MobilRaporStokSiparisDuzenleme.cs",
            "MobilRaporStokSiparisDuzenleme", ["MobilRaporStokSiparis"],
            "catalog/parameters/ui/mobilrapor-stoksiparis.json"),

        new($"{ExeRoot}Fora.App.Win.Mikro.ForaAndroid/MobilRaporYapilacakTahsilatlarDuzenleme.cs",
            "MobilRaporYapilacakTahsilatlarDuzenleme", ["MobilRaporYapilacakTahsilatlar"],
            "catalog/parameters/ui/mobilrapor-yapilacaktahsilatlar.json"),
    ];

    /// <summary>
    /// Walks up from <paramref name="start"/> until it finds the repository root.
    /// Tests run from the build output directory, so they cannot assume the working directory.
    /// </summary>
    public static string FindRepositoryRoot(string start)
    {
        var directory = new DirectoryInfo(Path.GetFullPath(start));

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ErpBridge.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"No ErpBridge.sln found at or above '{start}'; cannot locate the repository root.");
    }

    public static string Resolve(string repositoryRoot, string relativePath) =>
        Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
}
