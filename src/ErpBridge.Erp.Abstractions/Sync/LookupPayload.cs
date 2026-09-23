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
///   <c>"expense_card"</c>         — gider kartları (<c>MASRAF_HESAPLARI</c>)
///   <c>"vat_rate"</c>             — KDV tanımları (Mikro vergi işaretçisi → oran)
/// For Phase 5 a single record is enough; typed variants are kept as
/// skeletons to be filled in by Phase 6+ readers that need richer fields
/// (warehouse group numbers, salesperson names, etc.).
/// </summary>
/// <param name="Kind">Lookup kind tag (see class doc for closed values).</param>
/// <param name="Code">Lookup code (primary key within its kind).</param>
/// <param name="Name">Display name.</param>
/// <param name="ParentCode">Optional parent lookup (e.g. warehouse group code).</param>
/// <param name="Currency">Optional currency code (e.g. for currency lookups).</param>
/// <param name="IncludesVat">
/// <c>price_list</c> only: whether the list's prices include VAT (Mikro <c>sfl_kdvdahil</c>). The phone must not add
/// VAT on top of such a price, or its total would differ from the one the ERP writer books (goal ERP yazım Y4g).
/// </param>
/// <param name="TypeCode"><c>expense_card</c> only: Mikro's <c>his_tipkod</c> heading.</param>
/// <param name="ClassCode"><c>expense_card</c> only: Mikro's <c>his_sinifkod</c> heading.</param>
/// <param name="Unit"><c>expense_card</c> only: Mikro's <c>his_birim_ad</c>.</param>
/// <param name="Rate">
/// <c>vat_rate</c> only: the percentage Mikro holds for the pointer in <see cref="Code"/> (<c>fn_VergiYuzde</c>).
/// The phone picks a pointer and the expense writer books the VAT into that pointer's <c>cha_vergiN</c> column.
/// </param>
public sealed record LookupPayload(
    string Kind,
    string Code,
    string Name,
    string? ParentCode,
    string? Currency,
    bool? IncludesVat = null,
    string? TypeCode = null,
    string? ClassCode = null,
    string? Unit = null,
    decimal? Rate = null);

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
