using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Faz 15.5 — agent push + Android pull for the Mikro
/// <c>_ERPB_PARAMETRELER</c> snapshot. The agent reads the table on a
/// schedule and pushes the rows here; Android clients then read the
/// current snapshot without a second Mikro round-trip.
/// </summary>
public static class ParameterEndpoints
{
    /// <summary>JSON contract used by the agent and Android client.</summary>
    public sealed record ParameterDto(
        string ParametreProgram,
        string ParametreUser,
        string ParametreAnaGrubu,
        string ParametreAltGrubu,
        string ParametreID,
        string ParametreAdi,
        string ParametreDegeri);

    public static IEndpointRouteBuilder MapParameterEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/ingest/parameters").WithTags("Parameters");

        group.MapPost("", IngestAsync)
            .WithName("IngestParameters")
            .Produces(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> IngestAsync(
        [FromBody] ParameterPushBody body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null || body.Parameters is null)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "INVALID_BODY", Message = "Body and parameters list required." });
        }

        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Tenant id missing." });
        }

        if (string.IsNullOrWhiteSpace(body.SourceDatabase))
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "MISSING_SOURCE", Message = "sourceDatabase is required." });
        }

        // Upsert each parameter by (TenantId, SourceDatabase, Program, User, ID).
        // The unique index does the work; SaveChanges either inserts a new row
        // or updates ParametreDegeri (and UpdatedAtUtc) on an existing match.
        var accepted = 0;
        var updated = 0;
        var incoming = body.Parameters
            .Where(p => !string.IsNullOrWhiteSpace(p.ParametreID))
            .ToList();

        if (incoming.Count > 0)
        {
            var existing = await db.Parameters
                .Where(p => p.TenantId == tenantId && p.SourceDatabase == body.SourceDatabase)
                .ToListAsync(ct);

            var lookup = existing.ToDictionary(
                p => (p.ParametreProgram, p.ParametreUser, p.ParametreID),
                p => p);

            var now = DateTimeOffset.UtcNow;
            foreach (var dto in incoming)
            {
                var key = (dto.ParametreProgram ?? string.Empty, dto.ParametreUser ?? string.Empty, dto.ParametreID);
                if (lookup.TryGetValue(key, out var existingRow))
                {
                    if (!string.Equals(existingRow.ParametreDegeri, dto.ParametreDegeri, StringComparison.Ordinal))
                    {
                        existingRow.ParametreDegeri = dto.ParametreDegeri ?? string.Empty;
                        existingRow.ParametreAdi = dto.ParametreAdi ?? string.Empty;
                        existingRow.ParametreAnaGrubu = dto.ParametreAnaGrubu ?? string.Empty;
                        existingRow.ParametreAltGrubu = dto.ParametreAltGrubu ?? string.Empty;
                        existingRow.UpdatedAtUtc = now;
                        updated++;
                    }
                }
                else
                {
                    db.Parameters.Add(new ParameterRecord
                    {
                        TenantId = tenantId,
                        SourceDatabase = body.SourceDatabase,
                        ParametreProgram = dto.ParametreProgram ?? string.Empty,
                        ParametreUser = dto.ParametreUser ?? string.Empty,
                        ParametreAnaGrubu = dto.ParametreAnaGrubu ?? string.Empty,
                        ParametreAltGrubu = dto.ParametreAltGrubu ?? string.Empty,
                        ParametreID = dto.ParametreID,
                        ParametreAdi = dto.ParametreAdi ?? string.Empty,
                        ParametreDegeri = dto.ParametreDegeri ?? string.Empty,
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now,
                    });
                    accepted++;
                }
            }

            await db.SaveChangesAsync(ct);
        }

        return Results.Ok(new
        {
            accepted,
            updated,
            total = incoming.Count,
        });
    }

    /// <summary>JSON body for the agent push.</summary>
    public sealed record ParameterPushBody(
        string SourceDatabase,
        DateTimeOffset? PulledAtUtc,
        IReadOnlyList<ParameterDto>? Parameters);
}
