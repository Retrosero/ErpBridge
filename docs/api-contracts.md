# ErpBridge — API Contracts (Central SaaS API)

Bu doküman, Windows Agent ile merkezi SaaS API arasındaki HTTP sözleşmelerini tanımlar.
Tüm endpoint'ler **outbound** çağrılır; agent inbound port açmaz.

## Kimlik doğrulama

- `Authorization: Bearer <jwt>` — agent registration sonrası alınan JWT.
- License key ile validate edilir.

## Endpointler

### POST /api/v1/agents/register

Yeni agent kaydı. Body: `{ licenseKey, machineId, agentVersion }`. Yanıt:
`{ agentId, jwt, tenantId }`.

*(Log Merkezi L3g)* Ajanın yaptığı **her** istek `X-Correlation-Id` taşır. Bir iş
işlenirken bu, işin `correlationId`'sidir (`GET /jobs/pending` yanıtındaki alan);
iş dışındaki çağrılar taze bir kimlik üretir. `POST /api/v1/ingest/jobs`'a gelen
başlık `jobs.correlation_id`'ye yazılır; istemci göndermezse ara katman üretir ve
yanıt başlığında döner.

### POST /api/v1/agents/heartbeat

Periyodik (örn. her 60 sn). Body: `AgentHeartbeat { agentId, tenantId, status,
lastSyncAtUtc, queueDepth, lastError? }` **+ (Log Merkezi L3f, hepsi isteğe bağlı)**
`appVersion`, `hostKind` (`service`/`ui`), `erpKind`, `erpVersion`, `lastSyncResult`
(`ok`/`failed`), `lastErrorCode`. Yanıt: 204.

Kimlik ve firma yalnız token'dan okunur. Yeni alanların hiçbiri zorunlu değildir:
**eski gövde aynen kabul edilir** ve gönderilmeyen bir alan sunucuda saklı değeri
**silmez**. `lastSyncAtUtc` artık "şimdi" değil, ajanın gerçekten tamamladığı son
senkron turudur (hiç senkronize etmemiş ajan `null` gönderir); canlılık ölçüsü
`agents.last_heartbeat_at`'tir. `lastError` ajanda maskelenir, sunucuda bir kez daha
maskelenerek saklanır. Her heartbeat `agent_heartbeat_log`'a satır **yazmaz**: satır
ancak okunabilir bir şey değiştiğinde (durum, kuyruk derinliği, sürüm, hata) ya da son
satır 15 dakikadan eskiyse yazılır.

### POST /api/v1/agents/telemetry

Windows Agent'ın gizlilikten arındırılmış hata kaydı. Agent JWT'si zorunludur.
Body: `{ eventId, occurredAtUtc, kind, severity, appVersion, windowsVersion,
machineName, operation, exceptionType, message, stackTrace }`. Yanıt: `204`.
Lisans anahtarı, JWT, SQL şifresi, bağlantı dizesi ve ERP payloadları kesinlikle
gönderilmez; agent bunları göndermeden önce maskeler. Kayıtlar mobil tanılama
kayıtlarıyla aynı yönetim ekranında tutulur.

### POST /api/v1/agents/logs/batch

Ajanın kuyrukta bekleyen tanılama olayları (Log Merkezi L3c). Agent JWT'si zorunlu.
Body: `{ events: [{ eventId, occurredAtUtc, severity, kind, operation, message,
exceptionType, stackTrace, appVersion, osVersion, machineName, correlationId,
propertiesJson, source, repeatCount }] }` — 1–50 olay; `eventId` en çok 64 karakter ve
kaynak içinde tekildir (kaybolan yanıt sonrası tekrar gönderim ikinci kez saklanmaz).
`source` `windows_service` (varsayılan) ya da `windows_agent`. Firma ve ajan token'dan
okunur. Yanıt: `200 { accepted, duplicate }`; gövde 1–50 aralığında değilse 400
`INVALID_LOG_BATCH`, kimliksiz olayda 400 `INVALID_EVENT_ID`. Yazım hatası 5xx'tir;
ajan olayları kuyrukta tutup yeniden dener. Gizli bilgi ajanda maskelenir.

### POST /api/v1/licenses/validate

`{ licenseKey }` → `{ valid, tenantId, expiresAtUtc }`. Agent başlangıcında bir kez
çağrılır; süre bitiminde tekrar doğrulanır. Yalnız `erpbridge` ürünü anahtarları geçer; Go anahtarı 404 alır.

### POST /api/v1/go/license/activate

Go masaüstü uygulamasının tek çağrısı (anonim). `{ licenseKey, machineId, machineName?, appVersion? }` →
200 `{ token, tenantName, validUntilUtc, licenseExpiresAtUtc, modules[] }`. `token` = `base64url(json).base64url(ES256)`;
uygulama yalnız imzalı iddialara güvenir. Etkinleştirme ve yenileme aynı çağrıdır; anahtar ilk makineye bağlanır.
Hatalar: 400 `MISSING_LICENSE_KEY`/`INVALID_MACHINE_ID`, 404 `LICENSE_NOT_FOUND`, 409 `DEVICE_LIMIT_REACHED`,
410 `LICENSE_REVOKED`/`LICENSE_EXPIRED`, 503 `GO_LICENSING_UNAVAILABLE`. Ayrıntı: KB 00 kural 30.

### GET /api/v1/jobs/pending

Query: `?take=50&type=sales_order`. Yanıt: `RemoteJob[] { jobId, externalId,
documentType, payload, enqueuedAtUtc, attempt, erpContext? }`.
Kiralama 10 dakikadır; süresi dolan `Processing` iş ve `nextAttemptAtMs`'i gelmiş
`Pending` iş yeniden verilir. `erpContext` yalnız ERP'li firmada, kiralama anında
firma ayarı + gönderen kullanıcının eşlemesinden kurulur (bkz. `docs/mobil-belge-sozlesmesi.md`).

### POST /api/v1/jobs/ack

