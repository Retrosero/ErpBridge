using System.Globalization;

namespace ErpBridge.Portal.Api;

/// <summary>
/// Turkish text for the central API's error codes. The wording follows the phone app
/// (Siparis_Cepte <c>AccountRepository.messageFor</c>) so a manager reads the same message
/// in both places. No purchase wording: seats are sold outside the apps.
/// </summary>
public static class PortalMessages
{
    public static string For(string code) => Known(code) ?? $"İşlem tamamlanamadı ({code}). Lütfen tekrar deneyin.";

    /// <summary>Whether the panel has its own wording for the code.</summary>
    public static bool Knows(string code) => Known(code) is not null;

    /// <summary>
    /// The text shown for a refusal: the panel's own for a code it knows; for one it does not, the server's message
    /// when the server worded it in Turkish (the catalog endpoints do), else the generic text quoting the code.
    /// </summary>
    public static string For(string code, string? serverMessage) =>
        Known(code) ?? (LooksTurkish(serverMessage) ? serverMessage!.Trim() : For(code));

    /// <summary>A message with a letter only Turkish uses — the server's English messages have none.</summary>
    public static bool LooksTurkish(string? message) =>
        !string.IsNullOrWhiteSpace(message) && message.IndexOfAny(TurkishLetters) >= 0;

    private static readonly char[] TurkishLetters = ['ç', 'ğ', 'ı', 'ö', 'ş', 'ü', 'Ç', 'Ğ', 'İ', 'Ö', 'Ş', 'Ü'];

