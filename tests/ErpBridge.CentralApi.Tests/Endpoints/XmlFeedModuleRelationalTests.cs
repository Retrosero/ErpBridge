using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// XML ürün modülü: the operator sells the module from the Admin console (<c>tenant_modules</c>,
/// carried in the session), the company administrator saves the feed from the phone and every
/// phone reads it (<c>tenant_xml_feed_settings</c>). An ERP company never gets a full import.
/// </summary>
public sealed class XmlFeedModuleRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private const string ConfigPath = "/api/v1/android/xml-feed/config";

    private readonly SqliteCentralApiFactory _factory;

    public XmlFeedModuleRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task The_session_carries_the_modules_the_operator_switched_on()
    {
        var c = await CompanyAsync(TenantDataSources.Native, withModule: false);
        (await MeAsync(c.Patron)).Modules.Should().BeEmpty();

        (await SetModulesAsync(c, "XML_IMPORT")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await MeAsync(c.Patron)).Modules.Should().Equal("xml_import");
        (await MeAsync(c.Ali)).Modules.Should().Equal("xml_import");
        var login = await (await LoginResponseAsync(c.Code, "ali", "DEV-ALI-2")).ReadAsJsonAsync<MobileLoginResponse>();
        login.Session.Modules.Should().Equal("xml_import");

        var overview = await (await _factory.CreateClient().GetAsync($"/api/v1/admin/tenants/{c.Id}/mobile", c.AdminToken))
            .ReadAsJsonAsync<TenantMobileOverviewResponse>();
        overview.Modules.Should().Equal("xml_import");
        using (var scope = _factory.Services.CreateScope())
        {
            var row = await scope.ServiceProvider.GetRequiredService<CentralApiDbContext>().TenantModules.SingleAsync(m => m.TenantId == c.Id);
            row.EnabledBy.Should().Be(c.AdminEmail);
        }

        (await SetModulesAsync(c)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await MeAsync(c.Patron)).Modules.Should().BeEmpty();
    }

    [Fact]
    public async Task Unknown_modules_and_tenants_are_refused()
    {
        var c = await CompanyAsync(TenantDataSources.Native, withModule: false);
        var unknown = await SetModulesAsync(c, "xml_import", "barcode_labels");
        unknown.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await unknown.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("UNKNOWN_MODULE");
        (await MeAsync(c.Patron)).Modules.Should().BeEmpty("a refused set changes nothing");

        (await SendAsync(HttpMethod.Put, $"/api/v1/admin/tenants/{Guid.NewGuid()}/mobile/modules", new { modules = new[] { "xml_import" } }, c.AdminToken))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Without_the_module_the_feed_is_closed_to_everyone()
    {
        var c = await CompanyAsync(TenantDataSources.Native, withModule: false);
        foreach (var (method, token) in new[] { (HttpMethod.Get, c.Ali), (HttpMethod.Get, c.Patron), (HttpMethod.Put, c.Patron), (HttpMethod.Delete, c.Patron) })
        {
            var response = await SendAsync(method, ConfigPath, method == HttpMethod.Put ? ValidConfig() : null, token);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden, method.Method);
            (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("MODULE_NOT_ENABLED", method.Method);
        }
    }

    [Fact]
    public async Task An_unconfigured_feed_reads_as_defaults()
    {
        var c = await CompanyAsync(TenantDataSources.Native, withModule: true);
        var config = await GetConfigAsync(c.Ali);
        config.Configured.Should().BeFalse();
        config.Url.Should().BeNull();
        config.RecordPath.Should().BeNull();
        config.Mapping.Should().BeEmpty();
        config.DownloadImages.Should().BeTrue();
        config.ImportDescriptions.Should().BeFalse();
        config.FullImport.Should().BeFalse();
        config.UpdatedAtUtc.Should().BeNull();
        config.UpdatedByName.Should().BeNull();
    }

    [Fact]
    public async Task Only_a_company_administrator_changes_the_feed()
    {
        var c = await CompanyAsync(TenantDataSources.Native, withModule: true);
        foreach (var method in new[] { HttpMethod.Put, HttpMethod.Delete })
        {
            var response = await SendAsync(method, ConfigPath, method == HttpMethod.Put ? ValidConfig() : null, c.Ali);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden, method.Method);
            (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("ADMIN_REQUIRED", method.Method);
        }
        (await GetConfigAsync(c.Ali)).Configured.Should().BeFalse();
    }

    [Theory]
    [InlineData("ftp://example.com/feed.xml", "urunler/urun", "CODE")]
    [InlineData("feed.xml", "urunler/urun", "CODE")]
    [InlineData("https://example.com/feed.xml", " ", "CODE")]
    [InlineData("https://example.com/feed.xml", "urunler/urun", "code")]
    [InlineData("https://example.com/feed.xml", "urunler/urun", "IMAGE")]
    [InlineData("https://example.com/feed.xml", "urunler/urun", "NAME")]
    public async Task An_invalid_feed_is_refused(string url, string recordPath, string target)
    {
        var c = await CompanyAsync(TenantDataSources.Native, withModule: true);
        var mapping = new Dictionary<string, string[]> { [target] = ["kod"] };
        if (target == "NAME") mapping["CODE"] = ["kod"];
        var response = await SendAsync(HttpMethod.Put, ConfigPath, new { url, recordPath, mapping, downloadImages = true, importDescriptions = true, fullImport = true }, c.Patron);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.ReadAsJsonAsync<ApiError>();
        error.ErrorCode.Should().Be("INVALID_XML_FEED_CONFIG");
        error.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Path_limits_and_a_blank_code_are_refused()
    {
        var c = await CompanyAsync(TenantDataSources.Native, withModule: true);
        foreach (var mapping in new[]
                 {
                     new Dictionary<string, string[]> { ["CODE"] = [" ", ""] },
                     new Dictionary<string, string[]> { ["CODE"] = Enumerable.Range(0, 11).Select(i => "k" + i).ToArray() },
                     new Dictionary<string, string[]> { ["CODE"] = [new string('a', 257)] },
                 })
        {
            var response = await SendAsync(HttpMethod.Put, ConfigPath, new { url = "https://example.com/f.xml", recordPath = "u", mapping }, c.Patron);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_XML_FEED_CONFIG");
        }
        var longUrl = await SendAsync(HttpMethod.Put, ConfigPath,
            new { url = "https://example.com/" + new string('a', 2048), recordPath = "u", mapping = new Dictionary<string, string[]> { ["CODE"] = ["kod"] } }, c.Patron);
        longUrl.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_saved_feed_reads_back_the_same_on_every_phone()
    {
        var c = await CompanyAsync(TenantDataSources.Native, withModule: true);
        var saved = await PutConfigOkAsync(c.Patron, ValidConfig(fullImport: true));
        saved.Configured.Should().BeTrue();
        saved.FullImport.Should().BeTrue("a native company may import every field");
        saved.UpdatedByName.Should().Be("Patron Bey");

        var read = await GetConfigAsync(c.Ali);
        read.Configured.Should().BeTrue();
        read.Url.Should().Be("https://example.com/urunler.xml");
        read.RecordPath.Should().Be("Urunler/Urun");
        read.Mapping.Keys.Should().BeEquivalentTo("CODE", "IMAGE", "DESCRIPTION");
        read.Mapping["CODE"].Should().Equal("StokKodu", "Kod");
        read.Mapping["IMAGE"].Should().Equal("Resimler/Resim1", "Resim");
        read.Mapping["DESCRIPTION"].Should().Equal("Aciklama");
        read.DownloadImages.Should().BeFalse();
        read.ImportDescriptions.Should().BeTrue();
        read.FullImport.Should().BeTrue();
        read.UpdatedAtUtc.Should().NotBeNull();
        read.UpdatedByName.Should().Be("Patron Bey");

        // A second save replaces the first.
        (await PutConfigOkAsync(c.Patron, new { url = "http://example.com/v2.xml", recordPath = "r", mapping = new Dictionary<string, string[]> { ["CODE"] = ["k"] } })).Url
            .Should().Be("http://example.com/v2.xml");
        (await GetConfigAsync(c.Ali)).Mapping.Keys.Should().Equal("CODE");
    }

    [Fact]
    public async Task An_erp_company_never_stores_a_full_import()
    {
        var c = await CompanyAsync(TenantDataSources.Erp, withModule: true);
        (await PutConfigOkAsync(c.Patron, ValidConfig(fullImport: true))).FullImport.Should().BeFalse();
        (await GetConfigAsync(c.Ali)).FullImport.Should().BeFalse("the ERP keeps the master data");
    }

    [Fact]
    public async Task A_company_switched_to_its_erp_stops_getting_a_full_import()
    {
        var c = await CompanyAsync(TenantDataSources.Native, withModule: true);
        (await PutConfigOkAsync(c.Patron, ValidConfig(fullImport: true))).FullImport.Should().BeTrue();
        (await SendAsync(HttpMethod.Put, $"/api/v1/admin/tenants/{c.Id}/mobile/data-source", new { dataSource = TenantDataSources.Erp }, c.AdminToken))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await GetConfigAsync(c.Ali)).FullImport.Should().BeFalse("the ERP now keeps the master data");
    }

    [Fact]
    public async Task A_long_operator_email_does_not_break_enabling_a_module()
    {
        var c = await CompanyAsync(TenantDataSources.Native, withModule: false);
        var admin = await _factory.SeedAdminAsync(email: new string('a', 200) + "@test.local");
        var response = await SendAsync(HttpMethod.Put, $"/api/v1/admin/tenants/{c.Id}/mobile/modules",
            new { modules = new[] { TenantModules.XmlImport } }, _factory.IssueAdminJwt(admin.Id));
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await MeAsync(c.Ali)).Modules.Should().Equal(TenantModules.XmlImport);
    }

    [Fact]
    public async Task Deleting_the_feed_returns_it_to_defaults_and_is_idempotent()
    {
        var c = await CompanyAsync(TenantDataSources.Native, withModule: true);
        await PutConfigOkAsync(c.Patron, ValidConfig());
        (await SendAsync(HttpMethod.Delete, ConfigPath, null, c.Patron)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await GetConfigAsync(c.Ali)).Configured.Should().BeFalse();
        (await SendAsync(HttpMethod.Delete, ConfigPath, null, c.Patron)).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ---- setup --------------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Code, string AdminToken, string AdminEmail, string Patron, string Ali);

    private static object ValidConfig(bool fullImport = false) => new
    {
        url = "https://example.com/urunler.xml",
        recordPath = "Urunler/Urun",
        mapping = new Dictionary<string, string[]>
        {
            ["CODE"] = ["StokKodu", " Kod "],
            ["IMAGE"] = ["Resimler/Resim1", "Resim", ""],
            ["DESCRIPTION"] = ["Aciklama"],
            ["BRAND"] = [" "],
        },
        downloadImages = false,
        importDescriptions = true,
        fullImport,
    };

    private async Task<Company> CompanyAsync(string dataSource, bool withModule)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"XML-{suffix}", $"XML tenant {suffix}");
        var email = $"ops-{Guid.NewGuid():N}@test.local";
        var admin = await _factory.SeedAdminAsync(email: email);
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        (await SendAsync(HttpMethod.Put, $"{basePath}/data-source", new { dataSource }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SendAsync(HttpMethod.Put, $"{basePath}/subscription", new { seats = 10, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, fullName, role) in new[] { ("patron", "Patron Bey", "ADMIN"), ("ali", "Ali", "SALES") })
            (await SendAsync(HttpMethod.Post, $"{basePath}/users", new { username, fullName, password = Password, roles = new[] { role } }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await _factory.CreateClient().GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;

        var company = new Company(tenant.Id, code, adminToken, email,
            await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "ali", $"DEV-A-{suffix}"));
        if (withModule) (await SetModulesAsync(company, TenantModules.XmlImport)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        return company;
    }

    private Task<HttpResponseMessage> SetModulesAsync(Company c, params string[] modules) =>
        SendAsync(HttpMethod.Put, $"/api/v1/admin/tenants/{c.Id}/mobile/modules", new { modules }, c.AdminToken);

    private async Task<MobileSessionDto> MeAsync(string token)
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/android/account/me", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<MobileSessionDto>();
    }

    private async Task<XmlFeedConfigDto> GetConfigAsync(string token)
    {
        var response = await _factory.CreateClient().GetAsync(ConfigPath, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<XmlFeedConfigDto>();
    }

    private async Task<XmlFeedConfigDto> PutConfigOkAsync(string token, object body)
    {
        var response = await SendAsync(HttpMethod.Put, ConfigPath, body, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<XmlFeedConfigDto>();
    }

    private Task<HttpResponseMessage> LoginResponseAsync(string code, string username, string deviceId) =>
        _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.300" });

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await LoginResponseAsync(code, username, deviceId);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? value, string token)
    {
        var request = new HttpRequestMessage(method, path);
        if (value is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }
}
