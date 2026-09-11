using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Sync;

/// <summary>
/// Turns the ERP-shaped rows in <see cref="MobileRecord"/> into the shapes the
/// Android client already stores.
///
/// <para>The two do not line up one-to-one, and that is deliberate. The app's
/// product row is denormalised — one row carries the stock card, its barcodes,
/// its best price, its per-list prices and its stock per warehouse — while the
/// ERP keeps those in five separate tables. Something has to join them.</para>
///
/// <para>That join stays on the server. Streaming the raw sections instead would
/// move it into the client, which would need new local tables for prices and
/// price-list names plus the logic to recompute a product whenever any part of
/// it changed. The server already does exactly this join in
/// <c>AndroidEndpoints.ProductCatalogAsync</c> — the difference here is that it
/// runs for the handful of records a delta actually touched rather than for the
/// whole catalogue on every sync.</para>
/// </summary>
public static class MobileEntityAssembler
{
    /// <summary>Mobile entity name for the product catalogue, as the client knows it.</summary>
    public const string ProductEntity = "urun";

    /// <summary>Mobile entity name for customers, as the client knows it.</summary>
    public const string CustomerEntity = "cari";

    /// <summary>
    /// Which client records a change to <paramref name="record"/> affects.
    ///
    /// <para>Usually one, but not always. A barcode, price or inventory row is not
    /// something the client stores on its own, so it resolves to the product it
    /// belongs to. A cash register is stored twice — once as a definition, once
    /// for accounting — so one ERP row produces two records.</para>
    ///
    /// <para>Yields nothing for sections the client does not read — their
    /// positions still pass under the cursor, which is why adding an entity later
    /// has to raise <see cref="SyncCursor.FormatVersion"/> so devices resync
    /// rather than silently miss the history.</para>
    /// </summary>
    public static IEnumerable<Target> Affects(MobileRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        switch (record.Entity.ToLowerInvariant())
        {
            case "stocks" or "barcodes" or "prices" or "inventory":
                if (!string.IsNullOrWhiteSpace(record.StockKey))
                    yield return new Target(ProductEntity, record.StockKey!, BuildKind.Product);
                break;

            // Contacts are folded into the customer card by the app, so they only
            // ever surface as a change to that card.
            case "customers" or "customercontacts":
                if (!string.IsNullOrWhiteSpace(record.CustomerKey))
                    yield return new Target(CustomerEntity, record.CustomerKey!, BuildKind.Customer);
                break;

            case "customeraddresses":
                yield return new Target(Entities.CustomerAddress, record.RecordKey, BuildKind.Direct);
                break;

            case "customertransactions":
                yield return new Target(Entities.CustomerMovement, record.RecordKey, BuildKind.Direct);
                break;

            case "stocktransactions":
                yield return new Target(Entities.StockMovement, record.RecordKey, BuildKind.Direct);
                break;

            case "cashandbank":
            {
                // The kind travels in the record key as well as the payload, so a
                // tombstone — which has no payload — can still be routed to the
                // right client entity.
                var (kind, code) = SplitKeyed(record.RecordKey);
                if (code.Length == 0) break;
                if (string.Equals(kind, "bank", StringComparison.OrdinalIgnoreCase))
                {
                    yield return new Target(Entities.Bank, code, BuildKind.Direct);
                }
                else if (string.Equals(kind, "cash", StringComparison.OrdinalIgnoreCase))
                {
                    // The app keeps cash registers twice: once as definitions and
                    // once for accounting. One ERP row, two client records.
                    yield return new Target(Entities.CashRegister, code, BuildKind.Direct);
                    yield return new Target(Entities.CashManagement, code, BuildKind.Direct);
                }
                break;
            }

            case "lookups":
            {
                var (kind, code) = SplitKeyed(record.RecordKey);
                if (code.Length > 0 && string.Equals(kind, "price_list", StringComparison.OrdinalIgnoreCase))
                    yield return new Target(Entities.PriceListDefinition, code, BuildKind.Direct);
                break;
            }
        }
    }

    /// <summary>Mobile entity names the feed produces, as the client knows them.</summary>
    public static class Entities
    {
        /// <summary>Customer addresses.</summary>
        public const string CustomerAddress = "cariAdresleri";

