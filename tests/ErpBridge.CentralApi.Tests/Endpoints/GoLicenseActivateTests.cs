using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.GoLicensing;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// <c>POST /api/v1/go/license/activate</c>: a Go key binds to one computer and returns a token
/// signed with the configured ECDSA key; keys of the two products never open each other's doors.
/// </summary>
public class GoLicenseActivateTests : IClassFixture<CentralApiFactory>
{
    private const string Path = "/api/v1/go/license/activate";
    private readonly CentralApiFactory _factory;

    public GoLicenseActivateTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task First_activation_binds_the_machine_and_returns_a_verifiable_token()
    {
        var license = await SeedGoLicenseAsync(expiresAtUtc: DateTimeOffset.UtcNow.AddDays(30));

        var response = await _factory.CreateClient().PostJsonAsync(Path, new { licenseKey = license.LicenseKey, machineId = "machine-a", machineName = "KASA-PC", appVersion = "0.1.0" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<GoLicenseActivateResponse>();
        var claims = VerifyAndRead(body.Token);
        claims.Product.Should().Be("go");
        claims.LicenseId.Should().Be(license.Id);
        claims.MachineId.Should().Be("machine-a");
        claims.ValidUntilUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(3), TimeSpan.FromMinutes(1));
        claims.LicenseExpiresAtUtc.Should().BeCloseTo(license.ExpiresAtUtc!.Value, TimeSpan.FromSeconds(1));
        body.TenantName.Should().Be(claims.TenantName).And.NotBeEmpty();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var installation = await db.GoInstallations.SingleAsync(i => i.LicenseId == license.Id);
        installation.MachineName.Should().Be("KASA-PC");
        installation.AppVersion.Should().Be("0.1.0");
    }

    [Fact]
    public async Task Same_machine_renews_and_another_machine_is_refused()
    {
        var license = await SeedGoLicenseAsync();
        var client = _factory.CreateClient();

        (await client.PostJsonAsync(Path, new { licenseKey = license.LicenseKey, machineId = "machine-a" })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostJsonAsync(Path, new { licenseKey = license.LicenseKey, machineId = "machine-a" })).StatusCode.Should().Be(HttpStatusCode.OK);

        var other = await client.PostJsonAsync(Path, new { licenseKey = license.LicenseKey, machineId = "machine-b" });

        other.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await other.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("DEVICE_LIMIT_REACHED");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.GoInstallations.CountAsync(i => i.LicenseId == license.Id)).Should().Be(1);
    }

