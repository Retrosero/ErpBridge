using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

// GOAL_HEDEF_RUT: /api/v1/portal/teams, /api/v1/portal/targets, /api/v1/android/targets.
// Days are Istanbul calendar days, "yyyy-MM-dd"; times are unix milliseconds (UTC).

// ---- teams ----------------------------------------------------------------------------------

public sealed class TeamDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary><c>REGION</c> or <c>TEAM</c>.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;

    /// <summary>The region of a team.</summary>
    [JsonPropertyName("parentId")] public Guid? ParentId { get; set; }

    [JsonPropertyName("isActive")] public bool IsActive { get; set; }

    /// <summary>Members of a team; always empty on a region.</summary>
    [JsonPropertyName("memberIds")] public Guid[] MemberIds { get; set; } = [];

    [JsonPropertyName("managerIds")] public Guid[] ManagerIds { get; set; } = [];
}

public sealed class TeamPersonDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("roles")] public string[] Roles { get; set; } = [];

    [JsonPropertyName("teamId")] public Guid? TeamId { get; set; }

    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
}

public sealed class TeamsResponse
{
    [JsonPropertyName("teams")] public TeamDto[] Teams { get; set; } = [];

    /// <summary>The people the caller sees.</summary>
    [JsonPropertyName("people")] public TeamPersonDto[] People { get; set; } = [];

    /// <summary>Only an administrator changes the team structure.</summary>
    [JsonPropertyName("canEdit")] public bool CanEdit { get; set; }

    [JsonPropertyName("wholeCompany")] public bool WholeCompany { get; set; }
}

public sealed class TeamSaveRequest
{
    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("kind")] public string? Kind { get; set; }

    [JsonPropertyName("parentId")] public Guid? ParentId { get; set; }

    [JsonPropertyName("isActive")] public bool? IsActive { get; set; }

    /// <summary>The whole member list; a person in another team moves here. Null leaves members as they are.</summary>
    [JsonPropertyName("memberIds")] public Guid[]? MemberIds { get; set; }

    /// <summary>The whole list of responsible managers. Null leaves them as they are.</summary>
    [JsonPropertyName("managerIds")] public Guid[]? ManagerIds { get; set; }
}

// ---- targets ----------------------------------------------------------------------------------

public sealed class TargetInput
{
    /// <summary><c>DAILY</c>, <c>WEEKLY</c> or <c>MONTHLY</c>.</summary>
    [JsonPropertyName("periodType")] public string? PeriodType { get; set; }

    /// <summary><c>2026-10-05</c>, <c>2026-W41</c> or <c>2026-10</c>.</summary>
    [JsonPropertyName("periodKey")] public string? PeriodKey { get; set; }

    [JsonPropertyName("metric")] public string? Metric { get; set; }

    /// <summary>Optional: the metric's default measure when absent.</summary>
    [JsonPropertyName("measure")] public string? Measure { get; set; }

    [JsonPropertyName("itemCode")] public string? ItemCode { get; set; }

    [JsonPropertyName("itemName")] public string? ItemName { get; set; }

    /// <summary><c>USER</c>, <c>TEAM</c> or <c>COMPANY</c>.</summary>
    [JsonPropertyName("ownerKind")] public string? OwnerKind { get; set; }

    /// <summary>The user or team; absent for the company.</summary>
    [JsonPropertyName("ownerId")] public Guid? OwnerId { get; set; }

    /// <summary>Null deletes the target.</summary>
    [JsonPropertyName("value")] public decimal? Value { get; set; }

    [JsonPropertyName("note")] public string? Note { get; set; }
}

public sealed class TargetsSaveRequest
{
    /// <summary>Made by the panel per save; a repeated save with the same id is not applied twice.</summary>
    [JsonPropertyName("operationId")] public Guid OperationId { get; set; }

    [JsonPropertyName("items")] public TargetInput[] Items { get; set; } = [];
}

public sealed class TargetErrorDto
{
    /// <summary>Index of the item in the request.</summary>
    [JsonPropertyName("index")] public int Index { get; set; }

    [JsonPropertyName("errorCode")] public string ErrorCode { get; set; } = string.Empty;

    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
}

public sealed class TargetsSaveResponse
{
    [JsonPropertyName("saved")] public int Saved { get; set; }

    [JsonPropertyName("deleted")] public int Deleted { get; set; }

    [JsonPropertyName("unchanged")] public int Unchanged { get; set; }

    /// <summary>The operation was applied before; nothing was written now.</summary>
    [JsonPropertyName("duplicate")] public bool Duplicate { get; set; }

    [JsonPropertyName("errors")] public TargetErrorDto[] Errors { get; set; } = [];
}

public sealed class TargetCopyRequest
{
    [JsonPropertyName("periodType")] public string? PeriodType { get; set; }

    [JsonPropertyName("fromPeriodKey")] public string? FromPeriodKey { get; set; }

    [JsonPropertyName("toPeriodKey")] public string? ToPeriodKey { get; set; }

