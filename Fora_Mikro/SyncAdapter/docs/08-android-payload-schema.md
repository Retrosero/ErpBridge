# Android Payload Şeması (SyncAdapter)

> Bu doküman, Android saha uygulamasının `SyncAdapter` projesine göndereceği
> outbox kayıtlarının tam şemasını, her `AndroidAktarimTipi` enum değeri için
> payload formatını, HTTP endpoint'ini ve validasyon kurallarını belgeler.
> Android geliştirici ekibin referans noktasıdır.

## 1. Genel Akış

```
[Android]                                            [SyncAdapter]
  │                                                       │
  │ 1. Kullanıcı aksiyonu (sipariş, GPS, ziyaret)        │
  │ 2. OfflineEvrakV2 envelope oluştur                    │
  │ 3. SQLite OFFLINE_KAYITLAR INSERT (Durum=Beklemede)  │
  │                                                       │
  │ 4. POST /SaveOfflineEvrakV3/{firmaid}/{mikroDbName}   │
  │    Content-Type: application/octet-stream             │
  │    Content-Encoding: gzip                             │
  │    Body: gzip(utf8(JSON(OfflineEvrakV2)))             │
  ├──────────────────────────────────────────────────────►│
  │                                                       │ 5. SQL Server'a yaz
  │                                                       │    (tek transaction)
  │                                                       │
  │ 6. Response: güncellenmiş envelope                    │
  │    { offlineRecNo, durum: Aktarildi|Hatali, hataString }
  │◄──────────────────────────────────────────────────────┤
  │                                                       │
  │ 7. OFFLINE_KAYITLAR UPDATE (Durum=Aktarildi|Hatali)   │
  │                                                       │
```

## 2. HTTP Endpoint

```
POST /SaveOfflineEvrakV3/{firmaid}/{mikroDbName}
```

| Parametre | Konum | Tip | Açıklama |
|-----------|-------|-----|----------|
| `firmaid` | URL path | string | Tenant kimliği (Firmalar tablosundan) |
| `mikroDbName` | URL path | string | Hedef Mikro DB adı (örn. `MikroDB_V15_02`) |

**Request Headers:**

| Header | Değer |
|--------|-------|
| `Content-Type` | `application/octet-stream` (binary GZip) veya `application/json;charset=utf-8` (raw JSON) |
| `Content-Encoding` | `gzip` (sıkıştırılmış gövde için) |

**Request Body:**

Envelope (`OfflineEvrakV2`) — JSON olarak, opsiyonel GZip sıkıştırma.

**Response:**

Envelope'un güncellenmiş hali (sunucu tarafı `Aktarildi` veya `Hatali` yapar):

```json
{
  "offlineRecNo": 1234,
  "durum": 3,
  "yeniKayit": true,
  "aktarilmaTarihi": "2026-09-07T18:00:00.000Z",
  "tipi": 0,
  "evrakJson": "...",
  "hataString": null
}
```

## 3. Envelope: OfflineEvrakV2

```json
{
  "offlineRecNo": 0,                  // PK, sunucuda değişmez. Android 0 gönderir.
  "durum": 1,                         // EvrakAktarimDurumu
                                      //   1=Beklemede, 2=Aktarilacak,
                                      //   3=Aktarildi, 4=Hatali
  "yeniKayit": true,                  // true=insert, false=update
  "aktarilmaTarihi": "2026-09-07T18:00:00.000Z",  // ISO 8601 UTC
  "tipi": 0,                          // AndroidAktarimTipi (aşağıdaki enum)
  "evrakJson": "<type-specific JSON, aşağıdaki şemalardan biri>",
  "hataString": null                  // Sunucu hatası (varsa)
}
```

**C# Tanımı:** `SyncAdapter/src/SyncAdapter.Core/Models/OfflineEvrakV2.cs:12-39`

**Serileştirme:** `SyncAdapter/src/SyncAdapter.Infrastructure/Serialization/OfflineEvrakSerializer.cs`
(JSON + GZip, `JsonStringEnumConverter` aktif, `CamelCase` policy).

## 4. Enum Tipleri

### 4.1 AndroidAktarimTipi

