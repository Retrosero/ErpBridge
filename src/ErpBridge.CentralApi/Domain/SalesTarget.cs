namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A region or a team of field users (GOAL_HEDEF_RUT K2). A team may sit under one region; a region
/// holds teams, never people directly. Kept only in the central database for ERP and ERP-less
/// companies alike. Times are unix milliseconds (UTC).
/// </summary>
public sealed class SalesTeam
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary><see cref="SalesTeamKinds.Region"/> or <see cref="SalesTeamKinds.Team"/>.</summary>
    public string Kind { get; set; } = SalesTeamKinds.Team;

    /// <summary>The region a team belongs to; always null on a region.</summary>
    public Guid? ParentId { get; set; }

    public bool IsActive { get; set; } = true;

    public long CreatedAtMs { get; set; }

    public long UpdatedAtMs { get; set; }
}

/// <summary>A user's team. One team per user: the primary key is (tenant, user).</summary>
public sealed class SalesTeamMember
{
    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public Guid TeamId { get; set; }

    public long AddedAtMs { get; set; }
}

/// <summary>
/// A manager responsible for a team or a region (K2). Responsibility for a region covers every team
/// under it. A manager may be responsible for several.
/// </summary>
public sealed class SalesTeamManager
{
    public Guid TenantId { get; set; }

    public Guid TeamId { get; set; }

    public Guid UserId { get; set; }
}

public static class SalesTeamKinds
{
    public const string Region = "REGION";
    public const string Team = "TEAM";

    public static bool IsValid(string? kind) => kind is Region or Team;
}

/// <summary>
/// One target (GOAL_HEDEF_RUT K5): for a period, what (metric, measure, item) and whose (owner). The
/// natural key — period, metric, measure, item, owner — is unique; writing it again replaces the value.
/// </summary>
public sealed class SalesTarget
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary><see cref="TargetPeriodTypes"/>.</summary>
    public string PeriodType { get; set; } = TargetPeriodTypes.Monthly;

    /// <summary><c>2026-10-05</c>, <c>2026-W41</c> or <c>2026-10</c> (<see cref="Targets.TargetPeriod"/>).</summary>
    public string PeriodKey { get; set; } = string.Empty;

    /// <summary>First Istanbul day of the period, <c>yyyy-MM-dd</c>; for range queries.</summary>
    public string PeriodStartDay { get; set; } = string.Empty;

    /// <summary>Last Istanbul day of the period (inclusive), <c>yyyy-MM-dd</c>.</summary>
    public string PeriodEndDay { get; set; } = string.Empty;

    /// <summary><see cref="TargetMetrics"/>.</summary>
    public string Metric { get; set; } = TargetMetrics.Revenue;

    /// <summary><see cref="TargetMeasures"/>.</summary>
    public string Measure { get; set; } = TargetMeasures.Amount;

    /// <summary>Product code, category or brand code; empty for metrics without an item.</summary>
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>Snapshot of the item's name when written; the card may be renamed later.</summary>
    public string? ItemName { get; set; }

    /// <summary><see cref="TargetOwnerKinds"/>.</summary>
    public string OwnerKind { get; set; } = TargetOwnerKinds.User;

    /// <summary>The user or team; <see cref="Guid.Empty"/> for the company.</summary>
    public Guid OwnerId { get; set; }

    public decimal Value { get; set; }

    public string? Note { get; set; }

    public Guid CreatedByUserId { get; set; }

    public Guid UpdatedByUserId { get; set; }

    public long CreatedAtMs { get; set; }

    public long UpdatedAtMs { get; set; }

    /// <summary>Soft delete: the row keeps its history; writing the key again revives it.</summary>
    public bool IsDeleted { get; set; }
}

/// <summary>Append-only history of target values (K12).</summary>
public sealed class SalesTargetEvent
{
    public long Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TargetId { get; set; }

    /// <summary><c>SET</c> or <c>DELETE</c>.</summary>
    public string Action { get; set; } = string.Empty;

    public decimal? OldValue { get; set; }

    public decimal? NewValue { get; set; }

    public Guid ActorUserId { get; set; }

    public string ActorName { get; set; } = string.Empty;

    public long OccurredAtMs { get; set; }

    /// <summary>The panel save that wrote it (<see cref="SalesTargetOperation"/>); null for a team deletion.</summary>
    public Guid? OperationId { get; set; }
}

/// <summary>
/// A target save the server applied, by the panel's operation id. The primary key makes a retried save that overlaps
/// the first one wait for it and then find it, instead of writing the batch twice.
/// </summary>
public sealed class SalesTargetOperation
{
    public Guid TenantId { get; set; }

    public Guid OperationId { get; set; }

    public Guid UserId { get; set; }

    public long AppliedAtMs { get; set; }
}

/// <summary>A company's target settings; a missing row means the defaults.</summary>
public sealed class TargetSettings
{
    /// <summary>Monday–Saturday.</summary>
    public const int DefaultWorkDays = 0b0111111;

    public Guid TenantId { get; set; }

    /// <summary>Bit mask of working days: Monday = 1, Tuesday = 2 … Sunday = 64 (K9).</summary>
    public int WorkDays { get; set; } = DefaultWorkDays;

    public long UpdatedAtMs { get; set; }
}

public static class TargetPeriodTypes
{
    public const string Daily = "DAILY";
    public const string Weekly = "WEEKLY";
    public const string Monthly = "MONTHLY";

    public static readonly IReadOnlyList<string> All = [Daily, Weekly, Monthly];
}

public static class TargetMetrics
{
    /// <summary>Net sales without VAT, returns taken off (K6).</summary>
    public const string Revenue = "REVENUE";

    public const string Collection = "COLLECTION";

    public const string Product = "PRODUCT";

    /// <summary>Main group (ERP) or <c>kategori</c> (ERP-less card).</summary>
    public const string Category = "CATEGORY";

    /// <summary>Mikro stock sub-group; its code is <c>mainGroup|subGroup</c>. ERP companies only.</summary>
    public const string SubCategory = "SUB_CATEGORY";

    public const string Brand = "BRAND";

    /// <summary>Completed route visits (K10).</summary>
    public const string Visit = "VISIT";

    /// <summary>Sales documents sent from the phones.</summary>
    public const string DocumentCount = "DOCUMENT_COUNT";

    public static readonly IReadOnlyList<string> All = [Revenue, Collection, Product, Category, SubCategory, Brand, Visit, DocumentCount];

    /// <summary>Metrics that name an item (<see cref="SalesTarget.ItemCode"/>).</summary>
    public static bool HasItem(string metric) => metric is Product or Category or SubCategory or Brand;

    /// <summary>The measures a metric may be counted in; the first is its default.</summary>
    public static IReadOnlyList<string> MeasuresOf(string metric) => metric switch
    {
        Product or Category or SubCategory or Brand => [TargetMeasures.Amount, TargetMeasures.Quantity],
        Visit or DocumentCount => [TargetMeasures.Count],
        _ => [TargetMeasures.Amount],
    };
}

public static class TargetMeasures
{
    public const string Amount = "AMOUNT";
    public const string Quantity = "QUANTITY";
    public const string Count = "COUNT";
}

public static class TargetOwnerKinds
{
    public const string User = "USER";

    /// <summary>A team or a region (<see cref="SalesTeam"/>).</summary>
    public const string Team = "TEAM";

    public const string Company = "COMPANY";

    public static readonly IReadOnlyList<string> All = [User, Team, Company];
}