        /// <summary>Bank definitions.</summary>
        public const string Bank = "bankalar";

        /// <summary>Cash-register definitions.</summary>
        public const string CashRegister = "kasalar";

        /// <summary>Accounting view of the same rows as <see cref="CashRegister"/>.</summary>
        public const string CashManagement = "kasaYonetim";

        /// <summary>Price-list definitions.</summary>
        public const string PriceListDefinition = "fiyatTanim";

        /// <summary>Customer ledger movements.</summary>
        public const string CustomerMovement = "cariHareketleri";

        /// <summary>Stock movements.</summary>
        public const string StockMovement = "stokHareketleri";
    }

    /// <summary>How a target record is produced from the ERP rows behind it.</summary>
    public enum BuildKind
    {
        /// <summary>Mapped from the single row that changed.</summary>
        Direct = 0,

        /// <summary>Joined from a stock card and its barcodes, prices and inventory.</summary>
        Product = 1,

        /// <summary>Read from the customer card alone.</summary>
        Customer = 2,
    }

    /// <summary>A record the client stores, and how to rebuild it.</summary>
    /// <param name="Entity">Entity name the client knows.</param>
    /// <param name="Key">Identity within that entity.</param>
    /// <param name="Kind">How it is produced.</param>
    public readonly record struct Target(string Entity, string Key, BuildKind Kind);

    /// <summary>
    /// Builds a record that needs nothing but the row that changed.
    ///
    /// <para>Movements pass straight through: the app stores them exactly as the
    /// ERP sends them. The rest are renamed into the Turkish field names the app's
    /// Room tables use — handing over the raw payload there would silently leave
    /// cariKod, il and sokak empty.</para>
    /// </summary>
    /// <param name="target">What is being built.</param>
    /// <param name="record">The row that changed.</param>
    public static object? BuildDirect(Target target, MobileRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (string.IsNullOrWhiteSpace(record.PayloadJson)) return null;

        using var document = JsonDocument.Parse(record.PayloadJson);
        var item = document.RootElement;

        return target.Entity switch
        {
            Entities.CustomerMovement or Entities.StockMovement => item.Clone(),
            Entities.CustomerAddress => BuildAddress(item),
            Entities.Bank => BuildCashOrBank(item, isCash: false),
            Entities.CashRegister or Entities.CashManagement => BuildCashOrBank(item, isCash: true),
            Entities.PriceListDefinition => BuildPriceListDefinition(item),
            _ => null,
        };
    }

    private static object BuildAddress(JsonElement address)
    {
        var cariKod = AndroidEndpoints.GetString(address, "customerCode") ?? string.Empty;
        var adresNo = AndroidEndpoints.GetInt32(address, "addressNo") ?? 0;
        var telUlkeKodu = AndroidEndpoints.GetString(address, "phoneCountryCode");
        var telBolgeKodu = AndroidEndpoints.GetString(address, "phoneAreaCode");
        var telNo = AndroidEndpoints.GetString(address, "phoneNo");
        var phone = string.Join(" ", new[] { telUlkeKodu, telBolgeKodu, telNo }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

        return new
        {
            erpRef = "ADR-" + cariKod + "-" + adresNo,
            erp = "MIKRO",
            cariKod,
            adresNo,
            yazdirilabilir = AndroidEndpoints.GetBoolean(address, "isPrintable") ?? false,
            cadde = AndroidEndpoints.GetString(address, "avenue"),
            mahalle = AndroidEndpoints.GetString(address, "neighborhood"),
            sokak = AndroidEndpoints.GetString(address, "streetName") ?? AndroidEndpoints.GetString(address, "street"),
            semt = AndroidEndpoints.GetString(address, "quarter"),
            ilce = AndroidEndpoints.GetString(address, "district"),
            il = AndroidEndpoints.GetString(address, "city"),
            ulke = AndroidEndpoints.GetString(address, "country"),
            postaKodu = AndroidEndpoints.GetString(address, "postalCode"),
            telUlkeKodu,
            telBolgeKodu,
            telNo1 = telNo,
            telefon = phone,
            gpsEnlem = AndroidEndpoints.GetDouble(address, "latitude"),
            gpsBoylam = AndroidEndpoints.GetDouble(address, "longitude"),
            ziyaretPeriyodu = AndroidEndpoints.GetInt32(address, "visitPeriod"),
            ziyaretGunu = AndroidEndpoints.GetInt32(address, "visitDay"),
            eFaturaAlias = AndroidEndpoints.GetString(address, "eInvoiceAlias"),
            adresSatir1 = AndroidEndpoints.JoinAddressLine(
                AndroidEndpoints.GetString(address, "avenue"),
                AndroidEndpoints.GetString(address, "neighborhood"),
                AndroidEndpoints.GetString(address, "streetName")),
            adresSatir2 = AndroidEndpoints.JoinAddressLine(
                AndroidEndpoints.GetString(address, "quarter"),
                AndroidEndpoints.GetString(address, "apartmentNo"),
                AndroidEndpoints.GetString(address, "flatNo")),
            updatedAt = AndroidEndpoints.GetString(address, "updatedAt"),
            isDeleted = false,
        };
    }

