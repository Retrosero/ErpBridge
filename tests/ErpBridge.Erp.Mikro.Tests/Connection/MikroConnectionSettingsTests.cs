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
}