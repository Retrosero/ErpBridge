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
        return routes;
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
