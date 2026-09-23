using Dapper;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Writers.Session;

/// <summary>A customer card as a document needs it.</summary>
/// <param name="Code"><c>cari_kod</c>.</param>
/// <param name="Title"><c>cari_unvan1</c>.</param>
/// <param name="TaxOffice"><c>cari_vdaire_adi</c>.</param>
/// <param name="TaxNumber"><c>cari_vdaire_no</c>.</param>
public sealed record MikroCustomer(string Code, string Title, string TaxOffice, string TaxNumber);

/// <summary>A stock card as a line needs it.</summary>
/// <param name="Code"><c>sto_kod</c>.</param>
/// <param name="VatPointer"><c>sto_toptan_vergi</c>.</param>
/// <param name="VatRate"><c>fn_VergiYuzde(sto_toptan_vergi)</c> in percent.</param>
public sealed record MikroStock(string Code, byte VatPointer, decimal VatRate);

/// <summary>What a document does with a customer, for the card's <c>cari_hareket_tipi</c>.</summary>
public enum MikroCustomerUse
{
    Sale,
    SaleReturn,
    Collection,

    /// <summary>Ondan mal alıyoruz (alış faturası).</summary>
    Purchase,
}

/// <summary>What a kasa is for (<c>kas_tip</c>): money, customer cheques, customer notes.</summary>
public enum MikroCashBoxKind : byte
{
    Cash = 0,
    Cheque = 1,
    Note = 3,
}

/// <summary>
/// Checks inside the write session that what a document names exists in Mikro and may be used, the
/// way Fora checks before it writes (goal ERP yazım Y3b). A refusal is a <see cref="MikroWriteException"/>
/// with a permanent Turkish reason. Reads only; reads are cached for the session.
/// </summary>
public sealed class MikroDocumentLookup(MikroWriteSession session)
{
    private readonly Dictionary<string, MikroStock> _stocks = new(StringComparer.Ordinal);
    private readonly Dictionary<byte, decimal> _vatRates = [];

    private MikroWriteSession Session { get; } = session ?? throw new ArgumentNullException(nameof(session));

