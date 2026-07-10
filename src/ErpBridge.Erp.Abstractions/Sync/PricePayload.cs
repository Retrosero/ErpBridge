namespace ErpBridge.Erp.Abstractions.Sync;

/// <summary>
/// Fiyat listesi (price list) record carried inside a <see cref="SyncPackage"/>.
/// Mikro typically keeps 4 numbered lists (1=base, 2-4=customer-tiered) and the
/// Phase 5 reader surfaces every active list per stok. <see cref="Price"/> and
/// <see cref="Currency"/> are nullable because the reader can opt to push only
/// the rows that have an actual price (cheaper payloads).
/// </summary>
/// <param name="StockCode">Stok kodu the price applies to.</param>
/// <param name="ListNumber">List number (1-4 in Mikro).</param>
/// <param name="Price">List price in <see cref="Currency"/>.</param>
/// <param name="Currency">Currency code of <see cref="Price"/>.</param>
/// <param name="DiscountCode">Optional discount / kampanya kodu attached to the row.</param>
public sealed record PricePayload(
    string StockCode,
    int ListNumber,
    decimal? Price,
    string? Currency,
    string? DiscountCode);

/// <summary>
/// Satış koşulu (sales condition) record carried inside a <see cref="SyncPackage"/>.
/// Sales conditions are per-customer / per-stok / per-warehouse price + discount
/// overrides that Mikro applies when the matching order is entered. The
/// <see cref="Discounts"/> list holds the per-line discount rates in Mikro
/// order (typically 6 slots). Stock/Customer pointers are nullable because a
/// condition may be wildcarded (e.g. all customers in a group).
/// </summary>
/// <param name="StockCode">Stok kodu (nullable for wildcard conditions).</param>
/// <param name="CustomerCode">Cari kodu (nullable for wildcard conditions).</param>
/// <param name="WarehouseNo">Depo numarası (nullable for warehouse-agnostic).</param>
/// <param name="PaymentPlanNo">Ödeme planı numarası (nullable for any plan).</param>
/// <param name="StartDate">Optional validity start (inclusive).</param>
/// <param name="EndDate">Optional validity end (inclusive).</param>
/// <param name="GrossPrice">Brüt fiyat in <see cref="Currency"/>.</param>
/// <param name="Currency">Currency code of <see cref="GrossPrice"/>.</param>
/// <param name="Discounts">Discount rates in apply-order (Mikro: 6 slots).</param>
public sealed record SalesConditionPayload(
    string? StockCode,
    string? CustomerCode,
    int? WarehouseNo,
    int? PaymentPlanNo,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? GrossPrice,
    string? Currency,
    IReadOnlyList<decimal> Discounts);
