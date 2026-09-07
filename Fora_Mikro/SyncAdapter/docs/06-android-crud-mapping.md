# Android CRUD Mapping Tablosu

> Bu doküman, `Fora Mikro` WinForms uygulamasının Android saha uygulamasıyla
> konuştuğu tüm veri akışını, `AndroidAktarimTipi` enum'undaki her değer için
> hangi Mikro tablosuna ne yazıldığını, hangi handler'ın çağrıldığını ve payload
> formatını belgeler. Yeni `SyncAdapter` projesinin writer katmanı yazılırken
> referans noktası olarak kullanılmalıdır.

## 1. Genel Akış (Push / Pull)

```
┌──────────────┐          ┌────────────────────────┐          ┌──────────────────┐
│  Android     │  HTTP    │  Windows Makinesi      │   TCP    │  Mikro SQL       │
│  Saha App    │ ◄──────► │  Fora Mikro WinForms   │ ◄──────► │  (V15 veya V16)  │
│  (Kotlin)    │  JSON+GZ │  veya SyncAdapter      │  ADO.NET │  MSSQL           │
└──────────────┘          └────────────────────────┘          └──────────────────┘
        │                              │                               ▲
        │                              │  Trigger Tabanlı Delta        │
        │                              └──────────────────────────────►│
        │                              (UPDATE/DELETE trigger tablosu)│
```

- **Android → Mikro:** Doğrudan değil. Android, outbox (`OFFLINE_KAYITLAR`,
  yerel SQLite) üzerinden HTTP POST gönderir. Sunucu payload'u alır, Mikro
  SQL'e transaction içinde yazar, RecId/Guid döner.
- **Mikro → Android:** Doğrudan değil. MSSQL trigger tablosu değişen/silinen
  RECno'ları toplar. Windows servis periyodik `GetTabloDegisenKayitlarTopluV2`
  ve `GetSilinenKayitlarTopluV2` ile delta çeker, lokal SQLite'a uygular.
  Android lokal tablodan okur.

**Kaynaklar (kod referansları):**
- `Fora_Mikro/.decompiled/Service/Fora.App.Win.Mikro.Service/MikroService.cs:2596` — `SaveOfflineEvrakV3` entrypoint
- `Fora_Mikro/.decompiled/Service/Fora.App.Win.Mikro.Service/MikroService.cs:2689-2716` — `switch (offlineEvrakV.Tipi)` dispatcher
- `Fora_Mikro/.decompiled/Core/Fora.Mikro.Evraklar/OfflineEvrakV2.cs:7-91` — envelope tanımı
- `Fora_Mikro/.decompiled/Core/Fora.Mikro.Evraklar/OfflineEvrakSqlite.cs:11-316` — outbox SQLite işlemleri
- `Fora_Mikro/.decompiled/Core/Fora.Mikro.Senkronizasyon/GuncellemeServisi.cs:130-240` — sync ana akış (lisans, versiyon tespiti)
- `Fora_Mikro/.decompiled/Core/Fora.Mikro.Senkronizasyon/GuncellemeServisi.cs:243-700` — tablo bazlı delta çekme
- `Fora_Mikro/SyncAdapter/src/SyncAdapter.Core/Enums/AndroidAktarimTipi.cs` — yeni projedeki enum
- `Fora_Mikro/SyncAdapter/src/SyncAdapter.Core/Enums/EvrakAktarimDurumu.cs` — yeni projedeki durum enum
- `Fora_Mikro/SyncAdapter/src/SyncAdapter.Core/Models/OfflineEvrakV2.cs` — yeni projedeki envelope
- `Fora_Mikro/SyncAdapter/src/SyncAdapter.Infrastructure/Serialization/OfflineEvrakSerializer.cs` — JSON+GZip serileştirme

## 2. AndroidAktarimTipi Enum

```csharp
public enum AndroidAktarimTipi
{
    Evrak = 0,                  // Asıl ticari evrak (sipariş, tahsilat, irsaliye, fatura)
    CariLokasyon = 1,           // Müşteri GPS koordinat güncellemesi
    GunAcilisi = 2,             // Temsilcinin güne başlaması
    GunKapanisi = 3,            // Temsilcinin günü kapatması
    Ziyaret = 4,                // Müşteri ziyaret raporu
    YazBozTahtasi = 5,          // Cari serbest not (whiteboard)
    YeniCariOlusturma = 6       // Sahada yeni müşteri kartı açma
}
```

