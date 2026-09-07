# Mikro ERP Veri Okuma/Yazma Dokumani

Bu dokuman, bu klasordeki eski `Fora Mikro` uygulamasinin Mikro ERP SQL veritabanindan hangi tablolari okudugunu, hangi tablolara veri yazdigini ve ayni mantikta yeni bir senkronizasyon uygulamasi yazdirirken hangi kurallarin uygulanmasi gerektigini anlatir.

Kaynak inceleme notu:

- Uygulamanin asil kodu derlenmis DLL/EXE olarak duruyor; detaylar `.decompiled/` altindaki decompile edilmis C# kaynaklardan cikarildi.
- Ana SQL katmani: `.decompiled/DataSql/Fora.Mikro.Data.Sql/`
- Evrak yazma katmani: `.decompiled/DataSql/Fora.Mikro.Data.Sql/EvrakData.cs`
- Cari okuma/yazma: `.decompiled/DataSql/Fora.Mikro.Data.Sql/CariData.cs`, `CariExtensions.cs`
- Stok okuma/yazma: `.decompiled/DataSql/Fora.Mikro.Data.Sql/StokData.cs`
- Siparis okuma/onay: `.decompiled/DataSql/Fora.Mikro.Data.Sql/SiparislerData.cs`
- Yeni nesil adapter fikri: `Sync Adapter/README.md`, `Sync Adapter/docs/01-system-architecture.md`, `Sync Adapter/docs/02-mikro-table-rules.md`, `Sync Adapter/docs/03-api-contracts.md`

## Genel Mimari

Eski uygulama, musteri bilgisayarindan Mikro SQL Server'a dogrudan baglanir. SQL baglanti bilgileri `data/sqlbaglantibilgileri.xml` dosyasindan okunur. Firma/Mikro database secimi `data/servisbilgileri.xml` icindeki `MikroDbName` alanindan gelir.

Baglanti sekli:

- `SqlServer`
- `SqlUserName`
- `SqlPassword`
- `Initial Catalog = Mikro firma veritabani`
- `MultipleActiveResultSets=True`

Yeni yazilacak uygulamada bu bilgi sifreli saklanmalidir. Mevcut XML icindeki parola veya benzeri secret alanlar loglanmamalidir.

Onerilen yeni mimari:

1. Android/web/mobile uygulama Mikro SQL'e direkt baglanmaz.
2. Mobile uygulama merkezi API'ye JSON payload gonderir.
3. Musteri makinesindeki Windows agent merkezi API'den pending job ceker.
4. Agent payload'u validate eder.
5. Agent payload'u Mikro tablo modellerine transform eder.
6. Agent Mikro SQL'e tek transaction icinde yazar.
7. Basarili yazimdan sonra local mapping kaydeder ve merkezi API'ye ack gonderir.

## Mikro Versiyon Farki

Kod V15 ve V16 ayrimini destekliyor.

V15:

- Kimlik alanlari genellikle `RECno`, `*_RECid_DBCno`, `*_RECid_RECno`
- Insert sonrasi `SCOPE_IDENTITY()` ile `RECno` bulunup `*_RECid_RECno` alanina yaziliyor.
- Ornek: `STOK_HAREKETLERI.sth_RECid_RECno`, `CARI_HESAPLAR.cari_RECid_RECno`

V16:

- Kimlik alanlari genellikle `Guid` veya `uid`
- Insert sirasinda `NEWID()` veya uygulama tarafindan uretilmis Guid kullaniliyor.
- Baglanti alanlari `*_uid` seklinde.
- Ornek: `STOK_HAREKETLERI.sth_Guid`, `sth_sip_uid`, `sth_fat_uid`

Yeni uygulama her tablo icin versiyon adapter katmani kullanmalidir:

- V15 icin `RECno/RECid` baglantisi
- V16 icin `Guid/uid` baglantisi
- Dis API tarafinda her yazilan evrak icin `evrakSeri`, `evrakSira`, `recno` veya `guid` mapping kaydedilmelidir.

## Ana Okunan Tablolar

### Cari ve cari risk/ekstre

`CARI_HESAPLAR`

- Cari kodu: `cari_kod`
- Unvan: `cari_unvan1`, `cari_unvan2`
- Vergi bilgisi: `cari_vdaire_adi`, `cari_vdaire_no`, `cari_VergiKimlikNo`, `cari_sicil_no`
- Gruplar: `cari_bolge_kodu`, `cari_grup_kodu`, `cari_sektor_kodu`
- Temsilci: `cari_temsilci_kodu`
- Odeme/fiyat: `cari_odemeplan_no`, `cari_satis_fk`, `cari_satis_isk_kod`
- Doviz: `cari_doviz_cinsi`, `cari_doviz_cinsi1`, `cari_doviz_cinsi2`
- Varsayilan depolar: `cari_VarsayilanGirisDepo`, `cari_VarsayilanCikisDepo`
- Iletisim: `cari_CepTel`, `cari_EMail`
- Kilit/e-fatura: `cari_cari_kilitli_flg`, `cari_efatura_fl`

