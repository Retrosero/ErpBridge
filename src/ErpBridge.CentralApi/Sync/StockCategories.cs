using System.Text.Json;
using ErpBridge.CentralApi.Endpoints;

namespace ErpBridge.CentralApi.Sync;

/// <summary>
/// The product's category on the phone (<c>kategori</c>): the name of its Mikro stock sub-group.
///
/// <para>The agent sends <c>STOK_ALT_GRUPLARI</c> as <c>lookups</c> rows of kind
/// <see cref="LookupKind"/>. A sub-group code alone is not unique — Mikro keys the table by
/// (main group, sub-group), and MikroDB_V16_03 has <c>10</c> as both "YELEK" and "HAVUZ" under
/// different main groups — so the lookup code is <c>mainGroup|subGroup</c>.</para>
///
/// <para>Resolution: the (main, sub) pair; failing that the sub-group code when it names exactly one
/// sub-group (a few cards carry a main group that does not match their sub-group's); failing that the
/// code itself. Mikro's <c>#YOK</c> ("none") placeholder and blank codes give no category, and the
/// phone keeps its own fallback. Never an empty string: the phone only falls back on null.</para>
/// </summary>
public sealed class StockCategories
{
    /// <summary><c>lookups</c> kind the Mikro reader gives stock sub-groups.</summary>
    public const string LookupKind = "stock_sub_group";

    private readonly Dictionary<string, string> _byPair = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _bySubGroup = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Builds the lookup from <c>lookups</c> rows; other kinds are ignored.</summary>
    public static StockCategories From(IEnumerable<JsonElement> lookups)
    {
        ArgumentNullException.ThrowIfNull(lookups);
        var categories = new StockCategories();
        foreach (var item in lookups)
        {
            if (!string.Equals(AndroidEndpoints.GetString(item, "kind"), LookupKind, StringComparison.OrdinalIgnoreCase))
                continue;
            categories.Add(AndroidEndpoints.GetString(item, "code"), AndroidEndpoints.GetString(item, "name"));
        }
        return categories;
    }

    /// <summary>Adds one sub-group; <paramref name="code"/> is <c>mainGroup|subGroup</c>.</summary>
    public void Add(string? code, string? name)
    {
        var (main, sub) = SplitCode(code);
        var trimmed = name?.Trim();
        if (sub is null || string.IsNullOrEmpty(trimmed)) return;
        _byPair[main + "|" + sub] = trimmed;
        if (!_bySubGroup.TryGetValue(sub, out var names))
        {
            names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _bySubGroup[sub] = names;
        }
        names.Add(trimmed);
    }

    /// <summary>The category for a stock card's group codes, or null for none.</summary>
    public string? Resolve(string? mainGroupCode, string? subGroupCode)
    {
        var sub = subGroupCode?.Trim();
        if (string.IsNullOrEmpty(sub) || IsPlaceholder(sub)) return null;
        var main = mainGroupCode?.Trim() ?? string.Empty;
        if (_byPair.TryGetValue(main + "|" + sub, out var name)) return name;
        if (_bySubGroup.TryGetValue(sub, out var names) && names.Count == 1) return names.First();
        return sub;
    }

    /// <summary>The category of a stock row as uploaded (<c>mainGroupCode</c>/<c>subGroupCode</c>).</summary>
    public string? Resolve(JsonElement stock) =>
        Resolve(AndroidEndpoints.GetFirstString(stock, "mainGroupCode", "sto_anagrup_kod"),
            AndroidEndpoints.GetFirstString(stock, "subGroupCode", "sto_altgrup_kod"));

    /// <summary>
    /// Sets <c>kategori</c> on a product being assembled when a category resolves. A card with no
    /// sub-group (an ERP-less tenant's card carries its own <c>kategori</c>) is left alone.
    /// </summary>
    public void Apply(IDictionary<string, object?> product, JsonElement stock)
    {
        ArgumentNullException.ThrowIfNull(product);
        var category = Resolve(stock);
        if (category is not null) product["kategori"] = category;
    }

    /// <summary>
    /// The sub-group a <c>lookups</c> record key names (<c>stock_sub_group|main|sub</c> → <c>sub</c>),
    /// or null for any other key.
    /// </summary>
    public static string? SubGroupOfLookupKey(string? recordKey)
    {
        const string prefix = LookupKind + "|";
        if (recordKey is null || !recordKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;
        return SplitCode(recordKey[prefix.Length..]).Sub;
    }

    /// <summary>The trimmed sub-group code of a stock row as uploaded, or null.</summary>
    public static string? SubGroupOf(JsonElement stock)
    {
        var sub = AndroidEndpoints.GetFirstString(stock, "subGroupCode", "sto_altgrup_kod")?.Trim();
        return string.IsNullOrEmpty(sub) ? null : sub;
    }

    private static bool IsPlaceholder(string code) => code.StartsWith('#');

    private static (string Main, string? Sub) SplitCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return (string.Empty, null);
        var separator = code.IndexOf('|', StringComparison.Ordinal);
        var main = separator < 0 ? string.Empty : code[..separator].Trim();
        var sub = (separator < 0 ? code : code[(separator + 1)..]).Trim();
        return (main, sub.Length == 0 ? null : sub);
    }
}
