using System.Text.Json;
using System.Text.Json.Nodes;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>POST /api/v1/bootstrap</c>. Persists the caller's reference-data
/// snapshot as a <see cref="BootstrapPackage"/> row scoped to the tenant from
/// the JWT.
/// </summary>
public static class BootstrapEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Register an <see cref="IEndpointRouteBuilder"/> extension that maps the bootstrap endpoint.</summary>
    public static IEndpointRouteBuilder MapBootstrapEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/bootstrap", BootstrapAsync)
            .WithName("Bootstrap")
            .WithTags("Bootstrap")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Program.AgentPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);
        routes.MapGet("/api/v1/bootstrap/status", StatusAsync)
            .WithName("BootstrapStatus")
            .WithTags("Bootstrap")
            .Produces<BootstrapStatusResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Program.AgentPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);
        return routes;
    }

    private static async Task<IResult> StatusAsync(
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "JWT missing tenant claim." });

        var lastPulledAtUtc = await db.BootstrapPackages.AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderByDescending(item => item.PulledAtUtc)
            .Select(item => (DateTimeOffset?)item.PulledAtUtc)
            .FirstOrDefaultAsync(ct);
        return JsonResults.Ok(new BootstrapStatusResponse
        {
            HasSnapshot = lastPulledAtUtc.HasValue,
            LastPulledAtUtc = lastPulledAtUtc,
        });
    }

    private static async Task<IResult> BootstrapAsync(
        [FromBody] BootstrapRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null) return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });
        if (string.IsNullOrWhiteSpace(body.SourceDatabase))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_SOURCE", Message = "sourceDatabase is required." });

        if (!http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "JWT missing tenant claim." });

        var payloadJson = body.Payload is null
            ? "{}"
            : JsonSerializer.Serialize(body.Payload, JsonOptions);

        // A manual per-section push contains empty arrays for every unrelated
        // section. Merge only the requested section into the previous snapshot
        // so sending one table cannot make all other tables disappear.
        var partialSection = TryGetPartialSection(payloadJson);
        if (partialSection is not null || IsIncremental(payloadJson))
        {
            var previous = await db.BootstrapPackages.AsNoTracking()
                .Where(item => item.TenantId == tenantId)
                .OrderByDescending(item => item.PulledAtUtc)
                .ThenByDescending(item => item.ReceivedAtUtc)
                .FirstOrDefaultAsync(ct);
            if (previous is not null)
                payloadJson = partialSection is not null
                    ? MergePartialPayload(previous.PayloadJson, payloadJson, partialSection)
                    : MergeIncrementalPayload(previous.PayloadJson, payloadJson);
        }

        var package = new BootstrapPackage
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PayloadJson = payloadJson,
            SourceDatabase = body.SourceDatabase,
            PulledAtUtc = body.PulledAtUtc,
            ReceivedAtUtc = DateTimeOffset.UtcNow,
        };
        db.BootstrapPackages.Add(package);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static string? TryGetPartialSection(string payloadJson)
    {
        try
        {
            var node = JsonNode.Parse(payloadJson) as JsonObject;
            return node?["partialSection"]?.GetValue<string>();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool IsIncremental(string payloadJson)
    {
        try
        {
            var node = JsonNode.Parse(payloadJson) as JsonObject;
            return node?["isIncremental"]?.GetValue<bool>() == true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string MergePartialPayload(string previousJson, string partialJson, string section)
    {
        var previous = JsonNode.Parse(previousJson) as JsonObject ?? new JsonObject();
        var partial = JsonNode.Parse(partialJson) as JsonObject ?? new JsonObject();
        var keys = section.ToLowerInvariant() switch
        {
            "customers" => new[] { "customers", "customerAddresses", "customerContacts" },
            "stocks" => new[] { "stocks", "barcodes" },
            "prices" => new[] { "prices", "salesConditions" },
            "openorders" => new[] { "openOrders" },
            "cashandbank" => new[] { "cashAndBank" },
            "lookups" => new[] { "lookups" },
            "inventory" => new[] { "inventory" },
            "customertransactions" or "carihareketleri" => new[] { "customerTransactions" },
            "stocktransactions" or "stokhareket" or "stokhareketleri" => new[] { "stockTransactions" },
            _ => Array.Empty<string>(),
        };

        foreach (var key in keys)
        {
            if (TryTakeProperty(partial, key, out var value))
                SetProperty(previous, key, value);
        }

        previous["pulledAtUtc"] = partial["pulledAtUtc"]?.DeepClone();
        previous["sourceDatabase"] = partial["sourceDatabase"]?.DeepClone();
        previous.Remove("partialSection");
        return previous.ToJsonString(JsonOptions);
    }

    private static string MergeIncrementalPayload(string previousJson, string incrementalJson)
    {
        var previous = JsonNode.Parse(previousJson) as JsonObject ?? new JsonObject();
        var incremental = JsonNode.Parse(incrementalJson) as JsonObject ?? new JsonObject();
        foreach (var property in incremental)
        {
            if (property.Key is "isIncremental" or "changedSinceUtc") continue;
            if (property.Value is not JsonArray incoming)
            {
                previous[property.Key] = property.Value?.DeepClone();
                continue;
            }

            var existing = FindProperty(previous, property.Key) as JsonArray ?? new JsonArray();
            var keys = existing
                .OfType<JsonObject>()
                .Select(item => (Key: RowKey(property.Key, item), Item: item))
                .Where(item => item.Key is not null)
                .ToDictionary(item => item.Key!, item => item.Item, StringComparer.OrdinalIgnoreCase);
            foreach (var item in incoming)
            {
                if (item is not JsonObject objectItem)
                {
                    existing.Add(item?.DeepClone());
                    continue;
                }

                var key = RowKey(property.Key, objectItem);
                if (key is not null && keys.TryGetValue(key, out var oldItem))
                    existing.Remove(oldItem);
                existing.Add(objectItem.DeepClone());
            }
            SetProperty(previous, property.Key, existing);
        }

        previous.Remove("isIncremental");
        previous.Remove("changedSinceUtc");
        return previous.ToJsonString(JsonOptions);
    }

    private static JsonNode? FindProperty(JsonObject source, string key) =>
        source.FirstOrDefault(property => string.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase)).Value;

    private static string? RowKey(string collection, JsonObject item)
    {
        static string? Value(JsonObject obj, string key)
        {
            var value = obj.FirstOrDefault(property => string.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase)).Value;
            return value is JsonValue ? value.ToJsonString().Trim('"') : null;
        }
        static string? Key(JsonObject obj, params string[] fields)
        {
            var values = fields.Select(field => Value(obj, field)).ToArray();
            return values.Any(string.IsNullOrWhiteSpace) ? null : string.Join('|', values);
        }

        return collection.ToLowerInvariant() switch
        {
            "customers" => Key(item, "customerCode"),
            "customeraddresses" => Key(item, "customerCode", "addressNo"),
            "customercontacts" => Key(item, "customerCode", "email", "mobile"),
            "stocks" => Key(item, "stockCode"),
            "barcodes" => Key(item, "barcode"),
            "prices" => Key(item, "stockCode", "listNumber"),
            "salesconditions" => Key(item, "stockCode", "customerCode", "warehouseNo", "paymentPlanNo"),
            "inventory" => Key(item, "stockCode", "warehouseNo"),
            "openorders" => Key(item, "series", "number", "lineNo"),
            "cashandbank" => Key(item, "kind", "code"),
            "lookups" => Key(item, "kind", "code"),
            "customertransactions" or "stocktransactions" => Key(item, "id"),
            _ => null,
        };
    }

    private static bool TryTakeProperty(JsonObject source, string key, out JsonNode? value)
    {
        var match = source.FirstOrDefault(property =>
            string.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase));
        value = match.Value?.DeepClone();
        return match.Key is not null;
    }

    private static void SetProperty(JsonObject target, string key, JsonNode? value)
    {
        var existing = target.FirstOrDefault(property =>
            string.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase));
        if (existing.Key is not null && existing.Key != key)
            target.Remove(existing.Key);
        target[key] = value;
    }
}