`CARI_HESAP_ADRESLERI`

- Cari kod: `adr_cari_kod`
- Adres no: `adr_adres_no`
- Adres: `adr_cadde`, `adr_sokak`, `adr_posta_kodu`, `adr_ilce`, `adr_il`, `adr_ulke`
- Telefon: `adr_tel_ulke_kodu`, `adr_tel_bolge_kodu`, `adr_tel_no1`, `adr_tel_no2`
- Rut/rota: `adr_temsilci_kodu`, `adr_yon_kodu`, `adr_uzaklik_kodu`, `adr_ziyaretperyodu`, `adr_ziyaretgunu`, `adr_ziyarethaftasi`
- Konum: `adr_gps_enlem`, `adr_gps_boylam`

`CARI_HESAP_YETKILILERI`

- Yetkili ad/soyad: `mye_isim`, `mye_soyisim`
- Iletisim: `mye_email_adres`, `mye_cep_telno`, `mye_dahili_telno`
- Kimlik/vergi: `mye_tc_kimlikno`, `mye_vergi_dairesi`, `mye_vergi_kimlikno`
- Adres ve unvan bilgileri

`CARI_HESAP_HAREKETLERI`

- Cari bakiye, ekstre, yapilacak tahsilatlar, ciro ve fatura hareketleri buradan okunur.
- Onemli alanlar: `cha_kod`, `cha_ciro_cari_kodu`, `cha_tarihi`, `cha_vade`, `cha_tip`, `cha_cinsi`, `cha_evrak_tip`, `cha_evrakno_seri`, `cha_evrakno_sira`, `cha_grupno`, `cha_meblag`, `cha_d_cins`, `cha_d_kur`

`ODEME_EMIRLERI`

- Odenmemis cek/senet/kredi karti/havale riskleri buradan okunur.
- Onemli alanlar: `sck_sahip_cari_kodu`, `sck_nerede_cari_kodu`, `sck_tip`, `sck_sonpoz`, `sck_tutar`, `sck_doviz`, `sck_vade`

`ODEME_PLANLARI`

- Vade/aratop hesaplari icin okunur.
- Onemli alan: `odp_no`, `odp_aratop`

`CARI_HESAP_TEMINATLARI`

- Cari teminat riski icin okunur.
- Onemli alanlar: `ct_carikodu`, `ct_tutari`, `ct_DovizCinsi`, `ct_vade`, `ct_Aciklama_no`

### Stok, fiyat ve envanter

`STOKLAR`

- Stok kodu: `sto_kod`
- Isim: `sto_isim`, `sto_kisa_ismi`, `sto_yabanci_isim`
- Vergi: `sto_perakende_vergi`, `sto_toptan_vergi`
- Birimler: `sto_birim1_ad`, `sto_birim1_katsayi`, `sto_birim2_ad`, `sto_birim2_katsayi`, `sto_birim3_ad`, `sto_birim3_katsayi`, `sto_birim4_ad`, `sto_birim4_katsayi`
- Siniflar: `sto_anagrup_kod`, `sto_altgrup_kod`, `sto_sektor_kodu`, `sto_marka_kodu`, `sto_model_kodu`, `sto_uretici_kodu`, `sto_reyon_kodu`
- Takip: `sto_bedenli_takip`, `sto_renkDetayli`, `sto_beden_kodu`, `sto_renk_kodu`
- Maliyet: `sto_standartmaliyet`, `sto_doviz_cinsi`
- Ambar adresi: `sto_yer_kod`

`BARKOD_TANIMLARI`

- Barkoddan stok bulma ve stok barkodlari icin okunur.
- Alanlar: `bar_kodu`, `bar_stokkodu`, `bar_partikodu`, `bar_lotno`, `bar_serino_veya_bagkodu`, `bar_barkodtipi`, `bar_icerigi`, `bar_birimpntr`, `bar_master`, `bar_bedenpntr`, `bar_renkpntr`, `bar_baglantitipi`

`STOK_HAREKETLERI`

- Depo miktari, son satis/alis fiyati, satis miktari, faturalasmamis irsaliye ve evrak satirlari icin okunur.
- Onemli alanlar: `sth_stok_kod`, `sth_cari_kodu`, `sth_tarih`, `sth_tip`, `sth_cins`, `sth_evraktip`, `sth_miktar`, `sth_tutar`, `sth_iskonto1..6`, `sth_masraf1..4`, `sth_vergi`, `sth_giris_depo_no`, `sth_cikis_depo_no`, `sth_evrakno_seri`, `sth_evrakno_sira`

`STOK_SATIS_FIYAT_LISTELERI`

- Liste fiyatlari icin okunur.
- Alanlar: `sfiyat_stokkod`, `sfiyat_listesirano`, `sfiyat_fiyati`, `sfiyat_doviz`, `sfiyat_iskontokod`

`SATIS_SARTLARI`

- Cari/stok/depo/odeme planina gore satis fiyat ve iskonto kosullari icin okunur.
- Alanlar: `sat_stok_kod`, `sat_cari_kod`, `sat_depo_no`, `sat_odeme_plan`, `sat_basla_tarih`, `sat_bitis_tarih`, `sat_brut_fiyat`, `sat_doviz_cinsi`, iskonto/masraf detay alanlari

