namespace ErpBridge.Erp.Abstractions.Sync;

/// <summary>
/// Genel amaçlı lookup (dimension) record carried inside a <see cref="SyncPackage"/>.
/// The reader uses <see cref="Kind"/> to disambiguate which Mikro master
/// produced the row. Closed <see cref="Kind"/> values:
///   <c>"warehouse"</c>            — depolar
///   <c>"salesperson"</c>          — plasiyerler
///   <c>"payment_plan"</c>         — ödeme planları
///   <c>"currency"</c>             — döviz kurları
///   <c>"project"</c>              — projeler
///   <c>"responsibility_center"</c> — sorumluluk merkezleri
///   <c>"tax_office"</c>           — vergi daireleri
/// For Phase 5 a single record is enough; typed variants are kept as
/// skeletons to be filled in by Phase 6+ readers that need richer fields
/// (warehouse group numbers, salesperson names, etc.).
/// </summary>
/// <param name="Kind">Lookup kind tag (see class doc for closed values).</param>
/// <param name="Code">Lookup code (primary key within its kind).</param>
/// <param name="Name">Display name.</param>
/// <param name="ParentCode">Optional parent lookup (e.g. warehouse group code).</param>
/// <param name="Currency">Optional currency code (e.g. for currency lookups).</param>
public sealed record LookupPayload(
    string Kind,
    string Code,
    string Name,
    string? ParentCode,
    string? Currency);

/// <summary>Skeleton — typed warehouse lookup. Phase 5 carries via <see cref="LookupPayload"/>.</summary>
/// <param name="WarehouseNo">Depo numarası.</param>
/// <param name="Name">Depo adı.</param>
/// <param name="GroupNo">Optional depo grubu numarası.</param>
public sealed record WarehousePayload(string WarehouseNo, string Name, int? GroupNo);

/// <summary>Skeleton — typed salesperson lookup. Phase 5 carries via <see cref="LookupPayload"/>.</summary>
/// <param name="Code">Plasiyer kodu.</param>
/// <param name="FirstName">Plasiyer adı.</param>
/// <param name="LastName">Plasiyer soyadı.</param>
public sealed record SalespersonPayload(string Code, string FirstName, string? LastName);

/// <summary>Skeleton — typed payment-plan lookup. Phase 5 carries via <see cref="LookupPayload"/>.</summary>
/// <param name="PlanNo">Ödeme planı numarası.</param>
/// <param name="Aratop">Aratop gün sayısı (vade farkı).</param>
/// <param name="Name">Ödeme planı adı.</param>
public sealed record PaymentPlanPayload(int PlanNo, int? Aratop, string? Name);

/// <summary>Skeleton — typed currency lookup. Phase 5 carries via <see cref="LookupPayload"/>.</summary>
/// <param name="Code">Döviz kodu (USD, EUR, ...).</param>
/// <param name="Name">Döviz adı.</param>
public sealed record CurrencyPayload(string Code, string Name);

/// <summary>Skeleton — typed project lookup. Phase 5 carries via <see cref="LookupPayload"/>.</summary>
/// <param name="Code">Proje kodu.</param>
/// <param name="Name">Proje adı.</param>
public sealed record ProjectPayload(string Code, string Name);

/// <summary>Skeleton — typed responsibility-center lookup. Phase 5 carries via <see cref="LookupPayload"/>.</summary>
/// <param name="Code">Sorumluluk merkezi kodu.</param>
/// <param name="Name">Sorumluluk merkezi adı.</param>
public sealed record ResponsibilityCenterPayload(string Code, string Name);
