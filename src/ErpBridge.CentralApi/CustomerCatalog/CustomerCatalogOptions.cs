namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary><c>CustomerCatalog</c> configuration section (docs/GOAL_MUSTERI_KATALOGU.md).</summary>
public sealed class CustomerCatalogOptions
{
    public const string SectionName = "CustomerCatalog";

    /// <summary>
    /// Host the web catalog is served on (<c>sipariscepte.appsgo.cloud</c>); empty = not served anywhere. Setting it
    /// outside tests requires the reverse proxy in <c>ForwardedHeaders</c> (<c>Program.ValidateRuntimeConfiguration</c>).
    /// </summary>
    public string PublicHost { get; set; } = string.Empty;

    /// <summary>What share links start with (<c>https://sipariscepte.appsgo.cloud</c>); the company code follows.</summary>
    public string PublicBaseUrl { get; set; } = string.Empty;

    /// <summary>Life of a "remember me" customer session.</summary>
    public int TokenDays { get; set; } = 30;

    /// <summary>Life of a customer session without "remember me".</summary>
    public int SessionHours { get; set; } = 12;

    /// <summary>The detail picture (<c>l</c>): at most 1 MB.</summary>
    public int MaxImageBytesLarge { get; set; } = 1024 * 1024;

    /// <summary>The list thumbnail (<c>s</c>): at most 200 KB.</summary>
    public int MaxImageBytesSmall { get; set; } = 200 * 1024;

    public int MaxImagesPerProduct { get; set; } = 8;

    /// <summary>Stored pictures of one company, both sizes together.</summary>
    public long TenantImageQuotaBytes { get; set; } = 1024L * 1024 * 1024;

    /// <summary>Order requests a customer may have waiting (<c>NEW</c> or <c>CLAIMED</c>) at once.</summary>
    public int MaxOpenOrders { get; set; } = 20;

    public int MaxOrderLines { get; set; } = 200;

    /// <summary>The static web catalog, relative to the content root.</summary>
    public string WebRoot { get; set; } = "wwwroot/katalog";
}
