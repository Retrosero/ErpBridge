using System.Net;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// <c>GET /api/v1/storage/files/{id}</c> (GOAL_DEPOLAMA_R2 S1): a 302 to a short presigned address for a private file, to
/// the CDN for a public one; only the uploader, a storage manager (admin, manager) or an area rule may open a file; every
/// refusal is the same 404.
/// </summary>
public sealed class StorageFileEndpointRelationalTests : IClassFixture<StorageCentralApiFactory>
{
    private readonly StorageCentralApiFactory _factory;

    public StorageFileEndpointRelationalTests(StorageCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task The_uploader_and_managers_get_a_short_presigned_redirect_to_a_private_file()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var file = await company.PutAsync(_factory, StorageAreas.Task, company["ali"].Id);

        foreach (var user in new[] { "ali", "mudur", "patron" })
        {
            var response = await OpenAsync(file.Id, company[user].Token);
            response.StatusCode.Should().Be(HttpStatusCode.Redirect, user);
            response.Headers.Location!.AbsoluteUri.Should().Be($"https://r2.test/private/{file.ObjectKey}?X-Amz-Expires=300&X-Amz-Signature=test");
            response.Headers.CacheControl!.NoStore.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Another_user_and_another_company_get_the_same_not_found()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var other = await StorageTestCompany.CreateAsync(_factory);
        var file = await company.PutAsync(_factory, StorageAreas.Expense, company["ali"].Id);

        var colleague = await OpenAsync(file.Id, company["veli"].Token);
        var stranger = await OpenAsync(file.Id, other["patron"].Token);
        var missing = await OpenAsync(Guid.NewGuid(), company["patron"].Token);

        foreach (var response in new[] { colleague, stranger, missing })
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await response.ReadAsJsonAsync<StorageErrorResponse>()).ErrorCode.Should().Be("STORED_FILE_NOT_FOUND");
        }
    }

    [Fact]
    public async Task A_file_in_the_trash_is_not_served()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var file = await company.PutAsync(_factory, StorageAreas.Task, company["ali"].Id);
        using (var scope = _factory.Services.CreateScope())
            (await scope.ServiceProvider.GetRequiredService<FileStore>().TrashAsync(company.TenantId, file.Id, null, default)).Succeeded.Should().BeTrue();

        (await OpenAsync(file.Id, company["patron"].Token)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_public_file_redirects_to_the_cdn_address()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var file = await company.PutAsync(_factory, StorageAreas.Product, company["ali"].Id);

        var response = await OpenAsync(file.Id, company["ali"].Token);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.AbsoluteUri.Should().Be($"https://img.test/{file.ObjectKey}");
    }

    [Fact]
    public async Task An_area_rule_opens_a_file_to_someone_else()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var file = await company.PutAsync(_factory, StorageAreas.Vehicle, company["ali"].Id);
        using var withRule = _factory.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IStoredFileReadRule>(new EveryoneReadsVehicles())));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/storage/files/{file.Id}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", company["veli"].Token);
        var response = await withRule.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false }).SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task Without_a_token_the_endpoint_is_closed()
    {
        (await _factory.CreateClient().GetAsync($"/api/v1/storage/files/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private Task<HttpResponseMessage> OpenAsync(Guid id, string token) =>
        StorageTestCompany.SendAsync(_factory, HttpMethod.Get, $"/api/v1/storage/files/{id}", null, token, followRedirects: false);

    private sealed class EveryoneReadsVehicles : IStoredFileReadRule
    {
        public string Area => StorageAreas.Vehicle;

        public Task<bool> AllowsAsync(ErpBridge.CentralApi.Data.CentralApiDbContext db, MobileUser user, StoredFile file, CancellationToken ct) => Task.FromResult(true);
    }
}

/// <summary>Without the <c>Storage:*</c> settings the API runs and the store answers 503 (T10).</summary>
public sealed class StorageUnavailableRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public StorageUnavailableRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Without_settings_uploads_and_downloads_answer_storage_unavailable()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<FileStore>();
            store.IsAvailable.Should().BeFalse();
            var result = await store.PutAsync(company.TenantId, StorageAreas.Task, "test", "k", StoredFileVariants.Original, "image/jpeg", FileStoreRelationalTests.Jpeg(10), null, default);
            result.Error!.Status.Should().Be(503);
            result.Error.Code.Should().Be("STORAGE_UNAVAILABLE");
        }

        var id = await company.SeedRowAsync(_factory, StorageAreas.Task, company["ali"].Id);
        var response = await StorageTestCompany.SendAsync(_factory, HttpMethod.Get, $"/api/v1/storage/files/{id}", null, company["ali"].Token, followRedirects: false);
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await response.ReadAsJsonAsync<StorageErrorResponse>()).ErrorCode.Should().Be("STORAGE_UNAVAILABLE");
    }
}
