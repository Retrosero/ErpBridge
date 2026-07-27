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
        routes.MapGet("/api/v1/bootstrap/state", StateAsync)
            .WithName("BootstrapState").WithTags("Bootstrap")
            .RequireAuthorization(Program.AgentPolicy);
        routes.MapPost("/api/v1/bootstrap/delta", DeltaAsync)
            .WithName("BootstrapDelta").WithTags("Bootstrap")
            .RequireAuthorization(Program.AgentPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);
        return routes;
    }

    private static async Task<IResult> StateAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId)) return Results.Unauthorized();
        var latest = await db.BootstrapPackages.AsNoTracking().Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ReceivedAtUtc).FirstOrDefaultAsync(ct);
        return Results.Ok(new
        {
            exists = latest is not null,
            receivedAtUtc = latest?.ReceivedAtUtc,
            revision = latest is null ? null : $"{latest.Id:N}:{latest.ReceivedAtUtc.ToUnixTimeMilliseconds()}"
        });
    }

    private static async Task<IResult> DeltaAsync(BootstrapDeltaRequest body, HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId)) return Results.Unauthorized();
        if (body.Delta is null || string.IsNullOrWhiteSpace(body.SourceDatabase))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_DELTA", Message = "Delta and sourceDatabase are required." });
        var previous = await db.BootstrapPackages.Where(x => x.TenantId == tenantId).OrderByDescending(x => x.ReceivedAtUtc).FirstOrDefaultAsync(ct);
        if (previous is null)
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError { ErrorCode = "BOOTSTRAP_REQUIRED", Message = "A full bootstrap is required before a delta can be applied." });
        previous.PayloadJson = ApplyDelta(previous.PayloadJson, body.Delta);
        previous.SourceDatabase = body.SourceDatabase;
        previous.PulledAtUtc = body.PulledAtUtc;
        previous.ReceivedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static string ApplyDelta(string snapshotJson, BootstrapDeltaBody delta)
    {
        var root = JsonNode.Parse(snapshotJson) as JsonObject ?? new JsonObject();
        foreach (var section in delta.Upserts)
        {
            var keys = section.Value.Select(x => x.Key).ToHashSet(StringComparer.Ordinal);
            var current = root[section.Key] as JsonArray ?? new JsonArray();
            // Existing rows without a stored key are matched through their stable
            // natural key; delta keys use the same field concatenation convention.
            var retained = new JsonArray();
            foreach (var node in current)
                if (!keys.Contains(RowKey(section.Key, node as JsonObject))) retained.Add(node?.DeepClone());
            foreach (var row in section.Value) retained.Add(JsonNode.Parse(row.PayloadJson));
            root[section.Key] = retained;
        }
        foreach (var section in delta.Deletes)
        {
            if (root[section.Key] is not JsonArray current) continue;
            var deleted = section.Value.ToHashSet(StringComparer.Ordinal);
            var retained = new JsonArray();
            foreach (var node in current)
                if (!deleted.Contains(RowKey(section.Key, node as JsonObject))) retained.Add(node?.DeepClone());
            root[section.Key] = retained;
        }
        root.Remove("partialSection");
        return root.ToJsonString(JsonOptions);
    }

    private static string RowKey(string section, JsonObject? row)
    {
        if (row is null) return string.Empty;
        string Get(string name) => row[name]?.ToString() ?? string.Empty;
        return section.ToLowerInvariant() switch
        {
            "customers" => Get("customerCode"), "customeraddresses" => Get("customerCode") + "|" + Get("addressNo"),
            "customercontacts" => Get("customerCode") + "|" + Get("email") + "|" + Get("mobile"), "stocks" => Get("stockCode"),
            "barcodes" => Get("barcode"), "prices" => Get("stockCode") + "|" + Get("listNumber"),
            "salesconditions" => Get("stockCode") + "|" + Get("customerCode") + "|" + Get("warehouseNo") + "|" + Get("paymentPlanNo") + "|" + Get("startDate") + "|" + Get("endDate"),
            "inventory" => Get("stockCode") + "|" + Get("warehouseNo"),
            "openorders" => Get("series") + "|" + Get("number") + "|" + Get("lineNo"),
            "customertransactions" or "stocktransactions" => Get("erpRef"), "cashandbank" => Get("kind") + "|" + Get("code"),
            "lookups" => Get("kind") + "|" + Get("code"), _ => string.Empty
        };
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
        if (partialSection is not null)
        {
            var previous = await db.BootstrapPackages.AsNoTracking()
                .Where(item => item.TenantId == tenantId)
                .OrderByDescending(item => item.PulledAtUtc)
                .ThenByDescending(item => item.ReceivedAtUtc)
                .FirstOrDefaultAsync(ct);
            if (previous is not null)
                payloadJson = MergePartialPayload(previous.PayloadJson, payloadJson, partialSection);
        }

        var package = await db.BootstrapPackages
            .Where(item => item.TenantId == tenantId)
            .OrderByDescending(item => item.ReceivedAtUtc)
            .FirstOrDefaultAsync(ct);
        if (package is null)
        {
            package = new BootstrapPackage { TenantId = tenantId };
            db.BootstrapPackages.Add(package);
        }
        package.PayloadJson = payloadJson;
        package.SourceDatabase = body.SourceDatabase;
        package.PulledAtUtc = body.PulledAtUtc;
        package.ReceivedAtUtc = DateTimeOffset.UtcNow;
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

        var accountKind = section.ToLowerInvariant() switch
        {
            "kasalar" => "cash",
            "bankalar" => "bank",
            _ => null,
        };
        if (accountKind is not null)
        {
            // KASALAR and BANKALAR share one payload array. Replacing only
            // the requested type prevents a bank-only push from deleting cash.
            MergeCashAndBankKind(previous, partial, accountKind);
            previous["pulledAtUtc"] = partial["pulledAtUtc"]?.DeepClone();
            previous["sourceDatabase"] = partial["sourceDatabase"]?.DeepClone();
            previous.Remove("partialSection");
            return previous.ToJsonString(JsonOptions);
        }

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

    private static void MergeCashAndBankKind(JsonObject previous, JsonObject partial, string accountKind)
    {
        var existing = TryTakeProperty(previous, "cashAndBank", out var previousNode)
            ? previousNode as JsonArray : null;
        var incoming = TryTakeProperty(partial, "cashAndBank", out var partialNode)
            ? partialNode as JsonArray : null;
        var merged = new JsonArray();

        if (existing is not null)
            foreach (var item in existing)
                if (!string.Equals(item?["kind"]?.GetValue<string>(), accountKind, StringComparison.OrdinalIgnoreCase))
                    merged.Add(item?.DeepClone());
        if (incoming is not null)
            foreach (var item in incoming)
                merged.Add(item?.DeepClone());

        SetProperty(previous, "cashAndBank", merged);
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
