using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.Adapters;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.Erp.Mikro.Tests.Adapters;

/// <summary>
/// Guards the contract that <see cref="MikroAdapter.ChangeLog"/> never throws.
///
/// <para><c>ErpChangeLogSyncService.RunOnceAsync</c> dereferences the property
/// <b>outside</b> its try block, and <c>BootstrapWorker</c> catches around the
/// whole iteration — including the snapshot-delta pass that runs after the
/// change-log pass. A throw here therefore did not merely disable change-log
/// sync: it aborted the entire iteration, so the affected database also stopped
/// receiving bootstrap refreshes.</para>
/// </summary>
public class MikroAdapterChangeLogTests
{
    private static MikroAdapter BuildAdapter()
    {
        // Point at a server that cannot resolve so the version probe fails fast
        // rather than reaching any machine on the developer's network.
        var settings = new MikroConnectionSettings(
            "unresolvable.invalid", "sa", "unused", "MikroDB_V15_TEST");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Mikro:Server"] = "unresolvable.invalid",
                ["Mikro:UserId"] = "sa",
                ["Mikro:Password"] = "unused",
                ["Mikro:DatabaseName"] = "MikroDB_V15_TEST",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddErpBridgeMikro(settings, configuration);
        var provider = services.BuildServiceProvider();
        return (MikroAdapter)provider.GetRequiredService<IErpAdapterFactory>().Create(ErpType.Mikro);
    }

    [Fact]
    public void ChangeLog_returns_null_instead_of_throwing_when_the_version_probe_fails()
    {
        // MikroVersionDetector.DetectAsync opens the connection with no
        // try/catch, so an unreachable server surfaces as a SqlException. The
        // property used to let that escape (and Lazy<T> cached the exception,
        // so a single blip disabled sync until the process restarted).
        var adapter = BuildAdapter();

        var read = () => adapter.ChangeLog;

        read.Should().NotThrow("a throw aborts the whole BootstrapWorker iteration, snapshot refresh included");
        read().Should().BeNull();

        // A failed probe is transient, not a verdict: a second read must
        // re-probe rather than serve a cached null for the process's life.
        read.Should().NotThrow();
        read().Should().BeNull();
    }
}