| Değer | Ad | Anlam |
|-------|-----|-------|
| 0 | `Evrak` | Asıl ticari evrak (sipariş, irsaliye, fatura, tahsilat, tediye) |
| 1 | `CariLokasyon` | Müşteri GPS koordinat güncellemesi |
| 2 | `GunAcilisi` | Temsilcinin güne başlaması |
| 3 | `GunKapanisi` | Temsilcinin günü kapatması |
| 4 | `Ziyaret` | Müşteri ziyaret raporu |
| 5 | `YazBozTahtasi` | Cari serbest not (whiteboard) |
| 6 | `YeniCariOlusturma` | Sahada yeni müşteri kartı açma |

### 4.2 EvrakAktarimDurumu

| Değer | Ad | Anlam |
|-------|-----|-------|
| 1 | `Beklemede` | Lokal olarak oluşturuldu, henüz gönderilmedi |
| 2 | `Aktarilacak` | Sync motoru tarafından bir sonraki turda gönderilmek üzere işaretlendi |
| 3 | `Aktarildi` | Sunucu tarafından başarıyla alındı |
| 4 | `Hatali` | Sunucu veya ağ hatası, retryable |

### 4.3 GenelEvrakTipleri (Tip 0 / Evrak için)

| Değer | Ad | Header Tablo | Satır Tabloları |
|-------|-----|--------------|-----------------|
| 1 | `SatisSiparis` | `SIPARISLER` | — |
| 2 | `SatisIrsaliye` | — | `STOK_HAREKETLERI` |
| 3 | `SatisFatura` | `CARI_HESAP_HAREKETLERI` (Borç) | `STOK_HAREKETLERI` |
| 4 | `AlisSiparis` | `SIPARISLER` | — |
| 5 | `AlisIrsaliye` | — | `STOK_HAREKETLERI` |
| 6 | `AlisFatura` | `CARI_HESAP_HAREKETLERI` (Alacak) | `STOK_HAREKETLERI` |
| 7 | `DepolarArasiNakliyeOnaylama` | `DEPOLAR_ARASI_SIPARISLER` | — |
| 8 | `DepolarArasiSevk` | `DEPOLAR_ARASI_SIPARISLER` | `STOK_HAREKETLERI` |
| 9 | `DepolarArasiNakliyeFisi` | `DEPOLAR_ARASI_SIPARISLER` | `STOK_HAREKETLERI` |
| 50 | `Tahsilat` | `CARI_HESAP_HAREKETLERI` (Alacak) | — (opsiyonel `ODEME_EMIRLERI`) |
| 51 | `Tediye` | `CARI_HESAP_HAREKETLERI` (Borç) | — (opsiyonel `ODEME_EMIRLERI`) |

## 5. Tip'e Göre Payload Şemaları

### 5.1 Tip 0 — Evrak

**Endpoint:** `POST /SaveOfflineEvrakV3/{firmaid}/{mikroDbName}` — handler `EvrakKaydetV2`
(MikroService.cs:2704, Evrak.cs:9167 satır).

**`evrakJson` içeriği:**

```json
{
  "evrakTipi": 1,                      // GenelEvrakTipleri (yukarıdaki tablo)
  "evrakTarihi": "2026-09-07T00:00:00",
  "evrakNoSeri": "SIP",                // "SIP", "FAT", "IRS", "THS"
  "evrakNoSira": 0,                    // 0 ise sunucu otomatik atar (YeniSeriNoBul)
  "belgeNo": "M0001234",
  "belgeTarihi": "2026-09-07T00:00:00",
  "cariKodu": "120.01.0001",
  "temsilciKodu": "T01",               // opsiyonel
  "sorumlulukMerkeziKodu": "MRK01",    // opsiyonel
  "projeKodu": "PRJ01",                // opsiyonel
  "subeKodu": "01",                    // opsiyonel
  "dovizCinsi": 0,                     // 0=TL, 1=USD, 2=EUR ...
  "dovizKuru": 1.0,
  "aciklama1": "Açıklama 1",
  "aciklama2": "...",
  "aciklama3": "...",
  "siparisKarsilama": true,            // true ise sipariş kapatma
  "siparisRefRecNo": 0,                // V15 sipariş RECno (V16 için EvrakV16DTO açılacak)

  "satirlar": [                        // STOK_HAREKETLERI satırları
    {
      "satirNo": 1,
      "stokKodu": "STK001",
      "miktar": 10.0,
      "birimFiyat": 25.5,
      "iskontoOran1": 5.0, "iskontoOran2": 0, "iskontoOran3": 0,
      "iskontoOran4": 0,  "iskontoOran5": 0, "iskontoOran6": 0,
      "kdvOrani": 20,
      "girisDepoNo": 1,                 // İrsaliye/Fatura'da
      "cikisDepoNo": 1,
      "aciklama": "1. kalem"
    }
  ],

  "cariHareketler": [                  // Tahsilat/Tediye'de dolu
    {
      "satirNo": 1,
      "cariKodu": "120.01.0001",
      "tutar": 1500.0,
      "dovizCinsi": "TL",
      "vadeTarihi": "2026-09-14T00:00:00",
      "bankaNo": 0,                     // 0=nakit, >0=banka
      "aciklama": "Nakit tahsilat"
    }
  ]
}
```

