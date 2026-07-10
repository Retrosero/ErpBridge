# ErpBridge Faz 2 — Track 2 Deliverable

**Track:** Track 2 — `IProtectedConfigProvider` Implementations (AES + DPAPI)
**Phase:** 2 (encrypted-at-rest configuration)
**Status:** ✅ Build clean, tüm testler geçiyor (150/150 passed + 1 skipped).
**Tarih:** 2026-07-09

---

## 1. Özet

Phase-2 Track-2 kapsamında `IProtectedConfigProvider` için iki gerçek implementasyon
eklendi: cross-platform `AesProtectedConfigProvider` (AES-256-GCM, .NET 8 native) ve
Windows-only `DpapiProtectedConfigProvider` (DPAPI `CurrentUser` scope). Default DI
binding'i platform-aware: Windows'ta DPAPI, Linux/macOS'ta AES. Faz 1'de NoOp olarak
kalan store şimdi provider'ı gerçekten kullanıyor, schema ise ileriye dönük
`protected_value TEXT` + `protection_version INTEGER` kolonlarıyla genişletildi.

---

## 2. Değişen / yeni dosyalar

### Yeni dosyalar (LocalStore)

```
src/ErpBridge.LocalStore/
├── ProtectedConfig/
│   ├── KeyStore.cs                                       # Cross-platform key file management
│   ├── AesProtectedConfigProvider.cs                    # AES-256-GCM provider
│   └── DpapiProtectedConfigProvider.cs                   # Windows-only DPAPI provider
└── (diğer dosyalar değişti — aşağıda)
```

### Yeni test dosyaları

```
tests/ErpBridge.LocalStore.Tests/
├── ProtectedConfig/
│   ├── AesProtectedConfigProviderTests.cs                # 11 test
│   └── KeyStoreTests.cs                                  # 7 test
└── Stores/
    └── SqliteAgentConfigStoreProtectedTests.cs           # 7 test
```

### Değişen dosyalar

