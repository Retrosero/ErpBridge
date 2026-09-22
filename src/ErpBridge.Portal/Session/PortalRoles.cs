namespace ErpBridge.Portal.Session;

/// <summary>Parts of the portal a role opens. A page names the one it belongs to.</summary>
public enum PortalArea
{
    /// <summary>Company-wide sales, collection and route reports (Özet, Plasiyerler, Ziyaretler).</summary>
    Reports,

    /// <summary>Customer balances and stock (Cariler, Stok).</summary>
    Ledger,

    /// <summary>The approval centre (Onaylar).</summary>
    Approvals,

    /// <summary>Order preparation (Depo).</summary>
    Warehouse,

    /// <summary>Users and their roles (Kullanıcılar).</summary>
    Users,

    /// <summary>Warehouse TV boards and the warehouse thresholds (Ekranlar).</summary>
    Displays,

    /// <summary>How phone documents are written into the company's ERP (ERP Aktarım Ayarları, Mikro karşılıkları).</summary>
    ErpWrite,

    /// <summary>Documents sent to the ERP and what the agent did with them (ERP Belgeleri).</summary>
    ErpDocuments,

    /// <summary>Who changed a card/payment from the portal, and when (Denetim, GOAL_PANEL_ERPSIZ E7b/D5).</summary>
    NativeAudit,
}

/// <summary>
/// Role names and what each opens in the portal. Mirrors the central API's
/// <c>RolePermissions</c> so the menu shows only what the server will serve; the server stays
/// the real gate (403 <c>PORTAL_REQUIRES_MANAGER</c>). A user's permissions are the union of their roles.
/// </summary>
public static class PortalRoles
{
    public const string Admin = "ADMIN";
    public const string Manager = "MANAGER";
    public const string Accounting = "ACCOUNTING";
    public const string Warehouse = "WAREHOUSE";
    public const string Sales = "SALES";

    /// <summary>Every role, in the server's precedence order.</summary>
    public static readonly IReadOnlyList<string> All = [Admin, Manager, Accounting, Warehouse, Sales];

    public static string Label(string role) => role switch
    {
        Admin => "Admin",
        Manager => "Yönetici",
        Accounting => "Muhasebe",
        Warehouse => "Depo",
        Sales => "Saha",
        _ => role,
    };

    /// <summary>What the role may do, in the words the users page shows.</summary>
    public static string Description(string role) => role switch
    {
        Admin => "Her şeyi görür ve yönetir; kullanıcıları ve rollerini düzenler.",
        Manager => "Raporları, carileri ve stoğu görür; izin verilirse onay verir. Telefonda da çalışır.",
        Accounting => "Yalnız panelde: satış, iade, tahsilat, tediye ve alış onayları; cariler ve stok.",
        Warehouse => "Yalnız panelde: siparişleri hazırlar, paketler ve araca yükler.",
        Sales => "Yalnız telefonda: satış, tahsilat ve ziyaret. Panele giremez.",
        _ => string.Empty,
    };

    public static bool MayUsePortal(IEnumerable<string> roles) => roles.Any(r => r is Admin or Manager or Accounting or Warehouse);

    public static bool Allows(IReadOnlyCollection<string> roles, PortalArea area) => area switch
    {
        PortalArea.Reports => roles.Any(r => r is Admin or Manager),
        PortalArea.Ledger => roles.Any(r => r is Admin or Manager or Accounting),
        PortalArea.Approvals => roles.Any(r => r is Admin or Manager or Accounting),
        PortalArea.Warehouse => roles.Any(r => r is Admin or Manager or Warehouse),
        PortalArea.Users => roles.Contains(Admin),
        PortalArea.Displays => roles.Any(r => r is Admin or Manager),
        PortalArea.ErpWrite => roles.Contains(Admin),
        PortalArea.ErpDocuments => roles.Any(r => r is Admin or Manager or Accounting),
        // D4: only ADMIN edits a native tenant's books, so only ADMIN needs its audit trail.
        PortalArea.NativeAudit => roles.Contains(Admin),
        _ => false,
    };

    /// <summary>
    /// Where a user lands after signing in, and where a page they may not open sends them:
    /// the day summary for managers, the approval desk for accounting, the order queue for the warehouse.
    /// </summary>
    public static string HomePage(IReadOnlyCollection<string> roles) =>
        Allows(roles, PortalArea.Reports) ? ""
        : Allows(roles, PortalArea.Approvals) ? "muhasebe"
        : Allows(roles, PortalArea.Warehouse) ? "depo"
        : "login";
}
