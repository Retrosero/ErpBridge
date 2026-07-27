using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/admin/tenants</c>: list, create, get, patch. Admin-only.
/// </summary>
public static class AdminTenantsEndpoints
{
    public static IEndpointRouteBuilder MapAdminTenantsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/tenants")
            .WithTags("Admin/Tenants")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/", ListAsync)
            .WithName("AdminTenantsList")
            .Produces<TenantDto[]>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateAsync)
            .WithName("AdminTenantsCreate")
            .Produces<TenantDto>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetAsync)
            .WithName("AdminTenantsGet")
            .Produces<TenantDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}", PatchAsync)
            .WithName("AdminTenantsPatch")
            .Produces<TenantDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        return routes;
    }

    private static async Task<IResult> ListAsync(
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var rows = await db.Tenants.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);
        var dtos = rows.Select(ToDto).ToArray();
        return JsonResults.Ok(dtos);
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateTenantRequest body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.Name))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_NAME", Message = "name is required." });

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = body.Name.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsActive = true,
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);
        return JsonResults.Status(StatusCodes.Status201Created, ToDto(tenant));
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tenant is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "TENANT_NOT_FOUND", Message = "Tenant not found." });
        return JsonResults.Ok(ToDto(tenant));
    }

    private static async Task<IResult> PatchAsync(
        Guid id,
        [FromBody] PatchTenantRequest body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tenant is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "TENANT_NOT_FOUND", Message = "Tenant not found." });

        if (body.IsActive.HasValue) tenant.IsActive = body.IsActive.Value;
        if (body.DeviceSeatLimit.HasValue)
        {
            if (body.DeviceSeatLimit.Value is < 1 or > 10000)
                return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_DEVICE_SEAT_LIMIT", Message = "Device seat limit must be between 1 and 10000." });
            tenant.DeviceSeatLimit = body.DeviceSeatLimit.Value;
        }
        if (body.StockDetailFields is not null)
        {
            if (!TryNormalizeStockDetailFields(body.StockDetailFields, out var fields, out var error))
                return JsonResults.Status(StatusCodes.Status400BadRequest,
                    new ApiError { ErrorCode = "INVALID_STOCK_DETAIL_FIELDS", Message = error });

            tenant.StockDetailFieldsJson = JsonSerializer.Serialize(fields);
        }
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(ToDto(tenant));
    }

    private static TenantDto ToDto(Tenant t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        CreatedAtUtc = t.CreatedAtUtc,
        IsActive = t.IsActive,
        DeviceSeatLimit = t.DeviceSeatLimit,
        StockDetailFields = ReadStockDetailFields(t.StockDetailFieldsJson),
    };

    private static readonly IReadOnlyDictionary<string, string> AllowedSources =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["shelfCode"] = "shelfCode", ["sto_yer_kod"] = "sto_yer_kod",
            ["sectorCode"] = "sectorCode", ["sto_sektor_kodu"] = "sto_sektor_kodu",
            ["packageCode"] = "packageCode", ["sto_ambalaj_kodu"] = "sto_ambalaj_kodu",
            ["brandCode"] = "brandCode", ["sto_marka_kodu"] = "sto_marka_kodu",
            ["cartonCode"] = "cartonCode", ["sto_kalkon_kodu"] = "sto_kalkon_kodu",
        };

    internal static IReadOnlyList<StockDetailFieldDefinition> DefaultStockDetailFields { get; } =
    [
        new() { Key = "aisle", Label = "Reyon Kodu", SourceField = "shelfCode", VisibleByDefault = true },
        new() { Key = "measurement", Label = "Ölçü", SourceField = "sectorCode", VisibleByDefault = true },
        new() { Key = "packaging", Label = "Ambalaj", SourceField = "packageCode", VisibleByDefault = true },
        new() { Key = "brand", Label = "Marka", SourceField = "brandCode", VisibleByDefault = true },
        new() { Key = "cartonQuantity", Label = "Koli Adet", SourceField = "cartonCode", VisibleByDefault = true },
    ];

    internal static IReadOnlyList<StockDetailFieldDefinition> ReadStockDetailFields(string? json)
    {
        try
        {
            var fields = JsonSerializer.Deserialize<List<StockDetailFieldDefinition>>(json ?? "[]");
            return fields is { Count: > 0 } && TryNormalizeStockDetailFields(fields, out var normalized, out _)
                ? normalized
                : DefaultStockDetailFields;
        }
        catch (JsonException) { return DefaultStockDetailFields; }
    }

    private static bool TryNormalizeStockDetailFields(
        IEnumerable<StockDetailFieldDefinition> requested,
        out IReadOnlyList<StockDetailFieldDefinition> normalized,
        out string error)
    {
        var rows = new List<StockDetailFieldDefinition>();
        foreach (var field in requested)
        {
            var key = field.Key?.Trim() ?? string.Empty;
            var label = field.Label?.Trim() ?? string.Empty;
            var source = field.SourceField?.Trim() ?? string.Empty;
            if (key is not ("aisle" or "measurement" or "packaging" or "brand" or "cartonQuantity"))
            { normalized = Array.Empty<StockDetailFieldDefinition>(); error = "Unknown stock detail key."; return false; }
            if (label.Length is < 1 or > 48)
            { normalized = Array.Empty<StockDetailFieldDefinition>(); error = "Each stock detail label must be 1-48 characters."; return false; }
            if (!AllowedSources.TryGetValue(source, out var canonicalSource))
            { normalized = Array.Empty<StockDetailFieldDefinition>(); error = $"Unsupported bootstrap stock field: {source}."; return false; }
            if (rows.Any(item => item.Key == key))
            { normalized = Array.Empty<StockDetailFieldDefinition>(); error = "Stock detail keys must be unique."; return false; }
            rows.Add(new StockDetailFieldDefinition { Key = key, Label = label, SourceField = canonicalSource, VisibleByDefault = field.VisibleByDefault });
        }
        normalized = rows.Count == 0 ? DefaultStockDetailFields : rows;
        error = string.Empty;
        return true;
    }
}
