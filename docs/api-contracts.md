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
çağrılır; süre bitiminde tekrar doğrulanır.

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
Her yazma `native_audit_log`'a düşer (`GET /native/audit`).

| Uç | Açıklama |
|---|---|
| `GET /stock-cards/{code}` · `POST /stock-cards` · `DELETE /stock-cards/{code}` | Ürün kartı oku/aç/düzenle/sil (hareketli ürün silinmez) |
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

Evrak (`documents/{key}/void|edit`) ve sayım (`stock-counts/{key}/void`) iptal/düzenleme uçları "zaten iptal" kontrolünden **önce** aynı `operationId`'li işi arar: yanıtı kaybolan tekrar 200 alır, 409 değil. `operationId` içindeki `|` `-` olur (hareket anahtarları `{iş}|{ek}`).

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
