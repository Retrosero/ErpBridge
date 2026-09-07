# Faz 2 Planı — Android Sync (Writer Katmanı)

> Bu plan, `docs/06-android-crud-mapping.md`, `docs/07-mikro-v15-v16-stok-cari-diff.md`
> ve `docs/08-android-payload-schema.md` dokümanlarındaki kontratı uygulayan
> `SyncAdapter.Mikro` writer katmanının adım adım teslimini belgeler.
> Faz 1 (agent shell, local store, encrypted settings) zaten teslim edildi.

## 0. Hedef ve Kapsam

### 0.1 Hedef

Android saha uygulamasının gönderdiği `OfflineEvrakV2` envelope'larını
**idempotent**, **transactional**, **V15/V16 şeffaf** şekilde Mikro ERP'ye
yazan, tüm yazma hatalarını envelope'a geri bildiren bir writer katmanı.

### 0.2 Kapsam İçi

- 7 `AndroidAktarimTipi` enum değerinin tamamı (Evrak, CariLokasyon, GunAcilisi,
  GunKapanisi, Ziyaret, YazBozTahtasi, YeniCariOlusturma)
- V15 ve V16 şema desteği (`IIdentityMapper` abstraction)
- 11 alt evrak tipi (SatisSiparis, SatisIrsaliye, SatisFatura, AlisSiparis,
  AlisIrsaliye, AlisFatura, DepolarArasiNakliyeOnaylama, DepolarArasiSevk,
  DepolarArasiNakliyeFisi, Tahsilat, Tediye)
- Idempotency (mappings tablosu ile `externalId` → `mikro_recno/guid` eşleme)
- Parametreli SQL (string concat yasak)
- Tek transaction (header + tüm satırlar + ek tablolar atomik)
- Hata yönetimi: envelope'a `Hatali` durumu + `HataString` yazımı
- Outbox ack flow (sunucu → Android envelope update)

### 0.3 Kapsam Dışı (Faz 3+)

- Pull (Mikro → Android) writer — Faz 3'te CDC ile
- Beden/renk/seri lot/cihareket takipli stok detayları — Faz 2.5'te
- Lisans aktivasyonu / heartbeat — Faz 1'de zaten var, writer'da kullanım
- Central API (Android → merkezi API → agent) — henüz sadece doğrudan agent
- Windows Service installer / tray UI — Faz 7

## 1. Bağımlılıklar ve Referanslar

### 1.1 Kod Referansları (Doğrulanmış)

- `SyncAdapter/src/SyncAdapter.Core/Models/OfflineEvrakV2.cs` — envelope
- `SyncAdapter/src/SyncAdapter.Core/Models/EvrakDTO.cs` — Tip 0 payload
- `SyncAdapter/src/SyncAdapter.Core/Enums/AndroidAktarimTipi.cs` — 7 tip
- `SyncAdapter/src/SyncAdapter.Core/Enums/EvrakAktarimDurumu.cs` — durum enum
- `SyncAdapter/src/SyncAdapter.Core/Abstractions/IOfflineEvrakRepository.cs` — outbox contract
- `SyncAdapter/src/SyncAdapter.Infrastructure/Serialization/OfflineEvrakSerializer.cs` — JSON+GZip

### 1.2 Legacy Kod (Davranış Referansı, decompile edilmiş)

- `Fora_Mikro/.decompiled/Service/Fora.App.Win.Mikro.Service/MikroService.cs:2596-5350` — handler implementasyonları
- `Fora_Mikro/.decompiled/Core/Fora.Mikro.Evraklar/Evrak.cs` — Evrak yazma transaction mantığı (9167 satır)
- `Fora_Mikro/.decompiled/DataSql/Fora.Mikro.Data.Sql/CariData.cs:1253-1270` — SetCariLokasyon
- `Fora_Mikro/.decompiled/DataSql/Fora.Mikro.Data.Sql/CariExtensions.cs:138, 377` — V15/V16 INSERT kalıbı
- `Fora_Mikro/.decompiled/Core/Fora.Mikro.Tablolar/TabloHelper.cs` — V15 field listeleri
- `Fora_Mikro/.decompiled/Core/Fora.Mikro.TablolarV16/TabloHelperV16.cs` — V16 field listeleri

