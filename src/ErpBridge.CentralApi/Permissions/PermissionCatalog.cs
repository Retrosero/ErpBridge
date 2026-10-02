using System.Globalization;
using ErpBridge.CentralApi.Domain;
using K = ErpBridge.CentralApi.Permissions.PermissionKeys;
using R = ErpBridge.CentralApi.Domain.MobileUserRoles;

namespace ErpBridge.CentralApi.Permissions;

public enum PermissionGroup
{
    /// <summary>Phone screens.</summary>
    Modules,

    /// <summary>Web portal pages.</summary>
    Portal,

    Actions,
    Visibility,
    Limits,
}

public enum PermissionType
{
    /// <summary>Allowed or not; stored <c>"1"</c>/<c>"0"</c>.</summary>
    Bool,

    /// <summary>A number the user may not go over without approval; stored as an invariant decimal, <c>""</c> = unlimited.</summary>
    Limit,
}

/// <summary>
/// One permission. <see cref="Defaults"/> holds the value of every role when the company has not changed the role's
/// template; values use the storage encoding (<see cref="PermissionValues"/>).
/// </summary>
public sealed record PermissionDefinition(
    string Key,
    PermissionGroup Group,
    PermissionType Type,
    string Label,
    string Description,
    IReadOnlyDictionary<string, string> Defaults,
    string? Unit = null,
    bool ServerEnforced = false,
    bool Locked = false);

/// <summary>
/// The permission catalogue (GOAL_YETKILER): the single list the server, the portal and the phone render and check.
///
/// <para><b>Defaults keep today's behaviour.</b> A company that changed nothing gets exactly what the hard-coded role
/// checks gave before (<c>RolePermissions</c>, <c>ApprovalPermissions</c>); a parity test guards this. Storage keeps
/// only differences from these defaults, so a key added later takes its default everywhere by itself.</para>
///
/// <para>Every key here is enforced somewhere. A right that has no feature behind it yet (changing a document's date,
/// price list or warehouse, customer scoping by route…) is not listed: a switch that does nothing is the fake matrix
/// this replaces.</para>
/// </summary>
public static class PermissionCatalog
{
    /// <summary>Bumped when keys are added or their meaning changes; phones and the portal read it from the session.</summary>
    /// <remarks>
    /// 2: <see cref="K.CustomerCatalogManage"/> (GOAL_MUSTERI_KATALOGU). 3: <see cref="K.StorageManage"/> (GOAL_DEPOLAMA_R2).
    /// 4: <see cref="K.ProductsPhoto"/> (GOAL_DEPOLAMA_R2 S6).
    /// </remarks>
    public const int Version = 4;

    /// <summary>The roles whose template a company may change (ADMIN is fixed at everything).</summary>
    public static readonly IReadOnlyList<string> EditableRoles = [R.Manager, R.Sales, R.Warehouse, R.Accounting];

    public static string GroupLabel(PermissionGroup group) => group switch
    {
        PermissionGroup.Modules => "Modüller (telefon)",
        PermissionGroup.Portal => "Modüller (panel)",
        PermissionGroup.Actions => "İşlemler",
        PermissionGroup.Visibility => "Görünürlük",
        PermissionGroup.Limits => "Limitler",
        _ => group.ToString(),
    };

    public static readonly IReadOnlyList<PermissionDefinition> All = Build();

    private static readonly IReadOnlyDictionary<string, PermissionDefinition> ByKey =
        All.ToDictionary(d => d.Key, StringComparer.Ordinal);

    public static PermissionDefinition? Find(string key) => ByKey.GetValueOrDefault(key);

