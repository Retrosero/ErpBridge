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

### POST /api/v1/agents/heartbeat

Periyodik (örn. her 60 sn). Body: `AgentHeartbeat { agentId, tenantId, status,
lastSyncAtUtc, queueDepth, lastError? }`. Yanıt: 204.

### POST /api/v1/agents/telemetry

Windows Agent'ın gizlilikten arındırılmış hata kaydı. Agent JWT'si zorunludur.
Body: `{ eventId, occurredAtUtc, kind, severity, appVersion, windowsVersion,
machineName, operation, exceptionType, message, stackTrace }`. Yanıt: `204`.
Lisans anahtarı, JWT, SQL şifresi, bağlantı dizesi ve ERP payloadları kesinlikle
gönderilmez; agent bunları göndermeden önce maskeler. Kayıtlar mobil tanılama
kayıtlarıyla aynı yönetim ekranında tutulur.

### POST /api/v1/licenses/validate

`{ licenseKey }` → `{ valid, tenantId, expiresAtUtc }`. Agent başlangıcında bir kez
çağrılır; süre bitiminde tekrar doğrulanır.

### GET /api/v1/jobs/pending

Query: `?take=50&type=sales_order`. Yanıt: `RemoteJob[] { jobId, externalId,
documentType, payload, enqueuedAtUtc }`.

### POST /api/v1/jobs/ack

Body: `JobAck { jobId, status: "succeeded" | "failed", errorCode?, errorMessage?,
erpDocumentSeries?, erpDocumentNumber?, erpRecno?, erpGuid? }`. Yanıt: 204.

### POST /api/v1/bootstrap

Body: `SyncPackage { customers, stocks, prices, inventory, openOrders, cashAndBank,
lookups }`. Yanıt: 204. Tenant başına periyodik (Faz 9: her 60 sn delta push).
Başarılı insert'ten sonra sunucu, bu tenant'ın `/api/v1/bootstrap/notify` long-poll
bekleyenlerini cursor ile uyandırır.

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
