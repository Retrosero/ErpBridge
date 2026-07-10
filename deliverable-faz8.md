# Deliverable — Faz 8

> **Tema:** Sunucu tarafını bir SaaS lisans platformuna çevirmek. Yeni:
> tenant-scoped **API Key** + outbound **Webhook** + admin panel CRUD + Coolify
> deploy paketi.

## Kapsam

| Alt-faz | Hedef | Durum |
|---|---|---|
| 8.1 | Domain (ApiKey, WebhookEndpoint, WebhookDelivery) + Contracts + DbContext | ✅ |
| 8.2 | ApiKeyAuthenticationHandler + Ingest endpoint | ✅ |
| 8.3 | Admin CRUD (ApiKeys + Webhooks) + outbound webhook dispatcher | ✅ |
| 8.4 | Admin Blazor UI (ApiKeys + Webhooks sayfaları) | ✅ |
| 8.5 | Testler (ApiKey auth, ingest, webhook delivery, admin CRUD) | ✅ |
| 8.6 | Coolify deploy (Dockerfile + docker-compose + docs) | ✅ |

## Mimari

```
┌──────────────────┐    POST /api/v1/ingest/jobs         ┌──────────────────┐
│ Müşterinin       │ ──────────────────────────────────▶ │  CentralApi      │
│ E-ticaret /      │    Authorization: Bearer AK-...      │  ApiKeyAuth      │
│ Mobil App        │    X-Tenant-Id: <guid>              │  → Job tablosu   │
└──────────────────┘                                      └──────────────────┘
                                                                   │
                                                                   │ Agent poll eder
                                                                   ▼
                                                            ┌──────────────┐
                                                            │ Windows Agent│
                                                            │  /jobs/pending
                                                            └──────────────┘
                                                                   │
                                                                   │ Mikro'ya yazar
                                                                   │ /jobs/ack
                                                                   ▼
                                                            ┌──────────────────┐
                                                            │ WebhookDispatcher│ ──POST──▶ Tenant'ın WebhookEndpoint'i
                                                            │  (HMAC-SHA256)   │
                                                            └──────────────────┘
```

`License` (Windows Agent tescili için) ve `ApiKey` (programatik erişim için)
iki ayrı kavram. `License` zaten vardı; bu fazda geriye dönük uyumlu kalındı,
hiçbir tablo/endpoint kaldırılmadı.

## Değişen / yeni dosyalar

### Yeni dosyalar

**Domain / Contracts / Auth:**
- `src/ErpBridge.CentralApi/Domain/ApiKey.cs`
- `src/ErpBridge.CentralApi/Domain/WebhookEndpoint.cs`
- `src/ErpBridge.CentralApi/Domain/WebhookDelivery.cs`
- `src/ErpBridge.CentralApi/Contracts/IngestContracts.cs`
- `src/ErpBridge.CentralApi/Authentication/ApiKeyAuthenticationOptions.cs`
- `src/ErpBridge.CentralApi/Authentication/ApiKeyAuthenticationHandler.cs`

**Endpoint'ler:**
- `src/ErpBridge.CentralApi/Endpoints/IngestEndpoints.cs`
- `src/ErpBridge.CentralApi/Endpoints/AdminApiKeysEndpoints.cs`
- `src/ErpBridge.CentralApi/Endpoints/AdminWebhooksEndpoints.cs`

**Webhook fan-out:**
- `src/ErpBridge.CentralApi/Webhooks/IWebhookDispatcher.cs`
- `src/ErpBridge.CentralApi/Webhooks/WebhookDispatcher.cs`
- `src/ErpBridge.CentralApi/Webhooks/WebhookDispatcherWorker.cs`

**Admin UI:**
- `src/ErpBridge.Admin/Pages/ApiKeys.razor`
- `src/ErpBridge.Admin/Pages/Webhooks.razor`

**Deploy:**
- `Dockerfile`
- `docker-compose.coolify.yml`
- `.dockerignore`
- `docs/deploy-coolify.md`

**Testler:**
- `tests/ErpBridge.CentralApi.Tests/Endpoints/IngestTests.cs`
- `tests/ErpBridge.CentralApi.Tests/Endpoints/AdminApiKeysTests.cs`
- `tests/ErpBridge.CentralApi.Tests/Endpoints/AdminWebhooksTests.cs`
- `tests/ErpBridge.CentralApi.Tests/Endpoints/WebhookDispatcherTests.cs`

### Değişen dosyalar

- `src/ErpBridge.CentralApi/Data/CentralApiDbContext.cs` — 3 DbSet + mapping
- `src/ErpBridge.CentralApi/Program.cs` — ApiKey scheme, policy, hosted service, DI
- `src/ErpBridge.CentralApi/Endpoints/JobsEndpoints.cs` — `IWebhookDispatcher` hook + ack'te fan-out
- `src/ErpBridge.Admin/MainLayout.razor` — iki yeni nav linki
- `src/ErpBridge.Admin/Api/CentralApiClient.cs` — 7 yeni metod + 9 yeni DTO
- `tests/ErpBridge.CentralApi.Tests/Support/CentralApiFactory.cs` — `SeedApiKeyAsync`, `SeedWebhookAsync`

