namespace ErpBridge.CentralApi.LogCenter;

/// <summary>
/// Groups WARN+ events stored without a group — the 90 days the L0 migration copied from the legacy telemetry
/// table — shortly after start-up, in small batches, so the error-group view covers that history. Once done the
/// first query finds nothing and the worker exits; it is safe on every start.
/// </summary>
public sealed class LogGroupBackfillWorker : BackgroundService
{
    private const int BatchSize = 500;
    private static readonly TimeSpan StartDelay = TimeSpan.FromSeconds(30);
    private readonly IServiceScopeFactory _scopes;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LogGroupBackfillWorker> _logger;

    public LogGroupBackfillWorker(IServiceScopeFactory scopes, IConfiguration configuration, ILogger<LogGroupBackfillWorker> logger)
    {
        _scopes = scopes;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.GetValue("LogRetention:Enabled", true)) return;
        try
        {
            await Task.Delay(StartDelay, stoppingToken);
            var total = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopes.CreateScope();
                var grouped = await scope.ServiceProvider.GetRequiredService<ILogEventWriter>().BackfillGroupsAsync(BatchSize, stoppingToken);
                if (grouped == 0) break;
                total += grouped;
            }
            if (total > 0) _logger.LogInformation("Grouped {Count} stored warning+ log events that had no error group.", total);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            // Stop instead of spinning on a batch that keeps failing; the next start tries again.
            _logger.LogError(ex, "Backfilling log error groups failed.");
        }
    }
}
