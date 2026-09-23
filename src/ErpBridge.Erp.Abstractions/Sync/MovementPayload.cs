using System.Text.Json.Serialization;

namespace ErpBridge.Erp.Abstractions.Sync;

/// <summary>A single row from Mikro CARI_HESAP_HAREKETLERI.</summary>
public sealed record CustomerTransactionPayload(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("erpRef")] string ErpRef,
    [property: JsonPropertyName("erp")] string Erp,
    [property: JsonPropertyName("cariKod")] string CustomerCode,
    [property: JsonPropertyName("tarih")] DateTime Date,
    [property: JsonPropertyName("evrakTip")] int DocumentType,
    [property: JsonPropertyName("evrakNo")] string? DocumentNo,
    [property: JsonPropertyName("tip")] int Type,
    [property: JsonPropertyName("tutar")] decimal Amount,
    [property: JsonPropertyName("borcMu")] bool IsDebit,
    [property: JsonPropertyName("aciklama")] string? Description,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt,
    [property: JsonPropertyName("cha_recno")] int? RecNo,
    [property: JsonPropertyName("type")] string TransactionType = "HAREKET",
    [property: JsonPropertyName("cins")] int Kind = 0,
    [property: JsonPropertyName("normalIade")] bool IsReturn = false,
    [property: JsonPropertyName("ciroCariKod")] string? CounterpartyCode = null,
    [property: JsonPropertyName("kapali")] bool IsClosed = false,
    // Mikro cha_kasa_hizmet / cha_kasa_hizkod. On a kasa masraf fişi (evrak tip 37) the service is
    // Giderimiz (5) and the code is the expense card (MASRAF_HESAPLARI.his_kod): the phone's expense
    // screen lists ERP expenses under their card. The names are the ones the phone already reads.
    [property: JsonPropertyName("cha_kasa_hizmet")] int? CashServiceKind = null,
    [property: JsonPropertyName("cha_kasa_hizkod")] string? CashServiceCode = null);

/// <summary>A single row from Mikro STOK_HAREKETLERI.</summary>
public sealed record StockTransactionPayload(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("erpRef")] string ErpRef,
    [property: JsonPropertyName("erp")] string Erp,
    [property: JsonPropertyName("stokKod")] string StockCode,
    [property: JsonPropertyName("urunKod")] string ProductCode,
    [property: JsonPropertyName("tarih")] DateTime Date,
    [property: JsonPropertyName("tip")] int Type,
    [property: JsonPropertyName("cins")] int Kind,
    [property: JsonPropertyName("evrakTip")] int DocumentType,
    [property: JsonPropertyName("evrakNo")] string? DocumentNo,
    [property: JsonPropertyName("girisMiktar")] decimal InQuantity,
    [property: JsonPropertyName("cikisMiktar")] decimal OutQuantity,
    [property: JsonPropertyName("miktar")] decimal SignedQuantity,
    [property: JsonPropertyName("birimFiyat")] decimal UnitPrice,
    [property: JsonPropertyName("tutar")] decimal Amount,
    [property: JsonPropertyName("cariKod")] string? CustomerCode,
    [property: JsonPropertyName("girisDepoNo")] int? InWarehouseNo,
    [property: JsonPropertyName("cikisDepoNo")] int? OutWarehouseNo,
    [property: JsonPropertyName("aciklama")] string? Description,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt,
    [property: JsonPropertyName("faturaRecno")] int? InvoiceRecNo);