    private static object BuildCashOrBank(JsonElement item, bool isCash) => new
    {
        erpRef = AndroidEndpoints.GetString(item, "code") ?? string.Empty,
        erp = "MIKRO",
        kod = AndroidEndpoints.GetString(item, "code") ?? string.Empty,
        isim = AndroidEndpoints.GetString(item, "name") ?? string.Empty,
        sube = AndroidEndpoints.GetString(item, "branch"),
        hesapNumarasi = AndroidEndpoints.GetString(item, "accountNo"),
        tCMBKodu = AndroidEndpoints.GetString(item, "tcmbCode"),
        dovizCinsi = AndroidEndpoints.GetInt32(item, "currency"),
        muhasebeKod = (string?)null,
        tip = isCash ? 0 : 1,
        isDeleted = false,
    };

    private static object BuildPriceListDefinition(JsonElement item)
    {
        var code = AndroidEndpoints.GetString(item, "code") ?? string.Empty;
        return new
        {
            id = code,
            erpRef = code,
            listNo = int.TryParse(code, out var number) ? number : 0,
            aciklama = AndroidEndpoints.GetString(item, "name"),
            sfl_aciklama = AndroidEndpoints.GetString(item, "name"),
            sfl_fiyatformul = AndroidEndpoints.GetString(item, "parentCode"),
            isDeleted = false,
        };
    }

    /// <summary>Splits a <c>kind|code</c> record key; empty parts when it has no separator.</summary>
    private static (string Kind, string Code) SplitKeyed(string recordKey)
    {
        var separator = recordKey.IndexOf(MobileRecordKey.Separator);
        return separator <= 0 || separator == recordKey.Length - 1
            ? (string.Empty, string.Empty)
            : (recordKey[..separator], recordKey[(separator + 1)..]);
    }

    /// <summary>
    /// The parts of every product and customer named in <paramref name="keys"/>,
    /// read in two indexed queries rather than one per record.
    /// </summary>
    public static async Task<AssemblySources> LoadAsync(
        CentralApiDbContext db,
        Guid tenantId,
        IReadOnlyCollection<string> stockCodes,
        IReadOnlyCollection<string> customerCodes,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);

        var stockParts = new Dictionary<string, List<MobileRecord>>(StringComparer.OrdinalIgnoreCase);
        if (stockCodes.Count > 0)
        {
            var codes = stockCodes.ToList();
            var rows = await db.MobileRecords.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.StockKey != null && codes.Contains(x.StockKey))
                .ToListAsync(ct).ConfigureAwait(false);
            foreach (var row in rows)
            {
                if (!stockParts.TryGetValue(row.StockKey!, out var list))
                {
                    list = [];
                    stockParts[row.StockKey!] = list;
                }
                list.Add(row);
            }
        }