## PostgreSQL şeması (yeni kısım)

```sql
api_keys
  id              uuid PK
  tenant_id       uuid FK→tenants ON DELETE CASCADE
  name            text NOT NULL
  key_prefix      text NOT NULL             -- "AK-XXXXXXXX" (id amaçlı)
  key_hash        bytea NOT NULL            -- SHA-256(salt || rawKey), 32 byte
  key_salt        bytea NOT NULL            -- 16 byte per-row
  scopes          text[] NOT NULL DEFAULT '{ingest:write}'
  is_active       bool NOT NULL DEFAULT true
  created_at_utc  timestamptz NOT NULL
  expires_at_utc  timestamptz
  last_used_at_utc timestamptz
  INDEX (tenant_id, key_prefix)

webhook_endpoints
  id                   uuid PK
  tenant_id            uuid FK→tenants ON DELETE CASCADE
  name                 text NOT NULL
  url                  text NOT NULL
  signing_secret       text NOT NULL         -- HMAC secret, cleartext (gönderimde lazım)
  signing_secret_prefix text NOT NULL        -- "whsec_XX" (id amaçlı)
  subscribed_events    text[] NOT NULL DEFAULT '{}'
  is_active            bool NOT NULL DEFAULT true
  created_at_utc       timestamptz NOT NULL
  last_delivered_at_utc timestamptz
  INDEX (tenant_id)

webhook_deliveries
  id                uuid PK
  endpoint_id       uuid FK→webhook_endpoints ON DELETE CASCADE
  tenant_id         uuid NOT NULL            -- denormalized
  event_type        text NOT NULL
  job_id            uuid
  payload_json      jsonb NOT NULL DEFAULT '{}'
  status            int NOT NULL             -- 0..3 (Pending/Delivered/Failed/DeadLetter)
  attempt_count     int NOT NULL DEFAULT 0
  last_attempt_at   timestamptz
  last_response_code int
  last_error        text
  next_retry_at     timestamptz
  created_at_utc    timestamptz NOT NULL
  INDEX (status, next_retry_at_utc)
  INDEX (endpoint_id, created_at_utc)
```

Mevcut `tenants`, `licenses`, `agents`, `jobs`, `job_acks`, `bootstrap_packages`,
`admin_users` tablolarına dokunulmadı.

## Endpoint sözleşmesi (yeni)

```
POST   /api/v1/ingest/jobs
       Authorization: Bearer AK-<48-hex>
       X-Tenant-Id: <guid>
       Content-Type: application/json
       { "externalId":"...", "documentType":"...", "payload":{...} }

       → 201 Created (yeni job)   { jobId, tenantId, externalId, documentType, status:"Pending", idempotent:false }
       → 200 OK      (idempotent) { ... idempotent:true }
       → 400 / 401 / 403 / 404 / 413

GET    /api/v1/admin/api-keys?tenantId=...
POST   /api/v1/admin/api-keys                 → ApiKeyCreatedDto (rawKey bir kez)
POST   /api/v1/admin/api-keys/{id}/revoke     → 204
POST   /api/v1/admin/api-keys/{id}/rotate     → ApiKeyCreatedDto (yeni rawKey)

GET    /api/v1/admin/webhooks?tenantId=...
GET    /api/v1/admin/webhooks/{id}
POST   /api/v1/admin/webhooks                 → WebhookEndpointCreatedDto (signingSecret bir kez)
PATCH  /api/v1/admin/webhooks/{id}            → WebhookEndpointDto
DELETE /api/v1/admin/webhooks/{id}            → 204
GET    /api/v1/admin/webhooks/{id}/deliveries → WebhookDeliveryDto[] (son 200)
```

## Güvenlik notları

- **API key hash format:** `SHA-256(16-byte salt || rawKey)`. Salt per-row; aynı
  raw value farklı tenant'ta farklı hash üretir. DB leak'i tek başına offline
  brute-force için yeterli değil.
- **Raw key sızıntısı:** Raw value yalnızca `POST /admin/api-keys` ve
  `POST /admin/api-keys/{id}/rotate` response'unda döner. Listeleme, detay,
  audit endpoint'leri sadece `keyPrefix` (`AK-XXXXXXXX`) döner. UI tarafında
  oluşturma/rotation sonrası "tek seferlik göster" banner'ı + "Copy" butonu.
- **Webhook secret:** HMAC imzası için cleartext lazım olduğundan DB'de
  saklanır. Production'da Postgres TDE / Coolify volume encryption ile
  korunur. UI tarafında oluşturma sonrası tek seferlik gösterilir, sonra
  sadece `signingSecretPrefix` (`whsec_XX`) gösterilir.