    private static List<PermissionDefinition> Build()
    {
        var field = new[] { R.Admin, R.Manager, R.Sales };
        var managers = new[] { R.Admin, R.Manager };
        var ledger = new[] { R.Admin, R.Manager, R.Accounting };
        var customerFacing = new[] { R.Admin, R.Manager, R.Sales, R.Accounting };
        var admin = new[] { R.Admin };

        PermissionDefinition Module(string key, string label, string description, bool server = false) =>
            new(key, PermissionGroup.Modules, PermissionType.Bool, label, description, Allow(field), ServerEnforced: server);

        PermissionDefinition Flag(string key, PermissionGroup group, string label, string description, string[] allowed, bool server = false, bool locked = false) =>
            new(key, group, PermissionType.Bool, label, description, Allow(allowed), ServerEnforced: server, Locked: locked);

        PermissionDefinition Limit(string key, string label, string description, string unit) =>
            new(key, PermissionGroup.Limits, PermissionType.Limit, label, description, Unlimited(), unit, ServerEnforced: true);

        return
        [
            Module(K.ModuleSales, "Satış", "Satış ekranı ve satış belgesi gönderme.", server: true),
            Module(K.ModuleQuotes, "Teklif", "Teklif hazırlama."),
            Module(K.ModuleSuspendedSales, "Bekleyen satışlar", "Beklemeye alınan satışları görme ve açma."),
            Module(K.ModulePurchase, "Alış", "Alış faturası girme.", server: true),
            Module(K.ModuleReturns, "İade", "Satış iadesi alma.", server: true),
            Module(K.ModuleCollection, "Tahsilat", "Tahsilat makbuzu kesme.", server: true),
            Module(K.ModuleDisbursement, "Tediye", "Tediye (ödeme) makbuzu kesme.", server: true),
            Module(K.ModuleCashbox, "Kasa", "Kasa defteri."),
            Module(K.ModuleEod, "Gün sonu", "Gün sonu raporu ve Z raporu."),
            Module(K.ModuleCustomers, "Cariler", "Cari listesi ve cari kartı."),
            Module(K.ModuleCatalog, "Katalog", "Ürün kataloğu."),
            Module(K.ModuleStocks, "Stoklar", "Stok listesi ve stok detayı."),
            Module(K.ModuleCounting, "Sayım", "Stok sayımı.", server: true),
            Module(K.ModuleWarehouses, "Depolar", "Depolar arası transfer ve depo stokları."),
            Module(K.ModuleExpenses, "Masraflar", "Gider girişi."),
            Module(K.ModuleVehicles, "Araçlar", "Şirket araçları."),
            Module(K.ModuleReports, "Raporlar", "Telefondaki raporlar."),
            Module(K.ModuleApprovals, "Onaylar", "Kendi onay taleplerini izleme."),
            Module(K.ModuleRoutePlans, "Rut planı", "Rut planını görme."),
            Module(K.ModuleTasks, "Görevler", "Görevleri görme ve yapma."),
            Module(K.ModuleTargets, "Hedefler", "Kendi hedeflerini görme."),
            Flag(K.ModuleWarehouseQueue, PermissionGroup.Modules, "Depo hazırlama", "Siparişleri hazırlama, paketleme ve araca yükleme (telefon ve panel Depo sayfası).",
                [R.Admin, R.Manager, R.Warehouse], server: true),

            Flag(K.PortalReports, PermissionGroup.Portal, "Raporlar", "Özet, plasiyerler ve ziyaretler.", managers, server: true),
            Flag(K.PortalLedger, PermissionGroup.Portal, "Cariler ve stok", "Cari bakiyeleri, ekstreler ve stok.", ledger, server: true),
            Flag(K.PortalApprovals, PermissionGroup.Portal, "Onay merkezi", "Onay taleplerini panelde görme.", ledger),
            Flag(K.PortalErpDocuments, PermissionGroup.Portal, "ERP belgeleri", "ERP'ye gönderilen belgeler ve durumları.", ledger, server: true),
            Flag(K.PortalTargets, PermissionGroup.Portal, "Hedefler ve ekipler", "Hedefler, ekipler, bölgeler ve rut planı sayfaları.", managers, server: true),
            Flag(K.PortalDisplays, PermissionGroup.Portal, "Depo ekranları", "Depo TV ekranları ve eşikleri.", managers, server: true),
            Flag(K.PortalErpSettings, PermissionGroup.Portal, "ERP aktarım ayarları", "Belgelerin ERP'ye nasıl yazılacağı, Mikro karşılıkları.", admin),
            Flag(K.PortalAudit, PermissionGroup.Portal, "Denetim", "Panelden yapılan kart ve belge değişiklikleri.", admin),

            Flag(K.UsersManage, PermissionGroup.Actions, "Kullanıcı ve yetki yönetimi", "Kullanıcı ekleme, rol ve yetki verme. Yalnız Admin.", admin, server: true, locked: true),
            Flag(K.ApprovalsDecide, PermissionGroup.Actions, "Onay verebilir", "Onay taleplerini onaylama ve reddetme. Muhasebe yalnız para belgelerini karar verir; Yönetici için kişiye açılır.",
                [R.Admin, R.Accounting], server: true, locked: true),
            Flag(K.ApprovalsManageRules, PermissionGroup.Actions, "Onay kurallarını değiştirebilir", "Hangi işlemlerin onaya gideceğini belirleme. Yönetici için kişiye açılır.",
                admin, server: true, locked: true),
            Flag(K.RoutePlan, PermissionGroup.Actions, "Rut planlayabilir", "Plasiyerlerin rut planını hazırlama.", managers, server: true),
            Flag(K.TargetsManage, PermissionGroup.Actions, "Hedef girebilir", "Satış hedeflerini girme ve dağıtma.", managers, server: true),
            Flag(K.TasksManage, PermissionGroup.Actions, "Tüm görevleri yönetir", "Firmadaki bütün görevleri görme ve başkasına görev verme.", managers, server: true),
            Flag(K.SuspendedSalesManageOthers, PermissionGroup.Actions, "Başkasının bekleyen satışını siler", "Başkasının beklettiği satışı iptal etme ya da değiştirme.", managers, server: true),
            Flag(K.WarehouseManage, PermissionGroup.Actions, "Depo yönetimi", "Sipariş iptali, yeniden atama, başkasının adımını geri alma, depo ayarları.", managers, server: true),
            Flag(K.ProductsEdit, PermissionGroup.Actions, "Ürün kartı ekler ve düzenler", "ERP'siz firmada ürün kartı açma, değiştirme ve silme.", admin, server: true),
            Flag(K.ProductsPhoto, PermissionGroup.Actions, "Ürün fotoğrafı ekler",
                "Ürüne fotoğraf çekme ya da yükleme, sıralama ve silme. Yalnız görseli değiştirir, ürün kartına dokunmaz; ERP'li firmada da çalışır.", field, server: true),
            Flag(K.CustomersEdit, PermissionGroup.Actions, "Cari kartı ekler ve düzenler", "ERP'siz firmada müşteri kartı açma ve değiştirme.", field),
            Flag(K.MasterDataImport, PermissionGroup.Actions, "Excel ile toplu aktarım", "Ürün ve carileri Excel'den içe alma.", admin),
            Flag(K.NativeBooksEdit, PermissionGroup.Actions, "Belge ve hesap düzeltme", "ERP'siz firmada panelden belge, ödeme ve cari hareketi düzeltme veya iptal.", admin, server: true),
            Flag(K.XmlFeedManage, PermissionGroup.Actions, "XML ürün modülü ayarları", "XML ürün beslemesini kurma ve değiştirme.", admin, server: true),
            // Locked: customers' passwords and discounts stay with admin and manager; a template cannot give them away.
            Flag(K.CustomerCatalogManage, PermissionGroup.Actions, "Web katalog yönetimi",
                "Müşteri kataloğu düzeni, müşteri erişimi (kullanıcı adı/şifre, iskonto) ve katalog siparişleri.", managers, server: true, locked: true),
            // Locked: deleting a company's pictures and seeing every area's usage stays with admin and manager.
            Flag(K.StorageManage, PermissionGroup.Actions, "Depolama yönetimi",
                "Firma depolamasının alanlara göre kullanımı, kullanılmayan görselleri temizleme ve çöp kutusu.", managers, server: true, locked: true),
            Flag(K.SaleNegativeStock, PermissionGroup.Actions, "Eksi stoğa satış", "Firma eksi stoğa izin veriyorsa, stoğu olmayan ürünü satabilir.", field),
            Flag(K.SaleOpenAccount, PermissionGroup.Actions, "Açık hesap (veresiye) satış", "Satışı cari borç olarak kapatabilir; yetkisi yoksa cari borçlu satış onaya gider.", field, server: true),

            Flag(K.ViewEodAllUsers, PermissionGroup.Visibility, "Gün sonunda bütün ekip", "Gün sonunda herkesin işlemlerini görür; kapalıysa yalnız kendisininkini.", managers),
            Flag(K.ViewExpensesAllUsers, PermissionGroup.Visibility, "Bütün ekibin masrafları", "ERP masraf geçmişinde herkesin masraflarını görür.", managers),
            Flag(K.ViewCustomerBalance, PermissionGroup.Visibility, "Cari bakiyesi", "Cari kartında bakiye.", customerFacing),
            Flag(K.ViewCustomerLedger, PermissionGroup.Visibility, "Cari ekstresi", "Cari kartında hareketler (ekstre).", customerFacing),
            Flag(K.ViewCustomerRiskLimit, PermissionGroup.Visibility, "Risk ve kredi limiti", "Cari kartında risk, kredi limiti ve çek riski.", customerFacing),
            Flag(K.ViewCustomerPurchasedProducts, PermissionGroup.Visibility, "Carinin aldığı ürünler", "Cari kartında önceki alımlar.", customerFacing),
            Flag(K.ViewProductLastPurchasePrice, PermissionGroup.Visibility, "Son alış fiyatı", "Alış ekranında ürünün son alış fiyatı.", customerFacing),

            Limit(K.LimitSaleLineDiscountPct, "Satır iskontosu sınırı", "Üstündeki satır iskontosu satışı onaya gönderir.", "%"),
            Limit(K.LimitSaleGeneralDiscountPct, "Sipariş iskontosu sınırı", "Üstündeki genel iskonto satışı onaya gönderir.", "%"),
            Limit(K.LimitSaleAmount, "Onaysız satış tutarı", "Üstündeki satış onaya gider.", "TL"),
            Limit(K.LimitReturnAmount, "Onaysız iade tutarı", "Üstündeki iade onaya gider.", "TL"),
            Limit(K.LimitPurchaseAmount, "Onaysız alış tutarı", "Üstündeki alış onaya gider.", "TL"),
            Limit(K.LimitDisbursementAmount, "Onaysız tediye tutarı", "Üstündeki tediye onaya gider.", "TL"),
        ];
    }

