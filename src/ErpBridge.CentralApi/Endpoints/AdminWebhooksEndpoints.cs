using System.Security.Cryptography;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Webhooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/admin/webhooks</c>: list, create, patch, delete, deliveries.
/// Admin-only. The signing secret is returned in cleartext exactly once at
/// creation; subsequent reads expose only the prefix.
/// </summary>
public static class AdminWebhooksEndpoints
{
    private const int SigningSecretRandomBytes = 32; // 64 hex chars after whsec_ prefix

    public static IEndpointRouteBuilder MapAdminWebhooksEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/webhooks")
            .WithTags("Admin/Webhooks")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/", ListAsync)
            .WithName("AdminWebhooksList")
            .Produces<WebhookEndpointDto[]>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetAsync)
            .WithName("AdminWebhooksGet")
            .Produces<WebhookEndpointDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("AdminWebhooksCreate")
            .Produces<WebhookEndpointCreatedDto>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}", PatchAsync)
            .WithName("AdminWebhooksPatch")
            .Produces<WebhookEndpointDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("AdminWebhooksDelete")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/deliveries", DeliveriesAsync)
            .WithName("AdminWebhooksDeliveries")
            .Produces<WebhookDeliveryDto[]>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        return routes;
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] Guid? tenantId,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var query = db.WebhookEndpoints.AsNoTracking();
        if (tenantId.HasValue) query = query.Where(w => w.TenantId == tenantId.Value);
        var rows = await query.OrderByDescending(w => w.CreatedAtUtc).ToListAsync(ct);
        return JsonResults.Ok(rows.Select(ToDto).ToArray());
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var row = await db.WebhookEndpoints.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, ct);
        if (row is null)
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "WEBHOOK_NOT_FOUND", Message = "Webhook endpoint not found." });
        return JsonResults.Ok(ToDto(row));
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateWebhookEndpointRequest body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null || body.TenantId == Guid.Empty)
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_TENANT", Message = "tenantId is required." });
        if (string.IsNullOrWhiteSpace(body.Name))
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_NAME", Message = "name is required." });
        if (string.IsNullOrWhiteSpace(body.Url))
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_URL", Message = "url is required." });
        if (!WebhookTargetValidator.TryParsePublicHttpsUri(body.Url, out _, out var targetError))
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "INVALID_URL", Message = targetError! });
        }

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == body.TenantId, ct);
        if (tenant is null || !tenant.IsActive)
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "TENANT_NOT_FOUND", Message = "Tenant not found or inactive." });

        var secret = GenerateSecret();
        var prefix = secret.Substring(0, 12);

        var endpoint = new WebhookEndpoint
        {
            Id = Guid.NewGuid(),
            TenantId = body.TenantId,
            Name = body.Name.Trim(),
            Url = body.Url,
            SigningSecret = secret,
            SigningSecretPrefix = prefix,
            SubscribedEvents = body.SubscribedEvents ?? Array.Empty<string>(),
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
        db.WebhookEndpoints.Add(endpoint);
        await db.SaveChangesAsync(ct);

        return JsonResults.Status(StatusCodes.Status201Created, new WebhookEndpointCreatedDto
        {
            Id = endpoint.Id,
            TenantId = endpoint.TenantId,
            Name = endpoint.Name,
            Url = endpoint.Url,
            SigningSecret = secret,
            SigningSecretPrefix = prefix,
            SubscribedEvents = endpoint.SubscribedEvents,
            CreatedAtUtc = endpoint.CreatedAtUtc,
        });
    }

    private static async Task<IResult> PatchAsync(
        Guid id,
        [FromBody] PatchWebhookEndpointRequest body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null)
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });

        var row = await db.WebhookEndpoints.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (row is null)
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "WEBHOOK_NOT_FOUND", Message = "Webhook endpoint not found." });

        if (!string.IsNullOrWhiteSpace(body.Name)) row.Name = body.Name.Trim();
        if (!string.IsNullOrWhiteSpace(body.Url))
        {
            if (!WebhookTargetValidator.TryParsePublicHttpsUri(body.Url, out _, out var targetError))
            {
                return JsonResults.Status(StatusCodes.Status400BadRequest,
                    new ApiError { ErrorCode = "INVALID_URL", Message = targetError! });
            }
            row.Url = body.Url;
        }
        if (body.SubscribedEvents is not null) row.SubscribedEvents = body.SubscribedEvents;
        if (body.IsActive.HasValue) row.IsActive = body.IsActive.Value;
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(ToDto(row));
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var row = await db.WebhookEndpoints.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (row is null)
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "WEBHOOK_NOT_FOUND", Message = "Webhook endpoint not found." });
        db.WebhookEndpoints.Remove(row);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> DeliveriesAsync(
        Guid id,
        [FromQuery] int? take,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var exists = await db.WebhookEndpoints.AsNoTracking().AnyAsync(w => w.Id == id, ct);
        if (!exists)
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "WEBHOOK_NOT_FOUND", Message = "Webhook endpoint not found." });

        var takeClamped = Math.Clamp(take ?? 50, 1, 200);
        var rows = await db.WebhookDeliveries.AsNoTracking()
            .Where(d => d.EndpointId == id)
            .OrderByDescending(d => d.CreatedAtUtc)
            .Take(takeClamped)
            .ToListAsync(ct);
        return JsonResults.Ok(rows.Select(ToDeliveryDto).ToArray());
    }

    /// <summary>Generate <c>whsec_&lt;64 hex chars&gt;</c>. Stripe-style prefix aids recognition.</summary>
    private static string GenerateSecret()
    {
        Span<byte> bytes = stackalloc byte[SigningSecretRandomBytes];
        RandomNumberGenerator.Fill(bytes);
        return "whsec_" + Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static WebhookEndpointDto ToDto(WebhookEndpoint w) => new()
    {
        Id = w.Id,
        TenantId = w.TenantId,
        Name = w.Name,
        Url = w.Url,
        SigningSecretPrefix = w.SigningSecretPrefix,
        SubscribedEvents = w.SubscribedEvents,
        IsActive = w.IsActive,
        CreatedAtUtc = w.CreatedAtUtc,
        LastDeliveredAtUtc = w.LastDeliveredAtUtc,
    };

    private static WebhookDeliveryDto ToDeliveryDto(WebhookDelivery d) => new()
    {
        Id = d.Id,
        EndpointId = d.EndpointId,
        TenantId = d.TenantId,
        EventType = d.EventType,
        JobId = d.JobId,
        Status = d.Status.ToString(),
        AttemptCount = d.AttemptCount,
        LastAttemptAtUtc = d.LastAttemptAtUtc,
        LastResponseCode = d.LastResponseCode,
        LastError = d.LastError,
        NextRetryAtUtc = d.NextRetryAtUtc,
        CreatedAtUtc = d.CreatedAtUtc,
    };
}
