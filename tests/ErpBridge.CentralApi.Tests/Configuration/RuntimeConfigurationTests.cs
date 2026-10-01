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
    public void Non_test_host_rejects_missing_allowed_origins()
    {
        var configuration = BuildConfiguration(
            "production-signing-key-that-is-long-enough-for-hs256",
            "Host=postgres;Database=erpbridge",
            VaultKey());

        var act = () => Program.ValidateRuntimeConfiguration(configuration, allowTestDefaults: false);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cors:AllowedOrigins*");
    }

    [Fact]
    public void Non_test_host_with_a_public_catalog_requires_the_reverse_proxy_list()
    {
        var catalog = new Dictionary<string, string?> { ["CustomerCatalog:PublicHost"] = "sipariscepte.appsgo.cloud" };

        var withoutProxy = () => Program.ValidateRuntimeConfiguration(ValidConfiguration(catalog), allowTestDefaults: false);
        withoutProxy.Should().Throw<InvalidOperationException>()
            .WithMessage("*CustomerCatalog:PublicHost*ForwardedHeaders:KnownNetworks*");

        catalog["ForwardedHeaders:KnownNetworks:0"] = "10.0.1.0/24";
        var withProxy = () => Program.ValidateRuntimeConfiguration(ValidConfiguration(catalog), allowTestDefaults: false);
        withProxy.Should().NotThrow();

        var withoutCatalog = () => Program.ValidateRuntimeConfiguration(ValidConfiguration(new() { ["CustomerCatalog:PublicHost"] = "" }), allowTestDefaults: false);
        withoutCatalog.Should().NotThrow("a host without the catalog runs as before");
    }

    [Fact]
    public void Test_host_allows_factory_defaults()
    {
        var configuration = BuildConfiguration(signingKey: null);

        var act = () => Program.ValidateRuntimeConfiguration(configuration, allowTestDefaults: true);

        act.Should().NotThrow();
    }

    private static IConfiguration BuildConfiguration(string? signingKey, string? connectionString = null, string? vaultKey = null, string[]? allowedOrigins = null)
    {
        var settings = new Dictionary<string, string?>();
        if (signingKey is not null) settings["Jwt:SigningKey"] = signingKey;
        if (connectionString is not null) settings["ConnectionStrings:CentralApi"] = connectionString;
        if (vaultKey is not null) settings["ApiKeyVault:MasterKey"] = vaultKey;
        if (allowedOrigins is not null)
        {
            for (var index = 0; index < allowedOrigins.Length; index++)
                settings[$"Cors:AllowedOrigins:{index}"] = allowedOrigins[index];
        }
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    private static string VaultKey() => Convert.ToBase64String(new byte[32]);

    /// <summary>Everything a production host needs, plus <paramref name="extra"/>.</summary>
    private static IConfiguration ValidConfiguration(Dictionary<string, string?> extra)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:SigningKey"] = "production-signing-key-that-is-long-enough-for-hs256",
            ["ConnectionStrings:CentralApi"] = "Host=postgres;Database=erpbridge",
            ["ApiKeyVault:MasterKey"] = VaultKey(),
            ["Cors:AllowedOrigins:0"] = "https://admin.example.com",
        };
        foreach (var (key, value) in extra) settings[key] = value;
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }
}
