using ErpBridge.Core;
using ErpBridge.Core.Jobs;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ErpBridge.Core.Tests.Jobs;

/// <summary>
/// <c>AddErpBridgeCore</c> is what both agent hosts call. The job pump belongs to it, not to the
/// Windows service's composition root: while the service registered it alone, the desktop agent had
/// no inbound path and the phone's documents stayed pending on the server for ever.
/// </summary>
public class CoreServiceRegistrationTests
{
    /// <summary>The collaborators an agent host supplies — LocalStore, RemoteApi and the ERP adapter module.</summary>
    private static ServiceProvider BuildHostContainer()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton(Mock.Of<IRemoteApiClient>());
        services.AddSingleton(Mock.Of<ILocalQueueStore>());
        services.AddSingleton(Mock.Of<IAgentConfigStore>());
        services.AddSingleton(Mock.Of<IErpAdapterFactory>());
        services.AddErpBridgeCore();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void An_agent_host_can_resolve_the_job_pump()
    {
        using var provider = BuildHostContainer();

        provider.GetService<AgentJobPump>().Should().NotBeNull(
            "both the Windows service and the desktop agent drive the same pump");
    }

    [Fact]
    public void The_pump_is_one_instance_per_process()
    {
        using var provider = BuildHostContainer();

        provider.GetRequiredService<AgentJobPump>().Should().BeSameAs(provider.GetRequiredService<AgentJobPump>());
    }

    [Fact]
    public void The_sales_order_payload_deserializer_comes_with_it()
    {
        using var provider = BuildHostContainer();

        provider.GetService<SalesOrderPayloadDeserializer>().Should().NotBeNull();
    }
}
