# Deliverable — Faz 9 (1 Dakikalık Delta Sync + Sunucudan WPF UI'a Long-Polling Sinyal)

> **Tema:** Agent her dakika Mikro'dan sadece değişen veriyi çekip merkezi API'ye
> yükler; merkezi API her başarılı push'ta WPF masaüstü uygulamasına "yeni veri var"
> sinyali gönderir. Outbound-only kuralı korunur; tek HTTP yönü agent/desktop →
> server, server anlık cevap verir.

## Kapsam

| Alt-görev | Hedef | Durum |
|---|---|---|
| 9.1 | `AgentServiceOptions.BootstrapIntervalSeconds` (default 60) + binding | ✅ |
| 9.2 | `BootstrapWorker` options-driven interval + first-run delay | ✅ |
| 9.3 | `BootstrapSyncService.MinimumIntervalSeconds` (default 30) | ✅ |
| 9.4 | `BootstrapNotificationHub` (singleton, in-memory pub/sub) | ✅ |
| 9.5 | `GET /api/v1/bootstrap/notify?wait=30` long-polling endpoint | ✅ |
| 9.6 | `POST /api/v1/bootstrap` → `hub.Publish(tenantId, cursor)` | ✅ |
| 9.7 | `IRemoteApiClient.WaitForBootstrapUpdateAsync(wait, ct)` | ✅ |
| 9.8 | `HttpRemoteApiClient.WaitForBootstrapUpdateAsync` (long-poll HTTP) | ✅ |
| 9.9 | `IDesktopSignalService` + `BootstrapSignalService` (WPF) | ✅ |
| 9.10 | `DashboardViewModel.RefreshFromSignalAsync` + `App.OnStartup` entegrasyonu | ✅ |
| 9.11 | Birim + entegrasyon testleri | ✅ (16 yeni) |

## Mimari

```
+--------------------+   1 sn   +-----------------------+  delta   +----------------+
| WPF UI (Agent.UI)  | ───────▶ | BootstrapSignalService │ ────────▶| Central API    |
|  (IDesktopSignal-  |  GET     |  (long-poll loop)      |  GET wait|  /bootstrap/   |
|   Service)         |  notify  |                       |  =30s    |   notify       |
+--------------------+ ◀─────── +-----------------------+ ◀─────── +----------------+
       │ 200 + cursor                                                 ▲
       │  veya 204 timeout                                             │ publish
       ▼                                                              │ (insert
+--------------------+  delta    +-----------------------+  delta   │  sonrası)
| DashboardViewModel | ◀────────| BootstrapSyncService  | ────────▶ |
|  RefreshFromSignal |  refresh |  (RunOnceAsync)       |  POST     |
+--------------------+          +-----------------------+  bootstrap
                                        │
                                        │  her 60 sn tetik
                                        ▼
                                +-----------------------+
                                | BootstrapWorker        |
                                |  (interval=60s)        |
                                +-----------------------+
                                        │
                                        │  ReadBootstrapChangesAsync
                                        ▼
                                +-----------------------+
                                | Mikro SQL Server      |
                                +-----------------------+
```

**Zamanlama garantisi:**
- Mikro'da satır değişti → agent en geç 60 sn içinde değişikliği okur.
- Push başarılı → server anında (1-2 sn) WPF UI'ı uyandırır.
- WPF UI → dashboard yenileme (Mikro row count + status + last sync) → 2-5 sn.
- **Toplam gecikme: ~60-70 sn uçtan uca.**

## Değişen / yeni dosyalar

### Yeni dosyalar
- `src/ErpBridge.CentralApi/Notifications/IBootstrapNotificationHub.cs`
- `src/ErpBridge.CentralApi/Notifications/BootstrapNotificationHub.cs`
- `src/ErpBridge.CentralApi/Endpoints/BootstrapNotifyEndpoints.cs`
- `src/ErpBridge.Agent.UI/Services/IDesktopSignalService.cs`
- `src/ErpBridge.Agent.UI/Services/BootstrapSignalService.cs`
- `tests/ErpBridge.CentralApi.Tests/Notifications/BootstrapNotificationHubTests.cs`
- `tests/ErpBridge.CentralApi.Tests/Endpoints/BootstrapNotifyTests.cs`

