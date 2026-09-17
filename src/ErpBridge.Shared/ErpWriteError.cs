using System.Globalization;

namespace ErpBridge.Shared;

/// <summary>
/// Why a phone document could not be written into the ERP (goal GOAL_ERP_YAZIM, Y2d). The code
/// is stable for software (job ack, Portal filters); the message is shown as-is to the phone
/// user and the company admin, in Turkish. Messages carry ERP codes (which the agent or the
/// company settings supplied) and amounts of difference only — never a customer's title, a
/// document's total or a free-text value copied from the phone body (log privacy rule).
/// </summary>
/// <param name="Code">Stable machine-readable code.</param>
/// <param name="Message">Turkish text for people.</param>
/// <param name="Retryable">
/// <c>true</c> only when the same document may succeed later without anyone changing anything
/// (the ERP was unreachable); data and mapping problems are permanent until fixed.
/// </param>
public sealed record ErpWriteError(string Code, string Message, bool Retryable = false)
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>ERP code widths are at most 25; anything longer or odd came from a malformed body.</summary>
    private const int ShownCodeMaxLength = 25;

    /// <summary>A code as shown in a message: control characters removed, cut to an ERP code's width.</summary>
    private static string Shown(string? code)
    {
        var clean = new string((code ?? string.Empty).Where(c => !char.IsControl(c)).ToArray()).Trim();
        return clean.Length <= ShownCodeMaxLength ? clean : clean[..ShownCodeMaxLength] + "…";
    }

    // ---- the phone document itself ---------------------------------------------------------

    public const string MobileAppUpdateRequiredCode = "MOBILE_APP_UPDATE_REQUIRED";
    public const string MissingCustomerCodeCode = "MISSING_CUSTOMER_CODE";
    public const string MissingStockCodeCode = "MISSING_STOCK_CODE";
    public const string InvalidQuantityCode = "INVALID_QUANTITY";
    public const string InvalidAmountCode = "INVALID_AMOUNT";
    public const string InvalidDiscountCode = "INVALID_DISCOUNT";
    public const string InvalidDocumentDateCode = "INVALID_DOCUMENT_DATE";
    public const string UnsupportedCurrencyCode = "UNSUPPORTED_CURRENCY";
    public const string UnsupportedPaymentTypeCode = "UNSUPPORTED_PAYMENT_TYPE";
    public const string MissingChequeDetailsCode = "MISSING_CHEQUE_DETAILS";
    public const string MissingNoteDetailsCode = "MISSING_NOTE_DETAILS";

    public static ErpWriteError MobileAppUpdateRequired() =>
        new(MobileAppUpdateRequiredCode, "Bu belge eski bir Sipariş Cepte sürümünden geldi ve ERP'ye doğru işlenemez. Uygulamayı güncelleyip belgeyi yeniden gönderin.");

    public static ErpWriteError MissingCustomerCode() =>
        new(MissingCustomerCodeCode, "Belgede müşteri kodu yok. Müşteri ERP'de kayıtlı olmalı.");

    public static ErpWriteError MissingStockCode(int lineNo) =>
        new(MissingStockCodeCode, $"{lineNo}. satırda ürün kodu yok.");

    public static ErpWriteError InvalidQuantity(int lineNo) =>
        new(InvalidQuantityCode, $"{lineNo}. satırın miktarı sıfırdan büyük olmalı.");

    public static ErpWriteError InvalidAmount() =>
        new(InvalidAmountCode, "Belgedeki tutar geçersiz.");

    public static ErpWriteError InvalidDiscount(int lineNo) =>
        new(InvalidDiscountCode, $"{lineNo}. satırın iskonto ya da kondisyon oranı geçersiz.");

    public static ErpWriteError InvalidDocumentDate() =>
        new(InvalidDocumentDateCode, "Belge tarihi okunamadı.");

    // The phone's raw value is never echoed: a malformed body could carry anything into acks and logs (PR #80 Codex).
    public static ErpWriteError UnsupportedCurrency() =>
        new(UnsupportedCurrencyCode, "Yalnız TL belgeler ERP'ye yazılabilir.");

    public static ErpWriteError UnsupportedPaymentType() =>
        new(UnsupportedPaymentTypeCode, "Ödeme şekli tanınmadı. Nakit, kredi kartı, havale/EFT, çek, senet ya da cari borç olmalı.");

    public static ErpWriteError MissingChequeDetails() =>
        new(MissingChequeDetailsCode, "Çek tahsilatında çek numarası ve vade tarihi zorunlu.");

    public static ErpWriteError MissingNoteDetails() =>
        new(MissingNoteDetailsCode, "Senet tahsilatında senet numarası ve vade tarihi zorunlu.");

    // ---- company settings and user mapping ------------------------------------------------

    public const string ErpContextMissingCode = "ERP_CONTEXT_MISSING";
    public const string ErpMappingMissingCode = "ERP_MAPPING_MISSING";

    public static ErpWriteError ErpContextMissing() =>
        new(ErpContextMissingCode, "Sunucu belgeyle birlikte ERP ayarlarını göndermedi. Sunucu güncellemesi bekleniyor.", Retryable: true);

    /// <param name="setting">What is missing, in Turkish (e.g. "depo", "kasa kodu", "kart bankası").</param>
    public static ErpWriteError ErpMappingMissing(string setting) =>
        new(ErpMappingMissingCode, $"ERP aktarım ayarlarında {setting} tanımlı değil. Portal > ERP Aktarım Ayarları ya da Kullanıcılar > Mikro karşılıkları bölümünden girin.");

    // ---- what the ERP says ----------------------------------------------------------------

    public const string CustomerNotFoundCode = "CUSTOMER_NOT_FOUND";
    public const string CustomerLockedCode = "CUSTOMER_LOCKED";
    public const string StockNotFoundCode = "STOCK_NOT_FOUND";
    public const string WarehouseNotFoundCode = "WAREHOUSE_NOT_FOUND";
    public const string CashAccountNotFoundCode = "CASH_ACCOUNT_NOT_FOUND";
    public const string BankAccountNotFoundCode = "BANK_ACCOUNT_NOT_FOUND";
    public const string SalespersonNotFoundCode = "SALESPERSON_NOT_FOUND";
    public const string PriceListNotFoundCode = "PRICE_LIST_NOT_FOUND";
    public const string FieldTooLongCode = "FIELD_TOO_LONG";
    public const string TotalMismatchCode = "TOTAL_MISMATCH";
    public const string ErpVersionNotSupportedCode = "ERP_VERSION_NOT_SUPPORTED";
    public const string ErpUnavailableCode = "ERP_UNAVAILABLE";

    public static ErpWriteError CustomerNotFound(string customerCode) =>
        new(CustomerNotFoundCode, $"Müşteri ERP'de bulunamadı: {Shown(customerCode)}.");

    public static ErpWriteError CustomerLocked(string customerCode) =>
        new(CustomerLockedCode, $"Müşteri ERP'de kilitli: {Shown(customerCode)}.");

    public static ErpWriteError StockNotFound(string stockCode) =>
        new(StockNotFoundCode, $"Ürün ERP'de bulunamadı: {Shown(stockCode)}.");

    public static ErpWriteError WarehouseNotFound(int warehouseNo) =>
        new(WarehouseNotFoundCode, $"Depo ERP'de bulunamadı: {warehouseNo}.");

    public static ErpWriteError CashAccountNotFound(string code) =>
        new(CashAccountNotFoundCode, $"Kasa ERP'de bulunamadı: {Shown(code)}.");

    public static ErpWriteError BankAccountNotFound(string code) =>
        new(BankAccountNotFoundCode, $"Banka hesabı ERP'de bulunamadı: {Shown(code)}.");

    public static ErpWriteError SalespersonNotFound(string code) =>
        new(SalespersonNotFoundCode, $"Temsilci ERP'de bulunamadı: {Shown(code)}.");

    public static ErpWriteError PriceListNotFound(int priceListNo) =>
        new(PriceListNotFoundCode, $"Fiyat listesi ERP'de bulunamadı: {priceListNo}.");

    /// <param name="field">The field in Turkish (e.g. "müşteri kodu").</param>
    /// <param name="maxLength">The ERP column width.</param>
    public static ErpWriteError FieldTooLong(string field, int maxLength) =>
        new(FieldTooLongCode, $"{field} ERP'nin kabul ettiği uzunluğu ({maxLength} karakter) aşıyor.");

    /// <param name="difference">ERP total minus phone total.</param>
    public static ErpWriteError TotalMismatch(decimal difference) =>
        new(TotalMismatchCode, $"Telefondaki toplam ile ERP hesabı tutmuyor (fark {Math.Abs(difference).ToString("N2", Turkish)} TL). Belge yazılmadı.");

    public static ErpWriteError ErpVersionNotSupported(string version) =>
        new(ErpVersionNotSupportedCode, $"Bu ERP sürümüne ({Shown(version)}) telefon belgesi henüz yazılamıyor.");

    public static ErpWriteError ErpUnavailable() =>
        new(ErpUnavailableCode, "ERP veritabanına şu an ulaşılamıyor; belge otomatik olarak yeniden denenecek.", Retryable: true);
}