**C# Tanımı:** `SyncAdapter/src/SyncAdapter.Core/Models/EvrakDTO.cs:12-101`

**Sunucu Yazım Kalıbı:**
- `CARI_HESAP_HAREKETLERI` header INSERT (varsa) + `STOK_HAREKETLERI` her kalem için
  INSERT, **tek transaction**.
- V15'te `SCOPE_IDENTITY()` ile `*_RECid_RECno` geri set, V16'da `NEWID()` ile
  `*_Guid` üretilir.
- Sipariş karşılama ise `SIPARISLER.sip_teslim_miktar` atomik artırılır.
- Beden/renk/seri lot takipli stoklarda `BEDEN_HAREKETLERI`, `STOK_SERINO_TANIMLARI`,
  `CIHAZ_HAREKETLERI` de yazılır (opsiyonel).

### 5.2 Tip 1 — CariLokasyon

**Endpoint:** handler `CariLokasyonGuncelleV2` (MikroService.cs:2691).

**`evrakJson` içeriği:**

```json
{
  "cariKod": "120.01.0001",
  "adresNo": 1,
  "enlem": 41.0082,
  "boylam": 28.9784,
  "tarih": "2026-09-07T10:23:00Z"
}
```

**Sunucu SQL (V15/V16 ortak):**

```sql
UPDATE CARI_HESAP_ADRESLERI
SET adr_gps_enlem = @adr_gps_enlem,
    adr_gps_boylam = @adr_gps_boylam,
    adr_lastup_date = getdate()
WHERE adr_cari_kod = @cari_kod
  AND adr_adres_no = @adr_adres_no
```

**Kaynak:** `CariData.cs:1253-1270` (`SetCariLokasyon`).

### 5.3 Tip 2 — GunAcilisi

**Endpoint:** handler `GunAcilisiYapV2` (MikroService.cs:2694).

**`evrakJson` içeriği:**

```json
{
  "temsilciKodu": "T01",
  "tarih": "2026-09-07T00:00:00",
  "saati": "08:30:00",
  "enlem": 41.0082,
  "boylam": 28.9784,
  "aracKm": 125430,
  "mesaj": "Günaydın, başlıyorum."
}
```

**Sunucu İşlemi:**
- Aynı `Temsilci_Kodu + Tarih` için mevcut kayıt varsa **HATA**:
  `durum = Hatali`, `hataString = "Gün açılışı daha önce XXX kayıt numarası ile yapılmış!"`
- Yoksa `_TEMSILCI_GUNLUK_HAREKETLER` INSERT.
- V16'da `Guid + NEWID()` (MikroService.cs:5077-5084) eklenir.

### 5.4 Tip 3 — GunKapanisi

**Endpoint:** handler `GunKapanisiYapV2` (MikroService.cs:2697).

**`evrakJson` içeriği:**

```json
{
  "temsilciKodu": "T01",
  "tarih": "2026-09-07T00:00:00",
  "saati": "18:45:00",
  "enlem": 41.0082,
  "boylam": 28.9784,
  "aracKm": 125678,
  "mesaj": "Gün tamam."
}
```

