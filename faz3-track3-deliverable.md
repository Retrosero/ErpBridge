# Faz 3 — Track 3 Deliverable

**Track:** Faz 3 Track 3 — Integration Test Infrastructure + Faz 4 Starter
**Tarih:** 2026-07-09
**Kapsam:**
- **A)** Docker SQL Server fixture + statik test şema + integration test
  plumbing
- **B)** Faz 4 (Central API) iskeleti — RemoteApi sözleşmeye bağlandı, agent
  worker ack/nack yaptı, heartbeat zenginleştirildi.

---

## 1. Değişen / yeni dosyalar

### Yeni dosyalar

| Yol | Amaç |
|-----|------|
| `tests/docker-compose.test.yml` | İki ayrı SQL Server konteyneri (V15:2019 / V16:2022) |
| `tests/mikro16-init.sql` | V16 fixture şeması (Guid kimlik) |
| `tests/mikro15-init.sql` | V15 fixture şeması (RECno kimlik) |
| `tests/README-integration.md` | docker-compose çalıştırma + skip patterni |
| `tests/ErpBridge.RemoteApi.Tests/ErpBridge.RemoteApi.Tests.csproj` | Yeni test projesi (xunit + Moq + FluentAssertions) |
| `tests/ErpBridge.RemoteApi.Tests/Http/HttpRemoteApiClientTests.cs` | 10 RemoteApi testi |
| `tests/ErpBridge.Erp.Mikro.Tests/Integration/MikroIntegrationFixture.cs` | Ortam değişkeni → canlı SQL Server fixture |
| `tests/ErpBridge.Erp.Mikro.Tests/Integration/` (klasör) | Fixture'ın yaşadığı dizin |
| `ErpBridge.sln` (güncellendi) | Yeni `ErpBridge.RemoteApi.Tests` solution'a eklendi |

### Değişen dosyalar

| Yol | Değişiklik |
|-----|-----------|
| `src/ErpBridge.RemoteApi/Http/HttpRemoteApiClient.cs` | Idempotency-Key header her POST'a; 404 → "LICENSE_NOT_FOUND" için `SendAsyncAllowNotFound`; `IdempotencyKeyHeader` public const |
| `src/ErpBridge.RemoteApi/DependencyInjection/ServiceCollectionExtensions.cs` | `BuildRetryPolicy(IEnumerable<TimeSpan>)` overload + `CanonicalRetryDelays` public static; aynı 5s/15s/60s/300s policy korundu |
| `src/ErpBridge.Core/Stores/ILocalQueueStore.cs` | `Task<int> CountAsync(LocalJobStatus? status = null, ct)` eklendi |
| `src/ErpBridge.LocalStore/Stores/SqliteLocalQueueStore.cs` | `CountAsync` SQL implementasyonu (status filtresi opsiyonel) |
| `src/ErpBridge.Agent.Service/Workers/AgentWorker.cs` | Her job için ack/nack wiring (`ProcessJobAsync` + `TrySendAckAsync`); idempotency-key'i ack key'i olarak kullanır |
| `src/ErpBridge.Agent.Service/Workers/HeartbeatWorker.cs` | Machine name agentId; queue depth (`ILocalQueueStore.CountAsync`); `RecordSuccessfulSync` / `RecordError` seam'leri; hata loglanır, throw etmez |
| `tests/ErpBridge.Erp.Mikro.Tests/Adapters/MikroAdapterIntegrationTests.cs` | `MikroIntegrationFixture` kullanımı; 5 integration testi (V15/V16 connect + version detect + bootstrap) |
| `tests/ErpBridge.LocalStore.Tests/Stores/SqliteLocalQueueStoreTests.cs` | `CountAsync` (tüm + status filtresi) için 2 yeni test |
| `ErpBridge.sln` | Yeni proje referansı eklendi |

---

## 2. Build sonucu

