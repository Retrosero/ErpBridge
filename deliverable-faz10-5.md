# Deliverable — Faz 10.5 (MikroSalesOrderWriter Multi-Firm)

> **Tema:** `MikroSalesOrderWriter` içindeki hardcode `DefaultFirmNo = 1` ve
> `DefaultBranchNo = 0` sabitleri kaldırıldı. INSERT sırasında
> `sip_firmano` / `sip_sube_no` (header) ve `sth_firmano` / `sth_sube_no`
> (line) kolonları artık `MikroConnectionSettings.CompanyNo` ve
> `MikroConnectionSettings.BranchNo`'dan besleniyor. Multi-firm (çok-firma)
> ve multi-branch (çok-şube) Mikro kurulumları artık sales order write
> path'inde tam destekleniyor.

## Kapsam

| Alt-görev | Hedef | Durum |
|---|---|---|
| 10.5.1 | `MikroConnectionSettings.BranchNo` alanı (default 0) | ✅ |
| 10.5.2 | `MikroConnectionSettings.FromConfiguration` BranchNo parse (invariant, default 0) | ✅ |
| 10.5.3 | `AgentConfigMapper.FromAgentConfig` BranchNo propagation (önceden düşürülüyordu) | ✅ |
| 10.5.4 | `MikroConnectionFactory` aktif settings initializer'ına BranchNo eklendi | ✅ |
| 10.5.5 | `MikroSalesOrderWriter.DefaultFirmNo` / `DefaultBranchNo` hardcode'ları kaldırıldı | ✅ |
| 10.5.6 | `InsertHeaderAsync` + `InsertLineAsync` parametreleri `connectionSettings.CompanyNo` / `BranchNo` ile besleniyor | ✅ |
| 10.5.7 | Debug-level audit log: connection açılmadan önce CompanyNo / BranchNo / Strategy yazılıyor | ✅ |
| 10.5.8 | 4 yeni BranchNo testi (`MikroConnectionSettingsTests`) | ✅ |
| 10.5.9 | 4 yeni writer multi-firm testi (`MikroSalesOrderWriterMultiFirmTests`) | ✅ |

## Mimari

```
+--------------------+   +----------------------+
| AgentConfig        |   | MikroConnectionSettings
| - CompanyNo    ────┼───┼─▶ CompanyNo
| - BranchNo     ────┼───┼─▶ BranchNo
| - WarehouseNo  ────┼───┼─▶ WarehouseNo
+--------------------+   +----------------------+
        ▲                          │
        │ Save (WPF)               │ FromConfiguration
        │                          │ (live IConfiguration)
        ▼                          ▼
+--------------------+   +----------------------+
| SqliteAgentConfig  |   | MikroSalesOrderWriter
| Store              |   | (INSERT path)
+--------------------+   +----------------------+
                                       │
                                       │ FirmNo/BranchNo
                                       ▼
                                +----------------------+
                                | Mikro SQL Server      |
                                | - sip_firmano        |
                                | - sip_sube_no        |
                                | - sth_firmano        |
                                | - sth_sube_no        |
                                +----------------------+
```

**Veri akışı (sales order write):**
1. Operatör WPF'te Firma No = 3, Şube No = 5 girip "Kaydet"e basar.
2. `AgentConfig` (CompanyNo=3, BranchNo=5, WarehouseNo=…) SQLite'a yazılır.
3. Aynı anda `MutableMemoryConfigurationProvider`'a `Mikro:CompanyNo=3`,
   `Mikro:BranchNo=5` yazılır.
4. `AgentWorker` adapter'a `WriteAsync(payload, mappings, settings)` çağırır.
5. Writer validate → idempotency → lookups → version detect'ten geçer.
6. `InsertSalesOrderAsync` connection açılmadan **önce** debug log yazar:
   `companyNo=3, branchNo=5, strategy=V15/RECno, …` (her `MARS` yazımında
   denetlenebilir).
7. INSERT header (`SIPARISLER`) ve her line (`STOK_HAREKETLERI`) `@FirmNo =
   settings.CompanyNo` ve `@BranchNo = settings.BranchNo` ile bağlanır.
