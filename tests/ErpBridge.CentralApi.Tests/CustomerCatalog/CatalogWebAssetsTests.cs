using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.CustomerCatalog;

/// <summary>
/// GOAL_MUSTERI_KATALOGU §6–§7 (W1/W5): the customer web catalogue is plain files with no build step,
/// so these checks stand in for a bundler. They read <c>src/ErpBridge.CentralApi/wwwroot/katalog</c>
/// straight from the repository and need no server code: every import and every <c>/assets/%V%/</c>
/// reference resolves to a file, nothing the CSP would block or that builds markup from strings is
/// used, the shell keeps its placeholders, and JS+CSS stay within the gzip budget.
/// </summary>
public class CatalogWebAssetsTests
{
    private const int GzipBudgetBytes = 100 * 1024;

    private static readonly string Root = LocateWebRoot();
    private static readonly string JsRoot = Path.Combine(Root, "js");

    // import x from '…' · import { a, b } from '…' · import '…' · export { a } from '…' · export * from '…'
    private static readonly Regex StaticImport = new(
        @"(?:^|[;\s])(?:import\s+(?:[\w*{}\s,$]+?\s+from\s+)?|export\s+(?:\*(?:\s+as\s+\w+)?|\{[^}]*\})\s+from\s+)['""]([^'""]+)['""]",
        RegexOptions.Multiline);

    private static readonly Regex DynamicImport = new(@"\bimport\s*\(\s*['""]([^'""]+)['""]\s*\)");
    private static readonly Regex AnyDynamicImport = new(@"\bimport\s*\(");
    private static readonly Regex AssetReference = new(@"/assets/%V%/([^""'\s>]+)");

