using ErpBridge.CentralApi;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace ErpBridge.CentralApi.Tests.Configuration;

public sealed class RuntimeConfigurationTests
{
    [Fact]
    public void Non_test_host_rejects_missing_database_connection()
    {
        var configuration = BuildConfiguration(signingKey: "production-signing-key-that-is-long-enough-for-hs256");

        var act = () => Program.ValidateRuntimeConfiguration(configuration, allowTestDefaults: false);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ConnectionStrings:CentralApi*");
    }

    [Fact]
    public void Non_test_host_rejects_test_signing_key()
    {
        var configuration = BuildConfiguration(TestJwtConstants.TestSigningKey, "Host=postgres;Database=erpbridge", VaultKey());

        var act = () => Program.ValidateRuntimeConfiguration(configuration, allowTestDefaults: false);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*non-test Jwt:SigningKey*");
    }

    [Fact]
    public void Non_test_host_rejects_missing_api_key_vault_key()
    {
        var configuration = BuildConfiguration(
            "production-signing-key-that-is-long-enough-for-hs256",
            "Host=postgres;Database=erpbridge");

        var act = () => Program.ValidateRuntimeConfiguration(configuration, allowTestDefaults: false);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ApiKeyVault:MasterKey*");
    }

    [Fact]
    public void Test_host_allows_factory_defaults()
    {
        var configuration = BuildConfiguration(signingKey: null);

        var act = () => Program.ValidateRuntimeConfiguration(configuration, allowTestDefaults: true);

        act.Should().NotThrow();
    }

    private static IConfiguration BuildConfiguration(string? signingKey, string? connectionString = null, string? vaultKey = null)
    {
        var settings = new Dictionary<string, string?>();
        if (signingKey is not null) settings["Jwt:SigningKey"] = signingKey;
        if (connectionString is not null) settings["ConnectionStrings:CentralApi"] = connectionString;
        if (vaultKey is not null) settings["ApiKeyVault:MasterKey"] = vaultKey;
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    private static string VaultKey() => Convert.ToBase64String(new byte[32]);
}
