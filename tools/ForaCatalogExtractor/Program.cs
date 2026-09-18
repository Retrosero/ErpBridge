using ErpBridge.Tools.ForaCatalog;

// Regenerates catalog/parameters/*.json from the decompiled Fora sources.
//
//   dotnet run --project tools/ForaCatalogExtractor
//   dotnet run --project tools/ForaCatalogExtractor -- --check
//
// --check writes nothing and fails when the committed catalogue is stale, which is what
// CI and the golden test rely on.

var check = args.Contains("--check", StringComparer.OrdinalIgnoreCase);
var buildArgument = args.SkipWhile(a => !string.Equals(a, "--source-build", StringComparison.OrdinalIgnoreCase))
    .Skip(1)
    .FirstOrDefault();

try
{
    var root = ForaCatalogPaths.FindRepositoryRoot(Directory.GetCurrentDirectory());
    var sourcePath = ForaCatalogPaths.Resolve(root, ForaCatalogPaths.DefaultsSource);
    var outputPath = ForaCatalogPaths.Resolve(root, ForaCatalogPaths.DefaultsOutput);

    if (!File.Exists(sourcePath))
    {
        await Console.Error.WriteLineAsync($"Decompiled source not found: {ForaCatalogPaths.DefaultsSource}");
        return 2;
    }

    // Keep whatever build identifier the committed file already carries unless the caller
    // passes a new one, so regenerating does not silently reset it to "unknown".
    var sourceBuild = buildArgument ?? ExistingSourceBuild(outputPath) ?? "unknown";

    var catalog = DefaultsExtractor.Extract(
        await File.ReadAllTextAsync(sourcePath),
        ForaCatalogPaths.DefaultsSource,
        sourceBuild);

    var json = CatalogJson.Serialize(catalog);

    if (check)
    {
        if (!File.Exists(outputPath))
        {
            await Console.Error.WriteLineAsync($"{ForaCatalogPaths.DefaultsOutput} is missing. Run the extractor without --check.");
            return 1;
        }

        var committed = await File.ReadAllTextAsync(outputPath);
        if (!string.Equals(Normalize(committed), json, StringComparison.Ordinal))
        {
            await Console.Error.WriteLineAsync(
                $"{ForaCatalogPaths.DefaultsOutput} is out of date. Run: dotnet run --project tools/ForaCatalogExtractor");
            return 1;
        }

        Console.WriteLine($"{ForaCatalogPaths.DefaultsOutput} is up to date ({catalog.SetCount} sets, {catalog.ParameterCount} parameters).");
        return 0;
    }

    await File.WriteAllTextAsync(outputPath, json, CatalogJson.FileEncoding);
    Console.WriteLine($"Wrote {ForaCatalogPaths.DefaultsOutput}: {catalog.SetCount} sets, {catalog.ParameterCount} parameters, build '{sourceBuild}'.");
    return 0;
}
catch (CatalogExtractionException ex)
{
    await Console.Error.WriteLineAsync($"Extraction failed: {ex.Message}");
    return 2;
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
