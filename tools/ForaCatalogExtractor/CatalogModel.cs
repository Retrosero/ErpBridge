using System.Text.Json.Serialization;

namespace ErpBridge.Tools.ForaCatalog;

/// <summary>
/// One parameter definition as Fora declares it: a stable numeric id, a name kept
/// only for readability, and the default value the application falls back to when
/// the database holds no row for it.
/// </summary>
public sealed record ParameterDefault
{
    [JsonPropertyOrder(1)] public required int Id { get; init; }
    [JsonPropertyOrder(2)] public required string Name { get; init; }
    [JsonPropertyOrder(3)] public required string Default { get; init; }

    /// <summary>
    /// True when an earlier entry in the same set already claimed this <see cref="Id"/>.
    /// Fora's <c>Parametreler._GetParametre(int)</c> returns the first match, so a shadowed
    /// entry can never be read back — it is a defect in Fora's own catalogue, recorded here
    /// so the collision stays visible instead of being silently dropped.
    /// </summary>
    [JsonPropertyOrder(4)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Shadowed { get; init; }
}

/// <summary>
/// Which of the three Fora scope columns carries the caller-supplied scope value.
/// Fora is not consistent about this: <c>akilli</c> puts it in <c>ParametreUser</c>,
/// the import templates leave that column empty and use <c>ParametreAltGrubu</c>
/// instead, with <c>ParametreAnaGrubu</c> acting as a fixed discriminator.
/// </summary>
public static class ScopeFields
{
    public const string None = "none";
    public const string User = "user";
    public const string AnaGrubu = "anaGrubu";
    public const string AltGrubu = "altGrubu";
}

/// <summary>What the scope value means, so the panel can offer the right picker.</summary>
public static class ScopeKinds
{
    public const string None = "None";
    public const string MobileUser = "MobileUser";
    public const string DesktopUser = "DesktopUser";
    public const string ReportCode = "ReportCode";
    public const string ImportTemplate = "ImportTemplate";
    public const string CriteriaName = "CriteriaName";
    public const string EdiRelation = "EdiRelation";
    public const string PrinterTemplate = "PrinterTemplate";
}

/// <summary>
/// One <c>ParametrelerDefault</c> factory method: the whole parameter set a single
/// scope instance owns (one mobile user, one import template, one report code…).
/// </summary>
public sealed record CatalogSet
{
    /// <summary>The C# method that declares this set, e.g. <c>MobilKullanici</c>.</summary>
    [JsonPropertyOrder(1)] public required string CatalogMethod { get; init; }

    /// <summary>Value written to <c>ParametreProgram</c>, e.g. <c>akilli</c>.</summary>
    [JsonPropertyOrder(2)] public required string Program { get; init; }

    /// <summary>One of <see cref="ScopeKinds"/>.</summary>
    [JsonPropertyOrder(3)] public required string ScopeKind { get; init; }

    /// <summary>One of <see cref="ScopeFields"/>.</summary>
    [JsonPropertyOrder(4)] public required string ScopeField { get; init; }

    /// <summary>Name of the method parameter that supplies the scope, or null when the set is global.</summary>
    [JsonPropertyOrder(5)] public string? ScopeParameter { get; init; }

    /// <summary>Constant written to <c>ParametreUser</c> (empty when this column carries the scope).</summary>
    [JsonPropertyOrder(6)] public required string User { get; init; }

    /// <summary>Constant written to <c>ParametreAnaGrubu</c>.</summary>
    [JsonPropertyOrder(7)] public required string AnaGrubu { get; init; }

    /// <summary>Constant written to <c>ParametreAltGrubu</c>.</summary>
    [JsonPropertyOrder(8)] public required string AltGrubu { get; init; }

    /// <summary>
    /// Ids declared more than once in this set, ascending. Empty for every healthy set;
    /// see <see cref="ParameterDefault.Shadowed"/> for what a collision costs.
    /// </summary>
    [JsonPropertyOrder(9)] public required IReadOnlyList<int> DuplicateIds { get; init; }

    /// <summary>
    /// Names declared more than once under different ids. Fora's
    /// <c>Parametreler._GetParametre(string)</c> also returns the first match, so the later
    /// copies can never be reached by name — the editor binds only the first and the rest sit
    /// at their defaults forever.
    /// </summary>
    [JsonPropertyOrder(10)] public required IReadOnlyList<string> DuplicateNames { get; init; }

    [JsonPropertyOrder(11)] public required IReadOnlyList<ParameterDefault> Parameters { get; init; }
}

/// <summary>The whole extracted catalogue, written to <c>catalog/parameters/defaults.json</c>.</summary>
public sealed record DefaultsCatalog
{
    [JsonPropertyOrder(1)] public required int SchemaVersion { get; init; }

    /// <summary>
    /// Fora build the catalogue was read from. "unknown" until someone confirms which
    /// version of <c>Fora Mikro.exe</c> was decompiled — see the goal document's open items.
    /// </summary>
    [JsonPropertyOrder(2)] public required string SourceBuild { get; init; }

    [JsonPropertyOrder(3)] public required string SourceType { get; init; }

    /// <summary>Repository-relative path of the decompiled file, with forward slashes.</summary>
    [JsonPropertyOrder(4)] public required string SourceFile { get; init; }

    [JsonPropertyOrder(5)] public required int SetCount { get; init; }

    [JsonPropertyOrder(6)] public required int ParameterCount { get; init; }

    /// <summary>
    /// How many entries Fora's own catalogue makes unreachable by reusing an id
    /// inside one set. Expected to be small and to stay that way; a jump means the
    /// decompiled source changed shape.
    /// </summary>
    [JsonPropertyOrder(7)] public required int ShadowedCount { get; init; }

    [JsonPropertyOrder(8)] public required IReadOnlyList<CatalogSet> Sets { get; init; }
}
