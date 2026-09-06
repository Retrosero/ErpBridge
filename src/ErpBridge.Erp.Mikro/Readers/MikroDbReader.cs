using Dapper;
using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.Erp.Mikro.Connection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ErpBridge.Erp.Mikro.Readers;

/// <summary>
/// Dapper-backed implementation of <see cref="IMikroDbReader"/>. Every method
/// opens its own short-lived <see cref="SqlConnection"/>, executes a single
/// fully-parameterised query, and materialises the rows via Dapper's positional
/// record-constructor binding. <c>@firmNo</c> / <c>@warehouseNo</c> parameters
/// keep every call injection-safe.
/// </summary>
/// <remarks>
/// Construction is cheap — the class is intentionally registered as a singleton
/// in <see cref="DependencyInjection.ServiceCollectionExtensions"/>. Connections
/// are opened per call so the adapter never holds an open idle connection; the
/// underlying <c>SqlConnection</c> pooling is handled by ADO.NET.
/// </remarks>
public sealed class MikroDbReader : IMikroDbReader
{
    private static readonly TimeZoneInfo MikroTimeZone = ResolveMikroTimeZone();
    private readonly MikroConnectionFactory _factory;
    private readonly ILogger<MikroDbReader> _logger;

    static MikroDbReader()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    /// <summary>
    /// Build a reader. The factory formats connection strings; the logger
    /// receives one <c>Information</c> line per read with row counts so the
    /// operator can see what came back from Mikro without enabling debug SQL.
    /// </summary>
    public MikroDbReader(MikroConnectionFactory factory, ILogger<MikroDbReader> logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Mikro stores its *_create_date/*_lastup_date columns as Turkey local time,
    // while the API watermark is UTC. Comparing the raw UTC value made every
    // existing row look newer by three hours on the Linux agent.
    private static DateTime? MikroDateTime(DateTimeOffset? utc) =>
        utc is { } value ? TimeZoneInfo.ConvertTime(value, MikroTimeZone).DateTime : null;

    private static TimeZoneInfo ResolveMikroTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); }
    }

    /// <inheritdoc />
    public async Task<BootstrapRecordCounts> GetBootstrapRecordCountsAsync(
        int firmNo,
        int warehouseNo,
        CancellationToken ct = default)
    {
        // This is deliberately a single, scalar-only command. The dashboard
        // must never enumerate the large movement tables merely to display a
        // row count.
        const string sql = @"
SELECT
    (SELECT COUNT_BIG(1) FROM CARI_HESAPLAR WHERE ISNULL(cari_iptal, 0) = 0) AS Customers,
    (SELECT COUNT_BIG(1) FROM CARI_HESAP_ADRESLERI WHERE ISNULL(adr_iptal, 0) = 0) AS CustomerAddresses,
    (SELECT COUNT_BIG(1) FROM CARI_HESAP_YETKILILERI WHERE ISNULL(mye_iptal, 0) = 0) AS CustomerContacts,
    (SELECT COUNT_BIG(1) FROM STOKLAR WHERE ISNULL(sto_iptal, 0) = 0 AND ISNULL(sto_pasif_fl, 0) = 0) AS Stocks,
    (SELECT COUNT_BIG(1) FROM BARKOD_TANIMLARI WHERE ISNULL(bar_iptal, 0) = 0) AS Barcodes,
    (SELECT COUNT_BIG(1) FROM SIPARISLER WHERE sip_firmano = @firmNo AND ISNULL(sip_iptal, 0) = 0 AND sip_kapat_fl = 0) AS OpenOrders,
    ((SELECT COUNT_BIG(1) FROM KASALAR WHERE kas_firma_no = @firmNo AND ISNULL(kas_iptal, 0) = 0)
      + (SELECT COUNT_BIG(1) FROM BANKALAR WHERE ban_firma_no = @firmNo AND ISNULL(ban_iptal, 0) = 0)) AS CashAndBank,
    ((SELECT COUNT_BIG(1) FROM DEPOLAR WHERE dep_firmano = @firmNo AND ISNULL(dep_iptal, 0) = 0)
      + (SELECT COUNT_BIG(1) FROM CARI_PERSONEL_TANIMLARI WHERE ISNULL(cari_per_iptal, 0) = 0)
      + (SELECT COUNT_BIG(1) FROM ODEME_PLANLARI WHERE ISNULL(odp_iptal, 0) = 0)
      + (SELECT COUNT_BIG(1) FROM PROJELER WHERE ISNULL(pro_iptal, 0) = 0)
      + (SELECT COUNT_BIG(1) FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI WHERE ISNULL(sfl_iptal, 0) = 0)) AS Lookups,
    (SELECT COUNT_BIG(1) FROM STOK_SATIS_FIYAT_LISTELERI WHERE ISNULL(sfiyat_iptal, 0) = 0) AS Prices,
    (SELECT COUNT_BIG(1) FROM SATIS_SARTLARI WHERE ISNULL(sat_iptal, 0) = 0) AS SalesConditions,
    (SELECT COUNT_BIG(1) FROM dbo.STOK_HAREKETTEN_ELDEKI_MIKTAR_VIEW WHERE NULLIF(LTRIM(RTRIM(sth_stok_kod)), '') IS NOT NULL) AS Inventory,
    (SELECT COUNT_BIG(1) FROM CARI_HESAP_HAREKETLERI WHERE ISNULL(cha_iptal, 0) = 0) AS CustomerTransactions,
    (SELECT COUNT_BIG(1) FROM STOK_HAREKETLERI WHERE ISNULL(sth_iptal, 0) = 0) AS StockTransactions;";

        var counts = await QuerySingleAsync<BootstrapRecordCounts>(
            sql,
            new { firmNo, warehouseNo },
            ct).ConfigureAwait(false);
        _logger.LogInformation(
            "Read bootstrap row counts for firmNo={FirmNo}, warehouseNo={WarehouseNo}: total={Total}.",
            firmNo,
            warehouseNo,
            counts.Customers + counts.CustomerAddresses + counts.CustomerContacts + counts.Stocks
                + counts.Barcodes + counts.OpenOrders + counts.CashAndBank + counts.Lookups
                + counts.Prices + counts.SalesConditions + counts.Inventory + counts.CustomerTransactions
                + counts.StockTransactions);
        return counts;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CustomerPayload>> ReadCustomersAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        const string sql = @"
SELECT
    CAST(ISNULL(cari_kod, '') AS NVARCHAR(50))       AS CustomerCode,
    CAST(ISNULL(cari_unvan1, '') AS NVARCHAR(200))    AS Title1,
    CAST(cari_unvan2 AS NVARCHAR(200))                AS Title2,
    CAST(cari_vdaire_adi AS NVARCHAR(100))            AS TaxOffice,
    CAST(cari_vdaire_no AS NVARCHAR(20))              AS TaxNo,
    CAST(cari_grup_kodu AS NVARCHAR(50))              AS GroupCode,
    CAST(cari_bolge_kodu AS NVARCHAR(50))             AS RegionCode,
    CAST(cari_temsilci_kodu AS NVARCHAR(50))          AS SalespersonCode,
    CAST(cari_doviz_cinsi AS NVARCHAR(10))            AS Currency,
    CAST(cari_VarsayilanCikisDepo AS NVARCHAR(50))    AS DefaultWarehouseCode,
    CAST(ISNULL(cari_cari_kilitli_flg, 0) AS BIT)     AS IsLocked,
    CAST(ISNULL(cari_efatura_fl, 0) AS BIT)           AS IsEInvoiceEnabled,
    CAST(cari_CepTel AS NVARCHAR(50))                 AS Phone,
    CAST(cari_EMail AS NVARCHAR(200))                 AS Email,
    CAST(ISNULL(ledger.Balance, 0) AS DECIMAL(18,6))   AS Balance
FROM CARI_HESAPLAR
LEFT JOIN (
    -- The official cari balance is determined by accounting direction, not
    -- invoice/receipt labels.  `cha_tip=0` is debit, `cha_tip=1` is credit.
    SELECT cha_kod,
           SUM(CASE WHEN ISNULL(cha_tip, 0) = 0
                    THEN ISNULL(cha_meblag, 0)
                    ELSE -ISNULL(cha_meblag, 0) END) AS Balance
    FROM CARI_HESAP_HAREKETLERI
    WHERE ISNULL(cha_iptal, 0) = 0
    GROUP BY cha_kod
) AS ledger ON ledger.cha_kod = cari_kod
WHERE ISNULL(cari_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(cari_lastup_date, cari_create_date) > @changedSinceUtc)";

        var rows = await QueryAsync<CustomerRow>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false);
        // Drop the Addresses/Contacts fields the constructor will initialise to null —
        // supply proper empty lists after Dapper hydrates the scalar columns.
        var result = rows
            .Select(c => new CustomerPayload(
                c.CustomerCode,
                c.Title1,
                c.Title2,
                c.TaxOffice,
                c.TaxNo,
                c.GroupCode,
                c.RegionCode,
                c.SalespersonCode,
                c.Currency,
                c.DefaultWarehouseCode,
                c.IsLocked,
                c.IsEInvoiceEnabled,
                c.Phone,
                c.Email,
                Array.Empty<CustomerAddressPayload>(),
                Array.Empty<CustomerContactPayload>(),
                c.Balance))
            .ToList();
        _logger.LogInformation("Read {Count} customers for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CustomerAddressPayload>> ReadCustomerAddressesAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        const string sql = @"
SELECT CAST(ISNULL(adr_cari_kod, '') AS NVARCHAR(50)) AS CustomerCode,
       CAST(ISNULL(adr_adres_no, 0) AS INT) AS AddressNo,
       CAST(adr_il AS NVARCHAR(100)) AS City,
       CAST(adr_ilce AS NVARCHAR(100)) AS District,
       CAST(LTRIM(RTRIM(ISNULL(adr_cadde, '') + ' ' + ISNULL(adr_mahalle, '') + ' ' +
            ISNULL(adr_sokak, '') + ' ' + ISNULL(adr_Semt, '') + ' ' +
            ISNULL(adr_Apt_No, '') + ' ' + ISNULL(adr_Daire_No, ''))) AS NVARCHAR(500)) AS Street,
       CAST(adr_posta_kodu AS NVARCHAR(20)) AS PostalCode,
       CAST(adr_gps_enlem AS FLOAT) AS Latitude,
       CAST(adr_gps_boylam AS FLOAT) AS Longitude,
       CAST(adr_temsilci_kodu AS NVARCHAR(50)) AS SalespersonCode,
       CAST(ISNULL(adr_aprint_fl, 0) AS BIT) AS IsPrintable,
       CAST(adr_cadde AS NVARCHAR(127)) AS Avenue,
       CAST(adr_mahalle AS NVARCHAR(127)) AS Neighborhood,
       CAST(adr_sokak AS NVARCHAR(127)) AS StreetName,
       CAST(adr_Semt AS NVARCHAR(50)) AS Quarter,
       CAST(adr_Apt_No AS NVARCHAR(20)) AS ApartmentNo,
       CAST(adr_Daire_No AS NVARCHAR(20)) AS FlatNo,
       CAST(adr_ulke AS NVARCHAR(100)) AS Country,
       CAST(adr_tel_ulke_kodu AS NVARCHAR(10)) AS PhoneCountryCode,
       CAST(adr_tel_bolge_kodu AS NVARCHAR(10)) AS PhoneAreaCode,
       CAST(adr_tel_no1 AS NVARCHAR(30)) AS PhoneNo,
       CAST(adr_ziyaretperyodu AS INT) AS VisitPeriod,
       CAST(adr_ziyaretgunu AS INT) AS VisitDay,
       CAST(adr_efatura_alias AS NVARCHAR(120)) AS EInvoiceAlias,
       CAST(COALESCE(adr_lastup_date, adr_create_date) AS DATETIME) AS UpdatedAt
FROM CARI_HESAP_ADRESLERI
WHERE ISNULL(adr_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(adr_lastup_date, adr_create_date) > @changedSinceUtc)";
        var result = (await QueryAsync<CustomerAddressPayload>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false)).ToList();
        _logger.LogInformation("Read {Count} customer addresses for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CustomerContactPayload>> ReadCustomerContactsAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        const string sql = @"
SELECT CAST(ISNULL(mye_cari_kod, '') AS NVARCHAR(50)) AS CustomerCode,
       CAST(mye_isim AS NVARCHAR(100)) AS FirstName,
       CAST(mye_soyisim AS NVARCHAR(100)) AS LastName,
       CAST(mye_email_adres AS NVARCHAR(200)) AS Email,
       CAST(mye_cep_telno AS NVARCHAR(50)) AS Mobile,
       CAST(mye_tc_kimlikno AS NVARCHAR(20)) AS TcIdentityNo,
       CAST(mye_vergi_kimlikno AS NVARCHAR(20)) AS TaxNo
FROM CARI_HESAP_YETKILILERI
WHERE ISNULL(mye_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(mye_lastup_date, mye_create_date) > @changedSinceUtc)";
        var result = (await QueryAsync<CustomerContactPayload>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false)).ToList();
        _logger.LogInformation("Read {Count} customer contacts for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StockPayload>> ReadStocksAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        const string sql = @"
SELECT
    CAST(ISNULL(sto_kod, '') AS NVARCHAR(50))         AS StockCode,
    CAST(ISNULL(sto_isim, '') AS NVARCHAR(200))       AS Name,
    CAST(sto_kisa_ismi AS NVARCHAR(100))              AS ShortName,
    CAST(sto_yabanci_isim AS NVARCHAR(200))           AS ForeignName,
    CAST(sto_perakende_vergi AS INT)                  AS DefaultTaxPointer,
    CAST(ISNULL(sto_birim1_ad, '') AS NVARCHAR(20))   AS Unit1,
    CAST(sto_birim1_katsayi AS DECIMAL(18,6))         AS Unit1Factor,
    CAST(sto_birim2_ad AS NVARCHAR(20))               AS Unit2,
    CAST(sto_birim2_katsayi AS DECIMAL(18,6))         AS Unit2Factor,
    CAST(sto_birim3_ad AS NVARCHAR(20))               AS Unit3,
    CAST(sto_birim3_katsayi AS DECIMAL(18,6))         AS Unit3Factor,
    CAST(sto_anagrup_kod AS NVARCHAR(50))             AS MainGroupCode,
    CAST(sto_altgrup_kod AS NVARCHAR(50))             AS SubGroupCode,
    CAST(sto_sektor_kodu AS NVARCHAR(50))             AS SectorCode,
    CAST(sto_marka_kodu AS NVARCHAR(50))              AS BrandCode,
    CAST(NULL AS NVARCHAR(50))                        AS ModelCode,
    CAST(NULL AS NVARCHAR(50))                        AS ManufacturerCode,
    CAST(sto_yer_kod AS NVARCHAR(50))                 AS ShelfCode,
    CAST(sto_ambalaj_kodu AS NVARCHAR(50))             AS PackageCode,
    CAST(sto_kalkon_kodu AS NVARCHAR(50))              AS CartonCode,
    CAST(ISNULL(sto_bedenli_takip, 0) AS BIT)         AS BedenliTakip,
    CAST(ISNULL(sto_renkDetayli, 0) AS BIT)           AS RenkDetayli,
    CAST(NULL AS DECIMAL(18,6))                       AS StandardCost,
    CAST(NULL AS NVARCHAR(10))                        AS Currency
FROM STOKLAR
WHERE ISNULL(sto_iptal, 0) = 0
  AND ISNULL(sto_pasif_fl, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(sto_lastup_date, sto_create_date) > @changedSinceUtc)";

        var rows = await QueryAsync<StockRow>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false);
        var result = rows
            .Select(s => new StockPayload(
                s.StockCode,
                s.Name,
                s.ShortName,
                s.ForeignName,
                s.DefaultTaxPointer,
                s.Unit1,
                s.Unit1Factor,
                s.Unit2,
                s.Unit2Factor,
                s.Unit3,
                s.Unit3Factor,
                s.MainGroupCode,
                s.SubGroupCode,
                s.SectorCode,
                s.BrandCode,
                s.ModelCode,
                s.ManufacturerCode,
                s.ShelfCode,
                s.BedenliTakip,
                s.RenkDetayli,
                s.StandardCost,
                s.Currency,
                Array.Empty<BarcodePayload>(),
                s.PackageCode,
                s.CartonCode))
            .ToList();
        _logger.LogInformation("Read {Count} stocks for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BarcodePayload>> ReadBarcodesAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        const string sql = @"
SELECT CAST(ISNULL(bar_kodu, '') AS NVARCHAR(100)) AS Barcode,
       CAST(ISNULL(bar_stokkodu, '') AS NVARCHAR(50)) AS StockCode,
       CAST(bar_partikodu AS NVARCHAR(50)) AS PartCode,
       CAST(bar_lotno AS NVARCHAR(50)) AS LotNo,
       CAST(bar_serino_veya_bagkodu AS NVARCHAR(100)) AS SerialNo,
       CAST(ISNULL(bar_birimpntr, 0) AS INT) AS UnitPointer
FROM BARKOD_TANIMLARI
WHERE ISNULL(bar_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(bar_lastup_date, bar_create_date) > @changedSinceUtc)";
        var result = (await QueryAsync<BarcodePayload>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false)).ToList();
        _logger.LogInformation("Read {Count} barcodes for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OpenOrderPayload>> ReadOpenOrdersAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        const string sql = @"
SELECT
    CAST(ISNULL(sip_evrakno_seri, '') AS NVARCHAR(20)) AS Series,
    CAST(ISNULL(sip_evrakno_sira, 0) AS INT)          AS Number,
    CAST(ISNULL(sip_satirno, 0) AS INT)               AS [LineNo],
    CAST(sip_musteri_kod AS NVARCHAR(50))             AS CustomerCode,
    CAST(ISNULL(sip_stok_kod, '') AS NVARCHAR(50))    AS StockCode,
    CAST(ISNULL(sip_miktar, 0) AS DECIMAL(18,6))      AS Quantity,
    CAST(ISNULL(sip_teslim_miktar, 0) AS DECIMAL(18,6)) AS DeliveredQuantity,
    CAST(ISNULL(sip_miktar, 0) - ISNULL(sip_teslim_miktar, 0) AS DECIMAL(18,6)) AS RemainingQuantity,
    CAST(ISNULL(sip_depono, 0) AS INT)                AS WarehouseNo,
    CAST(sip_satici_kod AS NVARCHAR(50))              AS SalespersonCode,
    CAST(sip_tarih AS DATE)                           AS OrderDate,
    CAST(sip_teslim_tarih AS DATE)                    AS DeliveryDate,
    CAST(sip_tutar AS DECIMAL(18,6))                  AS TotalAmount
FROM SIPARISLER
WHERE sip_firmano = @firmNo
  AND ISNULL(sip_iptal, 0) = 0
  AND sip_kapat_fl = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(sip_lastup_date, sip_create_date) > @changedSinceUtc)";

        var rows = await QueryAsync<OpenOrderPayload>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false);
        var result = rows.ToList();
        _logger.LogInformation("Read {Count} open orders for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CashAndBankPayload>> ReadCashAndBankAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        const string sql = @"
SELECT kas_kod AS Code, kas_isim AS Name, 'cash' AS Kind, NULL AS Branch,
       NULL AS AccountNo, kas_firma_no AS FirmNo, CAST(kas_doviz_cinsi AS NVARCHAR(10)) AS Currency,
       NULL AS TcmbCode
FROM KASALAR WHERE kas_firma_no = @firmNo AND ISNULL(kas_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(kas_lastup_date, kas_create_date) > @changedSinceUtc)
UNION ALL
SELECT ban_kod, ban_ismi, 'bank', ban_sube, ban_hesapno, ban_firma_no,
       CAST(ban_doviz_cinsi AS NVARCHAR(10)), ban_TCMB_Kodu
FROM BANKALAR WHERE ban_firma_no = @firmNo AND ISNULL(ban_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(ban_lastup_date, ban_create_date) > @changedSinceUtc)";

        var rows = await QueryAsync<CashAndBankPayload>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false);
        var result = rows.ToList();
        _logger.LogInformation("Read {Count} cash+bank for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LookupPayload>> ReadLookupsAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        // Lookups span several tables; one unioned query keeps the round-trips
        // down and lets Dapper map a single uniform result shape.
        _ = firmNo; // Suppress "unused parameter" — kept for signature parity.
        const string sql = @"
SELECT 'warehouse' AS Kind, CAST(dep_no AS NVARCHAR(20)) AS Code, CAST(dep_adi AS NVARCHAR(100)) AS Name,
       CAST(NULL AS NVARCHAR(50)) AS ParentCode, CAST(NULL AS NVARCHAR(10)) AS Currency
FROM DEPOLAR
WHERE dep_firmano = @firmNo AND ISNULL(dep_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(dep_lastup_date, dep_create_date) > @changedSinceUtc)
UNION ALL
SELECT 'salesperson', CAST(cari_per_kod AS NVARCHAR(20)), CAST(ISNULL(cari_per_adi,'') + ' ' + ISNULL(cari_per_soyadi,'') AS NVARCHAR(200)),
       CAST(NULL AS NVARCHAR(50)), CAST(NULL AS NVARCHAR(10))
FROM CARI_PERSONEL_TANIMLARI
WHERE ISNULL(cari_per_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(cari_per_lastup_date, cari_per_create_date) > @changedSinceUtc)
UNION ALL
SELECT 'payment_plan', CAST(odp_no AS NVARCHAR(20)), CAST(ISNULL(odp_aratop,0) AS NVARCHAR(200)),
       CAST(NULL AS NVARCHAR(50)), CAST(NULL AS NVARCHAR(10))
FROM ODEME_PLANLARI
WHERE ISNULL(odp_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(odp_lastup_date, odp_create_date) > @changedSinceUtc)
UNION ALL
SELECT 'project', CAST(pro_kodu AS NVARCHAR(50)), CAST(pro_adi AS NVARCHAR(200)),
       CAST(NULL AS NVARCHAR(50)), CAST(NULL AS NVARCHAR(10))
FROM PROJELER
WHERE ISNULL(pro_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(pro_lastup_date, pro_create_date) > @changedSinceUtc)
UNION ALL
SELECT 'price_list', CAST(sfl_sirano AS NVARCHAR(20)), CAST(ISNULL(sfl_aciklama, '') AS NVARCHAR(200)),
       CAST(ISNULL(sfl_fiyatformul, '') AS NVARCHAR(500)), CAST(NULL AS NVARCHAR(10))
FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI
WHERE ISNULL(sfl_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(sfl_lastup_date, sfl_create_date) > @changedSinceUtc)";

        var rows = await QueryAsync<LookupPayload>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false);
        var result = rows.ToList();
        _logger.LogInformation("Read {Count} lookups.", result.Count);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PricePayload>> ReadPricesAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        const string sql = @"
SELECT
    CAST(ISNULL(sfiyat_stokkod, '') AS NVARCHAR(50)) AS StockCode,
    CAST(ISNULL(sfiyat_listesirano, 0) AS INT)       AS ListNumber,
    CAST(sfiyat_fiyati AS DECIMAL(18,6))            AS Price,
    CAST(sfiyat_doviz AS NVARCHAR(10))               AS Currency,
    CAST(NULL AS NVARCHAR(50))                       AS DiscountCode
FROM STOK_SATIS_FIYAT_LISTELERI
WHERE ISNULL(sfiyat_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(sfiyat_lastup_date, sfiyat_create_date) > @changedSinceUtc)";

        var rows = await QueryAsync<PricePayload>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false);
        var result = rows.ToList();
        _logger.LogInformation("Read {Count} price-list rows for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SalesConditionPayload>> ReadSalesConditionsAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        const string sql = @"
SELECT CAST(sat_stok_kod AS NVARCHAR(50)) AS StockCode,
       CAST(sat_cari_kod AS NVARCHAR(50)) AS CustomerCode,
       CAST(sat_depo_no AS INT) AS WarehouseNo,
       CAST(sat_odeme_plan AS INT) AS PaymentPlanNo,
       CAST(sat_basla_tarih AS DATE) AS StartDate,
       CAST(sat_bitis_tarih AS DATE) AS EndDate,
       CAST(sat_brut_fiyat AS DECIMAL(18,6)) AS GrossPrice,
       CAST(sat_doviz_cinsi AS NVARCHAR(10)) AS Currency,
       CAST(ISNULL(sat_det_isk_yuzde1, 0) AS DECIMAL(18,6)) AS Discount1,
       CAST(ISNULL(sat_det_isk_yuzde2, 0) AS DECIMAL(18,6)) AS Discount2,
       CAST(ISNULL(sat_det_isk_yuzde3, 0) AS DECIMAL(18,6)) AS Discount3,
       CAST(ISNULL(sat_det_isk_yuzde4, 0) AS DECIMAL(18,6)) AS Discount4,
       CAST(ISNULL(sat_det_isk_yuzde5, 0) AS DECIMAL(18,6)) AS Discount5,
       CAST(ISNULL(sat_det_isk_yuzde6, 0) AS DECIMAL(18,6)) AS Discount6
FROM SATIS_SARTLARI
WHERE ISNULL(sat_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(sat_lastup_date, sat_create_date) > @changedSinceUtc)";
        var rows = await QueryAsync<SalesConditionRow>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false);
        var result = rows.Select(row => new SalesConditionPayload(
            row.StockCode, row.CustomerCode, row.WarehouseNo, row.PaymentPlanNo,
            row.StartDate, row.EndDate, row.GrossPrice, row.Currency,
            new[] { row.Discount1, row.Discount2, row.Discount3, row.Discount4, row.Discount5, row.Discount6 })).ToList();
        _logger.LogInformation("Read {Count} sales conditions for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InventoryPayload>> ReadInventoryAsync(int firmNo, int warehouseNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        // Mikro's own stock-movement view contains the authoritative on-hand
        // quantity after applying the ERP's movement, cancellation, return and
        // transfer rules. Do not recalculate this value from STOK_HAREKETLERI:
        // doing so can diverge from Mikro for movement types with special rules.
        // The view is company-wide (no warehouse column), so expose its result
        // as the requested/default warehouse while preserving the exact total.
        const string sql = @"
SELECT
    CAST(ISNULL(sth_stok_kod, '') AS NVARCHAR(50))       AS StockCode,
    CAST(@warehouseNo AS INT)                            AS WarehouseNo,
    CAST(ISNULL(sth_eldeki_miktar, 0) AS DECIMAL(18,6))  AS Quantity,
    CAST(0 AS DECIMAL(18,6))                             AS ReservedQuantity,
    CAST(NULL AS DATE)                                   AS LastMovementDate
FROM dbo.STOK_HAREKETTEN_ELDEKI_MIKTAR_VIEW
WHERE NULLIF(LTRIM(RTRIM(sth_stok_kod)), '') IS NOT NULL
  AND (@changedSinceUtc IS NULL OR EXISTS (
      SELECT 1 FROM STOK_HAREKETLERI changed
      WHERE changed.sth_stok_kod = sth_stok_kod
        AND COALESCE(changed.sth_lastup_date, changed.sth_create_date, changed.sth_tarih) > @changedSinceUtc))";

        var rows = await QueryAsync<InventoryPayload>(sql, new { firmNo, warehouseNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false);
        var result = rows.ToList();
        _logger.LogInformation(
            "Read {Count} inventory rows from STOK_HAREKETTEN_ELDEKI_MIKTAR_VIEW for firmNo={FirmNo}, warehouseNo={WarehouseNo}.",
            result.Count, firmNo, warehouseNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CustomerTransactionPayload>> ReadCustomerTransactionsAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        const string sql = @"
SELECT CAST(cha_RECno AS NVARCHAR(50)) AS Id,
       CAST(cha_RECno AS NVARCHAR(50)) AS ErpRef,
       CAST('MIKRO' AS NVARCHAR(20)) AS Erp,
       CAST(ISNULL(cha_kod, '') AS NVARCHAR(50)) AS CustomerCode,
       CAST(cha_tarihi AS DATETIME) AS Date,
       CAST(ISNULL(cha_evrak_tip, 0) AS INT) AS DocumentType,
       CAST(CONCAT(NULLIF(cha_evrakno_seri, ''),
                   CASE WHEN NULLIF(cha_evrakno_seri, '') IS NULL THEN '' ELSE '-' END,
                   cha_evrakno_sira) AS NVARCHAR(50)) AS DocumentNo,
       CAST(ISNULL(cha_tip, 0) AS INT) AS Type,
       CAST(ISNULL(cha_meblag, 0) AS DECIMAL(18,6)) AS Amount,
       CAST(CASE WHEN ISNULL(cha_tip, 0) = 0 THEN 1 ELSE 0 END AS BIT) AS IsDebit,
       CAST(cha_aciklama AS NVARCHAR(500)) AS Description,
       CAST(COALESCE(cha_lastup_date, cha_create_date, cha_tarihi) AS DATETIME) AS UpdatedAt,
       CAST(cha_RECno AS INT) AS RecNo,
       CAST(CASE
            WHEN ISNULL(cha_evrak_tip, 0) = 63 AND ISNULL(cha_normal_Iade, 0) = 0 AND ISNULL(cha_tip, 0) = 0 THEN 'SATIS'
            WHEN ISNULL(cha_evrak_tip, 0) = 63 AND (ISNULL(cha_normal_Iade, 0) <> 0 OR ISNULL(cha_tip, 0) = 1) THEN 'SATIS_IADE'
            WHEN ISNULL(cha_evrak_tip, 0) = 0 AND ISNULL(cha_normal_Iade, 0) = 0 AND ISNULL(cha_tip, 0) = 1 THEN 'ALIS'
            WHEN ISNULL(cha_evrak_tip, 0) = 0 AND (ISNULL(cha_normal_Iade, 0) <> 0 OR ISNULL(cha_tip, 0) = 0) THEN 'ALIS_IADE'
            WHEN ISNULL(cha_evrak_tip, 0) = 1 AND ISNULL(cha_tip, 0) = 1 THEN 'TAHSILAT'
            WHEN ISNULL(cha_evrak_tip, 0) IN (64, 65) AND ISNULL(cha_tip, 0) = 0 THEN 'TEDIYE'
            ELSE 'HAREKET'
       END AS NVARCHAR(20)) AS TransactionType,
       CAST(ISNULL(cha_cinsi, 0) AS INT) AS Kind,
       CAST(ISNULL(cha_normal_Iade, 0) AS BIT) AS IsReturn
FROM CARI_HESAP_HAREKETLERI
WHERE ISNULL(cha_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(cha_lastup_date, cha_create_date, cha_tarihi) > @changedSinceUtc)
ORDER BY cha_RECno";

        var result = (await QueryAsync<CustomerTransactionPayload>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false)).ToList();
        _logger.LogInformation("Read {Count} customer transactions for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StockTransactionPayload>> ReadStockTransactionsAsync(int firmNo, CancellationToken ct = default, DateTimeOffset? changedSinceUtc = null)
    {
        const string sql = @"
SELECT CAST(sth_RECno AS NVARCHAR(50)) AS Id,
       CAST(sth_RECno AS NVARCHAR(50)) AS ErpRef,
       CAST('MIKRO' AS NVARCHAR(20)) AS Erp,
       CAST(ISNULL(sth_stok_kod, '') AS NVARCHAR(50)) AS StockCode,
       CAST(ISNULL(sth_stok_kod, '') AS NVARCHAR(50)) AS ProductCode,
       CAST(sth_tarih AS DATETIME) AS Date,
       CAST(CASE WHEN ISNULL(sth_normal_iade, 0) = 0 THEN ISNULL(sth_tip, 0)
                 WHEN ISNULL(sth_tip, 0) = 1 THEN 2
                 ELSE 3 END AS INT) AS Type,
       CAST(ISNULL(sth_cins, 0) AS INT) AS Kind,
       CAST(ISNULL(sth_evraktip, 0) AS INT) AS DocumentType,
       CAST(CONCAT(NULLIF(sth_evrakno_seri, ''),
                   CASE WHEN NULLIF(sth_evrakno_seri, '') IS NULL THEN '' ELSE '-' END,
                   sth_evrakno_sira) AS NVARCHAR(50)) AS DocumentNo,
       CAST(CASE WHEN ISNULL(sth_tip, 0) = 0 THEN ISNULL(sth_miktar, 0) ELSE 0 END AS DECIMAL(18,6)) AS InQuantity,
       CAST(CASE WHEN ISNULL(sth_tip, 0) = 1 THEN ISNULL(sth_miktar, 0) ELSE 0 END AS DECIMAL(18,6)) AS OutQuantity,
       CAST(CASE WHEN ISNULL(sth_tip, 0) = 0 THEN ISNULL(sth_miktar, 0) ELSE -ISNULL(sth_miktar, 0) END AS DECIMAL(18,6)) AS SignedQuantity,
       CAST(CASE WHEN ISNULL(sth_miktar, 0) = 0 THEN 0 ELSE ISNULL(sth_tutar, 0) / sth_miktar END AS DECIMAL(18,6)) AS UnitPrice,
       CAST(ISNULL(sth_tutar, 0) AS DECIMAL(18,6)) AS Amount,
       CAST(sth_cari_kodu AS NVARCHAR(50)) AS CustomerCode,
       CAST(sth_giris_depo_no AS INT) AS InWarehouseNo,
       CAST(sth_cikis_depo_no AS INT) AS OutWarehouseNo,
       CAST(sth_aciklama AS NVARCHAR(500)) AS Description,
       CAST(COALESCE(sth_lastup_date, sth_create_date, sth_tarih) AS DATETIME) AS UpdatedAt,
       CAST(sth_fat_recid_recno AS INT) AS InvoiceRecNo
FROM STOK_HAREKETLERI
WHERE ISNULL(sth_iptal, 0) = 0
  AND (@changedSinceUtc IS NULL OR COALESCE(sth_lastup_date, sth_create_date, sth_tarih) > @changedSinceUtc)
ORDER BY sth_RECno";

        var result = (await QueryAsync<StockTransactionPayload>(sql, new { firmNo, changedSinceUtc = MikroDateTime(changedSinceUtc) }, ct).ConfigureAwait(false)).ToList();
        _logger.LogInformation("Read {Count} stock transactions for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <summary>
    /// Open a fresh connection, run <paramref name="sql"/> with
    /// <paramref name="parameters"/>, and materialise the rows. Errors are
    /// logged at <c>Error</c> and rethrown so the bootstrap pipeline surfaces
    /// them as a failed cycle.
    /// </summary>
    private async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object parameters,
        CancellationToken ct)
    {
        var connectionString = _factory.BuildConnectionStringFromActive();

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        try
        {
            // Hard cap the per-query command timeout so a single hanging SQL
            // statement can't stall the whole bootstrap (the network-level
            // Connect Timeout=30 in the connection string governs the initial
            // handshake; this one fires once the command is executing on the
            // server). 60s is plenty for a typical Mikro reference-data
            // query and short enough that a deadlock surfaces quickly.
            var rows = await conn.QueryAsync<T>(new CommandDefinition(
                commandText: sql,
                parameters: parameters,
                commandTimeout: 60,
                cancellationToken: ct)).ConfigureAwait(false);
            // Guard against null payloads from Dapper (defensive — empty result
            // typically materialises as an empty sequence, not null).
            return rows?.AsList() ?? new List<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MikroDbReader failed: {Message}", ex.Message);
            throw;
        }
    }

    private async Task<T> QuerySingleAsync<T>(string sql, object parameters, CancellationToken ct)
    {
        var connectionString = _factory.BuildConnectionStringFromActive();
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        try
        {
            return await conn.QuerySingleAsync<T>(new CommandDefinition(
                commandText: sql,
                parameters: parameters,
                commandTimeout: 30,
                cancellationToken: ct)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MikroDbReader scalar query failed: {Message}", ex.Message);
            throw;
        }
    }

    private sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value)
        {
            return value switch
            {
                DateOnly dateOnly => dateOnly,
                DateTime dateTime => DateOnly.FromDateTime(dateTime),
                string text when DateTime.TryParse(text, out var parsed) => DateOnly.FromDateTime(parsed),
                _ => throw new DataException($"Cannot convert {value.GetType().Name} to DateOnly."),
            };
        }

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
            parameter.DbType = DbType.Date;
        }
    }

    private sealed record CustomerRow(
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
        decimal Balance);

    private sealed record StockRow(
        string StockCode,
        string Name,
        string? ShortName,
        string? ForeignName,
        int? DefaultTaxPointer,
        string? Unit1,
        decimal? Unit1Factor,
        string? Unit2,
        decimal? Unit2Factor,
        string? Unit3,
        decimal? Unit3Factor,
        string? MainGroupCode,
        string? SubGroupCode,
        string? SectorCode,
        string? BrandCode,
        string? ModelCode,
        string? ManufacturerCode,
        string? ShelfCode,
        string? PackageCode,
        string? CartonCode,
        bool BedenliTakip,
        bool RenkDetayli,
        decimal? StandardCost,
        string? Currency);

    private sealed record SalesConditionRow(
        string? StockCode,
        string? CustomerCode,
        int? WarehouseNo,
        int? PaymentPlanNo,
        DateOnly? StartDate,
        DateOnly? EndDate,
        decimal? GrossPrice,
        string? Currency,
        decimal Discount1,
        decimal Discount2,
        decimal Discount3,
        decimal Discount4,
        decimal Discount5,
        decimal Discount6);
}
