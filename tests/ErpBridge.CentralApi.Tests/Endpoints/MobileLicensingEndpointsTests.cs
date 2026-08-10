using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

public sealed class MobileLicensingEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public MobileLicensingEndpointsTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Activate_when_all_device_seats_are_used_returns_conflict()
    {
        var (tenant, _) = await _factory.SeedTenantAsync("DEVICE-SEAT-LIMIT", "Device seat tenant");
        const string activationCode = "ERP-DEVICE-LIMIT";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var storedTenant = await db.Tenants.FindAsync(tenant.Id);
            storedTenant!.DeviceSeatLimit = 1;
            db.MobileDevices.Add(new MobileDevice
            {
                TenantId = tenant.Id,
                InstallationId = "existing-device",
                DisplayName = "Existing device",
                Platform = "android",
                IsActive = true,
            });
            db.DeviceActivationCodes.Add(new DeviceActivationCode
            {
                TenantId = tenant.Id,
                CodeHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(activationCode))).ToLowerInvariant(),
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(10),
            });
            await db.SaveChangesAsync();
        }

        var response = await _factory.CreateClient().PostAsJsonAsync("/api/v1/mobile/activate", new
        {
            code = activationCode,
            installationId = "new-device",
            deviceName = "New device",
            appVersion = "1.0.0",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).Should().Contain("DEVICE_LIMIT_REACHED");
    }
}