8. Mapping store (SQLite) eskisi gibi `external_id` UNIQUE anahtarıyla
   kaydedilir — mapping store şeması değişmedi (out-of-scope).

## Değişen / yeni dosyalar

### Değişen dosyalar (Mikro paketi içinde)
- `src/ErpBridge.Erp.Mikro/Connection/MikroConnectionSettings.cs` — `BranchNo`
  alanı + parse + XML doc
- `src/ErpBridge.Erp.Mikro/Connection/MikroConnectionFactory.cs` — aktif
  settings initializer'ında `BranchNo: 0`
- `src/ErpBridge.Erp.Mikro/Connection/AgentConfigMapper.cs` — `branchNo`
  parametresi artık `MikroConnectionSettings`'e akıyor (Faz 10'da düşüyordu;
  bu bir bug fix)
- `src/ErpBridge.Erp.Mikro/Writers/MikroSalesOrderWriter.cs` — `DefaultFirmNo` /
  `DefaultBranchNo` sabitleri kaldırıldı; `connectionSettings.CompanyNo` /
  `BranchNo` parametre bağlama; connection açılmadan önce debug audit log
- `tests/ErpBridge.Erp.Mikro.Tests/Connection/MikroConnectionSettingsTests.cs`
  — +4 BranchNo testi

### Yeni dosyalar (Mikro paketi içinde)
- `tests/ErpBridge.Erp.Mikro.Tests/Writers/MikroSalesOrderWriterMultiFirmTests.cs`
  — 4 yeni writer testi (CapturingLogger tabanlı)

## Wire şeması (değişen SQL parametreleri)

| Sürüm | Kolon | Önceki | Şimdi |
|-------|-------|--------|-------|
| V15 header | `sip_firmano` | `DefaultFirmNo` (1) | `settings.CompanyNo` |
| V15 header | `sip_sube_no`  | `DefaultBranchNo` (0) | `settings.BranchNo` |
| V15 line   | `sth_firmano` | `DefaultFirmNo` (1) | `settings.CompanyNo` |
| V15 line   | `sth_sube_no`  | `DefaultBranchNo` (0) | `settings.BranchNo` |
| V15 line   | `sth_sip_RECid_DBCno` | `DefaultActiveDbNo` (0) | aynı (0) |
| V15 line   | `sth_sip_RECid_RECno` | `SCOPE_IDENTITY()` | aynı |
| V16 header | `sip_firmano` | `DefaultFirmNo` (1) | `settings.CompanyNo` |
| V16 header | `sip_sube_no`  | `DefaultBranchNo` (0) | `settings.BranchNo` |
| V16 header | `sip_Guid`    | `Guid.NewGuid()` | aynı |
| V16 line   | `sth_firmano` | `DefaultFirmNo` (1) | `settings.CompanyNo` |
| V16 line   | `sth_sube_no`  | `DefaultBranchNo` (0) | `settings.BranchNo` |
| V16 line   | `sth_sip_uid` | header Guid | aynı |

`@FirmNo` / `@BranchNo` parametre isimleri SQL şablonunda değişmedi; sadece
bind edilen değer kaynağı `connectionSettings`'e kaydırıldı.

## Mimari kararlar

