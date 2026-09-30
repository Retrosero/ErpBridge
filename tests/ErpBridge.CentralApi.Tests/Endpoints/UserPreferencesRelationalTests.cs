using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Kullanıcı görünüm tercihleri (<c>/api/v1/android/account/preferences</c>): her kullanıcının kendi JSON belgesi,
/// sürüm her kayıtta artar, başka kullanıcı okuyamaz/yazamaz, gövde tavanı ve şekil doğrulaması.
/// </summary>
public sealed class UserPreferencesRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private const string Path = "/api/v1/android/account/preferences";

    private readonly SqliteCentralApiFactory _factory;

    public UserPreferencesRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_user_who_never_saved_gets_version_zero_and_no_data()
    {
        var (ali, _) = await TwoUsersAsync();
        var dto = await GetAsync(ali);
        dto.Version.Should().Be(0);
        dto.Data.Should().BeNull();
    }

    [Fact]
    public async Task Saving_keeps_the_document_as_sent_and_counts_the_version_up()
    {
        var (ali, _) = await TwoUsersAsync();

        var first = await PutOkAsync(ali, """{"catalog":{"viewMode":"List"},"gelecek":{"x":[1,2]}}""");
        first.Version.Should().Be(1);

        var second = await PutOkAsync(ali, """{"catalog":{"viewMode":"Grid"}}""");
        second.Version.Should().Be(2);

        var read = await GetAsync(ali);
        read.Version.Should().Be(2);
        read.Data!.Value.GetProperty("catalog").GetProperty("viewMode").GetString().Should().Be("Grid");
        read.Data!.Value.TryGetProperty("gelecek", out _).Should().BeFalse("the second save replaces the whole document");
        read.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Users_of_one_company_never_see_each_others_preferences()
    {
        var (ali, veli) = await TwoUsersAsync();
        await PutOkAsync(ali, """{"home":{"showQuickLabels":false}}""");

        (await GetAsync(veli)).Version.Should().Be(0);
        await PutOkAsync(veli, """{"home":{"showQuickLabels":true}}""");

        (await GetAsync(ali)).Data!.Value.GetProperty("home").GetProperty("showQuickLabels").GetBoolean().Should().BeFalse();
        (await GetAsync(veli)).Data!.Value.GetProperty("home").GetProperty("showQuickLabels").GetBoolean().Should().BeTrue();
    }

    [Theory]
    [InlineData("""{}""")]
    [InlineData("""{"data":[1,2]}""")]
    [InlineData("""{"data":"metin"}""")]
    [InlineData("""{"data":null}""")]
    [InlineData("""[1]""")]
    public async Task A_body_without_a_json_object_as_data_is_refused(string body)
    {
        var (ali, _) = await TwoUsersAsync();
        var response = await SendAsync(HttpMethod.Put, Path, body, ali);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_PREFERENCES");
        (await GetAsync(ali)).Version.Should().Be(0, "a refused save changes nothing");
    }

    [Fact]
    public async Task A_document_over_the_limit_is_refused()
    {
        var (ali, _) = await TwoUsersAsync();
        var big = "{\"data\":{\"x\":\"" + new string('a', MobileUserPreferencesEndpoints.MaxJsonLength) + "\"}}";
        (await SendAsync(HttpMethod.Put, Path, big, ali)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await GetAsync(ali)).Version.Should().Be(0);
    }

    [Fact]
    public async Task Without_a_token_the_endpoint_is_closed()
    {
        var client = _factory.CreateClient();
        (await client.GetAsync(Path)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<UserPreferencesDto> GetAsync(string token)
    {
        var response = await _factory.CreateClient().GetAsync(Path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<UserPreferencesDto>();
    }

    private async Task<UserPreferencesDto> PutOkAsync(string token, string data)
    {
        var response = await SendAsync(HttpMethod.Put, Path, "{\"data\":" + data + "}", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<UserPreferencesDto>();
    }

    /// <summary>A company with two salespeople; returns their tokens.</summary>
    private async Task<(string Ali, string Veli)> TwoUsersAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"PRF-{suffix}", $"Prefs tenant {suffix}");
        var email = $"ops-{Guid.NewGuid():N}@test.local";
        var admin = await _factory.SeedAdminAsync(email: email);
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        (await SendAsync(HttpMethod.Put, $"{basePath}/data-source", JsonSerializer.Serialize(new { dataSource = TenantDataSources.Native }, Web), adminToken))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SendAsync(HttpMethod.Put, $"{basePath}/subscription", JsonSerializer.Serialize(new { seats = 10, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, Web), adminToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var username in new[] { "ali", "veli" })
            (await SendAsync(HttpMethod.Post, $"{basePath}/users",
                JsonSerializer.Serialize(new { username, fullName = username, password = Password, roles = new[] { "SALES" } }, Web), adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await _factory.CreateClient().GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        return (await LoginAsync(code, "ali", $"DEV-A-{suffix}"), await LoginAsync(code, "veli", $"DEV-V-{suffix}"));
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.300" });
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, string? json, string token)
    {
        var request = new HttpRequestMessage(method, path);
        if (json is not null) request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }
}
