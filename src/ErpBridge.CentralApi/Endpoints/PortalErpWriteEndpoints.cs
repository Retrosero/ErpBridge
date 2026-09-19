using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Portal endpoints for how phone documents are written into the company's ERP (goal ERP yazım Y1b):
/// the company settings, each user's ERP counterparts and the ERP codes to choose from. Company
/// administrators only (403 <c>ADMIN_REQUIRED</c>); a company without an ERP gets 409
/// <c>ERP_NOT_CONNECTED</c>. The server checks shapes and widths only — whether a code exists in the
/// ERP is the agent's check when it writes.
/// </summary>
public static class PortalErpWriteEndpoints
{
    /// <summary>Mikro's <c>*_create_user</c> is a smallint.</summary>
    public const int MaxErpUserNo = short.MaxValue;

    public const int MaxDeliveryDayOffset = 365;

    public static IEndpointRouteBuilder MapPortalErpWriteEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/erp-settings", GetSettingsAsync).WithName("PortalGetErpSettings");
        group.MapPut("/erp-settings", PutSettingsAsync).WithName("PortalPutErpSettings");
        group.MapGet("/users/{id:guid}/erp-mapping", GetMappingAsync).WithName("PortalGetUserErpMapping");
        group.MapPut("/users/{id:guid}/erp-mapping", PutMappingAsync).WithName("PortalPutUserErpMapping");
        group.MapGet("/erp-lookups", LookupsAsync).WithName("PortalErpLookups");
        return routes;
    }

    private static async Task<IResult> GetSettingsAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        var settings = await db.ErpWriteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenant!.Id, ct);
        return JsonResults.Ok(ToDto(settings ?? new ErpWriteSettings()));
    }

    private static async Task<IResult> PutSettingsAsync(HttpContext http, [FromBody] PortalErpWriteSettingsDto? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, user, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null) return Invalid("Body required.");

        var kind = body.SalesDocumentKind?.Trim().ToLowerInvariant();
        if (kind is null || !SalesDocumentKinds.All.Contains(kind)) return Invalid("salesDocumentKind must be order, dispatch or invoice.");
        var approval = body.OrderApprovalMode?.Trim().ToLowerInvariant();
        if (approval is null || !OrderApprovalModes.All.Contains(approval)) return Invalid("orderApprovalMode must be approved or pending.");
        if (SeriesError(body.Series, nameof(body.Series)) is { } seriesError) return Invalid(seriesError);
        if ((CodeError(body.DefaultCashCode, "defaultCashCode") ?? CodeError(body.DefaultCardBankCode, "defaultCardBankCode")
            ?? CodeError(body.DefaultTransferBankCode, "defaultTransferBankCode") ?? CodeError(body.DefaultSalespersonCode, "defaultSalespersonCode")
            ?? CodeError(body.ResponsibilityCenterCode, "responsibilityCenterCode") ?? CodeError(body.ProjectCode, "projectCode")
            ?? RequiredCodeError(body.ChequePortfolioCode, "chequePortfolioCode") ?? RequiredCodeError(body.NotePortfolioCode, "notePortfolioCode")
            ?? NumberError(body.DefaultWarehouseNo, "defaultWarehouseNo", 1, int.MaxValue) ?? NumberError(body.DefaultPriceListNo, "defaultPriceListNo", 1, int.MaxValue)
            ?? NumberError(body.DefaultErpUserNo, "defaultErpUserNo", 0, MaxErpUserNo) ?? NumberError(body.DeliveryDayOffset, "deliveryDayOffset", 0, MaxDeliveryDayOffset)
            ) is { } fieldError)
            return Invalid(fieldError);

        var settings = await db.ErpWriteSettings.FirstOrDefaultAsync(s => s.TenantId == tenant!.Id, ct);
        if (settings is null)
        {
            settings = new ErpWriteSettings { TenantId = tenant!.Id };
            db.ErpWriteSettings.Add(settings);
        }

        settings.SalesDocumentKind = kind;
        settings.OrderApprovalMode = approval;
        settings.OrderSeries = Series(body.Series?.Order) ?? string.Empty;
        settings.DispatchSeries = Series(body.Series?.Dispatch) ?? string.Empty;
        settings.InvoiceSeries = Series(body.Series?.Invoice) ?? string.Empty;
        settings.ReturnSeries = Series(body.Series?.Return) ?? string.Empty;
        settings.CollectionSeries = Series(body.Series?.Collection) ?? string.Empty;
        settings.DefaultWarehouseNo = body.DefaultWarehouseNo;
        settings.DefaultCashCode = Code(body.DefaultCashCode);
        settings.DefaultCardBankCode = Code(body.DefaultCardBankCode);
        settings.DefaultTransferBankCode = Code(body.DefaultTransferBankCode);
        settings.DefaultErpUserNo = body.DefaultErpUserNo;
        settings.DefaultSalespersonCode = Code(body.DefaultSalespersonCode);
        settings.DefaultPriceListNo = body.DefaultPriceListNo;
        settings.ChequePortfolioCode = Code(body.ChequePortfolioCode)!;
        settings.NotePortfolioCode = Code(body.NotePortfolioCode)!;
        settings.ResponsibilityCenterCode = Code(body.ResponsibilityCenterCode);
        settings.ProjectCode = Code(body.ProjectCode);
        settings.DeliveryDayOffset = body.DeliveryDayOffset;
        settings.UpdatedAtUtc = DateTimeOffset.UtcNow;
        settings.UpdatedByUserId = user!.Id;
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(ToDto(settings));
    }

    private static async Task<IResult> GetMappingAsync(HttpContext http, Guid id, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        if (!await UserExistsAsync(db, tenant!.Id, id, ct)) return UserNotFound();
        var mapping = await db.MobileUserErpMappings.AsNoTracking().FirstOrDefaultAsync(m => m.UserId == id && m.TenantId == tenant.Id, ct);
        return JsonResults.Ok(ToDto(mapping ?? new MobileUserErpMapping { UserId = id, TenantId = tenant.Id }));
    }

    private static async Task<IResult> PutMappingAsync(HttpContext http, Guid id, [FromBody] PortalUserErpMappingDto? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, user, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        if (!await UserExistsAsync(db, tenant!.Id, id, ct)) return UserNotFound();
        if (body is null) return Invalid("Body required.");
        if (SeriesError(body.Series, nameof(body.Series)) is { } seriesError) return Invalid(seriesError);
        if ((CodeError(body.SalespersonCode, "salespersonCode") ?? CodeError(body.CashCode, "cashCode")
            ?? CodeError(body.CardBankCode, "cardBankCode") ?? CodeError(body.TransferBankCode, "transferBankCode")
            ?? NumberError(body.WarehouseNo, "warehouseNo", 1, int.MaxValue) ?? NumberError(body.ErpUserNo, "erpUserNo", 0, MaxErpUserNo)
            ) is { } fieldError)
            return Invalid(fieldError);

        var mapping = await db.MobileUserErpMappings.FirstOrDefaultAsync(m => m.UserId == id, ct);
        if (mapping is null)
        {
            mapping = new MobileUserErpMapping { UserId = id, TenantId = tenant.Id };
            db.MobileUserErpMappings.Add(mapping);
        }

        mapping.SalespersonCode = Code(body.SalespersonCode);
        mapping.WarehouseNo = body.WarehouseNo;
        mapping.CashCode = Code(body.CashCode);
        mapping.CardBankCode = Code(body.CardBankCode);
        mapping.TransferBankCode = Code(body.TransferBankCode);
        mapping.ErpUserNo = body.ErpUserNo;
        // null: the company's series; "" (after trimming): series-less for this user on purpose.
        mapping.OrderSeries = Series(body.Series?.Order);
        mapping.DispatchSeries = Series(body.Series?.Dispatch);
        mapping.InvoiceSeries = Series(body.Series?.Invoice);
        mapping.ReturnSeries = Series(body.Series?.Return);
        mapping.CollectionSeries = Series(body.Series?.Collection);
        mapping.UpdatedAtUtc = DateTimeOffset.UtcNow;
        mapping.UpdatedByUserId = user!.Id;
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(ToDto(mapping));
    }

    private static async Task<IResult> LookupsAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;

        var rows = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == tenant!.Id && !r.IsDeleted && (r.Entity == "lookups" || r.Entity == "cashAndBank"))
            .Select(r => r.PayloadJson)
            .ToListAsync(ct);

        var byKind = new Dictionary<string, Dictionary<string, PortalErpLookupItem>>(StringComparer.OrdinalIgnoreCase);
        foreach (var json in rows)
        {
            if (string.IsNullOrWhiteSpace(json)) continue;
            using var document = JsonDocument.Parse(json);
            var item = document.RootElement;
            if (item.ValueKind != JsonValueKind.Object) continue;
            var kind = AndroidEndpoints.GetString(item, "kind")?.Trim();
            var code = AndroidEndpoints.GetString(item, "code")?.Trim();
            if (string.IsNullOrEmpty(kind) || string.IsNullOrEmpty(code)) continue;
            var name = AndroidEndpoints.GetString(item, "name")?.Trim() ?? string.Empty;
            if (!byKind.TryGetValue(kind, out var items)) byKind[kind] = items = new(StringComparer.Ordinal);
            items[code] = new PortalErpLookupItem { Code = code, Name = name };
        }

        return JsonResults.Ok(new PortalErpLookupsResponse
        {
            Warehouses = Sorted(byKind, "warehouse"),
            CashAccounts = Sorted(byKind, "cash"),
            Banks = Sorted(byKind, "bank"),
            Salespersons = Sorted(byKind, "salesperson"),
            PriceLists = Sorted(byKind, "price_list"),
            Projects = Sorted(byKind, "project"),
            ExpenseCards = Sorted(byKind, "expense_card"),
        });
    }

    // ---- helpers --------------------------------------------------------------------------

    private static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access;
        if (access.Tenant!.DataSource != TenantDataSources.Erp)
        {
            return (null, null, JsonResults.Status(StatusCodes.Status409Conflict, new ApiError
            {
                ErrorCode = "ERP_NOT_CONNECTED",
                Message = "This company keeps its books on the server; there is no ERP to write into.",
            }));
        }

        return access;
    }

    private static Task<bool> UserExistsAsync(CentralApiDbContext db, Guid tenantId, Guid userId, CancellationToken ct) =>
        db.MobileUsers.AsNoTracking().AnyAsync(u => u.Id == userId && u.TenantId == tenantId && u.DeletedAtUtc == null, ct);

    private static IReadOnlyList<PortalErpLookupItem> Sorted(Dictionary<string, Dictionary<string, PortalErpLookupItem>> byKind, string kind) =>
        byKind.TryGetValue(kind, out var items)
            ? items.Values
                .OrderBy(i => int.TryParse(i.Code, NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : int.MaxValue)
                .ThenBy(i => i.Code, StringComparer.Ordinal)
                .ToList()
            : [];

    private static string? SeriesError(PortalErpSeriesDto? series, string field)
    {
        if (series is null) return null;
        foreach (var (name, value) in new[] { ("order", series.Order), ("dispatch", series.Dispatch), ("invoice", series.Invoice), ("return", series.Return), ("collection", series.Collection) })
        {
            if (value is not null && value.Trim().Length > ErpWriteSettings.SeriesMaxLength)
                return $"{char.ToLowerInvariant(field[0])}{field[1..]}.{name} is at most {ErpWriteSettings.SeriesMaxLength} characters (ERP series width).";
        }

        return null;
    }

    private static string? CodeError(string? code, string field) =>
        code is not null && code.Trim().Length > ErpWriteSettings.CodeMaxLength
            ? $"{field} is at most {ErpWriteSettings.CodeMaxLength} characters."
            : null;

    private static string? RequiredCodeError(string? code, string field) =>
        string.IsNullOrWhiteSpace(code) ? $"{field} is required." : CodeError(code, field);

    private static string? NumberError(int? value, string field, int min, int max) =>
        value is { } number && (number < min || number > max) ? $"{field} must be between {min} and {max}." : null;

    private static string? Code(string? code) => string.IsNullOrWhiteSpace(code) ? null : code.Trim();

    private static string? Series(string? series) => series?.Trim();

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_ERP_SETTINGS", Message = message });

    private static IResult UserNotFound() =>
        JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "USER_NOT_FOUND", Message = "No user with that id in this company." });

    private static PortalErpWriteSettingsDto ToDto(ErpWriteSettings s) => new()
    {
        SalesDocumentKind = s.SalesDocumentKind,
        OrderApprovalMode = s.OrderApprovalMode,
        Series = new PortalErpSeriesDto { Order = s.OrderSeries, Dispatch = s.DispatchSeries, Invoice = s.InvoiceSeries, Return = s.ReturnSeries, Collection = s.CollectionSeries },
        DefaultWarehouseNo = s.DefaultWarehouseNo,
        DefaultCashCode = s.DefaultCashCode,
        DefaultCardBankCode = s.DefaultCardBankCode,
        DefaultTransferBankCode = s.DefaultTransferBankCode,
        DefaultErpUserNo = s.DefaultErpUserNo,
        DefaultSalespersonCode = s.DefaultSalespersonCode,
        DefaultPriceListNo = s.DefaultPriceListNo,
        ChequePortfolioCode = s.ChequePortfolioCode,
        NotePortfolioCode = s.NotePortfolioCode,
        ResponsibilityCenterCode = s.ResponsibilityCenterCode,
        ProjectCode = s.ProjectCode,
        DeliveryDayOffset = s.DeliveryDayOffset,
        UpdatedAtUtc = s.UpdatedAtUtc,
    };

    private static PortalUserErpMappingDto ToDto(MobileUserErpMapping m) => new()
    {
        UserId = m.UserId,
        SalespersonCode = m.SalespersonCode,
        WarehouseNo = m.WarehouseNo,
        CashCode = m.CashCode,
        CardBankCode = m.CardBankCode,
        TransferBankCode = m.TransferBankCode,
        ErpUserNo = m.ErpUserNo,
        Series = new PortalErpSeriesDto { Order = m.OrderSeries, Dispatch = m.DispatchSeries, Invoice = m.InvoiceSeries, Return = m.ReturnSeries, Collection = m.CollectionSeries },
        UpdatedAtUtc = m.UpdatedAtUtc,
    };
}