| Karar | Gerekçe |
|------|---------|
| `DefaultFirmNo` / `DefaultBranchNo` sabitleri tamamen kaldırıldı | Faz 10'da `CompanyNo` / `BranchNo` zaten MikroConnectionSettings'te taşınıyordu; writer'ın hâlâ hardcode default kullanması tek-firmalı kurulum dışındaki her şeyi kırıyordu. |
| `BranchNo` default = **0** (Faz 10'da `CompanyNo` default = 1, `WarehouseNo` default = 1) | Tek-şubeli (single-branch) Mikro veritabanlarında `sip_sube_no` / `sth_sube_no` = 0 yaygın; default=1 zorla koysaydık tek-şubeli kurulumlar sessizce bozulurdu. |
| `AgentConfigMapper.FromAgentConfig` artık `branchNo`'yu da akıtıyor | Faz 10'da `branchNo` parametre olarak alınıp **kullanılmıyordu** (parametreden constructor'a geçirilmiyordu) — bu bir bug fix; UI'da Şube No girilmesine rağmen Mikro tarafında her zaman 0 yazılıyordu. |
| Debug-level audit log INSERT'ten önce | Connection açılmadan önce loglandığı için (a) SQL connect hatalarında bile hangi CompanyNo/BranchNo ile denendiği gözükür, (b) hermetic testler SqlException'ı catch'ledikten sonra log üzerinden parametre değerlerini doğrulayabilir. Production Information seviyesi zaten ayrı (`Resolved Mikro {Strategy}…`). |
| `DefaultActiveDbNo` = 0 sabit olarak korundu | V15 link kolonu `sth_sip_RECid_DBCno` Mikro'nun "orijin DB" kolonu — agent aynı DB'ye yazdığı için 0. Bu sabit kalır, sadece dokümantasyon eklendi. |
| Mapping store şeması değişmedi (out-of-scope) | Spec'in "Sınırlamalar" bölümünde açıkça kapsam dışı; multi-firma'da aynı `external_id` iki firmada gelirse bug; reconciliation ayrı bir track. |

## Test özeti

| Suite | Yeni | Toplam (Faz 10.5 sonrası) | Durum |
|-------|------|----------------------------|-------|
| `MikroConnectionSettingsTests` (BranchNo) | 4 | 4 | ✅ |
| `MikroSalesOrderWriterMultiFirmTests` (yeni) | 4 | 4 | ✅ |
| **Yeni toplam** | **8 yeni** | | |
| `MikroConnectionSettingsTests` (regression) | — | 12/12 | ✅ |
| `AgentConfigMapperTests` (regression) | — | 14/14 | ✅ |
| `MikroAdapterMultiFirmTests` (regression) | — | 5/5 | ✅ |
| `MikroSalesOrderWriterTests` (regression) | — | 11/11 (+ 3 skip) | ✅ |
| Diğer Mikro non-integration | — | 75/75 | ✅ |
| **Mikro non-integration toplam** | | **117/117 + 3 skip** | ✅ |
| Mikro integration (16 test) | — | 0/0 + 13 skip (gated) | ✅ |
| **Mikro toplam** | | **117/117 + 16 skip** | ✅ |

## Test detayları

**`MikroConnectionSettingsTests` BranchNo (4 yeni):**
- `FromConfiguration_parses_BranchNo_when_present` — `Mikro:BranchNo=5`
  parse edilir; CompanyNo / WarehouseNo propagation da doğrulanır.
- `FromConfiguration_defaults_BranchNo_to_0_when_missing` — Eski
  single-branch kurulum için default 0 (1 değil).
- `FromConfiguration_falls_back_to_0_when_BranchNo_is_unparsable` — `"ana
  sube"` gibi non-integer değer hata fırlatmaz, 0'a düşer.
- `FromConfiguration_uses_invariant_culture_for_BranchNo_parsing` — Türkçe
  locale'ta `"  12  "` invariant olarak parse edilir.

**`MikroSalesOrderWriterMultiFirmTests` (4 yeni):**
- `WriteSalesOrderAsync_V15_uses_CompanyNo_and_BranchNo_from_settings` —
  V15 path, `CompanyNo=3, BranchNo=5` ayarlarla, header parametre log'unda
  bu değerlerin gözüktüğü doğrulanır.
- `WriteSalesOrderAsync_V16_uses_CompanyNo_and_BranchNo_from_settings` —
  V16 path (GuidStrategy) için aynı doğrulama.
- `WriteSalesOrderAsync_defaults_to_CompanyNo_1_BranchNo_0_when_settings_are_minimal` —
  Sadece SQL-auth alanları verilmiş minimal settings → writer
  `CompanyNo=1, BranchNo=0` (record default) ile INSERT deniyor.
- `WriteSalesOrderAsync_propagates_CompanyNo_3_BranchNo_5_to_link_columns` —
  V15 (RecnoStrategy) + `CompanyNo=3, BranchNo=5` ayarlarla, log'da hem
  `companyNo=3` / `branchNo=5` hem `strategy=V15/RECno` (yani V15 link
  kolonu `sth_sip_RECid_RECno` seçildi) doğrulanır.

## Build / test komutları

```powershell
# Build (her zaman temiz olmalı)
dotnet build ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor

# Tüm Faz 10.5 Mikro testleri (integration skip'li)
dotnet test tests\ErpBridge.Erp.Mikro.Tests\ErpBridge.Erp.Mikro.Tests.csproj `
  -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor `
  --filter "FullyQualifiedName!~Integration"

# Sadece Faz 10.5 yeni testleri
dotnet test tests\ErpBridge.Erp.Mikro.Tests\ErpBridge.Erp.Mikro.Tests.csproj `
  -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor `
  --filter "FullyQualifiedName~MikroSalesOrderWriterMultiFirmTests|FullyQualifiedName~BranchNo"
```

## Bilinen sınırlar / sonraki faz önerileri

- **Mapping store firma/şube bağlamı yok (out-of-scope, kapsam dışı).**
  Aynı `external_id` iki firmada gelirse mapping key çakışır; bu Faz 10.5
  kapsamı dışında bilinçli olarak bırakıldı. İleride `idempotency_mapping`
  tablosuna `company_no` / `branch_no` kolonları eklenip UNIQUE index
  genişletilebilir (küçük migration).
- **Cross-DB atomicity hâlâ yok (Faz 6 sınırı).** Mapping save SQL Server
  COMMIT'ten sonra; mapping save başarısız olursa evrak oluşur ama mapping
  yoktur → sonraki retry idempotency hit bulamaz. Reconciliation ayrı track.
- **Debug-level audit log default kapalı.** Production Information level
  kullanır; audit log ancak `LogLevel: Debug` ile görünür. Operasyonel
  ihtiyaç olursa Information seviyesine çekilebilir.
- **V15 default firma/şube zorunluluğu.** Tüm Mikro kurulumları için
  `Mikro:CompanyNo` / `Mikro:BranchNo` configuration key'leri operator
  tarafından WPF'te girilebilir; mevcut tek-firmalı kurulumlar
  `AgentConfig`'in default 1/0/1 değerlerini kullanır (regresyon yok).
- **Tahsilat / İrsaliye / Fatura (Wave 4).** Aynı pattern bu writer'lara da
  uygulanacak; Faz 10.5 bu pattern için referans implementasyon oldu.
- **Pre-existing test host bug** — `AndroidEndpointsTests` +
  `LicensesValidateTests.Health_check_*` .NET 8.0.x `PipeWriter.UnflushedBytes`
  hatası. Faz 10.5 kapsamı dışı; yeni kod bu sorundan etkilenmiyor
  (writer SQL yazmıyor, hermetic log-based doğrulama yapıyor).

## Manuel smoke test (entegre TULPAR + lisans sunucusu)

1. WPF Ayarlar sekmesini aç → "Firma No = 3", "Şube No = 5", "Depo No = 7" gir.
2. "Kaydet" → SQLite `agent_config` tablosuna yazılır; IConfiguration'a
   `Mikro:CompanyNo=3, Mikro:BranchNo=5, Mikro:WarehouseNo=7` yansır.
3. WPF'te "Bağlantıyı test et" → başarılı (Faz 3 detector aynı).
4. Bootstrap'ı tetikle → log: `companyNo=3, warehouseNo=7` (Faz 10 reader).
5. Satış siparişi yazma akışı (AgentWorker) → debug log'da
   `Mikro sales-order INSERT parameters: companyNo=3, branchNo=5,
   warehouseNo=7, strategy=V15/RECno, …` gözükür.
6. SQL Server Management Studio'da `SELECT sip_firmano, sip_sube_no FROM
   SIPARISLER WHERE sip_evrakno_sira = @N` → `(3, 5)` satırı oluştuğu
   doğrulanır.
7. `STOK_HAREKETLERI`'nde `sth_firmano = 3, sth_sube_no = 5` (header
   siparişle aynı firma/şubede).
8. Aynı siparişi tekrar gönder → idempotency hit (mapping store),
   Mikro'da yeni satır oluşmaz.