**Sunucu İşlemi:**
- Aynı `Temsilci_Kodu + Tarih` için mevcut kayıt bulunur (V15 `Recno` veya V16 `Guid`).
- `_TEMSILCI_GUNLUK_HAREKETLER` UPDATE:
  ```sql
  UPDATE _TEMSILCI_GUNLUK_HAREKETLER
  SET Bitis_Yapildi = 1,
      Bitis_Saati = @Bitis_Saati,
      Bitis_Kayit_Saati = @Bitis_Kayit_Saati,
      Bitis_Enlem = @Bitis_Enlem,
      Bitis_Boylam = @Bitis_Boylam,
      Bitis_Arac_Km = @Bitis_Arac_Km,
      Bitis_Mesaj = @Bitis_Mesaj,
      Lastup_Date = getdate()
  WHERE Recno = @Recno      -- V15
        -- Guid = @Guid    -- V16
  ```
- Kayıt yoksa HATA: "Gün açılışı yapılmamış!".

### 5.5 Tip 4 — Ziyaret

**Endpoint:** handler `ZiyaretKaydetV2` (MikroService.cs:2700).

**`evrakJson` içeriği:**

```json
{
  "zyrt_iptal": false,
  "zyrt_Tarihi": "2026-09-07T00:00:00",
  "zyrt_Temsilci_Kodu": "T01",
  "zyrt_Bolge_Kodu": "BLG01",
  "zyrt_Cari_Kodu": "120.01.0001",
  "zyrt_Cari_Adres_No": 1,
  "zyrt_Cari_Adres_Enlem": 41.0082,
  "zyrt_Cari_Adres_Boylam": 28.9784,
  "zyrt_Baslama_Saati": "10:30:00",
  "zyrt_Baslama_Enlem": 41.0082,
  "zyrt_Baslama_Boylam": 28.9784,
  "zyrt_Bitis_Saati": "11:15:00",
  "zyrt_Bitis_Enlem": 41.0090,
  "zyrt_Bitis_Boylam": 28.9790,
  "zyrt_Tamamlandi": true,
  "zyrt_Fotograf_Id": 0,
  "zyrt_Proje_Kodu": "PRJ01",
  "zyrt_Sor_Mer_Kodu": "MRK01",
  "zyrt_Bakim_Evrak_Seri": null,
  "zyrt_Bakim_Evrak_Sira": 0,
  "zyrt_Satis_Yapildi": true,
  "zyrt_Siparis_Alindi": true,
  "zyrt_Urun_Teslim_Edildi": false,
  "zyrt_Tahsilat_Yapildi": true,
  "zyrt_Katalog_Birakildi": false,
  "zyrt_Fiyat_Listesi_Birakildi": true,
  "zyrt_Numune_Urun_Birakildi": false,
  "zyrt_Konsinye_Urun_Birakildi": false,
  "zyrt_Promosyon_Birakildi": false,
  "zyrt_Firma_Durumu": 0,
  "zyrt_Rakip_Firma_Var": false,
  "zyrt_Rakip_Firma_Durumu": 0,
  "zyrt_Urun_Yerlesim_Durumu": 0,
  "zyrt_Rakip_Urun_Yerlesim_Durumu": 0,
  "zyrt_Temsilci_Aciklama": "Rafa yeni ürün konulmuş.",
  "zyrt_Cari_Firma_Memnuniyeti": 4,
  "zyrt_Cari_Urun_Memnuniyeti": 5,
  "zyrt_Cari_Fiyat_Memnuniyeti": 3,
  "zyrt_Cari_Temsilci_Memnuniyeti": 5,
  "zyrt_Cari_Aciklama": "Fiyattan memnun değil.",

  "zyrt_Temsilci_Ozel_01_Var_Yok": true,
  "zyrt_Temsilci_Ozel_01_Evet_Hayir": true,
  "zyrt_Temsilci_Ozel_01_Derece": 3,
  "zyrt_Temsilci_Ozel_01_TamSayi": 0,
  "zyrt_Temsilci_Ozel_01_OndalikliSayi": 0.0,
  "zyrt_Temsilci_Ozel_01_Metin": "Rafa eklenen ürünler",
  "zyrt_Temsilci_Ozel_01_Fotograf_Id": 0,

  "zyrt_Temsilci_Ozel_02_Var_Yok": false,
  "zyrt_Temsilci_Ozel_02_Evet_Hayir": false,
  "zyrt_Temsilci_Ozel_02_Derece": 0,
  "zyrt_Temsilci_Ozel_02_TamSayi": 0,
  "zyrt_Temsilci_Ozel_02_OndalikliSayi": 0.0,
  "zyrt_Temsilci_Ozel_02_Metin": "",
  "zyrt_Temsilci_Ozel_02_Fotograf_Id": 0,

  "zyrt_Temsilci_Ozel_03_Var_Yok": false,
  "zyrt_Temsilci_Ozel_03_Evet_Hayir": false,
  "zyrt_Temsilci_Ozel_03_Derece": 0,
  "zyrt_Temsilci_Ozel_03_TamSayi": 0,
  "zyrt_Temsilci_Ozel_03_OndalikliSayi": 0.0,
  "zyrt_Temsilci_Ozel_03_Metin": "",
  "zyrt_Temsilci_Ozel_03_Fotograf_Id": 0,

  "zyrt_Temsilci_Ozel_04_Var_Yok": false,
  "zyrt_Temsilci_Ozel_04_Evet_Hayir": false,
  "zyrt_Temsilci_Ozel_04_Derece": 0,
  "zyrt_Temsilci_Ozel_04_TamSayi": 0,
  "zyrt_Temsilci_Ozel_04_OndalikliSayi": 0.0,
  "zyrt_Temsilci_Ozel_04_Metin": "",
  "zyrt_Temsilci_Ozel_04_Fotograf_Id": 0,

  "zyrt_Temsilci_Ozel_05_Var_Yok": false,
  "zyrt_Temsilci_Ozel_05_Evet_Hayir": false,
  "zyrt_Temsilci_Ozel_05_Derece": 0,
  "zyrt_Temsilci_Ozel_05_TamSayi": 0,
  "zyrt_Temsilci_Ozel_05_OndalikliSayi": 0.0,
  "zyrt_Temsilci_Ozel_05_Metin": "",
  "zyrt_Temsilci_Ozel_05_Fotograf_Id": 0
}
```

