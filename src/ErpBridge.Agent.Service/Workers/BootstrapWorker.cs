using ErpBridge.Core.Stores;
using ErpBridge.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Periodic background worker that drives <see cref="IBootstrapSyncService"/>.
/// One iteration = one full bootstrap cycle (read Mikro → push central API).
/// The interval is <see cref="AgentConstants.DefaultBootstrapPushIntervalMinutes"/>
/// (60 minutes by default) so the central API keeps a fresh reference-data
/// snapshot without flooding the agent. The first iteration is delayed
/// <see cref="FirstRunDelaySeconds"/> seconds so the service can finish booting
/// (DB migrations, config load, Mikro connection pool warmup) before the first
/// push is attempted.
/// </summary>
/// <remarks>
/// The worker is intentionally lightweight: the actual retry / checkpoint /
/// orchestration logic lives in <see cref="IBootstrapSyncService"/>. The
/// worker only (a) opens a DI scope per iteration (because BootstrapSyncService
/// may eventually be scoped once typed payload readers land) and (b) catches
/// unexpected throws so a programmer bug cannot kill the loop.
/// </remarks>
public sealed class BootstrapWorker : BackgroundService
{
    private static readonly TimeSpan FirstRunDelay = TimeSpan.FromSeconds(30);

    private readonly IServiceProvider _services;
    private readonly ILogger<BootstrapWorker> _logger;
    private readonly TimeSpan _interval;

    /// <summary>
    /// Build the worker. <paramref name="services"/> is the root provider —
    /// each iteration creates a fresh scope because <see cref="IBootstrapSyncService"/>
    /// is registered as a singleton but future collaborators (typed readers,
    /// mapping store) are scoped.
    /// </summary>
    public BootstrapWorker(
        IServiceProvider services,
        ILogger<BootstrapWorker> logger)
        : this(services, logger, TimeSpan.FromMinutes(AgentConstants.DefaultBootstrapPushIntervalMinutes))
    {
    }

    /// <summary>
    /// Test seam — explicitly set the interval so unit tests can drive the
    /// loop in milliseconds.
    /// </summary>
    public BootstrapWorker(
        IServiceProvider services,
        ILogger<BootstrapWorker> logger,
        TimeSpan interval)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be positive.");
        }
        _interval = interval;
    }

    /// <inheritdoc />
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "BootstrapWorker starting (interval = {IntervalMinutes} min, first run delayed {FirstDelay}s).",
            _interval.TotalMinutes, FirstRunDelay.TotalSeconds);
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Defer the first push so the service can finish booting (DB
        // migrations, config load, Mikro connection warmup) without the
        // bootstrap push competing for resources on the first tick.
        try
        {
            await Task.Delay(FirstRunDelay, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunSingleIterationAsync(stoppingToken).ConfigureAwait(false);

            try
            {
                await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("BootstrapWorker stopped.");
    }

    /// <summary>
    /// Open a DI scope, resolve <see cref="IBootstrapSyncService"/>, and run
    /// a single cycle. Surfaces every result via the logger so the operator
    /// can correlate log lines with the corresponding checkpoint.
    /// </summary>
    private async Task RunSingleIterationAsync(CancellationToken stoppingToken)
    {
        BootstrapSyncResult result;
        try
        {
            using var scope = _services.CreateScope();
            var sync = scope.ServiceProvider.GetRequiredService<IBootstrapSyncService>();
            result = await sync.RunOnceAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // graceful shutdown
            return;
        }
        catch (Exception ex)
        {
            // The orchestrator is supposed to return a failed result for every
            // known business error. Anything escaping here is a programmer
            // bug — log loudly and keep the loop alive.
            _logger.LogError(ex, "BootstrapWorker iteration crashed unexpectedly.");
            return;
        }

        if (result.Success)
        {
            if (result.CustomersCount == 0 && result.StocksCount == 0 && result.PricesCount == 0
                && result.InventoryCount == 0 && result.OpenOrdersCount == 0
                && result.CashAndBankCount == 0 && result.LookupsCount == 0
                && result.CustomerAddressesCount == 0 && result.CustomerContactsCount == 0
                && result.BarcodesCount == 0 && result.SalesConditionsCount == 0)
            {
                // The success row of all-zeros is the "skipped" path: the
                // idempotency window is still active. No new push — log at
                // Debug to avoid flooding the operator's log.
                _logger.LogDebug(
                    "Bootstrap sync skipped (idempotency window active).");
                return;
            }

            _logger.LogInformation(
                "Bootstrap sync completed: ok={Ok} customers={C} stocks={S} prices={P} inventory={I} openOrders={O} cashAndBank={CB} lookups={L} duration={D}ms",
                result.Success,
                result.CustomersCount, result.StocksCount, result.PricesCount,
                result.InventoryCount, result.OpenOrdersCount, result.CashAndBankCount,
                result.LookupsCount, result.DurationMs);
        }
        else
        {
            _logger.LogWarning(
                "Bootstrap sync failed: code={Code} message={Message} duration={D}ms",
                result.ErrorCode, result.ErrorMessage, result.DurationMs);
        }
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BootstrapWorker stopping; cancelling in-flight iteration.");
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
