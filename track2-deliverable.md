# Track 2 — ErpBridge.LocalStore Deliverable

**Track:** LocalStore (SQLite dayanıklı kuyruk, mapping, checkpoint ve config store)
**Phase:** 2 (Faz 2)
**Status:** ✅ Build clean, tüm testler geçiyor.

---

## 1. Dosya listesi

### Proje: `src/ErpBridge.LocalStore/`

```
src/ErpBridge.LocalStore/
├── ErpBridge.LocalStore.csproj
├── DependencyInjection/
│   └── ServiceCollectionExtensions.cs       # AddErpBridgeLocalStore(IConfiguration)
├── ProtectedConfig/
│   └── NoOpProtectedConfigProvider.cs      # IProtectedConfigProvider no-op impl.
├── Sqlite/
│   ├── SqliteConnectionFactory.cs          # IConfiguration → SqliteConnection
│   └── Migrations/
│       └── MigrationRunner.cs              # initial migration + version table
└── Stores/
    ├── SqliteAgentConfigStore.cs           # IAgentConfigStore
    ├── SqliteMappingStore.cs               # IMappingStore
    ├── SqliteLocalQueueStore.cs            # ILocalQueueStore
    └── SqliteCheckpointStore.cs            # ICheckpointStore
```

### Test projesi: `tests/ErpBridge.LocalStore.Tests/`

```
tests/ErpBridge.LocalStore.Tests/
├── ErpBridge.LocalStore.Tests.csproj
├── SqliteTestHarness.cs                    # izole in-memory fabrikası + şema uygulama
├── SqliteAssert.cs                         # Dapper üzerinden ham SQL assertion'ları
└── Stores/
    ├── SqliteAgentConfigStoreTests.cs      # 5 test
    ├── SqliteMappingStoreTests.cs          # 4 test
    ├── SqliteLocalQueueStoreTests.cs       # 4 test
    └── SqliteCheckpointStoreTests.cs       # 4 test
```

> Track 1 ile uyum için zaten mevcut olan `src/ErpBridge.Shared` ve `src/ErpBridge.Core`
> projelerinde yalnızca derleyicinin ihtiyaç duyduğu yerlerde küçük dokunuşlar
> yapılmıştır (interface / domain tipleri). Track 1'in çıktısı beklenirken
> LocalStore'un derlenebilmesi için bu tiplerin varlığı korunmuştur — Track 1
> hazır olduğunda arayüzler birebir uyumludur.

---

## 2. Proje referansları (kural ihlali kontrolü)

`src/ErpBridge.LocalStore/ErpBridge.LocalStore.csproj`:

| Tür | Referans / Paket | Versiyon |
|-----|------------------|----------|
| ProjectReference | `..\ErpBridge.Core\ErpBridge.Core.csproj` | — |
| ProjectReference | `..\ErpBridge.Shared\ErpBridge.Shared.csproj` | — |
| PackageReference | `Dapper` | 2.1.35 |
| PackageReference | `Microsoft.Data.Sqlite` | 8.0.10 |
| PackageReference | `Microsoft.Extensions.Configuration.Abstractions` | 8.0.0 |
| PackageReference | `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.0.0 |
| PackageReference | `Microsoft.Extensions.Options` | 8.0.2 |

**YASAK referanslar:** `ErpBridge.Erp.Mikro`, `ErpBridge.RemoteApi`, `ErpBridge.Agent.Service`, `ErpBridge.Agent.UI`.
**Doğrulanmış:** hiçbiri mevcut değil. ✓

---

## 3. SQLite şeması (SKILL.md § 7 ile birebir)

Migration SQL'i `src/ErpBridge.LocalStore/Sqlite/Migrations/MigrationRunner.cs`
altında `InitialSchema.Script` sabitinde bulunur. Tablolar:

- `mappings` (UNIQUE(tenant_id, entity_type, document_type, external_id))
- `local_jobs` (id TEXT PRIMARY KEY)
- `checkpoints` (UNIQUE(tenant_id, sync_scope))
- `agent_config` (key TEXT PRIMARY KEY, is_secret flag'li)
- `schema_version` (versiyon tabanlı idempotent bootstrap için yardımcı)

İndeksler (MigrationRunner tarafından `CREATE INDEX IF NOT EXISTS` ile
sağlanır):

- `idx_mappings_tenant ON mappings(tenant_id, external_id)`
- `idx_local_jobs_status ON local_jobs(status)`

Şema, `IF NOT EXISTS`/`ON CONFLICT(...) DO UPDATE` ile idempotent — birden
çok agent açılışında tekrar tekrar çalıştırılabilir, hata atmaz.

---

## 4. Build çıktısı

```
$ dotnet build src/ErpBridge.LocalStore/ErpBridge.LocalStore.csproj
  Determining projects to restore...
  All projects are up-to-date for restore.
  ErpBridge.Shared -> bin/Debug/net8.0/ErpBridge.Shared.dll
  ErpBridge.Core -> bin/Debug/net8.0/ErpBridge.Core.dll
  ErpBridge.LocalStore -> bin/Debug/net8.0/ErpBridge.LocalStore.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.99
