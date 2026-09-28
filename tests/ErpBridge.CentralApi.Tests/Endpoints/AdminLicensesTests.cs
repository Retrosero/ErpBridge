using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

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
    public async Task Create_with_a_non_UTC_expiry_normalises_the_offset_to_zero()
    {
        // The admin panel's date picker sends a DateTimeOffset carrying the
        // browser's local offset (e.g. +03:00). PostgreSQL's `timestamp with
        // time zone` only accepts offset 0, so the endpoint must normalise —
        // otherwise Npgsql throws and the request 500s. (The in-memory test
        // provider does not enforce that, so this asserts the normalisation
        // directly rather than relying on a provider error.)
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();

        var localExpiry = new DateTimeOffset(2027, 1, 1, 12, 0, 0, TimeSpan.FromHours(3));
        var response = await client.PostJsonAsync(
            "/api/v1/admin/licenses",
            new { tenantId = tenant.Id, expiresAtUtc = localExpiry },
            token);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var license = await response.ReadAsJsonAsync<LicenseDto>();
        license.ExpiresAtUtc.Should().NotBeNull();
        license.ExpiresAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
        license.ExpiresAtUtc.Value.Should().Be(localExpiry.ToUniversalTime());
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

    [Fact]
    public async Task Create_defaults_to_erpbridge_and_issues_GO_keys_for_go()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "go-product-admin@test.local");
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "GO-PRODUCT-SEED", tenantName: "Go Product Tenant");

        var erp = await (await client.PostJsonAsync("/api/v1/admin/licenses", new { tenantId = tenant.Id }, token)).ReadAsJsonAsync<LicenseDto>();
        var go = await (await client.PostJsonAsync("/api/v1/admin/licenses", new { tenantId = tenant.Id, product = "Go" }, token)).ReadAsJsonAsync<LicenseDto>();
        var unknown = await client.PostJsonAsync("/api/v1/admin/licenses", new { tenantId = tenant.Id, product = "other" }, token);

        erp.Product.Should().Be("erpbridge");
        erp.LicenseKey.Should().StartWith("LIC-");
        go.Product.Should().Be("go");
        go.LicenseKey.Should().StartWith("GO-");
        unknown.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await unknown.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("UNKNOWN_PRODUCT");
    }

    [Fact]
    public async Task Go_license_shows_its_computer_and_can_be_released_and_renewed()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "go-release-admin@test.local");
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "GO-RELEASE-SEED", tenantName: "Go Release Tenant");
        var go = await (await client.PostJsonAsync("/api/v1/admin/licenses", new { tenantId = tenant.Id, product = "go" }, token)).ReadAsJsonAsync<LicenseDto>();
        (await client.PostJsonAsync("/api/v1/go/license/activate", new { licenseKey = go.LicenseKey, machineId = "pc-1", machineName = "DEPO-PC" })).StatusCode.Should().Be(HttpStatusCode.OK);

        var listed = (await (await client.GetAsync($"/api/v1/admin/licenses?tenantId={tenant.Id}", token)).ReadAsJsonAsync<LicenseDto[]>())!.Single(l => l.Id == go.Id);
        listed.GoInstallation.Should().NotBeNull();
        listed.GoInstallation!.MachineName.Should().Be("DEPO-PC");

        var newEnd = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.FromHours(3));
        var renewed = await client.PutJsonAsync($"/api/v1/admin/licenses/{go.Id}/expiry", new { expiresAtUtc = newEnd }, token);
        renewed.StatusCode.Should().Be(HttpStatusCode.OK);
        (await renewed.ReadAsJsonAsync<LicenseDto>()).ExpiresAtUtc.Should().Be(newEnd.ToUniversalTime());

        (await client.PostJsonAsync($"/api/v1/admin/licenses/{go.Id}/go-installation/release", new { }, token)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostJsonAsync($"/api/v1/admin/licenses/{go.Id}/go-installation/release", new { }, token)).StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Released: another computer may now take the license.
        (await client.PostJsonAsync("/api/v1/go/license/activate", new { licenseKey = go.LicenseKey, machineId = "pc-2" })).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Go_modules_replace_only_the_companys_go_rows_and_reach_every_Go_license_and_the_token()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "go-modules-admin@test.local");
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, erpLicense) = await _factory.SeedTenantAsync(licenseKey: "GO-MODULES-SEED", tenantName: "Go Modules Tenant");
        var go = await (await client.PostJsonAsync("/api/v1/admin/licenses", new { tenantId = tenant.Id, product = "go" }, token)).ReadAsJsonAsync<LicenseDto>();
        go.GoModules.Should().BeEmpty("a Go license always reports its company's modules, even none");
        var enabledEarlier = DateTimeOffset.UtcNow.AddDays(-10);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            db.TenantModules.Add(new TenantModule { TenantId = tenant.Id, ModuleKey = TenantModules.XmlImport, EnabledAtUtc = enabledEarlier, EnabledBy = "phone-sales@test.local" });
            db.TenantModules.Add(new TenantModule { TenantId = tenant.Id, ModuleKey = TenantModules.GoErp, EnabledAtUtc = enabledEarlier });
            db.TenantModules.Add(new TenantModule { TenantId = tenant.Id, ModuleKey = TenantModules.GoAi, EnabledAtUtc = enabledEarlier, EnabledBy = "first@test.local" });
            await db.SaveChangesAsync();
        }

        var response = await client.PutJsonAsync($"/api/v1/admin/licenses/{go.Id}/go-modules", new { modules = new[] { "GO_REPORTS", " go_ai ", "go_ai" } }, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.ReadAsJsonAsync<LicenseDto>();
        updated.Id.Should().Be(go.Id);
        updated.GoModules.Should().Equal("go_ai", "go_reports");
        using (var scope = _factory.Services.CreateScope())
        {
            var rows = await scope.ServiceProvider.GetRequiredService<CentralApiDbContext>().TenantModules.AsNoTracking()
                .Where(m => m.TenantId == tenant.Id).ToListAsync();
            rows.Select(m => m.ModuleKey).Should().BeEquivalentTo(new[] { "xml_import", "go_ai", "go_reports" },"go_erp was switched off and the phone add-on stays");
            rows.Single(m => m.ModuleKey == "xml_import").EnabledBy.Should().Be("phone-sales@test.local");
            var kept = rows.Single(m => m.ModuleKey == "go_ai");
            kept.EnabledBy.Should().Be("first@test.local", "a module already on keeps who switched it on");
            kept.EnabledAtUtc.Should().BeCloseTo(enabledEarlier, TimeSpan.FromSeconds(1));
            rows.Single(m => m.ModuleKey == "go_reports").EnabledBy.Should().Be("go-modules-admin@test.local");
        }

        // Modules belong to the company: a second Go license shows the same set at once, the ErpBridge license none.
        var second = await (await client.PostJsonAsync("/api/v1/admin/licenses", new { tenantId = tenant.Id, product = "go" }, token)).ReadAsJsonAsync<LicenseDto>();
        second.GoModules.Should().Equal("go_ai", "go_reports");
        var listResponse = await client.GetAsync($"/api/v1/admin/licenses?tenantId={tenant.Id}", token);
        var listed = (await listResponse.ReadAsJsonAsync<LicenseDto[]>())!;
        listed.Where(l => l.Product == "go").Should().HaveCount(2).And.OnlyContain(l => l.GoModules!.SequenceEqual(new[] { "go_ai", "go_reports" }));
        listed.Single(l => l.Id == erpLicense.Id).GoModules.Should().BeNull();
        System.Text.RegularExpressions.Regex.Matches(await listResponse.Content.ReadAsStringAsync(), "\"goModules\"")
            .Should().HaveCount(2, "the field is omitted for the ErpBridge license");

        var activated = await client.PostJsonAsync("/api/v1/go/license/activate", new { licenseKey = go.LicenseKey, machineId = "pc-modules" });
        activated.StatusCode.Should().Be(HttpStatusCode.OK);
        GoLicenseActivateTests.VerifyAndRead((await activated.ReadAsJsonAsync<GoLicenseActivateResponse>()).Token).Modules.Should().Equal("go_ai", "go_reports");

        var cleared = await client.PutJsonAsync($"/api/v1/admin/licenses/{second.Id}/go-modules", new { modules = Array.Empty<string>() }, token);
        (await cleared.ReadAsJsonAsync<LicenseDto>()).GoModules.Should().BeEmpty();
        using (var scope = _factory.Services.CreateScope())
        {
            (await scope.ServiceProvider.GetRequiredService<CentralApiDbContext>().TenantModules.AsNoTracking()
                .Where(m => m.TenantId == tenant.Id).Select(m => m.ModuleKey).ToListAsync()).Should().Equal("xml_import");
        }
    }

    [Fact]
    public async Task Go_modules_refuse_unknown_keys_ErpBridge_licenses_missing_licenses_and_a_missing_set()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "go-modules-refuse-admin@test.local");
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, erpLicense) = await _factory.SeedTenantAsync(licenseKey: "GO-MODULES-REFUSE-SEED", tenantName: "Go Modules Refuse Tenant");
        var go = await (await client.PostJsonAsync("/api/v1/admin/licenses", new { tenantId = tenant.Id, product = "go" }, token)).ReadAsJsonAsync<LicenseDto>();
        (await client.PutJsonAsync($"/api/v1/admin/licenses/{go.Id}/go-modules", new { modules = new[] { "go_erp" } }, token)).StatusCode.Should().Be(HttpStatusCode.OK);

        foreach (var modules in new[] { new[] { "go_ai", "go_barcode" }, new[] { TenantModules.XmlImport }, new[] { " " } })
        {
            var unknown = await client.PutJsonAsync($"/api/v1/admin/licenses/{go.Id}/go-modules", new { modules }, token);
            unknown.StatusCode.Should().Be(HttpStatusCode.BadRequest, string.Join(",", modules));
            (await unknown.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("UNKNOWN_MODULE");
        }

        var notGo = await client.PutJsonAsync($"/api/v1/admin/licenses/{erpLicense.Id}/go-modules", new { modules = new[] { "go_ai" } }, token);
        notGo.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await notGo.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("NOT_GO_LICENSE");

        var missing = await client.PutJsonAsync($"/api/v1/admin/licenses/{Guid.NewGuid()}/go-modules", new { modules = new[] { "go_ai" } }, token);
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await missing.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("LICENSE_NOT_FOUND");

        (await client.PutJsonAsync($"/api/v1/admin/licenses/{go.Id}/go-modules", new { }, token)).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var scope = _factory.Services.CreateScope();
        (await scope.ServiceProvider.GetRequiredService<CentralApiDbContext>().TenantModules.AsNoTracking()
            .Where(m => m.TenantId == tenant.Id).Select(m => m.ModuleKey).ToListAsync()).Should().Equal(["go_erp"], "a refused set changes nothing");
    }
}
