using System.Reflection;
using ErpBridge.Agent.Logging;
using ErpBridge.Core.Logging;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Log Merkezi L3d: the service says when it started and when it is stopping, and turns a crash that no catch
/// block saw into a Log Centre event. Without this, a service that dies on an unobserved task or a startup
/// failure looks — from the panel — exactly like one that was never installed.
/// </summary>
public sealed class AgentLifecycleWorker : BackgroundService
{
    private readonly IAgentLogReporter _reporter;
    private readonly IAgentConfigStore _configStore;
    private readonly AgentLogUploader _uploader;
    private readonly ILogger<AgentLifecycleWorker> _logger;

    public AgentLifecycleWorker(
        IAgentLogReporter reporter,
        IAgentConfigStore configStore,
        AgentLogUploader uploader,
        ILogger<AgentLifecycleWorker> logger)
    {
        _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
        _uploader = uploader ?? throw new ArgumentNullException(nameof(uploader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>The version the panel shows for this agent.</summary>
    public static string Version => Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";

    /// <summary>
    /// Hooks the two "nobody caught it" events. Public so the host can install them before the container is
    /// built: a failure during startup is exactly the one worth reporting.
    /// </summary>
    public static void HookUnhandledExceptions(Func<IAgentLogReporter?> reporter, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(reporter);
        ArgumentNullException.ThrowIfNull(logger);

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            using (Handled(logger))
                logger.LogCritical(exception, "The agent is terminating on an unhandled exception (terminating={Terminating}).", args.IsTerminating);
            // Waited on deliberately: the process is going down and the event has to reach the queue first.
            reporter()?.ReportAsync("FATAL", "SERVICE_UNHANDLED_EXCEPTION", "host.unhandled",
                exception?.Message ?? "Unhandled exception", exception,
                new Dictionary<string, object?> { ["terminating"] = args.IsTerminating, ["version"] = Version })
                .GetAwaiter().GetResult();
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            using (Handled(logger))
                logger.LogError(args.Exception, "A background task failed and nobody observed it.");
            args.SetObserved();
            reporter()?.ReportAsync("ERROR", "SERVICE_UNOBSERVED_TASK", "host.unobserved",
                args.Exception.Message, args.Exception,
                new Dictionary<string, object?> { ["version"] = Version })
                .GetAwaiter().GetResult();
        };
    }

    /// <summary>Marks the local line as already reported, so the Serilog sink does not queue it a second time.</summary>
    private static IDisposable? Handled(ILogger logger) =>
        logger.BeginScope(new Dictionary<string, object> { [AgentLogCentreSink.HandledProperty] = true });

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = await Safe(() => _configStore.LoadAsync(stoppingToken)).ConfigureAwait(false);
        await _reporter.ReportAsync("INFO", "AGENT_STARTED", "host.start", $"Agent {Version} started.", null,
            new Dictionary<string, object?>
            {
                ["version"] = Version,
                ["hostKind"] = "service",
                ["erpKind"] = config?.ErpType.ToString() ?? ErpType.Mikro.ToString(),
                ["erpDatabase"] = config?.ErpDatabaseName,
                ["machine"] = Environment.MachineName,
            }, ct: stoppingToken).ConfigureAwait(false);
        _logger.LogInformation("Agent {Version} started on {Machine}.", Version, Environment.MachineName);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Stopping: report it and give the queue one last chance to drain before the process exits.
            using var flush = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _reporter.ReportAsync("INFO", "AGENT_STOPPING", "host.stop", $"Agent {Version} is stopping.", null,
                new Dictionary<string, object?> { ["version"] = Version, ["hostKind"] = "service" }, ct: flush.Token).ConfigureAwait(false);
            await _uploader.FlushAsync(flush.Token).ConfigureAwait(false);
        }
    }

    private static async Task<T?> Safe<T>(Func<Task<T?>> read)
    {
        try
        {
            return await read().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // The start event matters more than the detail it carries.
            return default;
        }
    }
}
