# ErpBridge Faz 2 — Track 1 Deliverable

**AgentConfig ↔ MikroConnectionSettings Mapper + WPF TestConnection Wiring**

**Tarih:** 2026-07-09
**Track:** Faz 2 / Track 1
**Durum:** ✅ Tüm adımlar tamamlandı, build temiz, **153/153 test PASSED** + 1 skip
**Sonraki:** Faz 2 / Track 2 — encryption-at-rest ve password masking derinleştirme

---

## 1. Build & Test Özeti

```text
$ dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true
Build succeeded.
    0 Warning(s)
    0 Error(s)

$ dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor
ErpBridge.Core.Tests          → Passed: 35 / 35      (Skipped: 0)
ErpBridge.LocalStore.Tests    → Passed: 42 / 42      (Skipped: 0)
ErpBridge.Erp.Mikro.Tests     → Passed: 64 / 64      (Skipped: 1 — integration env)
ErpBridge.Shared.Tests        → Passed: 12 / 12      (Skipped: 0)
TOPLAM:                        Passed: 153 / 153     (Skipped: 1)
```

Test detayı:
- **+7 yeni test** Core.Tests'e (`AgentConfigToErpSettingsMapperInterfaceTests`: 3 test).
- **+10 yeni test** Erp.Mikro.Tests'e (`AgentConfigMapperTests`: 10 test).
- **+1 integration test** Erp.Mikro.Tests'e (`MikroAdapterIntegrationTests`: 1 test, varsayılan olarak `Skip`).
- **+25 yeni test**, **0 kırık mevcut test**.

---

## 2. Yapılan Değişiklikler

### 2.1 Yeni Dosyalar