`SATINALMA_SARTLARI`

- Alis fiyat ve iskonto kosullari icin okunur.
- Alanlar: `sas_stok_kod`, `sas_cari_kod`, `sas_depo_no`, `sas_basla_tarih`, `sas_bitis_tarih`, `sas_brut_fiyat`, `sas_doviz_cinsi`, iskonto/masraf detay alanlari

`STOK_CARI_ISKONTO_TANIMLARI`

- Stok/cari iskonto koduna ve odeme planina gore iskonto/masraf hesaplamak icin okunur.

Sinif/lookup tablolari:

- `STOK_ANA_GRUPLARI`: `san_kod`, `san_isim`
- `STOK_ALT_GRUPLARI`: `sta_kod`, `sta_ana_grup_kod`, `sta_isim`
- `STOK_SEKTORLERI`: `sktr_kod`, `sktr_ismi`
- `STOK_MARKALARI`
- `STOK_MODEL_TANIMLARI`
- `STOK_REYONLARI`
- `STOK_URETICILERI`: `urt_kod`, `urt_ismi`
- `STOK_KATEGORILERI`

### Siparis ve evrak okuma

`SIPARISLER`

- Acik siparisler, kalan miktar, siparis onaylama/reddetme, siparisten evrak karşilama icin okunur.
- Ana alanlar: `sip_evrakno_seri`, `sip_evrakno_sira`, `sip_satirno`, `sip_tarih`, `sip_teslim_tarih`, `sip_tip`, `sip_cins`, `sip_musteri_kod`, `sip_stok_kod`, `sip_miktar`, `sip_teslim_miktar`, `sip_kapat_fl`, `sip_depono`, `sip_satici_kod`, `sip_tutar`, `sip_iskonto_1..6`, `sip_masraf_1..4`, `sip_vergi`, `sip_masvergi`

`PROFORMA_SIPARISLER`

- Proforma evrak satirlari icin okunur/yazilir.

`DEPOLAR_ARASI_SIPARISLER`

- Depolar arasi siparis satirlari ve karşilama icin okunur/yazilir.

`EVRAK_ACIKLAMALARI`

- Evrak aciklama satirlari icin okunur/yazilir.
- Alanlar: `egk_dosyano`, `egk_hareket_tip`, `egk_evr_tip`, `egk_evr_seri`, `egk_evr_sira`, `egk_evracik1..10`

### Kasa, banka, kullanici, depo ve diger lookup

`BANKALAR`

- Banka secimi ve banka hesaplari icin okunur.
- Alanlar: `ban_kod`, `ban_ismi`, `ban_sube`, `ban_hesapno`, `ban_firma_no`, `ban_doviz_cinsi`, `ban_temsilci_email`, `ban_TCMB_Kodu`

`KASALAR`

- Kasa secimi/tahsilat icin okunur.

`DEPOLAR`

- Depo secimi, rapor filtreleri ve stok miktari icin okunur.

`DEPO_GRUPLARI`

- Depo grup lookup icin okunur.

`CARI_PERSONEL_TANIMLARI`

- Temsilci/plasiyer bilgisi icin okunur.
- Alanlar: `cari_per_kod`, `cari_per_adi`, `cari_per_soyadi`

`SORUMLULUK_MERKEZLERI`

- Rapor ve evrak sorumluluk merkezi icin okunur/yazilir.

`PROJELER`

- Proje kodu ve proje bazli filtreler icin okunur/yazilir.

`KARGO_TANIMLARI`, `SUBELER`, `PERSONELLER`, `MUHASEBE_HESAP_PLANI`, `MASRAF_HESAPLAR`, `HIZMET_HESAPLARI`

- F10/secim ekranlari, evrak alanlari ve lookup dogrulamalari icin okunur.

`KUR_ISIMLERI` ve `DOVIZ_KURLARI`

- Doviz sembolu ve kur cevirimi icin okunur.

## Uygulamanin Yazdigi Tablolar

### 1. Cari kart acma/guncelleme

Ana tablolar:

- `CARI_HESAPLAR`
- `CARI_HESAP_ADRESLERI`
- `CARI_HESAP_YETKILILERI`
- `mye_TextData`

Yazim mantigi:

- Yeni cari icin once `CARI_HESAPLAR` insert edilir.
- Ardindan cari adresleri `CARI_HESAP_ADRESLERI` tablosuna yazilir.
- Yetkili varsa `CARI_HESAP_YETKILILERI` tablosuna yazilir.
- V15 insert sonrasi `cari_RECid_RECno`, `adr_RECid_RECno`, `mye_RECid_RECno` alanlari `SCOPE_IDENTITY()` ile update edilir.
- V16 insert sirasinda `cari_Guid`, `adr_Guid`, `mye_Guid` kullanilir.
- Cari not/aciklama gibi uzun metinler `mye_TextData` tablosuna `TableID=31` ile yazilir.
- Cari lokasyon guncellemesi `CARI_HESAP_ADRESLERI.adr_gps_enlem`, `adr_gps_boylam`, `adr_lastup_date` alanlarini update eder.