### 1.3 Yeni Dokümanlar (Bu PR'la geldi)

- `docs/06-android-crud-mapping.md` — Tip bazlı mapping
- `docs/07-mikro-v15-v16-stok-cari-diff.md` — V15/V16 şema farkları
- `docs/08-android-payload-schema.md` — JSON payload şemaları

## 2. Hedeflenen Dosya Yapısı

```
src/
  SyncAdapter.Core/
    Abstractions/
      IIdentityMapper.cs                  # YENİ
      IOutboxWriter.cs                    # YENİ
      IMappingRepository.cs               # YENİ
    Models/
      OfflineEvrakV2.cs                   # (mevcut)
      EvrakDTO.cs                         # (mevcut)
      MappingEntry.cs                     # YENİ
      ZiyaretDTO.cs                       # YENİ
      GuneBaslaBitirDTO.cs                # YENİ
      CariAdresDTO.cs                     # YENİ (CariLokasyon için)
      CariYazBozDTO.cs                    # YENİ
      YeniCariDTO.cs                      # YENİ
  SyncAdapter.Mikro/                       # YENİ PROJE
    IdentityMapper/
      IIdentityMapper.cs
      V15IdentityMapper.cs
      V16IdentityMapper.cs
      IdentityMapperFactory.cs
    Writers/
      IOutboxWriter.cs                    # veya SyncAdapter.Core'da
      OutboxDispatcher.cs                 # Tip → Writer
      EvrakWriter.cs                      # Tip 0
      CariLokasyonWriter.cs               # Tip 1
      GunBaslaBitirWriter.cs              # Tip 2 + 3
      ZiyaretWriter.cs                    # Tip 4
      YazBozTahtasiWriter.cs              # Tip 5
      YeniCariWriter.cs                   # Tip 6
    Mapping/
      SqlMappingRepository.cs             # externalId → recno/guid
    Lookup/
      ILookupValidator.cs
      SqlLookupValidator.cs
  SyncAdapter.Infrastructure/
    Sqlite/
      SqliteMappingRepository.cs          # YENİ
      ... (mevcut)
  SyncAdapter.Api/
    Endpoints/
      OutboxEndpoints.cs                  # YENİ
      ... (mevcut)
```

## 3. Adım Adım Teslim Planı

### Adım 1 — Identity Mapper (V15/V16 soyutlama)

**Amaç:** V15 (`RECno + *_RECid_*`) ve V16 (`Guid + *_uid`) şema farklarını tek
yerde gizlemek. Core / Service / UI yalnızca DTO ve "recno/guid değerini
ver, kolon adını sen bil" der.

**Çıktılar:**
- `SyncAdapter.Core/Abstractions/IIdentityMapper.cs`:
  ```csharp
  public interface IIdentityMapper
  {
      string GetPrimaryKeyColumn(string tableName);
      string GetCrossRefColumn(string fromTable, string toTable);
      string GetOrderLinkColumn(string stokHareketTable);
      string GetInvoiceLinkColumn(string stokHareketTable);
      string GetNewIdExpression();           // "SCOPE_IDENTITY()" | "NEWID()"
      string GetGuidLiteral(Guid? value);    // V16 için: "NULL" veya "0x..."
      string GetRecNoLiteral(int? value);    // V15 için: "0" veya rakam
      DbParameter CreateIdentityParam(object value);
  }
  ```
- `SyncAdapter.Mikro/IdentityMapper/V15IdentityMapper.cs` (V15 kuralları)
- `SyncAdapter.Mikro/IdentityMapper/V16IdentityMapper.cs` (V16 kuralları)
- `SyncAdapter.Mikro/IdentityMapper/IdentityMapperFactory.cs` (DB adından seçim)

**Doğrulama:**
- V15 mapper testleri: 11 alt tablo için doğru kolon döner.
- V16 mapper testleri: aynı 11 tablo için Guid kolonu döner.
- DB adı `MikroDB_V15_02` → V15, `MikroDB_V16_03` → V16.

**Kaynak:** `docs/07-mikro-v15-v16-stok-cari-diff.md` (tam şema).

---

### Adım 2 — Mapping Repository (Idempotency)