    /// <summary>API that turns strings into markup or code, or that the page's CSP / ES2020 floor rules out.</summary>
    private static readonly (string Pattern, string Why)[] Forbidden =
    [
        (@"\binnerHTML\b", "markup from strings (use h())"),
        (@"\bouterHTML\b", "markup from strings (use h())"),
        (@"\binsertAdjacentHTML\b", "markup from strings (use h())"),
        (@"\bdocument\s*\.\s*write", "markup from strings"),
        (@"\beval\s*\(", "code from strings"),
        (@"\bnew\s+Function\b", "code from strings"),
        (@"setAttribute\(\s*['""]style['""]", "inline style is blocked by CSP style-src 'self'"),
        (@"\bdocument\s*\.\s*cookie\b", "the session cookie is HttpOnly; the page never touches cookies"),
        (@"\.replaceAll\s*\(", "ES2021 (iOS 15.4 / Chrome 92 floor is ES2020)"),
        (@"\?\?=|\|\|=|&&=", "ES2021 logical assignment"),
        (@"\.at\s*\(", "ES2022 Array.prototype.at"),
        (@"\bObject\s*\.\s*hasOwn\s*\(", "ES2022"),
        (@"\bstructuredClone\s*\(", "not in the browser floor"),
        (@"\bAbortSignal\s*\.\s*(?:any|timeout)\s*\(", "not in the browser floor"),
    ];

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".css", ".js", ".svg", ".txt",
    };

    [Fact]
    public void Index_html_keeps_the_version_and_title_placeholders()
    {
        var html = ReadIndex();

        html.Should().Contain("%V%");
        html.Should().Contain("<title>%TITLE%</title>");
        html.Should().Contain("<html lang=\"tr\">");
        html.Should().Contain("<script type=\"module\" src=\"/assets/%V%/js/main.js\"></script>");
        html.Should().Contain("<script nomodule src=\"/assets/%V%/js/unsupported.js\"></script>");
        html.Should().Contain("<noscript>");
    }

    [Fact]
    public void Index_html_has_no_inline_script_style_or_handlers()
    {
        var html = ReadIndex();

        Regex.IsMatch(html, @"<script\b(?![^>]*\bsrc\s*=)[^>]*>", RegexOptions.IgnoreCase)
            .Should().BeFalse("CSP script-src 'self' blocks inline scripts");
        Regex.IsMatch(html, @"<style\b", RegexOptions.IgnoreCase).Should().BeFalse("CSP style-src 'self' blocks <style>");
        Regex.IsMatch(html, @"\sstyle\s*=", RegexOptions.IgnoreCase).Should().BeFalse("CSP blocks style attributes");
        Regex.IsMatch(html, @"\son[a-z]+\s*=", RegexOptions.IgnoreCase).Should().BeFalse("CSP blocks inline handlers");
        html.Should().NotContainEquivalentOf("javascript:");
    }

    [Fact]
    public void Every_asset_path_in_index_html_is_a_real_file()
    {
        var references = AssetReference.Matches(ReadIndex()).Select(m => m.Groups[1].Value).ToList();

        references.Should().NotBeEmpty();
        foreach (var reference in references)
        {
            File.Exists(Path.Combine(Root, reference.Replace('/', Path.DirectorySeparatorChar)))
                .Should().BeTrue($"index.html references /assets/%V%/{reference}");
        }
    }

    [Fact]
    public void Every_import_resolves_to_a_file_inside_the_catalogue()
    {
        var problems = new List<string>();
        foreach (var file in JsFiles())
        {
            var source = File.ReadAllText(file);
            var literalDynamic = DynamicImport.Matches(source).Count;
            if (AnyDynamicImport.Matches(source).Count != literalDynamic)
            {
                problems.Add($"{Relative(file)}: import() with a computed specifier cannot be checked");
            }

            foreach (var specifier in Imports(source))
            {
                var target = Resolve(file, specifier, out var error);
                if (target is null) problems.Add($"{Relative(file)}: '{specifier}' {error}");
            }
        }

        problems.Should().BeEmpty();
    }

    [Fact]
    public void Every_script_is_reachable_from_index_html()
    {
        var entries = AssetReference.Matches(ReadIndex())
            .Select(m => m.Groups[1].Value)
            .Where(p => p.EndsWith(".js", StringComparison.Ordinal))
            .Select(p => Path.GetFullPath(Path.Combine(Root, p.Replace('/', Path.DirectorySeparatorChar))));
        var reached = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>(entries);
        while (queue.Count > 0)
        {
            var file = queue.Dequeue();
            if (!reached.Add(file) || !File.Exists(file)) continue;
            foreach (var specifier in Imports(File.ReadAllText(file)))
            {
                var target = Resolve(file, specifier, out _);
                if (target is not null) queue.Enqueue(target);
            }
        }

        JsFiles().Where(f => !reached.Contains(Path.GetFullPath(f))).Select(Relative)
            .Should().BeEmpty("a script nothing loads is dead weight in the budget");
    }

    [Fact]
    public void Scripts_use_no_forbidden_api()
    {
        var hits = new List<string>();
        foreach (var file in JsFiles())
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                foreach (var (pattern, why) in Forbidden)
                {
                    if (Regex.IsMatch(lines[i], pattern)) hits.Add($"{Relative(file)}:{i + 1}: {why}");
                }
            }
        }

        hits.Should().BeEmpty();
    }

    [Fact]
    public void Stylesheets_load_nothing_from_elsewhere()
    {
        foreach (var file in Directory.EnumerateFiles(Root, "*.css", SearchOption.AllDirectories))
        {
            var css = File.ReadAllText(file);
            css.Should().NotContain("@import", Relative(file));
            Regex.IsMatch(css, @"url\(\s*['""]?(?:https?:)?//", RegexOptions.IgnoreCase)
                .Should().BeFalse($"{Relative(file)} may only use same-origin resources (CSP)");
        }
    }

    [Fact]
    public void Tokens_css_names_its_source()
    {
        File.ReadAllText(Path.Combine(Root, "css", "tokens.css"))
            .Should().Contain("KAYNAK: Siparis_Cepte docs/design-tokens.css");
    }

    [Fact]
    public void Web_root_holds_only_servable_files()
    {
        Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)
            .Where(f => !AllowedExtensions.Contains(Path.GetExtension(f)))
            .Select(Relative)
            .Should().BeEmpty("tests, mocks and tooling live outside wwwroot so they are never published");
    }

    [Fact]
    public void Scripts_and_styles_fit_the_gzip_budget()
    {
        long total = 0;
        foreach (var file in Directory.EnumerateFiles(Root, "*.*", SearchOption.AllDirectories)
                     .Where(f => f.EndsWith(".js", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".css", StringComparison.OrdinalIgnoreCase)))
        {
            total += GzipLength(File.ReadAllBytes(file));
        }

        total.Should().BeGreaterThan(0);
        total.Should().BeLessThanOrEqualTo(GzipBudgetBytes, "GOAL_MUSTERI_KATALOGU §7: JS+CSS gzip ≤ 100 KB");
    }

    private static IEnumerable<string> Imports(string source) =>
        StaticImport.Matches(source).Concat(DynamicImport.Matches(source)).Select(m => m.Groups[1].Value);

    /// <summary>Relative specifiers only (no bare modules, no URLs), pointing at an existing file under the web root.</summary>
    private static string? Resolve(string fromFile, string specifier, out string error)
    {
        if (!specifier.StartsWith("./", StringComparison.Ordinal) && !specifier.StartsWith("../", StringComparison.Ordinal))
        {
            error = "is not a relative path (no bare modules or URLs without a build step)";
            return null;
        }

        var target = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fromFile)!, specifier.Replace('/', Path.DirectorySeparatorChar)));
        if (!target.StartsWith(Path.GetFullPath(Root) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            error = "points outside wwwroot/katalog";
            return null;
        }

        if (!File.Exists(target))
        {
            error = "does not exist";
            return null;
        }

        error = string.Empty;
        return target;
    }

    private static long GzipLength(byte[] data)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(data, 0, data.Length);
        }

        return output.Length;
    }

    private static string ReadIndex() => File.ReadAllText(Path.Combine(Root, "index.html"), Encoding.UTF8);

    private static IEnumerable<string> JsFiles() => Directory.EnumerateFiles(JsRoot, "*.js", SearchOption.AllDirectories);

    private static string Relative(string file) => Path.GetRelativePath(Root, file).Replace('\\', '/');

    /// <summary>
    /// Walks up from the test output folder to the directory holding ErpBridge.sln, so the check works
    /// from bin/Debug, bin/Release, CI and any worktree without copying the web files into the output.
    /// </summary>
    private static string LocateWebRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (!File.Exists(Path.Combine(dir.FullName, "ErpBridge.sln"))) continue;
            var root = Path.Combine(dir.FullName, "src", "ErpBridge.CentralApi", "wwwroot", "katalog");
            if (Directory.Exists(root)) return root;
            throw new DirectoryNotFoundException($"'{root}' is missing next to ErpBridge.sln.");
        }

        throw new DirectoryNotFoundException(
            $"No ErpBridge.sln at or above '{AppContext.BaseDirectory}'; cannot locate wwwroot/katalog.");
    }
}
