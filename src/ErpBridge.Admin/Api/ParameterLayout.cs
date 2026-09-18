using System.Globalization;
using System.Text.RegularExpressions;

namespace ErpBridge.Admin.Api;

/// <summary>
/// Turns a flat list of parameters into the shape Fora's own editor shows them in: a tab tree, and
/// inside a tab, groups (P2b).
///
/// The tree is derived from the catalogue rather than written out, because 63 tabs would otherwise
/// be 63 files to keep in step with a catalogue that is regenerated from Fora's sources (D11).
/// </summary>
public static partial class ParameterLayout
{
    /// <summary>Where parameters with no editor in Fora are collected, rather than hidden.</summary>
    public const string NoTabTitle = "Fora'da ekranı olmayanlar";

    /// <summary>How many members a repeating name pattern needs before it becomes one group.</summary>
    private const int RepeatThreshold = 3;

    /// <summary>One node of the tab tree.</summary>
    /// <param name="Title">The tab's own title.</param>
    /// <param name="Path">Full path from the root, joined with " / " — the selection key.</param>
    /// <param name="DirectCount">Parameters that sit on this tab itself.</param>
    /// <param name="TotalCount">Including every descendant tab.</param>
    public sealed record TabNode(
        string Title,
        string Path,
        IReadOnlyList<TabNode> Children,
        int DirectCount,
        int TotalCount)
    {
        /// <summary>Parameters on this tab that are not at their default.</summary>
        public int OverriddenCount { get; init; }
    }

    /// <summary>
    /// A block of fields inside one tab. Most are a single field; a repeating family is one group
    /// with many members.
    /// </summary>
    /// <param name="Title">What to show as the block's heading.</param>
    /// <param name="Pattern">The masked name for a repeating family, else null.</param>
    /// <param name="Items">The fields, in Fora's own reading order.</param>
    public sealed record FieldGroup(
        string Title,
        string? Pattern,
        IReadOnlyList<ParameterValueDto> Items)
    {
        /// <summary>True when this is a family that repeats by index, not a single field.</summary>
        public bool IsRepeating => Pattern is not null;
    }

    /// <summary>
    /// The tab tree, in the order the parameters arrive — which is Fora's reading order, because
    /// the server sorts by the editor order it extracted.
    /// </summary>
    public static IReadOnlyList<TabNode> BuildTabs(IEnumerable<ParameterValueDto> values)
    {
        var root = new Builder("", "");

        foreach (var value in values)
        {
            var segments = Split(value.TabPath);
            var node = root;

            foreach (var segment in segments)
            {
                node = node.Child(segment);
            }

            node.Direct++;
            if (value.IsOverridden)
            {
                node.Overridden++;
            }
        }

        return root.Children.Select(c => c.ToNode()).ToList();
    }

    /// <summary>The parameters that sit on exactly the given tab path.</summary>
    public static IReadOnlyList<ParameterValueDto> FieldsOf(
        IEnumerable<ParameterValueDto> values, string tabPath) =>
        values.Where(v => string.Join(" / ", Split(v.TabPath)) == tabPath).ToList();

    /// <summary>
    /// Groups one tab's fields so a big tab stays readable.
    ///
    /// Worth doing rather than listing everything: 830 of <c>akilli</c>'s 1,782 fields sit on the
    /// survey tab, and 800 of those are four question kinds repeated over 100 slots. Drawn as a
    /// flat list that tab is unusable, and the repetition is in the names, so the grouping can be
    /// derived instead of hand-written (P0f).
    /// </summary>
    public static IReadOnlyList<FieldGroup> GroupFields(IEnumerable<ParameterValueDto> fields)
    {
        var ordered = fields.ToList();

        var families = ordered
            .GroupBy(f => Mask(f.Name), StringComparer.Ordinal)
            .Where(g => g.Count() >= RepeatThreshold)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var groups = new List<FieldGroup>();
        var placed = new HashSet<string>(StringComparer.Ordinal);

        foreach (var field in ordered)
        {
            var pattern = Mask(field.Name);

            if (!families.TryGetValue(pattern, out var members))
            {
                groups.Add(new FieldGroup(field.Label ?? field.Name, null, [field]));
                continue;
            }

            // The family takes the place of its first member, so the tab keeps Fora's order.
            if (placed.Add(pattern))
            {
                groups.Add(new FieldGroup(FamilyTitle(pattern, members), pattern, members));
            }
        }

        return groups;
    }

    /// <summary>
    /// The family's heading. The masked name is unreadable on its own, so the first member's
    /// label carries the meaning and the count says how many there are.
    /// </summary>
    private static string FamilyTitle(string pattern, IReadOnlyList<ParameterValueDto> members)
    {
        var label = members.Select(m => m.Label).FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
        var name = label is null ? pattern : $"{label} ({pattern})";
        return $"{name} — {members.Count} adet";
    }

    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>
    /// A moment, written the way this panel's readers expect it.
    ///
    /// Formatted with an explicit culture rather than the ambient one: the server's culture is not
    /// something the panel controls, and a Turkish screen that prints "Sep" on one host and "Eyl"
    /// on another is a screen nobody can screenshot for support.
    /// </summary>
    public static string Moment(DateTimeOffset at) =>
        at.UtcDateTime.ToString("dd MMM yyyy, HH:mm", TurkishCulture);

    /// <summary>Replaces each run of digits with <c>#</c>, so indexed siblings share one pattern.</summary>
    internal static string Mask(string name) => DigitRun().Replace(name, "#");

    private static string[] Split(string? tabPath) =>
        string.IsNullOrWhiteSpace(tabPath)
            ? [NoTabTitle]
            : tabPath.Split(" / ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    [GeneratedRegex(@"\d+")]
    private static partial Regex DigitRun();

    /// <summary>Mutable tree node used while counting; order of first appearance is kept.</summary>
    private sealed class Builder(string title, string path)
    {
        private readonly Dictionary<string, Builder> _byTitle = new(StringComparer.Ordinal);

        public List<Builder> Children { get; } = [];

        public int Direct { get; set; }

        public int Overridden { get; set; }

        public Builder Child(string segment)
        {
            if (_byTitle.TryGetValue(segment, out var existing))
            {
                return existing;
            }

            var child = new Builder(segment, path.Length == 0 ? segment : $"{path} / {segment}");
            _byTitle[segment] = child;
            Children.Add(child);
            return child;
        }

        public TabNode ToNode()
        {
            var children = Children.Select(c => c.ToNode()).ToList();

            return new TabNode(
                title,
                path,
                children,
                Direct,
                Direct + children.Sum(c => c.TotalCount))
            {
                OverriddenCount = Overridden + children.Sum(c => c.OverriddenCount),
            };
        }
    }
}
