# Deliverable — Faz 10 (Multi-Firm Mikro Desteği)

> **Tema:** `MikroAdapter`'ın her bootstrap okumasında hardcode edilmiş
> `const int firmNo = 1; const int warehouseNo = 1;` kaldırıldı. Bu değerler
> artık `AgentConfig` üzerinden WPF UI'da ayarlanabiliyor, SQLite'ta
> persist ediliyor, `MikroConnectionSettings` üzerinden adapter'a akıyor.
> Çok-firmalı (multi-firm) ve çok-depolu (multi-warehouse) Mikro kurulumları
> artık destekleniyor.

## Kapsam

| Alt-görev | Hedef | Durum |
|---|---|---|
| 10.1 | `AgentConfig.WarehouseNo` alanı (default 1) | ✅ |
| 10.2 | `SqliteAgentConfigStore` WarehouseNo round-trip | ✅ |
| 10.3 | `MikroConnectionSettings.CompanyNo` + `WarehouseNo` alanları | ✅ |
| 10.4 | `MikroConnectionSettings.FromConfiguration` CompanyNo + WarehouseNo parse (invariant, default 1) | ✅ |
| 10.5 | `AgentConfigMapper.FromAgentConfig` CompanyNo + WarehouseNo propagation + validation | ✅ |
| 10.6 | `MikroAdapter` 3 hardcode lokasyonu (ReadBootstrapData, ReadBootstrapChanges, ReadBootstrapSection) | ✅ |
| 10.7 | WPF: `MainWindow.xaml` Firma No / Şube No / Depo No inputları | ✅ |
| 10.8 | WPF: `AgentSettingsViewModel` 3 yeni property + Save/Load + `WriteMikroSectionToConfiguration` | ✅ |
| 10.9 | Unit testler: settings + mapper + adapter + store | ✅ (16 yeni) |
| 10.10 | Paket çakışması düzeltmesi: `System.Security.Cryptography.ProtectedData 9.0.13` | ✅ |

## Mimari

```
+--------------------+   +----------------------+
| AgentConfig        |   | MikroConnectionSettings
| - CompanyNo    ────┼───┼─▶ CompanyNo
| - BranchNo         |   | - WarehouseNo
| - WarehouseNo  ────┼───┼─▶
+--------------------+   +----------------------+
        ▲                          │
        │ Save (WPF)               │ FromConfiguration
        │                          │ (live IConfiguration)
        │                          ▼
+--------------------+   +----------------------+
| SqliteAgentConfig  |   | MikroAdapter        |
| Store              |   | (3 bootstrap methods)
| - key/value        |   |  firmNo/wNo ◀──────┘
+--------------------+   +----------------------+
                                       │
                                       │ IMikroDbReader.Read*(firmNo, ..., wNo)
                                       ▼
                                +----------------------+
                                | Mikro SQL Server      |
                                +----------------------+
```

**Veri akışı:**
1. Operatör WPF'te Firma No = 3, Depo No = 7 girip "Kaydet"e basar.
2. `AgentConfig` (CompanyNo=3, BranchNo=0, WarehouseNo=7) SQLite'a yazılır.
3. Aynı zamanda `MutableMemoryConfigurationProvider`'a `Mikro:CompanyNo=3`, `Mikro:WarehouseNo=7` yazılır.
4. `BootstrapSyncService` `IBootstrapSyncService.RunOnceAsync` → adapter `ReadBootstrapChangesAsync(3, ..., 7)` çağırır.
5. Adapter tüm `IMikroDbReader.Read*Async` çağrılarını firma=3, depo=7 ile yapar; central API'ye yüklenen snapshot sadece bu firma/depoyu kapsar.

## Değişen / yeni dosyalar

