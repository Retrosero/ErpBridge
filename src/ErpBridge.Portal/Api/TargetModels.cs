using System.Text.Json.Serialization;

namespace ErpBridge.Portal.Api;

// Mirrors of ErpBridge.CentralApi/Contracts/TargetContracts.cs (GOAL_HEDEF_RUT). Kept local on purpose.

public sealed class TeamDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = TeamKinds.Team;
    [JsonPropertyName("parentId")] public Guid? ParentId { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; } = true;
    [JsonPropertyName("memberIds")] public Guid[] MemberIds { get; set; } = [];
    [JsonPropertyName("managerIds")] public Guid[] ManagerIds { get; set; } = [];

    public bool IsRegion => Kind == TeamKinds.Region;
}

public static class TeamKinds
{
    public const string Region = "REGION";
    public const string Team = "TEAM";
}

public sealed class TeamPersonDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("roles")] public string[] Roles { get; set; } = [];
    [JsonPropertyName("teamId")] public Guid? TeamId { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }

    public string DisplayName => string.IsNullOrWhiteSpace(FullName) ? Username : FullName;
}

public sealed class TeamsResponse
{
    [JsonPropertyName("teams")] public TeamDto[] Teams { get; set; } = [];
    [JsonPropertyName("people")] public TeamPersonDto[] People { get; set; } = [];
    [JsonPropertyName("canEdit")] public bool CanEdit { get; set; }
    [JsonPropertyName("wholeCompany")] public bool WholeCompany { get; set; }
}

public sealed class TeamSaveRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = TeamKinds.Team;
    [JsonPropertyName("parentId")] public Guid? ParentId { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; } = true;
    [JsonPropertyName("memberIds")] public Guid[]? MemberIds { get; set; }
    [JsonPropertyName("managerIds")] public Guid[]? ManagerIds { get; set; }
}

public sealed class TargetInput
{
    [JsonPropertyName("periodType")] public string PeriodType { get; set; } = string.Empty;
    [JsonPropertyName("periodKey")] public string PeriodKey { get; set; } = string.Empty;
    [JsonPropertyName("metric")] public string Metric { get; set; } = string.Empty;
    [JsonPropertyName("measure")] public string? Measure { get; set; }
    [JsonPropertyName("itemCode")] public string? ItemCode { get; set; }
    [JsonPropertyName("itemName")] public string? ItemName { get; set; }
    [JsonPropertyName("ownerKind")] public string OwnerKind { get; set; } = string.Empty;
    [JsonPropertyName("ownerId")] public Guid? OwnerId { get; set; }
    [JsonPropertyName("value")] public decimal? Value { get; set; }
    [JsonPropertyName("note")] public string? Note { get; set; }
}

public sealed class TargetsSaveRequest
{
    [JsonPropertyName("operationId")] public Guid OperationId { get; set; }
    [JsonPropertyName("items")] public TargetInput[] Items { get; set; } = [];
}

public sealed class TargetErrorDto
{
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("errorCode")] public string ErrorCode { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
}

public sealed class TargetsSaveResponse
{
    [JsonPropertyName("saved")] public int Saved { get; set; }
    [JsonPropertyName("deleted")] public int Deleted { get; set; }
    [JsonPropertyName("unchanged")] public int Unchanged { get; set; }
    [JsonPropertyName("duplicate")] public bool Duplicate { get; set; }
    [JsonPropertyName("errors")] public TargetErrorDto[] Errors { get; set; } = [];
}

public sealed class TargetCopyRequest
{
    [JsonPropertyName("periodType")] public string PeriodType { get; set; } = string.Empty;
    [JsonPropertyName("fromPeriodKey")] public string FromPeriodKey { get; set; } = string.Empty;
    [JsonPropertyName("toPeriodKey")] public string ToPeriodKey { get; set; } = string.Empty;
    [JsonPropertyName("percent")] public decimal Percent { get; set; }
    [JsonPropertyName("overwrite")] public bool Overwrite { get; set; }
    [JsonPropertyName("apply")] public bool Apply { get; set; }
    [JsonPropertyName("operationId")] public Guid OperationId { get; set; }
}

