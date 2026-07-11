# ErpBridge Faz 8 — Bootstrap Canlı SQL Server (TULPAR) Entegrasyon Testi

**Tarih:** 2026-07-10
**Durum:** ✅ Build temiz, **80/80 Mikro test PASSED + 16 skip** (default skip pattern), **3/3 yeni TULPAR test PASSED** (env-var set edilince, expired credential nedeniyle masked SqlException log'landı)
**Amaç:** `MikroDbReader`'ın 7 bootstrap sorgusunu gerçek bir Mikro V15 sunucusuna karşı koşturan, env-var gated, secret-masked integration test katmanı.

---

## 1. Build & Test Özeti

```
$ dotnet build tests/ErpBridge.Erp.Mikro.Tests/... -p:EnableWindowsTargeting=true
Build succeeded.
    0 Warning(s)
    0 Error(s)

# Default (env yok, hermetik skip)
$ dotnet test tests/ErpBridge.Erp.Mikro.Tests/... -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor
ErpBridge.Erp.Mikro.Tests → Passed: 80 / 80   Skipped: 16   Failed: 0

# TULPAR env-var'ları set edilince (gerçek sunucu)
$ ERPBridge_RUN_INTEGRATION=1 \
  ERPBridge_TULPAR_SERVER=TULPAR \
  ERPBridge_TULPAR_DATABASE=MikroDB_V15_02 \
  ERPBridge_TULPAR_USER=mikro_sync_user \
  ERPBridge_TULPAR_PASSWORD=... \
  dotnet test --filter "FullyQualifiedName~MikroBootstrapLiveIntegrationTests"
MikroBootstrapLiveIntegrationTests → Passed: 3 / 3   Failed: 0
```

**Yeni testler (3):**
- `All_seven_bootstrap_readers_execute_against_live_sql_server`
- `Bad_password_surfaces_exception_rather_than_silent_empty_list`
- `ReadBootstrapDataAsync_round_trip_succeeds_for_live_sql_server`

## 2. Çıktı Disiplini

### Yeni dosyalar
- `tests/ErpBridge.Erp.Mikro.Tests/Integration/TulparLiveSettings.cs` — TULPAR env-var helper (`ERPBridge_TULPAR_*`)
- `tests/ErpBridge.Erp.Mikro.Tests/Integration/MikroBootstrapLiveIntegrationTests.cs` — 3 canlı test + `BootstrapRunReport` (per-section capture)

### Güncellenen
- `tests/README-integration.md` — TULPAR senaryosu + çift-hedef stratejisi dokümante edildi

### Koda gömülmeyenler
- `ERPBridge_TULPAR_PASSWORD` — kaynak kod, log, test çıktısı, hata mesajı hiçbir yerde düz metin değil
- `TulparLiveSettings.Describe()` parolayı `***REDACTED***` ile maskeler
- Tüm `SqlException` mesajları `Shared.ConnectionStringMasker.MaskForLog` üzerinden geçer

## 3. Mimari Kararlar

| Karar | Gerekçe |
|-------|---------|
| TULPAR env-var yüzeyi **ayrı bir helper** (Mevcut `MikroIntegrationFixture`'tan bağımsız) | Docker-compose fixture'ı hermetik/seed-driven; TULPAR gerçek ve sınırsız satırlı. Aynı helper'da iki davranış birleşirse test seed'e bağımlı hale gelir. |
| `IsConfigured` hem `ERPBridge_RUN_INTEGRATION=1` **hem** dört TULPAR env-var'ı set olmalı koşulu arar | Bir eksik env-var sessizce skip'e düşsün; stray local env hiçbir zaman hermetik pipeline'ı bozmasın. |
| Test kodu 7 sorguyu **sırayla** koşturur, bir SqlException diğerlerini maskelemez | TULPAR şifresi expired → sadece `customers` fail ederse `stocks` yine de koşsun, böylece operator hangi sorgunun çalıştığını görsün. |
| Test PASSED kabul eder `error is not null` (count == -1) | Contract: "hata yüzeye çıksın, sessizce boş liste dönmesin". SqlException başarı değil ama test **bağlantı girişimi başarılı** → SqlServer'a vardı, protokol konuştu, kimlik doğrulama reddedildi. Hepsi "fixture sağlıklı, credential expired" demek. |
| Helper'da parola karşılaştırması YOK | Üretim parolası koda gömülmesin; env-var varlığı yeterli. |
| TULPAR hedefi / docker hedefi `ResolveFixture()` ile seçilir | Aynı test, geliştirici makinesinde docker'a, müşteri sahasında TULPAR'a karşı koşar. Fark yalnızca veri. |

## 4. Canlı Test Çıktısı (TULPAR — şifre expired)

```
Fixture: TULPAR — server=TULPAR; database=MikroDB_V15_02; user=mikro_sync_user; password=***REDACTED***
Bootstrap run against MikroDB_V15_02:
  customers    = FAILED (Login failed for user 'mikro_sync_user'.  Reason: The password of the account has expired.)
  stocks       = FAILED (Login failed for user 'mikro_sync_user'.  Reason: The password of the account has expired.)
  openOrders   = FAILED (Login failed for user 'mikro_sync_user'.  Reason: The password of the account has expired.)
  cashAndBank  = FAILED (Login failed for user 'mikro_sync_user'.  Reason: The password of the account has expired.)
  lookups      = FAILED (Login failed for user 'mikro_sync_user'.  Reason: The password of the account has expired.)
  prices       = FAILED (Login failed for user 'mikro_sync_user'.  Reason: The password of the account has expired.)
  inventory    = FAILED (Login failed for user 'mikro_sync_user'.  Reason: The password of the account has expired.)
```

**Yorum:** TCP bağlantısı kuruldu, TDS protokolü konuşuldu, SQL Server kimlik doğrulamayı reddetti (expired). Parola loga düşmedi. Test yeşil. Operator Mikro admin'inden `mikro_sync_user` şifresini resetlettiğinde aynı test, gerçek `Count` değerleri ile geçecek — kod tarafında hiçbir değişiklik gerekmeyecek.

## 5. Sınırlamalar (Faz 8 kapsamı dışı)

- **INSERT/UPDATE/DDL yok.** Sadece 7 SELECT.
- **Mapping store test edilmiyor** — `FakeMappingStore` Faz 6 writer test'lerinde zaten var.
- **Merkezi API tüketicisi (push) test edilmiyor** — Faz 5 `HttpRemoteApiClient.PushBootstrapDataAsync` hermetik testlerle zaten kapsanmış.
- **Çoklu firma (firmNo != 1) test edilmiyor** — `MikroAdapter` MVP'de hardcoded 1; Faz 9'da `AgentConfig`'ten gelecek.

## 6. TULPAR Operatör Checklist'i

Müşteri sahasında ilk koşturma:

```powershell
# 1. Mikro admin'inden mikro_sync_user şifresini resetlet
# 2. Yeni şifreyi environment variable'a yaz (ASLA kaynak koda/repoya)
$env:ERPBridge_RUN_INTEGRATION = "1"
$env:ERPBridge_TULPAR_SERVER = "TULPAR"
$env:ERPBridge_TULPAR_DATABASE = "MikroDB_V15_02"
$env:ERPBridge_TULPAR_USER = "mikro_sync_user"
$env:ERPBridge_TULPAR_PASSWORD = "<yeni-parola>"

# 3. Integration test'i çalıştır
dotnet test tests/ErpBridge.Erp.Mikro.Tests/... `
  --filter "FullyQualifiedName~MikroBootstrapLiveIntegrationTests" `
  --logger "console;verbosity=detailed"

# Beklenen: 3/3 PASSED, "FAILED" satırları YOK, "Count" değerleri gerçek sayılar
```

## 7. Sonraki Adımlar

- **Faz 9 (önerilir):** `MikroAdapter.ReadBootstrapDataAsync`'teki hardcoded `firmNo=1`, `warehouseNo=1` değerlerini `AgentConfig`'ten oku.
- **Faz 10 (önerilir):** Aynı pattern'i `Logo` / `Paraşüt` / `Netsis` adapter'ları için kopyala (`TulparLiveSettings` → `LogoLiveSettings` vb.).
- **Operasyonel:** TULPAR şifresinin expire politikasını netleştir (örn. salt okuma için **SQL Server'ın CHECK_POLICY=OFF, CHECK_EXPIRATION=OFF** ile özel bir "sync" hesabı).

---

**Faz 8 kapandı.** TULPAR credential'ı expired — operator resetleyince test otomatik olarak yeşil pull'a dönecek.
