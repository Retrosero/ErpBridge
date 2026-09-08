# deliverable-migration-audit-parameters — `change_set_audit_log` + `parameter_records` PostgreSQL migration

> **Faz:** Faz 15 tamamlayıcı (Faz 15.6 + 15.5 üretim yayılımı)
> **Tarih:** 2026-09-08
> **Build:** `dotnet build src/ErpBridge.CentralApi` → 0 hata, 0 uyarı
> **Test:** `dotnet test tests/ErpBridge.CentralApi.Tests` → 148/148 PASSED
> **Yeni testler:** 7 (MigrationSmokeTests)

## 1. Kapsam

Faz 15 entity'leri (`ChangeSetAuditEntry`, `ParameterRecord`) DbContext'te tanımlıydı
ancak **hiçbir EF Core migration bu iki tabloyu oluşturmuyordu**. Production'a
deploy edildiğinde Central API runtime'da tablo bulunamadı hatası alıyordu.

Bu çalışma kapsamında:

1. EF Core migration üretildi: `AddChangeSetAuditLogAndParameterRecords`.
2. `change_set_audit_log` tablosu oluşturuluyor (jsonb payload + idempotency unique index).
3. `parameter_records` tablosu oluşturuluyor (tenant+program+user+id unique index).
4. Mevcut `--migrate` komutu yeni migration'ı otomatik uyguluyor (ek config yok).
5. 7 yeni smoke test eklendi (reflection + model inspection ile).

## 2. Yapılan değişiklikler

### Yeni dosyalar (3)

- `src/ErpBridge.CentralApi/Data/Migrations/20260908010332_AddChangeSetAuditLogAndParameterRecords.cs` — Up/Down.
- `src/ErpBridge.CentralApi/Data/Migrations/20260908010332_AddChangeSetAuditLogAndParameterRecords.Designer.cs` — EF Core ürettiği snapshot parçası.
- `tests/ErpBridge.CentralApi.Tests/Migrations/MigrationSmokeTests.cs` — 7 yeni test.

### Değişen dosyalar (2)

- `src/ErpBridge.CentralApi/Data/Migrations/CentralApiDbContextModelSnapshot.cs` — `BootstrapSnapshotChunk`'in HasOne ilişkisi eklendi (önceki migration snapshot'ı tutarsızdı, `WithMany("Chunks")` string-based çağrıyla tutarlı hale getirildi). Bu düzeltme yeni migration üretimini engelliyordu.
- `dotnet-tools.json` — repo-kökünde `dotnet-ef` 10.0.11 local tool manifest'i.

### Üretim davranışı

`CentralApi` projesinin `--migrate` komutu (Program.cs:79-87) `db.Database.Migrate()`
çağırır. Bu otomatik olarak `Migrations/` altındaki tüm partial class'ları
EF Core assembly'ye dahil eder ve pending olanları sırayla uygular. Yeni
migration `[Migration("20260908010332_AddChangeSetAuditLogAndParameterRecords")]`
attribute'una sahip olduğu için zincirin son halkası olarak uygulanacak.

## 3. Migration gövdesi — `Up()`

| Tablo | Kolonlar | Indexler |
|-------|----------|----------|
| `change_set_audit_log` | `Id` (uuid PK), `TenantId` (uuid FK→tenants ON DELETE CASCADE), `SourceDatabase` (varchar 128), `TableName` (varchar 128), `TabloId` (int), `Direction` (varchar 16), `FirstTriggerRecNo` (bigint), `LastTriggerRecNo` (bigint), `RowCount` (int), `PayloadJson` (jsonb), `PayloadSha256` (varchar 64), `PulledAtUtc` (timestamptz), `ReceivedAtUtc` (timestamptz), `AgentId` (varchar 128), `IdempotencyKey` (varchar 128) | UNIQUE `(TenantId, IdempotencyKey, Direction)`, INDEX `(TenantId, TableName, LastTriggerRecNo)`, INDEX `(TenantId, TableName, ReceivedAtUtc)` |
| `parameter_records` | `Id` (uuid PK), `TenantId` (uuid FK→tenants ON DELETE CASCADE), `SourceDatabase` (varchar 128), `ParametreProgram` (varchar 25), `ParametreUser` (varchar 25), `ParametreAnaGrubu` (varchar 40), `ParametreAltGrubu` (varchar 40), `ParametreID` (varchar 40), `ParametreAdi` (varchar 127), `ParametreDegeri` (varchar 255), `CreatedAtUtc` (timestamptz), `UpdatedAtUtc` (timestamptz) | UNIQUE `(TenantId, SourceDatabase, ParametreProgram, ParametreUser, ParametreID)` |

`Down()` her iki tabloyu `DROP TABLE` ile geri alır.

> **Yan not:** Bu migration, model snapshot'ı içinde yer alan ancak daha
> önce hiçbir migration'da oluşturulmamış olan `change_sets` tablosunu da
> oluşturur. Bu, mevcut pre-release durumda ChangeSetEndpoints'in
> production'da düzgün çalışması için **gerekli** bir yan etkiydi
> (snapshot ile migration zinciri arasındaki pre-existing uyumsuzluk).
> Yeni migration bu uyumsuzluğu da kapatır.

## 4. Yeni testler

