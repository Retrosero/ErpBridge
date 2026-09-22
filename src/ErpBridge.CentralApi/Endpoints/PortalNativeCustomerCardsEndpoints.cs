using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Native;
using ErpBridge.CentralApi.Portal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal/native/customer-cards</c> (GOAL_PANEL_ERPSIZ E2a): a company without an
/// ERP creates and edits customer cards from the web portal instead of the phone. Same shape as
/// <see cref="PortalNativeCardsEndpoints"/>'s stock cards — a second door into
/// <see cref="NativeDocumentProcessor"/>, not a second write engine (D1).
///
/// <para>No delete endpoint yet: unlike a stock level (<see cref="ErpBridge.CentralApi.Domain.NativeStockLevel.LastMovementAtUtc"/>),
/// a customer's ledger movements carry neither <see cref="MobileRecord.CustomerKey"/> nor any other
/// indexed link back to the customer (by design — deleting a customer must not hide the documents
/// already booked against them), so "this customer has movements and cannot be deleted" cannot be
/// answered with an indexed lookup the way the product check can. Scoping that check correctly is
/// its own task; guessing an approximation here (e.g. balance != 0) would silently allow deleting a
/// customer whose balance nets to zero but who has real history.</para>
///
/// <para>Company administrators only (<see cref="RolePermissions.CanEditNativeData"/>, D4), and only
/// for a <c>DataSource=native</c> tenant (D3).</para>
/// </summary>
public static class PortalNativeCustomerCardsEndpoints
{
    private const string RejectedErrorCode = "CUSTOMER_CARD_REJECTED";

    public static IEndpointRouteBuilder MapPortalNativeCustomerCardsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/native")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/customer-cards", PutCustomerCardAsync).WithName("PortalPutNativeCustomerCard");
        return routes;
    }

    private static async Task<IResult> PutCustomerCardAsync(
        HttpContext http, [FromBody] PortalCustomerCardRequest? body, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null) return Invalid("Body required.");

        var code = body.CustomerCode?.Trim();
        if (string.IsNullOrWhiteSpace(code)) return Invalid("customerCode is required.");
        if (code.Length > 64) return Invalid("customerCode must be at most 64 characters.");
        if (string.IsNullOrWhiteSpace(body.Title)) return Invalid("title is required.");

        var customers = await PortalLedger.CustomersAsync(db, cache, tenant!.Id, ct);
        var existing = customers.TryGetValue(code, out var found) ? found : null;

        var payload = new
        {
            customerCode = code,
            title = body.Title.Trim(),
            taxNo = body.TaxNo,
            taxOffice = body.TaxOffice,
            phone = body.Phone,
            email = body.Email,
            regionCode = body.RegionCode,
            openingBalance = body.OpeningBalance,
        };
        var audit = new PortalNativeWriteHelpers.AuditInfo(
            Entity: "customer_card", EntityKey: code, Action: existing is null ? "create" : "edit",
            Summary: (existing is null ? "Müşteri oluşturuldu: " : "Müşteri düzenlendi: ") + $"{code} — {body.Title.Trim()}",
            BeforeJson: existing is null ? null : JsonSerializer.Serialize(new
            {
                customerCode = existing.Code, title = existing.Title, taxNo = existing.TaxNo, taxOffice = existing.TaxOffice,
                phone = existing.Phone, email = existing.Email, regionCode = existing.RegionCode,
            }));
        return await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant!, user!, NativeDocumentProcessor.CustomerCard,
            PortalNativeWriteHelpers.OperationKey("portal-customer", code, body.OperationId), payload, RejectedErrorCode, ct, audit);
    }

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_CUSTOMER_CARD", Message = message });
}