**Amaç:** Aynı `externalId` ile gelen ikinci push'ta Mikro'da yeni evrak
oluşmasını engelle. İlk push'ta mapping INSERT, sonrakilerde UPSERT.

**Şema:**
```sql
CREATE TABLE mappings (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    tenant_id TEXT NOT NULL,
    document_type TEXT NOT NULL,
    external_id TEXT NOT NULL,
    mikro_version INTEGER NOT NULL,
    mikro_db_name TEXT NOT NULL,
    evrak_seri TEXT NULL,
    evrak_sira INTEGER NULL,
    recno INTEGER NULL,
    guid TEXT NULL,
    checksum TEXT NOT NULL,
    created_at TEXT NOT NULL,
    UNIQUE (tenant_id, document_type, external_id)
);
```

**Çıktılar:**
- `SyncAdapter.Core/Models/MappingEntry.cs`
- `SyncAdapter.Core/Abstractions/IMappingRepository.cs`
- `SyncAdapter.Infrastructure/Sqlite/SqliteMappingRepository.cs`
- `SyncAdapter.Infrastructure/Sqlite/SqliteSchemaInitializer.cs` güncelleme (yeni tablo)

**Doğrulama:**
- Aynı `externalId` ile ikinci insert → `UNIQUE` constraint error yakalanır.
- Mapping tablosu UPDATE edilip yeni recno/guid set edilir.

**Kaynak:** `Fora_Mikro/MIKRO_ERP_VERI_AKIS_DOKUMANI.md:521-542` (önerilen şema).

---

### Adım 3 — Lookup Validator

**Amaç:** Payload içindeki FK'ların (cari_kod, stok_kod, depo_no, kasa_no,
banka_no, ödeme planı no, vs.) Mikro'da var olduğunu doğrula. Yoksa
`HataString` ile geri bildir.

**Çıktılar:**
- `SyncAdapter.Mikro/Lookup/ILookupValidator.cs`
- `SyncAdapter.Mikro/Lookup/SqlLookupValidator.cs`
- V15/V16 ortak (kolon adları aynı).

**Validasyon Listesi:**

| Tip | Kontrol |
|-----|---------|
| 0 (Evrak) | `cari_kod`, tüm `stok_kod`, `depo_no`, `temsilci_kodu` (varsa), `proje_kodu` (varsa), `sorumluluk_merkezi_kodu` (varsa) |
| 0 Tahsilat/Tediye | `cari_kod`, `banka_no` (varsa) |
| 1 (CariLokasyon) | `cari_kod` + `adr_adres_no` |
| 2/3 (Gun*) | `temsilci_kodu` |
| 4 (Ziyaret) | `temsilci_kodu`, `cari_kod`, `adr_adres_no`, `proje_kodu`/`sorumluluk_merkezi_kodu` (varsa) |
| 5 (YazBoz) | `cari_kod` |
| 6 (YeniCari) | (yeni cari oluşturma, lookup yok; adreslerde `depo_kodu` vs opsiyonel) |

---

### Adım 4 — Writer Implementasyonları (7 writer)

#### 4.1 CariLokasyonWriter (Tip 1) — İLK YAZILACAK (en basit)

```csharp
public class CariLokasyonWriter : IOutboxWriter
{
    public async Task<OfflineEvrakV2> HandleAsync(OfflineEvrakV2 env, SqlConnection conn, CancellationToken ct)
    {
        var dto = JsonSerializer.Deserialize<CariAdresDTO>(env.EvrakJson, _opts);
        // Lookup: cari_kod + adres_no mevcut mu?
        if (!await _lookup.CariAdresExistsAsync(dto.CariKod, dto.AdresNo, ct))
        {
            env.Durum = EvrakAktarimDurumu.Hatali;
            env.HataString = $"Cari adres bulunamadı: {dto.CariKod}/{dto.AdresNo}";
            return env;
        }

        // UPDATE (V15/V16 ortak, kolon adları aynı)
        const string sql = @"
            UPDATE CARI_HESAP_ADRESLERI
            SET adr_gps_enlem = @enlem, adr_gps_boylam = @boylam, adr_lastup_date = getdate()
            WHERE adr_cari_kod = @cariKod AND adr_adres_no = @adresNo";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@enlem", dto.Enlem);
        cmd.Parameters.AddWithValue("@boylam", dto.Boylam);
        cmd.Parameters.AddWithValue("@cariKod", dto.CariKod);
        cmd.Parameters.AddWithValue("@adresNo", dto.AdresNo);
        await cmd.ExecuteNonQueryAsync(ct);

        env.Durum = EvrakAktarimDurumu.Aktarildi;
        env.AktarilmaTarihi = DateTime.UtcNow;
        return env;
    }
}
```

