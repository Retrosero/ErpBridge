using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Webhooks;

/// <summary>
/// Default <see cref="IWebhookDispatcher"/>: when a job reaches a terminal
/// state, find every active endpoint subscribed to that event and create a
/// <see cref="WebhookDelivery"/> row per endpoint. The actual HTTP send +
/// retry loop lives in <see cref="WebhookDispatcherWorker"/>.
/// </summary>
public sealed class WebhookDispatcher : IWebhookDispatcher
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly CentralApiDbContext _db;

    public WebhookDispatcher(CentralApiDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <inheritdoc />
    public async Task EnqueueJobTerminalAsync(Job job, string eventType, CancellationToken ct)
    {
        // Pull active endpoints for the tenant. We don't filter by
        // SubscribedEvents here — empty array means "all events", and any
        // concrete entry must match.
        var endpoints = await _db.WebhookEndpoints
            .Where(w => w.TenantId == job.TenantId && w.IsActive)
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var ep in endpoints)
        {
            if (ep.SubscribedEvents.Length > 0
                && !ep.SubscribedEvents.Contains(eventType, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var payload = new
            {
                id = Guid.NewGuid(),
                type = eventType,
                tenantId = job.TenantId,
                jobId = job.Id,
                externalId = job.ExternalId,
                documentType = job.DocumentType,
                status = job.Status.ToString(),
                completedAtUtc = job.CompletedAtUtc,
            };

            var row = new WebhookDelivery
            {
                Id = Guid.NewGuid(),
                EndpointId = ep.Id,
                TenantId = ep.TenantId,
                EventType = eventType,
                JobId = job.Id,
                PayloadJson = JsonSerializer.Serialize(payload, Json),
                Status = WebhookDeliveryStatus.Pending,
                CreatedAtUtc = DateTimeOffset.UtcNow,
            };
            _db.WebhookDeliveries.Add(row);
        }

        // SaveChanges only if we actually wrote something — otherwise an
        // empty ack would still touch the DB. The SaveChanges call returns
        // 0 in that case but it's free to skip.
        if (_db.ChangeTracker.HasChanges())
            await _db.SaveChangesAsync(ct);
    }
}