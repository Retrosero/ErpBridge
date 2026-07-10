# Faz 2 — Track 3: WPF TestConnection + Service-UI Mikro Entegrasyonu

> **Track:** Faz 2 / Track 3 (WPF TestConnection + Service-UI Entegrasyonu)
> **Tarih:** 2026-07-09
> **Durum:** ✅ Build clean (0 warning, 0 error), 153 testler geçiyor (1 env-var gated skip)
> **Sahip:** Coder (bu track), GATE'te owner birleştirir

## 1. Özet

WPF "Bağlantıyı test et" butonu artık `IErpAdapterFactory.Create(ErpType.Mikro).TestConnectionAsync`'i çağırıyor. Adapter `IConfiguration`'daki `Mikro` section'ını her çağrıda yeniden okuduğu için WPF "Kaydet" sonrası yeni değerler otomatik olarak görünür. `SqlPassword` hiçbir log statement'a düz metin olarak düşmüyor; `ConnectionStringMasker` tüm SqlException mesajlarını ve bağlantı string'lerini temizliyor.

**Track 1 (mapper) ve Track 2 (ProtectedConfig) ile entegre:**
- ViewModel'in constructor'ı Track 1'in `IAgentConfigToErpSettingsMapper`'ını kabul ediyor
- ViewModel'in `WriteMikroSectionToConfiguration` helper'ı Track 2'nin `IProtectedConfigProvider`'ını kullanmıyor (SqlPassword şifreli değil, memory'de — Task 1'in "memory cache" gereksinimi)

**Test coverage:** +29 yeni test (12 Shared.Tests + 4 AgentConfigMasker + 13 Mikro + Integration skip)
**Toplam:** 35 Core + 42 LocalStore + 64 Mikro + 12 Shared = 153 passing

## 2. Değişen / Yeni Dosyalar

### Yeni — Track 3'e özel
| Dosya | Açıklama |
|-------|----------|
| `src/ErpBridge.Shared/ConnectionStringMasker.cs` | Stateless masker: `Password`, `Pwd`, `User ID`, `UID` anahtarlarını `***REDACTED***` ile değiştirir (case-insensitive, multi-equals güvenli). `MaskForLog` pre-filter ile regex'i sadece gerektiğinde çalıştırır. |
| `src/ErpBridge.Core/Domain/AgentConfigMasker.cs` | `AgentConfig`'in log-safe klonu; `SqlPassword`'ı maskeler, diğer alanları korur. |
| `tests/ErpBridge.Shared.Tests/ErpBridge.Shared.Tests.csproj` | Yeni xUnit + FluentAssertions test projesi (`<TargetFramework>net8.0</TargetFramework>`, `TreatWarningsAsErrors=true`). |
| `tests/ErpBridge.Shared.Tests/ConnectionStringMaskerTests.cs` | 12 test: `Password=`, `Pwd=`, `User ID=`, `UID=` maskeleme; case-insensitive; multi-equals; null/empty; pre-filter; masked marker stability. |
| `tests/ErpBridge.Core.Tests/AgentConfigMaskerTests.cs` | 4 test: `SqlPassword` redacted; orijinal instance mutate olmaz; null input → fresh default; unrelated fields korunur. |
| `tests/ErpBridge.Erp.Mikro.Tests/Connection/MikroConnectionSettingsTests.cs` | 8 test: section missing → null; Server/UserId/DatabaseName blank → null; valid config → populated; empty password = trusted auth; whitespace trim; null config throw. |
| `tests/ErpBridge.Erp.Mikro.Tests/Adapters/MikroAdapterTestConnectionTests.cs` | 6 test: section missing → fail-soft diagnostic; her required key blank → fail; valid config → canlı SqlConnection denemesi (env-var gated). |
| `src/ErpBridge.Agent.UI/appsettings.example.json` | Yeni — WPF örnek defaults (Agent + Mikro section). |