public sealed class TargetDistributeRequest
{
    [JsonPropertyName("periodType")] public string PeriodType { get; set; } = string.Empty;
    [JsonPropertyName("periodKey")] public string PeriodKey { get; set; } = string.Empty;
    [JsonPropertyName("metric")] public string Metric { get; set; } = string.Empty;
    [JsonPropertyName("measure")] public string? Measure { get; set; }
    [JsonPropertyName("itemCode")] public string? ItemCode { get; set; }
    [JsonPropertyName("sourceOwnerKind")] public string SourceOwnerKind { get; set; } = TargetOwners.Company;
    [JsonPropertyName("sourceOwnerId")] public Guid? SourceOwnerId { get; set; }
    [JsonPropertyName("value")] public decimal? Value { get; set; }
    [JsonPropertyName("method")] public string Method { get; set; } = "EQUAL";
    [JsonPropertyName("targetLevel")] public string TargetLevel { get; set; } = TargetOwners.User;
}

public sealed class TargetPreviewResponse
{
    [JsonPropertyName("items")] public TargetInput[] Items { get; set; } = [];
    [JsonPropertyName("result")] public TargetsSaveResponse? Result { get; set; }
}

public sealed class TargetRowDto
{
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
    [JsonPropertyName("pending")] public decimal Pending { get; set; }
    [JsonPropertyName("percent")] public decimal? Percent { get; set; }
    [JsonPropertyName("expectedToDate")] public decimal ExpectedToDate { get; set; }
    [JsonPropertyName("forecast")] public decimal Forecast { get; set; }
    [JsonPropertyName("requiredPerDay")] public decimal? RequiredPerDay { get; set; }
    [JsonPropertyName("derived")] public bool Derived { get; set; }
    [JsonPropertyName("childrenSum")] public decimal? ChildrenSum { get; set; }
    [JsonPropertyName("updatedAtMs")] public long? UpdatedAtMs { get; set; }
    [JsonPropertyName("updatedBy")] public string? UpdatedBy { get; set; }
}

public sealed class TargetSummaryDto
{
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
    [JsonPropertyName("teamId")] public Guid? TeamId { get; set; }
    [JsonPropertyName("teamName")] public string? TeamName { get; set; }
    [JsonPropertyName("teamKind")] public string? TeamKind { get; set; }
    [JsonPropertyName("warning")] public string? Warning { get; set; }
    [JsonPropertyName("summary")] public TargetSummaryDto Summary { get; set; } = new();
    [JsonPropertyName("targets")] public TargetRowDto[] Targets { get; set; } = [];

    /// <summary>The row's key in the page: <c>USER:guid</c>, <c>TEAM:guid</c>, <c>COMPANY</c>.</summary>
    public string Key => OwnerKind == TargetOwners.Company ? TargetOwners.Company : $"{OwnerKind}:{OwnerId}";

    public TargetRowDto? Explicit(string metric, string itemCode = "") =>
        Targets.FirstOrDefault(t => !t.Derived && t.Metric == metric && t.ItemCode == itemCode);

    /// <summary>The entered target of a metric, else the worked-out one.</summary>
    public TargetRowDto? Main(string metric) => Explicit(metric) ?? Targets.FirstOrDefault(t => t.Metric == metric && t.ItemCode.Length == 0);
}

public sealed class TargetBoardResponse
{
    [JsonPropertyName("periodType")] public string PeriodType { get; set; } = string.Empty;
    [JsonPropertyName("periodKey")] public string PeriodKey { get; set; } = string.Empty;
    [JsonPropertyName("start")] public string Start { get; set; } = string.Empty;
    [JsonPropertyName("end")] public string End { get; set; } = string.Empty;
    [JsonPropertyName("asOf")] public string AsOf { get; set; } = string.Empty;
    [JsonPropertyName("workDaysTotal")] public int WorkDaysTotal { get; set; }
    [JsonPropertyName("workDaysElapsed")] public int WorkDaysElapsed { get; set; }
    [JsonPropertyName("workDaysLeft")] public int WorkDaysLeft { get; set; }
    [JsonPropertyName("dataSource")] public string DataSource { get; set; } = string.Empty;
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
    [JsonPropertyName("productCount")] public int ProductCount { get; set; }
}

public sealed class TargetItemsResponse
{
    [JsonPropertyName("items")] public TargetItemDto[] Items { get; set; } = [];
    [JsonPropertyName("truncated")] public bool Truncated { get; set; }
}

