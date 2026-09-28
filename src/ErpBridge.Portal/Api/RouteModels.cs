using System.Text.Json.Serialization;

namespace ErpBridge.Portal.Api;

// Mirrors of ErpBridge.CentralApi/Contracts/RouteContracts.cs (GOAL_HEDEF_RUT P4–P5). Kept local on purpose.

public sealed class RouteStopDto
{
    [JsonPropertyName("stopId")] public string StopId { get; set; } = string.Empty;
    [JsonPropertyName("dayOfWeek")] public int DayOfWeek { get; set; } = 1;
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("visitOrder")] public int VisitOrder { get; set; }
}

public sealed class RoutePlanDto
{
    [JsonPropertyName("planId")] public string PlanId { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("startDate")] public string StartDate { get; set; } = string.Empty;
    [JsonPropertyName("isActive")] public bool IsActive { get; set; } = true;
    [JsonPropertyName("stops")] public List<RouteStopDto> Stops { get; set; } = [];
    [JsonPropertyName("assignees")] public List<string> Assignees { get; set; } = [];
    [JsonPropertyName("updatedBy")] public string? UpdatedBy { get; set; }
    [JsonPropertyName("updatedAtUtc")] public string? UpdatedAtUtc { get; set; }
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
    [JsonPropertyName("people")] public RoutePersonDto[] People { get; set; } = [];
    [JsonPropertyName("canPlan")] public bool CanPlan { get; set; }
    [JsonPropertyName("wholeCompany")] public bool WholeCompany { get; set; }
}

public sealed class RouteComplianceRow
{
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("teamName")] public string? TeamName { get; set; }
    [JsonPropertyName("planned")] public int Planned { get; set; }
    [JsonPropertyName("completed")] public int Completed { get; set; }
    [JsonPropertyName("skipped")] public int Skipped { get; set; }
    [JsonPropertyName("missed")] public int Missed { get; set; }
    [JsonPropertyName("unplanned")] public int Unplanned { get; set; }
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

public static class RouteDays
{
    /// <summary>1 = Monday … 7 = Sunday, the numbering route stops use.</summary>
    public static readonly IReadOnlyList<(int Day, string Short, string Long)> All =
    [
        (1, "Pzt", "Pazartesi"), (2, "Sal", "Salı"), (3, "Çar", "Çarşamba"), (4, "Per", "Perşembe"),
        (5, "Cum", "Cuma"), (6, "Cmt", "Cumartesi"), (7, "Paz", "Pazar"),
    ];

    public static int Of(DateOnly day) => day.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)day.DayOfWeek;
}
