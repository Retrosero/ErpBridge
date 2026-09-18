using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Parameters;

/// <summary>
/// Reads the generated Fora parameter catalogue that ships with the assembly.
///
/// The files under <c>catalog/parameters/</c> are embedded rather than read from disk: the API
/// runs in a container where the repository is not present, and a catalogue that can go missing
/// at run time would leave every stored value uninterpretable.
/// </summary>
public static class ParameterCatalogFile
{
    private const string DefaultsResource = "catalog.parameters.defaults.json";
    private const string LayoutPrefix = "catalog.parameters.ui.";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private static readonly Lazy<IReadOnlyList<CatalogRow>> Cached = new(Read, isThreadSafe: true);

    /// <summary>
    /// Everything the catalogue declares, with each parameter's editor metadata merged in.
    /// Parsed once per process: the files are fixed at build time and weigh about 1.5 MB.
    /// </summary>
    public static IReadOnlyList<CatalogRow> Load() => Cached.Value;

    private static IReadOnlyList<CatalogRow> Read()
    {
        var assembly = typeof(ParameterCatalogFile).Assembly;
        var defaults = Read<DefaultsDocument>(assembly, DefaultsResource);

        // One layout per Fora editor screen. A parameter is identified by its set and id, never by
        // name alone: 862 of the 3,365 distinct names appear in more than one set.
        var layouts = assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(LayoutPrefix, StringComparison.Ordinal))
            .Select(n => Read<LayoutDocument>(assembly, n))
            .SelectMany(d => d.Parameters)
            .ToDictionary(p => (p.CatalogMethod, p.Parameter), StringTupleComparer.Instance);

        var rows = new List<CatalogRow>();

        foreach (var set in defaults.Sets)
        {
            foreach (var parameter in set.Parameters)
            {
                // A shadowed entry is one Fora itself can never read back: an earlier entry in the
                // same set already claimed the id. Storing it would break the (set, id) key and
                // could not correspond to any real value.
                if (parameter.Shadowed)
                {
                    continue;
                }

                // An excluded parameter is one this product refuses to carry — Fora keeps the
                // mobile user's password as an ordinary parameter. Dropping it here means no
                // consumer of the catalogue can surface it by accident (D6).
                if (parameter.Excluded)
                {
                    continue;
                }

                layouts.TryGetValue((set.CatalogMethod, parameter.Name), out var layout);

                rows.Add(new CatalogRow(
                    Program: set.Program,
                    CatalogMethod: set.CatalogMethod,
                    ParametreId: parameter.Id,
                    Name: parameter.Name,
                    DefaultValue: parameter.Default,
                    DefaultSource: parameter.DefaultSource,
                    ScopeKind: set.ScopeKind,
                    ScopeFields: string.Join(',', set.Scopes.Select(s => s.Field)),
                    User: set.User,
                    AnaGrubu: set.AnaGrubu,
                    AltGrubu: set.AltGrubu,
                    Editor: layout?.Editor,
                    ReferenceKind: layout?.ReferenceKind,
                    SecretSource: layout?.SecretSource,
                    Label: layout?.Label,
                    TabPath: layout is null || layout.TabPath.Count == 0
                        ? null
                        : string.Join(" / ", layout.TabPath),
                    EditorOrder: layout?.Order,
                    OptionsJson: layout is null || layout.Options.Count == 0
                        ? null
                        : JsonSerializer.Serialize(layout.Options, Options),
                    SourceBuild: defaults.SourceBuild));
            }
        }

        return rows;
    }

    private static T Read<T>(Assembly assembly, string resource)
    {
        using var stream = assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException(
                $"Parameter catalogue resource '{resource}' is missing from {assembly.GetName().Name}.");

        return JsonSerializer.Deserialize<T>(stream, Options)
            ?? throw new InvalidOperationException($"Parameter catalogue resource '{resource}' is empty.");
    }

    /// <summary>One catalogue parameter, flattened for seeding.</summary>
    public sealed record CatalogRow(
        string Program,
        string CatalogMethod,
        int ParametreId,
        string Name,
        string DefaultValue,
        string? DefaultSource,
        string ScopeKind,
        string ScopeFields,
        string User,
        string AnaGrubu,
        string AltGrubu,
        string? Editor,
        string? ReferenceKind,
        string? SecretSource,
        string? Label,
        string? TabPath,
        int? EditorOrder,
        string? OptionsJson,
        string SourceBuild);

    private sealed record DefaultsDocument(string SourceBuild, IReadOnlyList<DefaultsSet> Sets);

    private sealed record DefaultsSet(
        string CatalogMethod,
        string Program,
        string ScopeKind,
        IReadOnlyList<ScopeColumn> Scopes,
        string User,
        string AnaGrubu,
        string AltGrubu,
        IReadOnlyList<DefaultsParameter> Parameters);

    private sealed record ScopeColumn(string Field, string Source);

    private sealed record DefaultsParameter(
        int Id,
        string Name,
        string Default,
        bool Shadowed,
        bool Excluded,
        string? DefaultSource);

    private sealed record LayoutDocument(IReadOnlyList<LayoutParameter> Parameters);

    private sealed record LayoutParameter(
        string Parameter,
        string CatalogMethod,
        string? Label,
        string Editor,
        IReadOnlyList<string> TabPath,
        int Order,
        string? ReferenceKind,
        string? SecretSource,
        IReadOnlyList<LayoutOption> Options);

    private sealed record LayoutOption(string Value, string Label);

    /// <summary>Ordinal comparison for the (set, name) lookup; parameter names are case-sensitive.</summary>
    private sealed class StringTupleComparer : IEqualityComparer<(string, string)>
    {
        public static readonly StringTupleComparer Instance = new();

        public bool Equals((string, string) x, (string, string) y) =>
            string.Equals(x.Item1, y.Item1, StringComparison.Ordinal)
            && string.Equals(x.Item2, y.Item2, StringComparison.Ordinal);

        public int GetHashCode((string, string) obj) =>
            HashCode.Combine(
                StringComparer.Ordinal.GetHashCode(obj.Item1),
                StringComparer.Ordinal.GetHashCode(obj.Item2));
    }
}
