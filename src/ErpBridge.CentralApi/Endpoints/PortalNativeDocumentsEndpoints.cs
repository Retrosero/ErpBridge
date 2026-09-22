using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Portal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal/native/documents</c> (GOAL_PANEL_ERPSIZ E5b): the company-wide sale/purchase/
/// return invoice list and one invoice's own detail — read-only, so the same <see cref="RolePermissions.CanViewLedger"/>
/// gate the customer statement already uses (not <c>CanEditNativeData</c>; an ERP tenant's own Mikro
/// invoices read here too, exactly like <c>/customers/ledger</c> already does).
/// </summary>
public static class PortalNativeDocumentsEndpoints
{
    public static IEndpointRouteBuilder MapPortalNativeDocumentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/native")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/documents", ListAsync).WithName("PortalNativeDocuments");
        group.MapGet("/documents/{key}", GetByKeyAsync).WithName("PortalNativeDocumentByKey");
        return routes;
    }

    private static async Task<IResult> ListAsync(
        HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache,
        string? from, string? to, string[]? kind, string? customer, Guid? userId, int? page, int? pageSize, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;

        DateOnly start;
        if (string.IsNullOrWhiteSpace(from))
        {
            var today = PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);
            start = new DateOnly(today.Year, today.Month, 1);
        }
        else if (!TryDay(from, out start)) return Invalid("from must be yyyy-MM-dd.");
        if (!TryDay(to, out var end)) return Invalid("to must be yyyy-MM-dd.");
        if (end < start) return Invalid("to is before from.");

        var kinds = Values(kind);
        if (kinds.FirstOrDefault(k => k is not ("sale" or "sale_return" or "purchase" or "purchase_return")) is { } unknown)
            return Invalid($"kind '{unknown}' is not one of: sale, sale_return, purchase, purchase_return.");

        var customers = await PortalLedger.CustomersAsync(db, cache, tenant!.Id, ct);
        var movements = await PortalLedger.MovementsAsync(db, cache, tenant.Id, ct);

        // Bounded by the document-kind rows the tenant has ever had, the same reasoning E3c's own
        // creator lookup uses — cheap for a native tenant (Jobs is its whole write history).
        var documentMovementIds = movements.ByCustomer.Values.SelectMany(list => list)
            .Where(m => m.DocumentKey is not null)
            .Select(m => PortalLedger.ExternalIdOf(m.Id))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var creatorLookup = (await db.Jobs.AsNoTracking()
                .Where(j => j.TenantId == tenant.Id && documentMovementIds.Contains(j.ExternalId))
                .Select(j => new { j.ExternalId, j.CreatedByUserId })
                .ToListAsync(ct))
            .GroupBy(j => j.ExternalId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().CreatedByUserId, StringComparer.Ordinal);
        var creatorIds = creatorLookup.Values.OfType<Guid>().Distinct().ToList();
        var userNames = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenant.Id && creatorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName, ct);

        return JsonResults.Ok(PortalLedger.Documents(customers, movements, start, end, kinds, customer, userId,
            externalId => creatorLookup.TryGetValue(externalId, out var uid) ? uid : null, userNames,
            Math.Max(1, page ?? 1), Math.Clamp(pageSize ?? 50, 1, PortalLedger.MaxPageSize)));
    }

    private static async Task<IResult> GetByKeyAsync(
        string key, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;

        var customers = await PortalLedger.CustomersAsync(db, cache, tenant!.Id, ct);
        var movements = await PortalLedger.MovementsAsync(db, cache, tenant.Id, ct);
        var document = PortalLedger.DocumentByKey(customers, movements, key);
        return document is null
            ? JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "DOCUMENT_NOT_FOUND", Message = "No document with that key for this company." })
            : JsonResults.Ok(document);
    }

    private static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access;
        if (!RolePermissions.CanViewLedger(access.User!))
            return (null, null, JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
            {
                ErrorCode = "PORTAL_REQUIRES_MANAGER",
                Message = "The document list is for company administrators, managers and accounting.",
            }));
        return access;
    }

    private static string[] Values(string[]? values) =>
        values is null ? [] : values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static bool TryDay(string? value, out DateOnly day)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            day = PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);
            return true;
        }
        return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day);
    }

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_QUERY", Message = message });
}