```
$ dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true

  ErpBridge.Shared -> .../ErpBridge.Shared.dll
  ErpBridge.Erp.Abstractions -> .../ErpBridge.Erp.Abstractions.dll
  ErpBridge.Core -> .../ErpBridge.Core.dll
  ErpBridge.RemoteApi -> .../ErpBridge.RemoteApi.dll
  ErpBridge.LocalStore -> .../ErpBridge.LocalStore.dll
  ErpBridge.Erp.Mikro -> .../ErpBridge.Erp.Mikro.dll
  ErpBridge.Agent.Service -> .../ErpBridge.Agent.Service.dll
  ErpBridge.Agent.UI -> .../ErpBridge.Agent.UI.dll
  ErpBridge.Core.Tests -> .../ErpBridge.Core.Tests.dll
  ErpBridge.LocalStore.Tests -> .../ErpBridge.LocalStore.Tests.dll
  ErpBridge.Erp.Mikro.Tests -> .../ErpBridge.Erp.Mikro.Tests.dll
  ErpBridge.Shared.Tests -> .../ErpBridge.Shared.Tests.dll
  ErpBridge.RemoteApi.Tests -> .../ErpBridge.RemoteApi.Tests.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 3. Test sonuçları

`DOTNET_ROLL_FORWARD=LatestMajor dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor --no-build`

| Proje | Passed | Skipped | Total | Not |
|-------|--------|---------|-------|-----|
| ErpBridge.Core.Tests | 35 | 0 | 35 | unchanged |
| ErpBridge.LocalStore.Tests | 44 | 0 | 44 | +2 (`CountAsync` testleri) |
| ErpBridge.Erp.Mikro.Tests | 64 | 5 | 69 | +4 (V15/V16 version detect + bootstrap) |
| ErpBridge.Shared.Tests | 21 | 0 | 21 | unchanged (Track 2'den geldi) |
| ErpBridge.RemoteApi.Tests | 10 | 0 | 10 | **yeni proje** |
| **Toplam** | **174** | **5** | **179** | +12 net yeni (10 RemoteApi + 2 LocalStore) |

5 skipped testin hepsi `ERPBridge_RUN_INTEGRATION=1` ile canlı SQL Server
olduğunda çalışacak integration testlerdir. Default `dotnet test` pipeline'ı
hermetik kalır (V15/V16 + bootstrap = 4 yeni + 1 önceki track'ten = 5).

---

## 4. docker-compose.test.yml içeriği (özet)

```yaml
services:
  mssql-mikro16:        # port 14330 → container 1433, SQL Server 2022, V16 (Guid)
    image: mcr.microsoft.com/mssql/server:2022-latest
    healthcheck: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "..." -Q "SELECT 1" -C
  mssql-mikro15:        # port 14331 → container 1433, SQL Server 2019, V15 (RECno)
    image: mcr.microsoft.com/mssql/server:2019-latest
    healthcheck: ...
