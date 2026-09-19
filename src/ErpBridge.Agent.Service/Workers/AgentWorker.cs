using ErpBridge.Agent.Service.Configuration;
using ErpBridge.Core.Jobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Hosts <see cref="AgentJobPump"/> — the agent's inbound half — for the lifetime of the Windows
/// Service.
///
/// <para>The per-job logic (lease → translate → ERP write → ack) lives in
/// <see cref="AgentJobPump"/> in Core, because the WPF agent needs exactly the same loop and that
/// process has no generic host to hang a hosted service off. While it lived here, an operator
/// running only the desktop agent had no inbound path at all: every document the phone sent stayed
/// <c>pending</c> on the server and nothing was ever written to the ERP. This class is the
/// service-side shell; <c>DesktopJobPumpService</c> is the desktop one.</para>
/// </summary>
public sealed class AgentWorker : BackgroundService
{
    private readonly AgentJobPump _pump;
    private readonly AgentJobPumpOptions _options;
    private readonly ILogger<AgentWorker> _logger;

    /// <summary>DI constructor.</summary>
    public AgentWorker(
        AgentJobPump pump,
        IOptions<AgentServiceOptions> options,
        ILogger<AgentWorker> logger)
    {
        _pump = pump ?? throw new ArgumentNullException(nameof(pump));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value.ToJobPumpOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AgentWorker starting ({Cadence}).", AgentJobPump.DescribeCadence(_options));
        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => _pump.RunAsync(_options, stoppingToken);

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AgentWorker stopping; sending final heartbeat.");
        await base.StopAsync(cancellationToken);
    }
}