public sealed class TargetSettingsDto
{
    [JsonPropertyName("workDays")] public int WorkDays { get; set; }
}

public static class TargetOwners
{
    public const string User = "USER";
    public const string Team = "TEAM";
    public const string Company = "COMPANY";
}

/// <summary>Target periods and metrics as the pages show them.</summary>
public static class TargetText
{
    public const string Daily = "DAILY";
    public const string Weekly = "WEEKLY";
    public const string Monthly = "MONTHLY";

    public const string Revenue = "REVENUE";
    public const string Collection = "COLLECTION";
    public const string Product = "PRODUCT";
    public const string Category = "CATEGORY";
    public const string SubCategory = "SUB_CATEGORY";
    public const string Brand = "BRAND";
    public const string Visit = "VISIT";
    public const string DocumentCount = "DOCUMENT_COUNT";

    /// <summary>The metrics that name no item: one column each in the entry table.</summary>
    public static readonly IReadOnlyList<string> PlainMetrics = [Revenue, Collection, Visit, DocumentCount];

    public static readonly IReadOnlyList<string> ItemMetrics = [Product, Category, SubCategory, Brand];

    public static string Metric(string metric) => metric switch
    {
        Revenue => "Ciro",
        Collection => "Tahsilat",
        Product => "Ürün",
        Category => "Kategori",
        SubCategory => "Alt kategori",
        Brand => "Marka",
        Visit => "Ziyaret",
        DocumentCount => "Fiş adedi",
        _ => metric,
    };

    public static string Period(string type) => type switch
    {
        Daily => "Gün",
        Weekly => "Hafta",
        Monthly => "Ay",
        _ => type,
    };

    public static string Measure(string measure) => measure switch
    {
        "QUANTITY" => "Miktar",
        "COUNT" => "Adet",
        _ => "Tutar",
    };

    /// <summary>A target value as its measure reads: money, quantity or a whole count (a pace of visits reads 76, not 76,15).</summary>
    public static string Value(string metric, string measure, decimal value) =>
        measure == "AMOUNT" || metric is Revenue or Collection ? Fmt.Money(value)
        : measure == "COUNT" ? Math.Round(value, 0, MidpointRounding.AwayFromZero).ToString("N0", Fmt.Turkish)
        : Fmt.Quantity(value);

    /// <summary>The period holding <paramref name="day"/>, as the API names it.</summary>
    public static string KeyOf(string type, DateOnly day) => type switch
    {
        Daily => Fmt.IsoDay(day),
        Weekly => $"{System.Globalization.ISOWeek.GetYear(day.ToDateTime(TimeOnly.MinValue)):D4}-W{System.Globalization.ISOWeek.GetWeekOfYear(day.ToDateTime(TimeOnly.MinValue)):D2}",
        _ => day.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture),
    };

    /// <summary>A day of the period after (<paramref name="step"/> 1) or before (-1) the one holding <paramref name="day"/>.</summary>
    public static DateOnly Shift(string type, DateOnly day, int step) => type switch
    {
        Daily => day.AddDays(step),
        Weekly => day.AddDays(7 * step),
        _ => new DateOnly(day.Year, day.Month, 1).AddMonths(step),
    };

    /// <summary>"Ekim 2026", "2026 · 41. hafta (5–11 Eki)", "15 Ekim 2026, Perşembe".</summary>
    public static string Title(string type, DateOnly day)
    {
        switch (type)
        {
            case Daily:
                return Fmt.Day(day);
            case Weekly:
                var monday = day.AddDays(-((7 + (int)day.DayOfWeek - 1) % 7));
                var week = System.Globalization.ISOWeek.GetWeekOfYear(day.ToDateTime(TimeOnly.MinValue));
                return $"{week}. hafta · {monday.ToString("d MMM", Fmt.Turkish)} – {monday.AddDays(6).ToString("d MMM yyyy", Fmt.Turkish)}";
            default:
                return day.ToString("MMMM yyyy", Fmt.Turkish);
        }
    }

    public static string Source(string source) => source switch
    {
        "erp-mirror" => "Mikro hareketleri (plasiyer koduna göre)",
        "phone-documents" => "Telefon belgeleri (geçici)",
        _ => "Sunucudaki belgeler",
    };
}