## 3. Push (Android → Mikro) Mapping Tablosu

Aşağıdaki tablo, `MikroService.SaveOfflineEvrakV3` içindeki `switch (offlineEvrakV.Tipi)`
dispatcher'ın her kolu için:

- **Handler:** Sunucu tarafındaki fonksiyon
- **Yazılan Tablolar:** MSSQL'e etki eden tablolar
- **İşlem:** INSERT / UPDATE / UPSERT
- **Payload:** `Evrak` (BLOB) içeriğinin binary veya JSON şeması

| `Tipi` | Enum | Handler (`MikroService.cs`) | Yazılan/Güncellenen Tablo(lar) | İşlem | Payload (binary field sırası) |
|--------|------|----------------------------|-------------------------------|-------|-------------------------------|
| **0** | `Evrak` | `EvrakKaydetV2` (satır 2704) | `CARI_HESAP_HAREKETLERI` (header) + `STOK_HAREKETLERI` (satırlar) + opsiyonel `EVRAK_ACIKLAMALARI`, `ODEME_EMIRLERI` + `SIPARISLER.sip_teslim_miktar` (UPDATE, sipariş karşılama ise) | INSERT, tek transaction | `Evrak.cs` 9167 satırlık DTO; sadeleştirilmiş hali `SyncAdapter/src/SyncAdapter.Core/Models/EvrakDTO.cs:12-72` |
| **1** | `CariLokasyon` | `CariLokasyonGuncelleV2` (satır 2691) | `CARI_HESAP_ADRESLERI` | UPDATE; kolonlar: `adr_gps_enlem`, `adr_gps_boylam`, `adr_lastup_date` | `CariAdres.ReadFromStream`: `cari_kod`, `adr_adres_no`, `enlem`, `boylam` |
| **2** | `GunAcilisi` | `GunAcilisiYapV2` (satır 2694) | `_TEMSILCI_GUNLUK_HAREKETLER` | INSERT; kolonlar: `Temsilci_Kodu`, `Tarih`, `Baslama_*`, `Enlem`, `Boylam`, `Arac_Km`, `Mesaj`. V16'da `Guid + NEWID()`. Varsa HATA: "Gün açılışı daha önce XXX kayıt numarası ile yapılmış!" | `GuneBaslaBitir`: temsilci, tarih, saat, enlem, boylam, km, mesaj |
| **3** | `GunKapanisi` | `GunKapanisiYapV2` (satır 2697) | `_TEMSILCI_GUNLUK_HAREKETLER` | UPDATE; kolonlar: `Bitis_Yapildi`, `Bitis_Saati`, `Bitis_Kayit_Saati`, `Bitis_Enlem`, `Bitis_Boylam`, `Bitis_Arac_Km`, `Bitis_Mesaj`, `Lastup_Date`. WHERE: V15 `Recno`, V16 `Guid` | Aynı `GuneBaslaBitir` (Bitis_* alanları dolu) |
| **4** | `Ziyaret` | `ZiyaretKaydetV2` (satır 2700) | `_ZIYARET_HAREKETLERI` | INSERT; 70+ kolon (tarih, temsilci, cari, adres, GPS, başlama/bitiş saat, anket skorları, fotoğraf ID, proje, sorumluluk, memnuniyet, özel 5 alan × 7 alt-alan). V16'da `zyrt_Guid + NEWID()`. | `Ziyaret` DTO |
| **5** | `YazBozTahtasi` | `CariYazBozGuncelle` (satır 2706) | `mye_TextData` | UPSERT; `TableID=31`. V15 `RecID_DBCno + RecID_RECno` ile, V16 `Record_uid` ile | `BinaryReader`: versiyon (int32), `cari_kod` (string), `data` (string) |
| **6** | `YeniCariOlusturma` | `YeniCariOlustur` (satır 2709) | `CARI_HESAPLAR` (INSERT) + `CARI_HESAP_ADRESLERI` (INSERT, N adet) + `CARI_HESAP_YETKILILERI` (INSERT, N adet) + `mye_TextData` (INSERT, TableID=31) | INSERT, tek transaction, çoklu tablo. `cari_kod` boşsa otomatik atanır (`prefix + MAX(sayısal) + 1`). | `Cari` DTO + `CariAdres[]` + `CariYetkili[]` |

### Detay: `Evrak` (Tip 0) Sub-Akışları

