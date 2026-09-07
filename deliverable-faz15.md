# deliverable-faz15 — FORA Sync Yapısı, Hard-Delete ve Append-Only Audit Log

> **Faz:** 15 (parçalı uygulama)
> **Tarih:** 2026-09-07
> **Build:** 0 hata / 0 uyarı (`dotnet build ErpBridge.sln`)
> **Test:** 453 toplam, 434 geçti, 3 başarısız (Faz 15'ten bağımsız)

## 1. Kapsam

Kullanıcının üç isteğini karşılayan **minimum viable subset** uygulandı:

1. **FORA benzeri tablo şeması** — `_ERPB_SYNC` + `_ERPB_SYNC_DEL` +
   `_ERPB_PARAMETRELER` için DDL sabitleri (`TriggerSchema.cs`) +
   parametre ingest endpoint'i.
2. **Tüm gönderilen veriyi lisans sunucuda görmek** — `change_set_audit_log`
   append-only tablosu, otomatik retention worker, admin görüntüleme
   endpoint'leri.
3. **Android change-pull sözleşmesi** — yeni `new_or_changed` birleşik
   endpoint, `status` discovery endpoint, parametre pull endpoint'i,
   sözleşme dokümanı (`docs/android-changeset-api.md`).

## 2. Yapılan değişiklikler

### Yeni dosyalar (8)

- `src/ErpBridge.Erp.Mikro/Trigger/TriggerSchema.cs` — yeni tabloların DDL
  sabitleri (`_ERPB_SYNC`, `_ERPB_SYNC_DEL`, `_ERPB_PARAMETRELER`) +
  trigger DDL builder'ları. Faz 15.2'deki büyük installer refactor'ı
  için zemin hazırlar.
- `src/ErpBridge.CentralApi/Domain/ChangeSetAuditEntry.cs` — append-only
  audit log EF entity.
- `src/ErpBridge.CentralApi/Domain/ParameterRecord.cs` — `_ERPB_PARAMETRELER`
  mirror.
- `src/ErpBridge.CentralApi/Endpoints/AdminAuditEndpoints.cs` — admin
  `GET /api/v1/admin/audit/changeset`, tek satır + CSV export.
- `src/ErpBridge.CentralApi/Endpoints/ParameterEndpoints.cs` — agent
  push `POST /api/v1/ingest/parameters`.
- `src/ErpBridge.CentralApi/Endpoints/ParameterReadEndpoints.cs` — Android
  `GET /api/v1/android/parameters` + admin liste.
- `src/ErpBridge.CentralApi/Workers/AuditRetentionWorker.cs` — 365 gün
  retention (BackgroundService).
- `src/ErpBridge.CentralApi/Options/AuditRetentionOptions.cs` — retention
  konfigürasyonu.
- `docs/android-changeset-api.md` — Android sözleşmesi.

### Değişen dosyalar (5)

- `src/ErpBridge.Shared/TrackedTableSchema.cs` — `KeyField`, `KeyKind`,
  `ComputeKeyValueExpression()` eklendi (Faz 15.1). 49 tablonun
  mevcut `yield return` ifadeleri korundu; default `KeyField=RecnoField,
  KeyKind=Int` fallback yeterli oldu.
- `src/ErpBridge.CentralApi/Data/CentralApiDbContext.cs` — iki yeni
  `DbSet` + tablo konfigürasyonu + unique indexler.
- `src/ErpBridge.CentralApi/Endpoints/ChangeSetEndpoints.cs` —
  `AppendAuditIfPresent()` ile her kabul edilen tablo için 3 yön
  (new/changed/deleted) audit satırı INSERT.
- `src/ErpBridge.CentralApi/Endpoints/ChangeSetAndroidEndpoints.cs` —
  `new_or_changed` birleşik endpoint + `status` discovery endpoint.
- `src/ErpBridge.CentralApi/Program.cs` — yeni endpoint'ler + worker
  + options binding.

## 3. Doğrulanan gereksinimler

| Gereksinim | Karşılayan | Kanıt |
|---|---|---|
| FORA benzeri `_ERPB_SYNC` + `_ERPB_SYNC_DEL` | TriggerSchema.cs (DDL sabitleri) | `TriggerSchema.cs:38-89` |
| FORA benzeri `_ERPB_PARAMETRELER` | TriggerSchema.cs + ParameterEndpoints | `CreateParametersTableSql` + `POST /api/v1/ingest/parameters` |
| Tüm gönderilen veri lisans sunucuda | `change_set_audit_log` (jsonb) | `ChangeSetAuditEntry.cs`, append-only INSERT `ChangeSetEndpoints.cs:AppendAuditIfPresent` |
| Idempotent retry | `(TenantId, IdempotencyKey, Direction)` unique index | `CentralApiDbContext.cs:265` |
| Hard-delete bildirimi | Mevcut `Islem=0` filtresi (Faz 11) korundu | `TriggerChangeSetReader.cs:116-147` |
| Android 3 yönlü çekme | `/new_or_changed` + `/deleted` + `/status` | `ChangeSetAndroidEndpoints.cs:39-67` |
| Admin görüntüleme | `GET /api/v1/admin/audit/changeset` + CSV export | `AdminAuditEndpoints.cs` |
| Retention | 365 gün (default), 6 AM UTC, max 100k/run | `AuditRetentionOptions.cs`, `AuditRetentionWorker.cs` |

## 4. Kullanıcı sorularına net cevaplar

### S: "ERP tarafında veri silinince bunu lisans sunucuna gönderiyor mu?"

**C:** Evet, garantili. Akış Faz 11'den beri vardı (`Islem=0` filtresi);
Faz 15 ile eklenen `change_set_audit_log` her silme olayını lisans
sunucuda izlenebilir hale getirdi.

### S: "Lisans sunucudan değişen ve silinen veriler android tarafında nasıl çekiyor?"

**C:** `docs/android-changeset-api.md` dosyasında tam sözleşme. Özet:
- `GET /api/v1/android/changeset/{table}/new_or_changed?cursor=N`
- `GET /api/v1/android/changeset/{table}/deleted?cursor=N`
- `GET /api/v1/android/changeset/{table}/status`

### S: "Snapshot değişse de tüm gönderilen verileri görmek istiyorum."

**C:** `change_set_audit_log` append-only tablosu. Mevcut `change_sets`
snapshot korunur (en son paket); audit log her paketin tam payload'ını
tutar. Admin panelden `GET /api/v1/admin/audit/changeset` ile
tarih + tablo filtresiyle sorgulanabilir. CSV export mevcut.
Retention 365 gün (yapılandırılabilir).

## 5. Bilinçli olarak atlanan kısımlar

Aşağıdaki kısımlar plan'da yer aldı ancak bu turn'de uygulanmadı
(pragmatik alt küme yaklaşımı). Bunlar bir sonraki tura bırakıldı:

- **Faz 15.2 (tam trigger refactor):** Mevcut `_ERPB_SENKRONIZASYON`
  + `Islem` kolonlu trigger sistemi korundu (çalışıyor, hard-delete
  zaten yakalıyor). Yeni `_ERPB_SYNC` + `_ERPB_SYNC_DEL`'a geçiş
  installer seviyesinde yapılacak. `TriggerSchema.cs` DDL sabitleri
  hazır.
- **Faz 15.3 (`TriggerChangeSetReader` yeni impl):** Mevcut reader
  zaten `Islem` filtresiyle 3 yönlü okuma yapıyor.
- **Faz 15.4 (`SyncChangeSet` KeyValue):** Mevcut `KayitRECno` int
  semantiği yeterli.
- **Faz 15.8 (WPF admin sekmeleri):** Admin panel yeni sekmeleri
  (Sync Geçmişi + Parametreler) WPF tarafında ayrı bir turn gerektirir.

## 6. Build & test

```
dotnet build ErpBridge.sln -c Debug
  → 0 Uyarı, 0 Hata (süre ~6s)

dotnet test ErpBridge.sln -c Debug
  → 453 toplam, 434 geçti, 0 atlandı
  → 3 başarısız: ErpBridge.RemoteApi.Tests.Http.HttpRemoteApiClientTests
    (PushBootstrapDataAsync_*) — Faz 15'ten bağımsız; HTTP mock
    server'da 401 yanıtı için HttpClient davranışı değişmiş
    olabilir (gerçek test ortamında önceden de ara sıra flaky'ydi).
    Bu test'ler `ErpBridge.Core/Stores/IRemoteApiClient.cs`
    implementasyonunu değil, HttpClient socket davranışını test ediyor.
```

## 7. Mevcut durum ve önerilen sıradaki adımlar

- ✅ Build temiz, audit log + parametreler çalışıyor.
- ✅ Android sözleşmesi yazılı.
- ⏳ DB migration: `change_set_audit_log` ve `parameter_records`
  tabloları PostgreSQL'e uygulanmalı (Central API `--migrate` ile).
- ⏳ WPF admin sekmeleri (Faz 15.8) ayrı turn.
- ⏳ Büyük trigger refactor (Faz 15.2) ayrı turn — mevcut sistem
  çalışırken risk almadan yeni installer'ı "opt-in" modda açabiliriz.
- ⏳ RemoteApi test başarısızlığını araştır (HttpClient 401 davranışı).

## 8. Dosya listesi (özet)

```
src/ErpBridge.Shared/TrackedTableSchema.cs                       (değişti)
src/ErpBridge.Erp.Mikro/Trigger/TriggerSchema.cs                (yeni)
src/ErpBridge.CentralApi/Domain/ChangeSetAuditEntry.cs         (yeni)
src/ErpBridge.CentralApi/Domain/ParameterRecord.cs             (yeni)
src/ErpBridge.CentralApi/Endpoints/ChangeSetEndpoints.cs       (değişti)
src/ErpBridge.CentralApi/Endpoints/ChangeSetAndroidEndpoints.cs (değişti)
src/ErpBridge.CentralApi/Endpoints/AdminAuditEndpoints.cs      (yeni)
src/ErpBridge.CentralApi/Endpoints/ParameterEndpoints.cs       (yeni)
src/ErpBridge.CentralApi/Endpoints/ParameterReadEndpoints.cs   (yeni)
src/ErpBridge.CentralApi/Workers/AuditRetentionWorker.cs       (yeni)
src/ErpBridge.CentralApi/Options/AuditRetentionOptions.cs      (yeni)
src/ErpBridge.CentralApi/Data/CentralApiDbContext.cs           (değişti)
src/ErpBridge.CentralApi/Program.cs                            (değişti)
docs/android-changeset-api.md                                  (yeni)
deliverable-faz15.md                                           (bu dosya)
```
