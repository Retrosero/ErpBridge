using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Webhooks;

/// <summary>
/// Interface used by <c>JobsEndpoints.AckAsync</c> to enqueue webhook
/// deliveries after a job reaches a terminal state. The default
/// implementation (<c>WebhookDispatcher</c>) writes a row per matching
/// endpoint to <c>webhook_deliveries</c> and lets a background service
/// handle the HTTP send + retry loop.
/// </summary>
public interface IWebhookDispatcher
{
    /// <summary>
    /// Schedule a <c>job.succeeded</c> or <c>job.failed</c> delivery for
    /// every active endpoint subscribed to the event for this tenant.
    /// </summary>
    Task EnqueueJobTerminalAsync(Job job, string eventType, CancellationToken ct);
}