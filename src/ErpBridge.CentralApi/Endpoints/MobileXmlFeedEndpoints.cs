using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/android/xml-feed</c>: the company's XML product feed (sellable module
/// <see cref="TenantModules.XmlImport"/>). The administrator saves it from the phone; every phone
/// of the company reads it and downloads and imports the feed itself — the server never fetches
/// the URL. In an ERP company only images and descriptions are imported: the ERP keeps the
/// master data, so <c>fullImport</c> is stored false there whatever the phone sends.
/// </summary>
public static class MobileXmlFeedEndpoints
{
    public const int MaxUrlLength = 2048;
    public const int MaxRecordPathLength = 512;
    public const int MaxPathsPerTarget = 10;
    public const int MaxPathLength = 256;
    public const string CodeTarget = "CODE";

    /// <summary>Fields a record can be mapped to; case-sensitive.</summary>
    public static readonly IReadOnlySet<string> Targets = new HashSet<string>(StringComparer.Ordinal)
    {
        "CODE", "IMAGE", "DESCRIPTION", "BARCODE", "TITLE", "BRAND", "CATEGORY", "PRICE", "VAT", "STOCK",
    };

    public static IEndpointRouteBuilder MapMobileXmlFeedEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/android/xml-feed")
            .WithTags("Android/XmlFeed")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/config", GetConfigAsync).WithName("MobileXmlFeedConfigGet")
            .Produces<XmlFeedConfigDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);
        group.MapPut("/config", PutConfigAsync).WithName("MobileXmlFeedConfigPut")
            .Produces<XmlFeedConfigDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);
        group.MapDelete("/config", DeleteConfigAsync).WithName("MobileXmlFeedConfigDelete")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);
        return routes;
    }

    /// <summary>The company's modules, sorted and lowercase — what the session carries.</summary>
    internal static async Task<string[]> ModulesAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct) =>
        (await db.TenantModules.AsNoTracking().Where(m => m.TenantId == tenantId).Select(m => m.ModuleKey).ToListAsync(ct))
        .Select(k => k.ToLowerInvariant()).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();

    private static async Task<IResult> GetConfigAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var row = await db.TenantXmlFeedSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == access.Tenant!.Id, ct);
        return JsonResults.Ok(await ToDtoAsync(db, row, ct));
    }

    private static async Task<IResult> PutConfigAsync(HttpContext http, [FromBody] XmlFeedConfigRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        if (body is null) return Invalid("Body required.");

        var url = body.Url?.Trim();
        if (string.IsNullOrEmpty(url) || url.Length > MaxUrlLength
            || !Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return Invalid($"url must be an absolute http or https address of at most {MaxUrlLength} characters.");
        var recordPath = body.RecordPath?.Trim();
        if (string.IsNullOrEmpty(recordPath) || recordPath.Length > MaxRecordPathLength)
            return Invalid($"recordPath is required and at most {MaxRecordPathLength} characters.");
        var mapping = NormalizeMapping(body.Mapping, out var mappingError);
        if (mapping is null) return Invalid(mappingError!);

        var tenant = access.Tenant!;
        var row = await db.TenantXmlFeedSettings.FirstOrDefaultAsync(s => s.TenantId == tenant.Id, ct);
        if (row is null)
        {
            row = new TenantXmlFeedSettings { TenantId = tenant.Id };
            db.TenantXmlFeedSettings.Add(row);
        }
        row.Url = url;
        row.RecordPath = recordPath;
        row.MappingJson = JsonSerializer.Serialize(mapping);
        row.DownloadImages = body.DownloadImages;
        row.ImportDescriptions = body.ImportDescriptions;
        // The ERP is the book of record for master data; phones only add images and descriptions there.
        row.FullImport = body.FullImport && tenant.DataSource == TenantDataSources.Native;
        row.UpdatedByUserId = access.User!.Id;
        row.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(await ToDtoAsync(db, row, ct));
    }

    private static async Task<IResult> DeleteConfigAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        var row = await db.TenantXmlFeedSettings.FirstOrDefaultAsync(s => s.TenantId == access.Tenant!.Id, ct);
        if (row is not null)
        {
            db.TenantXmlFeedSettings.Remove(row);
            await db.SaveChangesAsync(ct);
        }
        return Results.NoContent();
    }

    /// <summary>The module is checked before the role, so a company without it gets the same answer for everyone.</summary>
    private static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeAsync(
        HttpContext http, CentralApiDbContext db, bool requireAdmin, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access;
        if (!await db.TenantModules.AsNoTracking().AnyAsync(m => m.TenantId == access.Tenant!.Id && m.ModuleKey == TenantModules.XmlImport, ct))
            return (null, null, JsonResults.Status(StatusCodes.Status403Forbidden,
                new ApiError { ErrorCode = "MODULE_NOT_ENABLED", Message = "The XML product import module is not enabled for this company." }));
        if (requireAdmin && !RolePermissions.CanManageUsers(access.User!))
            return (null, null, JsonResults.Status(StatusCodes.Status403Forbidden,
                new ApiError { ErrorCode = "ADMIN_REQUIRED", Message = "Only company administrators can change the XML feed." }));
        return access;
    }

    /// <summary>
    /// Trims paths and drops blank ones (and targets left without paths); null with a reason when the
    /// mapping names an unknown target, lacks CODE, or exceeds the path limits.
    /// </summary>
    internal static Dictionary<string, string[]>? NormalizeMapping(IReadOnlyDictionary<string, string[]?>? mapping, out string? error)
    {
        error = null;
        var result = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var (target, paths) in mapping ?? new Dictionary<string, string[]?>())
        {
            if (!Targets.Contains(target))
            {
                error = $"Unknown mapping target '{target}'. Allowed: {string.Join(", ", Targets)}.";
                return null;
            }
            var list = paths ?? [];
            if (list.Length > MaxPathsPerTarget)
            {
                error = $"{target} may have at most {MaxPathsPerTarget} paths.";
                return null;
            }
            var cleaned = list.Select(p => p?.Trim()).Where(p => !string.IsNullOrEmpty(p)).Select(p => p!).ToArray();
            if (cleaned.Any(p => p.Length > MaxPathLength))
            {
                error = $"A path of {target} is longer than {MaxPathLength} characters.";
                return null;
            }
            if (cleaned.Length > 0) result[target] = cleaned;
        }
        if (!result.ContainsKey(CodeTarget))
        {
            error = "mapping must map CODE to at least one path.";
            return null;
        }
        return result;
    }

    private static async Task<XmlFeedConfigDto> ToDtoAsync(CentralApiDbContext db, TenantXmlFeedSettings? row, CancellationToken ct)
    {
        if (row is null) return new XmlFeedConfigDto();
        string? updatedBy = null;
        if (row.UpdatedByUserId is { } userId)
        {
            var user = await db.MobileUsers.AsNoTracking().Where(u => u.Id == userId)
                .Select(u => new { u.FullName, u.Username }).FirstOrDefaultAsync(ct);
            updatedBy = user is null ? null : string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;
        }
        return new XmlFeedConfigDto
        {
            Configured = true,
            Url = row.Url,
            RecordPath = row.RecordPath,
            Mapping = JsonSerializer.Deserialize<Dictionary<string, string[]>>(row.MappingJson) ?? new(),
            DownloadImages = row.DownloadImages,
            ImportDescriptions = row.ImportDescriptions,
            FullImport = row.FullImport,
            UpdatedAtUtc = row.UpdatedAtUtc,
            UpdatedByName = updatedBy,
        };
    }

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_XML_FEED_CONFIG", Message = message });
}