### Değişen dosyalar
- `src/ErpBridge.Core/Domain/AgentConfig.cs` — + `WarehouseNo`
- `src/ErpBridge.Erp.Mikro/Connection/MikroConnectionSettings.cs` — + `CompanyNo`, + `WarehouseNo`, + `ParseInt`
- `src/ErpBridge.Erp.Mikro/Connection/AgentConfigMapper.cs` — 3 parametreli overload
- `src/ErpBridge.Erp.Mikro/Connection/MikroConnectionFactory.cs` — placeholder güncellendi
- `src/ErpBridge.Erp.Mikro/Adapters/MikroAdapter.cs` — 3 method'ta hardcode kaldırıldı
- `src/ErpBridge.Erp.Mikro/DependencyInjection/ServiceCollectionExtensions.cs` — placeholder
- `src/ErpBridge.LocalStore/Stores/SqliteAgentConfigStore.cs` — `WarehouseNo` persist
- `src/ErpBridge.Agent.UI/Views/MainWindow.xaml` — 3 yeni input alanı
- `src/ErpBridge.Agent.UI/ViewModels/AgentSettingsViewModel.cs` — 3 yeni property + save/load
- `src/ErpBridge.Erp.Mikro/ErpBridge.Erp.Mikro.csproj` — `ProtectedData` direct ref
- `Directory.Packages.props` — `System.Security.Cryptography.ProtectedData 9.0.13`
- `tests/ErpBridge.Erp.Mikro.Tests/Connection/MikroConnectionSettingsTests.cs` — +4 test
- `tests/ErpBridge.Erp.Mikro.Tests/Connection/AgentConfigMapperTests.cs` — +4 test
- `tests/ErpBridge.LocalStore.Tests/Stores/SqliteAgentConfigStoreTests.cs` — +2 test (WarehouseNo)

### Yeni dosyalar
- `tests/ErpBridge.Erp.Mikro.Tests/Adapters/MikroAdapterMultiFirmTests.cs` — 4 test

## Wire şeması (yeni/değişen)

| Yön | Method | URL / Yer | Body | Response |
|------|--------|-----|------|----------|
| agent → Mikro SQL | `Read*Async(firmNo, wNo, ...)` | her reader | parametre | rows |
| WPF → SQLite | `SqliteAgentConfigStore.SaveAsync` | yerel DB | key/value | — |
| WPF → IConfiguration | `MutableMemoryConfigurationProvider[Mikro:CompanyNo]` | in-memory | — | — |

## Mimari kararlar

| Karar | Gerekçe |
|------|---------|
| `CompanyNo` + `WarehouseNo` `MikroConnectionSettings`'e eklendi | Adapter artık bu değerlere `ConnectionSettings.CompanyNo` üzerinden erişir; factory `FromConfiguration` ile canlı IConfiguration'dan okur. |
| `WarehouseNo` `AgentConfig`'e eklendi, `BranchNo` zaten vardı | UI'da 3 alan birden gösterilebilsin; `BranchNo` writer'lar için zaten tutuluyordu. |
| `BranchNo` şu an MikroAdapter'da kullanılmıyor | Adapter sadece okuma yapıyor; BranchNo writer'lar (sales order) için. Faz 11+ writer tarafında. |
| `FromConfiguration` invariant culture + default 1 | Türkçe locale operatör "1,5" yazarsa sessiz veri kaybı olmaz; eksik key default 1'e düşer. |
| `Directory.Packages.props` `ProtectedData 9.0.13` | `Microsoft.Data.SqlClient 7.0.2` üzerinden gelen transitive 8.0.0 ile çakışıyordu; 9.0.13'e sabitlendi. |

## Test özeti

| Suite | Yeni | Toplam | Durum |
|-------|------|--------|-------|
| `ErpBridge.Erp.Mikro.Tests.Connection.MikroConnectionSettingsTests` (multi-firm) | 4 | 4 | ✅ |
| `ErpBridge.Erp.Mikro.Tests.Connection.AgentConfigMapperTests` (multi-firm) | 4 | 4 | ✅ |
| `ErpBridge.Erp.Mikro.Tests.Adapters.MikroAdapterMultiFirmTests` | 4 | 4 | ✅ |
| `ErpBridge.LocalStore.Tests.Stores.SqliteAgentConfigStoreTests` (WarehouseNo) | 2 | 2 | ✅ |
| **Yeni toplam** | **14 yeni** | | |
| ErpBridge.Core.Tests (regression) | — | 66/66 | ✅ |
| ErpBridge.RemoteApi.Tests (regression) | — | 16/16 | ✅ |
| ErpBridge.LocalStore.Tests (regression) | — | 50/50 (+2 yeni) | ✅ |
| ErpBridge.Shared.Tests | — | 22/22 | ✅ |
| ErpBridge.Erp.Mikro.Tests (regression) | — | 94/94 (+ 16 skip) | ✅ |
| ErpBridge.Agent.Service.Tests | — | 8/8 | ✅ |

## Test detayları