**Test:** `CariData.cs:1259` ile aynı SQL.

#### 4.2 GunBaslaBitirWriter (Tip 2 + 3)

- Tip 2 (GunAcilisi): `_TEMSILCI_GUNLUK_HAREKETLER` INSERT, gün başına tek
  kayıt kontrolü (idempotency).
- Tip 3 (GunKapanisi): Mevcut kaydı bul, UPDATE.
- V15/V16 ayrımı: `Recno` vs `Guid` PK seçimi (`IIdentityMapper`).

**Kaynak:** `MikroService.cs:5046-5190`.

#### 4.3 YazBozTahtasiWriter (Tip 5)

- `mye_TextData` UPSERT, `TableID=31`.
- V15: `RecID_DBCno + RecID_RECno` ile.
- V16: `Record_uid` ile.

**Kaynak:** `CariSqlite.cs:1594`, `MikroService.cs:5192-5221`.

#### 4.4 ZiyaretWriter (Tip 4)

- `_ZIYARET_HAREKETLERI` INSERT, 70+ kolon.
- V16'da `zyrt_Guid, NEWID()` eklenir.

**Kaynak:** `MikroService.cs:5326-5341`.

#### 4.5 YeniCariWriter (Tip 6)

- Otomatik `cari_kod` atama: `cari_kod_prefix + (MAX(sayısal) + 1)`.
- Tek transaction: `CARI_HESAPLAR` + `CARI_HESAP_ADRESLERI[]` + `CARI_HESAP_YETKILILERI[]` + `mye_TextData`.
- V15: `SCOPE_IDENTITY()`, V16: `NEWID()`.

**Kaynak:** `MikroService.cs:5223-5324`, `CariExtensions.cs:138, 377`.

#### 4.6 EvrakWriter (Tip 0 — EN KARMAŞIK)

