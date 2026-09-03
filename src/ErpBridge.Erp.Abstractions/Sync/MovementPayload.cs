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
    [property: JsonPropertyName("normalIade")] bool IsReturn = false);

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
