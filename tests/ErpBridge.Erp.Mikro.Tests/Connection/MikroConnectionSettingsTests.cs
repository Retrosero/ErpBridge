using ErpBridge.Erp.Mikro.Connection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace ErpBridge.Erp.Mikro.Tests.Connection;

/// <summary>
/// Unit tests for <see cref="MikroConnectionSettings.FromConfiguration"/> — the
/// configuration bridge the WPF "Bağlantıyı test et" button relies on.
/// </summary>
public class MikroConnectionSettingsTests
{
    private static IConfiguration BuildConfig(params (string Key, string Value)[] pairs)
    {
        var dict = pairs.ToDictionary(p => p.Key, p => (string?)p.Value, StringComparer.OrdinalIgnoreCase);
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public void FromConfiguration_returns_null_when_section_is_missing()
    {
        var config = new ConfigurationBuilder().Build();

        var result = MikroConnectionSettings.FromConfiguration(config);

        result.Should().BeNull();
    }

    [Fact]
    public void FromConfiguration_returns_null_when_Server_is_blank()
    {
        var config = BuildConfig(
            ("Mikro:Server", ""),
            ("Mikro:UserId", "sa"),
            ("Mikro:DatabaseName", "MIKRO16"));

        var result = MikroConnectionSettings.FromConfiguration(config);

        result.Should().BeNull();
    }

    [Fact]
    public void FromConfiguration_returns_null_when_UserId_is_blank()
    {
        var config = BuildConfig(
            ("Mikro:Server", "MIKROSQL\\MIKRO"),
            ("Mikro:UserId", "  "),
            ("Mikro:DatabaseName", "MIKRO16"));

        var result = MikroConnectionSettings.FromConfiguration(config);

        result.Should().BeNull();
    }

    [Fact]
    public void FromConfiguration_returns_null_when_DatabaseName_is_blank()
    {
        var config = BuildConfig(
            ("Mikro:Server", "MIKROSQL\\MIKRO"),
            ("Mikro:UserId", "sa"),
            ("Mikro:DatabaseName", ""));

        var result = MikroConnectionSettings.FromConfiguration(config);

        result.Should().BeNull();
    }

    [Fact]
    public void FromConfiguration_returns_populated_settings_when_all_required_keys_present()
    {
        var config = BuildConfig(
            ("Mikro:Server", "MIKROSQL\\MIKRO"),
            ("Mikro:UserId", "sa"),
            ("Mikro:Password", "secret123"),
            ("Mikro:DatabaseName", "MIKRO16"));

        var result = MikroConnectionSettings.FromConfiguration(config);

        result.Should().NotBeNull();
        result!.Server.Should().Be("MIKROSQL\\MIKRO");
        result.UserId.Should().Be("sa");
        result.Password.Should().Be("secret123");
        result.DatabaseName.Should().Be("MIKRO16");
    }

    [Fact]
    public void FromConfiguration_treats_empty_password_as_trusted_auth()
    {
        var config = BuildConfig(
            ("Mikro:Server", "MIKROSQL\\MIKRO"),
            ("Mikro:UserId", "sa"),
            ("Mikro:DatabaseName", "MIKRO16"));

        var result = MikroConnectionSettings.FromConfiguration(config);

        result.Should().NotBeNull();
        result!.Password.Should().BeEmpty();
    }

    [Fact]
    public void FromConfiguration_trims_surrounding_whitespace()
    {
        var config = BuildConfig(
            ("Mikro:Server", "  MIKROSQL\\MIKRO  "),
            ("Mikro:UserId", "  sa  "),
            ("Mikro:DatabaseName", "  MIKRO16  "));

        var result = MikroConnectionSettings.FromConfiguration(config);

        result.Should().NotBeNull();
        result!.Server.Should().Be("MIKROSQL\\MIKRO");
        result.UserId.Should().Be("sa");
        result.DatabaseName.Should().Be("MIKRO16");
    }

    [Fact]
    public void FromConfiguration_throws_when_configuration_is_null()
    {
        Action act = () => MikroConnectionSettings.FromConfiguration(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ------------------------------------------------------------------------
    // Faz 10 — multi-firm Mikro support: CompanyNo + WarehouseNo parsing.
    // ------------------------------------------------------------------------

    [Fact]
    public void FromConfiguration_parses_CompanyNo_and_WarehouseNo_when_present()
    {
        var config = BuildConfig(
            ("Mikro:Server", "MIKROSQL\\MIKRO"),
            ("Mikro:UserId", "sa"),
            ("Mikro:DatabaseName", "MIKRO16_V16"),
            ("Mikro:CompanyNo", "3"),
            ("Mikro:WarehouseNo", "5"));

        var result = MikroConnectionSettings.FromConfiguration(config);

        result.Should().NotBeNull();
        result!.CompanyNo.Should().Be(3);
        result.WarehouseNo.Should().Be(5);
    }

    [Fact]
    public void FromConfiguration_defaults_CompanyNo_and_WarehouseNo_to_1_when_missing()
    {
        // Older single-firm installations don't carry CompanyNo / WarehouseNo
        // keys in appsettings.json. The defaults must keep the bootstrap path
        // working without forcing the operator to touch the file.
        var config = BuildConfig(
            ("Mikro:Server", "MIKROSQL\\MIKRO"),
            ("Mikro:UserId", "sa"),
            ("Mikro:DatabaseName", "MIKRO16"));

        var result = MikroConnectionSettings.FromConfiguration(config);

        result.Should().NotBeNull();
        result!.CompanyNo.Should().Be(1);
        result.WarehouseNo.Should().Be(1);
    }

    [Fact]
    public void FromConfiguration_falls_back_to_1_when_CompanyNo_is_unparsable()
    {
        var config = BuildConfig(
            ("Mikro:Server", "MIKROSQL\\MIKRO"),
            ("Mikro:UserId", "sa"),
            ("Mikro:DatabaseName", "MIKRO16"),
            ("Mikro:CompanyNo", "abc"),
            ("Mikro:WarehouseNo", "2"));

        var result = MikroConnectionSettings.FromConfiguration(config);

        result.Should().NotBeNull();
        result!.CompanyNo.Should().Be(1, "non-integer CompanyNo must default to 1, not throw");
        result.WarehouseNo.Should().Be(2);
    }

    [Fact]
    public void FromConfiguration_uses_invariant_culture_for_parsing()
    {
        // A Turkish-locale operator must not get his comma-separated number
        // silently re-interpreted. Invariant parsing keeps the value stable.
        var config = BuildConfig(
            ("Mikro:Server", "MIKROSQL\\MIKRO"),
            ("Mikro:UserId", "sa"),
            ("Mikro:DatabaseName", "MIKRO16"),
            ("Mikro:CompanyNo", "  4  "),
            ("Mikro:WarehouseNo", "  2  "));

        var result = MikroConnectionSettings.FromConfiguration(config);

        result.Should().NotBeNull();
        result!.CompanyNo.Should().Be(4);
        result.WarehouseNo.Should().Be(2);
    }
}