    /// <summary>Change applied to each value, in percent (-100 … 1000).</summary>
    [JsonPropertyName("percent")] public decimal Percent { get; set; }

    /// <summary>Only these owner kinds; all when empty.</summary>
    [JsonPropertyName("ownerKinds")] public string[]? OwnerKinds { get; set; }

    /// <summary>Replace targets the new period already has.</summary>
    [JsonPropertyName("overwrite")] public bool Overwrite { get; set; }

    /// <summary>False: only the preview.</summary>
    [JsonPropertyName("apply")] public bool Apply { get; set; }

    [JsonPropertyName("operationId")] public Guid OperationId { get; set; }
}

public sealed class TargetDistributeRequest
{
    [JsonPropertyName("periodType")] public string? PeriodType { get; set; }

    [JsonPropertyName("periodKey")] public string? PeriodKey { get; set; }

    [JsonPropertyName("metric")] public string? Metric { get; set; }

    [JsonPropertyName("measure")] public string? Measure { get; set; }

    [JsonPropertyName("itemCode")] public string? ItemCode { get; set; }

    [JsonPropertyName("itemName")] public string? ItemName { get; set; }

    /// <summary><c>COMPANY</c> or <c>TEAM</c>: whose value is shared out.</summary>
    [JsonPropertyName("sourceOwnerKind")] public string? SourceOwnerKind { get; set; }

    [JsonPropertyName("sourceOwnerId")] public Guid? SourceOwnerId { get; set; }

    /// <summary>The value to share; absent: the source's current target.</summary>
    [JsonPropertyName("value")] public decimal? Value { get; set; }

    /// <summary><c>EQUAL</c> or <c>LAST_PERIOD_SHARE</c>.</summary>
    [JsonPropertyName("method")] public string? Method { get; set; }

    /// <summary><c>USER</c> (the people under the source) or <c>TEAM</c> (the teams or regions right under it).</summary>
    [JsonPropertyName("targetLevel")] public string? TargetLevel { get; set; }
}

public sealed class TargetPreviewResponse
{
    [JsonPropertyName("items")] public TargetInput[] Items { get; set; } = [];

    /// <summary>Filled when the request asked to apply.</summary>
    [JsonPropertyName("result")] public TargetsSaveResponse? Result { get; set; }
}

public sealed class TargetRowDto
{
    /// <summary>Null on a derived target (<see cref="Derived"/>).</summary>
    [JsonPropertyName("id")] public Guid? Id { get; set; }

    [JsonPropertyName("periodType")] public string PeriodType { get; set; } = string.Empty;

    [JsonPropertyName("periodKey")] public string PeriodKey { get; set; } = string.Empty;

    [JsonPropertyName("metric")] public string Metric { get; set; } = string.Empty;

    [JsonPropertyName("measure")] public string Measure { get; set; } = string.Empty;

    [JsonPropertyName("itemCode")] public string ItemCode { get; set; } = string.Empty;

    [JsonPropertyName("itemName")] public string? ItemName { get; set; }

    [JsonPropertyName("value")] public decimal Value { get; set; }

    [JsonPropertyName("note")] public string? Note { get; set; }

    [JsonPropertyName("actual")] public decimal Actual { get; set; }

    /// <summary>ERP companies: on its way to the ERP, not in <see cref="Actual"/> (K8).</summary>
    [JsonPropertyName("pending")] public decimal Pending { get; set; }

    /// <summary>Actual ÷ value × 100; null when the value is 0.</summary>
    [JsonPropertyName("percent")] public decimal? Percent { get; set; }

    /// <summary>What the value would be by today at an even pace over working days.</summary>
    [JsonPropertyName("expectedToDate")] public decimal ExpectedToDate { get; set; }

    /// <summary>Where the period ends at today's pace.</summary>
    [JsonPropertyName("forecast")] public decimal Forecast { get; set; }

    /// <summary>What is left, per remaining working day (today included); null when none is left.</summary>
    [JsonPropertyName("requiredPerDay")] public decimal? RequiredPerDay { get; set; }

    /// <summary>
    /// Not entered but worked out: a day's share of the week/month target (K9) or a period's planned route stops (K10).
    /// </summary>
    [JsonPropertyName("derived")] public bool Derived { get; set; }

    /// <summary>Team and company targets: the sum of the same targets one level down, to spot a mismatch (K11).</summary>
    [JsonPropertyName("childrenSum")] public decimal? ChildrenSum { get; set; }

    [JsonPropertyName("updatedAtMs")] public long? UpdatedAtMs { get; set; }

    [JsonPropertyName("updatedBy")] public string? UpdatedBy { get; set; }
}

public sealed class TargetSummaryDto
{
    /// <summary>Net sales without VAT, returns taken off.</summary>
    [JsonPropertyName("revenue")] public decimal Revenue { get; set; }

    [JsonPropertyName("returns")] public decimal Returns { get; set; }

    [JsonPropertyName("collection")] public decimal Collection { get; set; }