    private static string? Known(string code) => code switch
    {
        "INVALID_CREDENTIALS" => "Firma kodu, kullanıcı adı veya parola hatalı.",
        "USER_INACTIVE" => "Kullanıcınız devre dışı bırakılmış. Firma yöneticinizle görüşün.",
        "DEVICE_REVOKED" => "Panel girişiniz firma için engellenmiş. Firma yöneticinizle görüşün.",
        "TENANT_INACTIVE" => "Firma hesabı kapalı.",
        "SUBSCRIPTION_EXPIRED" => "Firmanın kullanım süresi dolmuş. Firma yöneticinizle görüşün.",
        "SUBSCRIPTION_REQUIRED" => "Firmanın tanımlı kullanıcı hakkı yok. Firma yöneticinizle görüşün.",
        "SESSION_REVOKED" or "INVALID_TOKEN" => "Oturumunuz sona erdi. Lütfen yeniden giriş yapın.",
        "PORTAL_REQUIRES_MANAGER" => "Bu bölüm için rolünüz yetkili değil. Firma admininizle görüşün.",
        "TARGETS_REQUIRE_MANAGER" => "Hedefleri yalnız admin ve yöneticiler girebilir.",
        "TARGET_OUT_OF_SCOPE" => "Bu kişi ya da ekip sizin sorumluluğunuzda değil.",
        "TARGET_INVALID_PERIOD" => "Dönem okunamadı.",
        "TARGET_INVALID_VALUE" => "Değer geçersiz.",
        "TARGET_NO_CHILDREN" => "Dağıtılacak kişi ya da ekip yok.",
        "TARGET_INVALID_SETTINGS" => "En az bir çalışma günü seçilmeli.",
        "ROUTES_REQUIRE_MANAGER" => "Rut planını yalnız admin ve yöneticiler yapar.",
        "ROUTE_INVALID" => "Rut planı kaydedilemedi. Alanları kontrol edin.",
        "TARGET_CONFLICT" => "Aynı hedef şu anda başka biri tarafından da kaydedildi. Sayfayı yenileyip tekrar deneyin.",
        "TEAM_ADMIN_REQUIRED" => "Ekip yapısını yalnız admin değiştirebilir.",
        "TEAM_NOT_FOUND" => "Ekip bulunamadı; liste yenilendi.",
        "TEAM_NAME_TAKEN" => "Bu adla bir kayıt zaten var.",
        "TEAM_HAS_CHILDREN" => "Önce bu bölgedeki ekipleri başka bölgeye taşıyın ya da silin.",
        "TEAM_INVALID" => "Ekip bilgileri geçersiz. Alanları kontrol edin.",
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
        "INVALID_ERP_SETTINGS" => "Ayarlar kaydedilmedi: bir alan ERP'nin kabul ettiği biçimde değil (seri en çok 6, kod en çok 25 karakter; ERP kullanıcı no 0–32767; teslim günü 0–365; portföy kasaları boş olamaz).",
        "ERP_NOT_CONNECTED" => "Bu firmanın ERP bağlantısı yok; belgeler sunucudaki defterde tutuluyor.",
        "JOB_NOT_RETRYABLE" => "Belge zaten yazılmış ya da sırada bekliyor; yeniden gönderilmedi.",
        "JOB_NOT_FOUND" => "Belge bulunamadı.",
        "INVALID_QUERY" => "Süzgeç geçersiz: tarih aralığı en çok 31 gün olmalı.",
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
        "TENANT_IS_NOT_NATIVE" => "Bu firmanın kartları ERP'den yönetilir; panelden düzenlenemez.",
        "ROLE_NOT_ALLOWED" => "Kart ekleme ve düzenleme yalnızca firma admini içindir.",
        "STOCK_CARD_NOT_FOUND" => "Ürün bulunamadı; liste yenilendi.",
        "INVALID_STOCK_CARD" => "Ürün kaydedilemedi: kod ve ad zorunludur, kod en çok 64 karakter olabilir.",
        "STOCK_CARD_REJECTED" => "Bu üründe satış veya başka bir hareket var; silinemez.",
        "BARCODE_IN_USE" => "Bu barkod başka bir ürüne tanımlı.",
        "INVALID_CUSTOMER_CARD" => "Cari kaydedilemedi: kod ve unvan zorunludur, kod en çok 64 karakter olabilir.",
        "CUSTOMER_CARD_REJECTED" => "Bu cari kaydedilemedi.",
        "INVALID_PAYMENT" => "İşlem kaydedilemedi: cari bulunamadı ya da tutar geçersiz.",
        "PAYMENT_REJECTED" => "Bu işlem kaydedilemedi.",
        "INVALID_CARD_BATCH" => "İçe aktarma gönderilemedi: parça boş ya da 500 karttan büyük.",
        "CARD_BATCH_REJECTED" => "Bu parçadaki kartların hiçbiri kaydedilemedi.",
        "INVALID_SALES_DOCUMENT" => "Evrak kaydedilemedi: cari bulunamadı, kalem eksik ya da tarih geçersiz.",
        "SALES_DOCUMENT_REJECTED" => "Evrak kaydedilemedi: kalemlerden biri bilinmeyen bir ürün ya da tutar geçersiz.",
        "INVALID_DOCUMENT_EDIT_REQUEST" => "Düzeltme kaydedilemedi: gerekçe, cari ve kalemleri kontrol edin.",
        "DOCUMENT_EDIT_REJECTED" => "Düzeltme kaydedilemedi; eski evrak değişmedi. Kalemleri ve belge numarasını kontrol edin.",
        "INVALID_DOCUMENT_VOID_REQUEST" => "İptal için gerekçe gerekli.",
        "DOCUMENT_VOID_REJECTED" => "Evrak iptal edilemedi.",
        "DOCUMENT_NOT_FOUND" => "Evrak bulunamadı; liste yenilendi.",
        "DOCUMENT_NOT_EDITABLE" => "Bu evrak türü panelden düzenlenemez.",
        "ALREADY_VOIDED" => "Bu kayıt zaten iptal edilmiş.",
        "INVALID_STOCK_COUNT" => "Sayım kaydedilemedi: gerekçe zorunlu, miktar eksi olamaz ve her ürün bir kez sayılır.",
        "STOCK_COUNT_REJECTED" => "Sayım kaydedilemedi: ürünlerden biri bulunamadı ya da sayım zaten iptal edilmiş.",
        "STOCK_COUNT_NOT_FOUND" => "Sayım bulunamadı; liste yenilendi.",
        // Customer catalog management (GOAL_MUSTERI_KATALOGU §5.1).
        "MODULE_NOT_ENABLED" => "Bu ek modül firmanız için açık değil.",
        "CATALOG_MANAGE_REQUIRED" => "Müşteri kataloğunu yalnız admin ve yöneticiler yönetebilir.",
        "CATALOG_CHANGED" => "Katalog bu arada başka biri tarafından değiştirildi; güncel hâli yüklendi. Değişikliklerinizi yeniden yapın.",
        "UNKNOWN_PRICE_LIST" => "Seçilen fiyat listesi artık yok; listeyi yeniden seçin.",
        "CARTON_QUANTITY_REQUIRED" => "Yalnız koli satışı için ürünün koli adedi (en az 2) olmalı.",
        "CUSTOMER_NOT_FOUND" => "Cari bulunamadı.",
        "CATALOG_ACCOUNT_NOT_FOUND" => "Katalog erişimi bulunamadı; liste yenilendi.",
        "CATALOG_ACCOUNT_EXISTS" or "CUSTOMER_HAS_ACCOUNT" => "Bu carinin zaten bir katalog erişimi var.",
        "CATALOG_USERNAME_TAKEN" => "Bu kullanıcı adı başka bir katalog erişiminde kullanılıyor.",
        "INVALID_DISCOUNT" => "İskonto 0 ile 99,99 arasında olmalı.",
        "INVALID_IMAGE" => "Dosya JPEG, PNG ya da WebP görseli değil.",
        "IMAGE_TOO_LARGE" => "Görsel çok büyük: büyük boy en çok 1 MB, küçük boy en çok 200 KB olabilir.",
        "CATALOG_IMAGE_LIMIT" => "Bir ürüne en çok 8 görsel eklenebilir.",
        "CATALOG_IMAGE_QUOTA_EXCEEDED" => "Firmanın görsel kotası doldu; kullanılmayan görselleri silin.",
        "INVALID_IMAGE_URL" => "Görsel bağlantısı https:// ile başlayan, herkese açık bir adres olmalı.",
        "INVALID_CARTON_QUANTITY" => "Koli adedi en az 2 olmalı (boş bırakılırsa ERP'deki koli kullanılır).",
        "INVALID_VISIBILITY" => "Ürün görünürlüğü kaydedilemedi: seçilen kategori ya da ürünlerden biri geçersiz.",
        "INVALID_RESPONSIBLE_USER" => "Sorumlu kişi firmanın aktif bir kullanıcısı olmalı.",
        "CATALOG_IMAGE_NOT_FOUND" => "Görsel bulunamadı; başka biri silmiş olabilir. Ürünü yeniden açın.",
        "INVALID_BODY" => "İstek eksik ya da hatalı; alanları kontrol edip tekrar deneyin.",
        "CATALOG_ORDER_NOT_FOUND" => "Müşteri siparişi bulunamadı; liste yenilendi.",
        "CATALOG_ORDER_TAKEN" => "Bu siparişle az önce başka biri ilgilenmeye başladı.",
        "CATALOG_ORDER_CLOSED" => "Bu sipariş kapanmış (siparişe çevrilmiş ya da reddedilmiş); liste yenilendi.",
        "CATALOG_ORDER_ALREADY_CONVERTED" => "Bu müşteri siparişi başka bir belgeyle zaten siparişe çevrilmiş.",
        "PRICE_CHANGED" => "Fiyatlar bu arada değişti; güncel tutarı kontrol edip yeniden onaylayın.",
        "CART_INVALID" => "Talepteki bazı ürünler artık satılamıyor (gizlendi ya da talebin fiyat listesinde fiyatı yok).",
        "ERP_MAPPING_MISSING" => "Belgenin kesileceği kişinin ERP eşlemesi eksik; Kullanıcılar > Mikro eşlemesini ya da ERP ayarlarını tamamlayın.",
        "CATALOG_CONVERSION_FAILED" => "Satış deftere işlenemedi; talep açık kaldı. Cari ve ürün kartlarını kontrol edin.",
        "RATE_LIMITED" or "HTTP_429" => "Çok fazla istek gönderildi; biraz bekleyip tekrar deneyin.",
        _ => null,
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
