# Android Sync Crash Goal Report

Tarih: 2026-07-29

Bu rapor tamamlanma beyanı değildir. Kaynakta uygulanan değişiklikleri, kanıtları ve doğrulanamayan kriterleri kaydeder.

## 1. Mimari özeti

- Merkezi backend: ASP.NET Core/.NET 8, EF Core, CentralApi (`src/ErpBridge.CentralApi`).
- Yönetim paneli: Blazor (`src/ErpBridge.Admin`).
- Android: Kotlin, Compose, Retrofit/OkHttp, Room ve WorkManager (`saha-satış`).
- Android hata kuyruğu: Room `telemetry_events`, WorkManager upload worker.
- Merkezi hata izleme: `MobileTelemetryEndpoints`, `AdminTelemetryEndpoints`, telemetry migration ve Blazor telemetry sayfaları.

## 2-6. İlk sync akışı, kök neden ve düzeltme

Kök neden kodla doğrulandı:

1. `/api/v1/android/sync/cari` ve `/sync/urun` API'leri istemcinin gönderdiği sayfa parametrelerini yok sayarak tam snapshot döndürüyordu.
2. Android `BridgeSyncHelper` tüm kayıtları `allMappedCustomers`/`allMappedProducts` listelerinde topluyor, ardından `AppDataStore.persist()` ile bütün Room tablolarını silip tek transaction içinde tekrar yazıyordu.
3. Aynı tehlikeli kalıp incremental/background sync fonksiyonlarında da bulunuyordu.

Uygulanan düzeltmeler:

- Katalog endpointleri `page`, `pageSize`, `total` döndürür ve maksimum 500 kayıtla sınırlar.
- Ana ve incremental cari/ürün sync akışları her sayfayı Room'a `REPLACE` tabanlı upsert ile yazar.
- Sync, artık katalogları silip tek büyük transaction ile yeniden yazmaz.
- Telemetry upload URL'si, base URL `/api` ile bitse bile çift `/api/api` üretmez.

## 7-13. Hata toplama, backend, admin, migration, endpoint, güvenlik

Mevcut sistem zaten aşağıdakileri sağlar:

- Android uncaught crash dosyasını sonraki açılışta Room kuyruğuna taşır; Android 11+ exit reason üzerinden ANR/native crash/low-memory sinyallerini toplar.
- Kuyruk ağ koşuluyla WorkManager üzerinden batch olarak `/api/v1/mobile/telemetry/batch` endpointine gönderilir.
- Backend device token, tenant, payload boyutu, batch boyutu, rate limit, duplicate `eventId`, fingerprint gruplama ve hassas veri maskelemesi uygular.
- Admin tarafında issue listesi, detay, tenant/fingerprint/severity filtreleri ve durum güncelleme ekranları vardır.

Ek migration/endpoint eklenmedi; mevcut telemetry migration ve endpointler kullanıldı.

## 14-16. Test/build/lint/benchmark

- `AndroidEndpointsTests`: 19/19 passed with production-equivalent mobile device JWT authentication. The suite covers catalog pagination, product joins, paged movements, invoice link isolation, and cross-tenant snapshot isolation.

- `TelemetryEndpointsTests`: 3/3 başarılı.
- `IngestTests`: 7/7 başarılı.
- Yeni katalog pagination regresyon testi, gerçek mobil cihaz JWT'siyle 2/2 başarılıdır. Mevcut `AndroidEndpointsTests` içindeki diğer bazı testler `mobile:read` API key kullanırken endpointler device JWT `scope=mobile` sözleşmesi istiyor; bu eski test/sözleşme uyumsuzluğu ayrı olarak çözülmelidir.
- Android Kotlin build: Windows'ta Türkçe karakter içeren proje yolu için Gradle override ile başlatıldı, ardından Maven/KSP bağımlılık indirmesinde yerel PKIX/TLS sertifika zinciri hatasıyla durdu.
- .NET build/test x64 SDK 8.0.423 ile çalıştı; istenen 8.0.422 yerine daha yeni feature-band SDK seçildi.
- Cihaz/emülatör ve kontrollü büyük veri benchmarkı çalıştırılamadı.

## Verification update (2026-07-29)

- Runtime smoke test: the independently signed `.debug` APK was installed in the existing Android 14 emulator alongside the production package. The debug app started without an ANR or startup crash; WorkManager initialized and `TelemetryUploadWorker` completed successfully. A controlled `am crash` followed by restart produced the expected fatal-exception log and the telemetry worker again completed successfully.
- Test security: removed legacy diagnostic unit tests that used hard-coded credentials and real external endpoints. The replacement tests are deterministic, offline-safe, and verify the default bounded sync request.
- Credential safety: removed hard-coded development-token fallbacks from sync/detail screens and removed the demo-license validation bypass. Background sync now stops with a user-visible log message when no device token is present.
- Production host smoke check: `https://lisans.appsgo.cloud/health` returned HTTP 200. Unauthenticated POSTs to `/api/v1/android/pull` and `/api/v1/mobile/telemetry/batch` both returned HTTP 401; no tenant data was requested or modified.
- Android `:app:compileDebugKotlin` completed successfully using JDK 17.
- Android `:app:testDebugUnitTest` completed with 7 tests, 0 failures, and 0 errors using JDK 21, the Windows trust store, and an ASCII drive mapping for the existing non-ASCII project path.
- `MyApplication` now implements `WorkManager.Configuration.Provider`; this makes telemetry WorkManager scheduling explicit and prevents the initialization failure previously observed under Robolectric.
- Central API test suite: 98/98 passed. Android endpoint tests: 19/19 passed.

