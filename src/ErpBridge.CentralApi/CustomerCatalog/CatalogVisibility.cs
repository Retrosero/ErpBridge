using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// Which products one customer sees (<c>catalog_accounts.VisibilityJson</c>, GOAL_MUSTERI_KATALOGU §4): the main
/// catalog (<see cref="ModeAll"/>) or only what the rules allow (<see cref="ModeOnly"/>), with per-category and
/// per-product rules over the company's hidden flags. The screens map onto it: "ana katalog" = all + no rules,
/// "seçilenler hariç" = all + deny rules, "yalnız seçilenler" = only + allow rules, "bu cariye göster" = an allow rule.
/// </summary>
public sealed class CatalogVisibility
{
    public const string ModeAll = "all";
    public const string ModeOnly = "only";
    public const string TypeCategory = "category";
    public const string TypeProduct = "product";
    public const string Allow = "allow";
    public const string Deny = "deny";
    public const int MaxRules = 2000;
    public const int MaxCategoryKeyLength = 160;
    public const int MaxStockCodeLength = 64;

    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    /// <summary>The main catalog for everyone.</summary>
    public static readonly CatalogVisibility Default = new(ModeAll, []);

    private static readonly CatalogVisibility Nothing = new(ModeOnly, []);

    private readonly Dictionary<string, bool> _products = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _categories = new(StringComparer.Ordinal);

    private CatalogVisibility(string mode, IReadOnlyList<Rule> rules)
    {
        Mode = mode;
        Rules = rules;
        // A repeated key keeps its last rule.
        foreach (var rule in rules)
            (rule.Type == TypeProduct ? _products : _categories)[rule.Key] = rule.Effect == Allow;
    }

    public sealed record Rule(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("effect")] string Effect);

    private sealed record Stored(
        [property: JsonPropertyName("mode")] string? Mode,
        [property: JsonPropertyName("rules")] Rule[]? Rules);

    public string Mode { get; }

    public IReadOnlyList<Rule> Rules { get; }

    /// <summary>
    /// Whether the customer sees a product, first match wins: the product's own rule; the company hid it; its
    /// category's rule; "only" shows nothing else; otherwise unless the company hid its category. (Whether it has a
    /// price in the customer's list is decided elsewhere.)
    /// </summary>
    public bool IsVisible(string stockCode, bool productHidden, string categoryKey, bool categoryHidden)
    {
        if (_products.TryGetValue(stockCode, out var productAllowed)) return productAllowed;
        if (productHidden) return false;
        if (_categories.TryGetValue(categoryKey, out var categoryAllowed)) return categoryAllowed;
        if (Mode == ModeOnly) return false;
        return !categoryHidden;
    }

    /// <summary>
    /// Mode and rules as sent by staff, trimmed and lower-cased; null with a reason when something is not allowed.
    /// A null <paramref name="mode"/> is "all".
    /// </summary>
    public static CatalogVisibility? Create(string? mode, IEnumerable<(string? Type, string? Key, string? Effect)>? rules, out string? error)
    {
        error = null;
        var normalizedMode = string.IsNullOrWhiteSpace(mode) ? ModeAll : mode.Trim().ToLowerInvariant();
        if (normalizedMode is not (ModeAll or ModeOnly))
        {
            error = "visibility.mode must be 'all' or 'only'.";
            return null;
        }
        var list = new List<Rule>();
        foreach (var (type, key, effect) in rules ?? [])
        {
            if (list.Count == MaxRules)
            {
                error = $"visibility.rules may hold at most {MaxRules} rules.";
                return null;
            }
            var normalizedType = type?.Trim().ToLowerInvariant();
            var normalizedEffect = effect?.Trim().ToLowerInvariant();
            var normalizedKey = key?.Trim();
            if (normalizedType is not (TypeCategory or TypeProduct) || normalizedEffect is not (Allow or Deny))
            {
                error = "A visibility rule's type must be 'category' or 'product' and its effect 'allow' or 'deny'.";
                return null;
            }
            var maxLength = normalizedType == TypeProduct ? MaxStockCodeLength : MaxCategoryKeyLength;
            if (string.IsNullOrEmpty(normalizedKey) || normalizedKey.Length > maxLength)
            {
                error = $"A visibility rule's key is required and at most {maxLength} characters.";
                return null;
            }
            list.Add(new Rule(normalizedType, normalizedKey, normalizedEffect));
        }
        return new CatalogVisibility(normalizedMode, list);
    }

    /// <summary>
    /// The stored form. It is written only by <see cref="ToJson"/>; should a row ever be unreadable, the customer sees
    /// nothing rather than more than they were given.
    /// </summary>
    public static CatalogVisibility Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Default;
        try
        {
            var stored = JsonSerializer.Deserialize<Stored>(json, Web);
            return Create(stored?.Mode, stored?.Rules?.Select(r => ((string?)r.Type, (string?)r.Key, (string?)r.Effect)), out _) ?? Nothing;
        }
        catch (JsonException)
        {
            return Nothing;
        }
    }

    public string ToJson() => JsonSerializer.Serialize(new Stored(Mode, [.. Rules]), Web);
}