Yeni uygulama kurali:

- Upsert anahtari `cari_kod` olmalidir.
- Ayni `cari_kod` varsa tekrar insert yapma; mevcut kaydi guncelle veya data hatasi dondur.
- Adreslerde anahtar `adr_cari_kod + adr_adres_no` olmalidir.

### 2. Stok kart acma/guncelleme

Ana tablo:

- `STOKLAR`

Yazim mantigi:

- Yeni stok `STOKLAR` tablosuna insert edilir.
- V15'te insert sonrasi `sto_RECid_RECno` update edilir.
- V16'da `sto_Guid` ile insert edilir.
- Ambar adresi degisikligi `STOKLAR.sto_yer_kod`, `sto_lastup_user`, `sto_lastup_date` alanlarini update eder.

Yeni uygulama kurali:

- Upsert anahtari `sto_kod` olmalidir.
- Stok adi, birim, vergi, grup, marka, reyon, uretici ve maliyet alanlari Mikro kolon uzunluklarina gore trim veya validate edilmelidir.

### 3. Satis faturasi

Ana tablolar:

- `CARI_HESAP_HAREKETLERI`
- `STOK_HAREKETLERI`
- `EVRAK_ACIKLAMALARI` opsiyonel
- `STOK_SERINO_TANIMLARI` opsiyonel
- `CIHAZ_HAREKETLERI` opsiyonel
- `BEDEN_HAREKETLERI` opsiyonel

Yazim mantigi:

- Fatura cari hareketi `CARI_HESAP_HAREKETLERI` tablosuna header/cari hareket olarak yazilir.
- Her stok satiri `STOK_HAREKETLERI` tablosuna yazilir.
- Cari hareket ve stok satirlari ayni SQL transaction icinde commit edilmelidir.
- Stok satirlarinin fatura header baglantisi kurulmalidir:
  - V15: `sth_fat_recid_dbcno`, `sth_fat_recid_recno`
  - V16: `sth_fat_uid`
- Siparisten karşilama varsa `SIPARISLER.sip_teslim_miktar` transaction icinde update edilir.
- Seri/lot/cihaz takibi varsa `STOK_SERINO_TANIMLARI` ve `CIHAZ_HAREKETLERI` guncellenir.

Onemli `CARI_HESAP_HAREKETLERI` alanlari:

- `cha_firmano`, `cha_subeno`
- `cha_tarihi`, `cha_vade`
- `cha_tip`, `cha_cinsi`, `cha_normal_Iade`, `cha_evrak_tip`
- `cha_evrakno_seri`, `cha_evrakno_sira`, `cha_satir_no`
- `cha_kod`, `cha_ciro_cari_kodu`
- `cha_d_cins`, `cha_d_kur`, `cha_altd_kur`
- `cha_meblag`, `cha_aratoplam`
- `cha_ft_iskonto1..6`, `cha_ft_masraf1..4`, `cha_vergi1..10`
- `cha_satici_kodu`, `cha_projekodu`, `cha_srmrkkodu`

Onemli `STOK_HAREKETLERI` alanlari:

- `sth_firmano`, `sth_subeno`
- `sth_tarih`, `sth_tip`, `sth_cins`, `sth_normal_iade`, `sth_evraktip`
- `sth_evrakno_seri`, `sth_evrakno_sira`, `sth_satirno`
- `sth_stok_kod`, `sth_cari_kodu`, `sth_cari_cinsi`
- `sth_plasiyer_kodu`
- `sth_miktar`, `sth_miktar2`, `sth_birim_pntr`
- `sth_tutar`, `sth_iskonto1..6`, `sth_masraf1..4`
- `sth_vergi_pntr`, `sth_vergi`
- `sth_har_doviz_cinsi`, `sth_har_doviz_kuru`, `sth_alt_doviz_kuru`
- `sth_giris_depo_no`, `sth_cikis_depo_no`
- `sth_odeme_op`, `sth_aciklama`
- `sth_sip_recid_*` veya `sth_sip_uid`
- `sth_fat_recid_*` veya `sth_fat_uid`
- `sth_adres_no`, `sth_parti_kodu`, `sth_lot_no`, `sth_proje_kodu`

### 4. Satis irsaliyesi

Ana tablo:

- `STOK_HAREKETLERI`

Yazim mantigi:

- Irsaliye stok hareketleri `STOK_HAREKETLERI` tablosuna yazilir.
- Cari, depo, teslim adresi, temsilci ve sorumluluk merkezi lookup kontrolleri yapilir.
- Irsaliyeden siparis kapaniyorsa `SIPARISLER.sip_teslim_miktar` guncellenir.
- Sonradan faturaya baglanacaksa `sth_fat_recid_*` veya `sth_fat_uid` alanlari kullanilir.

### 5. Alinan siparis / verilen siparis

Ana tablo:

- `SIPARISLER`

Okuma/guncelleme:

