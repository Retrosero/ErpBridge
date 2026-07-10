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
lookups }`. Yanıt: 204. Tenant başına periyodik (örn. saatlik).

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