**Sunucu İşlemi:**
- `_ZIYARET_HAREKETLERI` INSERT, 70+ kolon.
- V16'da `zyrt_Guid, NEWID()` (MikroService.cs:5336-5337) eklenir.

### 5.6 Tip 5 — YazBozTahtasi

**Endpoint:** handler `CariYazBozGuncelle` (MikroService.cs:2706).

**`evrakJson` içeriği:**

```json
{
  "cariKod": "120.01.0001",
  "data": "Müşteri ödemeyi 15'inde yapacak. İrsaliyeyi fabrikaya gönderin."
}
```

**Sunucu İşlemi:**
- `mye_TextData` UPSERT, `TableID = 31`.
- V15'te `RecID_DBCno` ve `RecID_RECno` `CARI_HESAPLAR.cari_RECid_*`'dan çekilir.
- V16'da `Record_uid` `CARI_HESAPLAR.cari_Guid`'inden çekilir.

**Kaynak:** `CariSqlite.cs:1594` — okuma için kullanılan SQL pattern.

### 5.7 Tip 6 — YeniCariOlusturma

**Endpoint:** handler `YeniCariOlustur` (MikroService.cs:2709).

**`evrakJson` içeriği:**

```json
{
  "cari_kod_prefix": "120.01.",
  "cari_kod": "",                      // boş ise sunucu otomatik atar

  "cari_unvan1": "YENİ MÜŞTERİ A.Ş.",
  "cari_unvan2": "",
  "cari_vdaire_adi": "BEYOĞLU",
  "cari_vdaire_no": "1234567890",
  "cari_VergiKimlikNo": "12345678901",
  "cari_sicil_no": "12345",
  "cari_bolge_kodu": "BLG01",
  "cari_grup_kodu": "GR01",
  "cari_temsilci_kodu": "T01",
  "cari_odemeplan_no": 1,
  "cari_satis_fk": 1,
  "cari_doviz_cinsi": 0,
  "cari_VarsayilanGirisDepo": 1,
  "cari_VarsayilanCikisDepo": 1,
  "cari_CepTel": "05321234567",
  "cari_EMail": "info@yenimusteri.com",

  "cariCariAdresleri": [
    {
      "adr_adres_no": 1,
      "adr_cadde": "Atatürk Cad.",
      "adr_sokak": "No: 5",
      "adr_posta_kodu": "34000",
      "adr_ilce": "Beyoğlu",
      "adr_il": "İstanbul",
      "adr_ulke": "Türkiye",
      "adr_tel_ulke_kodu": "90",
      "adr_tel_bolge_kodu": "212",
      "adr_tel_no1": "5551234567",
      "adr_temsilci_kodu": "T01",
      "adr_yon_kodu": "K",
      "adr_uzaklik_kodu": 1,
      "adr_ziyaretperyodu": 7,
      "adr_ziyaretgunu": "1",
      "adr_gps_enlem": 41.0082,
      "adr_gps_boylam": 28.9784
    }
  ],

  "cariYetkilileri": [
    {
      "mye_isim": "Ahmet",
      "mye_soyisim": "Yılmaz",
      "mye_email_adres": "ahmet@yenimusteri.com",
      "mye_cep_telno": "05321234567",
      "mye_tc_kimlikno": "12345678901"
    }
  ]
}
```

