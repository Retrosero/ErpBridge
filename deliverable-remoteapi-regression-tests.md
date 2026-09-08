# deliverable-remoteapi-regression-tests — HttpClient 401/403/500/timeout ağ sınır durumu regresyon testleri

> **Kapsam:** `ErpBridge.RemoteApi.Http.HttpRemoteApiClient.PushBootstrapDataAsync` üzerindeki
> Polly v7 retry + HttpClient sınır durumları için 6 yeni deterministik regresyon testi.
> **Tarih:** 2026-09-08
> **Build:** 0 uyarı / 0 hata (`dotnet build ErpBridge.sln`)
> **Test:** RemoteApi paketi 26/26 PASSED; tam çözüm 461 PASSED + 16 SKIPPED (integration)

## 1. Arka plan

`deliverable-faz15.md:122-128` notundaki uyarı: Faz 15 döneminde 3 RemoteApi
`PushBootstrapDataAsync_*` testi gerçek HTTP mock server'da 401 için
flaky'ydi. Şu an yeşil ama **regresyon koruması yoktu** — Polly retry
sözleşmesinin (401/403 retry-dışı, 500/network/timeout retry-içi) ve
HttpClient 4xx/5xx classification sözleşmesinin kapsamı zayıftı.

Bu deliverable, sözleşmeyi **deterministik mock'la** kilitleyen 6 yeni
test ekler. Mevcut 20 test bozulmaz; toplam 26/26 yeşil.

## 2. Yapılan değişiklikler

### Değişen dosya (1)

- `tests/ErpBridge.RemoteApi.Tests/Http/HttpRemoteApiClientTests.cs`
  - **+1 `using Microsoft.Extensions.Http;`** — `PolicyHttpMessageHandler`
    için (derleme: `Microsoft.Extensions.Http.Polly.dll`, zaten `ErpBridge.RemoteApi`
    üzerinden transitively referans).
  - **+1 yardımcı `BuildClientWithRetryPolicy(responder, retries)`** — gerçek Polly
    v7 policy'yi `BuildRetryPolicy(delays)` üzerinden kısa (1 ms) back-off ile
    `PolicyHttpMessageHandler` aracılığıyla mock'un üstüne sarar. Üretim
    davranışı değişmez: `ServiceCollectionExtensions.BuildRetryPolicy(IEnumerable<TimeSpan>)`
    zaten test edilebilirlik için public static overload.
  - **+6 yeni test** — aşağıdaki bölüm.

### Değişmeyen dosyalar

- `src/ErpBridge.RemoteApi/**` — runtime davranışında **sıfır** değişiklik
  (görev kapsamı: sadece test paketi).
- Diğer paketler — dokunulmadı.

## 3. Yeni testler (6)

Tüm yeni testler `HttpRemoteApiClientTests.cs` içinde, "Phase X: HttpClient
retry / 4xx / 5xx / network regression tests" bölümünde.

| # | Test adı | Doğrulanan sözleşme |
|---|----------|---------------------|
| 1 | `PushBootstrapDataAsync_401_throws_permanent_exception_without_retry` | 401 → `BootstrapPermanentPushException`, call count = 1 (Polly retry YAPMAZ çünkü 4xx predicate dışı). |
| 2 | `PushBootstrapDataAsync_403_throws_permanent_exception_without_retry` | 403 → `BootstrapPermanentPushException`, call count = 1 (401 ile simetrik). |
| 3 | `PushBootstrapDataAsync_retries_on_500_then_succeeds` | İlk /start 500 → Polly 1 kez retry → 2. /start 200 → push başarıyla tamamlar, startCallCount = 2. |
| 4 | `PushBootstrapDataAsync_returns_failure_on_500_after_max_retries` | /start sürekli 500 → 1 initial + 3 retry = 4 çağrı, son 500 → `BootstrapPermanentPushException(ErrorCode="PERSISTENT_500")`. |
| 5 | `PushBootstrapDataAsync_returns_HttpRequestException_after_max_retries_on_network_failure` | Handler `HttpRequestException` throw eder → 1 + 3 retry = 4 throw, son exception `HttpRequestException` olarak propagate olur (`HttpRemoteApiClient` network exception'ı sarmaz; `BootstrapSyncService` orchestrator karar verir). |
| 6 | `PushBootstrapDataAsync_returns_TransientPushException_on_timeout_TaskCanceledException` | Handler `TaskCanceledException` throw eder → 1 + 3 retry = 4 throw, son exception `SendNoContentAsync` tarafından `TransientPushException`'a çevrilir (caller token'ı iptal edilmemiş). |

