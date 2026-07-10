using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>GET /api/v1/admin/bootstrap/latest</c>. Returns a row-count
/// summary of the most recent <see cref="Domain.BootstrapPackage"/> for a
/// tenant. Admin-only. The payload is stored as JSON; this endpoint
/// deserializes it into a generic dictionary of array lengths so we don't
/// have to take a dependency on <c>ErpBridge.Erp.Abstractions</c> from the
/// admin endpoint surface.
/// </summary>
public static class AdminBootstrapEndpoints
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapAdminBootstrapEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/bootstrap")
            .WithTags("Admin/Bootstrap")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/latest", LatestAsync)
            .WithName("AdminBootstrapLatest")
            .Produces<BootstrapSummaryDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        return routes;
    }

    private static async Task<IResult> LatestAsync(
        [FromQuery] Guid? tenantId,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_TENANT", Message = "tenantId query parameter is required." });
        }

        var package = await db.BootstrapPackages.AsNoTracking()
            .Where(p => p.TenantId == tenantId.Value)
            .OrderByDescending(p => p.PulledAtUtc)
            .FirstOrDefaultAsync(ct);

        if (package is null)
        {
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "BOOTSTRAP_NOT_FOUND", Message = "No bootstrap snapshot found for tenant." });
        }

        var (customers, stocks, prices, inventory, openOrders, cashAndBank, lookups) = CountRows(package.PayloadJson);

        return JsonResults.Ok(new BootstrapSummaryDto
        {
            TenantId = package.TenantId,
            CapturedAtUtc = package.PulledAtUtc,
            CustomersCount = customers,
            StocksCount = stocks,
            PricesCount = prices,
            InventoryCount = inventory,
            OpenOrdersCount = openOrders,
            CashAndBankCount = cashAndBank,
            LookupsCount = lookups,
        });
    }

    /// <summary>
    /// Best-effort row counts for the bootstrap payload. Counts each known
    /// collection by length. Returns 0 for missing/unparseable sections.
    /// </summary>
    private static (int customers, int stocks, int prices, int inventory, int openOrders, int cashAndBank, int lookups) CountRows(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            return (0, 0, 0, 0, 0, 0, 0);
        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;
            return (
                CountArray(root, "Customers"),
                CountArray(root, "Stocks"),
                CountArray(root, "Prices"),
                CountArray(root, "Inventory"),
                CountArray(root, "OpenOrders"),
                CountArray(root, "CashAndBank"),
                CountArray(root, "Lookups"));
        }
        catch
        {
            return (0, 0, 0, 0, 0, 0, 0);
        }
    }

    private static int CountArray(JsonElement root, string propertyName)
    {
        if (root.ValueKind != JsonValueKind.Object) return 0;
        if (!root.TryGetProperty(propertyName, out var prop)) return 0;
        if (prop.ValueKind != JsonValueKind.Array) return 0;
        return prop.GetArrayLength();
    }
}
