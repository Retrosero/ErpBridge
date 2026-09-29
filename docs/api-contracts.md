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

## Retry & backoff

- `429` / `5xx` → exponential backoff: 5s, 15s, 60s, 300s (cap)
- `4xx` (kendi payload hatası) → ack `failed` ile bildirilir, retry yok
- Timeout: 30 sn
- Idempotency: her `jobId` agent tarafında en az bir kez başarılı ack edilene kadar
  kuyrukta kalır

## Versiyonlama

Tüm endpointler `/api/v1/` prefixli. Geriye dönük kırılma olursa `/api/v2/` açılır;
v1 en az 12 ay deprecate uyarısıyla yaşar.