### Sözleşmeye dair notlar

- **Test 4 — 500'in exception türü:** `/start` çağrısı `SendAsync<T>(classifyBootstrapFailure: true)`
  üzerinden gider; bu metod 5xx dahil her non-success için `BootstrapPermanentPushException`
  fırlatır (üretim kodu davranışı, kasıtlı bir bug değil — `/start` başarısızsa tüm push
  abort edilir). `/chunks` ve `/complete` çağrıları `SendNoContentAsync` üzerinden gider ve
  5xx'i `TransientPushException`'a çevirir; bu kontrat `BootstrapSyncService`
  integration testlerinde zaten doğrulanmaktadır. Yeni test, gerçek kod yolunu kilitler.
- **Test 5 — HttpRequestException propagation:** `HttpRemoteApiClient` ağ hatalarını sarmaz;
  `BootstrapSyncService` orchestrator'ı yakalar ve section retry kararı verir. Bu test
  HttpClient → Polly → handler zincirinin doğru çalıştığını, HttpRequestException'ın
  retry budget dolduktan sonra caller'a ulaştığını doğrular.

## 4. Doğrulama (build + test)

### Build (tam çözüm)

```
dotnet build ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true
...
ErpBridge.LocalStore -> ...ErpBridge.LocalStore.dll
ErpBridge.RemoteApi -> ...ErpBridge.RemoteApi.dll
ErpBridge.Erp.Mikro -> ...ErpBridge.Erp.Mikro.dll
ErpBridge.Admin -> ...ErpBridge.Admin.dll
ErpBridge.RemoteApi.Tests -> ...ErpBridge.RemoteApi.Tests.dll
ErpBridge.Erp.Mikro.Tests -> ...ErpBridge.Erp.Mikro.Tests.dll
ErpBridge.Agent.Service -> ...ErpBridge.Agent.Service.dll
ErpBridge.LocalStore.Tests -> ...ErpBridge.LocalStore.Tests.dll
ErpBridge.Core.Tests -> ...ErpBridge.Core.Tests.dll
ErpBridge.Admin.Tests -> ...ErpBridge.Admin.Tests.dll
ErpBridge.Agent.Service.Tests -> ...ErpBridge.Agent.Service.Tests.dll
ErpBridge.Agent.UI -> ...ErpBridge.Agent.UI.dll
ErpBridge.CentralApi -> ...ErpBridge.CentralApi.dll
ErpBridge.CentralApi.Tests -> ...ErpBridge.CentralApi.Tests.dll

Oluşturma başarılı oldu.
    0 Uyarı
    0 Hata
```

### Test (RemoteApi paketi — sıkı scope doğrulaması)

```
dotnet test tests/ErpBridge.RemoteApi.Tests/ErpBridge.RemoteApi.Tests.csproj \
  -c Debug -p:EnableWindowsTargeting=true --no-build

C:\Users\Gürbüz Oyuncak\Documents\GitHub\ErpBridge\tests\ErpBridge.RemoteApi.Tests\bin\Debug\net10.0\ErpBridge.RemoteApi.Tests.dll
Başarılı!  - Başarısız:     0, Başarılı:    26, Atlanan:     0, Toplam:    26, Süre: 767 ms
```

Önceki durum: 20/20. Sonraki: **26/26 (+6)**.

### Test (tam çözüm — yan etki yokluğu)

