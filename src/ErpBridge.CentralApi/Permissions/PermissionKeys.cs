namespace ErpBridge.CentralApi.Permissions;

/// <summary>
/// Permission keys (GOAL_YETKILER). A key is permanent: it is stored in the database and on phones, so it
/// is never renamed; a key that is no longer used is left in place and ignored.
/// </summary>
public static class PermissionKeys
{
    // ---- Modüller (telefon) ---------------------------------------------------------------------
    public const string ModuleSales = "module.sales";
    public const string ModuleQuotes = "module.quotes";
    public const string ModuleSuspendedSales = "module.suspended_sales";
    public const string ModulePurchase = "module.purchase";
    public const string ModuleReturns = "module.returns";
    public const string ModuleCollection = "module.collection";
    public const string ModuleDisbursement = "module.disbursement";
    public const string ModuleCashbox = "module.cashbox";
    public const string ModuleEod = "module.eod";
    public const string ModuleCustomers = "module.customers";
    public const string ModuleCatalog = "module.catalog";
    public const string ModuleStocks = "module.stocks";
    public const string ModuleCounting = "module.counting";
    public const string ModuleWarehouses = "module.warehouses";
    public const string ModuleExpenses = "module.expenses";
    public const string ModuleVehicles = "module.vehicles";
    public const string ModuleReports = "module.reports";
    public const string ModuleApprovals = "module.approvals";
    public const string ModuleRoutePlans = "module.route_plans";
    public const string ModuleTasks = "module.tasks";
    public const string ModuleTargets = "module.targets";

    /// <summary>Order preparation, on the phone and the portal's Depo page (was <c>CanOperateWarehouse</c>).</summary>
    public const string ModuleWarehouseQueue = "module.warehouse_queue";

    // ---- Modüller (panel) -----------------------------------------------------------------------
    public const string PortalReports = "portal.reports";
    public const string PortalLedger = "portal.ledger";
    public const string PortalApprovals = "portal.approvals";
    public const string PortalErpDocuments = "portal.erp_documents";
    public const string PortalTargets = "portal.targets";
    public const string PortalDisplays = "portal.displays";
    public const string PortalErpSettings = "portal.erp_settings";
    public const string PortalAudit = "portal.audit";

    // ---- İşlemler -------------------------------------------------------------------------------
    public const string UsersManage = "action.users.manage";
    public const string ApprovalsDecide = "action.approvals.decide";
    public const string ApprovalsManageRules = "action.approvals.manage_rules";
    public const string RoutePlan = "action.route.plan";
    public const string TargetsManage = "action.targets.manage";
    public const string TasksManage = "action.tasks.manage";
    public const string SuspendedSalesManageOthers = "action.suspended_sales.manage_others";
    public const string WarehouseManage = "action.warehouse.manage";
    public const string ProductsEdit = "action.products.edit";
    public const string CustomersEdit = "action.customers.edit";
    public const string MasterDataImport = "action.master_data.import";
    public const string NativeBooksEdit = "action.native_books.edit";
    public const string XmlFeedManage = "action.xml_feed.manage";
    public const string SaleNegativeStock = "action.sale.negative_stock";
    public const string SaleOpenAccount = "action.sale.open_account";

    // ---- Görünürlük -----------------------------------------------------------------------------
    public const string ViewEodAllUsers = "view.eod.all_users";
    public const string ViewExpensesAllUsers = "view.expenses.all_users";
    public const string ViewCustomerBalance = "view.customer.balance";
    public const string ViewCustomerLedger = "view.customer.ledger";
    public const string ViewCustomerRiskLimit = "view.customer.risk_limit";
    public const string ViewCustomerPurchasedProducts = "view.customer.purchased_products";
    public const string ViewProductLastPurchasePrice = "view.product.last_purchase_price";

    // ---- Limitler -------------------------------------------------------------------------------
    public const string LimitSaleLineDiscountPct = "limit.sale.max_line_discount_pct";
    public const string LimitSaleGeneralDiscountPct = "limit.sale.max_general_discount_pct";
    public const string LimitSaleAmount = "limit.sale.max_amount";
    public const string LimitReturnAmount = "limit.return.max_amount";
    public const string LimitPurchaseAmount = "limit.purchase.max_amount";
    public const string LimitDisbursementAmount = "limit.disbursement.max_amount";
}
