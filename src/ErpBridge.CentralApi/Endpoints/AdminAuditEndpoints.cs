using System.Globalization;
using System.Text;
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
/// Faz 15.6 / 15.8 — operator-facing read endpoints for the
/// <c>change_set_audit_log</c> append-only table. The audit log is the
/// answer to "show me every bundle the agent sent for STOKLAR between
/// 2025-01-10 and 2025-01-20"; the snapshot in
/// <c>change_sets</c> only keeps the most recent bundle.
/// </summary>
public static class AdminAuditEndpoints
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 500;

    public static IEndpointRouteBuilder MapAdminAuditEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/audit").WithTags("AdminAudit");

        group.MapGet("/changeset", ListAsync)
            .WithName("AdminAuditListChangeSet")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/changeset/{id:guid}", GetByIdAsync)
            .WithName("AdminAuditGetChangeSet")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/changeset/export.csv", ExportCsvAsync)
            .WithName("AdminAuditExportChangeSet")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] string? sourceDatabase,
        [FromQuery] string? table,
        [FromQuery] string? direction,
        [FromQuery(Name = "from")] string? fromUtc,
        [FromQuery(Name = "to")] string? toUtc,
        [FromQuery] int? page,
        [FromQuery] int? size,
        [FromServices] CentralApiDbContext db,
        HttpContext http,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });
        }

        var pageSize = Math.Clamp(size ?? DefaultPageSize, 1, MaxPageSize);
        var pageIndex = Math.Max(1, page ?? 1);
        var (from, to) = ParseDateRange(fromUtc, toUtc);

        var query = db.ChangeSetAuditEntries.AsNoTracking()
            .Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(sourceDatabase))
            query = query.Where(c => c.SourceDatabase == sourceDatabase);
        if (!string.IsNullOrWhiteSpace(table))
            query = query.Where(c => c.TableName == table);
        if (!string.IsNullOrWhiteSpace(direction))
            query = query.Where(c => c.Direction == direction);
        if (from.HasValue)
            query = query.Where(c => c.ReceivedAtUtc >= from.Value);
        if (to.HasValue)
            query = query.Where(c => c.ReceivedAtUtc <= to.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.ReceivedAtUtc)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                id = c.Id,
                sourceDatabase = c.SourceDatabase,
                table = c.TableName,
                tableKey = c.TableKey, erpType = c.ErpType,
                direction = c.Direction,
                lastTriggerRecNo = c.LastTriggerRecNo,
                rowCount = c.RowCount,
                payloadSha256 = c.PayloadSha256,
                receivedAtUtc = c.ReceivedAtUtc,
                agentId = c.AgentId,
                idempotencyKey = c.IdempotencyKey,
            })
            .ToListAsync(ct);

        return JsonResults.Ok(new
        {
            tenantId,
            page = pageIndex,
            size = pageSize,
            total,
            items,
        });
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        HttpContext http,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });
        }

        var entry = await db.ChangeSetAuditEntries.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, ct);
        if (entry is null)
        {
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "NOT_FOUND", Message = "Audit entry not found." });
        }

        // Parse the payload so the viewer can render it as a tree.
        JsonElement payloadElement;
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrEmpty(entry.PayloadJson) ? "{}" : entry.PayloadJson);
            payloadElement = JsonDocument.Parse(document.RootElement.GetRawText()).RootElement.Clone();
        }
        catch (JsonException)
        {
            payloadElement = JsonDocument.Parse("{}").RootElement.Clone();
        }

        return JsonResults.Ok(new
        {
            entry.Id,
            entry.SourceDatabase,
            entry.TableName,
            entry.TableKey,
            entry.ErpType,
            entry.Direction,
            entry.FirstTriggerRecNo,
            entry.LastTriggerRecNo,
            entry.RowCount,
            entry.PayloadSha256,
            entry.PulledAtUtc,
            entry.ReceivedAtUtc,
            entry.AgentId,
            entry.IdempotencyKey,
            payload = payloadElement,
        });
    }

    private static async Task<IResult> ExportCsvAsync(
        [FromQuery] string? sourceDatabase,
        [FromQuery] string? table,
        [FromQuery(Name = "from")] string? fromUtc,
        [FromQuery(Name = "to")] string? toUtc,
        [FromServices] CentralApiDbContext db,
        HttpContext http,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });
        }

        var (from, to) = ParseDateRange(fromUtc, toUtc);

        var query = db.ChangeSetAuditEntries.AsNoTracking()
            .Where(c => c.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(sourceDatabase))
            query = query.Where(c => c.SourceDatabase == sourceDatabase);
        if (!string.IsNullOrWhiteSpace(table))
            query = query.Where(c => c.TableName == table);
        if (from.HasValue)
            query = query.Where(c => c.ReceivedAtUtc >= from.Value);
        if (to.HasValue)
            query = query.Where(c => c.ReceivedAtUtc <= to.Value);

        // Cap to the most recent 30 days worth of rows to avoid OOMing the
        // CSV generator on a full-history export.
        var lowerBound = from ?? DateTimeOffset.UtcNow.AddDays(-30);
        query = query.Where(c => c.ReceivedAtUtc >= lowerBound);
        var rows = await query.OrderByDescending(c => c.ReceivedAtUtc).Take(50_000).ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("receivedAtUtc,sourceDatabase,table,direction,rowCount,lastTriggerRecNo,payloadSha256,agentId,idempotencyKey");
        foreach (var r in rows)
        {
            sb.Append(r.ReceivedAtUtc.UtcDateTime.ToString("o", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(CsvEscape(r.SourceDatabase)).Append(',');
            sb.Append(CsvEscape(r.TableName)).Append(',');
            sb.Append(r.Direction).Append(',');
            sb.Append(r.RowCount.ToString(CultureInfo.InvariantCulture)).Append(',');
            sb.Append(r.LastTriggerRecNo.ToString(CultureInfo.InvariantCulture)).Append(',');
            sb.Append(r.PayloadSha256).Append(',');
            sb.Append(CsvEscape(r.AgentId)).Append(',');
            sb.Append(CsvEscape(r.IdempotencyKey)).AppendLine();
        }

        return Results.File(Encoding.UTF8.GetBytes(sb.ToString()),
            "text/csv; charset=utf-8",
            $"changeset-audit-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv");
    }

    private static string CsvEscape(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        return value;
    }

    private static (DateTimeOffset? From, DateTimeOffset? To) ParseDateRange(string? fromUtc, string? toUtc)
    {
        DateTimeOffset? from = null;
        DateTimeOffset? to = null;
        if (!string.IsNullOrWhiteSpace(fromUtc) && DateTimeOffset.TryParse(fromUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var f))
            from = f.ToUniversalTime();
        if (!string.IsNullOrWhiteSpace(toUtc) && DateTimeOffset.TryParse(toUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var t))
            to = t.ToUniversalTime();
        return (from, to);
    }
}
