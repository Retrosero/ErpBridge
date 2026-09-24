using System.Globalization;
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
/// Maps <c>/api/v1/portal/native/documents</c>: the company-wide sale/purchase/return invoice list and
/// one invoice's own detail (GOAL_PANEL_ERPSIZ E5b) — read-only, so the same <see cref="RolePermissions.CanViewLedger"/>
/// gate the customer statement already uses (not <c>CanEditNativeData</c>; an ERP tenant's own Mikro
/// invoices read here too, exactly like <c>/customers/ledger</c> already does). <c>…/documents/{key}/void</c>
/// (E5c, D11) is the one write here, so it uses the stricter <see cref="PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync"/>
/// gate every other native write endpoint uses — an ERP tenant's own Mikro invoices are read-only here.
/// </summary>
public static class PortalNativeDocumentsEndpoints
{
    private const string DocumentVoidRejectedErrorCode = "DOCUMENT_VOID_REJECTED";
    private const string DocumentEditRejectedErrorCode = "DOCUMENT_EDIT_REJECTED";
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static IEndpointRouteBuilder MapPortalNativeDocumentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/native")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/documents", ListAsync).WithName("PortalNativeDocuments");
        group.MapGet("/documents/{key}", GetByKeyAsync).WithName("PortalNativeDocumentByKey");
        group.MapPost("/documents/{key}/void", VoidAsync).WithName("PortalVoidNativeDocument");
        group.MapPost("/documents/{key}/edit", EditAsync).WithName("PortalEditNativeDocument");
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

    /// <summary>
    /// Cancels one whole sale/purchase/return document (GOAL_PANEL_ERPSIZ E5c, D11): both its ledger
    /// effect(s) and its stock effect are reversed together. <paramref name="key"/> is the document's own
    /// <c>documentKey</c> — the same identifier the list and detail endpoints already use — resolved here
    /// to the underlying ledger row's own id (<see cref="PortalDocumentResponse.Id"/>), the
    /// <c>targetKey</c> <see cref="NativeDocumentProcessor.DocumentVoid"/> actually books against.
    /// </summary>
    private static async Task<IResult> VoidAsync(
        string key, HttpContext http, [FromBody] PortalDocumentVoidRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        var reason = body?.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason)) return InvalidVoid("A void needs a reason.");

        var customers = await PortalLedger.CustomersAsync(db, cache, tenant!.Id, ct);
        var movements = await PortalLedger.MovementsAsync(db, cache, tenant.Id, ct);
        var document = PortalLedger.DocumentByKey(customers, movements, key);
        if (document is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "DOCUMENT_NOT_FOUND", Message = "No document with that key for this company." });
        if (document.Voided)
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError { ErrorCode = "ALREADY_VOIDED", Message = "This document was already cancelled." });

        var voidedKind = KindLabel(document.Kind);
        var record = await db.MobileRecords.AsNoTracking()
            .FirstOrDefaultAsync(r => r.TenantId == tenant.Id && r.Entity == "customerTransactions" && r.RecordKey == document.Id && !r.IsDeleted, ct);

        // The operation key names the party, not the document key: a document key embeds the document number,
        // which defaults to the original job's own external id, so key-based ids could pass Jobs.ExternalId's 128.
        var payload = new { targetKey = document.Id, reason };
        var audit = new PortalNativeWriteHelpers.AuditInfo(
            Entity: document.Kind, EntityKey: document.CustomerCode,
            Action: "void", Summary: $"İptal edildi ({voidedKind}): {reason}", BeforeJson: record?.PayloadJson);
        return await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant, user!, NativeDocumentProcessor.DocumentVoid,
            PortalNativeWriteHelpers.OperationKey("portal-document-void", document.CustomerCode, body?.OperationId), payload, DocumentVoidRejectedErrorCode, ct, audit);
    }

    /// <summary>
    /// Corrects one whole sale/purchase/return document (GOAL_PANEL_ERPSIZ E5d, D11): <c>document_edit</c> voids
    /// the original (ledger + stock, exactly as <see cref="VoidAsync"/>) and books the corrected document of the
    /// same kind in the same transaction — a correction the writer rejects leaves the original untouched.
    /// The body is checked by the same <see cref="PortalNativeSalesEndpoints.BuildDocumentAsync"/> a new document
    /// is; <paramref name="key"/> resolves to the original's own ledger row id as in <see cref="VoidAsync"/>.
    /// </summary>
    private static async Task<IResult> EditAsync(
        string key, HttpContext http, [FromBody] PortalDocumentEditRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null) return InvalidEdit("Body required.");
        var voidReason = body.VoidReason?.Trim();
        if (string.IsNullOrWhiteSpace(voidReason)) return InvalidEdit("An edit needs a reason for the correction.");

        var customers = await PortalLedger.CustomersAsync(db, cache, tenant!.Id, ct);
        var movements = await PortalLedger.MovementsAsync(db, cache, tenant.Id, ct);
        var document = PortalLedger.DocumentByKey(customers, movements, key);
        if (document is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "DOCUMENT_NOT_FOUND", Message = "No document with that key for this company." });
        if (document.Voided)
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError { ErrorCode = "ALREADY_VOIDED", Message = "This document was already cancelled." });

        var (documentType, linesRequired) = document.Kind switch
        {
            "sale" => (NativeDocumentProcessor.SalesOrder, true),
            "sale_return" => (NativeDocumentProcessor.SalesReturn, true),
            "purchase" => (NativeDocumentProcessor.PurchaseReceipt, false),
            _ => ((string?)null, false),
        };
        if (documentType is null)
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError { ErrorCode = "DOCUMENT_NOT_EDITABLE", Message = "Only a sale, purchase or return can be edited." });

        var corrected = await PortalNativeSalesEndpoints.BuildDocumentAsync(db, tenant.Id, new PortalNativeDocumentRequest
        {
            PartyCode = string.IsNullOrWhiteSpace(body.PartyCode) ? document.CustomerCode : body.PartyCode,
            Lines = body.Lines,
            Amount = body.Amount,
            PaymentType = body.PaymentType,
            OccurredAt = body.OccurredAt,
            Description = body.Description,
            DocumentNo = body.DocumentNo,
        }, documentType, linesRequired, ct);
        if (corrected.Error is not null) return InvalidEdit(corrected.Error);

        var record = await db.MobileRecords.AsNoTracking()
            .FirstOrDefaultAsync(r => r.TenantId == tenant.Id && r.Entity == "customerTransactions" && r.RecordKey == document.Id && !r.IsDeleted, ct);
        var payload = new { targetKey = document.Id, voidReason, documentType, document = corrected.Payload };
        var audit = new PortalNativeWriteHelpers.AuditInfo(
            Entity: document.Kind, EntityKey: document.CustomerCode, Action: "edit",
            Summary: $"Düzenlendi ({KindLabel(document.Kind)}): {voidReason} — yeni tutar {corrected.Total.ToString("0.00", Turkish)} TL",
            BeforeJson: record?.PayloadJson);
        return await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant, user!, NativeDocumentProcessor.DocumentEdit,
            PortalNativeWriteHelpers.OperationKey("portal-document-edit", document.CustomerCode, body.OperationId), payload, DocumentEditRejectedErrorCode, ct, audit);
    }

    private static string KindLabel(string kind) => kind switch
    {
        "sale" => "Satış",
        "sale_return" => "İade",
        "purchase" => "Alış",
        "purchase_return" => "Alış İadesi",
        _ => "Belge",
    };

    private static IResult InvalidEdit(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_DOCUMENT_EDIT_REQUEST", Message = message });

    private static IResult InvalidVoid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_DOCUMENT_VOID_REQUEST", Message = message });

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