    [JsonPropertyName("documentCount")] public int DocumentCount { get; set; }

    [JsonPropertyName("visitsPlanned")] public int VisitsPlanned { get; set; }

    [JsonPropertyName("visitsCompleted")] public int VisitsCompleted { get; set; }

    [JsonPropertyName("pendingRevenue")] public decimal PendingRevenue { get; set; }

    [JsonPropertyName("pendingCollection")] public decimal PendingCollection { get; set; }
}

public sealed class OwnerProgressDto
{
    [JsonPropertyName("ownerKind")] public string OwnerKind { get; set; } = string.Empty;

    [JsonPropertyName("ownerId")] public Guid OwnerId { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary>A user's team, a team's region.</summary>
    [JsonPropertyName("teamId")] public Guid? TeamId { get; set; }

    [JsonPropertyName("teamName")] public string? TeamName { get; set; }

    /// <summary>A region or a team, for a team owner.</summary>
    [JsonPropertyName("teamKind")] public string? TeamKind { get; set; }

    /// <summary>Why this owner's actual figures cannot be trusted, when so (an unmapped salesperson code).</summary>
    [JsonPropertyName("warning")] public string? Warning { get; set; }

    [JsonPropertyName("summary")] public TargetSummaryDto Summary { get; set; } = new();

    [JsonPropertyName("targets")] public TargetRowDto[] Targets { get; set; } = [];
}

public sealed class TargetBoardResponse
{
    [JsonPropertyName("periodType")] public string PeriodType { get; set; } = string.Empty;

    [JsonPropertyName("periodKey")] public string PeriodKey { get; set; } = string.Empty;

    [JsonPropertyName("start")] public string Start { get; set; } = string.Empty;

    [JsonPropertyName("end")] public string End { get; set; } = string.Empty;

    /// <summary>The day the pace is measured at: today, clipped to the period.</summary>
    [JsonPropertyName("asOf")] public string AsOf { get; set; } = string.Empty;

    [JsonPropertyName("workDaysTotal")] public int WorkDaysTotal { get; set; }

    [JsonPropertyName("workDaysElapsed")] public int WorkDaysElapsed { get; set; }

    /// <summary>Remaining working days, today included.</summary>
    [JsonPropertyName("workDaysLeft")] public int WorkDaysLeft { get; set; }

    [JsonPropertyName("dataSource")] public string DataSource { get; set; } = string.Empty;

    /// <summary><c>server-documents</c>, <c>erp-mirror</c> or <c>phone-documents</c>.</summary>
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;

    [JsonPropertyName("warnings")] public string[] Warnings { get; set; } = [];

    [JsonPropertyName("canManage")] public bool CanManage { get; set; }

    [JsonPropertyName("wholeCompany")] public bool WholeCompany { get; set; }

    [JsonPropertyName("owners")] public OwnerProgressDto[] Owners { get; set; } = [];
}

public sealed class TargetItemDto
{
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary>Products under a category or brand.</summary>
    [JsonPropertyName("productCount")] public int ProductCount { get; set; }
}

public sealed class TargetItemsResponse
{
    [JsonPropertyName("items")] public TargetItemDto[] Items { get; set; } = [];

    [JsonPropertyName("truncated")] public bool Truncated { get; set; }
}

public sealed class TargetSettingsDto
{
    /// <summary>Monday = 1, Tuesday = 2 … Sunday = 64.</summary>
    [JsonPropertyName("workDays")] public int WorkDays { get; set; }
}

// ---- phone -------------------------------------------------------------------------------------

public sealed class PeriodProgressDto
{
    [JsonPropertyName("periodType")] public string PeriodType { get; set; } = string.Empty;

    [JsonPropertyName("periodKey")] public string PeriodKey { get; set; } = string.Empty;

    [JsonPropertyName("start")] public string Start { get; set; } = string.Empty;

    [JsonPropertyName("end")] public string End { get; set; } = string.Empty;

    [JsonPropertyName("workDaysTotal")] public int WorkDaysTotal { get; set; }

    [JsonPropertyName("workDaysLeft")] public int WorkDaysLeft { get; set; }

    [JsonPropertyName("summary")] public TargetSummaryDto Summary { get; set; } = new();

    [JsonPropertyName("targets")] public TargetRowDto[] Targets { get; set; } = [];
}

public sealed class MyTargetsResponse
{
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;

    [JsonPropertyName("asOfMs")] public long AsOfMs { get; set; }

    [JsonPropertyName("dataSource")] public string DataSource { get; set; } = string.Empty;

    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;

    [JsonPropertyName("warnings")] public string[] Warnings { get; set; } = [];

    /// <summary>Day, week and month holding <see cref="Date"/>, in that order.</summary>
    [JsonPropertyName("periods")] public PeriodProgressDto[] Periods { get; set; } = [];

    /// <summary>Whether the user may open the team view.</summary>
    [JsonPropertyName("canViewTeam")] public bool CanViewTeam { get; set; }
}
