# Mikro V15 vs V16 Farkı — STOK_HAREKETLERI ve CARI_HESAPLAR

> Bu doküman, `SyncAdapter.Mikro.IdentityMapper` interface'i ve V15/V16 writer
> implementasyonları yazılırken referans alınacak şema farklarını belgeler.
> Amaç: Core / UI / Service katmanının V15/V16 ayrımını bilmemesini sağlamak
> için farkları tek bir yerde toplamak.

## 1. Genel Felsefe

| Versiyon | Primary Key | Yeni Kayıt Kimliği | Cross-table Bağlantı | Index Tipi |
|----------|------------|-------------------|----------------------|------------|
| **V15** | `RECno` (INT, IDENTITY) | `SCOPE_IDENTITY()` | `*_RECid_DBCno + *_RECid_RECno` (INT, INT) | Clustered/nonclustered INT |
| **V16** | `Guid` (UNIQUEIDENTIFIER) | `NEWID()` | `*_uid` veya `*_Guid` (UNIQUEIDENTIFIER) | Unique on Guid |

Kod seviyesinde bu fark `GenelUtility.GetMikroVersiyon(connection.Database)` ile
tespit edilir. DB adı `MikroDB_V15*` ise 15, `MikroDB_V16*` ise 16+ döner
(ör. `MikroService.cs:5048, 5057, 5131, 5159`).

