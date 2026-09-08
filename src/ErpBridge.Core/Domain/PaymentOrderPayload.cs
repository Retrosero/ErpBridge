namespace ErpBridge.Core.Domain;

/// <summary>
/// Ödeme emri (payment order) payload sent from the central API to the agent. The
/// Mikro adapter turns this into an INSERT against <c>ODEME_EMIRLERI</c>. Lives in
/// <c>ErpBridge.Core</c> (and not in <c>ErpBridge.Erp.Abstractions</c>) because the
/// Tahsilat modülü is currently Mikro-only.
/// </summary>
/// <remarks>
/// <para>
/// A payment order is a single-row document — no sub-line table. Exactly one of
/// <see cref="CustomerCode"/> / <see cref="BankCode"/> is normally populated by the
/// upstream; the adapter copies both verbatim to <c>ode_cari_kod</c> /
/// <c>ode_banka_kod</c> and lets Mikro reject empty combinations.
/// </para>
/// <para>
/// <see cref="Channel"/> encodes the tahsilat kanalı (nakit, havale, kart, çek,
/// senet, ...). Mikro stores this in <c>ode_kanal</c> and the column is reserved as
/// <c>NVARCHAR(20)</c> on the test schema.
/// </para>
/// </remarks>
/// <param name="TenantId">Multi-tenant identifier.</param>
/// <param name="ExternalId">Idempotency key.</param>
/// <param name="CustomerCode">Mikro cari kodu (e.g. "120.01.0001"). Either this or <see cref="BankCode"/> is required.</param>
/// <param name="BankCode">Optional banka kodu (e.g. "BNK01").</param>
/// <param name="OrderDate">UTC timestamp of the order.</param>
/// <param name="Amount">Net amount in <paramref name="Currency"/>.</param>
/// <param name="Currency">Currency code (TRY, USD, EUR, ...).</param>
/// <param name="Description">Free-form açıklama.</param>
/// <param name="Channel">Tahsilat kanalı (nakit, havale, kart, çek, senet, ...).</param>
/// <param name="DueDate">Optional vade tarihi (UTC).</param>
public sealed record PaymentOrderPayload(
    Guid TenantId,
    string ExternalId,
    string CustomerCode,
    string? BankCode,
    DateTime OrderDate,
    decimal Amount,
    string Currency,
    string Description,
    string Channel,
    DateTime? DueDate);
