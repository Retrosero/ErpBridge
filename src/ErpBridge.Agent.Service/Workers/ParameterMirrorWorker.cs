using ErpBridge.Core.Parameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Keeps each company's Mikro parameter table in line with the centre (P3b).
///
/// Periodic rather than event-driven: the mirror is idempotent — it makes the table match a
/// complete desired state — so a missed tick costs a delay, never correctness, and the agent does
/// not need a second channel to be told something changed.
/// </summary>
public sealed class ParameterMirrorWorker(
    IServiceScopeFactory scopes,
    ILogger<ParameterMirrorWorker> logger) : BackgroundService
{
    /// <summary>
    /// How often a run is attempted. Parameters change when a person changes them, which is rare
    /// and never urgent to the second; polling harder would ask a customer's SQL Server for
    /// nothing many times an hour.
    /// </summary>
    internal static readonly TimeSpan Period = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Period);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnceAsync(stoppingToken).ConfigureAwait(false);

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                {
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopes.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ParameterMirrorService>();

            var result = await service.RunAsync(ct).ConfigureAwait(false);

            if (result != ParameterMirrorService.RunResult.Nothing)
            {
                logger.LogDebug(
                    "Parameter mirror: {Mirrored} mirrored, {Skipped} skipped, {Failed} failed.",
                    result.Mirrored, result.Skipped, result.Failed);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Shutting down.
        }
        catch (Exception ex)
        {
            // A worker that dies on one bad run stops mirroring until someone restarts the
            // service, and nobody watches a service that looks like it is running.
            logger.LogError(ex, "Parameter mirror run failed; will try again in {Period}.", Period);
        }
    }
}