`EvrakKaydetV2` dispatcher içinde, `Evrak.cs` (9167 satır) tüm alt-tipler için ortak kod yolu izler:

| `GenelEvrakTipleri` | Değer | Header Tablo | Satır Tabloları | Ek |
|--------------------|-------|--------------|-----------------|-----|
| `SatisSiparis`     | 1     | `SIPARISLER` | —               | `sip_teslim_miktar = 0` (yeni sipariş) |
| `SatisIrsaliye`    | 2     | —            | `STOK_HAREKETLERI` | `sth_tip=2, sth_cins=1` |
| `SatisFatura`      | 3     | `CARI_HESAP_HAREKETLERI` (Borç) | `STOK_HAREKETLERI` (her kalem) | Header + satırlar tek transaction |
| `AlisSiparis`      | 4     | `SIPARISLER` | —               | `sip_musteri_kod = tedarikçi cari` |
| `AlisIrsaliye`     | 5     | —            | `STOK_HAREKETLERI` | `sth_tip=5` |
| `AlisFatura`       | 6     | `CARI_HESAP_HAREKETLERI` (Alacak) | `STOK_HAREKETLERI` | Header + satırlar |
| `DepolarArasiNakliyeOnaylama` | 7 | `DEPOLAR_ARASI_SIPARISLER` | — | `ssip_teslim_miktar = 0` |
| `DepolarArasiSevk` | 8     | `DEPOLAR_ARASI_SIPARISLER` | `STOK_HAREKETLERI` | Sevk + stok transfer |
| `DepolarArasiNakliyeFisi` | 9 | `DEPOLAR_ARASI_SIPARISLER` | `STOK_HAREKETLERI` | Nakliye fişi |
| `Tahsilat`         | 50    | `CARI_HESAP_HAREKETLERI` (Alacak) | — (opsiyonel `ODEME_EMIRLERI`) | `cha_tip=Alacak` |
| `Tediye`           | 51    | `CARI_HESAP_HAREKETLERI` (Borç) | — (opsiyonel `ODEME_EMIRLERI`) | `cha_tip=Borç` |

**Sipariş karşılama:** Irsaliye veya Fatura bir siparişi karşılıyorsa aynı transaction içinde `SIPARISLER.sip_teslim_miktar` atomik artırılır. Beden/renk/seri lot takipli stoklarda `BEDEN_HAREKETLERI`, `STOK_SERINO_TANIMLARI`, `CIHAZ_HAREKETLERI` de yazılır.

### Detay: `CariLokasyon` (Tip 1)

Tek satırlık UPDATE, V15/V16 aynı (kolon adları ortak):

```sql
UPDATE CARI_HESAP_ADRESLERI
SET adr_gps_enlem = @adr_gps_enlem,
    adr_gps_boylam = @adr_gps_boylam,
    adr_lastup_date = getdate()
WHERE adr_cari_kod = @cari_kod
  AND adr_adres_no = @adr_adres_no
```

Kaynak: `Fora_Mikro/.decompiled/DataSql/Fora.Mikro.Data.Sql/CariData.cs:1253-1270` (`SetCariLokasyon`).

## 4. Pull (Mikro → Android) — Sadece Okuma

Android, Mikro tablolarını **doğrudan** okumaz. Windows servis periyodik olarak
her senkronize edilecek tablo için delta çeker ve lokal SQLite'a yansıtır.

### 4.1 Tetikleme Mekanizması (CDC)