    [Fact]
    public async Task Offline_allowance_never_outlives_the_license()
    {
        var expires = DateTimeOffset.UtcNow.AddHours(5);
        var license = await SeedGoLicenseAsync(expiresAtUtc: expires);

        var response = await _factory.CreateClient().PostJsonAsync(Path, new { licenseKey = license.LicenseKey, machineId = "machine-a" });

        var claims = VerifyAndRead((await response.ReadAsJsonAsync<GoLicenseActivateResponse>()).Token);
        claims.ValidUntilUtc.Should().BeCloseTo(expires, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Expired_and_revoked_licenses_answer_410_with_distinct_codes()
    {
        var expired = await SeedGoLicenseAsync(expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(-1));
        var revoked = await SeedGoLicenseAsync(isActive: false);
        var client = _factory.CreateClient();

        var e = await client.PostJsonAsync(Path, new { licenseKey = expired.LicenseKey, machineId = "m" });
        var r = await client.PostJsonAsync(Path, new { licenseKey = revoked.LicenseKey, machineId = "m" });

        e.StatusCode.Should().Be(HttpStatusCode.Gone);
        (await e.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("LICENSE_EXPIRED");
        r.StatusCode.Should().Be(HttpStatusCode.Gone);
        (await r.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("LICENSE_REVOKED");
    }

    [Fact]
    public async Task Keys_of_one_product_do_not_open_the_other()
    {
        var client = _factory.CreateClient();
        var (_, erpLicense) = await _factory.SeedTenantAsync(licenseKey: "LIC-ERP-ONLY", tenantName: "Erp Only Tenant");
        var goLicense = await SeedGoLicenseAsync();

        var goWithErpKey = await client.PostJsonAsync(Path, new { licenseKey = erpLicense.LicenseKey, machineId = "m" });
        var validateWithGoKey = await client.PostJsonAsync("/api/v1/licenses/validate", new { licenseKey = goLicense.LicenseKey });
        var registerWithGoKey = await client.PostJsonAsync("/api/v1/agents/register", new { licenseKey = goLicense.LicenseKey, machineId = "m" });

        goWithErpKey.StatusCode.Should().Be(HttpStatusCode.NotFound);
        validateWithGoKey.StatusCode.Should().Be(HttpStatusCode.NotFound);
        registerWithGoKey.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Token_carries_only_the_tenants_go_modules()
    {
        var license = await SeedGoLicenseAsync();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            db.TenantModules.Add(new TenantModule { TenantId = license.TenantId, ModuleKey = "go_ai", EnabledAtUtc = DateTimeOffset.UtcNow });
            db.TenantModules.Add(new TenantModule { TenantId = license.TenantId, ModuleKey = TenantModules.XmlImport, EnabledAtUtc = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
        }

        var response = await _factory.CreateClient().PostJsonAsync(Path, new { licenseKey = license.LicenseKey, machineId = "m" });

        VerifyAndRead((await response.ReadAsJsonAsync<GoLicenseActivateResponse>()).Token).Modules.Should().Equal("go_ai");
    }

    [Fact]
    public async Task Missing_fields_answer_400()
    {
        var client = _factory.CreateClient();

        (await client.PostJsonAsync(Path, new { licenseKey = "", machineId = "m" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PostJsonAsync(Path, new { licenseKey = "GO-x", machineId = "" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PostJsonAsync(Path, new { licenseKey = "GO-x", machineId = new string('a', 129) })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Without_a_signing_key_activation_answers_503()
    {
        using var host = _factory.WithWebHostBuilder(b => b.UseSetting("GoLicense:SigningKey", ""));
        var license = await SeedGoLicenseAsync();

        var response = await host.CreateClient().PostJsonAsync(Path, new { licenseKey = license.LicenseKey, machineId = "m" });

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("GO_LICENSING_UNAVAILABLE");
    }

    private async Task<License> SeedGoLicenseAsync(DateTimeOffset? expiresAtUtc = null, bool isActive = true)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Go Tenant " + Guid.NewGuid().ToString("N")[..8] };
        var license = new License
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            LicenseKey = "GO-" + Guid.NewGuid().ToString("N"),
            Product = LicenseProducts.Go,
            IsActive = isActive,
            IssuedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = expiresAtUtc,
        };
        db.Tenants.Add(tenant);
        db.Licenses.Add(license);
        await db.SaveChangesAsync();
        return license;
    }

    /// <summary>Verifies the token the way the Go app does and returns its claims.</summary>
    internal static GoLicenseClaims VerifyAndRead(string token)
    {
        var parts = token.Split('.');
        parts.Should().HaveCount(2);
        var payload = FromBase64Url(parts[0]);
        var signature = FromBase64Url(parts[1]);

        using var key = ECDsa.Create();
        key.ImportPkcs8PrivateKey(Convert.FromBase64String(CentralApiFactory.TestGoSigningKey), out _);
        using var publicOnly = ECDsa.Create();
        publicOnly.ImportSubjectPublicKeyInfo(key.ExportSubjectPublicKeyInfo(), out _);
        publicOnly.VerifyData(payload, signature, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation)
            .Should().BeTrue("the token must be signed with the configured Go key");

        var claims = JsonSerializer.Deserialize<GoLicenseClaims>(Encoding.UTF8.GetString(payload))!;
        claims.KeyId.Should().Be(GoLicenseSigner.ComputeKeyId(key.ExportSubjectPublicKeyInfo()));
        return claims;
    }

    private static byte[] FromBase64Url(string value)
    {
        var s = value.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(s.PadRight(s.Length + (4 - s.Length % 4) % 4, '='));
    }
}