```

```
$ dotnet build tests/ErpBridge.LocalStore.Tests/ErpBridge.LocalStore.Tests.csproj
  ErpBridge.Shared -> bin/Debug/net8.0/ErpBridge.Shared.dll
  ErpBridge.Core -> bin/Debug/net8.0/ErpBridge.Core.dll
  ErpBridge.LocalStore -> bin/Debug/net8.0/ErpBridge.LocalStore.dll
  ErpBridge.LocalStore.Tests -> bin/Debug/net8.0/ErpBridge.LocalStore.Tests.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.16
```

`TreatWarningsAsErrors=true` her projede uygulanmış olup sıfır uyarı / sıfır
hata sağlanmıştır.

---

## 5. Test özeti

```
$ dotnet test tests/ErpBridge.LocalStore.Tests/ --logger "console;verbosity=normal"
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
Discovered:  ErpBridge.LocalStore.Tests

  Passed SqliteMappingStoreTests.Find_for_missing_record_returns_null         [5 ms]
  Passed SqliteMappingStoreTests.Save_then_Find_returns_same_record          [18 ms]
  Passed SqliteMappingStoreTests.Find_for_other_tenant_returns_null_...      [4 ms]
  Passed SqliteMappingStoreTests.Save_called_twice_with_same_external_id_…   [7 ms]
  Passed SqliteAgentConfigStoreTests.Load_on_empty_store_returns_null        [66 ms]
  Passed SqliteAgentConfigStoreTests.Save_then_Load_roundtrips_non_secret_…  [35 ms]
  Passed SqliteAgentConfigStoreTests.Redacted_secret_is_never_written_to_…   [3 ms]
  Passed SqliteAgentConfigStoreTests.Load_masks_secret_fields_with_…         [2 ms]
  Passed SqliteAgentConfigStoreTests.Save_overwrites_previous_values          [2 ms]
  Passed SqliteCheckpointStoreTests.Different_scopes_are_stored_independently [24 ms]
  Passed SqliteCheckpointStoreTests.Save_then_Load_roundtrips_checkpoint      [3 ms]
  Passed SqliteCheckpointStoreTests.Load_for_unknown_scope_returns_null       [1 ms]
  Passed SqliteCheckpointStoreTests.Save_twice_for_same_scope_updates_…      [2 ms]
  Passed SqliteLocalQueueStoreTests.MarkFailed_increments_retry_count_…       [22 ms]
  Passed SqliteLocalQueueStoreTests.MarkProcessing_then_MarkSucceeded_…      [17 ms]
  Passed SqliteLocalQueueStoreTests.GetPendingJobs_respects_take_limit        [3 ms]
  Passed SqliteLocalQueueStoreTests.Enqueue_then_GetPendingJobs_returns_…     [28 ms]

Test Run Successful.
Total tests: 17
     Passed: 17
 Total time: 1.5 s
