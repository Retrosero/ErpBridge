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
        "PORTAL_REQUIRES_MANAGER" => "Bu bölüm için rolünüz yetkili değil. Firma admininizle görüşün.",
        "PORTAL_SALES_ONLY" => "Saha hesapları telefonda çalışır; panele admin, yönetici, muhasebe ve depo rolleri girer.",
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
        "INVALID_ROLE" => "En az bir rol seçin.",
        "USER_NOT_FOUND" => "Kullanıcı bulunamadı.",
        "PAIRING_NOT_FOUND" => "Bu kodla bekleyen ekran yok. Ekrandaki kodu kontrol edin; kod on dakikada bir yenilenir.",
        "PAIRING_EXPIRED" => "Kodun süresi doldu; ekran yeni kod gösterecek.",
        "DISPLAY_NOT_FOUND" => "Ekran bulunamadı.",
        "FULFILLMENT_NOT_FOUND" => "Sipariş depo kuyruğunda bulunamadı.",
        "INVALID_DISPLAY_NAME" => "Ekran adı zorunludur (en çok 80 karakter).",
        "WAREHOUSE_MANAGER_REQUIRED" => "Bu işlem firma admini ve yöneticiler içindir.",
        "WAREHOUSE_ROLE_REQUIRED" => "Rolünüz depo işlerini kapsamıyor.",
        "FULFILLMENT_STATE_CHANGED" => "Sipariş az önce başka biri tarafından değiştirildi; liste yenilendi.",
        "FULFILLMENT_CANNOT_UNDO" => "Bu adım geri alınamaz.",
        "UNDO_NOT_ALLOWED" => "Adımı yalnızca atan kişi 5 dakika içinde ya da bir yönetici geri alabilir.",
        "INVALID_ASSIGNEE" => "Seçilen kişi firmada aktif bir depo çalışanı değil.",
        "UNKNOWN_FULFILLMENT_ACTION" => "Bu işlem tanınmıyor.",
        "INVALID_VEHICLE_PLATE" => "Plaka en çok 16 karakter olabilir.",
        "WAREHOUSE_DISABLED" => "Depo modülü kapalı. Önce modülü açın.",
        "INVALID_BACKFILL_DAYS" => "0 ile 30 gün arasında bir değer girin.",
        "INVALID_WAREHOUSE_SETTINGS" => "Eşikler 1-1440 dakika olmalı ve her sarı eşik kırmızıdan küçük olmalı.",
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

    /// <summary>The Istanbul business day of an instant.</summary>
    public static DateOnly DayOf(DateTimeOffset instant) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, Istanbul).DateTime);

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

    /// <summary>One or two capital letters for an avatar ("Ali Yılmaz" → "AY").</summary>
    public static string Initials(string? name)
    {
        var parts = (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..1].ToUpper(Turkish),
            _ => (parts[0][..1] + parts[^1][..1]).ToUpper(Turkish),
        };
    }

    public static string Role(string role) => Session.PortalRoles.Label(role);

    /// <summary>How long a request has waited: "az önce", "12 dk", "3 sa 5 dk", "2 gün".</summary>
    public static string Waiting(DateTimeOffset since, DateTimeOffset now)
    {
        var waited = now - since;
        if (waited < TimeSpan.FromMinutes(1)) return "az önce";
        if (waited < TimeSpan.FromHours(1)) return $"{(int)waited.TotalMinutes} dk";
        if (waited < TimeSpan.FromDays(1)) return waited.Minutes == 0 ? $"{(int)waited.TotalHours} sa" : $"{(int)waited.TotalHours} sa {waited.Minutes} dk";
        return $"{(int)waited.TotalDays} gün";
    }

    /// <summary>A measured duration: "—", "45 sn", "12 dk", "1 sa 5 dk", "2 gün 3 sa".</summary>
    public static string Duration(long? seconds)
    {
        if (seconds is not { } value) return "—";
        if (value < 60) return $"{Math.Max(value, 0)} sn";
        var span = TimeSpan.FromSeconds(value);
        if (span < TimeSpan.FromHours(1)) return $"{(int)span.TotalMinutes} dk";
        if (span < TimeSpan.FromDays(1)) return span.Minutes == 0 ? $"{(int)span.TotalHours} sa" : $"{(int)span.TotalHours} sa {span.Minutes} dk";
        return span.Hours == 0 ? $"{(int)span.TotalDays} gün" : $"{(int)span.TotalDays} gün {span.Hours} sa";
    }

    /// <summary>A warehouse step as the timeline names it.</summary>
    public static string FulfillmentAction(string action) => action switch
    {
        "QUEUED" => "Kuyruğa girdi",
        "START" => "Hazırlamaya başlandı",
        "PACK" => "Paketlendi",
        "LOAD" => "Araca yüklendi",
        "UNDO" => "Geri alındı",
        "CANCEL" => "İptal edildi",
        "REASSIGN" => "Başkasına verildi",
        "ERP_FAILED" => "ERP'ye yazılamadı",
        _ => action,
    };

    public static string FulfillmentStatus(string status) => status switch
    {
        "PENDING" => "Bekliyor",
        "PREPARING" => "Hazırlanıyor",
        "PACKED" => "Paketlendi",
        "LOADED" => "Yüklendi",
        "CANCELLED" => "İptal",
        _ => status,
    };

    public static string PaymentMethod(string? method) => string.IsNullOrWhiteSpace(method) ? "—" : method;

    /// <summary>"Yönetici · Depo".</summary>
    public static string Roles(IEnumerable<string> roles) => string.Join(" · ", roles.Select(Role));
}
