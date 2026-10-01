using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ErpBridge.CentralApi.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// Serves the web catalog (docs/GOAL_MUSTERI_KATALOGU.md §6–7) on <c>CustomerCatalog:PublicHost</c> only; with no host
/// configured nothing here exists. The files under <c>CustomerCatalog:WebRoot</c> are a build-free static app:
/// <list type="bullet">
///   <item><description><c>/assets/{v}/…</c>: the files, cached for good (<c>immutable</c>). <c>v</c> is the first 10 hex of
///   the SHA-256 of every file, taken once at start-up, so a deploy changes every address; an old <c>v</c> is 404
///   <c>no-store</c> (the page reloads). Served before routing and the rate limiter.</description></item>
///   <item><description><c>/{code}</c> and <c>/{code}/…</c>: the shell (<c>index.html</c>, read once; <c>%V%</c> → v,
///   <c>%TITLE%</c> → "{company} · Müşteri Kataloğu" for an open catalog, kept 5 minutes), <c>no-cache</c>. An unknown
///   or closed code gets the same page with 404.</description></item>
///   <item><description><c>/robots.txt</c> (nothing to index) and <c>/favicon.svg</c>.</description></item>
/// </list>
/// Everything else on that host is 404 but the customer API (<c>/api/v1/catalog/**</c>) and <c>/health*</c>: the
/// staff, agent and admin API are not reachable through the catalog's name. Every answer on the host carries the
/// catalog's security headers (CSP without inline script, no framing, no indexing, HSTS).
/// </summary>
public sealed class CatalogWeb
{
    public const string AssetsPrefix = "/assets";
    public const string GenericTitle = "Müşteri Kataloğu";
    public const string ContentSecurityPolicy =
        "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' https: data:; connect-src 'self'; "
        + "object-src 'none'; base-uri 'none'; frame-ancestors 'none'; form-action 'self'";

    private const string ImmutableCache = "public, max-age=31536000, immutable";
    private const string CodeConstraint = "regex(^[A-Za-z0-9]{{4,16}}$)";
    private static readonly TimeSpan TitleLifetime = TimeSpan.FromMinutes(5);
    private static readonly HtmlEncoder Html = HtmlEncoder.Create(UnicodeRanges.All);

    private readonly string _host;
    private readonly string _root;
    private readonly string? _shell;

    private CatalogWeb(string host, string root, string version, string? shell)
    {
        _host = host;
        _root = root;
        Version = version;
        _shell = shell;
    }

    /// <summary>The asset version: 10 hex digits.</summary>
    public string Version { get; }

