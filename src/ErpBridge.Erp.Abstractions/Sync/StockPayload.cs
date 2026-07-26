namespace ErpBridge.Erp.Abstractions.Sync;

/// <summary>
/// Stok (item / SKU) master record carried inside a <see cref="SyncPackage"/>.
/// Three unit slots are exposed because Mikro lets a single stok carry up to
/// three units (adet / kilo / metre, etc.) with conversion factors. The
/// <see cref="BedenliTakip"/> and <see cref="RenkDetayli"/> flags advertise
/// variant-tracking behaviour; Phase 6+ writers consume them when building
/// <c>STOK_HAREKETLERI</c> rows.
/// </summary>
/// <param name="StockCode">Unique stok kodu.</param>
/// <param name="Name">Primary display name.</param>
/// <param name="ShortName">Short display name (etiket / satır).</param>
/// <param name="ForeignName">English / foreign-language name (export / e-fatura).</param>
/// <param name="DefaultTaxPointer">Default KDV pointer index (0-8).</param>
/// <param name="Unit1">Birim 1 kodu (adet / kg / mt ...).</param>
/// <param name="Unit1Factor">Conversion factor from unit 1 to base unit.</param>
/// <param name="Unit2">Birim 2 kodu.</param>
/// <param name="Unit2Factor">Conversion factor from unit 2 to base unit.</param>
/// <param name="Unit3">Birim 3 kodu.</param>
/// <param name="Unit3Factor">Conversion factor from unit 3 to base unit.</param>
/// <param name="MainGroupCode">Ana grup kodu (e.g. "GIDA").</param>
/// <param name="SubGroupCode">Alt grup kodu (e.g. "SEBZE").</param>
/// <param name="SectorCode">Sektör kodu.</param>
/// <param name="BrandCode">Marka kodu.</param>
/// <param name="ModelCode">Model kodu.</param>
/// <param name="ManufacturerCode">Üretici kodu.</param>
/// <param name="ShelfCode">Raf kodu (depo içi konum).</param>
/// <param name="BedenliTakip">True when the stok tracks sizes (variant SKU).</param>
/// <param name="RenkDetayli">True when the stok tracks colours (variant SKU).</param>
/// <param name="StandardCost">Standard maliyet (for inventory valuation).</param>
/// <param name="Currency">Cost / list currency code.</param>
/// <param name="Barcodes">All barcodes attached to the stok (often multiple per unit).</param>
/// <param name="PackageCode">Mikro ambalaj kodu (<c>sto_ambalaj_kodu</c>).</param>
/// <param name="CartonCode">Mikro koli adedi/kodu (<c>sto_kalkon_kodu</c>).</param>
public sealed record StockPayload(
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
    string? Currency,
    IReadOnlyList<BarcodePayload> Barcodes,
    string? PackageCode = null,
    string? CartonCode = null);

/// <summary>
/// One barcode attached to a <see cref="StockPayload"/>. Lot and serial
/// numbers are optional because most barcodes do not carry them. The
/// <see cref="UnitPointer"/> identifies which unit (1/2/3) the barcode prices.
/// </summary>
/// <param name="Barcode">The barcode digits (EAN-13, EAN-8, Code-128, ...).</param>
/// <param name="StockCode">The stok kodu the barcode resolves to.</param>
/// <param name="PartCode">Optional variant / beden-renk part kodu.</param>
/// <param name="LotNo">Lot / parti numarası when the barcode is lot-tracked.</param>
/// <param name="SerialNo">Serial / seri numarası when the barcode is serial-tracked.</param>
/// <param name="UnitPointer">Which unit slot (1/2/3) the barcode prices.</param>
public sealed record BarcodePayload(
    string Barcode,
    string StockCode,
    string? PartCode,
    string? LotNo,
    string? SerialNo,
    int UnitPointer);