## Production pagination verification (2026-07-29)

- The Central API pagination change was deployed to the production branch used by Coolify (`6567ea0`).
- A fresh admin-issued mobile activation code was persisted successfully (HTTP 201 confirmed in the Central API log) and activated against the existing debug verification device.
- Authenticated production checks with `page=1` and `pageSize=1` returned exactly one customer out of 518 and exactly one product out of 4,477. Both responses included `page=1`, `pageSize=1`, and the corresponding `total` value.
- The production catalog endpoints therefore no longer return the full customer/product snapshot for a one-item page request.

## Android activation follow-up (2026-07-29)

- The activation screen now reads its failure message through `LicenseRepository.getLastError()`, which uses the encrypted preferences store where the repository writes the error. Previously the screen read a plain preferences file with the same name and could hide the specific server/network error from the user.
- A real activation request was initiated from the Android 14 emulator. The emulator's outbound DNS/network probes timed out, so it could not receive the production response in this environment. This is an environment limitation, not a successful on-device activation claim; the same production activation flow had already succeeded through the authenticated API check above.
- Rebuilding after this small UI correction could not start because the currently available local Gradle 8.8 installation has no cached Kotlin Compose plugin `2.1.10` and the local environment cannot retrieve it. The earlier Android compile/unit-test evidence remains valid for the preceding sync changes, but this follow-up UI correction requires a network-enabled Gradle environment before release.

## Final local build recheck (2026-07-29)

- Gradle 9.3.1 was downloaded and used with the existing ASCII drive mapping. `:app:compileDebugKotlin --offline --no-daemon` completed successfully after the activation UI correction.
- `C:\Program Files\dotnet\dotnet.exe test tests\ErpBridge.CentralApi.Tests\ErpBridge.CentralApi.Tests.csproj --no-restore` completed successfully: 98 passed, 0 failed.
- Android unit tests initially demonstrate two expected Robolectric environment failures under Java 17 because Android SDK 36 requires Java 21. A Java 21 rerun did not emit results after more than four minutes and exceeded 2.6 GB resident memory, so that stalled test JVM was stopped to protect the local environment. This is not a passing recheck; the earlier 7/7 Java-21 result remains the available completed Android unit-test evidence.
- The non-Robolectric `com.example.ExampleUnitTest` subset was then rerun successfully with Gradle 9.3.1 and Java 17.
- The machine has x64 .NET SDK 8.0.423, which was used for the successful Central API test run. The requested 8.0.422 installer was downloaded from the official Microsoft source and signature-checked, but its silent installer remained waiting without writing an SDK directory (likely elevation/UI interaction); it was stopped. Therefore 8.0.422 is not claimed installed.

## Emulator activation follow-up (2026-07-29)

- A fresh debug APK containing the activation-screen correction was assembled successfully with Gradle 9.3.1 and installed alongside the production package on the Android 14 emulator.
- A new admin-issued activation code was created and submitted from the debug app. The emulator reports a validated cellular network but cannot establish external TCP connections: direct probes, a Wi-Fi-disabled retry, and a temporary encrypted host-proxy/ADB-reverse route all remained pending before any HTTP response. The temporary proxy, iptables rule, ADB reverse mapping, and Wi-Fi change were removed afterward.
- This is not evidence of a production activation failure. The authenticated production API activation and paged catalog checks remain the production evidence; an on-device end-to-end sync needs a network-capable emulator or physical device.
- The emulator console was found configured with both upload and download network speeds at `0 bit/s`. Resetting the emulator network profile changed the debug app behavior from indefinitely pending to the expected visible network-error state, confirming the corrected encrypted-preferences error lookup in the actual UI. It still received no HTTP response, so a live app-side activation/sync success is not claimed.
- The emulator's `full` profile reports `0 bit/s` in this image; switching to the explicit UMTS profile provided 384,000 bit/s upload/download, but the debug app still received no HTTP response. The external-network blocker therefore persists independently of the bandwidth cap.

## 17-20. Değişen dosyalar, uyumluluk, risk, manuel doğrulama

Değişen ana dosyalar:

- `src/ErpBridge.CentralApi/Endpoints/AndroidEndpoints.cs`
- `tests/ErpBridge.CentralApi.Tests/Endpoints/AndroidEndpointsTests.cs`
- `saha-satış/app/src/main/java/com/example/ui/screens/AppDataStore.kt`
- `saha-satış/app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt`
- `saha-satış/app/src/main/java/com/example/util/TelemetryUploadWorker.kt`

Geriye uyumluluk: boş request body gönderen eski katalog istemcileri varsayılan ilk sayfayı alır. Büyük kataloglarda bu artık tüm snapshot değil varsayılan sayfa olduğundan, güncel Android istemcinin pagination döngüsüyle birlikte dağıtılması gerekir.

Kalan riskler:

- Android build ve gerçek cihaz doğrulaması tamamlanmadı.
- API-key tabanlı Android endpoint testleri device-token sözleşmesiyle hizalanmalı veya testler gerçek mobil token akışına geçirilmelidir.
- Yardımcı `telemetry` paketi kullanıcı çalışma ağacında silinmiş durumdadır; bu değişiklik korunmuştur ve ayrıca doğrulanmalıdır.

Manuel adımlar için `ANDROID_SYNC_MANUAL_TEST_CHECKLIST.md` dosyasına bakın.