- Acik siparisler `SIPARISLER` tablosundan okunur.
- Onay islemi `sip_OnaylayanKulNo` ve `sip_cagrilabilir_fl` alanlarini update eder.
- Reddetme/kapama islemi `sip_kapat_fl` ve `sip_kapatmanedenkod` alanlarini update eder.
- Kalan miktar `sip_miktar - sip_teslim_miktar` ile hesaplanir.

Yeni uygulama yazim kurali:

- Her payload satiri bir `SIPARISLER` satiri olmalidir.
- Ayni evraktaki tum satirlarda `sip_evrakno_seri` ve `sip_evrakno_sira` ayni olmalidir.
- `sip_satirno` 0'dan veya 1'den baslayip sirali artmalidir; proje mevcut Mikro/Fora standardina gore secilmelidir.
- Yeni sipariste `sip_teslim_miktar = 0` olmalidir.
- Sonraki fatura/irsaliye karşilamalarinda teslim miktari transaction icinde atomik artirilmalidir.

### 6. Tahsilat

Ana tablolar:

- `CARI_HESAP_HAREKETLERI`
- `ODEME_EMIRLERI` opsiyonel

Yazim mantigi:

- Nakit tahsilatta `CARI_HESAP_HAREKETLERI` hareketi yeterlidir.
- Cek, senet, kredi karti, havale/banka gibi tahsilat tiplerinde `ODEME_EMIRLERI` kaydi da olusturulur veya update edilir.
- Tahsilat evrak seri/sira bilgisi `cha_evrakno_seri`, `cha_evrakno_sira` ile tutulur.
- Cari kod, kasa/banka kodu, doviz, kur, vade ve tutar validate edilmelidir.

### 7. Proforma

Ana tablo:

- `PROFORMA_SIPARISLER`

Yazim/okuma mantigi:

- Proforma satirlari `PROFORMA_SIPARISLER` tablosunda tutulur.
- Stok bilgisi icin `STOKLAR` ile join edilir.
- Aciklamalar `EVRAK_ACIKLAMALARI` tablosundan okunur/yazilir.

### 8. Depolar arasi siparis / transfer

Ana tablolar:

- `DEPOLAR_ARASI_SIPARISLER`
- `STOK_HAREKETLERI`
- `EVRAK_ACIKLAMALARI`

Yazim/okuma mantigi:

- Depolar arasi siparis satirlari `DEPOLAR_ARASI_SIPARISLER` tablosunda tutulur.
- Gercek stok transferi veya nakliye hareketi `STOK_HAREKETLERI` ile yazilir.
- Kaynak depo ve hedef depo farkli olmalidir.
- Nakliye fislerinde `sth_nakliyedeposu`, `sth_nakliyedurumu`, giris/cikis depo alanlari dogru set edilmelidir.

### 9. Seri/lot/cihaz hareketleri

Ana tablolar:

- `STOK_SERINO_TANIMLARI`
- `CIHAZ_HAREKETLERI`

Yazim mantigi:

- Seri numarali stokta once `STOK_SERINO_TANIMLARI` kontrol edilir.
- Kayit yoksa insert edilir.
- Alis/satis evrakina gore ilgili alanlar update edilir:
  - Alis: `chz_al_cari_kodu`, `chz_al_evr_seri`, `chz_al_evr_sira`, `chz_al_tarih`, alis fiyat alanlari
  - Satis: `chz_st_cari_kodu`, `chz_st_evr_seri`, `chz_st_evr_sira`, `chz_st_tarih`, satis fiyat alanlari
- Cihaz hareket baglantisi `CIHAZ_HAREKETLERI` ile yazilir.
- V15'te master baglanti `ChHar_master_dbcno`, `ChHar_master_recno`; V16'da `ChHar_master_uid`.

### 10. Beden/renk ve ceki listesi

Ana tablolar:

- `BEDEN_HAREKETLERI`
- `CEKI_LISTESI`

Yazim mantigi:

- Beden/renk takipli stoklarda hareket satiri ile paralel `BEDEN_HAREKETLERI` yazilir veya update edilir.
- Ceki/paket bilgileri `CEKI_LISTESI` tablosuna yazilir.
- V15 insert sonrasi `*_RECid_RECno` update edilir; V16 Guid kullanilir.

### 11. Ozel Fora tabloları

`_FORA_PARAMETRELER`

- Android kullanici ayarlari, rapor parametreleri, yazici ayarlari ve uygulama parametreleri icin okunur/yazilir.
- Islem tipleri: select, insert, update, delete.
- Ornek kullanimlar:
  - `ParametreProgram='akilli'`: Android kullanici ayarlari
  - `ParametreProgram='YaziciAyarlari'`: yazici sablonlari
  - Rapor parametreleri

`_SIPARIS_KARSILAMA_ARA_TABLO`

- Siparis karşilama ara tablosu olarak uygulama tarafindan gerekirse olusturulur.
- Alanlar: `SiparisSeriNo`, `SiparisSiraNo`, `SiparisKarsilanmaTarihi`, `MikroyaAktarildi`, `EvrakTipi`, `MikroSeriNo`, `MikroSiraNo`, `MikroyaAktarilmaTarihi`, `EvrakIcerik`

