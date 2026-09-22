using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Native;
using ErpBridge.CentralApi.Portal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal/native/stock-cards</c> (GOAL_PANEL_ERPSIZ E1): a company without an ERP
/// creates and edits product cards from the web portal instead of the phone. This is a second door
/// into the one booking engine (D1) — it wraps the same <c>stock_card</c>/<c>stock_card_delete</c>
/// documents and calls the same <see cref="NativeDocumentProcessor"/> the phone's
/// <c>POST /api/v1/ingest/jobs</c> uses; a portal session cannot call that route itself
/// (<c>PORTAL_CANNOT_SUBMIT_DOCUMENTS</c>), so this group exists instead of reusing it.
///
/// <para>Company administrators only (<see cref="RolePermissions.CanEditNativeData"/>, D4), and only
/// for a <c>DataSource=native</c> tenant (D3) — an ERP tenant's product cards live in Mikro, not here.</para>
/// </summary>
public static class PortalNativeCardsEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapPortalNativeCardsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/native")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/stock-cards/{code}", GetStockCardAsync).WithName("PortalGetNativeStockCard");
        group.MapPost("/stock-cards", PutStockCardAsync).WithName("PortalPutNativeStockCard");
        group.MapDelete("/stock-cards/{code}", DeleteStockCardAsync).WithName("PortalDeleteNativeStockCard");
        return routes;
    }

    private static async Task<IResult> GetStockCardAsync(
        string code, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;

        var catalog = await PortalStockCatalog.LoadAsync(db, cache, tenant!.Id, ct);
        var product = catalog.Products.FirstOrDefault(p => string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase));
        if (product is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "STOCK_CARD_NOT_FOUND", Message = "The product does not exist." });

        return JsonResults.Ok(new PortalStockCardDetail
        {
            StockCode = product.Code,
            Name = product.Name,
            Unit = product.Unit,
            VatRate = product.VatRate,
            Category = product.MainGroup,
            Brand = product.Brand,
            Aisle = product.Shelf,
            Barcodes = [.. product.Barcodes],
            Prices = product.Prices.OrderBy(x => x.Key)
                .Select(x => new PortalStockPrice { ListNumber = x.Key, Name = PortalStockCatalog.PriceListName(catalog, x.Key), Price = x.Value })
                .ToList(),
            Quantity = product.TotalQuantity,
            LastMovementDate = product.LastMovement?.ToString("yyyy-MM-dd"),
        });
    }

    private static async Task<IResult> PutStockCardAsync(
        HttpContext http, [FromBody] PortalStockCardRequest? body, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var (tenant, user, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null) return Invalid("Body required.");

        var code = body.StockCode?.Trim();
        if (string.IsNullOrWhiteSpace(code)) return Invalid("stockCode is required.");
        if (code.Length > 64) return Invalid("stockCode must be at most 64 characters.");
        if (string.IsNullOrWhiteSpace(body.Name)) return Invalid("name is required.");

        var barcode = string.IsNullOrWhiteSpace(body.Barcode) ? null : body.Barcode.Trim();
        if (barcode is not null)
        {
            // Barcode records are keyed only by the barcode itself (MobileRecordProjector), so
            // assigning one already on another card would silently move it there — the next scan
            // or order would resolve to the wrong product.
            var catalog = await PortalStockCatalog.LoadAsync(db, cache, tenant!.Id, ct);
            var owner = catalog.Products.FirstOrDefault(p => p.Barcodes.Contains(barcode, StringComparer.OrdinalIgnoreCase));
            if (owner is not null && !string.Equals(owner.Code, code, StringComparison.OrdinalIgnoreCase))
                return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError
                {
                    ErrorCode = "BARCODE_IN_USE",
                    Message = $"This barcode already belongs to product {owner.Code}.",
                });
        }

        var payload = new
        {
            stockCode = code,
            name = body.Name.Trim(),
            unit = body.Unit,
            vatRate = body.VatRate,
            category = body.Category,
            brand = body.Brand,
            aisle = body.Aisle,
            barcode,
            price = body.Price,
            openingQuantity = body.OpeningQuantity,
        };
        return await BookAsync(http, db, tenant!, user!, NativeDocumentProcessor.StockCard, OperationKey("portal-stock", code, body.OperationId), payload, ct);
    }

    private static async Task<IResult> DeleteStockCardAsync(
        string code, string? operationId, HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, user, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        if (string.IsNullOrWhiteSpace(code)) return Invalid("stockCode is required.");

        return await BookAsync(http, db, tenant!, user!, NativeDocumentProcessor.StockCardDelete, OperationKey("portal-stock-delete", code, operationId), new { stockCode = code }, ct);
    }

    /// <summary>
    /// The job's idempotency key. A caller that wants a lost response to retry safely (rather than
    /// racing a second job, or — for a delete — failing 422 because the product is already gone)
    /// supplies its own <paramref name="operationId"/> once per save/delete attempt and resends the
    /// same one on retry; without one a fresh key is used, so today's caller-less requests still work.
    /// </summary>
    private static string OperationKey(string prefix, string code, string? operationId) =>
        $"{prefix}-{code}-{(string.IsNullOrWhiteSpace(operationId) ? Guid.NewGuid().ToString("N") : operationId.Trim())}";

    // ---- shared booking -----------------------------------------------------------

    /// <summary>
    /// Books one document through <see cref="NativeDocumentProcessor"/>, the same way the native
    /// branch of <c>IngestEndpoints</c> does for the phone: idempotent on
    /// (tenant, documentType, externalId), and a race with a concurrent retry resolves to whichever
    /// row committed first rather than surfacing the unique-index violation.
    /// </summary>
    private static async Task<IResult> BookAsync(
        HttpContext http, CentralApiDbContext db, Tenant tenant, MobileUser user,
        string documentType, string externalId, object payload, CancellationToken ct)
    {
        var existing = await db.Jobs.AsNoTracking()
            .FirstOrDefaultAsync(j => j.TenantId == tenant.Id && j.DocumentType == documentType && j.ExternalId == externalId, ct);
        if (existing is not null) return JobResult(existing, idempotent: true, StatusCodes.Status200OK);

        var job = new Job
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            ExternalId = externalId,
            DocumentType = documentType,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            Status = JobStatus.Pending,
            EnqueuedAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = user.Id,
            CorrelationId = ErpBridge.CentralApi.LogCenter.CorrelationId.Of(http),
        };

        var processor = http.RequestServices.GetRequiredService<NativeDocumentProcessor>();
        try
        {
            var booked = await processor.IngestAsync(db, tenant.Id, job, RolePermissions.IsAdmin(user), ct);
            if (booked.Status == JobStatus.Failed)
                return JsonResults.Status(StatusCodes.Status422UnprocessableEntity, new ApiError
                {
                    ErrorCode = "STOCK_CARD_REJECTED",
                    Message = booked.LastError ?? "The card could not be saved.",
                });
            return JobResult(booked, idempotent: false, StatusCodes.Status201Created);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            var winner = await db.Jobs.AsNoTracking()
                .FirstOrDefaultAsync(j => j.TenantId == tenant.Id && j.DocumentType == documentType && j.ExternalId == externalId, ct);
            if (winner is null) throw;
            return JobResult(winner, idempotent: true, StatusCodes.Status200OK);
        }
    }

    private static IResult JobResult(Job job, bool idempotent, int statusCode) => JsonResults.Status(statusCode, new IngestJobResponse
    {
        JobId = job.Id,
        TenantId = job.TenantId,
        ExternalId = job.ExternalId,
        DocumentType = job.DocumentType,
        Status = job.Status.ToString(),
        Idempotent = idempotent,
    });

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_STOCK_CARD", Message = message });

    private static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeAsync(
        HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access;
        if (!RolePermissions.CanEditNativeData(access.User!))
            return (null, null, JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
            {
                ErrorCode = "ROLE_NOT_ALLOWED",
                Message = "Only company administrators can manage products from the portal.",
            }));
        if (access.Tenant!.DataSource != TenantDataSources.Native)
            return (null, null, JsonResults.Status(StatusCodes.Status409Conflict, new ApiError
            {
                ErrorCode = "TENANT_IS_NOT_NATIVE",
                Message = "This company's products are managed in its ERP, not the portal.",
            }));
        return access;
    }
}