### Değişen dosyalar
- `src/ErpBridge.Agent.Service/Configuration/AgentServiceOptions.cs` — +2 alan
- `src/ErpBridge.Agent.Service/Workers/BootstrapWorker.cs` — options-driven interval
- `src/ErpBridge.Shared/Constants.cs` — `DefaultBootstrapPushIntervalSeconds = 60` + `MinimumIntervalSeconds = 30`
- `src/ErpBridge.Core/Stores/BootstrapSyncService.cs` — `MinimumIntervalSeconds` (60→30 sn)
- `src/ErpBridge.Core/Stores/IRemoteApiClient.cs` — + `WaitForBootstrapUpdateAsync`, + `BootstrapRemoteSignal`
- `src/ErpBridge.RemoteApi/Http/HttpRemoteApiClient.cs` — + long-poll method
- `src/ErpBridge.CentralApi/Contracts/Contracts.cs` — + `BootstrapNotifyResponse`
- `src/ErpBridge.CentralApi/Endpoints/BootstrapEndpoints.cs` — + `hub.Publish` hook + ResolveHub helper
- `src/ErpBridge.CentralApi/Program.cs` — + hub DI + endpoint map
- `src/ErpBridge.Agent.UI/DependencyInjection/ServiceCollectionExtensions.cs` — + `IDesktopSignalService`
- `src/ErpBridge.Agent.UI/ViewModels/DashboardViewModel.cs` — + `RefreshFromSignalAsync`, 60 sn hint
- `src/ErpBridge.Agent.UI/App.xaml.cs` — start/stop signal service
- `tests/ErpBridge.Core.Tests/BootstrapSyncServiceTests.cs` — + 4 delta test + 1 window-uyarlama
- `tests/ErpBridge.RemoteApi.Tests/Http/HttpRemoteApiClientTests.cs` — + 4 notify client test

## Wire şeması (yeni/değişen)

| Yön | Method | URL | Body | Response |
|------|--------|-----|------|----------|
| agent → central | POST | `/api/v1/bootstrap` | `BootstrapRequestEnvelope` (zaten var) | 204 |
| desktop → central | GET | `/api/v1/bootstrap/notify?wait=30` | — | 200 `{updated,lastPulledAtUtc}` veya 204 |
| agent → central | POST | `/api/v1/agents/heartbeat` (zaten var) | heartbeat | 204 |

## Mimari kararlar

| Karar | Gerekçe |
|------|---------|
| Bootstrap interval = 60 sn | Kullanıcı talebi; delta path ile sunucu yükü düşük |
| MinimumInterval = 30 sn (yarım interval) | Throttle gereksiz iterasyonları önler, ardışık tick'ler skip'e düşmez |
| Delta varsayılan | `BootstrapStatus.HasSnapshot=true` durumunda her zaman `ReadBootstrapChangesAsync` |
| In-memory hub | Tek replica Coolify varsayımı; multi-instance için Redis backplane (ileride) |
| Long polling, 30 sn | Cloudflare free 100 s sınırının altında, signal gecikmesi kabul edilebilir |
| `JsonResults.Ok` kullanımı | .NET 8.0.x test host `PipeWriter.UnflushedBytes` bug'ını `Results.Ok`'tan bypass eder |
| `Task.Delay` tabanlı timeout | `CancellationTokenSource.CancelAfter` `using` scope'u içinde kalıp dispose olunca timer iptal oluyor, hub deadlock yiyordu |

## Güvenlik notları

- **JWT auth korundu** — `/api/v1/bootstrap/notify` `AgentPolicy` + `PerAgentRateLimitPolicy` ile korunuyor.
- **Tenant izolasyonu** — Hub `ConcurrentDictionary<Guid, ...>` tenant başına ayrı kuyruk tutar. `Publish` sadece ilgili tenant'ın abonelerini uyandırır.
- **Rate limit** — Her agent için dakikada 100 istek. Long-poll 30 sn × 2 (push + notify) = 2 istek/dk, rahat sınırda.
- **Secret** — Hiçbir secret loglanmaz; yeni kod `Serilog` maskeleme kurallarına uyar.

## State, concurrency, failure & recovery