`_ZIYARET_HAREKETLERI`

- Ziyaret edilmis mi kontrolu icin okunur.
- Alanlar: `zyrt_Temsilci_Kodu`, `zyrt_Cari_Kodu`, `zyrt_Cari_Adres_No`, `zyrt_Tarihi`

## Bootstrap Veri Cekme Mantigi

Yeni uygulamada Android/web istemciyi beslemek icin Mikro'dan su paketler cekilmelidir:

1. `customers`
   - `CARI_HESAPLAR`
   - `CARI_HESAP_ADRESLERI`
   - `CARI_HESAP_YETKILILERI`
   - Filtre: aktif cariler, temsilciye atanmis adresler, kilitli olmayan cariler

2. `stocks`
   - `STOKLAR`
   - `BARKOD_TANIMLARI`
   - Grup/marka/reyon/uretici lookup tablolari
   - Filtre: satisi durmamis, pasif olmayan, mobilde gosterilecek stoklar

3. `prices`
   - `STOK_SATIS_FIYAT_LISTELERI`
   - `SATIS_SARTLARI`
   - `STOK_CARI_ISKONTO_TANIMLARI`
   - Gerekirse son fiyat icin `STOK_HAREKETLERI`

4. `inventory`
   - `STOK_HAREKETLERI`
   - Depo bazli miktar hesaplanir:
     - Giris hareketleri miktari artirir.
     - Cikis hareketleri miktari azaltir.
     - Transferde giris/cikis depoya gore isaret degisir.

5. `openOrders`
   - `SIPARISLER`
   - Filtre: `sip_kapat_fl=0`, `sip_teslim_miktar <> sip_miktar`

6. `cashAndBank`
   - `KASALAR`
   - `BANKALAR`

7. `lookups`
   - `DEPOLAR`
   - `CARI_PERSONEL_TANIMLARI`
   - `SORUMLULUK_MERKEZLERI`
   - `PROJELER`
   - `ODEME_PLANLARI`
   - `KUR_ISIMLERI`

## Yazma Akisi - Yeni Uygulama Icin Standart

Her Mikro yazimi ayni kalibi izlemelidir:

1. Payload semasini validate et.
2. `tenant_id + document_type + external_id` ile idempotency kontrolu yap.
3. Lookup kontrollerini yap:
   - Cari kod var mi?
   - Stok kodlari var mi?
   - Depo/kasa/banka/odeme plani/doviz/temsilci/proje/sorumluluk merkezi var mi?
4. Mikro versiyonunu tespit et.
5. Evrak seri/sira sec ve cakisiyor mu kontrol et.
6. SQL transaction baslat.
7. Header, satirlar, aciklamalar, seri/lot, siparis karşilama update'leri ayni transaction icinde yaz.
8. V15 ise `RECid_RECno`, V16 ise `uid/Guid` baglantilarini kur.
9. Transaction commit et.
10. Local `mappings` tablosuna external id ile Mikro kimligini kaydet.
11. Merkezi API'ye ack gonder.

Hicbir durumda:

- Ayni job tekrar geldiginde ikinci kez Mikro evragi olusturulmamali.
- Header yazilip satirlar yazilamadan commit edilmemeli.
- Mikro transaction basarisizken API'ye success ack gonderilmemeli.
- Kullanici girdisi string concat ile SQL'e eklenmemeli.

## Onerilen Local Mapping Tablosu

Yeni agent tarafinda su tablo tutulmalidir:

```sql
CREATE TABLE mappings (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    tenant_id TEXT NOT NULL,
    entity_type TEXT NOT NULL,
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
    UNIQUE (tenant_id, entity_type, document_type, external_id)
);
```

## Onerilen Payload Ornekleri

### Satis siparisi

```json
{
  "documentType": "sales_order",
  "externalId": "android-order-uuid",
  "occurredAt": "2026-07-09T10:30:00+03:00",
  "customerCode": "120.001",
  "salespersonCode": "PLS01",
  "warehouseNo": 1,
  "currency": 0,
  "documentNo": {
    "series": "A",
    "number": 123
  },
  "lines": [
    {
      "stockCode": "STK001",
      "quantity": 2,
      "unitPointer": 1,
      "unitPrice": 100,
      "taxPointer": 4,
      "discounts": [0, 0, 0, 0, 0, 0]
    }
  ]
}
```

Mikro hedefi:

- `SIPARISLER`

### Satis faturasi

```json
{
  "documentType": "sales_invoice",
  "externalId": "android-invoice-uuid",
  "customerCode": "120.001",
  "salespersonCode": "PLS01",
  "warehouseNo": 1,
  "currency": 0,
  "documentNo": {
    "series": "F",
    "number": 456
  },
  "lines": [
    {
      "stockCode": "STK001",
      "quantity": 2,
      "unitPointer": 1,
      "grossAmount": 200,
      "discounts": [0, 0, 0, 0, 0, 0],
      "taxPointer": 4,
      "taxAmount": 40,
      "sourceOrder": {
        "series": "A",
        "number": 123,
        "lineNo": 0
      }
    }
  ],
  "notes": ["Mobil faturadan geldi"]
}
```

