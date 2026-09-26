namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A sellable add-on switched on for one company. Only the operator (Admin console) changes
/// the set; phones read it from the session (<c>modules</c>) and hide what is not bought.
/// </summary>
public sealed class TenantModule
{
    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>One of <see cref="TenantModules.Known"/>, lowercase.</summary>
    public string ModuleKey { get; set; } = string.Empty;

    public DateTimeOffset EnabledAtUtc { get; set; }

    /// <summary>The operator who switched it on (admin e-mail); null when unknown.</summary>
    public string? EnabledBy { get; set; }
}

/// <summary>Keys of <see cref="TenantModule.ModuleKey"/>.</summary>
public static class TenantModules
{
    /// <summary>Product import from the company's XML feed (images and descriptions; every field in a native company).</summary>
    public const string XmlImport = "xml_import";

    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.Ordinal) { XmlImport };
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