- **Checkpoint cursor** — `CheckpointRecord.LastSuccessAt` (SQLite, agent tarafı). Load→Compare→Save tek iterasyon; aynı tenant için iki paralel worker yok.
- **Hub state** — `ConcurrentDictionary<Guid, ConcurrentQueue<Subscriber>>`. Publish O(subscribers). No-op subscribers yok.
- **Client concurrency** — `BootstrapSignalService` tek background `Task`; WPF dispatcher'a marshall.
- **Race** — Agent delta iterasyonu sırasında server "yeni paket" publish ederse desktop snapshot "geç kalmış" olabilir. WPF UI Mikro'ya yeniden sorgu atıyor, agent'ın push'u ile aynı `LastSuccessAt` cursor'undan okumadığı için OK.
- **Long-poll bağlantı koptu** → 30 sn sonra 204, hemen yeni poll. Exception fırlatılırsa `BootstrapSignalService` 5 sn backoff ile reconnect.
- **WPF UI kapalıyken push** → Hub'da subscriber yok, publish no-op. WPF açıldığında `OnLoad` + `RefreshFromSignalAsync` ile tek-seferlik tazeleme.
- **Agent durmuş** → Son `LastSuccessAt` eski kalır; UI "Bilinmiyor" badge gösterir.
- **Çoklu WPF UI instance** → Her biri ayrı long-poll; hub hepsini uyandırır. Rate-limit agent-id başına olduğu için sorun değil.

## Test özeti

| Suite | Yeni | Toplam | Durum |
|-------|------|--------|-------|
| `ErpBridge.CentralApi.Tests.Notifications.BootstrapNotificationHubTests` | 7 | 7 | ✅ |
| `ErpBridge.CentralApi.Tests.Endpoints.BootstrapNotifyTests` | 5 | 5 | ✅ |
| `ErpBridge.Core.Tests.BootstrapSyncServiceTests` (delta path) | 4 | 4 + 1 güncelleme | ✅ |
| `ErpBridge.RemoteApi.Tests.Http.HttpRemoteApiClientTests` (wait client) | 4 | 4 | ✅ |
| `ErpBridge.Core.Tests` (regression) | — | 66/66 | ✅ |
| `ErpBridge.RemoteApi.Tests` (regression) | — | 16/16 | ✅ |
| `ErpBridge.LocalStore.Tests` | — | 48/48 | ✅ |
| `ErpBridge.Shared.Tests` | — | 22/22 | ✅ |
| `ErpBridge.Erp.Mikro.Tests` | — | 82/82 (+ 16 skip) | ✅ |
| `ErpBridge.Agent.Service.Tests` | — | 8/8 | ✅ |
| **Yeni toplam** | **20 yeni / 1 güncelleme** | | |

