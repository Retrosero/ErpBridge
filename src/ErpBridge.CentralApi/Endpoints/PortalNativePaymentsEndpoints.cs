using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Native;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal/native/collections</c> and <c>…/disbursements</c> (GOAL_PANEL_ERPSIZ E3a):
/// a company without an ERP records a payment received from or paid to a customer directly from the
/// portal. Same shape as the product/customer card endpoints — a second door into
/// <see cref="NativeDocumentProcessor"/> via <see cref="PortalNativeWriteHelpers"/> (D1), not a
/// second write engine.
/// </summary>
public static class PortalNativePaymentsEndpoints
{
    private const string RejectedErrorCode = "PAYMENT_REJECTED";

    public static IEndpointRouteBuilder MapPortalNativePaymentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/native")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/collections", PostCollectionAsync).WithName("PortalPostNativeCollection");
        group.MapPost("/disbursements", PostDisbursementAsync).WithName("PortalPostNativeDisbursement");
        return routes;
    }

    private static Task<IResult> PostCollectionAsync(
        HttpContext http, [FromBody] PortalPaymentRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct) =>
        PostAsync(http, body, db, NativeDocumentProcessor.Collection, "portal-collection", ct);

    private static Task<IResult> PostDisbursementAsync(
        HttpContext http, [FromBody] PortalPaymentRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct) =>
        PostAsync(http, body, db, NativeDocumentProcessor.Disbursement, "portal-disbursement", ct);

    private static async Task<IResult> PostAsync(
        HttpContext http, PortalPaymentRequest? body, CentralApiDbContext db, string documentType, string keyPrefix, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null) return Invalid("Body required.");

        var code = body.CustomerCode?.Trim();
        if (string.IsNullOrWhiteSpace(code)) return Invalid("customerCode is required.");
        if (body.Amount <= 0) return Invalid("amount must be positive.");

        string? occurredAt = null;
        if (!string.IsNullOrWhiteSpace(body.OccurredAt))
        {
            if (!DateOnly.TryParseExact(body.OccurredAt, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
                return Invalid("occurredAt must be yyyy-MM-dd.");
            occurredAt = day.ToDateTime(new TimeOnly(12, 0)).ToString("O", CultureInfo.InvariantCulture);
        }

        // customerCode is checked here (not left to the processor) so a typo is reported as a plain
        // 400 rather than the generic 422 the booking failure path returns for every rejection.
        var exists = await db.MobileRecords.AsNoTracking()
            .AnyAsync(r => r.TenantId == tenant!.Id && r.Entity == "customers" && r.RecordKey == code && !r.IsDeleted, ct);
        if (!exists) return Invalid("The customer does not exist.");

        var payload = new
        {
            customerCode = code,
            amount = body.Amount,
            paymentType = body.PaymentType,
            occurredAt,
            description = body.Description,
        };
        return await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant!, user!, documentType,
            PortalNativeWriteHelpers.OperationKey(keyPrefix, code, body.OperationId), payload, RejectedErrorCode, ct);
    }

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_PAYMENT", Message = message });
}