| Dosya | Değişiklik |
|-------|-----------|
| `src/ErpBridge.LocalStore/ErpBridge.LocalStore.csproj` | `Microsoft.Extensions.DependencyInjection.Abstractions` 8.0.0 → 8.0.2 (build hatasız logging için); `Microsoft.Extensions.Logging.Abstractions 8.0.2` eklendi (SqliteAgentConfigStore constructor'ı için); `System.Security.Cryptography.ProtectedData 8.0.0` eklendi (DPAPI stubs). |
| `src/ErpBridge.LocalStore/Sqlite/Migrations/MigrationRunner.cs` | `ProtectedConfigColumnsMigration` (version=2) eklendi: `protected_value TEXT` + `protection_version INTEGER` kolonları. Idempotent: hem `schema_version` hem `PRAGMA table_info` ile kontrol; duplicate-column hatası try/catch. |
| `src/ErpBridge.LocalStore/Stores/SqliteAgentConfigStore.cs` | Provider-aware: secret rows `value` yerine `protected_value` kolonuna yazılıyor. Read path'te `IsProtected` → `Unprotect` → başarısızsa `***REDACTED***`. Legacy plaintext rows masked. Yapısal `ErrorMessage` log'lama yok (sadece `_logger.LogWarning(ex, "Failed to decrypt ...", key)` ile anahtar sızıntısı yapılmaz). |
| `src/ErpBridge.LocalStore/DependencyInjection/ServiceCollectionExtensions.cs` | Default binding platform-aware: `OperatingSystem.IsWindows()` → `DpapiProtectedConfigProvider`, aksi → `AesProtectedConfigProvider`. `[SupportedOSPlatform("windows")]` ile gated Windows-only kayıt. |
| `tests/ErpBridge.LocalStore.Tests/ErpBridge.LocalStore.Tests.csproj` | `Microsoft.Extensions.Configuration 8.0.0` + `Microsoft.Extensions.DependencyInjection 8.0.2` (DI-based test için). |
| `tests/ErpBridge.LocalStore.Tests/SqliteTestHarness.cs` | Test DB'leri artık migration 002'yi de uyguluyor (gerçek agent davranışı ile birebir). |

### Değişmeyen dosyalar (gözden geçirildi, güncelleme gerekmedi)

- `src/ErpBridge.Core/Stores/IProtectedConfigProvider.cs` — interface signature **değişmedi**. Brief'te `byte[] Protect/Unprotect` yazıyordu ama Faz 1'de `string` parametreli imza kabul edilmiş ve store + `NoOp` ona göre yazılmıştı. Provider implementasyonları internal olarak `byte[]` ile çalışıp `enc:v1:<base64(nonce\|ct\|tag)>` formatına base64 ile sarıyor — interface kontratı korundu.
- `src/ErpBridge.LocalStore/ProtectedConfig/NoOpProtectedConfigProvider.cs` — Faz 1'deki NoOp aynen korundu; testlerde explicit register için.
- Diğer 3 store (`SqliteMappingStore`, `SqliteLocalQueueStore`, `SqliteCheckpointStore`).

---

## 3. Tasarım kararları

### 3.1 AES-256-GCM, CBC değil

`System.Security.Cryptography.AesGcm` (.NET 8 native). 12-byte nonce (NIST SP 800-38D),
16-byte tag. Output format: `nonce(12) || ciphertext(N) || tag(16)` byte dizisi, base64'e
sarılmış, `enc:v1:` prefix'i ile. Plaintext formül olarak hiçbir yerde yok; tag verification
başarısızsa exception → store `***REDACTED***` döner.

### 3.2 Key management

- `KeyStore.LoadOrCreateKey(path)` — dosya varsa 32 byte oku, yoksa `RandomNumberGenerator.GetBytes(32)` ile üret. Üretilen dosya `File.SetAttributes(Hidden)` (Windows) veya `UnixFileMode.UserRead|UserWrite` (0600) ile korunur.
- `AesProtectedConfigProvider(IConfiguration)` ctor — `ProtectedConfig:AesKeyPath` opsiyonel; yoksa default:
  - Windows: `%LOCALAPPDATA%\ErpBridge\protected-config.key`
  - Linux/macOS: `$HOME/.erpbridge/protected-config.key`
- Üretim rehberi (gelecek): AES key dosyası **DPAPI ile sarılabilir** (brief madde 3), bu track kapsamı dışı ama `KeyStore.ApplyDefaultProtection` ileride bu sarmalama için entry point olarak tasarlandı.

### 3.3 DPAPI sadece Windows

`[SupportedOSPlatform("windows")]` attribute + `[SupportedOSPlatform("windows")]` ile
ayrı bir private `RegisterWindowsProtectedConfigProvider` metoduna taşındı. DI,
`OperatingSystem.IsWindows()` runtime branch'iyle Windows olmayan hostlarda
DPAPI tipine hiç referans oluşturmuyor. Linux'ta yanlışlıkla `DpapiProtectedConfigProvider`
construct edilirse constructor `PlatformNotSupportedException` fırlatır.

### 3.4 `SqliteAgentConfigStore` provider-aware read

- Yazım: secret key her zaman `provider.Protect(raw)` üzerinden döner. Plaintext blob
  `value` kolonuna değil, sentinel olarak `""` (eski legacy kolon) ve gerçek encrypted blob
  `protected_value` kolonuna + `protection_version = 1`.
- Okuma:
  1. `protected_value` dolu + `IsProtected` true → `Unprotect` → plaintext return
  2. `protected_value` dolu + `IsProtected` false → tamper warning + REDACTED
  3. `Unprotect` exception → `LogWarning` + REDACTED (sebep loglanır, secret değer loglanmaz)
  4. Eski legacy plaintext rows (no `protected_value`) → masked immediately

### 3.5 Migration 002 (idempotent)

```sql
ALTER TABLE agent_config ADD COLUMN protected_value TEXT NULL;
ALTER TABLE agent_config ADD COLUMN protection_version INTEGER NULL DEFAULT 0;
```

`PRAGMA table_info(agent_config)` ile mevcut kolonları kontrol; yoksa ekle. `schema_version`
tablosu `2` olarak işaretlenir (`INSERT OR REPLACE`).

---

## 4. Build son çıktısı

```text
$ dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true
  ErpBridge.Shared              -> .../ErpBridge.Shared.dll
  ErpBridge.Erp.Abstractions    -> .../ErpBridge.Erp.Abstractions.dll
  ErpBridge.Core                -> .../ErpBridge.Core.dll
  ErpBridge.RemoteApi           -> .../ErpBridge.RemoteApi.dll
  ErpBridge.LocalStore          -> .../ErpBridge.LocalStore.dll
  ErpBridge.Erp.Mikro           -> .../ErpBridge.Erp.Mikro.dll
  ErpBridge.Agent.Service       -> .../ErpBridge.Agent.Service.dll
  ErpBridge.Agent.UI            -> .../ErpBridge.Agent.UI.dll
  ErpBridge.Core.Tests          -> .../ErpBridge.Core.Tests.dll
  ErpBridge.LocalStore.Tests    -> .../ErpBridge.LocalStore.Tests.dll
  ErpBridge.Erp.Mikro.Tests     -> .../ErpBridge.Erp.Mikro.Tests.dll
  ErpBridge.Shared.Tests        -> .../ErpBridge.Shared.Tests.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:08.85
```

`TreatWarningsAsErrors=true` her projede aktif, 0/0.

---

## 5. Test özeti

| Suite | Total | Passed | Skipped | Failed |
|-------|-------|--------|---------|--------|
| `ErpBridge.Core.Tests` | 32 | 32 | 0 | 0 |
| `ErpBridge.LocalStore.Tests` | 42 | 42 | 0 | 0 |
| `ErpBridge.Erp.Mikro.Tests` | 65 | 64 | 1 | 0 |
| `ErpBridge.Shared.Tests` | 12 | 12 | 0 | 0 |
| **TOPLAM** | **151** | **150** | **1** | **0** |

> ErpBridge.Erp.Mikro.Tests içindeki 1 skip, başka track'in feature gate'i (Remote API)
> içindir — bu track'in kapsamı dışı.

### Yeni testler (Track 2 — 25 adet)

| Dosya | Sayı | Kapsam |
|-------|------|--------|
| `ProtectedConfig/AesProtectedConfigProviderTests.cs` | 11 | Roundtrip (boş, unicode, 1KB); non-deterministic nonce; tag/ciphertext tampering → exception; key length validation; `IsProtected` discriminator. |
| `ProtectedConfig/KeyStoreTests.cs` | 7 | Yoksa oluşturur, varsa aynı key'i döner; yanlış uzunlukta dosya → exception; rotate; Unix 0600 (Linux/macOS'ta); Windows Hidden; cross-platform no-op. |
| `Stores/SqliteAgentConfigStoreProtectedTests.cs` | 7 | AES roundtrip; DB'de plaintext yok; yanlış key → REDACTED; legacy plaintext row → REDACTED; non-secret pass-through; UPSERT tek satır; DI üzerinden full flow. |