Mikro hedefi:

- `CARI_HESAP_HAREKETLERI`
- `STOK_HAREKETLERI`
- `EVRAK_ACIKLAMALARI`
- Gerekirse `SIPARISLER` teslim miktari update

### Tahsilat

```json
{
  "documentType": "collection",
  "externalId": "android-collection-uuid",
  "customerCode": "120.001",
  "paymentType": "cash",
  "cashCode": "KASA01",
  "amount": 1000,
  "currency": 0,
  "dueDate": "2026-07-09",
  "documentNo": {
    "series": "T",
    "number": 99
  }
}
```

Mikro hedefi:

- Nakit: `CARI_HESAP_HAREKETLERI`
- Cek/senet/kredi karti/havale: `CARI_HESAP_HAREKETLERI` + `ODEME_EMIRLERI`

### Yeni cari

```json
{
  "entityType": "customer",
  "operation": "upsert",
  "externalId": "android-customer-uuid",
  "customerCode": "120.999",
  "title1": "ABC LTD",
  "title2": "",
  "taxOffice": "Kadikoy",
  "taxNo": "1234567890",
  "salespersonCode": "PLS01",
  "groupCode": "GRP",
  "regionCode": "IST",
  "phone": "5550000000",
  "email": "info@example.com",
  "addresses": [
    {
      "addressNo": 1,
      "city": "Istanbul",
      "district": "Kadikoy",
      "street": "Ornek Sokak",
      "latitude": 41.0,
      "longitude": 29.0
    }
  ]
}
```

Mikro hedefi:

- `CARI_HESAPLAR`
- `CARI_HESAP_ADRESLERI`
- `CARI_HESAP_YETKILILERI` opsiyonel

## Tablo Bazli Ozet

| Tablo | Okuma | Yazma | Amac |
| --- | --- | --- | --- |
| `CARI_HESAPLAR` | Evet | Evet | Cari kart, cari secim, risk, e-fatura/kilit kontrolu |
| `CARI_HESAP_ADRESLERI` | Evet | Evet | Adres, rota, GPS, teslim/fatura adresi |
| `CARI_HESAP_YETKILILERI` | Evet | Evet | Cari yetkili bilgileri |
| `CARI_HESAP_HAREKETLERI` | Evet | Evet | Fatura/tahsilat cari hareketi, bakiye, ekstre |
| `STOKLAR` | Evet | Evet | Stok kart, stok secim, maliyet, birim |
| `BARKOD_TANIMLARI` | Evet | Hayir | Barkoddan stok ve birim bulma |
| `STOK_HAREKETLERI` | Evet | Evet | Fatura/irsaliye/transfer satirlari, stok miktari |
| `SIPARISLER` | Evet | Evet | Siparis satirlari, onay/red, teslim miktari |
| `PROFORMA_SIPARISLER` | Evet | Evet | Proforma satirlari |
| `DEPOLAR_ARASI_SIPARISLER` | Evet | Evet | Depolar arasi siparis |
| `ODEME_EMIRLERI` | Evet | Evet | Cek/senet/kredi karti/havale |
| `EVRAK_ACIKLAMALARI` | Evet | Evet | Evrak aciklama satirlari |
| `STOK_SERINO_TANIMLARI` | Evet | Evet | Seri/cihaz bilgisi |
| `CIHAZ_HAREKETLERI` | Hayir | Evet | Seri/cihaz hareket baglantisi |
| `BEDEN_HAREKETLERI` | Hayir | Evet | Beden/renk takip hareketleri |
| `CEKI_LISTESI` | Hayir | Evet | Ceki/paket satirlari |
| `STOK_SATIS_FIYAT_LISTELERI` | Evet | Hayir | Liste fiyati |
| `SATIS_SARTLARI` | Evet | Hayir | Satis kosul/fiyat/iskonto |
| `SATINALMA_SARTLARI` | Evet | Hayir | Alis kosul/fiyat/iskonto |
| `STOK_CARI_ISKONTO_TANIMLARI` | Evet | Hayir | Cari/stok iskonto |
| `ODEME_PLANLARI` | Evet | Hayir | Vade/odeme plani |
| `BANKALAR` | Evet | Hayir | Banka hesaplari |
| `KASALAR` | Evet | Hayir | Kasa secimi |
| `DEPOLAR` | Evet | Hayir | Depo secimi |
| `CARI_PERSONEL_TANIMLARI` | Evet | Hayir | Temsilci/plasiyer |
| `SORUMLULUK_MERKEZLERI` | Evet | Evet | Sorumluluk merkezi |
| `PROJELER` | Evet | Evet | Proje |
| `_FORA_PARAMETRELER` | Evet | Evet | Uygulama/Android/yazici/rapor parametreleri |
| `_SIPARIS_KARSILAMA_ARA_TABLO` | Evet | Evet | Siparis karşilama ara kaydi |
| `_ZIYARET_HAREKETLERI` | Evet | Hayir | Ziyaret kontrolu |
| `mye_TextData` | Hayir | Evet | Cari uzun not/aciklama |

