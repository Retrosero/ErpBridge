namespace ErpBridge.Erp.Abstractions.Sync;

/// <summary>
/// Kasa / banka hesabı (cash and bank account) record carried inside a
/// <see cref="SyncPackage"/>. <see cref="Kind"/> is a closed string — either
/// <c>"cash"</c> (kasa) or <c>"bank"</c> (banka) — so the central API can
/// branch without learning Mikro table names. <see cref="Branch"/> and
/// <see cref="AccountNo"/> are bank-only; the cash reader leaves them null.
/// </summary>
/// <param name="Code">Hesap kodu (unique within tenant).</param>
/// <param name="Name">Hesap adı (display).</param>
/// <param name="Kind">"cash" for kasa, "bank" for banka hesabı.</param>
/// <param name="Branch">Banka şubesi (bank only).</param>
/// <param name="AccountNo">Banka hesap no / IBAN (bank only).</param>
/// <param name="FirmNo">Firma numarası (multi-firm installations).</param>
/// <param name="Currency">Hesap dövizi.</param>
/// <param name="TcmbCode">Optional TCMB kodu (Türkiye Cumhuriyet Merkez Bankası).</param>
public sealed record CashAndBankPayload(
    string Code,
    string Name,
    string Kind,
    string? Branch,
    string? AccountNo,
    int? FirmNo,
    string? Currency,
    string? TcmbCode);