    private static Dictionary<string, string> Allow(params string[] allowed) =>
        MobileUserRoles.All.ToDictionary(role => role, role => allowed.Contains(role) ? PermissionValues.True : PermissionValues.False, StringComparer.Ordinal);

    private static Dictionary<string, string> Unlimited() =>
        MobileUserRoles.All.ToDictionary(role => role, _ => PermissionValues.Unlimited, StringComparer.Ordinal);
}

/// <summary>The storage encoding of permission values, shared by role templates and personal overrides.</summary>
public static class PermissionValues
{
    public const string True = "1";
    public const string False = "0";

    /// <summary>A limit with no ceiling.</summary>
    public const string Unlimited = "";

    public const decimal MaxLimit = 999_999_999_999m;

    public static bool IsTrue(string? value) => value == True;

    /// <summary><c>null</c> = unlimited.</summary>
    public static decimal? ParseLimit(string? value) =>
        string.IsNullOrEmpty(value) ? null
        : decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var limit) ? limit
        : null;

    public static string FormatLimit(decimal? limit) =>
        limit is { } value ? value.ToString("0.####", CultureInfo.InvariantCulture) : Unlimited;

    /// <summary>A value as stored, or null when it does not fit the permission's type.</summary>
    public static string? Normalize(PermissionDefinition definition, string? value)
    {
        var trimmed = value?.Trim();
        if (definition.Type == PermissionType.Bool)
            return trimmed switch { True or "true" or "allow" => True, False or "false" or "deny" => False, _ => null };
        if (string.IsNullOrEmpty(trimmed) || trimmed == "unlimited") return Unlimited;
        if (!decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out var limit)) return null;
        if (limit < 0 || limit > MaxLimit || decimal.Round(limit, 4) != limit) return null;
        if (definition.Unit == "%" && limit > 100) return null;
        return FormatLimit(limit);
    }
}