**Kod kalıbı (her insert/update'de tekrarlanır):**

```csharp
string idCol = (mikroVersiyon <= 15) ? "Recno" : "Guid";

string extraCols = (mikroVersiyon > 15) ? "Guid," : "";
string extraVals = (mikroVersiyon > 15) ? "NEWID()," : "";
```

Veya V15'te **BEGIN/END** ile kimlik güncellemesi tek statement'ta:

```sql
BEGIN
  INSERT INTO CARI_HESAPLAR(..., cari_RECid_RECno)
  VALUES(..., 0)
  UPDATE CARI_HESAPLAR
  SET cari_RECid_RECno = (SELECT SCOPE_IDENTITY())
  WHERE cari_RECno = (SELECT SCOPE_IDENTITY())
END
```

V16'da bu gerekmez; `NEWID()` insert sırasında PK'yı üretir.

## 2. CARI_HESAPLAR (TabloID = 31)

### 2.1 Şema Farkları

| Konsept | V15 kolonları | V16 kolonları | Notlar |
|---------|---------------|---------------|--------|
| Primary key | `cari_RECno` (INT, IDENTITY) | `cari_Guid` (UNIQUEIDENTIFIER) | Lokal SQLite unique index bu kolona kurulur |
| DB internal id | `cari_RECid_DBCno` (INT), `cari_RECid_RECno` (INT) | `cari_DBCno` (INT) | V15'te `mye_TextData` bağlantısı bu iki kolonla yapılır |
| Identifier (kullanıcı) | `cari_kod` (TEXT) | `cari_kod` (TEXT) | **İş kuralı anahtarı** — her iki versiyonda aynı, mapping'de referans |
| e-Fatura opsiyonel | (yok) | `cari_efatura_fl`, `cari_def_efatura_cinsi`, `cari_wwwadresi` | V15'te `EFaturaAktif` parametresiyle runtime'da eklenebilir |
| Index (lokal SQLite) | `cari_RECno` (unique) + `cari_kod` (idx) | `cari_Guid` (unique) + `cari_kod` (idx) | `TabloHelper.cs:129-135` vs `TabloHelperV16.cs:160` |

**V15 field listesi (TabloHelper.cs:124-142):**
- `cari_RECno` (PK, RecNoMu=true)
- `cari_kod` (zorunlu, unique business key)
- `cari_unvan1, cari_unvan2, cari_muh_kod, cari_muh_kod1, cari_muh_kod2, cari_vdaire_adi, cari_vdaire_no, cari_Ana_cari_kodu, cari_bolge_kodu, cari_grup_kodu, cari_temsilci_kodu, cari_sektor_kodu, cari_satis_isk_kod, cari_special1..3, cari_sicil_no, cari_VergiKimlikNo, cari_banka_hesapno1, cari_CepTel, cari_EMail`
- `cari_vade_fark_yuz, cari_vade_fark_yuz1, cari_vade_fark_yuz2` (DOUBLE)
- `cari_doviz_cinsi, cari_doviz_cinsi1, cari_doviz_cinsi2, cari_odeme_gunu, cari_hareket_tipi, cari_odemeplan_no, cari_satis_fk, cari_KurHesapSekli, cari_odeme_cinsi, cari_cari_kilitli_flg, cari_tipi, cari_fatura_adres_no, cari_sevk_adres_no, cari_VarsayilanGirisDepo, cari_VarsayilanCikisDepo, cari_RECid_DBCno, cari_RECid_RECno`

**V16 field listesi (TabloHelperV16.cs:155-210):**
- `cari_Guid` (PK, BLOB = UNIQUEIDENTIFIER)
- `cari_kod, cari_unvan1, cari_unvan2, cari_muh_kod, cari_muh_kod1, cari_muh_kod2, cari_vdaire_adi, cari_vdaire_no, cari_Ana_cari_kodu, cari_bolge_kodu, cari_grup_kodu, cari_temsilci_kodu, cari_sektor_kodu, cari_satis_isk_kod, cari_special1..3, cari_sicil_no, cari_VergiKimlikNo, cari_banka_hesapno1, cari_CepTel, cari_EMail, cari_wwwadresi`
- `cari_vade_fark_yuz, cari_vade_fark_yuz1, cari_vade_fark_yuz2` (REAL)
- `cari_doviz_cinsi, cari_doviz_cinsi1, cari_doviz_cinsi2, cari_odeme_gunu, cari_hareket_tipi, cari_odemeplan_no, cari_satis_fk, cari_KurHesapSekli, cari_odeme_cinsi, cari_cari_kilitli_flg, cari_tipi, cari_fatura_adres_no, cari_sevk_adres_no, cari_VarsayilanGirisDepo, cari_VarsayilanCikisDepo, cari_DBCno, cari_efatura_fl, cari_def_efatura_cinsi`

### 2.2 Insert Kalıbı

**V15 (CariExtensions.cs:138):**
```sql
BEGIN
  INSERT INTO CARI_HESAP_ADRESLERI(
    adr_RECid_DBCno, adr_RECid_RECno, adr_SpecRECno, adr_iptal, adr_fileid,
    adr_hidden, adr_kilitli, adr_degisti, adr_checksum, adr_create_user,
    adr_create_date, adr_lastup_user, adr_lastup_date, ... adr_cari_kod, ...
  ) VALUES(@adr_RECid_DBCno, @adr_RECid_RECno, ...)
  UPDATE CARI_HESAP_ADRESLERI
  SET adr_RECid_RECno = (SELECT SCOPE_IDENTITY())
  WHERE adr_RECno = (SELECT SCOPE_IDENTITY())
END
```

**V16 (CariExtensions.cs:377):**
```sql
BEGIN
  INSERT INTO CARI_HESAP_ADRESLERI(
    adr_Guid, adr_DBCno, adr_SpecRECno, adr_iptal, adr_fileid,
    adr_hidden, adr_kilitli, adr_degisti, adr_checksum, adr_create_user,
    adr_create_date, ... adr_cari_kod, ..., adr_mahalle, adr_Semt,
    adr_Apt_No, adr_Daire_No, adr_Adres_kodu, adr_efatura_alias, adr_eirsaliye_alias
  ) VALUES(NEWID(), @adr_DBCno, ..., '', '', '', '', '', '')
END
```

V16 ek kolonlar: `adr_mahalle, adr_Semt, adr_Apt_No, adr_Daire_No, adr_Adres_kodu, adr_efatura_alias, adr_eirsaliye_alias`.

### 2.3 Update Kalıbı (CariLokasyon)

**V15 ve V16 ortak** (kolon adları aynı, `CariData.cs:1259`):

```sql
UPDATE CARI_HESAP_ADRESLERI
SET adr_gps_enlem = @adr_gps_enlem,
    adr_gps_boylam = @adr_gps_boylam,
    adr_lastup_date = getdate()
WHERE adr_cari_kod = @cari_kod
  AND adr_adres_no = @adr_adres_no
```

### 2.4 Idempotency Anahtarı

| Versiyon | Upsert WHERE koşulu | Kaynak |
|----------|---------------------|--------|
| V15 | `WHERE cari_RECid_DBCno = (SELECT cari_RECid_DBCno FROM CARI_HESAPLAR WHERE cari_kod = ?)` + `cari_RECid_RECno` | CariExtensions.cs:138 (insert kalıbı) |
| V16 | `WHERE cari_Guid = (SELECT cari_Guid FROM CARI_HESAPLAR WHERE cari_kod = ?)` | CariExtensions.cs:377 (insert kalıbı) |

## 3. STOK_HAREKETLERI (TabloID = 16)

### 3.1 Şema Farkları

| Konsept | V15 kolonları | V16 kolonları |
|---------|---------------|---------------|
| Primary key | `sth_RECno` (INT) | `sth_Guid` (UNIQUEIDENTIFIER) |
| Sipariş FK (üst evrak) | `sth_sip_recid_dbcno` (INT) + `sth_sip_recid_recno` (INT) | `sth_sip_uid` (UNIQUEIDENTIFIER) |
| Fatura FK (üst evrak) | `sth_fat_recid_dbcno` (INT) + `sth_fat_recid_recno` (INT) | `sth_fat_uid` (UNIQUEIDENTIFIER) |
| DB internal id | `sth_RECid_DBCno` (INT) | `sth_DBCno` (INT) |
| Beden/renk master FK | `BdnHar_RECid_RECno` (INT) | `BdnHar_Guid` (UNIQUEIDENTIFIER) |
| Index (lokal SQLite) | `sth_RECno` (unique) + `sth_tarih, sth_stok_kod` (idx) | `sth_Guid` (unique) + aynı idx'ler |

**V15 field listesi (TabloHelper.cs:660-676):**
- `sth_RECno` (PK, RecNoMu=true)
- TEXT: `sth_cari_kodu, sth_stok_kod, sth_evrakno_seri, sth_plasiyer_kodu, sth_cari_srm_merkezi, sth_proje_kodu, sth_aciklama`
- DOUBLE: `sth_tutar, sth_alt_doviz_kuru, sth_vergi, sth_har_doviz_kuru, sth_iskonto1..6, sth_masraf1..4, sth_masraf_vergi, sth_miktar, sth_miktar2, sth_stok_doviz_kuru`
- INTEGER: `sth_cari_grup_no, sth_firmano, sth_subeno, sth_normal_iade, sth_birim_pntr, sth_fiyat_liste_no, sth_adres_no, sth_tip, sth_giris_depo_no, sth_cikis_depo_no, sth_evrakno_sira, sth_cari_cinsi, sth_evraktip, sth_satirno, sth_fat_recid_dbcno, sth_fat_recid_recno, sth_sip_recid_dbcno, sth_sip_recid_recno, sth_cins, sth_vergi_pntr, sth_har_doviz_cinsi, sth_stok_doviz_cinsi, sth_nakliyedeposu, sth_nakliyedurumu, sth_isk_mas1..6`
- TEXT (datetime): `sth_tarih, sth_belge_tarih, sth_lastup_date`

**V16 field listesi (TabloHelperV16.cs:1591+):**
- `sth_Guid` (PK, BLOB)
- `sth_sip_uid` (BLOB), `sth_fat_uid` (BLOB) — FK bağlantıları için tek Guid kolonu
- TEXT: `sth_cari_kodu, sth_stok_kod, sth_evrakno_seri, sth_plasiyer_kodu, sth_cari_srm_merkezi, sth_proje_kodu, sth_aciklama`
- REAL: `sth_tutar, sth_alt_doviz_kuru, sth_vergi, sth_har_doviz_kuru, sth_iskonto1..6, sth_masraf1..4, sth_masraf_vergi, sth_miktar, sth_miktar2, sth_stok_doviz_kuru`
- INTEGER: `sth_DBCno, sth_cari_grup_no, sth_firmano, sth_subeno, sth_normal_iade, sth_birim_pntr, sth_fiyat_liste_no, sth_adres_no, sth_tip, sth_giris_depo_no, sth_cikis_depo_no, sth_evrakno_sira, sth_cari_cinsi, sth_evraktip, sth_satirno, sth_cins, sth_vergi_pntr, sth_har_doviz_cinsi, sth_stok_doviz_cinsi, sth_nakliyedeposu, sth_nakliyedurumu, sth_isk_mas1..6`

### 3.2 Insert Kalıbı

**V15 (Evrak.cs:6105):**
```csharp
STOK_HAREKETLERI sth = new STOK_HAREKETLERI();
sth.sth_RECid_DBCno = _DBCno;
sth.sth_create_user = mikrouserno;
sth.sth_lastup_user = mikrouserno;
sth.sth_firmano = Firma.fir_sirano;
// ... diğer alanlar
// SQL: INSERT INTO STOK_HAREKETLERI(..., sth_RECid_RECno) VALUES(..., 0)
//      UPDATE STOK_HAREKETLERI SET sth_RECid_RECno = SCOPE_IDENTITY()
//        WHERE sth_RECno = SCOPE_IDENTITY()
```

**V16 (CariExtensions.cs:377 kalıbı + Evrak.cs:6631):**
```csharp
STOK_HAREKETLERI sth = new STOK_HAREKETLERI();
sth.sth_DBCno = _DBCno;          // V15'te sth_RECid_DBCno
// sth_Guid SQL tarafında NEWID() ile üretilir
// ... diğer alanlar
// SQL: INSERT INTO STOK_HAREKETLERI(..., sth_Guid) VALUES(..., NEWID())
```

### 3.3 Üst Evrak Bağlantısı (Sipariş → İrsaliye → Fatura)

| Akış | V15 kolonları | V16 kolonları |
|------|---------------|---------------|
| Sipariş karşılama (irsaliye/fatura siparişten karşılanıyorsa) | `sth_sip_recid_dbcno = sip_RECid_DBCno`, `sth_sip_recid_recno = sip_RECid_RECno` | `sth_sip_uid = sip_Guid` |
| Faturadan irsaliye bağlantısı | `sth_fat_recid_dbcno = cha_RECid_DBCno`, `sth_fat_recid_recno = cha_RECid_RECno` | `sth_fat_uid = cha_Guid` |
| Beden/renk master bağlantısı | `BdnHar_RECid_RECno` | `BdnHar_Guid` (Evrak.cs:6459) |

### 3.4 Update Kalıbı (Sipariş Karşılama)

Sipariş karşılama sırasında `SIPARISLER.sip_teslim_miktar` artırılır.
Bu update V15/V16 ortaktır (kolon isimleri aynı):

```sql
UPDATE SIPARISLER
SET sip_teslim_miktar = sip_teslim_miktar + @teslim_miktar
WHERE sip_RECno = @sip_RECno          -- V15
  -- V16: WHERE sip_Guid = @sip_Guid
```

## 4. Adapter'ın Karar Verme Kodu

### 4.1 `mikroVersiyon` Tespiti

`GenelUtility.GetMikroVersiyon(connection.Database)` — DB adı `MikroDB_V15*` ise
`<= 15`, `MikroDB_V16*` ise `> 15` döner.

Kullanım yerleri (dosya:satır):
- `MikroService.cs:5048` — `GunAcilisiYapV2` (`_TEMSILCI_GUNLUK_HAREKETLER` PK seçimi)
- `MikroService.cs:5057` — V15'te `Recno`, V16'da `Guid` PK kolonu seçimi
- `MikroService.cs:5131, 5159` — `GunKapanisiYapV2` (aynı kalıp)
- `MikroService.cs:5328` — `ZiyaretKaydetV2`
- `CariExtensions.cs:138, 377` — `CariKaydetYeniCari` (V15 vs V16 insert BEGIN/END)
- `CariSqlite.cs:1594` — `mye_TextData` okuma (V15'te `RecID_DBCno+RecID_RECno`, V16'da `Record_uid`)

### 4.2 V15 INSERT + UPDATE Kimlik Kalıbı

```sql
BEGIN
  INSERT INTO <Tablo> (<kolonlar>, <PK_alan>)
  VALUES (@degerler, 0)
  UPDATE <Tablo> SET <PK_alan> = (SELECT SCOPE_IDENTITY())
  WHERE <RECno_PK> = (SELECT SCOPE_IDENTITY())
END
```

Bu kalıp, V15'te `cari_RECid_RECno`, `adr_RECid_RECno`, `mye_RECid_RECno`,
`sth_RECid_RECno`, `cha_RECid_RECno` gibi çapraz tablo bağlantı alanlarını
doldurmak için kullanılır. V16'da bu gerekmez çünkü `Guid` insert anında
üretilir.

### 4.3 V16 `NEWID()` Kalıbı

```sql
INSERT INTO <Tablo> (<Guid_col>, <diğer kolonlar>)
VALUES (NEWID(), @değerler)
```

Veya `MikroService.cs:5079-5084` kalıbı:
```csharp
if (mikroVersiyon > 15) {
    text3 = "Guid,";       // INSERT kolonlarına ekle
    text4 = "NEWID(),";    // VALUES'a ekle
}
```

## 5. Adapter Katmanı İçin Önerilen Soyutlama

`SyncAdapter.Mikro.IdentityMapper` interface'i:

```csharp
public interface IIdentityMapper
{
    /// <summary>V15: "sth_RECno", V16: "sth_Guid"</summary>
    string GetPrimaryKeyColumn(string tableName);

    /// <summary>V15: "sth_RECid_RECno", V16: "sth_Guid" (sadece çapraz ref için)</summary>
    string GetCrossRefColumn(string fromTable, string toTable);

    /// <summary>Sipariş FK: V15: "sth_sip_recid_recno", V16: "sth_sip_uid"</summary>
    string GetOrderLinkColumn(string stokHareketTable);

    /// <summary>Fatura FK: V15: "sth_fat_recid_recno", V16: "sth_fat_fat_uid"</summary>
    string GetInvoiceLinkColumn(string stokHareketTable);

    /// <summary>V15: "SCOPE_IDENTITY()", V16: "NEWID()"</summary>
    string GetNewIdExpression();

    /// <summary>Identity değerini SQL parametresine çevir.</summary>
    DbParameter CreateIdentityParam(object value);
}
```

V15 ↔ V16 eşlemesi tek bir `V15IdentityMapper` ve `V16IdentityMapper` sınıfında
yaşar; core / service / UI sadece DTO/RECno/Guid bilir, hangi kolonun
kullanıldığını bilmez.

### 5.1 Mapper Seçimi (DI)

```csharp
services.AddSingleton<IIdentityMapper>(sp =>
{
    var sqlConn = sp.GetRequiredService<ISqlConnectionFactory>();
    var version = GenelUtility.GetMikroVersiyon(sqlConn.GetDatabaseName());
    return version <= 15
        ? new V15IdentityMapper()
        : new V16IdentityMapper();
});
```

## 6. V15 → V16 Geçiş Senaryosu (Bilgi)

Mikro ERP tarafında V15 → V16 geçişi sırasında:

- V15 RECno int değerleri V16'ya doğrudan taşınmaz.
- V16 yeni kurulumda tüm `Guid` alanlarına `NEWID()` atanır.
- V15'ten V16'ya veri aktarımı sırasında mapping tablosu:
  - `(V15_RECno, V15_RECid_DBCno, V15_RECid_RECno) → V16_Guid` mapping
- SyncAdapter bu mapping'i lokal `mappings` tablosunda tutar, böylece eski
  Android offline kayıtları (V15 RECno referanslı) yeni kurulumda da çalışır.

## 7. Test Senaryoları (V15/V16 Parity)

- [ ] V15 ve V16 aynı payload ile insert → her iki DB'de `cari_kod` aynı, farklı PK.
- [ ] V15/V16 idempotency: aynı payload ikinci kez → ikinci kez insert yapılmaz.
- [ ] V15 → V16 cross-FK: irsaliye → fatura bağlantısı V15'te `sth_fat_recid_recno`,
      V16'da `sth_fat_uid` ile çalışır.
- [ ] Sipariş karşılama: irsaliye oluşturulduğunda `sip_teslim_miktar` artar (V15/V16 ortak).
- [ ] GPS update: V15 ve V16 aynı SQL, kolon isimleri ortak.
- [ ] `mye_TextData` UPSERT: V15'te `RecID_DBCno+RecID_RECno`, V16'da `Record_uid` (CariSqlite.cs:1594).

## 8. Doğruluk Kanıtı

| İddia | Kanıt |
|-------|-------|
| V15 CARI_HESAPLAR field listesi | `TabloHelper.cs:124-142` |
| V16 CARI_HESAPLAR field listesi | `TabloHelperV16.cs:155-210` |
| V15 STOK_HAREKETLERI field listesi | `TabloHelper.cs:660-676` |
| V16 STOK_HAREKETLERI field listesi | `TabloHelperV16.cs:1591+` |
| V15 INSERT + SCOPE_IDENTITY() kalıbı | `CariExtensions.cs:138` |
| V16 NEWID() kalıbı | `CariExtensions.cs:377` |
| `mikroVersiyon` tespiti | `MikroService.cs:5048, 5057, 5131, 5159, 5328` |
| V16'da `Guid + NEWID()` ekleme | `MikroService.cs:5079-5084` |
| `mye_TextData` V15/V16 farkı | `CariSqlite.cs:1594` |
| V15 RECno int PK | `TabloHelper.cs:116, 670` |
| V16 Guid PK | `TabloHelperV16.cs:163, 1597` |
| Sipariş karşılama update | `Evrak.cs:6955` |
| Beden/renk V15 RECid_RECno vs V16 Guid | `Evrak.cs:6459, 6487` |

— Üretildi: 2026-09-07