    /// <summary>
    /// The customer, when the card allows the use: <c>cari_hareket_tipi</c> 1 sells only, 2 buys only,
    /// 3 money only, 4 nothing (Fora <c>IsCariSecilebilir</c>); an order also refuses a card locked for orders.
    /// </summary>
    public async Task<MikroCustomer> CustomerAsync(string code, MikroCustomerUse use, bool isOrder = false, CancellationToken ct = default)
    {
        var row = await Session.Connection.QuerySingleOrDefaultAsync<(string Code, string? Title, string? TaxOffice, string? TaxNumber, byte? MovementType, bool? OrderLocked)>(
            new CommandDefinition(@"
SELECT cari_kod, cari_unvan1, cari_vdaire_adi, cari_vdaire_no, cari_hareket_tipi, cari_cari_kilitli_flg
FROM CARI_HESAPLAR WHERE cari_kod = @code", new { code }, Session.Transaction, cancellationToken: ct)).ConfigureAwait(false);
        if (row.Code is null) throw new MikroWriteException(ErpWriteError.CustomerNotFound(code));

        if (!CustomerAllows(row.MovementType ?? 0, use, isOrder, row.OrderLocked == true)) throw new MikroWriteException(ErpWriteError.CustomerLocked(code));
        return new MikroCustomer(row.Code, row.Title ?? string.Empty, row.TaxOffice ?? string.Empty, row.TaxNumber ?? string.Empty);
    }

    /// <summary>
    /// <c>cari_hareket_tipi</c>: 0 buys and sells, 1 sells only (no sales return, which is a purchase
    /// document), 2 buys only, 3 money only, 4 nothing. An order also needs the card open for orders.
    /// </summary>
    internal static bool CustomerAllows(byte movementType, MikroCustomerUse use, bool isOrder, bool orderLocked) =>
        !(isOrder && orderLocked) && movementType switch
        {
            0 => true,
            // Yalnız satış yapılan bir kart bize mal satamaz; alış da satış iadesi gibi bir alış belgesidir.
            1 => use is not (MikroCustomerUse.SaleReturn or MikroCustomerUse.Purchase),
            // Yalnız alış yapılan kart: ondan mal alınır, parası ödenir.
            2 => use is MikroCustomerUse.Collection or MikroCustomerUse.Purchase,
            3 => use == MikroCustomerUse.Collection,
            _ => false,
        };

    /// <summary>The stock card with its VAT; a sale also refuses a passive card or one closed to sales (<c>sto_satis_dursun</c>).</summary>
    public async Task<MikroStock> StockAsync(string code, bool forSale, CancellationToken ct = default)
    {
        var key = (forSale ? "S|" : "R|") + code;
        if (_stocks.TryGetValue(key, out var cached)) return cached;

        var row = await Session.Connection.QuerySingleOrDefaultAsync<(string Code, byte? VatPointer, byte? SaleBlocked, bool? Passive)>(
            new CommandDefinition(@"
SELECT sto_kod, sto_toptan_vergi, sto_satis_dursun, sto_pasif_fl
FROM STOKLAR WHERE sto_kod = @code", new { code }, Session.Transaction, cancellationToken: ct)).ConfigureAwait(false);
        if (row.Code is null) throw new MikroWriteException(ErpWriteError.StockNotFound(code));
        if (forSale && (row.SaleBlocked is > 0 || row.Passive == true)) throw new MikroWriteException(ErpWriteError.StockNotSaleable(code));

        var pointer = row.VatPointer ?? 0;
        var stock = new MikroStock(row.Code, pointer, await VatRateAsync(pointer, ct).ConfigureAwait(false));
        _stocks[key] = stock;
        return stock;
    }

    /// <summary>The rate of a VAT pointer in percent, from Mikro's own function.</summary>
    public async Task<decimal> VatRateAsync(byte pointer, CancellationToken ct = default)
    {
        if (_vatRates.TryGetValue(pointer, out var rate)) return rate;
        rate = Convert.ToDecimal(await Session.Connection.ExecuteScalarAsync<double>(
            new CommandDefinition("SELECT CAST(dbo.fn_VergiYuzde(@pointer) AS float)", new { pointer }, Session.Transaction, cancellationToken: ct)).ConfigureAwait(false));
        _vatRates[pointer] = rate;
        return rate;
    }

    public async Task EnsureWarehouseAsync(int warehouseNo, CancellationToken ct = default)
    {
        if (!await ExistsAsync("SELECT 1 FROM DEPOLAR WHERE dep_no = @warehouseNo", new { warehouseNo }, ct).ConfigureAwait(false))
            throw new MikroWriteException(ErpWriteError.WarehouseNotFound(warehouseNo));
    }

    /// <summary>A kasa of the given kind: money for cash, the cheque or note portfolio for cheques and notes.</summary>
    public async Task EnsureCashBoxAsync(string code, MikroCashBoxKind kind, CancellationToken ct = default)
    {
        if (!await ExistsAsync("SELECT 1 FROM KASALAR WHERE kas_kod = @code AND kas_tip = @kind", new { code, kind = (byte)kind }, ct).ConfigureAwait(false))
            throw new MikroWriteException(ErpWriteError.CashAccountNotFound(code));
    }

    public async Task EnsureBankAsync(string code, CancellationToken ct = default)
    {
        if (!await ExistsAsync("SELECT 1 FROM BANKALAR WHERE ban_kod = @code", new { code }, ct).ConfigureAwait(false))
            throw new MikroWriteException(ErpWriteError.BankAccountNotFound(code));
    }

    /// <summary>
    /// Gider kartı (<c>MASRAF_HESAPLARI.his_kod</c>) — referans §13. Kart yoksa gider yazılmaz:
    /// olmayan bir karta yazılan masraf fişi muhasebede sahipsiz kalır.
    /// </summary>
    public async Task EnsureExpenseCardAsync(string code, CancellationToken ct = default)
    {
        if (!await ExistsAsync("SELECT 1 FROM MASRAF_HESAPLARI WHERE his_kod = @code", new { code }, ct).ConfigureAwait(false))
            throw new MikroWriteException(ErpWriteError.ExpenseCardNotFound(code));
    }

    /// <summary>
    /// A VAT pointer that carries a rate in this company's Mikro (<c>fn_VergiYuzde</c>). An expense's VAT is
    /// booked into the pointer's own <c>cha_vergiN</c> column, so a pointer with no rate would put a VAT amount
    /// where Mikro's own reports read "no VAT".
    /// </summary>
    public async Task EnsureVatPointerAsync(byte pointer, CancellationToken ct = default)
    {
        if (pointer is < 1 or > MikroCodes.VatPointerMax
            || !await ExistsAsync("SELECT 1 WHERE dbo.fn_VergiYuzde(@pointer) > 0", new { pointer }, ct).ConfigureAwait(false))
            throw new MikroWriteException(ErpWriteError.VatRateNotFound(pointer));
    }

    /// <summary>
    /// The series this supplier's purchase invoices already use (referans §15, K7). Mikro has no series
    /// definition table: an alış faturası carries the <b>supplier's own</b> invoice series, so "the series
    /// defined in the ERP" is the one the data already shows for that supplier. The newest one wins; a
    /// supplier with no history gets the series-less sequence (empty string), which is what Mikro's own
    /// rows do for 373 of this company's purchases.
    /// </summary>
    public async Task<string> PurchaseSeriesAsync(string supplierCode, CancellationToken ct = default)
    {
        var series = await Session.Connection.QuerySingleOrDefaultAsync<string?>(new CommandDefinition(
            """
            SELECT TOP 1 cha_evrakno_seri
            FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK)
            WHERE cha_evrak_tip = 0 AND cha_cari_cins = 0 AND cha_kod = @supplierCode AND LEN(cha_evrakno_seri) > 0
            ORDER BY cha_RECno DESC
            """,
            new { supplierCode }, Session.Transaction, cancellationToken: ct)).ConfigureAwait(false);
        return series ?? string.Empty;
    }

    /// <summary>A salesperson named on the document; none named is fine.</summary>
    public async Task EnsureSalespersonAsync(string? code, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(code)) return;
        if (!await ExistsAsync("SELECT 1 FROM CARI_PERSONEL_TANIMLARI WHERE cari_per_kod = @code", new { code }, ct).ConfigureAwait(false))
            throw new MikroWriteException(ErpWriteError.SalespersonNotFound(code));
    }

    /// <summary>Whether the price list's prices include VAT (<c>sfl_kdvdahil</c>).</summary>
    public async Task<bool> PriceListIncludesVatAsync(int priceListNo, CancellationToken ct = default)
    {
        var includesVat = await Session.Connection.QuerySingleOrDefaultAsync<bool?>(new CommandDefinition(
            "SELECT CAST(ISNULL(sfl_kdvdahil, 0) AS bit) FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI WHERE sfl_sirano = @priceListNo",
            new { priceListNo }, Session.Transaction, cancellationToken: ct)).ConfigureAwait(false);
        return includesVat ?? throw new MikroWriteException(ErpWriteError.PriceListNotFound(priceListNo));
    }

    private async Task<bool> ExistsAsync(string sql, object parameters, CancellationToken ct) =>
        await Session.Connection.ExecuteScalarAsync<int?>(new CommandDefinition(sql, parameters, Session.Transaction, cancellationToken: ct)).ConfigureAwait(false) is not null;
}
