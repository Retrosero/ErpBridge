# Sipariş Cepte → ERP Belge Sözleşmesi (v2)

Tarih: 2026-09-17 · Goal: [GOAL_ERP_YAZIM.md](GOAL_ERP_YAZIM.md) (Y2b çevirici, Y4a–c telefon)

Telefonun `POST …/jobs` ile gönderdiği **`sales_order`**, **`sales_return`** ve **`collection`** belgelerinin gövdesi.
ERP'li firmada ajan bu gövdeyi `MobileDocumentTranslator` (Core) ile ERP'den bağımsız komuta çevirir; Mikro'ya nasıl
yazılacağı [mikro-yazim-referansi.md](mikro-yazim-referansi.md)'de. ERP'siz firmada `NativeDocumentProcessor` aynı
gövdeyi okur (yeni alanları yok sayar).

## Genel kurallar

| Kural | Açıklama |
|---|---|
| Tanıma | Gövdede `mobileDocumentId` (string) varsa telefon belgesidir; yoksa eski tipli ingest sözleşmesi (`SalesOrderPayload` vb.) |
| Kimlik | `externalId` = `mobileDocumentId`; aynı belge türü + kimlik ikinci evrak açmaz |
| Tarih `occurredAt` | `dd.MM.yyyy HH:mm`, `dd.MM.yyyy` ya da ISO 8601 (`2026-09-17T10:15:30+03:00`). ISO'da telefonun **duvar saati** alınır |
| Tutar `amount` | Telefonda gösterilen toplam (KDV dahil) — ajan kendi hesabıyla karşılaştırır, 0,05 TL'den fazla farkta yazmaz |
| Döviz `currency` | Yok ya da `TL`/`TRY`; başka döviz reddedilir |
| Cari `customerCode` | Zorunlu (ERP'de kayıtlı müşteri) |
| Kodlar | `cashCode` (kasa), `bankCode` (banka) telefonda seçildiyse gönderilir; yoksa Portal'daki kullanıcı/firma ayarı |
| Eski gövde | Liste fiyatı / yapılandırılmış ödeme taşımayan gövde **`MOBILE_APP_UPDATE_REQUIRED`** ile reddedilir; ajan net fiyattan iskonto, açıklama metninden çek/senet bilgisi **çıkarmaz** |
| Hatalar | Kod ve Türkçe mesaj: `ErpBridge.Shared/ErpWriteError` |

## `sales_order` — satış

```json
{
  "mobileDocumentId": "MOB-SO-6f1c…", "revision": 1,
  "occurredAt": "17.09.2026 10:15",
  "customerCode": "120.001", "counterparty": "Bakkal Ali",
  "amount": 684.00, "currency": "TL",
  "paymentType": "Cari Borç",
  "cashCode": null, "bankCode": null,
  "priceListNo": 1,
  "warehouseNo": null, "salespersonCode": null,
  "description": "Katalog siparişi",
  "lines": [
    {
      "productCode": "B575", "barcode": "869…", "productTitle": "…",
      "quantity": 2, "unitPointer": 1,
      "listUnitPrice": 400.00,
      "lineDiscountPercent": 10, "customerDiscountPercent": 0, "generalDiscountPercent": 5,
      "unitPrice": 342.00, "lineTotal": 684.00, "note": null
    }
  ],
  "payments": null
}
```

| Alan | Zorunlu | Anlam |
|---|---|---|
| `priceListNo` | evet (v2 işareti) | Fiyatın alındığı ERP fiyat listesi sıra no (`fiyatTanim` bölümündeki numara). KDV dahil/hariç bu listeden okunur |
| `lines[].listUnitPrice` | evet (v2 işareti) | İskontosuz liste birim fiyatı |
| `lines[].lineDiscountPercent` / `customerDiscountPercent` / `generalDiscountPercent` | hayır (0) | Zincir: satır → müşteri → genel, her biri kalandan; 0–100 |
| `lines[].unitPrice`, `lineTotal` | hayır | Eski alanlar, yalnız bilgi |
| `paymentType` | hayır | `Cari Borç`/boş → açık; `Nakit` → kasa; `Kredi Kartı`/`Banka Kartı` → kart bankası; `EFT / Havale`/`Havale / EFT` → havale bankası |
| `payments` | hayır | Karma/kısmi ödeme: doluysa satış **açık** yazılır, ödemeler aynı işlemde tahsilat makbuzu olur (biçim `collection.payments` ile aynı) |
| `warehouseNo`, `salespersonCode` | hayır | Yoksa Portal ayarı |

Satış türü (sipariş/irsaliye/fatura) telefonda değil **firma ayarında** seçilir. Peşin ödeme yalnız **faturada** kapalı
fatura olur; siparişte ve irsaliyede evrak açık kalır, para tahsilat makbuzuyla yazılır.

## `sales_return` — satış iadesi

```json
{
  "mobileDocumentId": "MOB-SR-…", "occurredAt": "2026-09-17T11:00:00+03:00",
  "customerCode": "120.001", "amount": 430.00,
  "settlementMethod": "Cari Alacak",
  "cashCode": null, "bankCode": null,
  "priceListNo": 1, "warehouseNo": 2,
  "lines": [
    { "productCode": "B575", "quantity": 1, "listUnitPrice": 400, "conditionPercent": 1, "reason": "Sağlam" },
    { "productCode": "XH1300", "quantity": 1, "listUnitPrice": 100, "conditionPercent": 0.3, "reason": "Hasarlı" }
  ]
}
```

| Alan | Anlam |
|---|---|
| `lines[].listUnitPrice` | Zorunlu (v2 işareti) |
| `lines[].conditionPercent` | İade edilen pay, **0–1** (1 = sağlam). 1'den büyük gelirse yüzde sayılır (30 → 0,3). Stok tam miktarla girer, fark iskonto olur |
| `settlementMethod` (ya da `paymentType`) | `Cari Alacak`/boş → açık iade faturası; `Nakit` → kasadan; `Banka İade`/`EFT / Havale` → bankadan |

ERP'li firmada iade **satırlı** `sales_return` olarak gönderilir; ayrıca kasa kaydı (`return`) ajana gönderilmez.

## `collection` — tahsilat

```json
{
  "mobileDocumentId": "MOB-TH-…", "occurredAt": "17.09.2026 12:00",
  "customerCode": "120.001", "amount": 5000, "description": "Eylül tahsilatı",
  "payments": [
    { "method": "cash", "amount": 1000, "cashCode": null },
    { "method": "card", "amount": 1500, "bankCode": "13", "installments": 3, "surchargeAmount": 45 },
    { "method": "transfer", "amount": 500, "bankCode": null },
    { "method": "cheque", "amount": 1200, "dueDate": "2026-11-30",
      "cheque": { "no": "27703", "bankName": "Ziraat", "branch": "Fethiye", "accountNo": "123", "drawer": "Ali" } },
    { "method": "note", "amount": 800, "dueDate": "02.10.2026", "note": { "no": "S-5", "debtor": "Ali" } }
  ]
}
```

| Alan | Anlam |
|---|---|
| `payments` | Zorunlu (v2 işareti); **tek makbuz, ödeme başına satır**. Tutarların toplamı `amount` ile aynı olmalı (±0,01) |
| `method` | `cash`, `card`, `transfer`, `cheque`, `note` (Türkçe etiketler de kabul: `Nakit`, `Kredi Kartı`, `Havale / EFT`, `Çek`, `Senet`) |
| `cheque.no` + `dueDate` / `note.no` + `dueDate` | Çek/senette zorunlu; hesap Portal'daki çek/senet portföy kasası |
| `installments`, `surchargeAmount` | Kart taksiti ve vade farkı; vade farkı ayrı hareket yazılmaz, açıklamaya eklenir |
| vade | Nakit, kart ve havalede belge tarihi |

## Sunucunun iş ile gönderdiği `erpContext`

`GET /api/v1/jobs/pending` yanıtında (Y1d). Kullanıcı eşlemesi firma ayarıyla birleştirilmiş hâlidir:

```json
{
  "salesDocumentKind": "invoice", "orderApprovalMode": "approved",
  "series": { "order": "", "dispatch": "", "invoice": "T", "return": "", "collection": "" },
  "warehouseNo": 1, "cashCode": "001", "cardBankCode": "14", "transferBankCode": "04",
  "erpUserNo": 1, "salespersonCode": "PLS01", "priceListNo": 1,
  "chequePortfolioCode": "ÇEK", "notePortfolioCode": "SENET", "createdByUsername": "plasiyer1"
}
```

C# karşılığı: `ErpBridge.Core.Jobs.ErpWriteContext`.