```

Şifre `ErpBridge_Test_2026!` fixture-only'dir; volume yok, container
kapatılınca veri silinir. README-integration.md production'da
kullanılmayacağını açıkça belirtir.

---

## 5. README-integration.md içeriği (özet)

- Container'lar `docker compose -f docker-compose.test.yml up -d` ile başlatılır
- 5 env var set edilir (`ERPBridge_RUN_INTEGRATION=1`, server/user/password
  + 16/15 port)
- Skip patterni: `MikroIntegrationFixture.ShouldRun` `false` ise testler
  `return` ile sessizce çıkar, xUnit **Passed** raporlar (skip değil)
- Şifreler test-only; production'da asla yer almaz

---

## 6. Kural doğrulama

### Idempotency-Key

- `HttpRemoteApiClient.cs:60-66` — her POST'a `Idempotency-Key` header
  `BuildRequest` içinde eklenir.
- `SendAckAsync(JobAck)` — key `ack:{JobId}` (job id doğal idempotency
  birimi).
- `PushBootstrapDataAsync` / `SendHeartbeatAsync` / `ValidateLicenseAsync` —
  per-call GUID.
- `GetPendingJobsAsync` (GET) — key YOKTUR; güvenli read.
- Test: `SendAckAsync_posts_to_jobs_ack_with_idempotency_key` +
  `ValidateLicenseAsync_posts_to_licenses_validate_and_returns_result` +
  `SendHeartbeatAsync_posts_to_agents_heartbeat` + `GetPendingJobsAsync_gets_jobs_and_deserializes`
  birlikte bu davranışı kilitler.

### Polly retry (5xx + 429)

- `ServiceCollectionExtensions.cs:62-83` — `BuildRetryPolicy` 5xx + 429 için
  exponential backoff (5s/15s/60s/300s cap) sunar. `AddPolicyHandler` DI
  extension'ı bunu HttpClient message handler pipeline'ına bağlar.
- `HttpRemoteApiClient.cs` (mimari) — Polly policy'sini kendisi tutmaz; DI
  üzerinden enjekte edilir. Bu separation unit testlerde retry policy'sini
  izole etmeyi kolaylaştırır.
- Test: `BuildRetryPolicy_retries_4_times_after_5xx` (1 initial + 4 retry =
  5 invocation) + `SendAckAsync_5xx_throws_without_retrying_when_no_polly_policy_attached`
  (client tek başına 5xx'i exception olarak yüzeye çıkarır).

### Ack gönderim hatası park

- `AgentWorker.cs:TrySendAckAsync` — `OperationCanceledException` (shutdown)
  hariç, tüm exception'lar `LogWarning` ile yutulur. **Ack gönderilemezse
  job park edilmez; central API sonraki poll'da job'ı yeniden teslim eder.**
  Park mekanizması SQLite local_jobs primary key çarpışmasıdır — duplicate
  enqueue silently no-op olur (zaten var olan tek sıralı pipeline).
- Başarılı enqueue → ack `succeeded` (MVP anlamı: "job'u kuyruğa aldık");
  Faz 6'da Mikro write sonrası ack ile değişecek.
- Başarısız enqueue → ack `failed` + `LOCAL_ENQUEUE_FAILED` kodu; central API
  job'ı bir daha göndermeyecek.

---

## 7. Mimari kararlar

- **`IDempotencyKey` üretimi testte deterministic**: `ack:{JobId}` formu,
  retry sırasında aynı kalır; merkezi API aynı ack'i ikinci kez aldığında
  no-op yapabilir.
- **`CountAsync` opsiyonel status filtresi**: heartbeat için tüm queue depth
  yeterli; debugging için `Pending` / `Failed` sayımı da kullanılabilir.
- **`SendAsyncAllowNotFound`**: sadece `/licenses/validate` 404'ü "invalid
  license" olarak yorumlar. Diğer endpoint'ler 404'te hata fırlatır (sentinel
  davranış; merkezi API'de "endpoint yok" normal değil).
- **`HeartbeatWorker.RecordSuccessfulSync` / `RecordError`**: public seam —
  Faz 6'da Mikro writer'ın her başarılı sipariş yazımında bu method çağrılır.
  Şu an çağıran yok; bu nedenle `LastSyncAtUtc` heartbeat zamanıdır ve
  `LastError` null kalır.
- **Retry policy public surface**: `BuildRetryPolicy(IEnumerable<TimeSpan>)`
  overload'u testlerin hızlı (1ms) schedule ile policy'yi doğrulamasını
  sağlar. Production canonical 5/15/60/300s policy aynı sınıftan gelir
  (`CanonicalRetryDelays`).

---

## 8. Çalıştırma — hızlı başlangıç

```bash
cd /workspace/ErpBridge

# Unit tests (hermetik, DB gerekmez)
dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor

# Integration tests (canlı SQL Server gerekir)
cd tests
docker compose -f docker-compose.test.yml up -d
export ERPBridge_RUN_INTEGRATION=1
export ERPBridge_SQL_SERVER_16=localhost,14330
export ERPBridge_SQL_SERVER_15=localhost,14331
export ERPBridge_SQL_USER=sa
export ERPBridge_SQL_PASSWORD="ErpBridge_Test_2026!"
cd .. && dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor
cd tests && docker compose -f docker-compose.test.yml down
```

> `.NET 9-only` runtime'da `DOTNET_ROLL_FORWARD=LatestMajor` zorunludur.
> Test konteyner tabanlı CI'da bu env otomatik set edilir.
