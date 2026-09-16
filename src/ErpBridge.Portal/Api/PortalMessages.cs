using System.Globalization;

namespace ErpBridge.Portal.Api;

/// <summary>
/// Turkish text for the central API's error codes. The wording follows the phone app
/// (Siparis_Cepte <c>AccountRepository.messageFor</c>) so a manager reads the same message
/// in both places. No purchase wording: seats are sold outside the apps.
/// </summary>
public static class PortalMessages
{
    public static string For(string code) => code switch
    {
        "INVALID_CREDENTIALS" => "Firma kodu, kullanıcı adı veya parola hatalı.",
        "USER_INACTIVE" => "Kullanıcınız devre dışı bırakılmış. Firma yöneticinizle görüşün.",
        "DEVICE_REVOKED" => "Panel girişiniz firma için engellenmiş. Firma yöneticinizle görüşün.",
        "TENANT_INACTIVE" => "Firma hesabı kapalı.",
        "SUBSCRIPTION_EXPIRED" => "Firmanın kullanım süresi dolmuş. Firma yöneticinizle görüşün.",
        "SUBSCRIPTION_REQUIRED" => "Firmanın tanımlı kullanıcı hakkı yok. Firma yöneticinizle görüşün.",
        "SESSION_REVOKED" or "INVALID_TOKEN" => "Oturumunuz sona erdi. Lütfen yeniden giriş yapın.",
        "PORTAL_REQUIRES_MANAGER" => "Yönetim paneli yalnızca firma admini ve yöneticiler içindir.",
        "ADMIN_REQUIRED" => "Kullanıcıları yalnızca firma admini yönetebilir.",
        "APPROVER_REQUIRED" => "Onay vermek için yetkiniz yok. Firma admininizle görüşün.",
        "SELF_APPROVAL_NOT_ALLOWED" => "Kendi talebinizi başka bir onay yetkilisi onaylamalı.",
        "APPROVAL_ALREADY_DECIDED" => "Bu talep az önce başka bir yetkili tarafından sonuçlandırıldı.",
        "APPROVAL_STATE_CHANGED" => "Talebin durumu değişmiş; liste yenilendi.",
        "APPROVAL_NOT_FOUND" => "Talep bulunamadı.",
        "APPROVAL_DOCUMENT_FAILED" => "Talepteki belge işlenemedi; talep beklemede kaldı.",
        "SEAT_LIMIT_REACHED" => "Tüm kullanıcı hakları dolu. Yeni kullanıcı için önce birini devre dışı bırakın veya silin.",
        "LAST_ADMIN" => "Firmada en az bir aktif admin kalmalı.",
        "USERNAME_TAKEN" => "Bu kullanıcı adı zaten kullanılıyor.",
        "INVALID_USERNAME" => "Kullanıcı adı 3-64 karakter olmalı; yalnızca küçük harf, rakam, nokta, alt çizgi ve tire.",
        "INVALID_PASSWORD" => "Parola en az 6 karakter olmalı.",
        "INVALID_FULL_NAME" => "Ad soyad zorunludur.",
        "INVALID_ROLE" => "Rol Admin, Yönetici veya Saha olmalı.",
        "USER_NOT_FOUND" => "Kullanıcı bulunamadı.",
        "INVALID_DATE" => "Tarih geçersiz.",
        "INVALID_RANGE" => "Bitiş tarihi başlangıçtan önce olamaz.",
        "RANGE_TOO_LONG" => "En fazla 92 günlük aralık seçilebilir.",
        _ => $"İşlem tamamlanamadı ({code}). Lütfen tekrar deneyin.",
    };
}

/// <summary>Turkish number and date formatting in one place.</summary>
public static class Fmt
{
    public static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static string Money(decimal amount) => amount.ToString("N2", Turkish) + " TL";

    public static string Quantity(decimal quantity) =>
        quantity == decimal.Truncate(quantity) ? quantity.ToString("N0", Turkish) : quantity.ToString("N2", Turkish);

    public static string Day(DateOnly day) => day.ToString("d MMMM yyyy, dddd", Turkish);

    /// <summary>Today in Istanbul — the business day every report counts by.</summary>
    public static DateOnly Today() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Istanbul).DateTime);

    /// <summary>A day as the <c>yyyy-MM-dd</c> the date inputs and the API use.</summary>
    public static string IsoDay(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static bool TryIsoDay(string? value, out DateOnly day) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day);

    /// <summary>Istanbul wall-clock time of an instant.</summary>
    public static string Time(DateTimeOffset instant) => TimeZoneInfo.ConvertTime(instant, Istanbul).ToString("dd.MM.yyyy HH:mm", Turkish);

    /// <summary>Istanbul time of day for Unix milliseconds (the phone's visit time).</summary>
    public static string ClockTime(long? unixMs) =>
        unixMs is { } ms ? TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(ms), Istanbul).ToString("HH:mm", Turkish) : "—";

    public static string ApprovalKind(string kind) => kind switch
    {
        "sale" => "Satış",
        "purchase" => "Alış",
        "return" => "İade",
        "collection" => "Tahsilat",
        "disbursement" => "Tediye",
        "stock_count" => "Sayım",
        "product_card" => "Ürün kartı",
        "customer_card" => "Cari kartı",
        _ => kind,
    };

    public static string VisitStatus(string status) => status switch
    {
        "COMPLETED" => "Ziyaret edildi",
        "SKIPPED" => "Atlandı",
        _ => "Bekliyor",
    };

    private static readonly TimeZoneInfo Istanbul = ResolveIstanbul();

    private static TimeZoneInfo ResolveIstanbul()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.CreateCustomTimeZone("Istanbul", TimeSpan.FromHours(3), "Istanbul", "Istanbul"); }
    }

    public static string Role(string role) => role switch
    {
        "ADMIN" => "Admin",
        "MANAGER" => "Yönetici",
        "SALES" => "Saha",
        _ => role,
    };
}
