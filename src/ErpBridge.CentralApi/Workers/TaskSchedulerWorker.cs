using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Tasks;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Workers;

/// <summary>
/// Runs <see cref="TaskService.RunSchedulerAsync"/> every <see cref="TaskOptions.SchedulerIntervalSeconds"/>:
/// repeating task occurrences, scheduled starts, "bitiş yaklaşıyor" / "gecikti" notifications and the
/// 30-day clean-up (docs/GOAL_GOREVLER.md §3). One CentralApi container is assumed, as for the event hubs;
/// a second replica would only repeat work the conditional flags already make harmless to repeat twice.
/// </summary>
public sealed class TaskSchedulerWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IOptionsMonitor<TaskOptions> _options;
    private readonly ILogger<TaskSchedulerWorker> _logger;

    public TaskSchedulerWorker(IServiceProvider services, IOptionsMonitor<TaskOptions> options, ILogger<TaskSchedulerWorker> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.CurrentValue.SchedulerEnabled)
        {
            _logger.LogInformation("TaskSchedulerWorker disabled by configuration.");
            return;
        }

        // Let migrations and the other start-up work finish first.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
                var tasks = scope.ServiceProvider.GetRequiredService<TaskService>();
                var done = await tasks.RunSchedulerAsync(db, TaskService.NowMs(), stoppingToken).ConfigureAwait(false);
                if (done > 0) _logger.LogInformation("Task scheduler pass did {Count} things.", done);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TaskSchedulerWorker iteration failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(10, _options.CurrentValue.SchedulerIntervalSeconds)), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
