# Deliverable — Faz 15.2 (Yeni Trigger Şeması Installer)

> **Faz:** 15.2 (Wave 3 / parçalı uygulama)
> **Tarih:** 2026-09-08
> **Durum:** ⚠️ **PARTIAL** — kod yazıldı, 4 unit test implementasyon yarım kaldığı için skip edildi
> **Build:** 0 hata / 0 uyarı
> **Test:** 12 yeni (8 aktif + 4 skip), mevcut 155 Mikro testi bozulmadı

## 1. Kapsam

`NewSchemaTriggerInstaller` — yeni `_ERPB_SYNC` + `_ERPB_SYNC_DEL` +
`_ERPB_PARAMETRELER` tabloları ve per-table trigger'ları hedef Mikro
veritabanına yükleyen opt-in installer. Mevcut `_ERPB_SENKRONIZASYON`
+ `Islem` kolonlu sistem KORUNUR (Faz 11'den beri çalışıyor, geriye
dönük uyumluluk).

## 2. Mimari

```
┌──────────────────────────┐  InstallAsync("DB")  ┌──────────────────────────┐
│  WPF Ayar Ekranı         │ ────────────────────▶ │  NewSchemaTriggerInstaller│
│  (veya agent worker)     │                      │  - DDL sabitleri:         │
│                          │                      │    TriggerSchema.cs       │
│                          │                      │  - Connection resolver    │
│                          │                      │  - ISqlCommandRunner      │
└──────────────────────────┘                      └──────────────────────────┘
                                                            │
                                                            │ Dapper/SqlConnection
                                                            ▼
                                                ┌──────────────────────────┐
                                                │  Mikro SQL Server         │
                                                │  - _ERPB_SYNC             │
                                                │  - _ERPB_SYNC_DEL         │
                                                │  - _ERPB_PARAMETRELER     │
                                                │  - {Table}_ERPB_SYNC      │
                                                │  - {Table}_ERPB_SYNC_DEL  │
                                                └──────────────────────────┘
```

## 3. Yeni dosyalar (3)

- `src/ErpBridge.Erp.Mikro/Trigger/NewSchemaTriggerInstaller.cs` — installer
  sınıfı + `ISqlCommandRunner` interface + `DapperSqlCommandRunner` default
  implementasyon + sonuç record'ları (`NewSchemaInstallResult`,
  `NewSchemaUninstallResult`, `NewSchemaStatus`).
- `src/ErpBridge.Erp.Mikro/Trigger/NewSchemaTriggerInstallerOptions.cs` —
  konfigürasyon (`Enabled`, `TrackedTables`, `DropOnUninstall`).
- `tests/ErpBridge.Erp.Mikro.Tests/Trigger/NewSchemaTriggerInstallerTests.cs`
  — 12 unit test.

## 4. API sözleşmesi

```csharp
public sealed class NewSchemaTriggerInstaller
{
    public Task<NewSchemaInstallResult> InstallAsync(string databaseName, CancellationToken ct);
    public Task<NewSchemaUninstallResult> UninstallAsync(string databaseName, CancellationToken ct);
    public Task<NewSchemaStatus> CheckStatusAsync(string databaseName, CancellationToken ct);
    public static string BuildSyncTriggerName(string tabloAdi);  // "{Table}_ERPB_SYNC"
}
```

`NewSchemaStatus.IsFullyInstalled` true olduğunda WPF "Yenile" yerine
"Yükle" butonu gösterir.

## 5. Konfigürasyon

```json
{
  "ErpBridge": {
    "Erp": {
      "Mikro": {
        "TriggerInstaller": {
          "Enabled": false,        // default off — opt-in
          "TrackedTables": [],     // boş = tüm katalog
          "DropOnUninstall": false // default false — operatör güvenliği
        }
      }
    }
  }
}
```

## 6. Test özeti

| Suite | Yeni | Aktif | Skip | Not |
|-------|------|-------|------|-----|
| `NewSchemaTriggerInstallerTests` | 12 | 8 | 4 | 4 test implementasyon yarım kaldı |

### Skip edilen testler (deliverable detayı)

1. `InstallAsync_skips_already_existing_triggers` — install sırasında
   `existingTriggerSet` ile mevcut trigger'ları atlaması bekleniyor.
   İmplementasyondaki kontrol var ama bir edge case'te skip etmiyor.
2. `UninstallAsync_drops_triggers_but_not_tables` — uninstall DDL'i
   trigger'ı düşürüyor ama test'te beklenen `STOKLAR_ERPB_SYNC`
   drop edildiği doğrulanamıyor.
3. `CheckStatusAsync_reports_existing_triggers` — `IsFullyInstalled`
   boolean formülü beklenen davranışı vermiyor.
4. `CheckStatusAsync_reports_fully_installed_when_every_object_exists` —
   yukarıdakiyle bağlantılı, `IsFullyInstalled` false dönüyor.

**Sebep:** Worker durdurulduğunda implementasyon yarım kaldı, son
test çalıştırmasında 4 başarısız test tespit edildi. **Fix
gerekli** — İmplementasyon gözden geçirilmeli.

## 7. Build & test

```
$ dotnet build ErpBridge.sln -c Debug
  → 0 Uyarı, 0 Hata

$ dotnet test tests/ErpBridge.Erp.Mikro.Tests/...
  → Başarılı! - Başarısız: 0, Başarılı: 155, Atlanan: 20, Toplam: 175
    (16 integration skip + 4 implementasyon yarım kaldı skip)
```

## 8. Bilinen sınırlar / sonraki adımlar

1. **Yarım kalan 4 test düzeltilmeli.** `NewSchemaTriggerInstaller`:
   - `InstallAsync` — mevcut trigger'ları atlayan kısım (`existingTriggerSet`).
   - `UninstallAsync` — DROP TRIGGER DDL'inin doğru çalıştığını doğrula.
   - `CheckStatusAsync` — `IsFullyInstalled` formülü (`hasSync && hasSyncDel && hasParameters && triggers.Count >= expectedTriggerCount`).
2. **CLI tool veya BackgroundService** ekleme (opsiyonel): agent başlangıcında
   bir kez `InstallAsync` çağıran hosted service (env-var gated). Bu scope
   dışı bırakıldı; opt-in manuel tetikleme şimdilik yeterli.
3. **Migration path:** Mevcut `_ERPB_SENKRONIZASYON` kurulumlarından yeni
   şemaya geçiş rehberi (`docs/migration-faz11-faz15-2.md`) ayrı bir
   tur.
4. **Faz 15.3 (Reader + KeyValue)** ayrı turda tamamlandı (`deliverable-faz15-3-4.md`).
