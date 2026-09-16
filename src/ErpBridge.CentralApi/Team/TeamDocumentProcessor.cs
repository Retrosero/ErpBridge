using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Sync;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Team;

/// <summary>
/// Documents a company's field team shares whatever its data source: route plans
/// and the visits made on them (Faz 39).
///
/// <para>They are not ERP data, so they are booked here for an ERP tenant and a
/// tenant without an ERP alike — an agent has no writer for them. The result is
/// projected into <c>mobile_records</c> and reaches every phone through the one
/// read path, <c>/api/v1/android/sync/pull</c>; no table of their own is needed.</para>
///
/// <para>Every document needs a signed-in company user: a visit is recorded under
/// the caller's user name from the token, never a name in the payload, and only
/// administrators and managers plan routes.</para>
/// </summary>
public sealed class TeamDocumentProcessor
{
    public const string RoutePlan = "route_plan";
    public const string RoutePlanDelete = "route_plan_delete";
    public const string Visit = "visit";

    public static readonly IReadOnlySet<string> DocumentTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { RoutePlan, RoutePlanDelete, Visit };

    /// <summary><c>mobile_records</c> section of route plans; one row per plan with its stops and assignees.</summary>
    public const string RoutePlansSection = "routePlans";

    /// <summary><c>mobile_records</c> section of visits.</summary>
    public const string RouteVisitsSection = "routeVisits";

    /// <summary>Marks rows this class produced.</summary>
    public const string SourceName = "team";

    public const int MaxIdLength = 64;
    public const int MaxNameLength = 120;
    public const int MaxTextLength = 500;
    public const int MaxStops = 500;
    public const int MaxAssignees = 100;

    public static readonly IReadOnlySet<string> VisitStatuses =
        new HashSet<string>(StringComparer.Ordinal) { "COMPLETED", "SKIPPED" };

    private readonly MobileRecordProjector _projector;
    private readonly IBootstrapNotificationHub _hub;
    private readonly ILogger<TeamDocumentProcessor> _logger;

    public TeamDocumentProcessor(MobileRecordProjector projector, IBootstrapNotificationHub hub, ILogger<TeamDocumentProcessor> logger)
    {
        _projector = projector;
        _hub = hub;
        _logger = logger;
    }

    public static bool CanPlanRoutes(MobileUser user) =>
        user.Role == MobileUserRoles.Admin || user.Role == MobileUserRoles.Manager;

