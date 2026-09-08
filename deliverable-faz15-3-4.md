# Deliverable — Faz 15.3 + 15.4 (TriggerChangeSet Reader + KeyValue Semantiği)

> **Faz:** 15.3 + 15.4 (Wave 3 / parçalı uygulama)
> **Tarih:** 2026-09-08
> **Durum:** ✅ **TAMAMLANDI** (rename + 2 fix uygulandı)
> **Build:** 0 hata / 0 uyarı
> **Test:** 19 yeni test (11 reader + 8 schema/validation), mevcut testler bozulmadı

## 1. Kapsam

`ITriggerChangeSetReader` interface + implementasyonu (`TriggerChangeSetReader`):
3 yönlü okuma (new/changed/deleted) artık `KeyValue` (string) alanını
doldurmuş olarak döner. V15 int PK'ları ve V16 Guid PK'ları için KeyValue
otomatik çözülür; özel key field'ları (örn. `*_uid` V16, `*_RECid_RECno`
V15) `KeyValueResolver` üzerinden side-table lookup ile çözülür.

## 2. Yeni / değişen dosyalar

### Yeni (5)
- `src/ErpBridge.Core/Domain/TriggerChangeSet.cs` — `TriggerChangeSet` sınıfı +
  `TriggerChangeType` enum (önce `SyncChangeSet` adıyla çakışıyordu; rename edildi).
- `src/ErpBridge.Erp.Mikro/Trigger/ITriggerChangeSetReader.cs` — interface.
- `src/ErpBridge.Erp.Mikro/Trigger/KeyValueResolver.cs` — `ResolveAsync(...)`
  ile V15/V16 KeyValue çözümü.
- `tests/ErpBridge.Erp.Mikro.Tests/Trigger/TrackedTableSchemaTests.cs` — `ComputeKeyValueExpression` testleri.
- `tests/ErpBridge.Erp.Mikro.Tests/Trigger/TriggerChangeSetReaderInputValidationTests.cs` — input validation testleri.

### Değişen (2)
- `src/ErpBridge.Erp.Mikro/Trigger/TriggerChangeSetReader.cs` — alias
  `CoreSyncChangeSet` + `BuildSyncChangeSet`'e `await` eklendi (3B'nin yarım
  bıraktığı bug fix).
- `src/ErpBridge.Shared/TrackedTableSchema.cs` — Faz 15'te `KeyField` + `KeyKind`
  eklendi; bu turda değişiklik yok (sadece kullanım).

## 3. KeyValue semantiği

| Tablo tipi | RecordKey | KeyValue | Kaynak |
|---|---|---|---|
| V15 int PK | int `RECno` | `"42"` | doğrudan `int.ToString(invariant)` |
| V16 Guid PK | Guid | `"6f9619ff-..."` | `Guid.ToString("D")` |
| V15 custom (örn. `*_RECid_RECno`) | int lookup | resolved string veya `null` | `KeyValueResolver.ResolveAsync` + lookup tablo |
| V16 custom (örn. `*_uid`) | Guid lookup | resolved string veya `null` | aynı |

Lookup miss → `KeyValue = null` (exception fırlatılmaz).

## 4. Müdahale notları

Worker 3B durdurulduğunda:
1. `Core.Domain.SyncChangeSet` adı `Shared.SyncChangeSet` ile çakışıyordu
   (`HttpRemoteApiClient.cs:207`). Yeniden adlandırıldı → `TriggerChangeSet` (ve
   enum `TriggerChangeType`).
2. `TriggerChangeSetReader.cs:582`'de `await BuildSyncChangeSet(...)`
   eksikti. Async imzaya rağmen result.Add(record) satırında derleme hatası
   veriyordu. `await` eklendi.
3. `NewSchemaTriggerInstallerOptions.cs`'te `using ErpBridge.Shared;` eksikti,
   `TrackedTableCatalog` / `TabloAdi` cref'leri unresolved oluyordu. Eklendi.
4. `TriggerSchema.cs:105` cref'inde `ComputeKeyValueExpression` ambiguous'dı
   (iki overload). Doğal metinle değiştirildi.

## 5. Test özeti

| Suite | Yeni | Toplam | Durum |
|-------|------|--------|-------|
| `ErpBridge.Erp.Mikro.Tests.Trigger.TriggerChangeSetReaderInputValidationTests` | 8 | 8 | ✅ |
| `ErpBridge.Erp.Mikro.Tests.Trigger.TrackedTableSchemaTests` | 3 | 3 | ✅ |
| `ErpBridge.Shared.Tests.TrackedTableSchemaTests` (KeyValue compute) | 3 | 3 | ✅ |
| `ErpBridge.Erp.Mikro.Tests.Trigger.TriggerChangeSetReaderTests` (3 yön KeyValue) | 5 | 5 | ✅ |
| **Yeni toplam** | **19** | | ✅ |

Tüm Wave 3 yeni testleri geçer.

## 6. Build & test

```
$ dotnet build ErpBridge.sln -c Debug
  → 0 Uyarı, 0 Hata

$ dotnet test tests/ErpBridge.Erp.Mikro.Tests/... --filter "FullyQualifiedName~Trigger|FullyQualifiedName~KeyValue"
  → Tüm KeyValue + Trigger testleri yeşil
```

## 7. Bilinen sınırlar / sonraki adımlar

- **Mevcut `IChangeSetReader` (Faz 11) davranışı korunuyor.** Yeni
  `ITriggerChangeSetReader` ek API; mevcut `MikroAdapter.ReadChangeSetAsync`
  (PushChangeSetAsync için) hâlâ eski interface'i kullanıyor. İki interface
  birleştirme ayrı bir track.
- **CentralApi tarafı henüz TriggerChangeSet'i tüketmiyor.** Mevcut ingestion
  `Shared.SyncChangeSet` (push bundle) kullanıyor. Yeni sınıf Android
  change-pull endpoint'ine (Faz 15 zaten mevcut) veya ilerideki micro-batch
  endpoint'ine bağlanabilir.
- **Wave 3A implementasyon yarım kaldı** (bkz. `deliverable-faz15-2.md`)
  — KeyValue semantiği yeni trigger şemasına (`_ERPB_SYNC` + `KayitKey`) henüz
  bağlı değil. Yeni şema aktif olduğunda `KeyValueResolver`'a schema-aware
  branch eklemek gerekecek.