MSSQL'de her Mikro tablosu için bir `UPDATE` ve bir `DELETE` trigger'ı vardır
(`mikro_sync` JavaScript ekibinin yazdığı `trg_*_change` trigger'ları). Trigger
her değişen/silinen RECno'yu bir yardımcı tabloya yazar.

### 4.2 İmleç (Cursor) Mekanizması

`OfflineBilgi` tablosu her Mikro tablosu için iki imleç tutar (`TabloID` başına):

```sql
CREATE TABLE OfflineBilgi (
    TabloID INTEGER PRIMARY KEY,           -- 13=STOKLAR, 16=STOK_HAREKETLERI, 21=SIPARISLER, 31=CARI_HESAPLAR, ...
    SonGuncellemeZamani TEXT,
    UpdateLastTriggerRecNo INTEGER,        -- Son işlenen UPDATE trigger RecNo
    DeleteLastTriggerRecNo INTEGER         -- Son işlenen DELETE trigger RecNo
)
```

- Kaynak: `Fora_Mikro/SyncAdapter/src/SyncAdapter.Infrastructure/Sqlite/SqliteSchemaInitializer.cs:81-86`
- Model: `Fora_Mikro/SyncAdapter/src/SyncAdapter.Core/Models/OfflineBilgi.cs:10-30`
- Repo: `Fora_Mikro/SyncAdapter/src/SyncAdapter.Infrastructure/Sqlite/SqliteOfflineBilgiRepository.cs`

### 4.3 Update Delta Akışı

`GuncellemeServisi.V15_Senkronizasyon_Main()` (satır 278+) her tablo için:

1. `SenkronizeEt_<TABLO>` parametresinin `true` olup olmadığını kontrol eder
   (GuncellemeServisi.cs:362-507 — yaklaşık 30+ tablo için parametre okunur).
2. `OfflineBilgi.GetOrCreateAsync(TabloID)` ile mevcut imleci alır.
3. `GetTabloDegisenKayitlarTopluV2` sunucu endpoint'ine imleç değerini gönderir,
   `BirSeferdekiMaksimumKayitSayisi = 5000`'lik sayfalar halinde delta alır.
4. Gelen satırlar `INSERT OR REPLACE` ile lokal SQLite'a yazılır.
5. İmleç `UpdateImleciAsync(tabloId, updateLastTriggerRecNo: yeniMaxRecNo)` ile
   ilerletilir.

### 4.4 Delete Delta Akışı

`GetSilinenKayitlarTopluV2` sunucu endpoint'i `DeleteLastTriggerRecNo`'dan
büyük RECno'ları döner. Lokal SQLite'tan `DELETE FROM <tablo> WHERE RECno = ?`
ile satırlar silinir, imleç ilerletilir.

### 4.5 Senkronize Edilen Tablolar

`GetMikroV14DefaultTablolar()` ve `GetMikroV14TeknikTesisBakimyonetimiTablolar()`
listelerinde tanımlı tüm tablolar:

| Tablo | Tip | Tipik Android Kullanımı |
|-------|-----|-------------------------|
| `STOKLAR` (TabloID=13) | Master | Ürün listesi, arama, detay |
| `BARKOD_TANIMLARI` | Master | Barkod okutarak ürün bulma |
| `STOK_HAREKETLERI` (16) | Hareket | Envanter, son alış/satış fiyatı |
| `STOK_SATIS_FIYAT_LISTELERI` | Master | Fiyat listesi |
| `SATIS_SARTLARI` | Master | Cari×stok×depo bazlı fiyat |
| `STOK_CARI_ISKONTO_TANIMLARI` | Master | İskonto hesabı |
| `CARI_HESAPLAR` (31) | Master | Müşteri kartı |
| `CARI_HESAP_ADRESLERI` (32) | Master | Adres + GPS |
| `CARI_HESAP_YETKILILERI` (33) | Master | İrtibat kişisi |
| `CARI_HESAP_HAREKETLERI` (51) | Hareket | Ekstre, bakiye, yapılacak tahsilat |
| `ODEME_EMIRLERI` (54) | Hareket | Çek/senet/kredi kartı riskleri |
| `CARI_HESAP_TEMINATLARI` | Master | Teminat riski |
| `SIPARISLER` (21) | Hareket | Açık siparişler, kalan miktar |
| `PROFORMA_SIPARISLER` | Hareket | Proforma |
| `DEPOLAR_ARASI_SIPARISLER` | Hareket | Depo transfer |
| `EVRAK_ACIKLAMALARI` | Hareket | Evrak notları |
| `DEPOLAR` | Lookup | Depo seçimi |
| `KASALAR` | Lookup | Kasa seçimi (tahsilat) |
| `BANKALAR` | Lookup | Banka seçimi (tahsilat) |
| `CARI_PERSONEL_TANIMLARI` | Lookup | Temsilci/plasiyer |
| `SORUMLULUK_MERKEZLERI` | Lookup | Sorumluluk merkezi |
| `PROJELER` | Lookup | Proje kodu |
| `SUBELER` | Lookup | Şube kodu |
| `ODEME_PLANLARI` | Lookup | Ödeme planı |
| `KUR_ISIMLERI` | Lookup | Döviz sembolü |
| `DOVIZ_KURLARI` | Lookup | Döviz kuru |
| `_FORA_PARAMETRELER` | Lookup | Kullanıcı/uygulama ayarları |
| `_ZIYARET_HAREKETLERI` | Hareket | Ziyaret geçmişi kontrol |

`SenkronizeEt_*` parametreleri ile her biri açılıp kapatılabilir
(GuncellemeServisi.cs:362-507).

## 5. Update (Güncelleme) Yönleri

### 5.1 Android → Mikro (Yazma Yönünde Güncelleme)

- Android, daha önce gönderdiği bir kaydı değiştirir (örn. sipariş satırı
  iptali, kalem ekleme).
- Lokal outbox'ta **yeni satır** olarak yazılır: `yeniKayit=false` yapılarak.
- Yeni payload aynı `externalId` (Mikro tarafında `evrakSeri + evrakSira`) ile
  gönderilir. Sunucu idempotency kontrolü yapar:
  - V15: `RECid_RECno` üzerinden mevcut kaydı bulur, günceller.
  - V16: `Guid` üzerinden mevcut kaydı bulur, günceller.
- **Yeni evrak açılmaz** (idempotency).

### 5.2 Mikro → Android (Okuma Yönünde Güncelleme)

- MSSQL `UPDATE` trigger'ı değişen RECno'yu trigger tablosuna yazar.
- Windows servis `GetTabloDegisenKayitlarTopluV2` ile delta çeker.
- `INSERT OR REPLACE` ile lokal SQLite'a yazılır (mevcut satır varsa değiştirir).
- `OfflineBilgi.UpdateLastTriggerRecNo` yeni son değere güncellenir.
- Android, lokal tablodan değişikliği okur.

## 6. Delete (Silme) Yönleri

### 6.1 Android → Mikro: **YOK**

- Outbox akışında delete tipi yoktur.
- Android yanlışlıkla bile Mikro'dan kayıt **silemez**.
- Bu bilinçli bir güvenlik kararıdır (kullanıcı profilinde de var: "ERP'de
  hiçbir veriyi silmeyeceğiz/yazmayacağız").

### 6.2 Mikro → Android

- MSSQL `DELETE` trigger'ı silinen RECno'yu trigger tablosuna yazar.
- Windows servis `GetSilinenKayitlarTopluV2` ile delta çeker.
- Lokal SQLite'tan `DELETE FROM <tablo> WHERE <PK> = ?` çalıştırılır.
- `OfflineBilgi.DeleteLastTriggerRecNo` ilerletilir.
- Android, lokal tablodaki satırın kaybolduğunu görür.

### 6.3 Outbox Kayıtlarının Silinmesi

`IOfflineEvrakRepository.SilAsync(offlineRecNo)` metodu tanımlı olsa da yorum
şunu söyler: "Genelde kullanılmaz, soft-delete tercih edilir." Yani başarılı
push sonrası outbox satırı `Aktarildi` durumuna güncellenir, fiziksel `DELETE`
yapılmaz. İhtiyaç halinde periyodik bakım job'ı ile eski tamamlanmış satırlar
toplu silinebilir (örn. 30 günden eski `Aktarildi` satırları).

## 7. Idempotency

Yazma akışı her zaman idempotent olmalı:

| Tablo | V15 idempotency anahtarı | V16 idempotency anahtarı |
|-------|--------------------------|--------------------------|
| `CARI_HESAPLAR` | `cari_kod` (unique) | `cari_kod` (unique) |
| `STOKLAR` | `sto_kod` (unique) | `sto_kod` (unique) |
| `CARI_HESAP_ADRESLERI` | `(adr_cari_kod, adr_adres_no)` | `(adr_cari_kod, adr_adres_no)` |
| `CARI_HESAP_HAREKETLERI` | `(cha_RECid_DBCno, cha_RECid_RECno)` | `(cha_Guid)` |
| `STOK_HAREKETLERI` | `(sth_RECid_DBCno, sth_RECid_RECno)` | `(sth_Guid)` |
| `SIPARISLER` | `(sip_RECid_DBCno, sip_RECid_RECno)` | `(sip_Guid)` |
| `ODEME_EMIRLERI` | `sck_no` | `sck_Guid` |

Yeni projede `SyncAdapter.LocalStore.Mappings` tablosu ile
`(tenant_id, document_type, external_id) → (mikro_recno, mikro_guid)` mapping
tutulur, böylece aynı `externalId` ile ikinci push geldiğinde yeni evrak
oluşturulmaz, mevcut `recno/guid` üzerinden güncellenir.

## 8. Outbox Mekaniği (OFFLINE_KAYITLAR)

```sql
CREATE TABLE OFFLINE_KAYITLAR (
    OfflineRECno      INTEGER PRIMARY KEY AUTOINCREMENT,
    AktarimDurumu     INTEGER NOT NULL,        -- 1=Beklemede, 2=Aktarilacak, 3=Aktarildi, 4=Hatali
    YeniKayit         INTEGER NOT NULL,        -- 1=yeni insert, 0=update
    AktarilmaTarihi   TEXT    NOT NULL,        -- ISO 8601 UTC
    Tipi              INTEGER NOT NULL,        -- AndroidAktarimTipi
    Evrak             BLOB    NOT NULL,        -- JSON+GZip envelope (yeni projede)
    HataString        TEXT    NULL             -- Sunucu hata mesajı (varsa)
)
CREATE INDEX IDX_OFFLINE_KAYITLAR_Durum_Tarih
    ON OFFLINE_KAYITLAR (AktarimDurumu, AktarilmaTarihi);
```

- Kaynak: `Fora_Mikro/SyncAdapter/src/SyncAdapter.Infrastructure/Sqlite/SqliteSchemaInitializer.cs:67-78`
- Eski binary şema: `Fora_Mikro/.decompiled/Core/Fora.Mikro.Evraklar/OfflineEvrakSqlite.cs:139-153` (`Evrak` kolonu `BLOB` veya eski `Json` TEXT varyantı)

### Push Akışı (Sunucu Tarafı)

`MikroService.SaveOfflineEvrakV3` (MikroService.cs:2596) — tip dispatch sonrası:

1. `AktarimDurumu` `Aktarildi` veya `Hatali` olarak güncellenir.
2. `AktarilmaTarihi` = sunucu zamanı.
3. `HataString` hata varsa dolar (örn. "Gün açılışı daha önce XXX kayıt numarası ile yapılmış!").
4. Outbox satırı **silinmez**, soft-delete.

## 9. Yeni SyncAdapter'da Yapılacaklar

- [ ] `SyncAdapter.Mikro.IdentityMapper` interface'i: V15/V16 için iki implementasyon
      (RECno vs Guid, SCOPE_IDENTITY() vs NEWID()).
- [ ] `SyncAdapter.Mikro.Writers`:
  - `EvrakWriter` (tip 0, 7 alt-tipi dispatch)
  - `CariLokasyonWriter` (tip 1)
  - `GunBaslaBitirWriter` (tip 2 ve 3, ortak)
  - `ZiyaretWriter` (tip 4)
  - `YazBozTahtasiWriter` (tip 5)
  - `YeniCariWriter` (tip 6)
- [ ] Her writer için **transaction** wrapping zorunlu (header + tüm satırlar atomik).
- [ ] `mappings` tablosuna `(tenant_id, document_type, external_id, mikro_recno, mikro_guid)`
      INSERT/UPSERT.
- [ ] Sunucu ack'i: `{ offlineRecNo, durum, hataString }` envelope güncellemesi.
- [ ] Tüm yazma SQL'leri **parametreli** (string concat yok).

## 10. Doğruluk Kanıtı

Bu dokümandaki her satır, aşağıdaki kod referanslarından doğrulanmıştır:

| İddia | Kanıt |
|-------|-------|
| Tip dispatcher switch var | `MikroService.cs:2689-2716` |
| `CariLokasyon` UPDATE sorgusu | `CariData.cs:1259` |
| `Evrak` yazma transaction mantığı | `Evrak.cs:4983-6107` |
| Sipariş karşılama `sip_teslim_miktar` update | `Evrak.cs:6955` |
| Yeni cari otomatik kod atama | `MikroService.cs:5235-5282` |
| Trigger tabanlı delta | `MIKRO_ERP_VERI_AKIS_DOKUMANI.md:62-228` |
| OfflineBilgi şeması | `SqliteSchemaInitializer.cs:81-86` |
| V15/V16 farkı (RECno vs Guid) | `TabloHelper.cs:124-142, 660-676` vs `TabloHelperV16.cs:155-210, 1591+` |
| `mye_TextData` UPSERT (V15 RECid, V16 Record_uid) | `CariSqlite.cs:1594` |
| GunAcilisi idempotency (gün başına tek kayıt) | `MikroService.cs:5054-5072` |

— Üretildi: 2026-09-07
