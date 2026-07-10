namespace ErpBridge.Erp.Abstractions.Sync;

/// <summary>
/// Cari (customer / vendor) master record carried inside a
/// <see cref="SyncPackage"/>. The field set is the Mikro-friendly superset
/// shared by Phase 5 readers; the adapter strips/expands per-version. Every
/// field other than <see cref="CustomerCode"/> and <see cref="Title1"/> is
/// optional because Mikro cari records are notoriously sparse in the wild.
/// </summary>
/// <param name="CustomerCode">Unique customer code (e.g. "120.01.0001").</param>
/// <param name="Title1">Primary display name — must be non-empty.</param>
/// <param name="Title2">Optional secondary display name (e.g. legal suffix).</param>
/// <param name="TaxOffice">Vergi dairesi.</param>
/// <param name="TaxNo">Vergi / TC kimlik numarası.</param>
/// <param name="GroupCode">Cari grup kodu (for reporting / pricing).</param>
/// <param name="RegionCode">Bölge kodu (used by sales-rep routing).</param>
/// <param name="SalespersonCode">Default plasiyer kodu for orders from this cari.</param>
/// <param name="Currency">Default currency code (TL, USD, EUR, ...).</param>
/// <param name="DefaultWarehouseCode">Default depo kodu for orders from this cari.</param>
/// <param name="IsLocked">True when the cari is locked and must not accept new orders.</param>
/// <param name="IsEInvoiceEnabled">True when the cari is registered for e-fatura.</param>
/// <param name="Phone">Primary phone number.</param>
/// <param name="Email">Primary e-mail address.</param>
/// <param name="Addresses">All addresses attached to the cari (often one).</param>
/// <param name="Contacts">All contact persons attached to the cari.</param>
public sealed record CustomerPayload(
    string CustomerCode,
    string Title1,
    string? Title2,
    string? TaxOffice,
    string? TaxNo,
    string? GroupCode,
    string? RegionCode,
    string? SalespersonCode,
    string? Currency,
    string? DefaultWarehouseCode,
    bool IsLocked,
    bool IsEInvoiceEnabled,
    string? Phone,
    string? Email,
    IReadOnlyList<CustomerAddressPayload> Addresses,
    IReadOnlyList<CustomerContactPayload> Contacts);

/// <summary>
/// One address attached to a <see cref="CustomerPayload"/>. Lat/long are
/// nullable because most legacy addresses do not carry geo coordinates.
/// </summary>
/// <param name="AddressNo">Address sequence number (1-based) within the cari.</param>
/// <param name="City">Şehir / il.</param>
/// <param name="District">İlçe.</param>
/// <param name="Street">Full street / address line.</param>
/// <param name="PostalCode">Posta kodu.</param>
/// <param name="Latitude">Geo latitude (optional).</param>
/// <param name="Longitude">Geo longitude (optional).</param>
/// <param name="SalespersonCode">Plasiyer override for this address (else falls back to <see cref="CustomerPayload.SalespersonCode"/>).</param>
public sealed record CustomerAddressPayload(
    int AddressNo,
    string? City,
    string? District,
    string? Street,
    string? PostalCode,
    double? Latitude,
    double? Longitude,
    string? SalespersonCode);

/// <summary>
/// One contact person attached to a <see cref="CustomerPayload"/>. Every field
/// is optional because some caris only carry a single sales rep pointer.
/// </summary>
/// <param name="FirstName">Ad.</param>
/// <param name="LastName">Soyad.</param>
/// <param name="Email">E-posta.</param>
/// <param name="Mobile">GSM / cep telefonu.</param>
/// <param name="TcIdentityNo">TC kimlik numarası.</param>
/// <param name="TaxNo">Vergi numarası (şahıs şirketleri için).</param>
public sealed record CustomerContactPayload(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Mobile,
    string? TcIdentityNo,
    string? TaxNo);
