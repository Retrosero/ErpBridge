using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ErpBridge.CentralApi.Storage;

/// <summary>A product of the feed: its code as written (trimmed) and its picture addresses in the feed's order.</summary>
public sealed record XmlFeedImageItem(string Code, IReadOnlyList<string> Urls);

/// <summary>What the feed held: the products with a code (first of each code), and how many records it had.</summary>
public sealed record XmlFeedImageResult(IReadOnlyList<XmlFeedImageItem> Items, int RecordCount, int SkippedWithoutCode, int DuplicateCodes);

/// <summary>
/// Reads the company's XML feed for codes and pictures (GOAL_DEPOLAMA_R2 S7) by the phone's own rules, so the server
/// and Sipariş Cepte find the same pictures for the same product — a port of Siparis_Cepte <c>XmlFeedScanner.forEachRecord</c>
/// and <c>XmlFeedReader.toItem</c>; change both together.
/// <list type="bullet">
/// <item>The record path is the element path from the root, names joined with <c>/</c> as written (prefix kept:
/// <c>rss/channel/item</c>, <c>g:image_link</c>; namespaces are not resolved).</item>
/// <item>Inside a record a field is the element path relative to the record; an attribute is <c>rel/@name</c>, the
/// record's own attribute <c>@name</c>. An element's value is its own direct text, trimmed; a wrapper whose children
/// were fields does not count when it has no text of its own.</item>
/// <item>The code is the first non-empty value over the mapped CODE paths. Pictures: the mapped IMAGE paths and the
/// fields of the same family (same parent, same name once a trailing number/separator is dropped; extras by number),
/// each value split into addresses, http/https only, distinct. A code seen again (trimmed, upper-case) keeps the first record.</item>
/// </list>
/// Streams (feeds are tens of MB) with DTDs prohibited and no resolver: no entity expansion, no outside fetch (XXE).
/// </summary>
public static partial class XmlFeedImageReader
{
    static XmlFeedImageReader()
    {
        // Turkish suppliers still write windows-1254 / ISO-8859-9 declarations; the phone's parser reads them.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>Reads the whole feed; throws <see cref="XmlException"/> for a broken or DTD-carrying document.</summary>
    public static XmlFeedImageResult Read(Stream input, string recordPath, IReadOnlyList<string> codePaths, IReadOnlyList<string> imagePaths)
    {
        var items = new List<XmlFeedImageItem>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        int records = 0, withoutCode = 0, duplicates = 0;
        ForEachRecord(input, recordPath, record =>
        {
            records++;
            var item = ToItem(record, codePaths, imagePaths);
            if (item is null) withoutCode++;
            else if (!seen.Add(CodeKey(item.Code))) duplicates++;
            else items.Add(item);
        });
        return new XmlFeedImageResult(items, records, withoutCode, duplicates);
    }

    /// <summary>The match key of a code: trimmed and upper-case (invariant; codes are matched as written, no Turkish folding).</summary>
    public static string CodeKey(string code) => code.Trim().ToUpperInvariant();

    /// <summary>Every record as field path → values, in document order (<c>XmlFeedScanner.forEachRecord</c>).</summary>
    public static void ForEachRecord(Stream input, string recordPath, Action<Dictionary<string, List<string>>> onRecord)
    {
        using var reader = new XmlTextReader(input)
        {
            // The phone's SAX parser is not namespace aware: names stay as written, an undeclared prefix is no error.
            Namespaces = false,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            WhitespaceHandling = WhitespaceHandling.All,
        };
        var stack = new List<string>();
        Dictionary<string, List<string>>? record = null;
        var relative = new List<string>();
        var texts = new List<StringBuilder>();

        void End()
        {
            var path = Pop(stack);
            if (record is null) return;
            if (path == recordPath && relative.Count == 0)
            {
                var done = record;
                record = null;
                onRecord(done);
                return;
            }
            var rel = Pop(relative);
            var text = Pop(texts).ToString().Trim();
            // Only elements with text are fields; a wrapper such as <images> stays out when its children were fields.
            var prefix = rel + "/";
            if (text.Length > 0 || !record.Keys.Any(k => k.StartsWith(prefix, StringComparison.Ordinal))) Add(record, rel, text);
        }

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                {
                    var name = reader.Name;
                    var path = stack.Count == 0 ? name : stack[^1] + "/" + name;
                    stack.Add(path);
                    var empty = reader.IsEmptyElement;
                    if (record is null)
                    {
                        if (path == recordPath)
                        {
                            record = new Dictionary<string, List<string>>(StringComparer.Ordinal);
                            while (reader.MoveToNextAttribute()) Add(record, "@" + reader.Name, reader.Value);
                            reader.MoveToElement();
                        }
                    }
                    else
                    {
                        var rel = relative.Count == 0 ? name : relative[^1] + "/" + name;
                        relative.Add(rel);
                        texts.Add(new StringBuilder());
                        while (reader.MoveToNextAttribute()) Add(record, rel + "/@" + reader.Name, reader.Value);
                        reader.MoveToElement();
                    }
                    if (empty) End();
                    break;
                }
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    if (record is not null && texts.Count > 0) texts[^1].Append(reader.Value);
                    break;
                case XmlNodeType.EndElement:
                    End();
                    break;
            }
        }
    }

    /// <summary>The product of one record, or null without a code (<c>XmlFeedReader.toItem</c>, code and pictures only).</summary>
    public static XmlFeedImageItem? ToItem(IReadOnlyDictionary<string, List<string>> record, IReadOnlyList<string> codePaths, IReadOnlyList<string> imagePaths)
    {
        var code = codePaths
            .SelectMany(p => record.TryGetValue(p, out var values) ? values : [])
            .Select(v => v.Trim())
            .FirstOrDefault(v => v.Length > 0);
        if (code is null) return null;
        var urls = ImageFields(record.Keys.ToList(), imagePaths)
            .SelectMany(p => record.TryGetValue(p, out var values) ? values : [])
            .SelectMany(SplitUrls)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        return new XmlFeedImageItem(code, urls);
    }

    /// <summary>
    /// The mapped picture fields and the fields of their families, in this order (<c>XmlFeedReader.imageFields</c>): a
    /// supplier filling <c>Resim3</c> later or adding <c>Resim4</c> is read without a new mapping. Family = same parent
    /// and the same name once the trailing number and separator are dropped (<c>Resimler/Resim1</c> → <c>Resimler/Resim2</c>,
    /// <c>image_1</c> → <c>image_2</c>); the extras come by number.
    /// </summary>
    public static IReadOnlyList<string> ImageFields(IReadOnlyCollection<string> recordFields, IReadOnlyList<string> mapped)
    {
        if (mapped.Count == 0) return [];
        var families = mapped.Select(FieldFamily).ToHashSet(StringComparer.Ordinal);
        var extra = recordFields
            .Where(f => !mapped.Contains(f, StringComparer.Ordinal) && families.Contains(FieldFamily(f)))
            .OrderBy(FieldFamily, StringComparer.Ordinal)
            .ThenBy(f => TrailingNumber(f) ?? -1)
            .ThenBy(f => f, StringComparer.Ordinal);
        return [.. mapped, .. extra];
    }

    /// <summary>One field's value as addresses: split before each further http(s) address and at line breaks; http/https only.</summary>
    public static IEnumerable<string> SplitUrls(string value) =>
        UrlSeparator().Split(value)
            .Select(v => v.Trim())
            .Where(v => v.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || v.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

    private static string FieldFamily(string path)
    {
        var slash = path.LastIndexOf('/');
        var parent = slash < 0 ? string.Empty : path[..slash];
        var name = TrailingIndex().Replace(path[(slash + 1)..], string.Empty).ToLowerInvariant();
        return parent + "/" + name;
    }

    private static int? TrailingNumber(string path)
    {
        var match = TrailingIndex().Match(path[(path.LastIndexOf('/') + 1)..]);
        return match.Success && int.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var n) ? n : null;
    }

    private static void Add(Dictionary<string, List<string>> record, string key, string value)
    {
        if (!record.TryGetValue(key, out var values)) record[key] = values = [];
        values.Add(value);
    }

    private static T Pop<T>(List<T> list)
    {
        var last = list[^1];
        list.RemoveAt(list.Count - 1);
        return last;
    }

    // Java's \s and \d are ASCII-only; the classes are spelled out so .NET matches the phone exactly.
    [GeneratedRegex(@"[,;| \t\n\x0B\f\r]+(?=https?://)|[\r\n]+", RegexOptions.CultureInvariant)]
    private static partial Regex UrlSeparator();

    [GeneratedRegex(@"[_\- \t\n\x0B\f\r]*([0-9]+)$", RegexOptions.CultureInvariant)]
    private static partial Regex TrailingIndex();
}
