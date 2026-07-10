using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.Adapters;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.DependencyInjection;
using ErpBridge.Erp.Mikro.Tests.Fakes;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Erp.Mikro.Writers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErpBridge.Erp.Mikro.Tests.Adapters;

/// <summary>
/// Unit tests for <see cref="MikroAdapter.TestConnectionAsync"/> covering the
/// "configuration missing" soft-failure path. The live connection path is gated
/// by <c>ERPBridge_TestConnection_Live</c> — when the env var is set and a real
/// SQL Server is reachable, the test opens a connection. Otherwise it skips.
/// </summary>
public class MikroAdapterTestConnectionTests
{
    private const string LiveEnvVar = "ERPBridge_TestConnection_Live";

    private static (MikroAdapter adapter, IConfiguration configuration) BuildAdapter(
        params (string Key, string Value)[] mikroOverrides)
    {
        var dict = mikroOverrides.ToDictionary(p => p.Key, p => (string?)p.Value, StringComparer.OrdinalIgnoreCase);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        // Bootstrap Mikro connection settings are not used by TestConnectionAsync
        // (the adapter re-reads IConfiguration on every call), but the constructor
        // and the DI registration still require a non-null instance.
        services.AddErpBridgeMikro(
            new MikroConnectionSettings("unused", "unused", "unused", "unused"),
            configuration);
        var provider = services.BuildServiceProvider();
        var adapter = (MikroAdapter)provider.GetRequiredService<IErpAdapterFactory>().Create(ErpType.Mikro);
        return (adapter, configuration);
    }

    [Fact]
    public async Task TestConnection_returns_failure_with_diagnostic_message_when_section_missing()
    {
        var (adapter, _) = BuildAdapter();

        var result = await adapter.TestConnectionAsync();

        result.Ok.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
        result.Message.Should().Contain("Mikro konfigürasyonu eksik");
        // Password MUST NOT leak into the message even on the failure path.
        result.Message.Should().NotContain("Password=");
        result.Message.Should().NotContain("Pwd=");
    }

    [Fact]
    public async Task TestConnection_returns_failure_when_Server_is_blank()
    {
        var (adapter, _) = BuildAdapter(
            ("Mikro:Server", ""),
            ("Mikro:UserId", "sa"),
            ("Mikro:DatabaseName", "MIKRO16"));

        var result = await adapter.TestConnectionAsync();

        result.Ok.Should().BeFalse();
        result.Message.Should().Contain("Mikro konfigürasyonu eksik");
    }

    [Fact]
    public async Task TestConnection_returns_failure_when_UserId_is_blank()
    {
        var (adapter, _) = BuildAdapter(
            ("Mikro:Server", "MIKROSQL\\MIKRO"),
            ("Mikro:UserId", ""),
            ("Mikro:DatabaseName", "MIKRO16"));

        var result = await adapter.TestConnectionAsync();

        result.Ok.Should().BeFalse();
        result.Message.Should().Contain("Mikro konfigürasyonu eksik");
    }

    [Fact]
    public async Task TestConnection_returns_failure_when_DatabaseName_is_blank()
    {
        var (adapter, _) = BuildAdapter(
            ("Mikro:Server", "MIKROSQL\\MIKRO"),
            ("Mikro:UserId", "sa"),
            ("Mikro:DatabaseName", ""));

        var result = await adapter.TestConnectionAsync();

        result.Ok.Should().BeFalse();
        result.Message.Should().Contain("Mikro konfigürasyonu eksik");
    }

    [Fact]
    public async Task TestConnection_attempts_to_open_a_real_SqlConnection_when_config_is_valid()
    {
        // Gate the live network path behind an env var so CI / Linux builds don't
        // need a SQL Server. When ERPBridge_TestConnection_Live is unset, the
        // test confirms the adapter at least attempts to open — we capture the
        // resulting exception and assert Ok=false plus a masked message.
        var (adapter, _) = BuildAdapter(
            ("Mikro:Server", "MIKROSQL\\MIKRO"),
            ("Mikro:UserId", "sa"),
            ("Mikro:Password", "secret"),
            ("Mikro:DatabaseName", "MIKRO16"));

        var liveEnabled = !string.IsNullOrEmpty(
            Environment.GetEnvironmentVariable(LiveEnvVar));

        if (liveEnabled)
        {
            var ok = await adapter.TestConnectionAsync();
            ok.Ok.Should().BeTrue($"a live SQL Server was reachable — server version: {ok.ServerVersion}");
            return;
        }

        // Without a live SQL Server the call must fail softly — never throw.
        var result = await adapter.TestConnectionAsync();

        result.Ok.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
        // The SqlException message must not echo the password back to the UI.
        result.Message.Should().NotContain("secret");
    }
}