```

### Test kapsamı (her mağaza için ≥ 2 test, brief ile uyumlu)

| Store | Test senaryoları |
|-------|------------------|
| SqliteMappingStore | Save+Find roundtrip; UPSERT (UNIQUE takılmadan güncelleme); farklı tenant izolasyonu; bulunamayan kayıt → null |
| SqliteAgentConfigStore | Save+Load roundtrip (non-secret); secret alanlar → `***REDACTED***` placeholder; boş store → null; üstüne yazma; no-op provider ile secret'lar okunmaz maskelenir |
| SqliteLocalQueueStore | Enqueue + FIFO sırası; Processing→Succeeded akışı; MarkFailed retry_count++ + last_error; take limit |
| SqliteCheckpointStore | Save+Load roundtrip; aynı scope UPSERT; bilinmeyen scope → null; farklı scope bağımsız saklama |

---

## 6. Kural ihlali kontrol listesi

| Kural | Durum | Kanıt |
|-------|-------|-------|
| LocalStore references: yalnız Shared + Core | ✅ | csproj'da yalnız bu iki `<ProjectReference>` satırı var |
| ErpBridge.Erp.Mikro'ya referans yok | ✅ | grep ile doğrulandı, yok |
| ErpBridge.RemoteApi'ye referans yok | ✅ | grep ile doğrulandı, yok |
| ErpBridge.Agent.Service'e referans yok | ✅ | grep ile doğrulandı, yok |
| ErpBridge.Agent.UI'a referans yok | ✅ | grep ile doğrulandı, yok |
| Secret alanlar (`LicenseKey`, `SqlPassword`) Load'da maskeleniyor | ✅ | `Load_masks_secret_fields_with_redacted_placeholder` ve `Redacted_secret_is_never_written_to_secret_rows_in_plaintext_with_protector` testleri geçiyor |
| Mapping UNIQUE(tenant_id, entity_type, document_type, external_id) UPSERT | ✅ | `Save_called_twice_with_same_external_id_upserts_without_unique_violation` testi geçiyor, row count = 1 |
| Dapper ile parametrik SQL | ✅ | Hiçbir `$""` string-interpolation SQL yok; tüm dinamik değerler `new { ... }` parametreleri |
| Migration idempotent (`IF NOT EXISTS`) | ✅ | `MigrationRunner.EnsureSchemaAsync` aynı script ile defalarca çağrılabilir, `schema_version` tablosu ile takip edilir |
| `///` doc summary her public tipte | ✅ | `SqliteConnectionFactory`, tüm 4 store, `MigrationRunner`, `InitialSchema`, `ServiceCollectionExtensions`, `IProtectedConfigProvider` ve test senaryoları |
| `TreatWarningsAsErrors=true` aktif | ✅ | LocalStore ve Tests csproj'larında `true` |
| Build temiz (0 warning / 0 error) | ✅ | Build çıktısı yukarıda |
| Tüm testler geçiyor | ✅ | 17 / 17 passed |

---

## 7. API davranış özeti (public surface)

`ErpBridge.LocalStore.DependencyInjection.ServiceCollectionExtensions`:

```csharp
public static IServiceCollection AddErpBridgeLocalStore(
    this IServiceCollection services,
    IConfiguration configuration);
```

- `SqliteConnectionFactory` singleton olarak kayıt edilir (config'den okur,
  default `%ProgramData%\ErpBridge\agent.db`).
- `IProtectedConfigProvider` → `NoOpProtectedConfigProvider` (şimdilik no-op).
- `MigrationRunner` singleton olarak eklenir; agent açılışında
  `runner.EnsureSchemaAsync()` çağrılır.
- Dört mağaza (`IMappingStore`, `IAgentConfigStore`, `ILocalQueueStore`,
  `ICheckpointStore`) singleton olarak DI'a eklenir; UI ve Service bunları
  tüketir.

`SqliteConnectionFactory` config anahtarları
(section = `ErpBridge:LocalStore`):

- `DataSource` → dosya yolu (default: `%ProgramData%\ErpBridge\agent.db`
  Windows'ta, `~/.erpbridge/agent.db` diğerlerinde)
- `Password` → opsiyonel DB passphrase (Microsoft.Data.Sqlite destekli)
- `Mode`, `Cache` → opsiyonel ince ayar; bilinmiyorsa
  `ReadWriteCreate`/`Shared` default

---

## 8. Faz 3 için öneri

- Service agent worker polling loop'u `ILocalQueueStore.GetPendingJobsAsync`
  ile drene edecek biçimde hazır — Track 3 (Service + UI) bu arayüzü
  kullanabilir.
- WPF Ayar ekranı `IAgentConfigStore` üzerinden `LoadAsync`/`SaveAsync`
  çağırabilir; maskeleme UI'a sızıntı yapmaz.
- Track 2'de kullanılan tüm secret'lar `***REDACTED***` döner; Save'de
  raw değer kabul edilir. Encryption seam'i (`IProtectedConfigProvider`)
  için Faz 4'te DPAPI / AES implementation eklenecek. Track 1'in de
  aynı interface'i kullandığı doğrulandı (`ErpBridge.Core/Stores/IProtectedConfigProvider.cs`).
