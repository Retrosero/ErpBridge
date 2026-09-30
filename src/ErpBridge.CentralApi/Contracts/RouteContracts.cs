using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

// GOAL_HEDEF_RUT P4–P5: /api/v1/portal/routes. A plan has the shape the phones already use (`route_plan`, KB rule 17).

public sealed class RouteStopDto
{
    [JsonPropertyName("stopId")] public string StopId { get; set; } = string.Empty;

    /// <summary>1 = Monday … 7 = Sunday.</summary>
    [JsonPropertyName("dayOfWeek")] public int DayOfWeek { get; set; }

    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;

    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("visitOrder")] public int VisitOrder { get; set; }
}

public sealed class RoutePlanDto
{
    [JsonPropertyName("planId")] public string PlanId { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

    /// <summary><c>yyyy-MM-dd</c>, or empty: from the start.</summary>
    [JsonPropertyName("startDate")] public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("isActive")] public bool IsActive { get; set; } = true;

    [JsonPropertyName("stops")] public RouteStopDto[] Stops { get; set; } = [];

    /// <summary>User names.</summary>
    [JsonPropertyName("assignees")] public string[] Assignees { get; set; } = [];

    [JsonPropertyName("updatedBy")] public string? UpdatedBy { get; set; }

    [JsonPropertyName("updatedAtUtc")] public string? UpdatedAtUtc { get; set; }

    /// <summary>Whether the caller may change or delete it (every assignee in their scope).</summary>
    [JsonPropertyName("canEdit")] public bool CanEdit { get; set; }
}

public sealed class RoutePersonDto
{
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("teamName")] public string? TeamName { get; set; }
}

public sealed class RoutePlansResponse
{
    [JsonPropertyName("plans")] public RoutePlanDto[] Plans { get; set; } = [];

    /// <summary>The people the caller may assign.</summary>
    [JsonPropertyName("people")] public RoutePersonDto[] People { get; set; } = [];

    [JsonPropertyName("canPlan")] public bool CanPlan { get; set; }

    [JsonPropertyName("wholeCompany")] public bool WholeCompany { get; set; }
}

public sealed class RouteComplianceRow
{
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("teamName")] public string? TeamName { get; set; }

    /// <summary>Planned stops in the range.</summary>
    [JsonPropertyName("planned")] public int Planned { get; set; }

    /// <summary>Planned stops visited.</summary>
    [JsonPropertyName("completed")] public int Completed { get; set; }

    [JsonPropertyName("skipped")] public int Skipped { get; set; }

    /// <summary>Planned stops of past days with nothing recorded.</summary>
    [JsonPropertyName("missed")] public int Missed { get; set; }

    /// <summary>Visits recorded outside the plan.</summary>
    [JsonPropertyName("unplanned")] public int Unplanned { get; set; }

    /// <summary>Completed ÷ planned × 100; null without planned stops.</summary>
    [JsonPropertyName("compliance")] public decimal? Compliance { get; set; }
}

public sealed class RouteComplianceDay
{
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;

    [JsonPropertyName("planned")] public int Planned { get; set; }

    [JsonPropertyName("completed")] public int Completed { get; set; }

    [JsonPropertyName("skipped")] public int Skipped { get; set; }

    [JsonPropertyName("missed")] public int Missed { get; set; }
}

public sealed class RouteComplianceResponse
{
    [JsonPropertyName("from")] public string From { get; set; } = string.Empty;

    [JsonPropertyName("to")] public string To { get; set; } = string.Empty;

    [JsonPropertyName("rows")] public RouteComplianceRow[] Rows { get; set; } = [];

    [JsonPropertyName("days")] public RouteComplianceDay[] Days { get; set; } = [];
}
