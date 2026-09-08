namespace ErpBridge.Erp.Abstractions.Documents;

/// <summary>
/// Tahsilat (collection) payload sent from the central API to the agent. The Mikro
/// adapter turns this into an INSERT against <c>CARI_HESAP_HAREKETLERI</c>. Lives in
/// <c>ErpBridge.Core</c> (and not in <c>ErpBridge.Erp.Abstractions</c>) because the
/// Tahsilat modülü is currently Mikro-only — promoting the type to abstractions can
/// happen when a second ERP adapter picks the payload up.
/// </summary>
/// <remarks>
/// <para>
/// A single collection document is the row inserted into <c>CARI_HESAP_HAREKETLERI</c>.
/// The optional <see cref="Lines"/> are sub-rows on the same table linked back to the
/// header by <c>cha_RECid_RECno</c> (V15) or <c>cha_uid</c> (V16) — the V15/V16 split
/// stays inside the adapter.
/// </para>
/// <para>
/// <see cref="TenantId"/> is a <see cref="Guid"/> (matches <c>ErpBridge.CentralApi</c>'s
/// <c>Tenant.Id</c>). <see cref="ExternalId"/> is the customer-side idempotency key.
/// </para>
/// </remarks>
/// <param name="TenantId">Multi-tenant identifier — every mapping is scoped under this.</param>
/// <param name="ExternalId">Idempotency key — a second call with the same ExternalId MUST
/// not create a duplicate Mikro document.</param>
/// <param name="CustomerCode">Mikro cari kodu (e.g. "120.01.0001").</param>
/// <param name="TransactionDate">UTC timestamp of the cash-collection posting.</param>
/// <param name="Amount">Net amount in <paramref name="Currency"/>. Negative amounts are rejected.</param>
/// <param name="Currency">Currency code (TRY, USD, EUR, ...).</param>
/// <param name="Description">Free-form açıklama (mapped to <c>cha_aciklama</c>).</param>
/// <param name="DocumentType">Movement type — e.g. <c>"tahsilat_makbuzu"</c> or <c>"odeme_emri"</c>.</param>
/// <param name="Lines">Optional sub-lines. Empty / null means a single-row header insert.</param>
/// <param name="DocumentSeries">
/// Evrak serisi (e.g. <c>"THS"</c>). Required: the ERP identifies a document by
/// its series + number, and Mikro enforces a unique index over
/// <c>(evrak_tip, evrakno_seri, evrakno_sira, satir_no)</c>.
/// </param>
/// <param name="DocumentNumber">Evrak sıra numarası. Required for the same reason as <paramref name="DocumentSeries"/>.</param>
public sealed record CollectionPayload(
    Guid TenantId,
    string ExternalId,
    string CustomerCode,
    DateTime TransactionDate,
    decimal Amount,
    string Currency,
    string Description,
    string DocumentType,
    IReadOnlyList<CollectionLinePayload>? Lines,
    string DocumentSeries = "",
    int DocumentNumber = 0);

/// <summary>
/// One sub-line of a <see cref="CollectionPayload"/>. Stored as a child row in
/// <c>CARI_HESAP_HAREKETLERI</c> with the parent link chosen by the adapter (V15
/// <c>cha_RECid_RECno</c> / V16 <c>cha_uid</c>).
/// </summary>
/// <param name="Description">Sub-line açıklama.</param>
/// <param name="Amount">Sub-line amount (may differ from the header total).</param>
/// <param name="DocumentType">Sub-line evrak tipi (e.g. <c>"cek"</c>, <c>"senet"</c>, <c>"nakit"</c>).</param>
public sealed record CollectionLinePayload(
    string Description,
    decimal Amount,
    string? DocumentType);