Body: `JobAck { jobId, status: "succeeded" | "failed", errorCode?, errorMessage?,
erpDocumentSeries?, erpDocumentNumber?, erpRecno?, erpGuid?, retryable?, attempt? }`. Yanıt: 204.
`failed` + `retryable: true` (ERP'ye ulaşılamadı) işi `Pending`'e döndürür, 1-2-4-8-15-30-60 dk
bekletir (en çok 10 deneme). `attempt` kiralamadaki değerden farklıysa 409 `STALE_LEASE`
(sonuç uygulanmaz). Hatalı/yeniden denenecek sonuç Log Merkezi'ne `ERP_WRITE_FAILED` /
`ERP_WRITE_RETRY` olarak yazılır.

### GET /api/v1/ingest/jobs/status

Telefonun (ingest ile aynı kimlik doğrulama) gönderdiği belgelerin ERP durumu.
Query: `?externalIds=MOB-SO-1,MOB-TH-2` (ya da tekrar eden parametre, 1–100). Yanıt:
`{ documents: [{ externalId, documentType, state: "pending" | "retrying" | "written" | "failed",
erpDocumentNo?, errorCode?, message?, attempt, nextAttemptAtMs? }] }`. Başka firmanın ve
bilinmeyen kimlikler yanıtta yoktur; 0 ya da 100'den fazla kimlik 400 `INVALID_EXTERNAL_IDS`.

### Portal ERP uçları (`/api/v1/portal`, firma kullanıcısı token'ı)

| Uç | Yetki | Açıklama |
|---|---|---|
| `GET/PUT /erp-settings` | Admin | Satış türü, sipariş onayı, seriler, varsayılan depo/kasa/banka/ERP kullanıcı/temsilci/fiyat listesi, portföy kasaları, teslim günü (400 `INVALID_ERP_SETTINGS`) |
| `GET/PUT /users/{id}/erp-mapping` | Admin | Kullanıcının ERP karşılıkları; boş değer firma ayarına düşer |
| `GET /erp-lookups` | Admin | Ajanın gönderdiği depo, kasa, banka, temsilci, fiyat listesi, proje kodları |
| `GET /erp-documents` | Admin, Yönetici, Muhasebe | `from`, `to` (≤31 gün, varsayılan son 7 gün), `state`, `documentType`, `userId`, `customer`, `page`, `pageSize` (≤100). Öğe: tür, gönderen, cari, tutar, durum, ERP no, neden, deneme, `canRetry` |
| `POST /erp-documents/{jobId}/retry` | Admin | Yalnız hatalı belge; `{ jobId, state: "pending" }`, aksi hâlde 409 `JOB_NOT_RETRYABLE` |

ERP'siz firmada bu uçlar 409 `ERP_NOT_CONNECTED` döner.

### Portal ERP'siz firma uçları (`/api/v1/portal/native`, firma kullanıcısı token'ı — GOAL_PANEL_ERPSIZ)

`GET /documents` ve `GET /documents/{key}` dışındaki uçların hepsi yalnız **ADMIN** + `DataSource=native` içindir
(403 `ROLE_NOT_ALLOWED`, ERP'li firmada 409 `TENANT_IS_NOT_NATIVE`); o iki okuma ucu ekstrenin kapısını
(`CanViewLedger`: Admin, Yönetici, Muhasebe; ERP'li firma da) kullanır. Yazma uçları `NativeDocumentProcessor`'a gider,
yanıt `IngestJobResponse` (`201` yeni, `200` aynı `operationId` ile tekrar) — **istisna** toplu içe aktarma uçları
(`/stock-cards/batch`, `/customer-cards/batch`): her zaman `200` ve aşağıdaki `PortalCardBatchResponse`.
İşleyicinin reddi 422 ve uca özgü `*_REJECTED` kodu.
Her yazma `native_audit_log`'a düşer (`GET /api/v1/portal/native/audit`).

| Uç | Açıklama |
|---|---|
| `GET /stock-cards/{code}` · `POST /stock-cards` · `DELETE /stock-cards/{code}?operationId=` | Ürün kartı oku/aç/düzenle/sil (hareketli ürün silinmez). Silmede `operationId` gövdede değil **sorguda**; verilmezse tekrar yeni işlem sayılır |
| `GET /stock-cards/{code}/movements` | `from`, `to`, `includeVoided`, `page`, `pageSize`. `opening` (devir), `closing`, `totalIn`/`totalOut`, satırlar en yeni üstte: `kind` (sale/purchase/sale_return/count/void/other), `in`/`out`, `balance` (yürüyen stok), `voided`, `reason` |
| `POST /customer-cards` | Cari kartı aç/düzenle |
| `POST /collections` · `POST /disbursements` | Tahsilat / tediye |
| `POST /ledger-adjustments` | Manuel bakiye düzeltmesi, gerekçe zorunlu |
| `POST /ledger/{key}/void` · `POST /ledger/{key}/edit` | Tek başına tahsilat/tediye/düzeltmenin iptali / düzeltilmesi (storno). İptalin gövdesi `reason`, düzeltmeninki düzeltilmiş hareket + `voidReason` (ikisi de zorunlu) |
| `POST /sales-orders` · `/purchase-receipts` · `/sales-returns` | Satırlı evrak: `partyCode`, `lines[{productCode, quantity, unitPrice, lineTotal?}]`, `amount?`, `paymentType?` (anında ödeme), `occurredAt?`, `documentNo?` |
| `GET /documents` | Admin, Yönetici, Muhasebe (salt okunur). `from`, `to`, `kind[]`, `customer`, `userId`, `status=all\|active\|voided`, sayfalı; satırda `voided` |
| `GET /documents/{key}` | Tek evrak: satırlar (ters satırlar hariç), `voided`, `paymentType` |
| `POST /documents/{key}/void` | Evrağın cari ve stok etkisi birlikte iptal (`reason`) |
| `POST /documents/{key}/edit` | Evrak gövdesi + `voidReason`; aynı türde düzeltilmiş evrak, `-D1` revizyon numarası (dolu numara atlanır; elle verilen dolu numara 422) |
| `POST /stock-counts` | `lines[{productCode, countedQuantity}]`, `reason` zorunlu; fark kayıt anındaki stoka göre |
| `POST /stock-counts/{key}/void` | Sayımın tüm satırlarını geri alır; `key` sayımın iş kimliği ya da bir hareket kimliği |
| `GET /barcodes/{barcode}` | Barkoda, yoksa ürün koduna **tam** eşleşen ürün kartı (sayım ekranının okutması) |
| `POST /stock-cards/batch` · `POST /customer-cards/batch` | Dosyadan içe aktarma: `cards[]` (≤ 500), `rows[]?` (her kartın dosya satırı) ya da `firstRow`, `operationId`. Yanıt `booked`, `skipped[{row, reason, message}]` (`CODE_REQUIRED`, `CODE_TOO_LONG`, `NAME_REQUIRED`, `DUPLICATE_CODE`, `DUPLICATE_BARCODE`, `BARCODE_IN_USE`, `REJECTED`), `idempotent` |

İptal/düzenleme uçları (`ledger/{key}/void|edit`, `documents/{key}/void|edit`, `stock-counts/{key}/void`) "zaten iptal" kontrolünden **önce** aynı `operationId`'li işi arar: yanıtı kaybolan tekrar 200 alır, 409 değil. `operationId` içindeki `|` `-` olur (hareket anahtarları `{iş}|{ek}`).

### Admin iş uçları (`/api/v1/admin/jobs`)

Liste öğesi `nextAttemptAtUtc`, `leasedUntilUtc` taşır. `GET /{id}` ek olarak `payloadJson`,
`createdByUserId`, `retryable` (son ajan sonucu `retry`), `lastErrorCode`, `erpDocumentNo`,
`acks[]` (yeniden eskiye) ve ERP'li firmada ajanın şimdi alacağı `erpContext`'i döner.

### POST /api/v1/bootstrap

Body: `SyncPackage { customers, stocks, prices, inventory, openOrders, cashAndBank,
lookups }`. Yanıt: 204. Tenant başına periyodik (Faz 9: her 60 sn delta push).
Başarılı insert'ten sonra sunucu, bu tenant'ın `/api/v1/bootstrap/notify` long-poll
bekleyenlerini cursor ile uyandırır.

Yeni agent'lar büyük tam snapshot'lar için chunked sözleşmeyi kullanır:

- `POST /api/v1/bootstrap/upload/start` → `{ uploadId, maxItemsPerChunk }`
- `POST /api/v1/bootstrap/upload/{uploadId}/chunks` → `{ section, chunkIndex, items[] }`
- `POST /api/v1/bootstrap/upload/{uploadId}/complete` → 204

Chunk'lar staging snapshot'a yazılır; complete başarılı olmadan Android aktif
snapshot'ı değiştirmez. Aynı `Idempotency-Key` ve chunk index tekrar gönderilirse
aynı işlem no-op olur. Tenant başına yalnızca bir aktif snapshot tutulur.

### GET /api/v1/bootstrap/notify

Long-polling. Agent Service veya WPF UI bir push'u beklemek için bu endpoint'i
çağırır. Body yok. Query: `wait` (int, default 30, max 60, min 1 saniye).
Yanıtlar:

- `200 OK` — `BootstrapNotifyResponse { updated: true, lastPulledAtUtc: <cursor> }`
  (yeni bootstrap paketi `wait` penceresi içinde geldi).
- `204 No Content` — `wait` süresi doldu, publish olmadı.
- `400 Bad Request` — `wait` 1..60 aralığında değil (`INVALID_WAIT`).
- `401 Unauthorized` — JWT yok / geçersiz.

```text
GET /api/v1/bootstrap/notify?wait=30
Authorization: Bearer <jwt>
```

Sunucu `POST /api/v1/bootstrap` başarılı olduğunda ilgili tenant'ın tüm
long-poll bekleyenlerini uyandırır ve 200 ile cursor'ı döner. Hub process-local
pub/sub kullanır (tek-replica Coolify varsayımı); çok-instans dağıtımda
Redis backplane gerekir (ileride).

## Android veri okuma API'si

Android istemcisi `https://lisans.appsgo.cloud` adresini kullanır. İlgili tenant için
admin panelinden `mobile:read` scope'lu ayrı bir API key oluşturulmalıdır. Her
istekte aşağıdaki header'lar zorunludur:

```text
Authorization: Bearer AK-...
X-Tenant-Id: <tenant-guid>
Accept: application/json
```

`POST /api/v1/android/bootstrap` en son ERP snapshot'unun metadata'sını,
`POST /api/v1/android/pull` ise snapshotun tamamını döner. Büyük veri setleri
için Android aşağıdaki daraltılmış endpointleri kullanabilir:

- `POST /api/v1/android/sync/cari` → `customers`
- `POST /api/v1/android/sync/urun` → `stocks`
- `POST /api/v1/android/sync/stokSeviye` → `inventory`
- `POST /api/v1/android/sync/fiyatlar` → `prices`
- `POST /api/v1/android/sync/acikSiparisler` → `openOrders`

Yanıtlar tenant'a kesin olarak izole edilir; body içinde API key veya tenant id
gönderilmez. `MOBILE_READ_SCOPE_REQUIRED` API key'in yalnızca yazma yetkisi
olduğunu, `BOOTSTRAP_NOT_FOUND` ise henüz ERP'den veri gelmediğini belirtir.

## SKT (son kullanma tarihi) kayıtları — `/api/v1/android/expiry`

Telefonlardan girilen raf verisi: hangi ürün hangi reyon/rafta, hangi son kullanma tarihiyle duruyor.
Firmanın bütün telefonları aynı kayıtları paylaşır; ERP'li ve ERP'siz firmada aynı çalışır, **ERP'ye hiçbir
şey yazılmaz** (Mikro ana verisi değildir). Kimlik: firma kullanıcısı token'ı (`MobileUserPolicy`), hız sınırı
kullanıcı başına (`PerMobileUserRateLimitPolicy`); her çağrıda kullanıcı, cihaz ve abonelik yeniden denetlenir.
Firmanın her aktif kullanıcısı okur ve yazar (rol şartı yok). Bilgi bankası kural 29.

| Uç | Açıklama |
|---|---|
| `GET /api/v1/android/expiry?changedSinceSeq={long}&take={int}` | Firmanın `updatedSeq > changedSinceSeq` kayıtları, `updatedSeq` sırasıyla. `take` varsayılan 500, 1–1000 aralığına kısılır. Silinenler `deleted=true` ile gelir (telefon yerel kopyasını düşürür). Yanıt `{ records: ExpiryRecordDto[], latestSeq, hasMore }`; `latestSeq` sayfanın son `updatedSeq`'i, sayfa boşsa gönderilen değer. Döngüyü `hasMore` sürdürür. |
| `POST /api/v1/android/expiry/ops` | Gövde `{ ops: ExpiryOp[] }` (≤ 200; fazlası 400 `EXPIRY_BATCH_TOO_LARGE`, hiçbiri uygulanmaz). Yanıt `{ results: [{ opId, status: "applied"\|"duplicate"\|"rejected", errorCode?, message? }], records: ExpiryRecordDto[] }` — `records` uygulanan işlemlerin dokunduğu kayıtların güncel hâli. |

**`ExpiryOp`:** `{ opId: guid, type: "upsert" | "delete", record: ExpiryRecordInput }`. İşlemler sırayla uygulanır;
daha önce uygulanmış `opId` `duplicate` döner (yeniden uygulanmaz), reddedilen işlem kendi savepoint'ine geri
döner ve partinin geri kalanını durdurmaz. Reddedilen işlem yeniden denenmez.

- `upsert`: kimlik firmada yoksa kayıt açılır (oluşturan kullanıcı ve zaman yalnız burada yazılır), varsa
  düzenlenebilir alanların **tamamı** üzerine yazılır (son yazan kazanır; `null` "boş" demektir, "değişmedi" değil).
  Silinmiş kayıt düzenlenmez: `EXPIRY_NOT_FOUND` ("Kayıt silinmiş.").
- `delete`: yumuşak silme (`deleted=true`, yeni `updatedSeq`); yalnız `record.id` okunur. Bilinmeyen kimlik
  `EXPIRY_NOT_FOUND`; zaten silinmiş kayıt `applied` (değişiklik sayılmaz, `updatedSeq` ilerlemez).
- Kimlik tüm firmalar arasında tekildir; başka firmanın kaydı görünmez, düzenlenmez, silinmez (`EXPIRY_NOT_FOUND`).
- Hata kodları: `EXPIRY_OP_ID_REQUIRED` (opId yok), `EXPIRY_OP_UNKNOWN` (bilinmeyen `type`), `EXPIRY_INVALID`
  (doğrulama; `message` alanı adlandırır, ör. "Son kullanma tarihi okunamadı.", "Reyon/raf boş olamaz.",
  "Ürün kodu boş olamaz."), `EXPIRY_NOT_FOUND`.

**`ExpiryRecordInput`:** `{ id: guid (telefon üretir, zorunlu), stockCode (zorunlu, ≤ 50), barcode? (≤ 50),
productName? (≤ 200), location (reyon/raf, zorunlu, ≤ 50), warehouse? (≤ 100), expiryDate: "yyyy-MM-dd"
(zorunlu, geçerli tarih, yıl 2000–2100), quantity?: number (0–999999999, en çok 3 ondalık), note? (≤ 500),
closed: bool (varsayılan false) }`. Metinler kırpılır (trim); boş isteğe bağlı alan `null` saklanır; sınırı aşan
metin reddedilir (kesilmez). Miktar bilgi amaçlıdır; satış düşmez, raf bitince kayıt `closed` yapılır.

**`ExpiryRecordDto`:** `{ id, stockCode, barcode, productName, location, warehouse, expiryDate: "yyyy-MM-dd",
quantity: number | null, note, closed, deleted, createdBy (oluşturanın görünen adı), createdAtMs, updatedAtMs,
updatedSeq }`. Zamanlar unix ms (UTC).

## Bekleyen siparişler — `/api/v1/android/suspended-sales`

Telefonda beklemeye alınan satış sepetleri (cari, depo, not, satırlar). Firmanın bütün telefonları aynı listeyi
görür; biri açıp tamamlayabilir. ERP'li ve ERP'siz firmada aynı çalışır, **ERP'ye hiçbir şey yazılmaz** (taslaktır,
evrak değil). Kimlik: firma kullanıcısı token'ı (`MobileUserPolicy`), hız sınırı kullanıcı başına; her çağrıda
kullanıcı, cihaz ve abonelik yeniden denetlenir. Bilgi bankası kural 32.

| Uç | Açıklama |
|---|---|
| `GET /api/v1/android/suspended-sales?changedSinceSeq={long}&take={int}` | Firmanın `updatedSeq > changedSinceSeq` siparişleri, `updatedSeq` sırasıyla; `take` varsayılan 500, 1–1000. Açılan/silinenler `deleted=true` ile (satırsız) gelir. Yanıt `{ sales: SuspendedSaleDto[], latestSeq, hasMore }`. |
| `POST /api/v1/android/suspended-sales/ops` | Gövde `{ ops: SuspendedSaleOp[] }` (≤ 200; fazlası 400 `SUSPENDED_SALE_BATCH_TOO_LARGE`). Yanıt `{ results: [{ opId, status: "applied"\|"duplicate"\|"rejected", errorCode?, message? }], sales: SuspendedSaleDto[] }` — `sales` partinin dokunduğu (uygulanan **ve reddedilen**) siparişlerin güncel hâli; telefon "başkası açtı" durumunu buradan öğrenir. |

**`SuspendedSaleOp`:** `{ opId: guid, type: "upsert" | "claim" | "delete", sale: SuspendedSaleInput }`. `duplicate`,
savepoint ve "reddedilen yeniden denenmez" kuralları SKT ile aynıdır.

- `upsert`: kimlik yoksa sipariş açılır (oluşturan yalnız burada yazılır); varsa tüm alanlar üzerine yazılır — yalnız
  oluşturan ya da ADMIN/MANAGER (`SUSPENDED_SALE_FORBIDDEN`). Açılmış/silinmiş sipariş yeniden yazılmaz (`SUSPENDED_SALE_TAKEN`).
- `claim`: telefonda **açmak**. Firmanın her kullanıcısı; sipariş herkesin listesinden kalkar (`closedReason="claimed"`,
  `closedBy`). Zaten açılmış/silinmiş sipariş `SUSPENDED_SALE_TAKEN`, mesaj kimin aldığını söyler ("… Veli tarafından
  açıldı."). Aynı anda iki telefon açarsa sayaç kilidinde sıraya girer; ikincisi reddedilir.
- `delete`: iptal; yalnız oluşturan ya da ADMIN/MANAGER (`SUSPENDED_SALE_FORBIDDEN`). Kendi sildiğini tekrar silmek `applied`.
- Kimlik tüm firmalar arasında tekildir; başka firmanın siparişi `SUSPENDED_SALE_NOT_FOUND`.
- Diğer kodlar: `SUSPENDED_SALE_OP_ID_REQUIRED`, `SUSPENDED_SALE_OP_UNKNOWN`, `SUSPENDED_SALE_INVALID` (en az 1, en çok
  500 satır; miktar > 0, en çok 3 ondalık; iskonto 0–100; tutarlar ≥ 0).

**`SuspendedSaleInput`:** `{ id: guid (telefon üretir), docNo (zorunlu, ≤ 20, görünen no "BS-4821"), customerId? (≤ 100;
telefonun cari kimliği), customerName? (≤ 200; boşsa "Perakende Müşteri"), warehouse? (≤ 100), note? (≤ 1000),
totalAmount (bilgi amaçlı, satır iskontolu toplam), lines: [{ barcode (zorunlu, ≤ 50), stockCode? (≤ 50), productName?
(≤ 200), quantity, price, lineDiscountPercent, note? (≤ 500) }] }`.

**`SuspendedSaleDto`:** girdideki alanlar + `deleted, closedReason ("claimed"|"deleted"), closedBy, createdBy (görünen ad),
createdByUserId (telefon "sil"i yalnız oluşturana gösterir), createdAtMs, updatedAtMs, updatedSeq`.

## Kullanıcı görünüm tercihleri — `/api/v1/android/account/preferences`

Oturumdaki kullanıcının kendi görünüm tercihleri (Katalog/Satış listesi varsayılanları, kart alanları, hızlı erişim tasarımı) tek JSON belge.
Kullanıcı ve firma her zaman jeton'dan gelir; gövdeden asla. Sunucu belgeyi opak tutar (şekli telefon belirler).

| Uç | Kim | Gövde / yanıt |
|---|---|---|
| `GET /preferences` | herkes | `{ version, updatedAtUtc?, data? }` — hiç kaydedilmediyse `version: 0`, `data: null` |
| `PUT /preferences` | herkes | gövde `{ "data": { … } }` — `data` JSON nesnesi olmalı, en çok 16 384 karakter; yanıt güncel `{ version, updatedAtUtc, data }`. Her kayıt `version`'u 1 artırır (belgenin tamamı yer değiştirir). |

Ret: `400 INVALID_PREFERENCES` (gövde nesne değil / `data` nesne değil / tavan aşıldı). Telefon yerelde bekleyen değişikliği kazandırır,
yoksa sunucudaki daha yüksek `version`'u alır; uç yoksa (eski sunucu, 404) tercihler yalnız telefonda kalır.

## Kullanıcı yetkileri — `/api/v1/android/account/…permissions` (GOAL_YETKILER)

Rol şablonu + kişiye istisna; telefon ve web panel aynı uçları kullanır. Değerler saklama biçimindedir: evet/hayır
`"1"`/`"0"`, limit sayı (`"10"`, `"25000"`), `""` = sınırsız. Bilgi bankası kural 33.

| Uç | Kim | Açıklama |
|---|---|---|
| `GET /permissions/catalog` | herkes | `{ version, groups: [{key,label}], items: [{key, group, type: bool\|limit, unit?, label, description, serverEnforced, locked, defaults: {rol: değer}}], editableRoles }` |
| `GET /roles/permissions` | Admin | `{ roles: [{ role, locked, values: {anahtar: değer}, customized: [anahtar] }] }` (ADMIN kilitli, her şey açık) |
| `PUT /roles/{role}/permissions` | Admin | `{ values: { anahtar: değer \| null } }` — `null` = katalog varsayılanı; adı geçmeyen anahtar değişmez. Yanıt güncel `RolePermissionsDto`. |
| `GET /users/{id}/permissions` | Admin; kişi kendisi | `{ userId, username, fullName, roles, locked, items: [{ key, value, source: admin\|role\|override, roleValue, roleSources, override }] }` |
| `PUT /users/{id}/permissions` | Admin | `{ overrides: { anahtar: "allow"\|"deny"\|"1"\|"0"\|limit\|"unlimited" \| null } }` — `null` = rollerden gelsin. |
| `GET /permissions/changes?userId=&role=&take=` | Admin | `{ changes: [{ id, actorName, client, scope, role?, targetUserId?, targetUserName?, key, oldValue?, newValue?, createdAtUtc }] }` |

Ret kodları: `ADMIN_REQUIRED` (403), `ADMIN_ROLE_LOCKED`, `ADMIN_USER_LOCKED`, `PERMISSION_LOCKED` (409),
`UNKNOWN_PERMISSION`, `INVALID_PERMISSION_VALUE` (400; limit 0 ve üstü, yüzde en çok 100).

Oturum (`/login`, `/me`): `permissions: {anahtar: bool}`, `limits: {anahtar: sayı \| null}`, `permissionsVersion` (0 = yetkisiz
eski sunucu), `permissionsStamp` (roller + yetkilerin 16 haneli özeti). Her imzalı mobil yanıt güncel damgayı
`X-Permissions-Stamp` başlığında taşır; telefon farklı damga görünce `/me`'yi yeniden okur (yeniden giriş gerekmez).
Ingest: mobil kullanıcının doğrudan belgesi modül yetkisi yoksa, açık hesap (cari borç) satış yetkisi olmadan cari borçlu satışsa ya da limit aşılırsa
`409 APPROVAL_REQUIRED` (mesaj nedeni söyler); belge onay talebi olarak yeniden gönderilir.

## Hedefler ve ekipler — `/api/v1/portal/{teams,targets}`, `/api/v1/android/targets` (GOAL_HEDEF_RUT)

Firma kullanıcısı token'ı; firma token'dan. Günler İstanbul `yyyy-MM-dd`. Ayrıntı: KB 00 kural 31.

| Uç | Gövde / yanıt |
|---|---|
| `GET /portal/teams` | `{teams:[{id,name,kind,parentId,isActive,memberIds,managerIds}], people:[{id,username,fullName,roles,teamId,isActive}], canEdit, wholeCompany}` |
| `POST /portal/teams` · `PUT /portal/teams/{id}` | `{name, kind (yalnız oluştururken), parentId, isActive, memberIds?, managerIds?}` → `TeamDto` (201/200). `name` ve `parentId` her seferinde tam gönderilir (bölgesiz gönderilen ekip bölgeden çıkar); `memberIds`/`managerIds` null ise dokunulmaz. Hatalar `TEAM_ADMIN_REQUIRED` 403, `TEAM_NOT_FOUND` 404, `TEAM_NAME_TAKEN` 409, `TEAM_INVALID` 400 |
| `DELETE /portal/teams/{id}` | 204; altında ekip olan bölge 409 `TEAM_HAS_CHILDREN` |
| `GET /portal/targets?periodType&periodKey&teamId` | Pano: `{periodType, periodKey, start, end, asOf, workDaysTotal/Elapsed/Left, dataSource, source, warnings[], canManage, wholeCompany, owners:[{ownerKind, ownerId, name, teamId, teamName, teamKind, warning, summary:{revenue, returns, collection, documentCount, visitsPlanned, visitsCompleted, pendingRevenue, pendingCollection}, targets:[{id, metric, measure, itemCode, itemName, value, note, actual, pending, percent, expectedToDate, forecast, requiredPerDay, derived, childrenSum, updatedAtMs, updatedBy}]}]}`. Varsayılan bu ay |
| `PUT /portal/targets` | `{operationId, items:[{periodType, periodKey, metric, measure?, itemCode?, itemName?, ownerKind, ownerId?, value (null = sil), note?}]}` ≤ 2000 → `{saved, deleted, unchanged, duplicate, errors:[{index, errorCode, message}]}`; hatada 400 (kapsam dışı 403), hiçbiri yazılmaz |
| `POST /portal/targets/copy` | `{periodType, fromPeriodKey, toPeriodKey, percent, ownerKinds?, overwrite, apply, operationId}` → `{items, result?}` |
| `POST /portal/targets/distribute` | `{periodType, periodKey, metric, measure?, itemCode?, itemName?, sourceOwnerKind (COMPANY\|TEAM), sourceOwnerId?, value?, method (EQUAL\|LAST_PERIOD_SHARE), targetLevel (USER\|TEAM)}` → `{items}` (önizleme) |
| `GET /portal/targets/items?metric&q` | `PRODUCT\|CATEGORY\|SUB_CATEGORY\|BRAND` → `{items:[{code,name,productCount}], truncated}` |
| `GET\|PUT /portal/targets/settings` | `{workDays}` (Pzt=1…Paz=64); yazma tüm firmayı gören yönetici |
| `GET /android/targets/mine?date` | `{date, asOfMs, dataSource, source, warnings[], canViewTeam, periods:[{periodType, periodKey, start, end, workDaysTotal, workDaysLeft, summary, targets}]}` — gün, hafta, ay |
| `GET /android/targets/team?date&periodType` | Pano biçimi; yalnız ADMIN/MANAGER (403 `TARGETS_REQUIRE_MANAGER`) |

## Müşteri kataloğu yönetimi — `/api/v1/customer-catalog` (GOAL_MUSTERI_KATALOGU §5.1)

Telefon ve panel ortak; firma kullanıcısı token'ı, hız sınırı kullanıcı başına. Gövde/yanıt alanları sözleşme belgesindedir
(`docs/GOAL_MUSTERI_KATALOGU.md` §5.1); burada sunucunun seçtiği ayrıntılar ve hata kodları var. Ayrıntı: KB 00 kural 36.

- **Sıra:** önce modül (`403 MODULE_NOT_ENABLED`, herkese aynı), sonra kilitli `action.customer_catalog.manage`
  (`403 CATALOG_MANAGE_REQUIRED`; yalnız ADMIN + MANAGER). Gövde eksik/bozuk `400 INVALID_BODY`.
- **Düzen yazımları** (`PUT settings|categories|products`) `catalog_settings.Revision`'ı bir
  artırır; gövdedeki `revision` eskiyse `409 CATALOG_CHANGED`, hiçbir şey yazılmaz. Hiçbir şeyi değiştirmeyen yazım
  revizyonu artırmaz (aynı bağlantıları yeniden gönderen telefon panelin sonraki kaydını çakışmaya düşürmez). `PUT settings` bilinmeyen liste
  `400 UNKNOWN_PRICE_LIST`.
- **`PUT categories`:** dizi sırası = kategori sırası (`sortOrder` = dizin); listede olmayan kategori yerini kaybeder, gizliliği
  kalır (gizli değilse satırı silinir). Anahtar dolu, ≤ 160, tekrarsız.
- **`GET products?category=&q=`:** `category` kategorinin tamamı (≤ 5000), `q` ad/kod/marka/barkod tr-TR harf duyarsız (≤ 50),
  ikisi birlikte kategori içinde arar; hiçbiri yoksa tüm katalog (≤ 5000). `truncated` fazlası olduğunu söyler. `listPrice`
  firmanın etkin varsayılan listesinden; `cartonOnly` geçerli olanı (istenmiş ve etkin koli ≥ 2), `cartonQuantity` firmanın
  kendi kolisi.
- **`PUT products`:** ≤ 5000; verilen her ürünün tüm ayarları yazılır, hepsi varsayılana dönen ürünün satırı silinir.
  `cartonQuantity` 2–100000 (`400 INVALID_CARTON_QUANTITY`); etkin koli yokken `cartonOnly` `400 CARTON_QUANTITY_REQUIRED`.
- **Hesaplar:** cari `PortalLedger` carilerinden doğrulanır (`404 CUSTOMER_NOT_FOUND`), hesap kartın kodunu saklar. Kullanıcı
  adı personelinki gibi normalize edilir (`" A.B "` → `a.b`, `400 INVALID_USERNAME`). Canlı hesaplarda ad tekrarı
  `409 CATALOG_USERNAME_TAKEN`, aynı cariye ikinci hesap `409 CATALOG_ACCOUNT_EXISTS`. Şifre boşsa sunucu 10 karakter üretir
  (`a–z` ve `2–9`, karışan `i l o 0 1` yok) ve yalnız o yanıtta `issuedPassword` döner; elle şifre 8–72 bayt
  (`400 INVALID_PASSWORD`). İskonto 0–99,99, iki haneye yuvarlanır (`400 INVALID_DISCOUNT`); bilinmeyen `priceListNo`
  `400 UNKNOWN_PRICE_LIST`; görünürlük modu `all|only`, kural `category|product` × `allow|deny`, ≤ 2000
  (`400 INVALID_VISIBILITY`); `responsibleUserId` firmanın aktif kullanıcısı (`400 INVALID_RESPONSIBLE_USER`). Bilinmeyen ya
  da silinmiş hesap `404 CATALOG_ACCOUNT_NOT_FOUND`.
- **`PATCH accounts/{id}`:** yalnız gönderilen alanlar değişir; `priceListNo` / `responsibleUserId` açıkça `null` gönderilirse
  temizlenir; `customerCode`/`password` yok sayılır. `TokenVersion` +1 — veritabanında atomik (`TokenVersion + 1`), eşzamanlı iki
  iptal ikisi de sayılır: pasifleştirme, `PUT password`, `revoke-sessions`, `DELETE` (yumuşak; ad ve cari serbest kalır). Eski
  sürümün oturumları ve cihaz çerezleri geçersiz olur.
- **`GET accounts?q=&page=`:** 50'lik sayfa, cari adına (tr-TR) göre; `q` kod/ad/kullanıcı adında arar.
- **`GET accounts/by-customer?code=`:** `suggestedUsername` cari adından (Türkçe harfler ASCII'ye, şirket ekleri ve tek harfler
  atılır, kelimeler `-` ile, ≤ 24), olmazsa koddan, olmazsa `musteri`; kullanılıyorsa sonuna 2, 3… eklenir.
  `notifyPreview {userId, userName, source}`: carinin yeni talebi şu an kime atanır (hesabın kayıtlı sorumlusuyla; S11).
  `source` = `responsible` | `salesperson` (carinin temsilci kodu) | `address` (adres temsilcisi) | `default` (firma varsayılan
  temsilcisi) | `route` (aktif rut planı) | `managersOnly` (kimse; `userId`/`userName` null). Yöneticiler her talepte ayrıca
  bildirim alır.
- **Görseller** (`images/*`; yükleme uçları `catalog-upload` hız sınırında: kullanıcı başına 300/dk, oturumsuz istek IP kovasına).
  Her görsel değişikliği **görsel sayacını** (`catalog_settings.ImageRevision`) artırır, düzen revizyonunu değil: panelde ya da
  telefonda açık bir düzen düzenlemesi görsel yüklemesi yüzünden `409 CATALOG_CHANGED` almaz; katalog görünümü iki sayaca birden
  bakarak tazelenir. Görsel yazımları yine `catalog_settings` satır kilidinden geçer. Ürün başına en çok 8 (`409 CATALOG_IMAGE_LIMIT`);
  baytlar merkezi dosya deposuna (R2, aşağıda "Merkezi dosya deposu") gider ve **firmanın tek depolama kotasına** sayılır: kota
  aşımında eski kod korunur, gövdeye rakamlar eklenir (`413 CATALOG_IMAGE_QUOTA_EXCEEDED {usedBytes, quotaBytes}`), depo yoksa
  `503 STORAGE_UNAVAILABLE`; bulunamayan ya da başka firmanın görseli `404 CATALOG_IMAGE_NOT_FOUND`. `GET images/manifest` ve
  `GET settings`'teki `usedBytes`/`limitBytes` (`imageQuota`) firmanın birleşik kotasıdır (bütün alanlar, çöp dahil).
  - `POST images {stockCode, sourceHash, source, url?}`: kimliği sunucu üretir; aynı (`stockCode`, `sourceHash`) var olanı döner
    (sınırı aşmaz). `source` `phone|panel`; `url` verilirse bağlantı görseli olur (panelin "bağlantı ekle"si).
  - `PUT images/{id}/{s|l}` ham gövde: `Content-Type` `image/jpeg|png|webp` ve ilk baytlar tutmalı (`415 INVALID_IMAGE`);
    `l` ≤ 1 MB, `s` ≤ 200 KB (`413 IMAGE_TOO_LARGE`; `Content-Length` yoksa okurken). Üst veri atılır (kütüphanesiz): JPEG APP1
    (EXIF/XMP), PNG `eXIf`/`tEXt`/`iTXt`/`zTXt`, WebP `EXIF`/`XMP ` (VP8X bayrakları ve RIFF boyu düzeltilir); okunamayan dosya
    olduğu gibi saklanır. JPEG yön bilgisi EXIF'le gider: telefon ve panel görseli zaten döndürüp yeniden kodlayarak gönderir.
    Aynı bayt tekrar → değişiklik yok. Bağlantı görseline dosya `400 INVALID_BODY`. Bayt R2'ye yazılır (alan `catalog`; stok
    kodu `~banner` ise `banner`), sonra görsele bağlanır; değiştirilen boyutun eski dosyası (ya da PostgreSQL'deki eski baytı)
    **kalıcı silinir** — yeni bayt için önce yer gerekir. Görsel silinirse (`DELETE images/{id}`, banner'ın kullanılmayan görseli)
    dosyaları **çöp kutusuna** gider (7 gün geri alınabilir, o süre kotaya sayılır).
  - `PUT images/links` (≤ 500 ürün): verilen ürünün yalnız `phone` kaynaklı bağlantılarını değiştirir; dosyalar ve panel
    görselleri kalır, sınırı aşan bağlantı alınmaz; `updated` = bağlantıları değişen ürün sayısı. Bağlantı: `https`, port 443,
    IP/`localhost`/`.local` yok, ≤ 2048 (`400 INVALID_IMAGE_URL`). Sunucu bağlantıyı **indirmez**.
  - `PUT images/order?stockCode=` `{ids}`: verilenler bu sırada, verilmeyenler eski sıralarıyla arkadan.
  - `thumbUrl`/`fullUrl`: depodaki boyut için tam CDN adresi `https://img.appsgo.cloud/{FIRMAKODU}/catalog/…-{s|l}.{uzantı}`
    (yeni bayt = yeni adres); depodan önce yüklenmiş boyut için göreli `/api/v1/catalog/img/{id}/{s|l}?h={sha256 ilk 8}`
    (istemci kendi kökünü ekler; S10 göçüne kadar); bağlantıda doğrudan adres; boyut henüz yüklenmemişse null. Panel ve telefon
    tam https adresini olduğu gibi kullanır.
- **Anonim görsel `GET /api/v1/catalog/img/{id}/{s|l}`** (`catalog-public`: IP başına 600/dk, IPv6 /64): `Cache-Control: public,
  max-age=31536000, immutable`, `ETag` = SHA-256, `If-None-Match` → 304 (bayt okunmaz), `X-Content-Type-Options: nosniff`,
  `Cross-Origin-Resource-Policy: same-site`. `s` yoksa `l`; bağlantı görseli, bilinmeyen kimlik, firma pasif ya da modülü kapalıysa
  `404 NOT_FOUND` (önbellek başlıksız). Firmanın "yayında" anahtarı (`IsEnabled`) **sorulmaz**: panel ve telefon katalog
  yayına alınmadan, hazırlanırken görselleri bu adresten gösterir. Boyut merkezi depodaysa bayt okunmaz: aynı denetimlerden
  sonra `302` → CDN adresi (`Cache-Control: public, max-age=3600`; eski adresi tutan istemci için).
- **Bannerlar (`banners*`, S12)** `Endpoints/CustomerCatalogBannerEndpoints`: modül + yönetim yetkisi (diğer yönetim uçları gibi).
  Yazımlar görsel kilidinden geçer (`WriteLayoutAsync(..., pictures: true)`): `ImageRevision` artar, düzen `revision`'ı **artmaz**
  (açık düzen düzenlemesi 409 almaz). Sürüm/çakışma denetimi yok (son yazan kazanır).
  - `GET banners` → `{items[Banner]}` sırayla (`sortOrder`, sonra oluşturma); `Banner {id, title, text, imageId, image: Image|null,
    linkType, linkValue, linkName, sortOrder, isActive, startsAtMs, endsAtMs, live, createdAtMs, updatedAtMs}`. `linkName` bağlantı
    verilen kategorinin/ürünün bugünkü adı (yoksa null), `live` = aktif ve şu an tarih aralığında.
  - `POST banners` → `201 Banner`; `PUT banners/{id}` → `Banner` (gövde her alanı taşır, PUT hepsini değiştirir):
    `{title, text, imageId?, linkType, linkValue, isActive, startsAtMs?, endsAtMs?}`. Başlık ≤ 120, metin ≤ 300 (kırpılır;
    aşan `400 INVALID_BODY`); başlık ya da görsel zorunlu (`400 INVALID_BODY`). `linkType` `none|category|product|url` (harf
    büyüklüğü fark etmez): `url` görsel bağlantısı kuralıyla (`https`, 443, IP/yerel yok, ≤ 2048), `category` katalogdaki kategori
    anahtarı, `product` katalogdaki stok kodu (kartın kodu saklanır) — değilse `400 INVALID_BANNER_LINK`. `endsAtMs ≤ startsAtMs`
    `400 INVALID_BANNER_DATES`; bitiş **hariçtir** (panel son günün ertesi gününün İstanbul başlangıcını gönderir). `imageId`
    firmanın `~banner` görseli olmalı (`400 INVALID_BANNER_IMAGE`). En çok 20 banner (`409 CATALOG_BANNER_LIMIT`). PUT'ta artık
    kullanılmayan eski görsel başka banner göstermiyorsa silinir.
  - `DELETE banners/{id}` → 204; görseli (ve iki boyutu) başka banner göstermiyorsa birlikte silinir. `PUT banners/order {ids}` →
    204: verilenler bu sırada, verilmeyenler eski sıralarıyla arkadan; tekrar eden kimlik `400 INVALID_BODY`. Bilinmeyen ya da
    başka firmanın banner'ı `404 CATALOG_BANNER_NOT_FOUND`.
  - **Görsel:** banner görseli sıradan katalog görselidir, `POST images {stockCode: "~banner", sourceHash, source, url?}` ile
    kaydedilir ve boyutları aynı `PUT images/{id}/{s|l}` ile gider (aynı bayt sınırları, sihirli bayt, üst veri temizliği, kota,
    anonim adres). `~` ile başlayan kod ürün değildir: ürün başına 8 sınırı yerine banner görselleri için firma başına 40
    (`409 CATALOG_IMAGE_LIMIT`); `~banner` dışındaki `~` kodları, `PUT images/links` ve `PUT images/order` için `~` kodları
    `400 INVALID_BODY`; `GET images/manifest` ve katalog görünümü (ürün `imageCount`/`thumbUrl`, müşteri `thumb`) bunları
    göstermez, `usedBytes` (kota) sayar. Yeni `~banner` kaydında, bir günden eski ve hiçbir bannerın kullanmadığı banner görselleri
    (kaydedilmemiş düzenlemeden kalan) silinir. Görsel images API'siyle silinirse banner görselsiz kalır (FK `SET NULL`). Panel
    banner görselini 1920 × 720 (`l`, ≤ 1 MB) ve 800 × 300 (`s`) kutusuna sığdırarak gönderir.

- **Talepler (`orders*`, S8)** `Endpoints/CustomerCatalogOrderEndpoints`: modül denetlenir, yönetim yetkisi **istenmez**;
  katalog yöneticisi (ADMIN/MANAGER) hepsini, diğerleri yalnız `AssignedUserId` ya da `ClaimedByUserId` kendisi olanları görür
  (görmediği talep `404 CATALOG_ORDER_NOT_FOUND`). `GET orders?status=&q=&page=`: yeniden eskiye, 50'lik sayfa; `status`
  `NEW|CLAIMED|COMPLETED|REJECTED` (başkası `400 INVALID_BODY`); `q` talep no / cari kodu / cari adı (tr-TR); `counts` görülebilen
  bütün taleplerin durum sayıları (süzgeçsiz). `GET orders/counts` yalnız bu sayıları döner (veritabanında sayılır; panel
  menüsündeki "yeni talep" rozeti). `assignedUserName` = talebin düştüğü kişi. Değiştirici uçlar talebin satır kilidi
  altında çalışır (aynı anda iki kişi: biri 200, öbürü 409) ve talebin son hâlini (`OrderDetail`) döner:
  `claim {force}` — kapalı `409 CATALOG_ORDER_CLOSED`, başkasında `409 CATALOG_ORDER_TAKEN`, kendisininkini yeniden almak
  değişiklik yapmaz; `force` yalnız yönetici (`403 CATALOG_MANAGE_REQUIRED`). `release` — alan kişi ya da yönetici; `NEW` ise
  değişiklik yok. `complete {documentRef?}` (≤ 128; boş = "başka yerde girildi") — aynı belgeyle (ya da belgesiz) tekrar değişiklik
  yapmaz, başka belge `409 CATALOG_ORDER_ALREADY_CONVERTED`, reddedilmiş `409 CATALOG_ORDER_CLOSED`. `reject {reason}` — gerekçe
  zorunlu (`400 INVALID_BODY`, ≤ 500'e kırpılır), tekrar değişiklik yapmaz, çevrilmiş `409 CATALOG_ORDER_CLOSED`. Başkasının
  aldığı talebi yönetici olmayan bırakamaz/çeviremez/reddedemez (`409 CATALOG_ORDER_TAKEN`). `reopen` — yalnız katalog
  yöneticisi (`403 CATALOG_MANAGE_REQUIRED`): `COMPLETED`/`REJECTED` → `NEW`; `documentRef`, `rejectReason`, `closedBy*` ve
  `claimedBy*` temizlenir (sonraki satış talebe ilk günkü gibi bağlanır); açık talepte değişiklik yok. Panel bunu onaylı uyarıyla
  sunar ("Yeniden aç"). Detay satırları talebin anlık
  fiyatlarıdır (`listPrice` talebin listesinden, `discountPercent` satırın müşteri iskontosu); `inStockNow` bugünkü stok.
- **Talep ↔ satış bağı** (`CustomerCatalog/CatalogOrderLinker`, T8): yalnız `documentType = sales_order` gövdesi (`POST
  /api/v1/ingest/jobs`, ya da onay isteğinin belgesi) üst düzeyde `catalogOrderId` taşıyorsa — başka belge türündeki alan yok
  sayılır —, iş yazılmadan hemen önce (idempotent iş ve onay/yetki denetimlerinden sonra; onayda karar anında) talep aynı firmada
  ve `NEW`/`CLAIMED` olmalı → işle aynı kayıtta `COMPLETED`, `documentRef` = satışın `externalId`'si, `closedBy*` = gönderen.
  Aynı `externalId` tekrar → sorun yok. Başkasının `CLAIMED` talebini, gönderen o kişi ya da katalog yöneticisi değilse
  `409 CATALOG_ORDER_TAKEN` (personel `complete`'iyle aynı kural). Başka belgeyle çevrilmiş `409 CATALOG_ORDER_ALREADY_CONVERTED`
  — **ancak** o belgenin işi (`TenantId` + `ExternalId`, `sales_order`) `Failed`/`DeadLetter` ise düzeltilmiş satış talebi
  devralır; iş olmayan referans (personelin "başka yerde girildi" numarası) kalıcıdır, onu yalnız `reopen` açar. Reddedilmiş
  `409 CATALOG_ORDER_CLOSED`, bilinmeyen/başka firmanın/kimlik olmayan değer `409 CATALOG_ORDER_NOT_FOUND` — reddedilen her
  durumda **iş yazılmaz** (onayda onay `Pending` kalır). Alan yoksa ya da `null` ise davranış aynen eskisi. ERP'siz firmada
  defter satışı reddederse (iş `Failed`) talep önceki hâline döner (açık ya da devraldığı başarısız satışa bağlı). Reddedilen
  onay hiç iş yazmadığı için talebe dokunmaz.
- **Panelden siparişe çevirme** (S10, `CustomerCatalog/CatalogOrderConversion` + `Jobs/SalesJobWriter`): panel belge
  göndermez (`PORTAL_CANNOT_SUBMIT_DOCUMENTS` aynen); belgeyi sunucu talepten kendisi kurar.
  `GET orders/{id}/conversion` (talebi gören herkes) → `Conversion {orderId, no, status, customerCode, customerName, ownerUserId,
  ownerName, ownerIsAssignee, priceListNo, priceListName, priceIncludesVat, lines[{stockCode, name, unit, quantity,
  orderedListPrice, listPrice?, discountPercent, vatRate, orderedTotal, total, priceChanged, issue?}], orderedTotal, total,
  priceChanged, erp, defaultWarehouseNo?, warehouses[{code, name}], missingMappings[], requiresApproval}`. Satırlar talebin
  listesinin **bugünkü** fiyatıyla, talepteki cari iskontosuyla (`noDiscount` ürün 0) `CatalogPricing` ile fiyatlanır;
  ürün yoksa, listede fiyatı yoksa ya da hesabın görünürlüğünden çıktıysa `issue = NOT_AVAILABLE`. Belgenin sahibi talebin
  `AssignedUserId`'si (aktif kullanıcıysa), yoksa çağıran (`ownerIsAssignee = false`). ERP'li firmada `defaultWarehouseNo` =
  sahibin eşlemesi → firma ayarı (`ErpWriteContextBuilder`), `warehouses` = ERP'nin `lookups` depoları, `missingMappings` ⊂
  {"ERP kullanıcı numarası", "depo", "satış belge türü"} (ajanın `ERP_MAPPING_MISSING` sebepleri). `requiresApproval` = çağıran
  satışa karar veremiyor ve firma kuralı ya da kendi yetki/limiti onaya gönderiyor.
  `POST orders/{id}/convert {warehouseNo?, expectedTotal}` → `201 {outcome: JOB|APPROVAL, documentRef, jobId?, jobStatus?,
  approvalRequestId?, order: OrderDetail}`. Erişim: katalog yöneticisi ya da talebi gören ve başkası almamış kişi (başkasının
  `CLAIMED` talebi yönetici değilse `409 CATALOG_ORDER_TAKEN`); `NEW` talep önce çağıran adına alınır. Sıra: `expectedTotal`
  yok / `warehouseNo` ≤ 0 `400 INVALID_BODY`; satılamayan satır `422 CART_INVALID`; toplam farkı > 0,05 `409 PRICE_CHANGED`;
  eşleme eksik `409 ERP_MAPPING_MISSING` (üçü de gövdede güncel `conversion` taşır, hiçbir şey yazılmaz). Gövde telefonun
  `salesOrderPayload`'ı alan alan: `mobileDocumentId` = `externalId` = `CAT-SO-{Guid}` (her çevirmede yeni), `revision` 1,
  `occurredAt` İstanbul saatiyle `dd.MM.yyyy HH:mm`, `transactionType` "Satış", `counterparty`, `customerCode`, `amount`
  (KDV dahil toplam), `currency` "TL", `paymentType` "Cari Borç", `description` "Katalog siparişi KT-…" (+ `\n[Notlar: …]`),
  `catalogOrderId`, `priceListNo` = talebin listesi, `warehouseNo` yalnız varsayılandan farklı seçildiyse (ERP'li firmada);
  satırlar `barcode` (varsa), `productCode`, `productTitle`, `quantity`, `unitPrice` (KDV hariç net / miktar), `lineTotal`
  (KDV hariç net), `unitPointer` 1, `listUnitPrice` (bugünkü), `lineDiscountPercent` 0, `customerDiscountPercent`,
  `generalDiscountPercent` 0. İşin `CreatedByUserId`'si belgenin sahibidir (ajan depo, seri, temsilci ve ERP kullanıcı
  numarasını onun eşlemesinden alır); talebi kapatan (`closedBy*`) çağırandır. Çağıran satışa karar verebiliyorsa (ya da onay
  gerekmiyorsa) iş `SalesJobWriter.WriteAsync` ile yazılır (ERP'siz firmada hemen deftere işlenir; defter reddederse
  `422 CATALOG_CONVERSION_FAILED`, talep açık kalır); değilse `ApprovalService.SubmitAsync` ile `CAT-APR-{Guid}` onay talebi
  açılır (`kind` sale, belge aynı gövde) ve talep onay anında kapanır — bu yolda işin sahibi onay isteyendir (çağıran).
  Aynı talebe ikinci belge linker'dan `409 CATALOG_ORDER_ALREADY_CONVERTED`.

## Müşteri kataloğu, müşteri tarafı — `/api/v1/catalog/{code}` (GOAL_MUSTERI_KATALOGU §5.2, §6, §7)

Firmanın carileri için; `{code}` firma kodu (`^[A-Za-z0-9]{4,16}$`, harf büyüklüğü fark etmez). Alanlar sözleşme belgesinde;
burada sunucunun seçtiği ayrıntılar. Ayrıntı: KB 00 kural 36.

- **Oturum:** JWT gövdede dönmez; `__Host-kt_{KOD}` çerezi (HttpOnly, Secure, SameSite=Strict, `Path=/`, Domain yok).
  "Beni hatırla" (`remember`) → `Max-Age` 30 gün, yoksa oturum çerezi + 12 saatlik token. Token: `sub` hesap, `tenant`,
  `scope=customer-catalog`, `tv` (hesabın `TokenVersion`'ı), `jti`. JwtBearer çerezi yalnız `/api/v1/catalog/{code}/…` yolunda,
  `Authorization` başlığı yokken ve yoldaki kodun çerezinden okur. Bu kapsam başka hiçbir politikadan geçmez (personel uçlarında
  403); personel token'ı katalogda `401 INVALID_TOKEN`.
- **Her istekte** (`CatalogCustomerPolicy` + `CatalogAccountStateRequirement`): token katalog token'ı değil, firması yok ya da
  yoldaki kod token'ın firmasının değil → `401 INVALID_TOKEN`; hesap silinmiş ya da `tv` eski → `401 SESSION_REVOKED`; hesap
  pasif → `403 ACCOUNT_INACTIVE`; firma pasif, modül kapalı ya da `IsEnabled=false` → `403 CATALOG_UNAVAILABLE`; abonelik
  → `403 SUBSCRIPTION_REQUIRED|SUBSCRIPTION_EXPIRED`. Gövde `ApiError` (Türkçe mesaj). Hız: hesap başına 120/dk (`per-catalog-account`;
  oturumsuz istek — süresi geçmiş ya da hiç olmayan çerez — ortak bir kovaya değil istemcinin IP kovasına sayılır).
- **CSRF:** her değiştirici istek (giriş dahil) `X-Katalog: 1` taşımalı; tarayıcı `Origin` gönderiyorsa `https://{PublicHost}`
  ile birebir aynı olmalı → yoksa `403 CSRF_REJECTED`. `PublicHost` boşken yalnız başlık aranır.
- **Önbellek:** bu gruptaki her yanıt `Cache-Control: private, no-store`.
- **`GET info`** (anonim, `catalog-public`): `{companyName, code}`; firma yok/pasif, modül kapalı ya da yayında değil →
  `404 CATALOG_NOT_FOUND`.
- **`POST login`** (anonim, `catalog-login`: IP başına 10/dk) `{username, password, remember}` → `{me}` + çerezler. Eksik alan
  `400 INVALID_REQUEST`. Sıra: cihaz çerezinin imzası (veritabanısız) → çerez yoksa firma başına 300/dk kova (`CatalogLoginGate`,
  aşılırsa `429 RATE_LIMITED`) → hesap okunur; çerez başka hesabın ya da eski `TokenVersion`'ın ise kova yine uygulanır → ad
  yavaşlatıcısı (`LoginThrottle` alan `catalog`; beklerken şifre denetlenmez, `429`) → BCrypt (hesap yoksa ortak sahte hash;
  aynı anda en çok 8) → bilinmeyen firma/kullanıcı/yanlış şifre tek cevap `401 INVALID_CREDENTIALS` → ancak şifre doğruysa
  `403 ACCOUNT_INACTIVE`, `403 CATALOG_UNAVAILABLE`, `403 SUBSCRIPTION_*`. Başarı `LastLoginAtMs` yazar ve imzalı
  **cihaz çerezi** `__Host-kt_dev` (180 gün; hesap kimliği + `TokenVersion` + bitiş, `Jwt:SigningKey`'den türetilmiş HMAC)
  verir: bu çerezi o hesabın **güncel** sürümüyle taşıyan tarayıcı firma kovasını harcamaz ve başkalarının hatalarıyla
  yavaşlamaz (sayacı hesabın kendisidir, `#{hesapId}`). Şifre değişikliği / oturum iptali / pasifleştirme eski çerezleri düşürür.
- **`POST logout`** → 204, yalnız bu tarayıcının oturum çerezini siler (cihaz çerezi kalır).
- **`GET me`:** `priceList` etkin liste (`account.PriceListNo ?? varsayılan`), firmada hiç fiyat yoksa null; `balance` yalnız
  `features.statement` açıkken (panelin gösterdiği bakiye: ERP'li firmada hareket toplamı), cari aynada yoksa null.
- **`POST password {current, next}`** → 204: yanlış `current` `400 INVALID_CREDENTIALS` (yavaşlatıcıya **hesap** anahtarıyla
  `#{hesapId}` sayılır — oturum zaten kanıt; giriş sayfasında adı deneyen yabancı müşterinin şifre değiştirmesini engelleyemez),
  `next` 8–72 bayt değilse `400 INVALID_PASSWORD`. `TokenVersion` +1 (atomik; diğer bütün oturumlar ve cihaz çerezleri düşer);
  bu tarayıcıya aynı türde (hatırlanan/oturum) yeni oturum çerezi ve yeni sürümde cihaz çerezi verilir.
- **Görünürlük ve fiyat** (`CatalogCustomerView`): ürün, hesabın görünürlüğü izin veriyorsa ve hesabın etkin listesinde
  fiyatı varsa görünür (başka listeye düşülmez). `price {list, net, discountPercent, includesVat}`: `discountPercent` hesabın
  iskontosu, `noDiscount` üründe 0; `net = R2(list × (1 − d/100))`; `includesVat` listenin. `box {qty, only}` etkin koli ≥ 2
  ise, yoksa null. `thumb` ilk görsel (CDN adresi, eski görselde göreli `/api/v1/catalog/img/…` ya da https bağlantı).
- **`GET categories`:** yalnız görünür ürünü olan kategoriler, katalog sırasında; `id` = SHA-256(kategori anahtarı) ilk 12 hex.
- **`GET products?category=&q=&page=&pageSize=`:** `pageSize` varsayılan 48, 1–60'a kırpılır; `page` ≥ 1. `category` bilinmeyen
  kimlik → boş liste. `q` kırpılır; 2 karakterden kısaysa yok sayılır; ad/kod/marka tr-TR harf ve şapka duyarsız
  (`IgnoreCase | IgnoreNonSpace`; ç ğ ı ö ş ü ayrı harf kalır), barkod içerir.
- **`GET banners`** (S12) → `{items[{id, title, text, image: {thumb, full}|null, link: {type, value, categoryId?, productKey?}|null}]}`:
  yalnız aktif ve şu an tarih aralığında olanlar (`startsAtMs ≤ şimdi < endsAtMs`), sırayla; banner yoksa boş liste. Bağlantı
  müşterinin görünürlüğüne göre süzülür: görmediği ürüne ya da hiç görünür ürünü olmayan kategoriye bağlantı `link: null` olur
  (banner kalır). `category` → `categoryId` (`products?category=`'nin aldığı), `product` → `productKey`, `url` → https adres.
  Görseli gitmiş (ya da baytı hiç yüklenmemiş) ve başlığı olmayan banner listelenmez.
- **`GET products/detail?key=`** (`key` = stok kodu, harf büyüklüğü fark etmez): görünmeyen ya da olmayan ürün
  `404 NOT_FOUND`; `images[{thumb, full}]` sıralı.
- **`POST cart/quote {lines[{key, quantity}]}`:** `lines` yok `400 INVALID_BODY`, 200'den fazla satır `400 INVALID_BODY`.
  Satır başına sunucu fiyatı (`CatalogPricing`, telefonun `ErpSalePricing`'i); `issue`: görünmeyen/olmayan → `NOT_AVAILABLE`
  (ürün hakkında hiçbir bilgi dönmez: `code/name/unit/box/price/vatRate` null, tutarlar 0); miktar tam sayı değil, ≤ 0 ya da
  100000'den büyük → `INVALID_QUANTITY` (tutarlar 0); stokta yok → `OUT_OF_STOCK`; yalnız-koli üründe koli katı değil →
  `CARTON_MULTIPLE` (bu ikisi fiyatlanır). `totals` yalnız sorunsuz satırların toplamıdır.
- **`POST orders {requestId, lines[{key, quantity}], note, expectedTotal}`** (S8, `Endpoints/CatalogCustomerOrderEndpoints`):
  `requestId` boş, `lines` boş ya da 200'den fazla, `expectedTotal` yok, `note` 1000 karakterden uzun → `400 INVALID_BODY`.
  Sıra: aynı `requestId` bu hesabınsa aynı talep aynı `201 {order}` ile döner (başka hesabın/firmanın kimliğiyse içerik vermeden
  `409 REQUEST_ID_CONFLICT`) → `CanOrder` kapalı ya da cari kartı kilitli (`isLocked`) `403 ORDERING_DISABLED` → sepet sunucuda
  yeniden fiyatlanır (`cart/quote` ile aynı): sorunlu satır varsa `422 CART_INVALID {…, quote}`; toplam `expectedTotal`'dan
  0,05'ten fazla farklıysa `409 PRICE_CHANGED {…, quote}` → hesabın satır kilidi altında açık (`NEW`+`CLAIMED`) talep sayısı
  `MaxOpenOrders`'a (20) ulaştıysa `429 TOO_MANY_OPEN_ORDERS` (gövde `ApiError`, `Retry-After` yok) → talep `No` = `KT-` + 6 karakter
  (`A–Z` I/O hariç, `2–9`; firma içinde tekil), satırlar sunucunun fiyatıyla `LinesJson`'a, bildirimler aynı kayıtta (sayaç
  `ReserveAsync` ile kaydın hemen önünde) → `Publish(Tasks)`. Bildirim: `CATALOG_ORDER_NEW`, `TaskId` null, başlık
  "Yeni müşteri siparişi: {cari}", gövde "{No} · {n} kalem · {toplam} TL" (tr-TR); alıcılar `ResponsibleUserId` (aktifse) ya da
  carinin plasiyer kodu (`salespersonCode`, kırpılmış, harf duyarsız) → `MobileUserErpMapping.SalespersonCode` → aktif kullanıcı
  (bu kişi `AssignedUserId` olur), artı aktif katalog yöneticileri; herkese bir kez.
- **`GET orders`:** hesabın en yeni 100 talebi. **`GET orders/detail?id=`:** başkasının/bilinmeyen `404 NOT_FOUND`; satırlar
  `{key, code, name, quantity, net, total}` talebin fiyatıyla.
- **Hesabım (S9, `Endpoints/CatalogCustomerLedgerEndpoints`):** bayrak kapalıysa `403 FEATURE_DISABLED` (`statement` ←
  `ShowStatement`, `invoices*` ← `ShowInvoices`, `purchased` ← `ShowPurchased`). Kaynak panelin aynaları (`PortalLedger`); cari
  aynada yoksa boş. `from`/`to` `yyyy-MM-dd` (bozuksa `400 INVALID_BODY`).
  - `GET statement?from=&to=`: `balance` carinin bugünkü bakiyesi (`/me` ile aynı), `rows` yeniden eskiye, yalnız cari tarafı
    satırlar (kapalı peşin/kasa-banka satırları yok), iptal edilen özgün satır gizli (karşı kaydı görünür); **açıklama yok**.
  - `GET invoices?from=&to=&page=`: `Kind ∈ {sale, sale_return}`, `!OtherSide`, belge anahtarı olan satırlar (peşin kapanmış
    faturalar dahil), belge başına bir, yeniden eskiye, 50'lik sayfa; `total` = borç + alacak.
  - `GET invoices/detail?key=`: anahtar önce **aynı süzgeçten** geçen carinin kendi listesinde aranır (başka carinin anahtarı,
    cari koduyla çakışan kasa/banka satırının `r…` anahtarı, tahsilat → `404 NOT_FOUND`); `DocumentByKey` kullanılmaz. ERP'siz
    anahtar `d{CARİ}|{evrakNo}` `|` ve `/` içerebilir: sorguda URL-kodlu gider. Satır `productKey` yalnız ürün müşteriye
    görünüyorsa.
  - `GET purchased?q=&page=`: iptal edilmemiş satış faturalarının satırları stok koduna göre: `lastDate`, `totalQuantity`, `times`
    (fatura sayısı); `name` stok kartından (yoksa kod); `product` görünürse `CProduct`, yoksa null; `q` ≥ 2 karakter ad/kod
    (tr-TR, şapka duyarsız); son alıma göre, 48'lik sayfa.
- **Web barındırma** (`CustomerCatalog/CatalogWeb`, yalnız `Host == CustomerCatalog:PublicHost`; boşsa hiçbiri yok):
  `/assets/{v}/…` dosyalar (`CustomerCatalog:WebRoot`; `v` = bütün dosyaların SHA-256'sının ilk 10 hex'i, açılışta bir kez;
  `Cache-Control: public, max-age=31536000, immutable`; yanlış `v` ya da olmayan dosya `404 no-store`; yönlendirme ve hız
  sınırından önce). Kabuk `GET /{code}` ve `/{code}/{**rest}`: `index.html` (`%V%` → v, `%TITLE%` → "{Firma} · Müşteri
  Kataloğu", HTML-encode, 5 dk önbellek), `no-cache`; bilinmeyen/kapalı kodda aynı sayfa genel başlıkla `404`.
  `/robots.txt` (`Disallow: /`), `/favicon.svg`. Bu host'ta yalnız `/api/v1/catalog/**` ve `/health*` geçer, gerisi `404`.
  Her yanıtta CSP (`default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' https: data:; connect-src 'self';
  object-src 'none'; base-uri 'none'; frame-ancestors 'none'; form-action 'self'`), `X-Robots-Tag: noindex`,
  `Referrer-Policy: same-origin`, `Strict-Transport-Security: max-age=31536000`, `X-Content-Type-Options: nosniff`.

## Merkezi dosya deposu — `/api/v1/storage`, `/api/v1/admin/tenants/{id}/storage` (GOAL_DEPOLAMA_R2 S1–S9)

Firmanın kalıcı resimleri Cloudflare R2'de, firma kodunun klasöründe (`{FIRMAKODU}/{alan}/{yyyy}/{MM}/{id}-{varyant}.{uzantı}`)
ve tek kotayla durur. Alanlar: herkese açık kova `product`, `xml`, `catalog`, `banner`; kimliğe bağlı kova `task`, `expense`,
`vehicle`. Katalog görseli ve banner (S3), görev eki (S4) bu depoyu kullanır (uç yolları değişmedi); gider/araç fişi (S5) ve ürün
fotoğrafı (S6) yeni uçlardır; XML görselleri (S7) sunucunun kendi kopyasıdır. Bilgi bankası kural 37.

**Görev eki (S4)** — `PUT/GET/DELETE /api/v1/android/tasks/{taskId}/attachments/{attachmentId}` yolları, gövdeleri ve yanıtları aynı.
Resim özel kovaya (`task`) yazılır ve firmanın tek kotasına sayılır; aşımda eski kod `413 TASK_ATTACHMENT_QUOTA` döner, gövdeye
`usedBytes`/`quotaBytes` eklenir; depo yoksa `503 STORAGE_UNAVAILABLE` (telefon kuyruğu yeniden dener). `GET` yönlendirme
**yapmaz**: sunucu yetkiyi (görevi görme) denetleyip baytı R2'den akıtır, `Cache-Control: private, max-age=31536000, immutable`
(telefonun Bearer başlıklı Coil isteği ve disk önbelleği değişmeden çalışır). Depodan önce yüklenen resim aynı uçtan PostgreSQL'den
okunur. `DELETE` resmi çöpe atar (7 gün geri alınabilir, kotaya sayılır). Aynı dosya `GET /api/v1/storage/files/{id}` ile görevi
gören herkese 302 imzalı adres olarak da açılır.

**Gider ve araç fişi (S5)** — telefonun gider/araç bakım belgesine bağlı fiş fotoğrafı. `{docId}` belgenin telefon kimliği
(kasa defteri satırı `K-{uuid}`, belgede `mobileDocumentId`); belge sunucuya henüz gitmemiş olabilir.

| Uç | Kim | Gövde / yanıt |
|---|---|---|
| `PUT /api/v1/android/expenses/{docId}/attachments/{id}?kind=expense\|vehicle_maintenance` | firma kullanıcısı | Ham gövde `image/jpeg\|png\|webp` ≤ 2 MB; `{id}` telefonun ürettiği GUID (tekrar = aynı fiş, `200`). Yanıt `ExpenseAttachment { id, documentId, kind, contentType, sizeBytes, createdAtMs, createdByUserId, createdByName }`. `kind` yoksa `expense`. Hatalar: `400 INVALID_DOCUMENT_ID` (harf/rakam/`.-_:`, ≤ 128), `400 INVALID_EXPENSE_KIND`, `415 INVALID_IMAGE`, `413 EXPENSE_ATTACHMENT_TOO_LARGE`, `409 EXPENSE_ATTACHMENT_LIMIT` (belge başı 5), `409 EXPENSE_ATTACHMENT_EXISTS` (kimlik başka belgede ya da silinmiş), `403 EXPENSE_FORBIDDEN` (başkasının belgesi; yönetici/muhasebe hariç), `413 STORAGE_QUOTA_EXCEEDED {usedBytes, quotaBytes}`, `503 STORAGE_UNAVAILABLE` |
| `GET /api/v1/android/expenses/{docId}/attachments` | firma kullanıcısı | `{ items: [ExpenseAttachment] }` — görebildikleri, yükleme sırasıyla |
| `GET /api/v1/android/expenses/{docId}/attachments/{id}` | yükleyen; yönetici, muhasebe | Bayt (sunucu R2'den akıtır), `Cache-Control: private, max-age=31536000, immutable`; göremeyen `404 EXPENSE_ATTACHMENT_NOT_FOUND` |
| `DELETE /api/v1/android/expenses/{docId}/attachments/{id}` | yükleyen; `action.storage.manage` | `204`, fiş çöpe gider (tekrar `204`); görebilen ama silemeyen `403 EXPENSE_FORBIDDEN` |
| `GET /api/v1/portal/expense-receipts?from=&to=&documentId=` | yönetici, muhasebe (`portal.ledger`) | `{ from, to, items: [{ id, fileId, documentId, kind, contentType, sizeBytes, createdAtMs, createdByName, url, document: { type, status, amount, description, counterparty, occurredAt, expenseCardCode } \| null }], truncated }` — yükleme gününe göre (İstanbul, varsayılan son 30 gün, ≤ 92 gün, ≤ 500 fiş) ya da `documentId` (≤ 200) ile; `url` `Storage:PresignMinutes` dakikalık imzalı adres (depo ayarsızsa null); başkasına `403 PORTAL_REQUIRES_MANAGER` |

| Uç | Kim | Gövde / yanıt |
|---|---|---|
| `GET /api/v1/storage/usage` | firma kullanıcısı | `{ available, usedBytes, quotaBytes, freeBytes, trashedBytes, areas?: [{ area, usedBytes, fileCount }], trashedCount? }` — `usedBytes` çöp kutusunu da içerir (dosya kalıcı silinene kadar kotaya sayılır; `trashedBytes` bunun çöpteki kısmı, çöp boşaltılınca açılır). `areas` (etkin dosyalar) ve `trashedCount` yalnız `action.storage.manage` (Admin, Yönetici; kilitli) sahibine. `available: false` = sunucuda R2 ayarı yok, yükleme `503` döner |
| `GET /api/v1/storage/files/{id}/link` | `files/{id}` ile aynı | `{ url, expiresAtMs }` — yönlendirmenin adresi veri olarak (panel fişi kendisi gösterir); herkese açık dosyada `expiresAtMs` null |
| `GET /api/v1/storage/files/{id}` | yükleyen; `action.storage.manage`; alan kuralı (görev: görevi gören; gider/araç: muhasebe, `portal.ledger`, `view.expenses.all_users`) | `302` → özel dosyada 5 dakikalık imzalı R2 adresi, herkese açık dosyada `https://img.appsgo.cloud/…`; `Cache-Control: private, no-store`. Başka firmanın, çöpteki ya da açma yetkisi olmayan dosya aynı `404 STORED_FILE_NOT_FOUND`; depo ayarsızsa `503 STORAGE_UNAVAILABLE` |
| `GET /api/v1/admin/tenants/{id}/storage` | Admin konsolu | `{ tenantId, available, usedBytes, reservedBytes, quotaBytes, defaultQuotaBytes, customQuotaBytes?, recountedAtMs?, areas: [...], trashedBytes, trashedCount }`; firma yoksa `404 TENANT_NOT_FOUND` |
| `PUT /api/v1/admin/tenants/{id}/storage` | Admin konsolu | `{ quotaBytes: sayı \| null }` — `null` = varsayılan (5 GB, `Storage:DefaultQuotaBytes`); 0..10 TB, değilse `400 INVALID_QUOTA`. Kullanılanın altına inebilir (yeni yükleme durur). Yanıt güncel görünüm |
| `POST /api/v1/admin/tenants/{id}/storage/recount` | Admin konsolu | `{ usedBytesBefore, usedBytesAfter, storage }` — kullanılan bayt defterdeki etkin ve çöpteki dosyalardan yeniden hesaplanır (aynı iş her gün `Storage:MaintenanceHourUtc`'de çalışır) |

**Ürün fotoğrafı (S6)** — firmanın kendi ürün fotoğrafları (telefonda kamera/galeri, panelde Stok). Yalnız görsel değişir, ürün kartı
değil: ERP'li firmada da çalışır, modül gerekmez. Yazım yetkisi `action.products.photo` (Admin, Yönetici, Satış; kilitsiz).

| Uç | Kim | Gövde / yanıt |
|---|---|---|
| `POST /api/v1/storage/products/images?stockCode=` | `action.products.photo` | Ham gövde `image/jpeg\|png\|webp` ≤ 10 MB, küçültmeden. Sunucu dik çevirir, üst veriyi atar, 1280 px ve 400 px WebP üretir. Yanıt `ProductImage { id, stockCode, sortOrder, thumbUrl, fullUrl, width, height, sizeBytes, createdAtMs, createdByName }` — `thumbUrl`/`fullUrl` tam CDN adresi (`https://img.appsgo.cloud/{FIRMAKODU}/product/…-{s\|l}.webp`). Aynı bayt tekrar = aynı fotoğraf (`200`, yeni dosya yok). Hatalar: `403 PRODUCT_PHOTO_FORBIDDEN`, `400 INVALID_STOCK_CODE`, `413 PRODUCT_IMAGE_TOO_LARGE`, `415 INVALID_IMAGE` (çözülemeyen resim dahil), `409 PRODUCT_IMAGE_LIMIT` (ürün başı 8), `413 STORAGE_QUOTA_EXCEEDED {usedBytes, quotaBytes}`, `503 STORAGE_UNAVAILABLE` |
| `GET /api/v1/storage/products/images?stockCode=` | firma kullanıcısı | `{ stockCode, items: [ProductImage], xmlItems: [XmlImage] }` sırayla (kapak ilk; `xmlItems` XML sırasıyla, S7) |
| `GET /api/v1/storage/products/images/manifest` | firma kullanıcısı | `{ items: [{ stockCode, items: [ProductImage] }], xmlItems: [{ stockCode, items: [XmlImage] }] }` — fotoğrafı olan bütün ürünler ve sunucuda XML görseli olan bütün ürünler (telefonun görsel sırası: firmanın fotoğrafı → XML → yerel kopya). `items` içindeki grupların `xmlItems`'ı burada boştur |
| `PUT /api/v1/storage/products/images/order?stockCode=` | `action.products.photo` | `{ ids }` bu sırayla, verilmeyenler eski sırasıyla arkadan; `204`; bilinmeyen kimlik `404 PRODUCT_IMAGE_NOT_FOUND` |
| `DELETE /api/v1/storage/products/images/{id}` | `action.products.photo` | `204`, iki dosya çöpe; yok/başka firma `404 PRODUCT_IMAGE_NOT_FOUND` |

Web katalog bir ürünün görseli olarak önce katalog görsellerini, yoksa ürün fotoğraflarını, o da yoksa XML görsellerini (S7)
gösterir (`products`/`products/detail` `thumb` ve `images`, talep detayındaki küçük resim); katalog yönetimi (`images/manifest`,
`imageCount`) yalnız katalog görsellerini sayar.

**XML görselleri (S7)** — sunucu firmanın kayıtlı XML beslemesini (`/api/v1/android/xml-feed/config`) telefonla aynı kurallarla
okur, kodu firmanın ürünleriyle (büyük/küçük harf duyarsız) eşleştirir ve ürün başı ilk 6 görsel adresini indirip 1280/400 px WebP
olarak herkese açık kovaya (`xml` alanı) koyar; kotaya sayılır. Günde bir (`Storage:MaintenanceHourUtc` + 1, UTC), panelden
"şimdi eşitle" ile ve XML ayarı görsel indirme açık kaydedilince çalışır. Kaynağı izler: adresi XML'den kalkan görsel ya da XML'den
kalkan ürünün görselleri **çöpe gitmeden** silinir; 7 günden eski kopya ETag/Last-Modified ile yeniden sorulur, içerik değiştiyse
yenisiyle değişir. XML indirilemez, okunamaz, kaydı yoksa, hiçbir ürünle eşleşmezse ya da eşleşen ürünlerin hiç görseli yoksa
**hiçbir şey silinmez** (`status: failed`). Dış adres güvenliği: yalnız http/https, yerel/özel/link-local/eşlenik IPv6 adresler
reddedilir, bağlantı doğrulanan IP'ye sabitlenir, yönlendirme elle en çok 3, görsel ≤ 10 MB / 20 sn, XML ≤ 100 MB / 120 sn.
`XmlImage { id, stockCode, position, sourceUrl, thumbUrl, fullUrl, width, height, sizeBytes, updatedAtMs }` (salt okunur).

| Uç | Kim | Gövde / yanıt |
|---|---|---|
| `GET /api/v1/storage/xml-images/status` | `action.storage.manage` | `{ configured, moduleEnabled, downloadImages, storageAvailable, requestedAtMs?, startedAtMs?, finishedAtMs?, status?, message?, stats?, imageCount, imageBytes, productCount }` — `status` `ok\|partial\|quota\|failed` (ilk çalışmadan önce null); `stats { feedRecords, matchedProducts, wanted, added, replaced, removed, unchanged, failed, remaining }`; `message` kısa Türkçe açıklama (adres içermez). Yetkisiz `403 STORAGE_FORBIDDEN` |
| `POST /api/v1/storage/xml-images/sync` | `action.storage.manage` | Gövde yok. `202` + status gövdesi; eşitleme bir dakika içinde başlar (bekleyen istek yerini korur). `403 STORAGE_FORBIDDEN`, modül yoksa `403 MODULE_NOT_ENABLED`, XML ayarı yoksa `409 XML_FEED_NOT_CONFIGURED`, görsel indirme kapalıysa `409 XML_IMAGES_DISABLED`, depo ayarsızsa `503 STORAGE_UNAVAILABLE` |

**Çöp kutusu ve temizlik (S9)** — yalnız `action.storage.manage` (Admin, Yönetici; kilitli), başkasına `403 STORAGE_FORBIDDEN`.
Kullanıcının bir silmesi (ürün fotoğrafı, katalog görseli, banner ve banner'ın değiştirilen görseli, görev resmi, fiş) ve "Alan aç"
çöpe bir **öğe** bırakır: dosyalar + kaydı geri koymaya yetecek anlık görüntü. Geri alma kaydı yüklemedeki kilit ve sınırlarla geri
koyar (ürün başı 8 fotoğraf ve aynı fotoğraf bir kez; katalog ürün görsel sınırı; banner sınırları; görev başı 10 resim; belge başı
5 fiş); çakışan öğe Türkçe gerekçeyle başarısız olur ve çöpte kalır, diğerleri geri alınır. Çöpteki dosya kalıcı silinene kadar
kotaya sayılır; `Storage:TrashDays` (7) gün sonra günlük iş kalıcı siler. Kaydı olmayan dosyalar (günlük süpürme, 30 günlük silinmiş
görev) çöpte görünür ama geri alınamaz (`restorable: false`). Her temizlik, geri alma ve kalıcı silme denetim kaydına (`native_audit_log`,
`entity = storage`) yalnız sayı ve baytla yazılır.

| Uç | Gövde / yanıt |
|---|---|
| `GET /api/v1/storage/trash?page=` | `{ page, pageSize (50), total, totalBytes, trashDays, items: [{ id, area, kind, label, sizeBytes, fileCount, source, trashedAtMs, trashedByName?, daysLeft, restorable, thumbUrl? }] }` en yeni önce. `kind` `product_image\|catalog_image\|banner\|banner_image\|task_attachment\|expense_attachment\|files`; `source` `user\|cleanup\|sweep\|owner_deleted`; `daysLeft` kalıcı silinmeye kalan gün (7…1, süresi dolmuşsa 0); `thumbUrl` küçük boyut — herkese açık dosyada CDN adresi, özel dosyada `Storage:PresignMinutes` dakikalık imzalı adres |
| `POST /api/v1/storage/trash/restore` | `{ ids }` (1–50). `{ restored, restoredBytes, failed, items: [{ id, label, restored, reason? }] }`. Gerekçe örnekleri: "Ürünün fotoğraf sınırı (8) dolu…", "Ürünün aynı fotoğrafı zaten var.", "Görev silinmiş; resmi geri alınamaz.", "Banner silinmiş; görseli geri alınamaz.", "Dosyalar kalıcı silinmiş; geri alınamaz.", "Bu dosyaların kaydı silinmiş; geri alınamaz…". `400 INVALID_BODY`, `503 STORAGE_UNAVAILABLE` |
| `POST /api/v1/storage/trash/purge` | `{ ids }` ya da `{ all: true }` (çöpü boşalt; öğesiz eski çöp dosyaları da). `{ purged, purgedBytes, failed }` — yer hemen açılır; R2'nin yanıt vermediği öğe kalır, günlük iş yeniden dener |
| `GET /api/v1/storage/cleanup/summary?days=` | `{ days, groups: [{ group, label, count, bytes, purgesDirectly }] }` — sıra `missing_products`, `out_of_stock`, `closed_tasks`, `ended_banners`, `xml_unused`; `days` kapanmış görev yaşı (varsayılan 90) |
| `GET /api/v1/storage/cleanup/candidates?group=&page=&days=` | `{ group, label, page, pageSize (50), total, totalBytes, items: [{ id, kind, label, area, sizeBytes, thumbUrl?, extra? }] }` büyükten küçüğe; `id` sahibin kimliği; bilinmeyen grup `400 INVALID_CLEANUP_GROUP` |
| `POST /api/v1/storage/cleanup` | `{ group, ids?, all?, days? }` → `{ group, trashedCount, trashedBytes, purgedCount, purgedBytes, remaining, message }`. Her kimlik grubun **o anki** adaylarıyla yeniden denetlenir (grup dışı kimliğe dokunulmaz). İstek başı en çok 500 sahip; kalan `remaining`. XML görselleri çöpe gitmez, kalıcı silinir (XML'den yeniden indirilebilir) |

Gruplar: `missing_products` — stok kodu firmanın ürünlerinde olmayan ürün fotoğrafı, katalog görseli ve XML görseli (firmanın hiç ürünü
yoksa grup boştur; "her şey kayıp" sayılmaz); `out_of_stock` — şu an stokta olmayan ürünlerin fotoğrafı ve katalog görseli;
`closed_tasks` — `days` günden önce tamamlanmış (`CompletedAtMs`) ya da iptal edilmiş (son değişiklik zamanı) silinmemiş görevlerin
resimleri; `ended_banners` — kapalı ya da bitiş tarihi geçmiş bannerlar ve görselleri (görseli başka bir banner da gösteriyorsa aday değildir);
`xml_unused` — XML modülü kaldırılmış, XML ayarı silinmiş ya da görsel indirme kapalı firmanın bütün XML görselleri.

**Karantina (T4)** — firma pasifleşince bütün herkese açık dosyaları, müşteri kataloğu modülü kaldırılınca yalnız `catalog`/`banner`
dosyaları özel kovaya `{FIRMAKODU}/_karantina/{özgün anahtar}` altına taşınır (kopyala → defter → sil; defterde `QuarantinedFromKey`);
firma ya da modül yeniden açılınca geri taşınır. Admin `PATCH /api/v1/admin/tenants/{id}` (`isActive`) ve `PUT …/mobile/modules`
dakika içinde bir denetim ister; ayrıca her gün bütün firmalar denetlenir. Haftada bir (pazar, günlük işin ardından) R2 listelenir:
defterde olmayan, 24 saatten eski ve ilk klasörü bir firma kodu olan nesneler silinir (en çok 1000); bilinmeyen nesneler listenin %30'unu
ya da 2000'i geçerse hiçbir şey silinmez.

Yeni hata kodları:
- `413 STORAGE_QUOTA_EXCEEDED` — `{ errorCode, message, traceId, usedBytes, quotaBytes }`. Telefon metni: "Firmanızın depolama
  alanı doldu. Yöneticiniz panelden alan açabilir." Katalog uçları aynı durumda eski `CATALOG_IMAGE_QUOTA_EXCEEDED`, görev eki
  `TASK_ATTACHMENT_QUOTA` kodunu (aynı rakamlarla) döner; eski telefonlar onlara bakıyor.
- `503 STORAGE_UNAVAILABLE` — R2 ayarı yok ya da R2 yanıt vermedi; telefon kuyruğu sonra yeniden dener.
- `415 INVALID_IMAGE` — yalnız JPEG, PNG, WebP (ilk baytlarından denetlenir).

## Hata modeli

```json
{
  "errorCode": "JOB_NOT_FOUND",
  "message": "Job 5b9e... was already acknowledged",
  "traceId": "..."
}
```

Yaygın kodlar:
- `LICENSE_INVALID` — lisans süresi dolmuş veya iptal edilmiş
- `LICENSE_EXPIRED` — geçerli ama süresi geçmiş
- `TENANT_MISMATCH` — agent kayıtlı tenant ile lisans tenant uyuşmuyor
- `JOB_NOT_FOUND` — ack gönderilen job zaten işlenmiş
- `TRANSIENT_UPSTREAM` — 5xx, agent exponential backoff ile retry
- `RATE_LIMITED` — HTTP 429. Hız sınırının ve giriş yavaşlatıcının her reddi bu gövdeyi taşır; bekleme biliniyorsa
  `Retry-After` (saniye) başlığı da gelir (GOAL_MUSTERI_KATALOGU §5).

**Giriş yavaşlatıcı** (`/api/v1/android/account/login`, `/api/v1/admin/login`, `/api/v1/catalog/{code}/login`; bilgi bankası kural 36): aynı ad
15 dakikada 5 kez yanlış girilirse 60 sn bekler; her yeni hata beklemeyi ikiye katlar (en çok 15 dk). Beklerken şifre
denetlenmez, doğru şifre de `429 RATE_LIMITED` alır. Hesap kilitlenmez; başarılı giriş sayacı sıfırlar. Devam eden denemeler de
sayılır: aynı anda en çok kalan hata hakkı kadar deneme yürür (bir kez bekletilmiş adda tek), fazlası `429` (`Retry-After: 1`).
Anahtarlar: personel = firma kodu + kullanıcı adı + **istemci adres bölümü** (IPv4 / IPv6 /64) — başka adresten deneyen kullanıcıyı
dışarıda bırakamaz; kullanıcının kayıtlı ve aktif telefonu (`mobile_devices`: aynı firma, `DeviceId`, `IsActive`,
`LastUserId` = kullanıcı) kendi sayacını kullanır (panelin cihaz kimliği kullanıcı adından türediği için muaf değildir). Admin =
e-posta + adres bölümü; bilinmeyen e-postada da sahte hash'le BCrypt çalışır. Katalog = firma kodu + kullanıcı adı (güncel cihaz
çerezli tarayıcı ve oturumdaki şifre değişikliği `#{hesapId}`). **Sınır:** panel ve Admin konsolu girişleri sunucuya kendi
konteynerlerinin adresinden gelir; orada adres ayrımı yoktur.

Sunucu Traefik arkasında gerçek istemci IP'sini yalnız `ForwardedHeaders:KnownNetworks` / `KnownProxies` ayarındaki
vekillerden gelen `X-Forwarded-For` ile öğrenir; IP başına sınırlarda IPv6 adresleri /64 önekine indirgenir. Güvenilmeyen bir
adresten `X-Forwarded-For` gelirse (ayar eksik ya da yanlış) ilk istekte bir kez uyarı loglanır (başlık 64 karaktere kısaltılır).

## Retry & backoff

- `429` / `5xx` → exponential backoff: 5s, 15s, 60s, 300s (cap)
- `4xx` (kendi payload hatası) → ack `failed` ile bildirilir, retry yok
- Timeout: 30 sn
- Idempotency: her `jobId` agent tarafında en az bir kez başarılı ack edilene kadar
  kuyrukta kalır

## Versiyonlama

Tüm endpointler `/api/v1/` prefixli. Geriye dönük kırılma olursa `/api/v2/` açılır;
v1 en az 12 ay deprecate uyarısıyla yaşar.