```
dotnet test ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true --no-build

Başarılı!  - Başarısız:     0, Başarılı:    26, Atlanan:     0, Toplam:    26, Süre: ~1 s   - ErpBridge.RemoteApi.Tests.dll
Başarılı!  - Başarısız:     0, Başarılı:    50, Atlanan:     0, Toplam:    50, Süre: ~0 s   - ErpBridge.LocalStore.Tests.dll
Başarılı!  - Başarısız:     0, Başarılı:    22, Atlanan:     0, Toplam:    22, Süre: ~0 s   - ErpBridge.Shared.Tests.dll
Başarılı!  - Başarısız:     0, Başarılı:     8, Atlanan:     0, Toplam:     8, Süre: ~0 s   - ErpBridge.Agent.Service.Tests.dll
Başarılı!  - Başarısız:     0, Başarılı:    17, Atlanan:     0, Toplam:    17, Süre: ~1 s   - ErpBridge.Admin.Tests.dll
Başarılı!  - Başarısız:     0, Başarılı:    73, Atlanan:     0, Toplam:    73, Süre: ~15 s  - ErpBridge.Core.Tests.dll
Başarılı!  - Başarısız:     0, Başarılı:   148, Atlanan:     0, Toplam:   148, Süre: ~22 s  - ErpBridge.CentralApi.Tests.dll
Başarılı!  - Başarısız:     0, Başarılı:   117, Atlanan:    16, Toplam:   133, Süre: ~40 s  - ErpBridge.Erp.Mikro.Tests.dll
```

**Toplam: 461 PASSED + 16 SKIPPED, 0 FAILED.** (16 skip = MSSQL docker integration
testleri; `ERPBridge_RUN_INTEGRATION=1` olmadan skip edilir — beklenen davranış.)

## 5. Kabul kriterleri kontrolü

| Kriter | Durum | Kanıt |
|--------|-------|-------|
| `HttpRemoteApiClientTests.cs` ≥ 26 test içerir | ✅ | 20 + 6 = 26 test, `--list-tests` çıktısı doğruladı |
| Tüm yeni testler geçer | ✅ | RemoteApi test run 26/26 PASSED |
| `dotnet build ErpBridge.sln` → 0 uyarı, 0 hata | ✅ | yukarıdaki build çıktısı |
| `dotnet test tests/ErpBridge.RemoteApi.Tests` → tümü yeşil | ✅ | 26/26 PASSED |
| Mevcut 20 test bozulmaz | ✅ | 20 orijinal test de yeşil |
| Kısa deliverable (kök dizinde) | ✅ | bu dosya |

## 6. Bilinçli sınırlamalar

- **Runtime davranışı değişmedi:** Üretim kodunda (`src/ErpBridge.RemoteApi/**`)
  sıfır değişiklik. `ServiceCollectionExtensions.BuildRetryPolicy(IEnumerable<TimeSpan>)`
  overload'u zaten test edilebilirlik için public static olarak mevcuttu;
  yeni bir interface veya factory injection'a gerek kalmadı.
- **Test edilebilirlik için interface ekleme önerisi (görev notu 4):** Mevcut
  `BuildRetryPolicy(delays)` public static overload yeterli olduğu için
  `IPollyPipelineProvider` interface'i eklenmedi. Görevin "gerekiyorsa ekle"
  kısmı zaten "opsiyonel" idi.
- **/start 5xx'i permanent olarak sınıflandırma (üretim kodu kararı):**
  `HttpRemoteApiClient.SendAsync<T>` `classifyBootstrapFailure: true` ile
  5xx'i `BootstrapPermanentPushException` fırlatıyor. Bu, BootstrapSyncService'in
  legacy fallback'ine (`HTTP_500` koduyla) yol açar; legacy'de de 5xx retry
  Polly NoOp policy'si nedeniyle yapılmaz. Bu kasıtlı bir üretim davranışı
  (görev kapsamı: üretim değişikliği yok) ve test 4'te bu davranış kilitlendi.

## 7. Dosya listesi (özet)

```
tests/ErpBridge.RemoteApi.Tests/Http/HttpRemoteApiClientTests.cs   (değişti — 6 yeni test + 1 yardımcı)
deliverable-remoteapi-regression-tests.md                          (yeni — bu dosya)
```

Üretim kodu: dokunulmadı.