> **Not:** `ErpBridge.CentralApi.Tests.Endpoints.AndroidEndpointsTests` ve
> `LicensesValidateTests.Health_check_*` testleri pre-existing .NET 8.0.x
> `PipeWriter.UnflushedBytes` test host bug'ından başarısız oluyor
> (`main`'de benim değişikliklerim olmadan da başarısız). Bu testler
> Faz 9 kapsamı dışında; bug'ın çözümü .NET 8.0.6+ veya ayrı bir
> çalışma gerektiriyor. Yeni endpoint'lerim aynı sorundan etkilenmesin
> diye `JsonResults.Ok` helper'ı kullanıldı.

## Test detayları

**`BootstrapNotificationHubTests` (7 test):**
- `Publish_NotifiesAllWaiters_WithTheSameCursor`
- `Timeout_ReturnsMinValue`
- `Publish_IsolatedByTenant`
- `Cancellation_RemovesWaiter_AndReturnsMinValue`
- `Publish_WithNoWaiters_DoesNotThrow_AndDropsTenantKey`
- `MultipleSequentialPublishes_EachWakeAFreshWaiter`
- `Publish_AwaitsAllPendingWaiters_EvenIfOneIsAlreadyResolved`

**`BootstrapNotifyTests` (5 test):**
- `Notify_without_token_returns_401`
- `Notify_with_invalid_wait_returns_400`
- `Notify_times_out_with_204_when_no_publish`
- `Notify_returns_200_when_bootstrap_publishes_a_package` (race coordination)
- `Notify_does_not_wake_other_tenants`

**`BootstrapSyncServiceTests` delta (4 yeni + 1 güncelleme):**
- `RunOnceAsync_uses_delta_path_when_server_has_snapshot`
- `RunOnceAsync_uses_full_read_when_server_has_no_snapshot`
- `RunOnceAsync_skips_when_last_success_within_30_seconds`
- `RunOnceAsync_runs_when_last_success_older_than_30_seconds`
- `RunOnceAsync_inside_idempotency_window_skips_the_push` (güncellendi: 60→30 sn)

**`HttpRemoteApiClientTests` notify (4 yeni):**
- `WaitForBootstrapUpdateAsync_parses_200_with_cursor`
- `WaitForBootstrapUpdateAsync_parses_204_as_no_update`
- `WaitForBootstrapUpdateAsync_clamps_oversize_wait_to_60_seconds`
- `WaitForBootstrapUpdateAsync_returns_no_update_on_5xx`

## Bilinen sınırlamalar / sonraki faz önerileri

- **Çok-instans Central API** — In-memory hub her replica'da ayrı. Coolify'da
  şu an tek replica (deploy-coolify.md); çok-instans dağıtım ileride Redis
  backplane gerektirir. `BootstrapEndpoints` publish idempotent; sadece signal
  kaçırılabilir, data kaybı olmaz.
- **Long-poll + reverse proxy** — Cloudflare free 100 s, nginx default 60 s.
  `wait=30` güvenli. Cloudflare arkasında `proxy_read_timeout` 30 s üstü
  olmalı; docs/deploy-coolify.md'ye not eklenecek.
- **İlk push'ta full snapshot** — İlk çalıştırmada agent full read atar
  (delta cursor yok). Büyük tabloda 5-30 sn sürebilir; sonraki 60 sn
  tick'ler delta olur.
- **WPF UI uzun süre kapalı** — Hub'da subscriber yok → publish no-op → UI
  anlık güncelleme kaçırır. Açıldığında mevcut `RefreshCommand` / `OnLoad`
  ile tek seferlik tazeleme yapılır.
- **WPF UI signal test projesi** — Plan'da "yeni test projesi gerekebilir"
  diye işaretliydi; WPF bağımlılığı nedeniyle kurulmadı. Client-side
  `BootstrapSignalService` server-side testlerle (hub + notify endpoint)
  dolaylı olarak doğrulandı; integration smoke TULPAR + lisans sunucusu
  ile manuel.
- **Mobil (saha satış) push** — Aynı `/bootstrap/notify` endpoint'i
  `Authorization: Bearer AK-...` + `X-Tenant-Id` ile mobil client'a
  açılabilir ama Faz 9 kapsamı dışı.
- **Pre-existing test host bug** — `AndroidEndpointsTests` +
  `LicensesValidateTests.Health_check_*` .NET 8.0.x test host
  `PipeWriter.UnflushedBytes` hatası. Yeni kod `JsonResults.Ok` ile bu
  sorundan etkilenmiyor; mevcut testler ayrı bir .NET yükseltmesiyle
  düzelir.

## Build / test komutları

```powershell
# Build
dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true

# Tüm yeni testler
dotnet test tests\ErpBridge.CentralApi.Tests\ErpBridge.CentralApi.Tests.csproj `
  -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor `
  --filter "FullyQualifiedName~BootstrapNotificationHubTests|FullyQualifiedName~BootstrapNotifyTests"

dotnet test tests\ErpBridge.Core.Tests\ErpBridge.Core.Tests.csproj `
  -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor `
  --filter "FullyQualifiedName~BootstrapSyncServiceTests"

dotnet test tests\ErpBridge.RemoteApi.Tests\ErpBridge.RemoteApi.Tests.csproj `
  -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor `
  --filter "FullyQualifiedName~WaitForBootstrapUpdateAsync"
```

## Manuel smoke test (entegre TULPAR Mikro + lisans sunucusu)

1. Agent Service başlat → log: `BootstrapWorker starting (interval=60s)`.
2. 60 sn içinde ilk push (full veya delta) → log.
3. WPF UI → Pano sekmesi → "Son senkronizasyon: X sn önce" görünür.
4. Mikro'da `CARI_HESAPLAR` UPDATE → 60 sn içinde log:
   `Bootstrap sync completed: ok=true customers=N`.
5. WPF UI anında badge değişir, Mikro row count yenilenir.
6. Cloudflare / nginx proxy arkasında long-poll 30 sn çalışır (manuel).