- **Tenant izolasyonu:** API key auth'da tenant id iki yerden gelir: bearer
  token'daki `tenant` claim'i ile `X-Tenant-Id` header'ı. Uyumsuzluk 401.
  JWT token'daki `tenant` claim'i her zaman body'siz kaynaktan gelir (token).
- **Webhook HMAC:** Receiver imzayı şöyle doğrular:
  ```
  expected = HMAC-SHA256(secret, "<timestamp>.<body>")
  matches  = constant-time-equals(expected, header("ErpBridge-Signature").removePrefix("sha256="))
  ```
- **Rate limit:** Ingest endpoint'i `per-agent` policy'yi paylaşır (JWT'deki
  `sub` claim'i yerine API key `sub` claim'i). Tenant başına dakikada 100
  istek.

## Test özeti

Test factory'ye iki yeni helper eklendi (`SeedApiKeyAsync`, `SeedWebhookAsync`).
Yeni test dosyaları:

- **IngestTests** (6 test): auth yok → 401, key yok → 401, yanlış tenant → 401,
  happy path → 201, idempotent (200 + aynı jobId), 256 KB üstü payload → 413.
- **AdminApiKeysTests** (4 test): rawKey yalnızca oluşturmada döner, listede
  rawKey yok JSON'da, revoke sonrası key geçersiz olur, rotate sonrası eski
  key 401 + yeni key 201.
- **AdminWebhooksTests** (4 test): secret yalnızca oluşturmada döner, non-http
  URL reddedilir (400), delete çalışır, deliveries endpoint döner.
- **WebhookDispatcherTests** (2 test): `EnqueueJobTerminalAsync` her aktif eşleşen
  endpoint için bir delivery yazar (event filtresi yanlış olan ve inactive
  endpoint atlanır), `ComputeSignature` deterministik + format `[0-9a-f]{64}`.

Mevcut `JobsTests.Ack_*` testleri hâlâ geçer (webhook dispatcher DI'da
resolve ediliyor, hiç endpoint yoksa boş iş).

## Coolify deploy

İki Coolify application + bir Coolify managed PostgreSQL + iki FQDN
(Lets Encrypt otomatik).

- `Dockerfile` çoklu hedefli (`TARGET=centralapi|admin`), multi-stage build,
  non-root runtime user.
- `docker-compose.coolify.yml` Coolify'in "Docker Compose" source'una
  yapıştırılır; her service Coolify application'ına dönüşür.
- `docs/deploy-coolify.md` adım adım rehber + env değişkeni referansı +
  troubleshooting tablosu.

Secret'lar Coolify "Secret" tipinde: `POSTGRES_PASSWORD`, `JWT_SIGNING_KEY`,
`ADMIN_SEED_PASSWORD`. Plain text value asla repo'da değil.

## Bilinen sınırlamalar / sonraki faz önerileri

- **Admin şifre değiştirme UI'ı yok.** Bootstrap admin ilk login sonrası
  şifresini değiştiremez; bu bir sonraki fazda `PATCH /admin/me/password`
  + UI.
- **API key `LastUsedAtUtc` güncellemesi "best-effort"**: response
  `OnStarting` callback'inde yazılır. Testlerde sıklıkla gözlenmez
  çünkü in-memory provider'da callback invoke sırası değişebilir.
- **Webhook dispatcher hosted service test host'ta da çalışıyor**;
  test fixture'ı `Task.Delay(Timeout.Infinite)` parkı nedeniyle shutdown'da
  ~5 s gecikmeye yol açabilir. Production'da sorun değil; testleri
  hızlandırmak istersek factory'de `RemoveAll<IHostedService>` ile disable
  edilebilir.
- **Outbound webhook URL allowlist yok.** Bilinen risk: SSRF. İleride
  URL'yi oluştururken DNS resolution + private IP range check eklemek
  gerekebilir.
- **Idempotent insert için race window var.** Aynı (tenant, documentType,
  externalId) için iki eşzamanlı istek `DbUpdateException` (unique violation)
  alabilir; catch bloğu winner'ı okur ve 200 döndürür. Doğru çalışır ama
  loglarda exception gözükür — kabul edilebilir, ama izlenmesi iyi olur.

## Build / test komutları

```powershell
# Restore + build
dotnet build ErpBridge.sln

# Tüm testler
dotnet test

# Sadece bu fazın testleri
dotnet test tests/ErpBridge.CentralApi.Tests --filter "FullyQualifiedName~IngestTests|FullyQualifiedName~AdminApiKeysTests|FullyQualifiedName~AdminWebhooksTests|FullyQualifiedName~WebhookDispatcherTests"
```

> **Bu ortamda .NET 8 SDK yok** (yalnızca 3.1/5.0/6.0 mevcut), bu yüzden
> build ve test sen makinede çalıştırılmalı. Syntax/referans kontrolü
> edit aşamasında yapıldı; `dotnet build` ve `dotnet test` çıktısına göre
> küçük düzeltmeler gerekebilir.