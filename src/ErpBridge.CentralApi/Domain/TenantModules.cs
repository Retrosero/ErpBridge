namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A sellable add-on switched on for one company. Only the operator (Admin console) changes
/// the set. Two products share the table: phone add-ons (<see cref="TenantModules.Known"/>),
/// which phones read from the session (<c>modules</c>) and hide what is not bought, and Go
/// desktop app modules (<see cref="TenantModules.GoKnown"/>, <c>go_</c> prefix), which travel in
/// the signed Go license token.
/// </summary>
public sealed class TenantModule
{
    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>One of <see cref="TenantModules.Known"/> or <see cref="TenantModules.GoKnown"/>, lowercase.</summary>
    public string ModuleKey { get; set; } = string.Empty;

    public DateTimeOffset EnabledAtUtc { get; set; }

    /// <summary>The operator who switched it on (admin e-mail); null when unknown.</summary>
    public const int EnabledByMaxLength = 128;
    public string? EnabledBy { get; set; }
}

/// <summary>Keys of <see cref="TenantModule.ModuleKey"/>.</summary>
public static class TenantModules
{
    /// <summary>Product import from the company's XML feed (images and descriptions; every field in a native company).</summary>
    public const string XmlImport = "xml_import";

    /// <summary>
    /// The web catalog customers sign in to (docs/GOAL_MUSTERI_KATALOGU.md). The company still publishes it
    /// itself (<c>catalog_settings.IsEnabled</c>); without this module it is never served.
    /// </summary>
    public const string CustomerCatalog = "customer_catalog";

    /// <summary>
    /// Company add-ons of the phone and the portal, managed by <c>PUT /admin/tenants/{id}/mobile/modules</c>
    /// and carried in the session's <c>modules</c>. Never holds a <see cref="GoPrefix"/> key: that endpoint
    /// leaves Go modules alone.
    /// </summary>
    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.Ordinal) { XmlImport, CustomerCatalog };

    /// <summary>
    /// Prefix of every Go desktop app module. The Go license token carries the company's keys
    /// with this prefix; phone sessions never do.
    /// </summary>
    public const string GoPrefix = "go_";

    /// <summary>Go: AI assistant.</summary>
    public const string GoAi = "go_ai";

    /// <summary>Go: e-invoice / e-archive.</summary>
    public const string GoEInvoice = "go_einvoice";

    /// <summary>Go: ERP connection.</summary>
    public const string GoErp = "go_erp";

    /// <summary>Go: reports and profit analysis.</summary>
    public const string GoReports = "go_reports";

    /// <summary>Go: competition tracking and smart pricing.</summary>
    public const string GoCompetition = "go_competition";

    /// <summary>
    /// Go desktop app modules, managed by <c>PUT /admin/licenses/{id}/go-modules</c>. The keys
    /// must match the Go app's module catalog exactly; the app opens a module whose key is in its token.
    /// </summary>
    public static readonly IReadOnlySet<string> GoKnown = new HashSet<string>(StringComparer.Ordinal)
    {
        GoAi, GoEInvoice, GoErp, GoReports, GoCompetition,
    };

    /// <summary>True for a Go desktop app module key (<see cref="GoPrefix"/>).</summary>
    public static bool IsGo(string moduleKey) => moduleKey.StartsWith(GoPrefix, StringComparison.Ordinal);
}

/// <summary>
/// The company's XML product feed, written by a company administrator from the phone and read
/// by every phone of the company, which downloads and imports the feed itself.
/// </summary>
public sealed class TenantXmlFeedSettings
{
    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string Url { get; set; } = string.Empty;

    /// <summary>Path of the repeating product element inside the document.</summary>
    public string RecordPath { get; set; } = string.Empty;

    /// <summary>Target field → candidate paths inside a record, as a JSON object of string arrays.</summary>
    public string MappingJson { get; set; } = "{}";

    public bool DownloadImages { get; set; } = true;

    public bool ImportDescriptions { get; set; }

    /// <summary>Import every mapped field; always false in an ERP company (the ERP keeps the master data).</summary>
    public bool FullImport { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
