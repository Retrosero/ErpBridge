using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>/api/v1/admin/licenses</c>: create, revoke, list with
/// optional tenant filter. License keys must be unique and start with "LIC-".
/// </summary>
public class AdminLicensesTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminLicensesTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_generates_unique_LicenseKey_starting_with_LIC_()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();

        var first = await client.PostJsonAsync("/api/v1/admin/licenses", new { tenantId = tenant.Id }, token);
        var second = await client.PostJsonAsync("/api/v1/admin/licenses", new { tenantId = tenant.Id }, token);

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.Created);

        var firstLicense = await first.ReadAsJsonAsync<LicenseDto>();
        var secondLicense = await second.ReadAsJsonAsync<LicenseDto>();

        firstLicense.LicenseKey.Should().StartWith("LIC-");
        firstLicense.TenantId.Should().Be(tenant.Id);
        firstLicense.IsActive.Should().BeTrue();

        // Each generated key should be unique (extremely high probability for
        // 32-hex randoms; the test fails if the generator is broken).
        firstLicense.LicenseKey.Should().NotBe(secondLicense.LicenseKey);
    }

    [Fact]
    public async Task Create_tenant_persists_selected_device_limit()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "device-limit-admin@test.local");
        var token = _factory.IssueAdminJwt(admin.Id);

        var create = await client.PostJsonAsync("/api/v1/admin/tenants", new { name = "Device Limit Tenant", maxDeviceCount = 3 }, token);

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenant = await create.ReadAsJsonAsync<TenantDto>();
        tenant!.MaxDeviceCount.Should().Be(3);

        var patch = await client.PatchAsync($"/api/v1/admin/tenants/{tenant.Id}", new { maxDeviceCount = 5 }, token);

        patch.StatusCode.Should().Be(HttpStatusCode.OK);
        (await patch.ReadAsJsonAsync<TenantDto>())!.MaxDeviceCount.Should().Be(5);
    }

    [Fact]
    public async Task Revoke_sets_IsActive_false()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, license) = await _factory.SeedTenantAsync();

        var response = await client.PostJsonAsync($"/api/v1/admin/licenses/{license.Id}/revoke", new { }, token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        db.Licenses.First(l => l.Id == license.Id).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task List_filter_by_tenantId()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);

        var (tenantA, licenseA) = await _factory.SeedTenantAsync(licenseKey: "TENANT-A-LIC");
        var (tenantB, licenseB) = await _factory.SeedTenantAsync(licenseKey: "TENANT-B-LIC");

        // Add another license to tenantA so we can prove the filter narrows to that tenant.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            db.Licenses.Add(new Domain.License
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                LicenseKey = "TENANT-A-LIC-2",
                IsActive = true,
                IssuedAtUtc = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync($"/api/v1/admin/licenses?tenantId={tenantA.Id}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<LicenseDto[]>();
        // No license from tenant B should leak into the tenant A filter result.
        body!.Select(l => l.TenantId).Should().OnlyContain(id => id == tenantA.Id);
        // Both seeded tenant A licenses must be in the result. Other tests in
        // the same factory might add rows, so we use membership assertions.
        body.Select(l => l.LicenseKey).Should().Contain(new[] { licenseA.LicenseKey, "TENANT-A-LIC-2" });
        body.Should().NotContain(l => l.TenantId == tenantB.Id);
    }
}