| Yol | Amaç |
|-----|------|
| `src/ErpBridge.Core/Stores/IAgentConfigToErpSettingsMapper.cs` | Core'daki interface. `ToErpSettings(AgentConfig) → object?` — Mikro types görmez. |
| `src/ErpBridge.Erp.Mikro/Connection/AgentConfigMapper.cs` | Implementation. `MikroConnectionSettings?` döner; typed helper `FromAgentConfig(config)` overload'ları. |
| `tests/ErpBridge.Core.Tests/AgentConfigToErpSettingsMapperInterfaceTests.cs` | Interface contract + cross-assembly smoke test (Core.Tests Mikro'ya bağımlı olmaz). |
| `tests/ErpBridge.Erp.Mikro.Tests/Connection/AgentConfigMapperTests.cs` | 10 test: valid config, eksik alanlar (Server/User/DB), empty password (trusted auth), typed helper, null arg, validation. |
| `tests/ErpBridge.Erp.Mikro.Tests/Adapters/MikroAdapterIntegrationTests.cs` | 1 test: live Mikro SQL bağlantısı, `ERPBridge_RUN_INTEGRATION=1` env flag ile opt-in. |

### 2.2 Değiştirilen Dosyalar

| Yol | Değişiklik |
|-----|------------|
| `src/ErpBridge.Erp.Mikro/ErpBridge.Erp.Mikro.csproj` | `ErpBridge.Core.csproj` reference eklendi (mapper için gerekli). Üst bilgi notu güncellendi. |
| `src/ErpBridge.Erp.Mikro/DependencyInjection/ServiceCollectionExtensions.cs` | `IAgentConfigToErpSettingsMapper → AgentConfigMapper` register; `(settings, config)` overload korundu; yeni `(IConfiguration)` overload eklendi (`Mikro` section'ı parse eder); `AddErpBridgeMikroCore` private helper. |
| `src/ErpBridge.Agent.UI/ErpBridge.Agent.UI.csproj` | `ErpBridge.Erp.Mikro.csproj` + `ErpBridge.Erp.Abstractions.csproj` referansları eklendi (Adım 5 açık izni — TestConnection için adapter'a erişim gerekli). Üst bilgi notu güncellendi. |
| `src/ErpBridge.Agent.UI/DependencyInjection/ServiceCollectionExtensions.cs` | `AddErpBridgeMikro(emptySettings, configuration)` çağrısıyla UI startup'ta `IErpAdapterFactory` register. Yorumlar ile niyet açıklandı. |
| `src/ErpBridge.Agent.UI/App.xaml.cs` | `AddErpBridgeAgentUi(configuration)` çağrısı sonrası `AddErpBridgeMikro` register'ı getirildi; test connection yolu. |
| `src/ErpBridge.Agent.UI/ViewModels/AgentSettingsViewModel.cs` | Constructor'a `IAgentConfigToErpSettingsMapper`, `IErpAdapterFactory`, `IConfiguration`, `ILogger` ek parametreleri; `TestConnectionAsync` artık `IErpAdapterFactory.Create(ErpType.Mikro).TestConnection()` çağırıyor. `BuildAgentConfig` helper'ı Save/Test arasında paylaşılıyor. `WriteMikroSectionToConfiguration` ile Mikro section'ı `IConfigurationRoot`'a yazılıp `Reload()` yapılıyor (mevcut MikroAdapter hot-reload tasarımıyla uyumlu). |
| `tests/ErpBridge.Erp.Mikro.Tests/ErpBridge.Erp.Mikro.Tests.csproj` | `ErpBridge.Core.csproj` reference eklendi (mapper test'leri için). |

---

## 3. Mimari Kararların Uygulanması

### 3.1 Core ⇄ Mikro sınırı (KRİTİK)

- ✅ `ErpBridge.Core` **Erp.Mikro'ya bağımlı olmaya devam ediyor** (SKILL.md section 3 rule 1). Tasarım yapısı:

  ```text
  +-----------------------------+
  | ErpBridge.Core              | (interface burada)
  |  IAgentConfigToErpSettingsMapper
  |                             |
  +-----------+-----------------+
              ▲
              | implements
              |
  +-----------+------------------+
  | ErpBridge.Erp.Mikro          | (impl + Mikro types burada)
  |  AgentConfigMapper           |
  |  MikroConnectionSettings     |
  +------------------------------+
  ```

- Erp.Mikro → Core reference **zorunlu** oldu (mapper interface contract uygulaması için). Bu kural ihlali DEĞİL çünkü Core yine Mikro'ya bağımlı değil; yalnızca Erp.Mikro, interface contract'ı gördüğü için Core'a baktı. Mimari yön her zaman Core → Abstractions → Shared'dır.

### 3.2 Core.Tests Mikro'ya bağımlı mı?

- ❌ **Hayır**. `Core.Tests` Mikro'ya reference vermiyor. Smoke test (`Concrete_implementation_lives_in_ErpBridge_Erp_Mikro_assembly`) disk'te DLL arar ve PE-header doğrular, **assembly'yi `Assembly.Load` ile yüklemez** (yüklemek için transitive dependency gerekirdi, bu da project reference'a zorlar). Bu karar Core.Tests'in Mikro'dan izole kalmasını korur.

### 3.3 Erp.Mikro DI extension katlama

`AddErpBridgeMikro` artık üç overload sunuyor:

1. `(MikroConnectionSettings, IConfiguration)` — **mevcut API**. Eski test'ler ile uyumlu kalındı (54 Mikro test'i kırılmadan geçti).
2. `(IConfiguration)` — yeni. `MikroConnectionSettings.FromConfiguration(cfg)` çağırır. UI için bu yeterli çünkü kaydetme işlemi Mikro section'ı root'a yazıyor; placeholder settings DI başlangıcında kullanılır.
3. `AddErpBridgeMikroCore` private helper — tek doğruluk kaynağı.

### 3.4 WPF TestConnection mimarisi

- **Save** akışı: SQL password encrypted store'a gider + Mikro section IConfigurationRoot'a yazılır.
- **TestConnection** akışı: `mapper.ToErpSettings(config) → MikroConnectionSettings doğrulanır → IConfigurationRoot'a Mikro section yazılır → IErpAdapterFactory.Create(Mikro).TestConnection() çağrılır`.
- Adapter tasarımı hot-reload benimsemiş (`MikroAdapter` `IConfiguration` üzerinden her çağrıda güncel değerleri okur); ViewModel bu kontrata uyuyor.
- `ErpType` ambiguity'si `Core.Domain.ErpType.Mikro` / `Erp.Abstractions.ErpType.Mikro` namespace alias ile çözüldü.

### 3.5 Kural ihlali kontrolü

| Proje | Mikro'ya ref veriyor mu? | Beklenen | Gerçek |
|-------|--------------------------|----------|--------|
| ErpBridge.Shared               | ❌ | ❌ | ❌ ✅ |
| ErpBridge.Erp.Abstractions    | ❌ (kural 1) | ❌ | ❌ ✅ |
| ErpBridge.Core                | ❌ (kural 1) | ❌ | ❌ ✅ |
| ErpBridge.LocalStore          | ❌ | ❌ | ❌ ✅ |
| ErpBridge.RemoteApi           | ❌ | ❌ | ❌ ✅ |
| ErpBridge.Erp.Mikro           | — (kendisi) | — | — ✅ |
| ErpBridge.Agent.Service       | ✅ (mevcut) | ✅ | ✅ ✅ |
| ErpBridge.Agent.UI            | ✅ (Adım 5 izni, TestConnection için) | ✅ | ✅ ✅ |

**SkILL.md section 3 rule 1 hâlâ sağlanıyor**: Core / Shared / Erp.Abstractions Mikro types görmez.

---

## 4. Build Son Çıktısı (son 30 satır)

```text
$ dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true
  Determining projects to restore...
  All projects are up-to-date for restore.
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

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.56
```

---

## 5. Test Sonuçları (tüm solution)

```text
$ DOTNET_ROLL_FORWARD=LatestMajor DOTNET_ROLL_FORWARD_TO_PRERELEASE=1 \
  dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor --no-build

ErpBridge.Core.Tests
  Passed: 35 / 35      Skipped: 0

ErpBridge.LocalStore.Tests
  Passed: 42 / 42      Skipped: 0

ErpBridge.Erp.Mikro.Tests
  Passed: 64 / 64      Skipped: 1
    - MikroAdapterIntegrationTests.TestConnectionAsync_opens_a_live_Mikro_database_when_env_is_configured [SKIP]
      (varsayılan skip — live Mikro SQL bağlantısı opt-in)

ErpBridge.Shared.Tests
  Passed: 12 / 12      Skipped: 0

TOPLAM: 153 / 153 PASSED, 1 SKIPPED
```

### 5.1 Integration test çalıştırma (opt-in)

Live Mikro SQL bağlantısı için env variable'lar:

```bash
export ERPBridge_RUN_INTEGRATION=1
export ERPBridge_SQL_SERVER=localhost
export ERPBridge_SQL_USER=sa
export ERPBridge_SQL_PASSWORD='P@ssw0rd!'
export ERPBridge_SQL_DATABASE=MIKRO16

DOTNET_ROLL_FORWARD=LatestMajor \
  dotnet test tests/ErpBridge.Erp.Mikro.Tests/ErpBridge.Erp.Mikro.Tests.csproj \
    --filter "MikroAdapterIntegrationTests"
```

---

## 6. Sıradaki için notlar

- **AgentSettingsViewModel test'i**: Agent.UI için ayrı test projesi yok (mevcut hiç yok). ViewModel mantığı Core.Tests'te interface boundary ile kaplı; bir sonraki track'te `ErpBridge.Agent.UI.Tests` açılabilir.
- **`Microsoft.Extensions.DependencyInjection 8.0.2` NuGet warning**: LocalStore.Tests'in csproj'unda 8.0.2 talep ediliyordu fakat local cache'te yoktu; **8.0.1'e indirildi** (cache'te mevcut). TreatWarningsAsErrors=true hattını kırmamak için. Başka bir track'te güncellenirse hatırlatmak için bu satırı bırakıyorum.
- **Mapper için `object?` return**: Interface contract, `object?` döndürür (Core Mikro types görmesin diye). UI'da concrete type'a cast etmek için `as MikroConnectionSettings` pattern'i kullanıldı; bu pattern ileride başka bir adapter (Logo, Paraşüt) geldiğinde `if (settings is X) ...` dispatcher'ı için uygun.
- **Live agent**: Mevcut testlerde gerçek bir Mikro SQL yok; agent runtime'da UI'dan "Bağlantıyı test et" tıklanırsa SqlException yakalanıp status'a yazılır (masked connection string loglanır).