        var customerRows = new Dictionary<string, MobileRecord>(StringComparer.OrdinalIgnoreCase);
        if (customerCodes.Count > 0)
        {
            var codes = customerCodes.ToList();
            var rows = await db.MobileRecords.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted
                            && x.Entity == "customers" && codes.Contains(x.RecordKey))
                .ToListAsync(ct).ConfigureAwait(false);
            foreach (var row in rows) customerRows[row.RecordKey] = row;
        }

        // Price-list names turn "list 3" into the label the app shows. Small
        // enough to read whole, and shared by every product on the page.
        var priceListNames = new Dictionary<int, string>();
        if (stockCodes.Count > 0)
        {
            var lookups = await db.MobileRecords.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Entity == "lookups")
                .Select(x => x.PayloadJson)
                .ToListAsync(ct).ConfigureAwait(false);
            foreach (var payload in lookups)
            {
                if (string.IsNullOrWhiteSpace(payload)) continue;
                using var document = JsonDocument.Parse(payload);
                var item = document.RootElement;
                if (!string.Equals(AndroidEndpoints.GetString(item, "kind"), "price_list", StringComparison.OrdinalIgnoreCase))
                    continue;
                var number = AndroidEndpoints.GetInt32(item, "code")
                             ?? (int.TryParse(AndroidEndpoints.GetString(item, "code"), out var parsed) ? parsed : 0);
                var name = AndroidEndpoints.GetString(item, "name");
                if (number > 0 && !string.IsNullOrWhiteSpace(name)) priceListNames[number] = name!;
            }
        }

        return new AssemblySources(stockParts, customerRows, priceListNames);
    }

    /// <summary>Everything needed to rebuild a page's worth of records.</summary>
    /// <param name="StockParts">Stock card, barcodes, prices and inventory, keyed by stock code.</param>
    /// <param name="Customers">Customer rows, keyed by customer code.</param>
    /// <param name="PriceListNames">Price-list number to display name.</param>
    public sealed record AssemblySources(
        Dictionary<string, List<MobileRecord>> StockParts,
        Dictionary<string, MobileRecord> Customers,
        Dictionary<int, string> PriceListNames);

    /// <summary>
    /// Rebuilds one product. Returns null when the stock card itself is gone —
    /// the caller turns that into a deletion.
    /// </summary>
    public static Dictionary<string, object?>? BuildProduct(
        string stockCode, AssemblySources sources, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(sources);
        if (!sources.StockParts.TryGetValue(stockCode, out var parts)) return null;

        var stockRow = parts.FirstOrDefault(x => string.Equals(x.Entity, "stocks", StringComparison.OrdinalIgnoreCase));
        if (stockRow?.PayloadJson is null) return null;

        using var stockDocument = JsonDocument.Parse(stockRow.PayloadJson);
        var stock = stockDocument.RootElement;

        var mapped = stock.EnumerateObject()
            .ToDictionary(p => p.Name, p => (object?)p.Value.Clone(), StringComparer.OrdinalIgnoreCase);

        // The app reads these under Mikro's own column names as well as the
        // neutral ones; both spellings are kept so no screen has to change.
        var reyonKod = AndroidEndpoints.GetFirstString(stock, "shelfCode", "sto_yer_kod");
        var olcu = AndroidEndpoints.GetFirstString(stock, "sectorCode", "sto_sektor_kodu");
        var ambalaj = AndroidEndpoints.GetFirstString(stock, "packageCode", "sto_ambalaj_kodu");
        var marka = AndroidEndpoints.GetFirstString(stock, "brandCode", "sto_marka_kodu");
        var koliAdet = AndroidEndpoints.GetFirstString(stock, "cartonCode", "sto_kalkon_kodu");
        mapped["reyonKod"] = reyonKod;
        mapped["olcu"] = olcu;
        mapped["ambalaj"] = ambalaj;
        mapped["marka"] = marka;
        mapped["koliAdet"] = koliAdet;
        mapped["sto_yer_kod"] = reyonKod;
        mapped["sto_sektor_kodu"] = olcu;
        mapped["sto_ambalaj_kodu"] = ambalaj;
        mapped["sto_marka_kodu"] = marka;
        mapped["sto_kalkon_kodu"] = koliAdet;

        var barcodes = Rows(parts, "barcodes").ToArray();
        if (barcodes.Length > 0)
        {
            mapped["barcodes"] = barcodes;
            mapped["barkod"] = AndroidEndpoints.GetString(barcodes[0], "barcode") ?? string.Empty;
        }

        var priceRows = Rows(parts, "prices")
            .Where(item => AndroidEndpoints.GetDecimal(item, "price") is > 0)
            .ToArray();

        // List 1 is the headline price; anything else only stands in for it
        // when list 1 is missing, lowest list number first.
        var best = priceRows
            .OrderBy(item => AndroidEndpoints.GetInt32(item, "listNumber") == 1 ? 0 : 1)
            .ThenBy(item => AndroidEndpoints.GetInt32(item, "listNumber") ?? int.MaxValue)
            .Select(item => AndroidEndpoints.GetDecimal(item, "price"))
            .FirstOrDefault(price => price is > 0);
        if (best is > 0)
        {
            mapped["satis_fiyati"] = best.Value;
            mapped["price"] = best.Value;
        }

        mapped["customPrices"] = priceRows
            .GroupBy(item => AndroidEndpoints.GetInt32(item, "listNumber") ?? 0)
            .Where(list => list.Key > 0)
            .ToDictionary(
                list => sources.PriceListNames.TryGetValue(list.Key, out var name) ? name : $"Liste {list.Key}",
                list => list.Select(item => AndroidEndpoints.GetDecimal(item, "price")).First(price => price is > 0)!.Value);

        var warehouses = Rows(parts, "inventory")
            .GroupBy(item => AndroidEndpoints.GetInt32(item, "warehouseNo") ?? 0)
            .ToDictionary(
                warehouse => $"Depo {warehouse.Key}",
                warehouse => (int)Math.Round(
                    warehouse.Sum(item => AndroidEndpoints.GetDecimal(item, "quantity") ?? 0m),
                    MidpointRounding.AwayFromZero));
        mapped["stockByWarehouse"] = warehouses;
        mapped["stok"] = warehouses.Values.Sum();

        mapped["updatedAt"] = updatedAt;
        mapped["isDeleted"] = false;
        return mapped;
    }

    /// <summary>
    /// Rebuilds one customer. Unlike a product this is a straight field mapping —
    /// the app's customer row has nothing folded into it. Returns null when the
    /// customer is gone.
    /// </summary>
    public static object? BuildCustomer(string customerCode, AssemblySources sources, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(sources);
        if (!sources.Customers.TryGetValue(customerCode, out var row) || row.PayloadJson is null) return null;

        using var document = JsonDocument.Parse(row.PayloadJson);
        var customer = document.RootElement;
        var code = AndroidEndpoints.GetString(customer, "customerCode");

        return new
        {
            id = code,
            erpRef = code,
            cariKod = code,
            unvan = AndroidEndpoints.JoinAddressLine(
                AndroidEndpoints.GetString(customer, "title1"), AndroidEndpoints.GetString(customer, "title2")),
            cariUnvan = AndroidEndpoints.GetString(customer, "title1"),
            vergiNo = AndroidEndpoints.GetString(customer, "taxNo"),
            vergiDairesi = AndroidEndpoints.GetString(customer, "taxOffice"),
            telefon = AndroidEndpoints.GetString(customer, "phone"),
            email = AndroidEndpoints.GetString(customer, "email"),
            cariBolgeKodu = AndroidEndpoints.GetString(customer, "regionCode"),
            paraBirimi = AndroidEndpoints.GetString(customer, "currency"),
            bakiye = AndroidEndpoints.GetDecimal(customer, "balance") ?? 0m,
            updatedAt,
            isDeleted = false,
        };
    }

    /// <summary>
    /// Parsed rows of one section, detached from the documents they came from.
    ///
    /// <para>Cloned deliberately: the assembled product outlives this call and
    /// some of these elements are handed straight into it, so leaving them bound
    /// to a document this method disposes would fail at serialization time
    /// rather than here.</para>
    /// </summary>
    private static List<JsonElement> Rows(List<MobileRecord> parts, string entity)
    {
        var rows = new List<JsonElement>();
        foreach (var part in parts)
        {
            if (!string.Equals(part.Entity, entity, StringComparison.OrdinalIgnoreCase)) continue;
            if (string.IsNullOrWhiteSpace(part.PayloadJson)) continue;
            using var document = JsonDocument.Parse(part.PayloadJson);
            rows.Add(document.RootElement.Clone());
        }
        return rows;
    }
}
