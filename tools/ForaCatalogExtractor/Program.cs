using ErpBridge.Tools.ForaCatalog;

// Regenerates catalog/parameters/*.json from the decompiled Fora sources.
//
//   dotnet run --project tools/ForaCatalogExtractor
//   dotnet run --project tools/ForaCatalogExtractor -- --check
//
// --check writes nothing and fails when a committed catalogue is stale, which is what
// CI and the golden tests rely on.

var check = args.Contains("--check", StringComparer.OrdinalIgnoreCase);
var buildArgument = args.SkipWhile(a => !string.Equals(a, "--source-build", StringComparison.OrdinalIgnoreCase))
    .Skip(1)
    .FirstOrDefault();

try
{
    var root = ForaCatalogPaths.FindRepositoryRoot(Directory.GetCurrentDirectory());

    // Keep whatever build identifier the committed file already carries unless the caller passes
    // a new one, so regenerating does not silently reset it to "unknown".
    var defaultsOutput = ForaCatalogPaths.Resolve(root, ForaCatalogPaths.DefaultsOutput);
    var sourceBuild = buildArgument ?? ExistingSourceBuild(defaultsOutput) ?? "unknown";

    var defaults = DefaultsExtractor.Extract(
        await ReadSourceAsync(root, ForaCatalogPaths.DefaultsSource),
        ForaCatalogPaths.DefaultsSource,
        sourceBuild);

    // The layout is checked against what the defaults declare, so a parameter that exists in the
    // catalogue but not in the editor is reported instead of quietly missing from the panel.
    var akilliDefaults = defaults.Sets
        .Where(s => s.Program == ForaCatalogPaths.AkilliProgram)
        .SelectMany(s => s.Parameters)
        .Select(p => p.Name)
        .ToHashSet(StringComparer.Ordinal);

    var ui = UiExtractor.Extract(
        await ReadSourceAsync(root, ForaCatalogPaths.AkilliUiSource),
        ForaCatalogPaths.AkilliUiSource,
        ForaCatalogPaths.AkilliUiType,
        sourceBuild,
        akilliDefaults);

    var outputs = new (string Relative, string Json, string Summary)[]
    {
        (ForaCatalogPaths.DefaultsOutput, CatalogJson.Serialize(defaults),
            $"{defaults.SetCount} sets, {defaults.ParameterCount} parameters, {defaults.ShadowedCount} shadowed"),
        (ForaCatalogPaths.AkilliUiOutput, CatalogJson.Serialize(ui),
            $"{ui.TabCount} tabs, {ui.ParameterCount} parameters, {ui.UnlabelledCount} unlabelled, {ui.Gaps.Count} gaps"),
    };

    var stale = 0;

    foreach (var (relative, json, summary) in outputs)
    {
        var path = ForaCatalogPaths.Resolve(root, relative);

        if (!check)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(path, json, CatalogJson.FileEncoding);
            Console.WriteLine($"Wrote {relative}: {summary}.");
            continue;
        }

        if (!File.Exists(path))
        {
            await Console.Error.WriteLineAsync($"{relative} is missing.");
            stale++;
            continue;
        }

        if (!string.Equals(Normalize(await File.ReadAllTextAsync(path)), json, StringComparison.Ordinal))
        {
            await Console.Error.WriteLineAsync($"{relative} is out of date.");
            stale++;
            continue;
        }

        Console.WriteLine($"{relative} is up to date ({summary}).");
    }

    if (stale > 0)
    {
        await Console.Error.WriteLineAsync(
            $"{stale} catalogue file(s) stale. Run: dotnet run --project tools/ForaCatalogExtractor");
        return 1;
    }

    return 0;
}
catch (CatalogExtractionException ex)
{
    await Console.Error.WriteLineAsync($"Extraction failed: {ex.Message}");
    return 2;
}
catch (FileNotFoundException ex)
{
    await Console.Error.WriteLineAsync(ex.Message);
    return 2;
}

static async Task<string> ReadSourceAsync(string root, string relative)
{
    var path = ForaCatalogPaths.Resolve(root, relative);

    if (!File.Exists(path))
    {
        throw new FileNotFoundException($"Decompiled source not found: {relative}");
    }

    return await File.ReadAllTextAsync(path);
}

static string Normalize(string text) =>
    text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n') + "\n";

static string? ExistingSourceBuild(string outputPath)
{
    if (!File.Exists(outputPath))
    {
        return null;
    }

    try
    {
        return CatalogJson.Deserialize<DefaultsCatalog>(File.ReadAllText(outputPath)).SourceBuild;
    }
    catch (Exception ex) when (ex is System.Text.Json.JsonException or InvalidOperationException)
    {
        // A malformed committed file is not a reason to refuse to regenerate it.
        return null;
    }
}