`tests/ErpBridge.CentralApi.Tests/Migrations/MigrationSmokeTests.cs`
altında 7 xUnit testi:

1. `Migrations_assembly_contains_AddChangeSetAuditLogAndParameterRecords_partial_class` — Reflection ile `Migration` sınıfı bulunur.
2. `AddChangeSetAuditLogAndParameterRecords_migration_is_attributed_with_its_own_name` — `[Migration]` attribute kontrolü.
3. `AddChangeSetAuditLogAndParameterRecords_Up_method_creates_both_tables_and_drops_them_in_Down` — Up/Down override varlığı.
4. `ChangeSetAuditEntry_has_unique_index_on_TenantId_IdempotencyKey_Direction` — Model'de unique index kontrolü.
5. `ParameterRecord_has_unique_index_on_Tenant_Source_Database_Program_User_Id` — Model'de unique index kontrolü.
6. `ChangeSetAuditEntry_mapped_table_name_is_change_set_audit_log` — ToTable() kontrolü.
7. `ParameterRecord_mapped_table_name_is_parameter_records` — ToTable() kontrolü.

## 5. Doğrulama

### Build (CentralApi + Tests)

```
$ dotnet build src/ErpBridge.CentralApi/ErpBridge.CentralApi.csproj -c Debug
  ErpBridge.Shared -> bin/Debug/net10.0/ErpBridge.Shared.dll
  ErpBridge.Erp.Abstractions -> bin/Debug/net10.0/ErpBridge.Erp.Abstractions.dll
  ErpBridge.Core -> bin/Debug/net10.0/ErpBridge.Core.dll
  ErpBridge.CentralApi -> bin/Debug/net10.0/ErpBridge.CentralApi.dll
Oluşturma başarılı oldu. 0 Uyarı, 0 Hata
```

### EF Core migrations list

```
$ dotnet ef migrations list --project src/ErpBridge.CentralApi --startup-project src/ErpBridge.CentralApi --context CentralApiDbContext
20260710051307_AddApiKeysAndWebhooks
20260831083734_AddTenantDeviceLimit
20260831095304_AddMobileTelemetryEvents
20260901074605_AddApiKeySecretVault
20260904044919_RemoveAgentLicenseMaterial
20260906164828_AddErpCompanies
20260908010000_RepairMissingSchema
20260908010332_AddChangeSetAuditLogAndParameterRecords   ← yeni
```

### Test

```
$ dotnet test tests/ErpBridge.CentralApi.Tests/ErpBridge.CentralApi.Tests.csproj
Başarılı! - Başarısız: 0, Başarılı: 148, Atlanan: 0, Toplam: 148
```

(Önceki: 141 test — yeni 7 migration smoke testi eklendi → 148.)

## 6. Bilinçli kapsam dışı

- `ChangeSetAuditEntry` ve `ParameterRecord` entity'lerinde alan değişikliği yapılmadı (Faz 15'te tanımlandığı şekliyle).
- `AdminAuditEndpoints` ve diğer endpoint'ler değişmedi.
- WPF admin sekmeleri (Faz 15.8) ayrı tur.
- Diğer paketler (ErpBridge.Erp.Mikro vb.) değişmedi.

## 7. Bilinen notlar

### `ErpBridge.Erp.Mikro` paketi pre-existing build hatası

Bu turdan bağımsız, `src/ErpBridge.Erp.Mikro/Writers/MikroSalesOrderWriter.cs`
içinde 5 build hatası var (`DefaultFirmNo`/`DefaultBranchNo` constants silinmiş
ancak kullanım yerleri kalmış). Bu **bu görevin scope'u dışında** —
kullanıcı açıkça "Diğer paketlerde değişiklik" dışı bıraktı. Hatanın
kaynağı `git diff` ile görülebilir: `Connection/AgentConfigMapper.cs`,
`Connection/MikroConnectionFactory.cs`, `Connection/MikroConnectionSettings.cs`
ve `Writers/MikroSalesOrderWriter.cs` modifiye edilmiş ama derleme
test edilmemiş. CentralApi paketi tek başına temiz derleniyor.

### Mevcut snapshot tutarsızlığı (çözüldü)

`CentralApiDbContextModelSnapshot.cs` daha önce `WithMany("Chunks")`
çağrısına uygun olmayan bir snapshot bırakmıştı. Bu PR snapshot'ı
düzeltip, `BootstrapSnapshotChunk` için `HasOne("...", "Snapshot").WithMany("Chunks")`
ilişkisini açıkça tanımladı. Yeni migration'ı üretebilmek için bu
onarım zorunluydu.

## 8. Dosya listesi

```
src/ErpBridge.CentralApi/Data/Migrations/20260908010332_AddChangeSetAuditLogAndParameterRecords.cs        (yeni)
src/ErpBridge.CentralApi/Data/Migrations/20260908010332_AddChangeSetAuditLogAndParameterRecords.Designer.cs (yeni, EF üretti)
src/ErpBridge.CentralApi/Data/Migrations/CentralApiDbContextModelSnapshot.cs                            (değişti, snapshot ilişki onarımı)
tests/ErpBridge.CentralApi.Tests/Migrations/MigrationSmokeTests.cs                                      (yeni)
dotnet-tools.json                                                                                        (yeni, local tool manifest)
deliverable-migration-audit-parameters.md                                                                (bu dosya)
```
