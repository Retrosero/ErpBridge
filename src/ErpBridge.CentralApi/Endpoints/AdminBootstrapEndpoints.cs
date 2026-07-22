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

        var counts = CountRows(package.PayloadJson);

        return JsonResults.Ok(new BootstrapSummaryDto
        {
            TenantId = package.TenantId,
            CapturedAtUtc = package.PulledAtUtc,
            CustomersCount = counts.Customers,
            StocksCount = counts.Stocks,
            PricesCount = counts.Prices,
            InventoryCount = counts.Inventory,
            OpenOrdersCount = counts.OpenOrders,
            CashAndBankCount = counts.CashAndBank,
            LookupsCount = counts.Lookups,
            CustomerAddressesCount = counts.CustomerAddresses,
            CustomerContactsCount = counts.CustomerContacts,
            BarcodesCount = counts.Barcodes,
            SalesConditionsCount = counts.SalesConditions,
        });
    }

    /// <summary>
    /// Best-effort row counts for the bootstrap payload. Counts each known
    /// collection by length. Returns 0 for missing/unparseable sections.
    /// </summary>
    private static BootstrapCounts CountRows(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            return new();
        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;
            return new BootstrapCounts(
                CountArray(root, "customers"), CountArray(root, "stocks"),
                CountArray(root, "prices"), CountArray(root, "inventory"),
                CountArray(root, "openOrders"), CountArray(root, "cashAndBank"),
                CountArray(root, "lookups"), CountArray(root, "customerAddresses"),
                CountArray(root, "customerContacts"), CountArray(root, "barcodes"),
                CountArray(root, "salesConditions"));
        }
        catch
        {
            return new();
        }
    }

    private sealed record BootstrapCounts(
        int Customers = 0,
        int Stocks = 0,
        int Prices = 0,
        int Inventory = 0,
        int OpenOrders = 0,
        int CashAndBank = 0,
        int Lookups = 0,
        int CustomerAddresses = 0,
        int CustomerContacts = 0,
        int Barcodes = 0,
        int SalesConditions = 0);

    private static int CountArray(JsonElement root, string propertyName)
    {
        if (root.ValueKind != JsonValueKind.Object) return 0;

        foreach (var property in root.EnumerateObject())
        {
            if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                continue;

            return property.Value.ValueKind == JsonValueKind.Array
                ? property.Value.GetArrayLength()
                : 0;
        }

        return 0;
    }
}