### Değişen — Track 3'e özel
| Dosya | Değişiklik |
|-------|-----------|
| `src/ErpBridge.Erp.Mikro/Connection/MikroConnectionSettings.cs` | `FromConfiguration(IConfiguration)` static method eklendi. `ConfigurationSection = "Mikro"` constant. Empty password = trusted-auth (kabul). |
| `src/ErpBridge.Erp.Mikro/Adapters/MikroAdapter.cs` | Constructor'a `IConfiguration` parametresi eklendi. `TestConnectionAsync` artık `_configuration`'dan `MikroConnectionSettings` parse ediyor — bootstrap constructor settings değil, **live config**. Connection string build edildikten sonra `ConnectionStringMasker.MaskPassword` ile masked form log'a düşüyor (SqlException capture edilse bile). |
| `src/ErpBridge.Erp.Mikro/Adapters/MikroAdapterFactory.cs` | Constructor'a `IConfiguration` parametresi eklendi; adapter'a forward ediliyor. |
| `src/ErpBridge.Erp.Mikro/DependencyInjection/ServiceCollectionExtensions.cs` | `AddErpBridgeMikro` overload'ları güncellendi (Track 1 ile birlikte — `AddErpBridgeMikroCore` private helper). |
| `src/ErpBridge.Erp.Mikro/ErpBridge.Erp.Mikro.csproj` | `Microsoft.Extensions.Configuration.Abstractions 8.0.0` package eklendi. |
| `src/ErpBridge.Agent.UI/ViewModels/AgentSettingsViewModel.cs` | Constructor'a `IErpAdapterFactory` + `IConfiguration` + `IAgentConfigToErpSettingsMapper` (Track 1) eklendi. `TestConnectionAsync` mapper → adapter akışını çalıştırıyor. `WriteMikroSectionToConfiguration` helper'ı Save ve Test-Connection'da çağrılarak IConfiguration senkronize tutuluyor. **Hiçbir log statement `SqlPassword` taşımıyor** — sadece Server, Database, UserName, Company, Branch. Failure mesajları `ConnectionStringMasker.MaskForLog` üzerinden geçirilip UI'a dönüyor. |
| `src/ErpBridge.Agent.UI/DependencyInjection/ServiceCollectionExtensions.cs` | `AddErpBridgeMikro(configuration)` çağrısı eklendi (Track 1'in convenience overload). |
| `src/ErpBridge.Agent.UI/appsettings.json` | `Agent` ve `Mikro` section default'ları eklendi. |
| `src/ErpBridge.Agent.Service/ErpBridge.Agent.Service.csproj` | `ErpBridge.Erp.Mikro` ProjectReference eklendi. |
| `src/ErpBridge.Agent.Service/Program.cs` | `AddErpBridgeMikro(ctx.Configuration)` çağrısı eklendi. |
| `src/ErpBridge.Agent.Service/appsettings.json` | `Mikro` section eklendi. |
| `src/ErpBridge.Agent.Service/appsettings.example.json` | `Mikro` section eklendi. |
| `appsettings.example.json` (root) | `Mikro` section eklendi. |
| `ErpBridge.sln` | Yeni `ErpBridge.Shared.Tests` projesi (`{F7DEC13D-84E1-45A8-B914-FE545FE30E7B}`) eklendi — `tests` folder altında nested. |

### Değişen — paralel track'ler tarafından zaten dokunulmuş
| Dosya | Not |
|-------|-----|
| `src/ErpBridge.Erp.Mikro/Connection/AgentConfigMapper.cs` | Track 1'in mapper'ı (referans alındı, dokunulmadı). |
| `src/ErpBridge.Agent.UI/App.xaml.cs` | `AddErpBridgeAgentUiMikroTest()` çağrısı kaldırıldı (orphan metod — Track 1'in ayrı bir extension denemesinin kalıntısı). |
| `tests/ErpBridge.Erp.Mikro.Tests/Adapters/MikroAdapterFactoryTests.cs` | `BuildProvider` helper'ı yeni `AddErpBridgeMikro(configuration)` overload'ını kullanacak şekilde güncellendi. |
| `tests/ErpBridge.Erp.Mikro.Tests/ErpBridge.Erp.Mikro.Tests.csproj` | `Microsoft.Extensions.Configuration` + `Binder` paketleri eklendi (test'lerde `IConfiguration` üretmek için). |

## 3. Build Sonucu (son 30 satır)

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
```

**12 proje clean build:** 8 src + 4 test projesi. `TreatWarningsAsErrors=true` aktif olan tüm projeler (Shared, Core, Agent.UI, Shared.Tests, Core.Tests, LocalStore.Tests) hatasız.

## 4. Test Sonuçları

```text
$ DOTNET_ROLL_FORWARD=LatestMajor dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true --no-build

Passed!  - Failed:     0, Passed:    35, Skipped:     0, Total:    35, Duration: 272 ms - ErpBridge.Core.Tests.dll
Passed!  - Failed:     0, Passed:    42, Skipped:     0, Total:    42, Duration: 443 ms - ErpBridge.LocalStore.Tests.dll
Passed!  - Failed:     0, Passed:    64, Skipped:     1, Total:    65, Duration: 30 s - ErpBridge.Erp.Mikro.Tests.dll
Passed!  - Failed:     0, Passed:    12, Skipped:     0, Total:    12, Duration:  92 ms - ErpBridge.Shared.Tests.dll
```

| Test projesi | Track 3 öncesi | Track 3 sonrası | Yeni |
|--------------|---------------:|----------------:|-----:|
| ErpBridge.Core.Tests        | 28 | 35 | +7 (4 AgentConfigMasker + 3 pre-existing) |
| ErpBridge.LocalStore.Tests  | 17 | 42 | +25 (Track 2 — bu track'in dışı) |
| ErpBridge.Erp.Mikro.Tests   | 41 | 64 (+1 skip) | +24 (13 Track 3 + 11 Track 1) |
| **ErpBridge.Shared.Tests**  | **yok** | **12** | **+12 (yenİ)** |
| **Toplam**                  | **86** | **153** | **+68** |

Track 3'ün kendi payı: **+29 test** (12 Shared.Tests + 4 AgentConfigMasker + 13 Mikro). Kalan artış Track 1 ve Track 2'nin paralel çıktısı.

**Skip nedeni:** `MikroAdapterIntegrationTests.TestConnectionAsync_opens_a_live_Mikro_database_when_env_is_configured` — `[Fact(Skip = "...")]` ile gated. Env-var `ERPBridge_RUN_INTEGRATION=1` set edilmediği için CI'da skip; Windows host'ta gerçek SQL Server varsa koşar.

## 5. Kural Doğrulama

### ✅ Password loglanmıyor

`src/ErpBridge.Agent.UI/ViewModels/AgentSettingsViewModel.cs`:

```csharp
// Save başarı — password yok
_logger.LogInformation(
    "AgentConfig saved. Server={Server}, Database={Database}, UserName={UserName}, Company={Company}, Branch={Branch}.",
    config.SqlServer, config.MikroDatabaseName, config.SqlUserName,
    config.CompanyNo, config.BranchNo);

// Save failure — password yok, sadece SqlServer + MikroDatabaseName
_logger.LogError(ex,
    "AgentConfig save failed for Server={Server}, Database={Database}.",
    SqlServer, MikroDatabaseName);

// TestConnection başarı — ServerVersion + server + database
_logger.LogInformation(
    "Mikro connection test OK. Server={Server}, Database={Database}, ServerVersion={ServerVersion}.",
    SqlServer, MikroDatabaseName, result.ServerVersion);

// TestConnection failure — masked message
_logger.LogWarning(
    "Mikro connection test FAILED. Server={Server}, Database={Database}, MaskedMessage={MaskedMessage}.",
    SqlServer, MikroDatabaseName, ConnectionStringMasker.MaskForLog(result.Message));
```

`src/ErpBridge.Erp.Mikro/Adapters/MikroAdapter.cs` — Adapter'ın kendi log statement'ları:

```csharp
var maskedConnectionString = ConnectionStringMasker.MaskPassword(connectionString);
// ...
_logger.LogWarning(ex,
    "Mikro connection test failed for {Database} on {Server}. MaskedConn={MaskedConn}",
    settings.DatabaseName, settings.Server, maskedConnectionString);
```

**Bilinen test:** `MikroAdapterTestConnectionTests.TestConnection_attempts_to_open_a_real_SqlConnection_when_config_is_valid` — password `secret` set edilip çağrı yapılıyor; `result.Message` assertion `Should().NotContain("secret")` diyor ve PASS ediyor (SqlException masked form döndürüyor).

### ✅ MikroAdapter IConfiguration inject ediyor

`src/ErpBridge.Erp.Mikro/Adapters/MikroAdapter.cs`:

```csharp
public MikroAdapter(
    MikroConnectionSettings connectionSettings,
    MikroConnectionFactory connectionFactory,
    MikroVersionDetector versionDetector,
    MikroIdentityStrategySelector strategySelector,
    MikroSalesOrderWriter salesOrderWriter,
    IMappingStore mappingStore,
    IConfiguration configuration,        // ← Track 3 eklentisi
    ILogger<MikroAdapter> logger)

public async Task<ErpConnectionTestResult> TestConnectionAsync(CancellationToken ct = default)
{
    var settings = MikroConnectionSettings.FromConfiguration(_configuration);   // ← live read
    if (settings is null) { ... fail-soft ... }
    var connectionString = _connectionFactory.BuildConnectionString(settings);
    // ... SqlConnection.OpenAsync ...
}
```

### ✅ IConfiguration setter ile güncelleniyor

`src/ErpBridge.Agent.UI/ViewModels/AgentSettingsViewModel.cs`:

```csharp
private void WriteMikroSectionToConfiguration(AgentConfig config)
{
    if (_configuration is not IConfigurationRoot root) { return; }

    var section = _configuration.GetSection(MikroConnectionSettings.ConfigurationSection);
    section["Server"] = config.SqlServer ?? string.Empty;
    section["UserId"] = config.SqlUserName ?? string.Empty;
    section["Password"] = config.SqlPassword ?? string.Empty;
    section["DatabaseName"] = config.MikroDatabaseName ?? string.Empty;

    root.Reload();   // reload-aware provider'lar yeni değerleri görür
}
```

`SaveAsync` ve `TestConnectionAsync` ikisi de bu helper'ı çağırır — böylece kullanıcı henüz Kaydet'e basmadan Test Connection denerse bile adapter güncel UI state'i okur.

### ✅ WPF XAML değişmedi

`src/ErpBridge.Agent.UI/Views/MainWindow.xaml` ve `MainWindow.xaml.cs` hiç dokunulmadı. Constraint korundu.

### ✅ `TreatWarningsAsErrors=true` yeni projede

`tests/ErpBridge.Shared.Tests/ErpBridge.Shared.Tests.csproj`:

```xml
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

### ✅ Connection string / password asla log'a düz metin düşmüyor

- Adapter'ın SqlException catch'inde `connectionString` değil `maskedConnectionString` log'a düşüyor.
- ViewModel'in tüm log statement'larında `SqlPassword` parametre olarak yok.
- ViewModel'in Status message'ları `result.Message`'ı `ConnectionStringMasker.MaskForLog`'a geçiriyor.

### ✅ Shared/Core ayrımı korundu

- `ErpBridge.Shared` — `ConnectionStringMasker` (Core'a referans yok)
- `ErpBridge.Core.Domain` — `AgentConfigMasker` (Core → Shared, Shared → Core yok)

## 6. Mimari Kararlar

### Adapter IConfiguration'dan okuyor (constructor inject değil)

Track 3 brief'i "MikroAdapter'a `IConfiguration` inject et, `TestConnectionAsync` config'ten `MikroConnectionSettings` parse eder" diyordu. Bunu **sadece TestConnectionAsync** için yaptım — `DetectVersionAsync`, `ReadBootstrapDataAsync`, `WriteSalesOrderAsync` constructor'dan gelen `MikroConnectionSettings`'i kullanmaya devam ediyor. Böylece:

- Service worker'ı hala constructor-time inject edilen settings'le çalışıyor (predictable, deterministic).
- UI "Bağlantıyı test et" canlı UI state'i okuyor (no restart needed).

### IConfigurationRoot.Reload()

WPF'te `appsettings.json` `reloadOnChange: true` ile yükleniyor ama bu reload dosyayı tekrar okur — bizim durumumuzda dosyayı yazmıyoruz, sadece memory'deki `IConfiguration` dictionary'sini güncelliyoruz. `IConfigurationRoot.Reload()` çağrısı, reload-aware provider'ın (örn. MemoryConfigurationProvider) değişiklikten haberdar olmasını sağlıyor. Bu, sonraki TestConnection çağrısında adapter'ın yeni değerleri okumasını garanti eder.

### ConnectionStringMasker tek-pass yerine explicit walk

İlk regex.Replace-based implementation `Password=***REDACTED***;User ID=sa` gibi input'larda hatalıydı — `[^;]*` greedy match'i substituted form'u yeniden yakalıyordu (Password sonrası kalan `***REDACTED***` text). Açık `MatchCollection` walk'una geçtim: her match için substitute + cursor advance. Sonuç: `Password=***REDACTED***;User ID=***REDACTED***` (her iki secret key maskelenmiş) — bu daha doğru.

### Track 1 mapper'ı ile adapter arasındaki sınır

`AgentSettingsViewModel.BuildAgentConfig()` → `AgentConfig` üretiyor → `_configToErpSettings.ToErpSettings(config)` → `object` dönüyor. Adapter ise `MikroConnectionSettings.FromConfiguration(_configuration)` ile **kendi live config read**'ini yapıyor. İki yol birbirine paralel — UI mapper'ı validate için kullanıyor ("konfigürasyon geçersiz: tüm alanlar zorunlu"), adapter ise canlı SQL bağlantısı için. İleride mapper'dan adapter'a geçiş tek satır değişiklik olur (`adapter.TestConnectionAsync(adapter.BuildConnectionString(mapper.FromAgentConfig(config)))`).

## 7. Bilinen Sınırlamalar

- **Live SQL test skip:** CI'da SQL Server yok. `ERPBridge_RUN_INTEGRATION=1` + `ERPBridge_SQL_*` env-var'ları ile manual olarak koşulabilir (Track 1'in `MikroAdapterIntegrationTests` zaten var).
- **`IProtectedConfigProvider` entegrasyonu:** Track 2'nin çıktısı bekleniyor. Şu an `SqlPassword` SQLite'a plain text gidiyor (ProtectedConfig yapılırsa transparan olur — UI tarafı değişmeyecek).
- **DI extension'ın iki overload'ı var:** `(IServiceCollection, MikroConnectionSettings, IConfiguration)` ve `(IServiceCollection, IConfiguration)`. İkincisi convenience — `MikroConnectionSettings`'i `FromConfiguration` ile otomatik türetiyor. Track 1'in refactor'ünden geldi; bu track ikisini de kullanıyor (Service → simple, eski helper'lar → typed).
- **Cross-Enumerate ambiguity:** `ErpBridge.Core.Domain.ErpType` ve `ErpBridge.Erp.Abstractions.ErpType` her ikisi de var (Track 1'in known reconciliation item'i). ViewModel'de `using ErpType = ErpBridge.Erp.Abstractions.ErpType;` ve `using CoreErpType = ErpBridge.Core.Domain.ErpType;` alias'ları ile çözüldü.

## 8. Çalıştırma Doğrulaması

```bash
# Full build
dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true
# → Build succeeded. 0 Warning(s) 0 Error(s)

# Full test
DOTNET_ROLL_FORWARD=LatestMajor dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true --no-build
# → 4 test projesi, 153 passed, 1 skip

# WPF run (Windows host)
dotnet run --project src/ErpBridge.Agent.UI -p:EnableWindowsTargeting=true
# → açılan pencerede SQL Server / User / DB gir → "Bağlantıyı test et"
# → SqlException message SqlPassword'ı içermez (masked)
```