    /// <summary>Null when <c>CustomerCatalog:PublicHost</c> is empty: the catalog is not served.</summary>
    public static CatalogWeb? Create(WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<CustomerCatalogOptions>>().Value;
        var host = options.PublicHost.Trim();
        if (host.Length == 0) return null;
        var root = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, options.WebRoot));
        var index = Path.Combine(root, "index.html");
        if (!File.Exists(index))
            app.Logger.LogWarning("The web catalog has no index.html under {WebRoot}; catalog pages answer 404.", root);
        var version = VersionOf(root);
        var shell = File.Exists(index) ? File.ReadAllText(index, Encoding.UTF8).Replace("%V%", version, StringComparison.Ordinal) : null;
        return new CatalogWeb(host, root, version, shell);
    }

    /// <summary>The first 10 hex digits of SHA-256 over every file (path and bytes, in path order).</summary>
    public static string VersionOf(string root)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        if (Directory.Exists(root))
        {
            var files = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(f => (Full: f, Relative: Path.GetRelativePath(root, f).Replace('\\', '/')))
                .OrderBy(f => f.Relative, StringComparer.Ordinal);
            foreach (var (full, relative) in files)
            {
                hash.AppendData(Encoding.UTF8.GetBytes(relative));
                hash.AppendData([0]);
                hash.AppendData(File.ReadAllBytes(full));
                hash.AppendData([0]);
            }
        }
        return Convert.ToHexStringLower(hash.GetHashAndReset())[..10];
    }

    public bool IsCatalogHost(HttpContext http) => string.Equals(http.Request.Host.Host, _host, StringComparison.OrdinalIgnoreCase);

    /// <summary>Before routing: headers, the files, robots and favicon, and the allow-list of the catalog host.</summary>
    public void UseFiles(IApplicationBuilder app) => app.UseWhen(IsCatalogHost, branch =>
    {
        branch.Use((http, next) =>
        {
            http.Response.OnStarting(() =>
            {
                var headers = http.Response.Headers;
                headers.ContentSecurityPolicy = ContentSecurityPolicy;
                headers.XContentTypeOptions = "nosniff";
                headers["X-Robots-Tag"] = "noindex";
                headers["Referrer-Policy"] = "same-origin";
                headers.StrictTransportSecurity = "max-age=31536000";
                return Task.CompletedTask;
            });
            return next(http);
        });
        if (Directory.Exists(_root))
        {
            branch.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(_root),
                RequestPath = AssetsPrefix + "/" + Version,
                OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = ImmutableCache,
            });
        }
        branch.Use(AllowListAsync);
    });

    /// <summary>The shell routes, on the catalog host only.</summary>
    public void MapShell(IEndpointRouteBuilder routes)
    {
        foreach (var pattern in new[] { "/{code:" + CodeConstraint + "}", "/{code:" + CodeConstraint + "}/{**rest}" })
        {
            routes.MapGet(pattern, ShellAsync)
                .RequireHost(_host)
                .AllowAnonymous()
                .RequireRateLimiting(Program.CatalogPublicRateLimitPolicy)
                .ExcludeFromDescription();
        }
        // The bare name (someone typed the address without a company code): the shell says to use the shared link.
        routes.MapGet("/", RootShell)
            .RequireHost(_host)
            .AllowAnonymous()
            .RequireRateLimiting(Program.CatalogPublicRateLimitPolicy)
            .ExcludeFromDescription();
    }

    private async Task AllowListAsync(HttpContext http, RequestDelegate next)
    {
        var request = http.Request;
        var path = request.Path;
        if (path.StartsWithSegments(CatalogCookies.ApiPrefix, StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
        {
            await next(http);
            return;
        }
        var readOnly = HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method);
        if (readOnly && path.Equals("/robots.txt", StringComparison.OrdinalIgnoreCase))
        {
            http.Response.Headers.CacheControl = "public, max-age=86400";
            http.Response.ContentType = "text/plain; charset=utf-8";
            await http.Response.WriteAsync("User-agent: *\nDisallow: /\n", http.RequestAborted);
            return;
        }
        if (readOnly && path.Equals("/favicon.svg", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(_root, "favicon.svg")))
        {
            http.Response.Headers.CacheControl = "public, max-age=86400";
            http.Response.ContentType = "image/svg+xml";
            await http.Response.SendFileAsync(Path.Combine(_root, "favicon.svg"), http.RequestAborted);
            return;
        }
        if (readOnly && (path.Value is null or "" or "/"))
        {
            await next(http);
            return;
        }
        // A shell path: its first segment is a company code ("assets" is one too, but an asset never reaches here).
        var first = path.Value?.Split('/', 3) is { Length: >= 2 } segments ? segments[1] : string.Empty;
        if (readOnly && !path.StartsWithSegments(AssetsPrefix, StringComparison.OrdinalIgnoreCase) && CatalogCookies.CodePattern().IsMatch(first))
        {
            await next(http);
            return;
        }
        http.Response.StatusCode = StatusCodes.Status404NotFound;
        http.Response.Headers.CacheControl = "no-store";
    }

    private IResult RootShell(HttpContext http)
    {
        http.Response.Headers.CacheControl = "no-cache";
        if (_shell is null) return Results.NotFound();
        return Results.Content(
            _shell.Replace("%TITLE%", Html.Encode(GenericTitle), StringComparison.Ordinal),
            "text/html; charset=utf-8",
            Encoding.UTF8,
            StatusCodes.Status200OK);
    }

    private async Task<IResult> ShellAsync(string code, HttpContext http, CentralApiDbContext db, IMemoryCache cache, CancellationToken ct)
    {
        http.Response.Headers.CacheControl = "no-cache";
        if (_shell is null) return Results.NotFound();
        var company = await cache.GetOrCreateAsync(("catalog-web-title", code.ToUpperInvariant()), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TitleLifetime;
            return (await CatalogCustomerAccess.OpenCompanyAsync(db, code, ct))?.Name;
        });
        var title = company is null ? GenericTitle : company + " · " + GenericTitle;
        return Results.Content(
            _shell.Replace("%TITLE%", Html.Encode(title), StringComparison.Ordinal),
            "text/html; charset=utf-8",
            Encoding.UTF8,
            company is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK);
    }
}
