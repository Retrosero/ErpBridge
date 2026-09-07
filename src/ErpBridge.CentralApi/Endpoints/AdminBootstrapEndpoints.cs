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

        var snapshot = await db.BootstrapSnapshots.AsNoTracking()
            .Where(p => p.TenantId == tenantId.Value && p.IsActive)
            .FirstOrDefaultAsync(ct);

        if (snapshot is not null)
        {
            var counts = await db.BootstrapSnapshotChunks.AsNoTracking()
                .Where(x => x.SnapshotId == snapshot.Id)
                .GroupBy(x => x.Section)
                .Select(g => new { Section = g.Key, Count = g.Sum(x => x.ItemCount) })
                .ToDictionaryAsync(x => x.Section, x => x.Count, ct);
            return JsonResults.Ok(new BootstrapSummaryDto
            {
                TenantId = snapshot.TenantId,
                CapturedAtUtc = snapshot.PulledAtUtc,
                CustomersCount = Count(counts, "customers"),
                StocksCount = Count(counts, "stocks"),
                PricesCount = Count(counts, "prices"),
                InventoryCount = Count(counts, "inventory"),
                OpenOrdersCount = Count(counts, "openOrders"),
                CashAndBankCount = Count(counts, "cashAndBank"),
                LookupsCount = Count(counts, "lookups"),
                CustomerAddressesCount = Count(counts, "customerAddresses"),
                CustomerContactsCount = Count(counts, "customerContacts"),
                BarcodesCount = Count(counts, "barcodes"),
                SalesConditionsCount = Count(counts, "salesConditions"),
                CustomerTransactionsCount = Count(counts, "customerTransactions"),
                StockTransactionsCount = Count(counts, "stockTransactions"),
            });
        }

        if (package is null)
        {
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "BOOTSTRAP_NOT_FOUND", Message = "No bootstrap snapshot found for tenant." });
        }

        var packageCounts = CountRows(package.PayloadJson);

        return JsonResults.Ok(new BootstrapSummaryDto
        {
            TenantId = package.TenantId,
            CapturedAtUtc = package.PulledAtUtc,
            CustomersCount = packageCounts.Customers,
            StocksCount = packageCounts.Stocks,
            PricesCount = packageCounts.Prices,
            InventoryCount = packageCounts.Inventory,
            OpenOrdersCount = packageCounts.OpenOrders,
            CashAndBankCount = packageCounts.CashAndBank,
            LookupsCount = packageCounts.Lookups,
            CustomerAddressesCount = packageCounts.CustomerAddresses,
            CustomerContactsCount = packageCounts.CustomerContacts,
            BarcodesCount = packageCounts.Barcodes,
            SalesConditionsCount = packageCounts.SalesConditions,
            CustomerTransactionsCount = packageCounts.CustomerTransactions,
            StockTransactionsCount = packageCounts.StockTransactions,
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
                CountArray(root, "salesConditions"), CountArray(root, "customerTransactions"),
                CountArray(root, "stockTransactions"));
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
        int SalesConditions = 0,
        int CustomerTransactions = 0,
        int StockTransactions = 0);

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

    private static int Count(IReadOnlyDictionary<string, int> counts, string section) =>
        counts.TryGetValue(section, out var count) ? count : 0;
}
