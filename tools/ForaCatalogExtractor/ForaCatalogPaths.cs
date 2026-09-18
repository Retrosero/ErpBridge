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

    /// <summary>Decompiled Fora editor for mobile-user (<c>akilli</c>) parameters.</summary>
    public const string AkilliUiSource =
        "Fora_Mikro/.decompiled/Exe/Fora.App.Win.Mikro.ForaAndroid/ForaAndroidKullaniciDuzenleme.cs";

    /// <summary><c>ParametreProgram</c> value the mobile-user editor maintains.</summary>
    public const string AkilliProgram = "akilli";

    /// <summary>Type declared in <see cref="AkilliUiSource"/>.</summary>
    public const string AkilliUiType = "ForaAndroidKullaniciDuzenleme";

    /// <summary>Generated tab, label and editor layout for the <c>akilli</c> parameters.</summary>
    public const string AkilliUiOutput = "catalog/parameters/ui.akilli.json";

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