- 11 alt tip dispatch (`GenelEvrakTipleri`).
- Header INSERT + satır INSERT'ler, **tek transaction**.
- Sipariş karşılama ise `sip_teslim_miktar` atomik artışı.
- Beden/renk/seri lot opsiyonel (Faz 2.5'te).
- Idempotency: `evrakSeri + evrakSira` → mapping tablosu kontrolü.

**Kaynak:** `Evrak.cs:4983-7000` (9167 satırın tamamı).

---

### Adım 5 — Outbox Dispatcher (Tek Giriş Noktası)

**Amaç:** `SaveOfflineEvrakV3` endpoint'i, envelope'u Tip'e göre ilgili
writer'a yönlendirir. Tüm writer'lar `IOutboxWriter` interface'ini uygular.

```csharp
public class OutboxDispatcher
{
    private readonly Dictionary<AndroidAktarimTipi, IOutboxWriter> _writers;
    private readonly SqlConnection _conn;
    private readonly IIdentityMapper _idMapper;
    private readonly IMappingRepository _mappings;
    private readonly ILookupValidator _lookup;

    public async Task<OfflineEvrakV2> DispatchAsync(OfflineEvrakV2 env, CancellationToken ct)
    {
        if (!_writers.TryGetValue(env.Tipi, out var writer))
        {
            env.Durum = EvrakAktarimDurumu.Hatali;
            env.HataString = $"Bilinmeyen AndroidAktarimTipi: {env.Tipi}";
            return env;
        }

        using var tx = _conn.BeginTransaction();
        try
        {
            var result = await writer.HandleAsync(env, _conn, tx, ct);
            tx.Commit();
            return result;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            env.Durum = EvrakAktarimDurumu.Hatali;
            env.HataString = $"Yazma hatası: {ex.Message}";
            _log.Error(ex, "Outbox writer exception");
            return env;
        }
    }
}
```

**Endpoint:** `SyncAdapter.Api/Endpoints/OutboxEndpoints.cs`:
```csharp
app.MapPost("/SaveOfflineEvrakV3/{firmaid}/{mikroDbName}", async (
    string firmaid, string mikroDbName, HttpContext ctx, OutboxDispatcher disp, CancellationToken ct) =>
{
    var body = await ctx.Request.Body.ReadToEndAsync(ct);
    var env = OfflineEvrakSerializer.Aç(body);
    var result = await disp.DispatchAsync(env, ct);
    return Results.Json(result, statusCode: 200);
});
```

---

### Adım 6 — Entegrasyon Testleri

**Test Senaryoları (V15 + V16 ayrı ayrı):**

| Test | Beklenen |
|------|----------|
| Tip 0 SatisSiparis INSERT | `SIPARISLER` + `MappingEntry` yazılır, recno döner |
| Tip 0 aynı externalId tekrar INSERT | Yeni kayıt açılmaz, mapping update edilir |
| Tip 0 SatisFatura → sipariş karşılama | `sip_teslim_miktar` artar, atomik |
| Tip 1 CariLokasyon UPDATE | `CARI_HESAP_ADRESLERI` güncellenir |
| Tip 1 olmayan cari | `HataString` = "Cari adres bulunamadı" |
| Tip 2 GunAcilisi INSERT | `_TEMSILCI_GUNLUK_HAREKETLER` yazılır |
| Tip 2 aynı gün tekrar INSERT | HATA: "Gün açılışı daha önce XXX kayıt numarası ile yapılmış!" |
| Tip 3 GunKapanisi UPDATE | Mevcut kayıt bulunur, `Bitis_*` alanları update |
| Tip 3 GunAcilisi yapılmadan UPDATE | HATA: "Gün açılışı yapılmamış!" |
| Tip 4 Ziyaret INSERT | 70+ kolon yazılır |
| Tip 5 YazBoz UPSERT | `mye_TextData` güncellenir |
| Tip 6 YeniCari INSERT (otomatik kod) | `cari_kod_prefix + MAX + 1` atanır |
| Tip 6 YeniCari çoklu adres | Tek transaction, 3 tablo + 1 mye_TextData |
| V15 ↔ V16 aynı payload | Farklı PK, aynı cari_kod (idempotency) |

**Test Altyapısı:**
- `dotnet test` ile xUnit.
- Her test gerçek Mikro instance'ına karşı değil, **Testcontainers MSSQL**
  veya mevcut `GURBUZ\...` instance'ına karşı.
- V15 + V16 instance'larının ikisi de CI'da olmalı.

---

### Adım 7 — Observability

- Her writer için `ILogger` structured log:
  - Tip, externalId, tenantId, duration, durum
- Hata durumunda exception + stack trace.
- Başarı durumunda Mikro recno/guid (yazma sonrası).
- Serilog enricher: `TenantId`, `DocumentType`, `ExternalId`.

---

## 4. Kabul Kriterleri

- [ ] 7 writer için `dotnet build` temiz, `dotnet test` temiz.
- [ ] V15 + V16 her ikisinde entegrasyon testleri geçer.
- [ ] `dotnet build` ile sıfır uyarı, sıfır hata.
- [ ] Her writer için en az 1 entegrasyon testi var.
- [ ] Her writer için `docs/08-android-payload-schema.md` ile uyumlu (şema değişirse dokümanı güncelle).
- [ ] Tüm SQL parametreli (string concat kontrolü için Roslyn analyzer veya manual review).
- [ ] Hiçbir writer doğrudan `_conn.Close()` çağırmaz (DI'da yaşam döngüsü yönetimi).
- [ ] Mapping tablosu ile idempotency garantili (aynı externalId → aynı recno).
- [ ] `deliverable.md` çıktısı: değişen dosyalar, test özetleri, build çıktısı.

## 5. Faz İçi Sıralama (Tahmini)

| Adım | Süre (tahmini) | Bağımlılık |
|------|----------------|------------|
| 1. IdentityMapper + testler | 0.5 gün | — |
| 2. MappingRepository + şema | 0.5 gün | Faz 1 SqliteSchemaInitializer |
| 3. LookupValidator | 0.5 gün | IdentityMapper (lookup kolon isimleri için) |
| 4.1 CariLokasyonWriter | 0.25 gün | 1, 2, 3 |
| 4.2 GunBaslaBitirWriter | 0.5 gün | 1, 2, 3 |
| 4.3 YazBozTahtasiWriter | 0.5 gün | 1, 2, 3 |
| 4.4 ZiyaretWriter | 0.5 gün | 1, 2, 3 |
| 4.5 YeniCariWriter | 1 gün | 1, 2, 3 |
| 4.6 EvrakWriter (11 alt tip) | 2 gün | 1, 2, 3, 4.5 |
| 5. OutboxDispatcher + endpoint | 0.5 gün | Tüm 4.x |
| 6. Entegrasyon testleri (V15+V16) | 1 gün | 5 |
| 7. Observability | 0.25 gün | 5 |
| **Toplam** | **~8 gün** | — |

## 6. Açık Sorular

1. **V15 vs V16 aynı CI ortamı:** Mavis geliştirici makinesinde V15 DB var.
   V16 instance'ı nasıl sağlanır? (Coolify'da test DB veya lokalde Docker MSSQL 2022)
2. **Eski Mikro lisansı:** Android → writer → MSSQL arasında lisans kontrolü
   tekrar gerekiyor mu, yoksa Faz 1'deki `LisansBitisTarihi` yeterli mi?
3. **mye_TextData insert'i:** Yeni cari oluştururken `TableID=31` notu opsiyonel
   mi? Legacy kodda `CariData.UpdateTextData` UPDATE olarak yapıyor.
   Insert için ayrı bir dal mı, mevcut UPSERT mi?
4. **Beden/renk/seri lot:** Faz 2'de mı yoksa Faz 2.5'te mi? Plan burada opsiyonel
   tutuyor (Faz 2.5).

## 7. Doğruluk Kanıtı

| Plan adımı | Doküman referansı |
|-----------|-------------------|
| Adım 1 (IdentityMapper) | `docs/07-mikro-v15-v16-stok-cari-diff.md:5-V15/V16 karar verme kodu` |
| Adım 2 (Mapping şema) | `MIKRO_ERP_VERI_AKIS_DOKUMANI.md:521-542` (önerilen tablo) |
| Adım 3 (Lookup listesi) | `docs/06-android-crud-mapping.md:3-Push mapping tablosu` |
| Adım 4.1 (CariLokasyon) | `CariData.cs:1253-1270` |
| Adım 4.2 (GunAcilisi/Kapanisi) | `MikroService.cs:5046-5190` |
| Adım 4.3 (YazBoz) | `CariSqlite.cs:1594`, `MikroService.cs:5192-5221` |
| Adım 4.4 (Ziyaret) | `MikroService.cs:5326-5341` |
| Adım 4.5 (YeniCari) | `MikroService.cs:5223-5324`, `CariExtensions.cs:138, 377` |
| Adım 4.6 (Evrak) | `Evrak.cs:4983-7000` |
| Adım 5 (Dispatcher) | `MikroService.cs:2689-2716` (tip switch) |
| Adım 6 (Test senaryoları) | `docs/06-android-crud-mapping.md:10-Doğruluk kanıtı` |
| Adım 7 (Observability) | `Fora_Mikro/.decompiled/Core/Fora.Mikro.Senkronizasyon/GuncellemeServisi.cs:87-105` (event-based log kalıbı) |

## 8. Faz 2 Kapanış Checklist'i

- [ ] Tüm writer'lar implement edildi.
- [ ] Tüm entegrasyon testleri V15 + V16'da yeşil.
- [ ] `dotnet build` ve `dotnet test` temiz.
- [ ] `deliverable.md` yazıldı (değişen dosyalar, test özetleri).
- [ ] Lisans + heartbeat + expiry purge (Faz 2 sonunda yeniden doğrulanır).
- [ ] PR açıldı, code review tamamlandı.
- [ ] Mavis Verifier FAIL yoksa Faz 3'e (Pull/CDC) geçiş onayı.

— Üretildi: 2026-09-07
— Sahibi: developer (Mavis worker)
— Sonraki Faz: 03-android-pull-cdc (henüz planlanmadı)