**Sunucu İşlemi (MikroService.cs:5223-5324):**
1. `cari_kod` boşsa otomatik atanır: `cari_kod_prefix + (MAX(sayısal parça) + 1)`.
2. Tek transaction içinde:
   - `CARI_HESAPLAR` INSERT
   - `CARI_HESAP_ADRESLERI` INSERT (her bir adres)
   - `CARI_HESAP_YETKILILERI` INSERT (her bir yetkili)
   - `mye_TextData` INSERT (TableID=31, opsiyonel)
3. V15'te `*_RECid_RECno` SCOPE_IDENTITY() ile geri set.
4. V16'da `*_Guid` insert anında NEWID() ile üretilir.

## 6. Validasyon Kuralları

### 6.1 Zorunlu Alanlar (Tüm Tipler)

| Alan | Zorunlu | Açıklama |
|------|---------|----------|
| `offlineRecNo` | Evet | PK, Android 0 gönderir |
| `durum` | Evet | `1=Beklemede` ile başlar |
| `yeniKayit` | Evet | true/false |
| `aktarilmaTarihi` | Evet | ISO 8601 UTC |
| `tipi` | Evet | AndroidAktarimTipi enum |
| `evrakJson` | Evet | Tip'e göre payload (yukarıdaki şemalardan biri) |

### 6.2 Tip Bazlı Zorunlu Alanlar

| Tip | Ek Zorunlu Alanlar |
|-----|---------------------|
| 0 (Evrak) | `evrakTipi, evrakTarihi, cariKodu, satirlar[]` (en az 1 kalem) |
| 0 Tahsilat/Tediye | `cariHareketler[]` (en az 1 hareket) |
| 1 (CariLokasyon) | `cariKod, adresNo, enlem, boylam, tarih` |
| 2 (GunAcilisi) | `temsilciKodu, tarih, saati, enlem, boylam` |
| 3 (GunKapanisi) | `temsilciKodu, tarih, saati, enlem, boylam, aracKm` |
| 4 (Ziyaret) | `zyrt_Tarihi, zyrt_Temsilci_Kodu, zyrt_Cari_Kodu, zyrt_Cari_Adres_No` |
| 5 (YazBozTahtasi) | `cariKod, data` |
| 6 (YeniCariOlusturma) | `cari_unvan1, cari_kod_prefix` (en az 1 adres) |

### 6.3 Sunucu Tarafı Validasyon

Sunucu payload'u kabul etmeden önce:
- `cari_kod` mevcut mu? (CARI_HESAPLAR SELECT)
- `stok_kod` mevcut mu? (STOKLAR SELECT, her kalem için)
- `depo_no, kasa_no, banka_no, odeme_plani_no, doviz_cinsi, temsilci_kodu, proje_kodu, sorumluluk_merkezi_kodu` mevcut mu?
- Varsa FK'lar (sipariş RECno, fatura RECno) gerçekten mevcut mu?

