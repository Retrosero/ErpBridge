namespace ErpBridge.Erp.Abstractions.SalesOrder;

/// <summary>
/// Sales order payload sent from the central API to the agent. Field set is fixed and
/// independent of the target ERP — Mikro-specific fields are introduced by the adapter.
/// </summary>
/// <param name="TenantId">Multi-tenant identifier — every mapping is scoped under this.</param>
/// <param name="ExternalId">Idempotency key — a second call with the same ExternalId MUST
/// not create a duplicate Mikro document.</param>
/// <param name="CustomerCode">Mikro cari kodu (e.g. "120.01.0001"). Validated by adapter.</param>
/// <param name="SalespersonCode">Optional plasiyer kodu.</param>
/// <param name="WarehouseNo">Depo numarası.</param>
/// <param name="DocumentSeries">Evrak serisi (e.g. "S" or "SS").</param>
/// <param name="DocumentNumber">Evrak numarası — must be &gt; 0.</param>
/// <param name="OccurredAt">UTC timestamp for the document date.</param>
/// <param name="Currency">Currency code (TL, USD, EUR, ...).</param>
/// <param name="Lines">Order lines — must be non-empty.</param>
public sealed record SalesOrderPayload(
    string TenantId,
    string ExternalId,
    string CustomerCode,
    string? SalespersonCode,
    int WarehouseNo,
    string DocumentSeries,
    int DocumentNumber,
    DateTime OccurredAt,
    string Currency,
    IReadOnlyList<SalesOrderLinePayload> Lines);
