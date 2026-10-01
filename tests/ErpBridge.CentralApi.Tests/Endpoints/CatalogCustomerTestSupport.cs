using System.Net;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// The web catalog served on its own host (<c>katalog.test</c>) from the test fixture's files
/// (<c>CustomerCatalog/WebFixture</c>), over SQLite.
/// </summary>
public sealed class CatalogHostFactory : SqliteCentralApiFactory
{
    public const string Host = "katalog.test";
    public const string Origin = "https://" + Host;

    public static string WebRoot => Path.Combine(AppContext.BaseDirectory, "CustomerCatalog", "WebFixture");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("CustomerCatalog:PublicHost", Host);
        builder.UseSetting("CustomerCatalog:WebRoot", WebRoot);
    }
}

/// <summary>A customer's browser: https on the catalog host, cookies kept, the page's own headers on changing requests.</summary>
internal static class CatalogCustomerTestSupport
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    public static HttpClient Browser(SqliteCentralApiFactory factory) => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri(CatalogHostFactory.Origin + "/"),
        HandleCookies = true,
        AllowAutoRedirect = false,
    });

    public static string Api(CatalogCompany company) => $"/api/v1/catalog/{company.Code}";

    /// <summary>A request as the catalog page sends it: <c>X-Katalog: 1</c> and its origin on a changing one.</summary>
    public static Task<HttpResponseMessage> SendAsync(HttpClient browser, HttpMethod method, string path, object? body = null, bool pageHeaders = true, string? bearer = null)
    {
        var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json");
        if (pageHeaders && method != HttpMethod.Get)
        {
            request.Headers.Add("X-Katalog", "1");
            request.Headers.Add("Origin", CatalogHostFactory.Origin);
        }
        if (bearer is not null) request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearer);
        return browser.SendAsync(request);
    }

    public static Task<HttpResponseMessage> GetAsync(HttpClient browser, string path, string? bearer = null) =>
        SendAsync(browser, HttpMethod.Get, path, bearer: bearer);

    public static Task<HttpResponseMessage> LoginAsync(HttpClient browser, CatalogCompany company, string username, string password, bool remember = false) =>
        SendAsync(browser, HttpMethod.Post, Api(company) + "/login", new { username, password, remember });

    /// <summary>The company publishes its catalog (<c>catalog_settings.IsEnabled</c>).</summary>
    public static async Task PublishAsync(SqliteCentralApiFactory factory, CatalogCompany company, bool enabled = true, int? defaultPriceListNo = null)
    {
        var settings = await OkAsync<CatalogSettingsDto>(await CustomerCatalogTestSupport.SendAsync(factory, HttpMethod.Get, Base + "/settings", company.Mudur));
        var saved = await CustomerCatalogTestSupport.SendAsync(factory, HttpMethod.Put, Base + "/settings", company.Mudur,
            new { revision = settings.Revision, isEnabled = enabled, defaultPriceListNo });
        saved.StatusCode.Should().Be(HttpStatusCode.OK, await saved.Content.ReadAsStringAsync());
    }

    /// <summary>A customer account made by staff, with its password.</summary>
    public static async Task<Guid> AccountAsync(SqliteCentralApiFactory factory, CatalogCompany company, string customerCode, string username,
        string password = "musteri123", decimal discountPercent = 0m, int? priceListNo = null, object? visibility = null,
        bool showStatement = false, bool isActive = true)
    {
        var created = await CustomerCatalogTestSupport.SendAsync(factory, HttpMethod.Post, Base + "/accounts", company.Mudur, new
        {
            customerCode,
            username,
            password,
            isActive,
            discountPercent,
            priceListNo,
            visibility = visibility ?? new { mode = "all", rules = Array.Empty<object>() },
            showStatement,
            showInvoices = false,
            showPurchased = false,
            canOrder = true,
        });
        return (await OkAsync<CatalogAccountSavedResponse>(created, HttpStatusCode.Created)).Account.Id;
    }

    /// <summary>A company with the module, the seeded stock and customers, its catalog published.</summary>
    public static async Task<CatalogCompany> OpenCatalogAsync(SqliteCentralApiFactory factory)
    {
        var company = await CompanyAsync(factory);
        await SeedCatalogAsync(factory, company.Id);
        await PublishAsync(factory, company);
        return company;
    }

    /// <summary>The value of a cookie this response set, or null.</summary>
    public static string? SetCookie(HttpResponseMessage response, string name) =>
        SetCookieHeader(response, name) is { } header ? header[(name.Length + 1)..].Split(';')[0] : null;

    public static string? SetCookieHeader(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues("Set-Cookie", out var values) ? values.FirstOrDefault(v => v.StartsWith(name + "=", StringComparison.Ordinal)) : null;
}