**`MikroConnectionSettingsTests` multi-firm (4 yeni):**
- `FromConfiguration_parses_CompanyNo_and_WarehouseNo_when_present`
- `FromConfiguration_defaults_CompanyNo_and_WarehouseNo_to_1_when_missing`
- `FromConfiguration_falls_back_to_1_when_CompanyNo_is_unparsable`
- `FromConfiguration_uses_invariant_culture_for_parsing`

**`AgentConfigMapperTests` multi-firm (4 yeni):**
- `FromAgentConfig_propagates_CompanyNo_and_WarehouseNo`
- `ToErpSettings_propagates_CompanyNo_and_WarehouseNo`
- `FromAgentConfig_with_zero_warehouseNo_returns_null`
- `FromAgentConfig_defaults_warehouseNo_to_1_when_AgentConfig_is_fresh`

**`MikroAdapterMultiFirmTests` (4 yeni):**
- `ReadBootstrapDataAsync_passes_CompanyNo_and_WarehouseNo_to_every_reader_call`
- `ReadBootstrapChangesAsync_passes_CompanyNo_and_WarehouseNo_to_every_reader_call`
- `ReadBootstrapSectionAsync_passes_CompanyNo_for_the_inventory_section`
- `Adapter_exposes_CompanyNo_and_WarehouseNo_via_ConnectionSettings`

**`SqliteAgentConfigStoreTests` (2 yeni):**
- `Save_then_Load_roundtrips_warehouseNo_for_multi_firm_Mikro`
- `Load_defaults_WarehouseNo_to_1_when_no_row_exists`

## Bilinen sınırlamalar / sonraki faz önerileri

- **Faz 10.5 (önerilir):** `MikroSalesOrderWriter.DefaultFirmNo = 1` ve
  `DefaultBranchNo = 0` sabitleri hâlâ writer'da. Sales order write
  pipeline'ı da `MikroConnectionSettings.CompanyNo` + `BranchNo`'dan
  beslenmeli. Bu Faz 10'da sadece reader path güncellendi; writer ayrı bir
  Faz olabilir çünkü mapping store / idempotency davranışını da etkiler.
- **Faz 10.6 (önerilir):** WPF'te "Firma/Şube/Depo seçili mi?" görsel hint'i.
  Şu an sadece 3 input var; operatör 1 dışında bir değer girerse
  `Status` badge'i bunu yansıtmıyor.
- **Faz 10.7 (önerilir):** Validation — `CompanyNo <= 0` veya
  `WarehouseNo <= 0` durumunda UI'da inline error göstermek.
- **Pre-existing test host bug** — `AndroidEndpointsTests` +
  `LicensesValidateTests.Health_check_*` .NET 8.0.x `PipeWriter.UnflushedBytes`
  hatası. Faz 10 kapsamı dışı; yeni kod bu sorundan etkilenmez (Z
  JsonResults.Ok kullanılmıyor; adapter SQL yazmıyor).

## Build / test komutları

```powershell
# Build
dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true

# Tüm Faz 10 testleri
dotnet test tests\ErpBridge.Erp.Mikro.Tests\ErpBridge.Erp.Mikro.Tests.csproj `
  -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor `
  --filter "FullyQualifiedName~MikroConnectionSettingsTests|FullyQualifiedName~AgentConfigMapperTests|FullyQualifiedName~MikroAdapterMultiFirmTests"

dotnet test tests\ErpBridge.LocalStore.Tests\ErpBridge.LocalStore.Tests.csproj `
  -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor `
  --filter "FullyQualifiedName~SqliteAgentConfigStoreTests"
```

## Manuel smoke test (entegre TULPAR + lisans sunucusu)

1. WPF Ayarlar sekmesini aç → yeni "Firma No", "Şube No", "Varsayılan Depo No" alanları görünür.
2. `MikroDatabaseName = MikroDB_V15_02` ile birlikte Firma No = 1, Depo No = 1 → Kaydet.
3. "Bağlantıyı test et" → başarılı.
4. Pano sekmesi → "Bootstrap tetikle" → log: `MikroAdapter.ReadBootstrapDataAsync invoked for database MikroDB_V15_02, companyNo=1, warehouseNo=1`.
5. WPF'te Firma No = 2, Depo No = 3'e değiştir → Kaydet.
6. "Bootstrap tetikle" → log: `companyNo=2, warehouseNo=3`. SQL sorguları yeni değerlerle çalışır.
7. SQLite Inspector: `SELECT key, value FROM agent_config WHERE key LIKE 'CompanyNo' OR key LIKE 'BranchNo' OR key LIKE 'WarehouseNo'` → doğru değerler.