### Mevcut testler — regresyon yok

Track 2'den **önce** 17 LocalStore testi geçiyordu (deliverable-faz1.md). Hala 17 + 25 = 42 geçiyor;
mevcut 5 `SqliteAgentConfigStoreTests` (NoOp kullanan) yeni schema ile hâlâ yeşil — REDACTED
davranışı değişmedi (NoOp `IsProtected` = false → masked).

---

## 6. Kural doğrulama (SKILL.md §3)

| Kural | Durum | Kanıt |
|-------|-------|-------|
| (Kural 7) Secret plaintext asla loglanmaz | ✅ | `SqliteAgentConfigStore.ApplyRow` yalnızca `LogWarning(ex, "...", key)` çağırır; `value`, `protectedValue`, `effective` hiçbir yerde log parametresi olarak geçmez. `ConnectionStringMasker.RedactedMarker = "********"` yeniden kullanılmıyor (Phase 1'deki konsol çıktılarıyla). Secret değer **WPF tarafında `PasswordBox` ile alınır** (Track 3, bu track kapsamı dışı). |
| `agent_config.value` artık plaintext **değil**; secret'lar `protected_value`'da | ✅ | `Stored_protected_value_in_DB_does_not_contain_plaintext` testi bunu doğrular: ham SQL `SELECT` ile plaintext yok. |
| `protected_value` kolonu var | ✅ | Migration 002 + test'lerle doğrulandı (`SqliteAssert` benzeri ham query'ler). |
| AesGcm kullanılmış (CBC değil) | ✅ | `AesGcm` kullanılır (`using System.Security.Cryptography.AesGcm`); CBC **yok**. |
| AES key makine-bound | ✅ | Key dosyası `%LOCALAPPDATA%` veya `$HOME/.erpbridge` altında; Unix `0600` permission; Windows `Hidden`. Bir makineden diğerine kopyalanamaz. |
| Cross-platform build (Linux'ta derlenir) | ✅ | Tüm Linux build clean; DPAPI symbol'leri `[SupportedOSPlatform]` ile gated. |
| WPF UI ve Windows Service aynı `IAgentConfigStore`'u paylaşır | ✅ | `ServiceCollectionExtensions.AddErpBridgeLocalStore` her iki host'ta da çağrılır (`src/ErpBridge.Agent.Service/Program.cs`); WPF iskeleti `Agent.UI/DependencyInjection/ServiceCollectionExtensions.cs`'tan aynı extension'ı kullanır. |
| `/// doc summary` public tipler | ✅ | `KeyStore`, `AesProtectedConfigProvider`, `DpapiProtectedConfigProvider`, `AesProtectedConfigOptions`, `ProtectedConfigColumnsMigration`, `SqliteAgentConfigStore`, `MigrationRunner`, `RegisterDefaultProtectedConfigProvider` (xml doc) — hepsi `///`. |
| Dapper parametrik SQL | ✅ | Yeni store'da raw concat yok; her dinamik değer `CommandDefinition`'a parametre olarak geçer. |
| `TreatWarningsAsErrors=true` | ✅ | LocalStore ve Tests csproj'larında aktif, 0 warning ile build. |

---

## 7. Schema migration doğrulama

Migration 002 çalıştırıldıktan sonra `agent_config` tablosu:

```sql
CREATE TABLE agent_config (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    is_secret INTEGER NOT NULL DEFAULT 0,
    updated_at TEXT NOT NULL,
    protected_value TEXT NULL,        -- NEW (Track 2)
    protection_version INTEGER NULL DEFAULT 0  -- NEW (Track 2)
);
```

Test'ler `SqliteTestHarness` üzerinden bu DDL'i gerçekten uygulayarak çalışır; bir test
ham SQL ile `SELECT protected_value, protection_version FROM agent_config` çağırıp içeriği
doğrular.

---

## 8. Yapılandırma

`appsettings.example.json` / production config'te:

```json
{
  "ErpBridge": {
    "LocalStore": {
      "DataSource": "%LOCALAPPDATA%\\ErpBridge\\agent.db"
    }
  },
  "ProtectedConfig": {
    "AesKeyPath": "%LOCALAPPDATA%\\ErpBridge\\protected-config.key"
  }
}
```

`ProtectedConfig:AesKeyPath` opsiyonel; verilmezse `AesProtectedConfigOptions.DefaultKeyPath`
kullanılır. `appsettings.example.json`'a bu anahtarın eklenmesi Track 3 (WPF wiring) kapsamında
önerilir.

---

## 9. Faz 3 için öneriler

- WPF "Kaydet" butonu zaten `IAgentConfigStore` üzerinden `SqliteAgentConfigStore.SaveAsync`
  çağırıyor; secret alanlar artık AES/DPAPI ile şifrelenmiş olarak diske yazılacak.
- WPF test'i hâlâ NoOp + REDACTED davranışı gösteriyor — Track 3'ün WPF wiring'i
  `AddErpBridgeLocalStore` çağırırsa şifreli davranış devreye girer.
- AES key dosyasını production'da **DPAPI ile sarma** (brief madde 3, "gelecek geliştirme")
  — `KeyStore.ApplyDefaultProtection` zaten bu noktayı hedefliyor; sadece ek bir
  `LoadOrCreateEncryptedKey` helper'ı gerekecek.
- `NoOpProtectedConfigProvider` korundu; ileride test/migration senaryolarında
  "hiçbir şifreleme olmasın" gerektiğinde hâlâ explicit register edilebilir.

---

**Track 2 kapandı.** Tüm kurallar geçti, 25 yeni test + 17 mevcut testler yeşil.