## AI Editore Verilecek Uygulama Talimati

Asagidaki talimat yeni uygulamayi yazacak AI editore verilebilir:

```text
Bir .NET 8 Windows Service + küçük ayar UI'si olan Mikro ERP Sync Agent yaz.

Amaç:
- Android/web saha satis uygulamasindan gelen JSON payload'larini merkezi API'den pull et.
- Payload'lari local SQLite kuyruğuna durable yaz.
- Mikro SQL Server'a sadece allowlist edilen tablolara transaction içinde yaz.
- Mikro'dan Android'i besleyecek cari, stok, fiyat, depo, kasa/banka ve siparis verilerini okuyup merkezi API'ye push et.

Zorunlu kurallar:
- Android veya merkezi API Mikro SQL'e direkt bağlanmayacak.
- Agent sadece outbound HTTPS kullanacak.
- SQL connection secret'lari encrypted local store'da tutulacak.
- Her job için idempotency mapping olacak.
- Aynı externalId tekrar gelirse ikinci Mikro evrakı oluşturulmayacak.
- Evrak yazarken header, satırlar, açıklamalar, seri/lot ve sipariş karşılama update'leri tek SQL transaction içinde olacak.
- Mikro transaction commit olmadan API'ye ack gönderilmeyecek.
- SQL parametreli yazılacak; kullanıcı verisi string concat ile SQL'e eklenmeyecek.
- V15 RECno/RECid ve V16 Guid/uid farkı adapter katmanında çözülecek.

Okunacak ana tablolar:
- CARI_HESAPLAR, CARI_HESAP_ADRESLERI, CARI_HESAP_YETKILILERI
- STOKLAR, BARKOD_TANIMLARI, STOK_SATIS_FIYAT_LISTELERI, SATIS_SARTLARI, SATINALMA_SARTLARI
- STOK_HAREKETLERI, SIPARISLER, CARI_HESAP_HAREKETLERI, ODEME_EMIRLERI
- DEPOLAR, KASALAR, BANKALAR, CARI_PERSONEL_TANIMLARI, ODEME_PLANLARI, PROJELER, SORUMLULUK_MERKEZLERI
- Lookup tabloları: STOK_ANA_GRUPLARI, STOK_ALT_GRUPLARI, STOK_MARKALARI, STOK_REYONLARI, STOK_SEKTORLERI, STOK_URETICILERI

Yazılacak ana tablolar:
- Sales order: SIPARISLER
- Sales invoice: CARI_HESAP_HAREKETLERI + STOK_HAREKETLERI + EVRAK_ACIKLAMALARI
- Sales dispatch: STOK_HAREKETLERI + EVRAK_ACIKLAMALARI
- Collection: CARI_HESAP_HAREKETLERI, gerekiyorsa ODEME_EMIRLERI
- Customer upsert: CARI_HESAPLAR + CARI_HESAP_ADRESLERI + CARI_HESAP_YETKILILERI
- Stock upsert: STOKLAR
- Serial/lot/device: STOK_SERINO_TANIMLARI + CIHAZ_HAREKETLERI
- Order fulfillment: SIPARISLER.sip_teslim_miktar update

Modüller:
- MikroDbConnector
- MikroVersionDetector
- SchemaExplorer
- RemoteApiClient
- QueueManager
- MappingService
- CheckpointService
- TransformerRegistry
- MikroWriterRegistry
- SalesOrderWriter
- SalesInvoiceWriter
- DispatchWriter
- CollectionWriter
- CustomerWriter
- StockWriter
- BootstrapSyncService
- AuditLogger

Testler:
- Aynı job iki kez çalıştırıldığında tek Mikro evrakı oluşmalı.
- Header yazılıp satır hatası olursa transaction rollback olmalı.
- V15 ve V16 identity/link alanları ayrı test edilmeli.
- Eksik cari/stok/depo/kasa/banka lookup'ında data error dönmeli.
- Timeout/deadlock/API 5xx transient retry edilmeli.
```

## Kritik Dikkat Noktalari

- Eski decompile kodda bazi yerlerde string concat ile SQL olusturulmus. Yeni uygulamada bu tekrar edilmemeli.
- Mikro tablolari muhasebe/yasal kayit tasidigi icin lisans suresi bitse bile Mikro'daki yazilmis evraklar silinmemeli.
- Local cache, token, kuyruk ve gecici veriler temizlenebilir; Mikro yasal tablolarina dokunulmaz.
- Her tablo icin kolon uzunluklari schema discovery ile bulunup string alanlar trim/validate edilmelidir.
- `WITH(NOLOCK)` eski uygulamada yaygin; yeni kritik finansal okumalarda tutarlilik ihtiyacina gore isolation seviyesi bilincli secilmelidir.
- Evrak seri/sira uretimi merkezi ve idempotent olmalidir; ayni seri/sira baska evrakla cakismamalidir.