    /// <summary>
    /// Stores <paramref name="job"/> and books it. A document that cannot be booked is
    /// stored as <see cref="JobStatus.Failed"/> with the reason, so the phone does not
    /// retry it forever and support sees why. Infrastructure errors throw and roll back.
    /// </summary>
    public async Task<Job> IngestAsync(CentralApiDbContext db, Guid tenantId, Job job, MobileUser caller, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(caller);

        var ownsTransaction = db.Database.IsRelational() && db.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction ? await db.Database.BeginTransactionAsync(ct) : null;

        var now = DateTimeOffset.UtcNow;
        var rows = new List<MobileRecordProjector.SectionRows>();
        var tombstones = new List<MobileRecord>();
        string? error;
        string? note = null;
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.PayloadJson) ? "{}" : job.PayloadJson))
        {
            var root = document.RootElement;
            switch (job.DocumentType.ToLowerInvariant())
            {
                case RoutePlan:
                    error = CanPlanRoutes(caller)
                        ? await BuildPlanAsync(db, tenantId, root, caller, now, rows, ct)
                        : "Only company administrators and managers can plan routes.";
                    break;
                case RoutePlanDelete:
                    if (!CanPlanRoutes(caller))
                    {
                        error = "Only company administrators and managers can delete routes.";
                        break;
                    }
                    (error, note) = await FindPlanToDeleteAsync(db, tenantId, root, tombstones, ct);
                    break;
                case Visit:
                    error = BuildVisit(root, caller, now, rows);
                    break;
                default:
                    error = $"{job.DocumentType} is not a team document.";
                    break;
            }
        }

        job.Status = error is null ? JobStatus.Succeeded : JobStatus.Failed;
        job.LastError = error ?? note;
        job.CompletedAtUtc = now;
        db.Jobs.Add(job);
        await db.SaveChangesAsync(ct);

        var devicesAffected = false;
        if (error is null && rows.Count > 0)
        {
            var projected = await _projector.ProjectAsync(db, tenantId, rows, fullUpload: false, SourceName, ct);
            await db.SaveChangesAsync(ct);
            devicesAffected = projected.Changed > 0;
        }
        if (error is null && tombstones.Count > 0)
        {
            devicesAffected |= await _projector.TombstoneAsync(db, tenantId, tombstones, ct) > 0;
            await db.SaveChangesAsync(ct);
        }

        if (transaction is not null) await transaction.CommitAsync(ct);

        if (devicesAffected && ownsTransaction) _hub.Publish(tenantId, now);
        else if (error is not null)
            _logger.LogWarning("Team {DocumentType} {ExternalId} for tenant {TenantId} was not booked: {Error}",
                job.DocumentType, job.ExternalId, tenantId, error);
        return job;
    }

    // ---- route plans -------------------------------------------------------

    private static async Task<string?> BuildPlanAsync(
        CentralApiDbContext db, Guid tenantId, JsonElement plan, MobileUser caller, DateTimeOffset now,
        List<MobileRecordProjector.SectionRows> rows, CancellationToken ct)
    {
        var planId = Text(plan, "planId");
        if (planId is null || planId.Length > MaxIdLength) return $"planId is required (at most {MaxIdLength} characters).";
        var name = Text(plan, "name");
        if (name is null || name.Length > MaxNameLength) return $"name is required (at most {MaxNameLength} characters).";
        var description = Text(plan, "description") ?? string.Empty;
        if (description.Length > MaxTextLength) return $"description is at most {MaxTextLength} characters.";
        var startDate = Text(plan, "startDate") ?? string.Empty;
        if (startDate.Length > 0 && !IsDate(startDate)) return "startDate must be yyyy-MM-dd.";

        var stops = new JsonArray();
        var stopIds = new HashSet<string>(StringComparer.Ordinal);
        if (plan.TryGetProperty("stops", out var stopsElement) && stopsElement.ValueKind == JsonValueKind.Array)
        {
            if (stopsElement.GetArrayLength() > MaxStops) return $"A route plan has at most {MaxStops} stops.";
            var index = 0;
            foreach (var stop in stopsElement.EnumerateArray())
            {
                index++;
                var stopId = Text(stop, "stopId");
                if (stopId is null || stopId.Length > MaxIdLength) return $"Stop {index}: stopId is required (at most {MaxIdLength} characters).";
                if (!stopIds.Add(stopId)) return $"Stop {index}: stopId {stopId} appears twice.";
                var day = Integer(stop, "dayOfWeek");
                if (day is null or < 1 or > 7) return $"Stop {index}: dayOfWeek must be 1 (Monday) to 7 (Sunday).";
                var customerCode = Text(stop, "customerCode");
                if (customerCode is null || customerCode.Length > MaxIdLength) return $"Stop {index}: customerCode is required.";
                var visitOrder = Integer(stop, "visitOrder") ?? 0;
                if (visitOrder < 0) return $"Stop {index}: visitOrder cannot be negative.";
                stops.Add(new JsonObject
                {
                    ["stopId"] = stopId,
                    ["dayOfWeek"] = day,
                    ["customerCode"] = customerCode,
                    ["customerName"] = Text(stop, "customerName") ?? string.Empty,
                    ["visitOrder"] = visitOrder,
                });
            }
        }

        var assignees = new List<string>();
        if (plan.TryGetProperty("assignees", out var assigneesElement) && assigneesElement.ValueKind == JsonValueKind.Array)
        {
            if (assigneesElement.GetArrayLength() > MaxAssignees) return $"A route plan has at most {MaxAssignees} assignees.";
            foreach (var assignee in assigneesElement.EnumerateArray())
            {
                var username = assignee.ValueKind == JsonValueKind.String ? assignee.GetString()?.Trim() : null;
                if (string.IsNullOrEmpty(username)) return "assignees must be user names.";
                if (!assignees.Contains(username, StringComparer.OrdinalIgnoreCase)) assignees.Add(username);
            }
        }
        if (assignees.Count > 0)
        {
            // A misspelt name would leave the route on nobody's phone without anyone noticing.
            var known = await db.MobileUsers.AsNoTracking()
                .Where(u => u.TenantId == tenantId && u.DeletedAtUtc == null)
                .Select(u => u.Username)
                .ToListAsync(ct);
            var unknown = assignees.Where(a => !known.Contains(a, StringComparer.OrdinalIgnoreCase)).ToList();
            if (unknown.Count > 0) return $"Unknown users: {string.Join(", ", unknown)}.";
        }

        var row = new JsonObject
        {
            ["planId"] = planId,
            ["name"] = name,
            ["description"] = description,
            ["startDate"] = startDate,
            ["isActive"] = Bool(plan, "isActive") ?? true,
            ["stops"] = stops,
            ["assignees"] = new JsonArray(assignees.Select(a => (JsonNode?)JsonValue.Create(a)).ToArray()),
            ["updatedBy"] = caller.Username,
            ["updatedAtUtc"] = now.ToString("O", CultureInfo.InvariantCulture),
        };
        rows.Add(Section(RoutePlansSection, row));
        return null;
    }

    private static async Task<(string? Error, string? Note)> FindPlanToDeleteAsync(
        CentralApiDbContext db, Guid tenantId, JsonElement body, List<MobileRecord> tombstones, CancellationToken ct)
    {
        var planId = Text(body, "planId");
        if (planId is null || planId.Length > MaxIdLength) return ($"planId is required (at most {MaxIdLength} characters).", null);
        var existing = await db.MobileRecords
            .Where(r => r.TenantId == tenantId && r.Entity == RoutePlansSection && r.RecordKey == planId && !r.IsDeleted)
            .ToListAsync(ct);
        // A plan created and deleted before its first upload never reached the server;
        // deleting it again is the outcome the phone wanted, not a failure.
        if (existing.Count == 0) return (null, $"Route plan {planId} was not on the server; nothing to delete.");
        tombstones.AddRange(existing);
        return (null, null);
    }

    // ---- visits -------------------------------------------------------------

    private static string? BuildVisit(JsonElement visit, MobileUser caller, DateTimeOffset now, List<MobileRecordProjector.SectionRows> rows)
    {
        var visitId = Text(visit, "visitId");
        if (visitId is null || visitId.Length > MaxIdLength) return $"visitId is required (at most {MaxIdLength} characters).";
        var customerCode = Text(visit, "customerCode");
        if (customerCode is null || customerCode.Length > MaxIdLength) return "customerCode is required.";
        var visitDate = Text(visit, "visitDate");
        if (visitDate is null || !IsDate(visitDate)) return "visitDate is required as yyyy-MM-dd.";
        var status = Text(visit, "status")?.ToUpperInvariant();
        if (status is null || !VisitStatuses.Contains(status)) return "status must be COMPLETED or SKIPPED.";
        var noteText = Text(visit, "note") ?? string.Empty;
        if (noteText.Length > MaxTextLength) return $"note is at most {MaxTextLength} characters.";
        var planId = Text(visit, "planId") ?? string.Empty;
        var stopId = Text(visit, "stopId") ?? string.Empty;
        if (planId.Length > MaxIdLength || stopId.Length > MaxIdLength) return $"planId and stopId are at most {MaxIdLength} characters.";
        var completedAt = Long(visit, "completedAt");
        if (completedAt is < 0) return "completedAt cannot be negative.";

        rows.Add(Section(RouteVisitsSection, new JsonObject
        {
            ["visitId"] = visitId,
            ["planId"] = planId,
            ["stopId"] = stopId,
            ["customerCode"] = customerCode,
            // Who visited comes from the token: a phone cannot record a visit for someone else.
            ["username"] = caller.Username,
            ["visitDate"] = visitDate,
            ["status"] = status,
            ["note"] = noteText,
            ["completedAt"] = completedAt ?? now.ToUnixTimeMilliseconds(),
            ["receivedAtUtc"] = now.ToString("O", CultureInfo.InvariantCulture),
        }));
        return null;
    }

    // ---- helpers ------------------------------------------------------------

    private static MobileRecordProjector.SectionRows Section(string section, JsonObject row)
    {
        using var parsed = JsonDocument.Parse(row.ToJsonString());
        return new MobileRecordProjector.SectionRows(section, [parsed.RootElement.Clone()]);
    }

    private static bool IsDate(string value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    private static string? Text(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(name, out var value)
        && value.ValueKind is JsonValueKind.String or JsonValueKind.Number
            ? value.ToString().Trim() is { Length: > 0 } text ? text : null
            : null;

    private static int? Integer(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(name, out var value)
        && value.ValueKind == JsonValueKind.Number
        && value.TryGetInt32(out var number)
            ? number
            : null;

    private static long? Long(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(name, out var value)
        && value.ValueKind == JsonValueKind.Number
        && value.TryGetInt64(out var number)
            ? number
            : null;

    private static bool? Bool(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(name, out var value)
        && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;
}
