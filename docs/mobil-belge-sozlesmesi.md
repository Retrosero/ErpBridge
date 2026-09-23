# Sipariş Cepte → ERP Belge Sözleşmesi (v3)

Tarih: 2026-09-22 · Goal: [GOAL_ERP_YAZIM.md](GOAL_ERP_YAZIM.md) (Y2b çevirici, Y4a–c telefon),
[GOAL_ERP_YAZIM_3.md](GOAL_ERP_YAZIM_3.md) (Y1a — `expense`, `stock_count`, `purchase_receipt`)

Telefonun `POST …/jobs` ile gönderdiği **`sales_order`**, **`sales_return`**, **`collection`**, **`disbursement`**,
**`expense`**, **`stock_count`** ve **`purchase_receipt`** belgelerinin gövdesi.
ERP'li firmada ajan bu gövdeyi `MobileDocumentTranslator` (Core) ile ERP'den bağımsız komuta çevirir; Mikro'ya nasıl
yazılacağı [mikro-yazim-referansi.md](mikro-yazim-referansi.md)'de (§10 alış, §13 gider, §14 sayım). ERP'siz firmada
`NativeDocumentProcessor` aynı gövdeyi okur (yeni alanları yok sayar). `stock_count` ve `purchase_receipt`
ERP'siz firmada da kendi defterine kaydedilir (`BookStockCountAsync`/`BookPurchaseReceiptAsync`); `expense` ise
ERP'siz firmada hiç üretilmez — telefon `expenseCardCode` olmadan `expense` türü göndermez, ERP'siz firmada gider
her zaman `disbursement` (kasa defteri "Gider: …") olarak kalır.

## Genel kurallar

| Kural | Açıklama |
|---|---|
| Tanıma | Gövdede `mobileDocumentId` (string) varsa telefon belgesidir; yoksa eski tipli ingest sözleşmesi (`SalesOrderPayload` vb.) |
| Kimlik | `externalId` = `mobileDocumentId` (birebir; farklıysa `DOCUMENT_ID_MISMATCH`, belge yazılmaz); aynı belge türü + kimlik ikinci evrak açmaz. Gövde nesne değilse ya da `lines`/`payments` nesne dizisi değilse `INVALID_DOCUMENT` |
| Tarih `occurredAt` | `dd.MM.yyyy HH:mm`, `dd.MM.yyyy` ya da ISO 8601 (`2026-09-17T10:15:30+03:00`). ISO'da telefonun **duvar saati** alınır |
| Tutar `amount` | Telefonda gösterilen toplam (KDV dahil) — ajan kendi hesabıyla karşılaştırır, 0,05 TL'den fazla farkta yazmaz. Satış ve iadede 0 olabilir (tam iskonto, bedelsiz iade; stok yine hareket eder, ödeme alınmaz), tahsilatta sıfırdan büyük olmalı |
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
| `priceListNo` | evet (v2 işareti) | Fiyatın alındığı ERP fiyat listesi sıra no (`fiyatTanim` bölümündeki numara). KDV dahil/hariç bu listeden okunur. Sıfırdan büyük tam sayı olmalı; `null`, ondalık ya da metin `INVALID_DOCUMENT` (firma varsayılanına düşülmez). `warehouseNo` gönderilirse aynı kural |
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

## `disbursement` — tediye *(ERP yazım 2, Z3b)*

Telefonun **kasa defteri** gövdesi; `collection`'dan farkı `payments[]` dizisi olmaması — tek ödeme taşır.
Saha personelinin tediyesi de, bir **alışın nakit ödemesi** de bu belgeyi üretir (`PurchaseModule` kasa kaydını
`Tediye` türüyle yazar).

```json
{
  "mobileDocumentId": "KL-9", "revision": 1, "occurredAt": "17.09.2026 12:00",
  "transactionType": "Tediye", "counterparty": "Bakkal Ali", "customerCode": "120.001",
  "amount": 1500.50, "paymentType": "Nakit", "bankName": null,
  "description": "Saha Alış Girişi (A-42)"
}
```

| Alan | Kural |
|---|---|
| `customerCode` | **zorunlu** — cari kodu olmayan belge reddedilir (`MISSING_CUSTOMER_CODE`) |
| `amount` | > 0; sıfır tediye belge değildir |
| `paymentType` | `Nakit` (varsayılan) ya da `Havale / EFT`. Çek ve senet **çıkışı** bu goal'de yok, adıyla reddedilir (`UNSUPPORTED_PAYMENT_TYPE`) |
| `cashCode` / `bankCode` | isteğe bağlı; yoksa firmanın Portal'daki kasa / havale bankası kullanılır |
| `bankName` | **tek başına yetmez.** Havalede kullanıcı bir banka seçtiyse ERP kodu (`bankCode`) da gelmeli; yalnız ad gelirse belge `MOBILE_APP_UPDATE_REQUIRED` ile reddedilir. Sessizce firma varsayılanına yazmak, kimsenin seçmediği hesaba para koymak olurdu (telefon tarafı Z4c) |
| `currency` | yoksa TL sayılır |

