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
/// <param name="Balance">Official local-currency ledger balance from Mikro; positive means debit/customer owes us.</param>
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
    IReadOnlyList<CustomerContactPayload> Contacts,
    decimal Balance = 0m);

/// <summary>
/// One address attached to a <see cref="CustomerPayload"/>. Lat/long are
/// nullable because most legacy addresses do not carry geo coordinates.
/// </summary>
/// <param name="CustomerCode">Cari code owning this address.</param>
/// <param name="AddressNo">Address sequence number (1-based) within the cari.</param>
/// <param name="City">Şehir / il.</param>
/// <param name="District">İlçe.</param>
/// <param name="Street">Full street / address line.</param>
/// <param name="PostalCode">Posta kodu.</param>
/// <param name="Latitude">Geo latitude (optional).</param>
/// <param name="Longitude">Geo longitude (optional).</param>
/// <param name="SalespersonCode">Plasiyer override for this address (else falls back to <see cref="CustomerPayload.SalespersonCode"/>).</param>
/// <param name="IsPrintable">Mikro <c>adr_aprint_fl</c> flag.</param>
/// <param name="Avenue">Cadde.</param>
/// <param name="Neighborhood">Mahalle.</param>
/// <param name="StreetName">Sokak.</param>
/// <param name="Quarter">Semt.</param>
/// <param name="ApartmentNo">Apartman numarası.</param>
/// <param name="FlatNo">Daire numarası.</param>
/// <param name="Country">Ülke.</param>
/// <param name="PhoneCountryCode">Telefon ülke kodu.</param>
/// <param name="PhoneAreaCode">Telefon bölge kodu.</param>
/// <param name="PhoneNo">Birincil telefon numarası.</param>
/// <param name="VisitPeriod">Ziyaret periyodu.</param>
/// <param name="VisitDay">Ziyaret günü.</param>
/// <param name="EInvoiceAlias">e-Fatura etiketi.</param>
/// <param name="UpdatedAt">Mikro son güncelleme zamanı.</param>
public sealed record CustomerAddressPayload(
    string CustomerCode,
    int AddressNo,
    string? City,
    string? District,
    string? Street,
    string? PostalCode,
    double? Latitude,
    double? Longitude,
    string? SalespersonCode,
    bool IsPrintable = false,
    string? Avenue = null,
    string? Neighborhood = null,
    string? StreetName = null,
    string? Quarter = null,
    string? ApartmentNo = null,
    string? FlatNo = null,
    string? Country = null,
    string? PhoneCountryCode = null,
    string? PhoneAreaCode = null,
    string? PhoneNo = null,
    int? VisitPeriod = null,
    int? VisitDay = null,
    string? EInvoiceAlias = null,
    DateTime? UpdatedAt = null);

/// <summary>
/// One contact person attached to a <see cref="CustomerPayload"/>. Every field
/// is optional because some caris only carry a single sales rep pointer.
/// </summary>
/// <param name="CustomerCode">Cari code owning this contact.</param>
/// <param name="FirstName">Ad.</param>
/// <param name="LastName">Soyad.</param>
/// <param name="Email">E-posta.</param>
/// <param name="Mobile">GSM / cep telefonu.</param>
/// <param name="TcIdentityNo">TC kimlik numarası.</param>
/// <param name="TaxNo">Vergi numarası (şahıs şirketleri için).</param>
public sealed record CustomerContactPayload(
    string CustomerCode,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Mobile,
    string? TcIdentityNo,
    string? TaxNo);