Hata durumunda:
- `durum = Hatali (4)`
- `hataString = "Cari kodu bulunamadı: 120.01.0001"` gibi açıklayıcı mesaj
- HTTP 200 dönülür (envelope hata bilgisi içerir), 4xx/5xx **kullanılmaz**
  (Android retry'ı boşa çıkmasın diye).

## 7. Pull (Okuma) Endpoint'leri

Android, lokal tabloları okumak için direkt MSSQL'e bağlanmaz. Windows servis
arka planda senkronize eder. Android'in kullandığı ortak endpoint'ler:

### 7.1 `GetTabloDegisenKayitlarTopluV2`

```
POST /GetTabloDegisenKayitlarTopluV2/{firmaid}/{tabloId}/{sonRecNo}/{limit}
```

Cevap: `[{RECno, kolonlar...}, ...]` (limit'e kadar, RECno > sonRecNo).

### 7.2 `GetSilinenKayitlarTopluV2`

```
POST /GetSilinenKayitlarTopluV2/{firmaid}/{tabloId}/{sonRecNo}/{limit}
```

Cevap: `[RECno, ...]` (limit'e kadar, RECno > sonRecNo).

### 7.3 Bootstrap (Tam Veri Çekme)

İlk kurulumda veya `OfflineBilgi.UpdateLastTriggerRecNo = 0` durumunda servis
`sonRecNo = 0` ile çağrı yapar, tüm tabloyu çeker.

## 8. Hata Kodları (HataString Örnekleri)

| Senaryo | hataString |
|---------|------------|
| Cari kodu bulunamadı | `Cari kodu bulunamadı: 120.01.0001` |
| Stok kodu bulunamadı | `Stok kodu bulunamadı: STK001` |
| Depo numarası geçersiz | `Geçersiz depo numarası: 99` |
| Gün açılışı zaten yapılmış | `Gün açılışı daha önce 123 kayıt numarası ile yapılmış!` |
| Gün açılışı yapılmamış | `Gün açılışı yapılmamış!` |
| Lookup referansı bozuk | `Ödeme planı bulunamadı: 5` |
| SQL bağlantı hatası | `SQL Bağlantı Hatası` |
| Cari kodu belirlenemedi | `Hata : Yeni cari kodu belirlenemedi` |
| Genel yazma hatası | `işlem gerçekleştirilemedi!` |
| Yetkisiz firma | `Lisans süresi dolmuş` (401 benzeri, 200 ile döner) |

## 9. Güvenlik

- **SQL Injection:** Tüm payload alanları parametreli SQL ile yazılır.
  String concat **kesinlikle yasak**.
- **Authentication:** `firmaid` URL'de; ayrıca `Authorization` header'ında
  tenant API key (RSA imzalı token) beklenir.
- **Yetki kontrolü:** `MikroUserNo` (lisanslı kullanıcı numarası) ile
  `_create_user / _lastup_user` alanları set edilir.
- **Lisans:** Payload gönderilmeden önce lisans bitiş tarihi kontrolü
  (`MikroService.cs:165-232`).

## 10. Örnek Tam Senaryo — Satış Siparişi

### Android Tarafı (Kotlin/JSON)

```kotlin
val envelope = OfflineEvrakV2(
    offlineRecNo = 0,
    durum = 1,           // Beklemede
    yeniKayit = true,
    aktarilmaTarihi = Instant.now().toString(),
    tipi = 0,            // Evrak
    evrakJson = """
        {
          "evrakTipi": 1,           // SatisSiparis
          "evrakTarihi": "2026-09-07T00:00:00",
          "evrakNoSeri": "SIP",
          "evrakNoSira": 0,         // Sunucu atayacak
          "cariKodu": "120.01.0001",
          "temsilciKodu": "T01",
          "dovizCinsi": 0,
          "dovizKuru": 1.0,
          "satirlar": [
            {
              "satirNo": 1,
              "stokKodu": "STK001",
              "miktar": 10.0,
              "birimFiyat": 25.5,
              "iskontoOran1": 5.0,
              "kdvOrani": 20,
              "cikisDepoNo": 1
            },
            {
              "satirNo": 2,
              "stokKodu": "STK002",
              "miktar": 5.0,
              "birimFiyat": 100.0,
              "kdvOrani": 20,
              "cikisDepoNo": 1
            }
          ]
        }
    """.trimIndent()
)

// 1. Lokal SQLite'a INSERT
db.execSQL("""
    INSERT INTO OFFLINE_KAYITLAR
    (AktarimDurumu, YeniKayit, AktarilmaTarihi, Tipi, Evrak, HataString)
    VALUES (1, 1, ?, 0, ?, NULL)
""", arrayOf(envelope.aktarilmaTarihi, gzip(envelope.toJson())))

// 2. HTTP POST (arkaplanda)
val response = httpClient.post(
    url = "https://server.example.com/SaveOfflineEvrakV3/F001/MikroDB_V15_02",
    body = gzip(envelope.toJson())
)

// 3. Response envelope güncelleme
val responseEnvelope = OfflineEvrakV2.fromJson(ungzip(response.body))
db.execSQL("""
    UPDATE OFFLINE_KAYITLAR
    SET AktarimDurumu = ?, AktarilmaTarihi = ?, HataString = ?
    WHERE OfflineRECno = ?
""", arrayOf(
    responseEnvelope.durum,
    responseEnvelope.aktarilmaTarihi,
    responseEnvelope.hataString,
    responseEnvelope.offlineRecNo
))
```

### Sunucu Tarafı (C#)

```csharp
[OperationContract]
public Stream SaveOfflineEvrakV3(string firmaid, string MikroDbName, Stream Content)
{
    // 1. Envelope deserialize
    var env = OfflineEvrakV2.ReadFromStream(Content);

    // 2. SQL Server connection
    var sql = new SqlDB();
    sql.ConnectionOpen(_baglantiBilgileri, MikroDbName);

    // 3. Tip dispatch
    env = env.Tipi switch
    {
        AndroidAktarimTipi.Evrak            => EvrakKaydetV2(env, _baglantiBilgileri, MikroDbName, _userNo),
        AndroidAktarimTipi.CariLokasyon     => CariLokasyonGuncelleV2(env, sql.Connection),
        AndroidAktarimTipi.GunAcilisi       => GunAcilisiYapV2(env, sql.Connection),
        AndroidAktarimTipi.GunKapanisi      => GunKapanisiYapV2(env, sql.Connection),
        AndroidAktarimTipi.Ziyaret          => ZiyaretKaydetV2(env, sql.Connection, MikroDbName),
        AndroidAktarimTipi.YazBozTahtasi    => CariYazBozGuncelle(env, sql.Connection),
        AndroidAktarimTipi.YeniCariOlusturma => YeniCariOlustur(env, sql.Connection, MikroDbName, _userNo),
        _ => throw new InvalidOperationException($"Bilinmeyen Tipi: {env.Tipi}")
    };

    // 4. Response: güncellenmiş envelope (JSON)
    var json = JsonSerializer.Serialize(env, _opts);
    var bytes = Encoding.UTF8.GetBytes(json);
    var response = WebOperationContext.Current.CreateTextResponse(
        Encoding.UTF8.GetString(bytes),
        "application/json;charset=utf-8",
        Encoding.UTF8);
    return new MemoryStream(bytes);
}
```

## 11. Doğruluk Kanıtı

| İddia | Kanıt |
|-------|-------|
| Endpoint URL | `MikroService.cs:2596` (`SaveOfflineEvrakV3`) |
| Tip dispatcher switch | `MikroService.cs:2689-2716` |
| Evrak sub-tip mapping (1-9, 50, 51) | `Evrak.cs:4983-6107` + `EvrakDTO.cs:59-72` |
| `CariLokasyon` SQL | `CariData.cs:1259` |
| `GunAcilisi` idempotency | `MikroService.cs:5054-5072` |
| `GunKapanisi` update | `MikroService.cs:5119-5189` |
| `Ziyaret` insert kalıbı | `MikroService.cs:5326-5341` |
| `YazBozTahtasi` mye_TextData | `MikroService.cs:5192-5221` + `CariSqlite.cs:1594` |
| `YeniCariOlustur` otomatik kod atama | `MikroService.cs:5235-5282` |
| V16 `Guid + NEWID()` ekleme | `MikroService.cs:5077-5084` |
| Yeni projedeki envelope tanımı | `OfflineEvrakV2.cs:12-39` |
| Yeni projedeki JSON+GZip serileştirme | `OfflineEvrakSerializer.cs` |

— Üretildi: 2026-09-07