Mikro karşılığı: tediye makbuzu (`cha_evrak_tip=64`), **borç** satırı, nakit `cinsi 0` + kasa, havale
`cinsi 20` (FirmaHavaleEmri) + banka — referans §11. Seri şimdilik tahsilat serisidir (Z1c'de ayrı ayar).

## `expense` — gider *(ERP yazım 3, Y1a/Y3b/Y4a-b)*

Telefonun gider ekranından çıkar. `disbursement`'tan farkı `expenseCardCode`: bu alan doluysa belge `expense`
türüyle kuyruğa girer (kasa defterinde yine "Tediye" görünür, yalnız merkeze giden tür değişir). Kart, karşı
taraf değildir — gider caride hiç iz bırakmaz.

```json
{
  "mobileDocumentId": "KL-42", "revision": 1, "occurredAt": "22.09.2026 09:30",
  "amount": 1250.75, "expenseCardCode": "YAKIT",
  "paymentType": "Nakit", "cashCode": null, "bankCode": null, "bankName": null,
  "vatAmount": 225.14, "vatPointer": 4,
  "description": "Saha aracı yakıt"
}
```

| Alan | Zorunlu | Anlam |
|---|---|---|
| `expenseCardCode` | evet | `MASRAF_HESAPLARI.his_kod` — telefona ERP'den senkronlanan gider kartı kataloğundan (K2). Eksikse/tanınmıyorsa `MOBILE_APP_UPDATE_REQUIRED` (eski telefon kendi kategori adını gönderiyorsa da aynı) |
| `amount` | evet, > 0 | Kasadan çıkan toplam rakam — KDV içindedir, üstüne eklenmez (K4) |
| `paymentType` (ya da `method`) | hayır (`Nakit`) | `Nakit`/boş → kasa; `Havale`/`EFT`/`Banka` → havale bankası; `Kredi Kartı`/`Banka Kartı` → kredi kartı bankası. Kredi kartı Mikro'da bir banka hesabıdır (K3, referans §13) |
| `cashCode` / `bankCode` | hayır | Yoksa ERP aktarım ayarlarındaki kasa/banka (nakit → Kasa (nakit), kredi kartı → Banka (kredi kartı), havale → Banka (havale/EFT)) |
| `bankName` | — | Tek başına yetmez; havale/kredi kartında banka seçildiyse `bankCode` de gelmeli, yoksa `MOBILE_APP_UPDATE_REQUIRED` (tediyedeki kuralla aynı) |
| `vatAmount` | hayır (0) | KDV telefondan gelir, ERP hesaplamaz (K4); `amount`'tan büyük olamaz |
| `vatPointer` | hayır (0) | Mikro'daki KDV tanım pointer'ı; `cha_vergi1`'e yazılır |
| `customerCode` | — | Gönderilmez/kullanılmaz — giderde cari yoktur |

Mikro karşılığı: kasa masraf fişi (`cha_evrak_tip=37`), tek `CARI_HESAP_HAREKETLERI` satırı, seri**siz** dizide
(§13). Gider kartı `cha_kasa_hizmet=5`/`cha_kasa_hizkod`'a, ödeyen hesap `cha_cari_cins`/`cha_kod`'a yazılır —
tediyenin tam tersi. Açıklama satırı (`EVRAK_ACIKLAMALARI`) yoktur, not `cha_aciklama`'da kalır.

## `stock_count` — sayım *(ERP yazım 3, Y1a/Y3c/Y4c)*

```json
{
  "mobileDocumentId": "SY-7", "occurredAt": "22.09.2026 08:00",
  "warehouseNo": 1,
  "lines": [
    { "stockCode": "B575", "barcode": "869…", "countedQuantity": 24 },
    { "stockCode": "XH1300", "barcode": null, "countedQuantity": 0 }
  ]
}
```

| Alan | Zorunlu | Anlam |
|---|---|---|
| `warehouseNo` | hayır | Yoksa firma ayarındaki depo |
| `lines[].stockCode` | evet | ERP stok kodu — barkoddan tahmin edilmez (K10); yalnız barkod gelen eski gövde `MOBILE_APP_UPDATE_REQUIRED` alır |
| `lines[].countedQuantity` | evet, ≥ 0 | Sayılan miktar; negatif `NEGATIVE_COUNTED_QUANTITY` |
| `amount` | — | Yok/sıfır sayılır — sayım para hareketi değildir |

Mikro karşılığı: `SAYIM_SONUCLARI`, depo bazlı fiş no MAX+1 (§14). Fiş yazılınca stok **kendiliğinden değişmez**;
farkın stoğa işlenmesi Mikro'nun kendi "sayım sonuçlarını uygula" adımıdır (K9) — Portal'da "ERP'de
kesinleştirilmeyi bekliyor" rozetiyle işaretlenir.

## `purchase_receipt` — alış faturası *(ERP yazım 3, Y1a/Y3a/Y4d)*

```json
{
  "mobileDocumentId": "AL-15", "occurredAt": "22.09.2026 10:00",
  "supplierCode": "320.010", "warehouseNo": null,
  "invoiceNo": "ÇINAR-000482",
  "settlementMethod": "Nakit", "cashCode": null,
  "lines": [
    { "productCode": "B575", "quantity": 50, "unitPrice": 210.00, "unitPointer": 1 }
  ]
}
```

| Alan | Zorunlu | Anlam |
|---|---|---|
| `supplierCode` | evet | Cari kodu (`customerCode` değil — alışta tedarikçi alanı ayrı) |
| `warehouseNo` | hayır | Yoksa ERP aktarım ayarındaki **alış deposu**, o da yoksa genel depo |
| `lines[].productCode`/`stockCode`, `quantity`, `unitPrice` | evet | Telefon KDV göstermez: `qty × unitPrice` toplamı, KDV stok kartının `vergi_pntr`'ından hesaplanır (K6); fiyatın KDV'li olup olmadığı ERP aktarım ayarındaki "Tedarikçi fiyatı" seçimiyle belirlenir |
| `settlementMethod` (ya da `paymentType`) | hayır (`Cari Borç`) | `Cari Borç`/boş → açık fatura; `Nakit` → kasadan kapalı; `Havale`/`EFT`/`Banka` → bankadan kapalı |
| `cashCode` / `bankCode` | hayır | Yoksa firma ayarındaki kasa/havale bankası |
| `invoiceNo` | hayır | Bilgi amaçlı; evrak numarası ERP'de tedarikçinin kendi serisinden MAX+1 devam eder (§15, K7) — telefondan seri gelmez |

Peşin alış **tek kapalı evraktır** (K13): ayrı bir tediye yazılmaz, ödeme aynı `CARI_HESAP_HAREKETLERI` satırını
kapatır. Eski telefon sürümü (ödeme bilgisiz gövde) fatura**yı açık** yazar; ödemesi ayrı bir `disbursement` olarak
zaten gelmiş olabilir.

## Yazım sonucu — `GET /api/v1/ingest/jobs/status` (Y4d)

Telefon gönderdiği belgelerin ERP'deki durumunu sorar: `?externalIds=MOB-SO-1,MOB-TH-2` (ya da tekrar eden parametre),
en çok 100 kimlik. Kimlik doğrulama `POST /api/v1/ingest/jobs` ile aynı; firma token'dan gelir, başka firmanın ve
bilinmeyen kimlikler yanıtta yoktur. Onay merkezinden geçen belge onaydan sonra aynı `externalId` ile görünür.

```json
{ "documents": [
  { "externalId": "MOB-SO-1", "documentType": "sales_order", "state": "written", "erpDocumentNo": "T-1234", "attempt": 1 },
  { "externalId": "MOB-TH-2", "documentType": "collection", "state": "retrying", "errorCode": "ERP_UNAVAILABLE",
    "message": "Mikro'ya ulaşılamadı.", "attempt": 2, "nextAttemptAtMs": 1789650000000 }
] }
```

| `state` | Telefonda | Anlam |
|---|---|---|
| `pending` | Bekliyor | Kuyrukta ya da ajan yazıyor |
| `retrying` | Yeniden denenecek | ERP'ye ulaşılamadı; `nextAttemptAtMs`'de yeniden alınır |
| `written` | Mikro'ya yazıldı: `erpDocumentNo` | Seri-sıra (seri yoksa yalnız sıra) |
| `failed` | Hata: `message` | Kendiliğinden düzelmez; `errorCode` `ErpWriteError` kataloğundan |

## Sunucunun iş ile gönderdiği `erpContext`

`GET /api/v1/jobs/pending` yanıtında (Y1d). Kullanıcı eşlemesi firma ayarıyla birleştirilmiş hâlidir:

```json
{
  "salesDocumentKind": "invoice", "orderApprovalMode": "approved",
  "series": { "order": "", "dispatch": "", "invoice": "T", "return": "", "collection": "" },
  "warehouseNo": 1, "cashCode": "001", "cardBankCode": "14", "transferBankCode": "04",
  "erpUserNo": 1, "salespersonCode": "PLS01", "priceListNo": 1,
  "chequePortfolioCode": "ÇEK", "notePortfolioCode": "SENET", "createdByUsername": "plasiyer1",
  "responsibilityCenterCode": null, "projectCode": null, "deliveryDayOffset": null
}
```

`responsibilityCenterCode` / `projectCode` belge başlığına (`ErpDocumentHeader`), `deliveryDayOffset` belge günü + gün
olarak satış komutunun `DeliveryDate`'ine geçer (sipariş/irsaliye teslim tarihi; boşsa belge günü).

C# karşılığı: `ErpBridge.Core.Jobs.ErpWriteContext`.
