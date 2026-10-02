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

    /// <summary>Sales targets, teams and route plans (GOAL_HEDEF_RUT): the server's <c>CanManageTargets</c>.</summary>
    Targets,

    /// <summary>
    /// The customer web catalog and its order requests (GOAL_MUSTERI_KATALOGU): the server's
    /// <c>CanManageCustomerCatalog</c>. Opens only when the company has the <see cref="PortalModules.CustomerCatalog"/> module.
    /// </summary>
    CustomerCatalog,

    /// <summary>
    /// The company's file storage: quota, "Alan aç" and the trash (GOAL_DEPOLAMA_R2 P1): the server's locked
    /// <c>action.storage.manage</c> (ADMIN and MANAGER).
    /// </summary>
    Storage,

    /// <summary>Entering a sale from the panel (GOAL_PANEL_GIRIS): the phone's <c>module.sales</c>.</summary>
    EntrySales,

    /// <summary>Entering a collection: <c>module.collection</c>.</summary>
    EntryCollection,

    /// <summary>Entering a purchase: <c>module.purchase</c>.</summary>
    EntryPurchase,

    /// <summary>Entering a return: <c>module.returns</c>.</summary>
    EntryReturns,

    /// <summary>Entering a tediye: <c>module.disbursement</c>.</summary>
    EntryDisbursement,

    /// <summary>Entering an expense: <c>module.expenses</c>.</summary>
    EntryExpenses,

    /// <summary>The panel's entries list: open to whoever may enter any kind.</summary>
    Entries,

    /// <summary>Tasks and notifications (GOAL_PANEL_GIRIS P6): the phone's <c>module.tasks</c>.</summary>
    Tasks,
}

/// <summary>Sellable add-ons a company may have (the server's <c>TenantModules</c>); the session carries the bought ones.</summary>
public static class PortalModules
{
    public const string CustomerCatalog = "customer_catalog";

    /// <summary>The module an area needs besides the role, or null when the role is enough.</summary>
    public static string? For(PortalArea area) => area == PortalArea.CustomerCatalog ? CustomerCatalog : null;
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

    /// <summary>
    /// The permission key behind an area (GOAL_YETKILER). Users stays with the ADMIN role: managing users and
    /// permissions is never delegated.
    /// </summary>
    public static string? KeyOf(PortalArea area) => area switch
    {
        PortalArea.Reports => "portal.reports",
        PortalArea.Ledger => "portal.ledger",
        PortalArea.Approvals => "portal.approvals",
        PortalArea.Warehouse => "module.warehouse_queue",
        PortalArea.Displays => "portal.displays",
        PortalArea.ErpWrite => "portal.erp_settings",
        PortalArea.ErpDocuments => "portal.erp_documents",
        PortalArea.NativeAudit => "portal.audit",
        PortalArea.Targets => "portal.targets",
        PortalArea.CustomerCatalog => "action.customer_catalog.manage",
        PortalArea.Storage => "action.storage.manage",
        PortalArea.EntrySales => "module.sales",
        PortalArea.EntryCollection => "module.collection",
        PortalArea.EntryPurchase => "module.purchase",
        PortalArea.EntryReturns => "module.returns",
        PortalArea.EntryDisbursement => "module.disbursement",
        PortalArea.EntryExpenses => "module.expenses",
        PortalArea.Tasks => "module.tasks",
        _ => null,
    };

    /// <summary>The entry areas, in the menu's order.</summary>
    public static readonly IReadOnlyList<PortalArea> EntryAreas =
        [PortalArea.EntrySales, PortalArea.EntryCollection, PortalArea.EntryPurchase, PortalArea.EntryReturns, PortalArea.EntryDisbursement, PortalArea.EntryExpenses];

    /// <summary>
    /// What the user's permissions open; a session without permissions (an older server, or a session saved
    /// before them) falls back to the roles. The customer catalog's and the storage's keys are locked to ADMIN and
    /// MANAGER on the server, so another role stays out even if a permission said otherwise.
    /// </summary>
    public static bool Allows(IReadOnlyCollection<string> roles, IReadOnlyDictionary<string, bool>? permissions, PortalArea area) =>
        area == PortalArea.Entries
            ? EntryAreas.Any(entry => Allows(roles, permissions, entry))
            : (area is not (PortalArea.CustomerCatalog or PortalArea.Storage) || Allows(roles, area))
        && (permissions is not null && KeyOf(area) is { } key && permissions.TryGetValue(key, out var allowed)
            ? allowed
            : Allows(roles, area));

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
        PortalArea.Targets => roles.Any(r => r is Admin or Manager),
        PortalArea.CustomerCatalog => roles.Any(r => r is Admin or Manager),
        PortalArea.Storage => roles.Any(r => r is Admin or Manager),
        // The phone modules' defaults (ADMIN, MANAGER, SALES); a sales user never reaches the panel.
        PortalArea.EntrySales or PortalArea.EntryCollection or PortalArea.EntryPurchase or PortalArea.EntryReturns
            or PortalArea.EntryDisbursement or PortalArea.EntryExpenses or PortalArea.Tasks => roles.Any(r => r is Admin or Manager),
        PortalArea.Entries => EntryAreas.Any(entry => Allows(roles, entry)),
        _ => false,
    };

    /// <summary>
    /// Where a user lands after signing in, and where a page they may not open sends them:
    /// the day summary for managers, the approval desk for accounting, the order queue for the warehouse.
    /// </summary>
    public static string HomePage(IReadOnlyCollection<string> roles, IReadOnlyDictionary<string, bool>? permissions = null) =>
        Allows(roles, permissions, PortalArea.Reports) ? ""
        : Allows(roles, permissions, PortalArea.Approvals) ? "muhasebe"
        : Allows(roles, permissions, PortalArea.Warehouse) ? "depo"
        : "login";
}
