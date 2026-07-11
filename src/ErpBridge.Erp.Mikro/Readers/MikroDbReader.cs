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

    /// <inheritdoc />
    public async Task<IReadOnlyList<CustomerPayload>> ReadCustomersAsync(int firmNo, CancellationToken ct = default)
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
    CAST(cari_EMail AS NVARCHAR(200))                 AS Email
FROM CARI_HESAPLAR
WHERE ISNULL(cari_iptal, 0) = 0";

        var rows = await QueryAsync<CustomerRow>(sql, new { firmNo }, ct).ConfigureAwait(false);
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
                Array.Empty<CustomerContactPayload>()))
            .ToList();
        _logger.LogInformation("Read {Count} customers for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StockPayload>> ReadStocksAsync(int firmNo, CancellationToken ct = default)
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
    CAST(ISNULL(sto_bedenli_takip, 0) AS BIT)         AS BedenliTakip,
    CAST(ISNULL(sto_renkDetayli, 0) AS BIT)           AS RenkDetayli,
    CAST(NULL AS DECIMAL(18,6))                       AS StandardCost,
    CAST(NULL AS NVARCHAR(10))                        AS Currency
FROM STOKLAR
WHERE ISNULL(sto_iptal, 0) = 0
  AND ISNULL(sto_pasif_fl, 0) = 0";

        var rows = await QueryAsync<StockRow>(sql, new { firmNo }, ct).ConfigureAwait(false);
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
                Array.Empty<BarcodePayload>()))
            .ToList();
        _logger.LogInformation("Read {Count} stocks for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OpenOrderPayload>> ReadOpenOrdersAsync(int firmNo, CancellationToken ct = default)
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
  AND sip_kapat_fl = 0";

        var rows = await QueryAsync<OpenOrderPayload>(sql, new { firmNo }, ct).ConfigureAwait(false);
        var result = rows.ToList();
        _logger.LogInformation("Read {Count} open orders for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CashAndBankPayload>> ReadCashAndBankAsync(int firmNo, CancellationToken ct = default)
    {
        const string sql = @"
SELECT kas_kod AS Code, kas_isim AS Name, 'cash' AS Kind, NULL AS Branch,
       NULL AS AccountNo, kas_firma_no AS FirmNo, CAST(kas_doviz_cinsi AS NVARCHAR(10)) AS Currency,
       NULL AS TcmbCode
FROM KASALAR WHERE kas_firma_no = @firmNo AND ISNULL(kas_iptal, 0) = 0
UNION ALL
SELECT ban_kod, ban_ismi, 'bank', ban_sube, ban_hesapno, ban_firma_no,
       CAST(ban_doviz_cinsi AS NVARCHAR(10)), ban_TCMB_Kodu
FROM BANKALAR WHERE ban_firma_no = @firmNo AND ISNULL(ban_iptal, 0) = 0";

        var rows = await QueryAsync<CashAndBankPayload>(sql, new { firmNo }, ct).ConfigureAwait(false);
        var result = rows.ToList();
        _logger.LogInformation("Read {Count} cash+bank for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LookupPayload>> ReadLookupsAsync(int firmNo, CancellationToken ct = default)
    {
        // Lookups span several tables; one unioned query keeps the round-trips
        // down and lets Dapper map a single uniform result shape.
        _ = firmNo; // Suppress "unused parameter" — kept for signature parity.
        const string sql = @"
SELECT 'warehouse' AS Kind, CAST(dep_no AS NVARCHAR(20)) AS Code, CAST(dep_adi AS NVARCHAR(100)) AS Name,
       CAST(NULL AS NVARCHAR(50)) AS ParentCode, CAST(NULL AS NVARCHAR(10)) AS Currency
FROM DEPOLAR
WHERE dep_firmano = @firmNo AND ISNULL(dep_iptal, 0) = 0
UNION ALL
SELECT 'salesperson', CAST(cari_per_kod AS NVARCHAR(20)), CAST(ISNULL(cari_per_adi,'') + ' ' + ISNULL(cari_per_soyadi,'') AS NVARCHAR(200)),
       CAST(NULL AS NVARCHAR(50)), CAST(NULL AS NVARCHAR(10))
FROM CARI_PERSONEL_TANIMLARI
WHERE ISNULL(cari_per_iptal, 0) = 0
UNION ALL
SELECT 'payment_plan', CAST(odp_no AS NVARCHAR(20)), CAST(ISNULL(odp_aratop,0) AS NVARCHAR(200)),
       CAST(NULL AS NVARCHAR(50)), CAST(NULL AS NVARCHAR(10))
FROM ODEME_PLANLARI
WHERE ISNULL(odp_iptal, 0) = 0
UNION ALL
SELECT 'project', CAST(pro_kodu AS NVARCHAR(50)), CAST(pro_adi AS NVARCHAR(200)),
       CAST(NULL AS NVARCHAR(50)), CAST(NULL AS NVARCHAR(10))
FROM PROJELER
WHERE ISNULL(pro_iptal, 0) = 0";

        var rows = await QueryAsync<LookupPayload>(sql, new { firmNo }, ct).ConfigureAwait(false);
        var result = rows.ToList();
        _logger.LogInformation("Read {Count} lookups.", result.Count);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PricePayload>> ReadPricesAsync(int firmNo, CancellationToken ct = default)
    {
        const string sql = @"
SELECT
    CAST(ISNULL(sfiyat_stokkod, '') AS NVARCHAR(50)) AS StockCode,
    CAST(ISNULL(sfiyat_listesirano, 0) AS INT)       AS ListNumber,
    CAST(sfiyat_fiyati AS DECIMAL(18,6))            AS Price,
    CAST(sfiyat_doviz AS NVARCHAR(10))               AS Currency,
    CAST(NULL AS NVARCHAR(50))                       AS DiscountCode
FROM STOK_SATIS_FIYAT_LISTELERI
WHERE ISNULL(sfiyat_iptal, 0) = 0";

        var rows = await QueryAsync<PricePayload>(sql, new { firmNo }, ct).ConfigureAwait(false);
        var result = rows.ToList();
        _logger.LogInformation("Read {Count} price-list rows for firmNo={FirmNo}.", result.Count, firmNo);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InventoryPayload>> ReadInventoryAsync(int firmNo, int warehouseNo, CancellationToken ct = default)
    {
        // Tips 1..3 are inflow, tips >= 4 are outflow — negate them so SUM nets
        // to the current on-hand quantity.
        const string sql = @"
SELECT
    CAST(ISNULL(sth_stok_kod, '') AS NVARCHAR(50))   AS StockCode,
    CAST(ISNULL(sth_cikis_depo_no, 0) AS INT)        AS WarehouseNo,
    CAST(SUM(CASE WHEN ISNULL(sth_tip, 0) < 4
                  THEN ISNULL(sth_miktar, 0)
                  ELSE -ISNULL(sth_miktar, 0) END) AS DECIMAL(18,6)) AS Quantity,
    CAST(0 AS DECIMAL(18,6))                         AS ReservedQuantity,
    CAST(MAX(sth_tarih) AS DATE)                     AS LastMovementDate
FROM STOK_HAREKETLERI
WHERE sth_firmano = @firmNo
  AND sth_cikis_depo_no = @warehouseNo
GROUP BY sth_stok_kod, sth_cikis_depo_no";

        var rows = await QueryAsync<InventoryPayload>(sql, new { firmNo, warehouseNo }, ct).ConfigureAwait(false);
        var result = rows.ToList();
        _logger.LogInformation(
            "Read {Count} inventory rows for firmNo={FirmNo}, warehouseNo={WarehouseNo}.",
            result.Count, firmNo, warehouseNo);
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
        string? Email);

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
        bool BedenliTakip,
        bool RenkDetayli,
        decimal? StandardCost,
        string? Currency);
}
