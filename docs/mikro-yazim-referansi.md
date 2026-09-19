# Mikro V15 — Satış, İade ve Tahsilat Evrakları Nasıl Yazılır

Tarih: 2026-09-17 · Kaynaklar: (1) `Fora_Mikro/.decompiled` — `Core/Fora.Mikro.Evraklar/Evrak.cs`
(`StokCariHesapHareketiOlustur`, tahsilat satırı, stok satırı), `DataSql/Fora.Mikro.Data.Sql/EvrakData.cs`
(`YeniNormalEvrakKaydet`, `V15_TahsilatOdemeEmriYaz`, `TahsilatSonRefNoBul`); (2) `localhost/MikroDB_V15_02`'de
**Mikro'nun kendi ekranından girilmiş** kayıtlar (seri boş, e-belge `cha_uuid` dolu olanlar) ve sahadan gelen `T/H/ST`
serili kayıtlar. İkisinin uyuştuğu yerler "doğrulandı", ayrıştığı yerler ayrıca belirtildi.

> Enum sayıları Fora enum'larının sıra değerleridir; canlı verideki değerlerle birebir karşılaştırıldı.
> Tablolarda yalnız sıfırdan farklı yazılan kolonlar listelenir; geri kalan her kolon §1'deki "boş kolon yok"
> kuralıyla tipine göre sıfır değerle doldurulur. Kodlar `MikroCodes` (`Erp.Mikro/Writers/MikroDocumentCodes.cs`) içinde;
> bu belge ile kodlar canlı veriye karşı `MikroNativeDocumentConventionTests` ile doğrulanır (§8).

---

## 1. Ortak kurallar

| Konu | Kural | Kanıt |
|---|---|---|
| **Boş kolon yok** | Hedef 5 tablonun (CHA, STH, ODEME_EMIRLERI, EVRAK_ACIKLAMALARI, SIPARISLER) kolonlarının neredeyse hepsi nullable ve varsayılansız (NOT NULL yalnız `*_RECno`, `*_RECid_DBCno`, `*_RECid_RECno`), ama Mikro'nun kendi kayıtlarında **hiç NULL yok**. Writer INSERT kolon listesini şemadan (INFORMATION_SCHEMA, önbellekli) kurar; açıkça atanmayan kolon tipine göre: `bit/int/smallint/tinyint/float` → 0, `nvarchar` → `''`, `datetime` → `1899-12-30` | canlı: 3 tabloda son Mikro kayıtlarında NULL sayısı 0 (test) |
| Kimlik (V15) | `*_RECno` identity; `*_RECid_DBCno=0`, `*_RECid_RECno = *_RECno` (aynı transaction'da UPDATE) | Fora INSERT + `UPDATE … SET *_RECid_RECno = SCOPE_IDENTITY()`; canlı veride eşit |
| Dosya no | `cha_fileid=51`, `sth_fileid=16`, `sck_fileid=54`, `egk_fileid=66` | canlı + Fora |
| Kullanıcı | `*_create_user`, `*_lastup_user` = Mikro kullanıcı no (Fora: parametre `MikroUserNo`); tarih `getdate()` | canlı (1, 4) |
| Tarih | `cha_tarihi`, `cha_belge_tarih`, `sth_tarih`, `sth_belge_tarih`, `sth_malkbl_sevk_tarihi` saatsiz gün | canlı |
| Döviz | TL: `cha_d_cins=0`, `cha_d_kur=1`, `cha_altd_kur=1`, `cha_karsid_kur=1`; `sth_har_doviz_kuru=sth_alt_doviz_kuru=sth_stok_doviz_kuru=1` | canlı |
| Seri/sıra | Aynı evrakın tüm satırları aynı `evrakno_seri` + `evrakno_sira`; satırlar `cha_satir_no` / `sth_satirno` / `sip_satirno` 0'dan artar. Sıra = MAX+1, anahtar Mikro'nun tekil indeksi: CHA `(cha_evrak_tip, seri, sira, satir_no)` → satış faturası `evrak_tip=63`, iade/alış `0` (**satış iadesi alış faturalarıyla aynı sırayı paylaşır**), tahsilat `1`; STH `(sth_evraktip, seri, sira, satirno)` → irsaliye `evraktip=1, tip=1`; SIPARISLER `(sip_tip, sip_cins, seri, sira, satirno)` → `0, 0`. Fora yazmadan önce aynı anahtarla `EvrakVarMi` yapar (varsa -2) | Fora `YeniSeriNoBul`/`EvrakVarMi` + indeksler |
| Tekil indeksler | `NDX_CARI_HESAP_HAREKETLERI_04`, `NDX_STOK_HAREKETLERI_05`, `NDX_SIPARISLER_06`, `NDX_ODEME_EMIRLERI_02 (sck_tip, sck_refno)`, `NDX_EVRAK_ACIKLAMALARI_02 (dosyano, hareket_tip, evr_tip, seri, sira, ustkod)` — çakışma yarışını veritabanı da yakalar | canlı |
| KDV oranı | `dbo.fn_VergiYuzde(pntr)` → V15_02'de 1=%0, 2=%1, 3=%10, 4=%20, 5=%26. Stok kartındaki pointer `sto_toptan_vergi` / `sto_perakende_vergi`; bu firmada 4.567 stokun hepsi 1 (%0) | canlı |
| KDV dahil fiyat | Mikro fiyat listesi tanımında bayrak var: `STOK_SATIS_FIYAT_LISTE_TANIMLARI.sfl_kdvdahil` (V15_02'deki 3 listede 0) | canlı |
| Açıklama | `EVRAK_ACIKLAMALARI` — `egk_dosyano=51`; satış faturası `(hareket 0, evr_tip 63)`, iade/alış `(1, 0)`, tahsilat `(1, 1)`, tediye `(0, 64)`, irsaliye `(dosyano 16, hareket 1, evr_tip 1)`. Fora satırı **yalnız açıklama doluysa** yazar (`egk_evracik1..10`, `egk_evr_ustkod=''`, `egk_tesaltarihi=1900-01-01`). Mikro'nun kendi satırları çoğunlukla açıklamasız, yazdırma sayaçlı (`egk_prevwiewsayisi`) — yani Mikro bu satırı basımda da açar | Fora `V15_YeniEvrakAciklamaKaydet` + canlı |
| Fatura uuid | Mikro ekranından kesilen **satış ve iade faturalarında** `cha_uuid` = büyük harfli 36 karakter GUID (kolon 40), benzersiz; tahsilat satırlarında `''`. Writer faturada yeni GUID yazar (ofis e-belgeye çevirirken Mikro bunu kullanır) | canlı |
| İskonto zinciri | `sth_isk_mas1=0` (1. iskonto brüt tutar üzerinden), `sth_isk_mas2..10=1` (bir öncekinden kalan tutar üzerinden). Örnek: 4800 brüt, iskonto1 2400 (%50), iskonto2 240 (kalan 2400'ün %10'u). Tutarlar `sth_iskonto1..6`'da **tutar** olarak. Siparişte karşılığı `sip_iskonto1..6` (alt çizgisiz) uygulama şekli, `sip_iskonto_1..6` tutar | canlı (en yaygın desen) + Fora `Iskonto_n_UygulamaSekli` |

## 2. Satış faturası — açık hesap (veresiye)

**`CARI_HESAP_HAREKETLERI` — tek başlık satırı (`cha_satir_no=0`)**

| Kolon | Değer |
|---|---|
| `cha_evrak_tip` | **63** (SatisFaturasi) |
| `cha_tip` | **0** (borç) |
| `cha_cinsi` | **6** ToptanFatura (perakendede 7) |
| `cha_normal_Iade` | 0 |
| `cha_tpoz` | **0** (açık) |
| `cha_cari_cins` / `cha_kod` | 0 / **müşteri kodu** |
| `cha_ciro_cari_kodu` | müşteri kodu |
| `cha_kasa_hizmet` / `cha_kasa_hizkod` | 0 / boş |
| `cha_aratoplam` | satırların `sth_tutar` toplamı (brüt, iskontosuz, KDV hariç) |
| `cha_ft_iskonto1..6` | satırların `sth_iskonto1..6` toplamları |
| `cha_vergi1..5` | satır KDV'leri **pointer'a göre** kovalanır (Fora: pntr 2→vergi2, 3→vergi3, 4→vergi4, 5→vergi5, diğer→vergi1) |
| `cha_meblag` | `aratoplam − iskontolar + masraflar + vergiler` |
| `cha_uuid` | yeni GUID (§1) |
| `cha_vade` | 0 ya da ödeme planı no (canlıda 0) |
| `cha_satici_kodu`, `cha_srmrkkodu`, `cha_projekodu` | temsilci / sorumluluk merkezi / proje |

Canlı örnek (Mikro ekranı, 5808): `aratoplam 15600`, `ft_iskonto1 3120`, `meblag 12480`.

**`STOK_HAREKETLERI` — kalem başına bir satır**

| Kolon | Değer |
|---|---|
| `sth_evraktip` | **4** (CikisFaturasi) |
| `sth_tip` | **1** (çıkış) |
| `sth_cins` | 0 Toptan (perakendede 1) |
| `sth_normal_iade` | 0 |
| `sth_cari_cinsi` / `sth_cari_kodu` | 0 / müşteri kodu |
| `sth_fat_recid_dbcno` / `sth_fat_recid_recno` | 0 / **başlığın `cha_RECno`'su** |
| `sth_stok_kod`, `sth_miktar`, `sth_miktar2` (=miktar), `sth_birim_pntr` | kalem |
| `sth_tutar` | **brüt** = liste birim fiyatı × miktar |
| `sth_iskonto1..6` | iskonto **tutarları** (canlıda iskonto1 = %20) |
| `sth_vergi_pntr` / `sth_vergi` | stok kartı pointer'ı / KDV tutarı |
| `sth_giris_depo_no` = `sth_cikis_depo_no` | depo |
| `sth_plasiyer_kodu` | temsilci (canlıda boş) |
| `sth_adres_no` | 1 |
| `sth_fiyat_liste_no` | fiyat listesi sıra no |
| `sth_isk_mas1..10` | 1 (canlıda; Fora'daki değer Y0b'de doğrulanacak) |

## 3. Satış faturası — peşin (kapalı)

Mikro ve Fora peşin satışı **ayrı tahsilat makbuzuyla değil, faturayı kasaya/bankaya kapatarak** yazar
(Fora `kapamasekli = KasadanKapanacak / BankadanKapanacak`). Stok satırları §2 ile aynıdır.

| Başlık kolonu | Açık hesaptan farkı |
|---|---|
| `cha_cari_cins` | **4** kasa (nakit) · **2** banka (kart/havale) |
| `cha_kod` | **kasa kodu** (ör. `001`) / banka kodu |
| `cha_grupno` | kasa **0** · banka **1** (canlı: bankaya kapalı faturaların tamamı) |
| `cha_ciro_cari_kodu` | **müşteri kodu** (stok satırlarının `sth_cari_kodu`'su da müşteri) |
| `cha_tpoz` | **1** (kapalı) |
| `cha_aciklama` | Fora: müşteri unvanı (40 karakter) |

Canlıda: `63/0/6 cc=4 tpoz=1` → **4.851** kayıt (sahadan `T`, `H`… serileri), `cc=2 tpoz=1` → 57 kayıt.
**Sonuç:** kapalı faturada müşterinin cari ekstresinde borç/alacak hareketi oluşmaz; para doğrudan kasaya/bankaya girer.
Mikro'nun cari bakiyesi yalnız `cha_cari_cins=0` hareketlerinden oluşur. ErpBridge okuyucusu kapalı faturanın müşterisini
`cha_ciro_cari_kodu`'dan almalı ve bakiyeye katmamalı (Y0e).

## 4. Satış iadesi faturası

Mikro, satıştan iadeyi bir **giriş (alış) faturası** olarak, iade bayrağıyla yazar.

| Tablo | Kolon | Değer |
|---|---|---|
| CHA | `cha_evrak_tip` | **0** (AlisFaturasi) |
| CHA | `cha_tip` | **1** (alacak) |
| CHA | `cha_cinsi` | 6 |
| CHA | `cha_normal_Iade` | **1** |
| CHA | `cha_tpoz`, `cha_cari_cins`, `cha_kod` | 0, 0, müşteri |
| STH | `sth_evraktip` | **3** (GirisFaturasi) |
| STH | `sth_tip` | **0** (giriş) |
| STH | `sth_normal_iade` | **1** |
| STH | `sth_iade_evrak_seri/sira` | Fora boş bırakır (orijinal fatura bağlanmıyor) |

Canlıda: CHA `0/1/6 iade=1` → 306 kayıt, STH `3/0 iade=1` → 1.935 satır.

**Fora'daki ikinci yol — iadeli satış faturası:** aynı satış faturasında iade kalemleri (`SatisFaturasiIade`) varsa aynı
seri/sırada **ikinci CHA satırı** (`cha_satir_no=1`, `cha_evrak_tip=0`, `cha_tip=1`, `cha_normal_Iade=1`) yazılır; iade
kalemleri `sth_evraktip=3`, `sth_tip=0`, `sth_normal_iade=1` olur ve bu ikinci başlığa bağlanır.

## 5. Tahsilat makbuzu

Tek evrak (seri + sıra), **ödeme yöntemi başına bir satır** (`cha_satir_no` 0,1,2…). Canlıda karışık yöntemli makbuz var
(1904: senet + çek; 1871: nakit + havale).

**Tüm satırlarda:** `cha_evrak_tip=1` (TahsilatMakbuzu), `cha_tip=1` (alacak), `cha_normal_Iade=0`, `cha_tpoz=0`,
`cha_cari_cins=0`, `cha_kod=müşteri`, `cha_meblag=cha_aratoplam=tutar`, `cha_vade=yyyymmdd` (int).

| Yöntem | `cha_cinsi` | `cha_kasa_hizmet` / `hizkod` | `cha_sntck_poz` | `cha_karsidgrupno` | `cha_trefno` | `ODEME_EMIRLERI` |
|---|---|---|---|---|---|---|
| Nakit | **0** | 4 kasa / kasa kodu (`001`) | 0 | 0 | boş | yok |
| Kredi kartı | **19** | 2 banka / banka kodu (`14`) | 2 | **7** | `MK-fff-sss-yyyy-nnnnnnnn` | `sck_tip=6`, `sck_sonpoz=2`, `sck_nerede_cari_cins=2`, `sck_nerede_cari_kodu=banka`, `sck_nerede_cari_grupno=7` |
| Havale/EFT | **17** | 2 banka / banka kodu (`04`) | 2 | **9** | `MH-…` | `sck_tip=4`, `sck_sonpoz=2`, nerede cins 2 / banka / grupno 9 |
| Müşteri çeki | **1** | 4 kasa / **çek portföy kasası** (`ÇEK`) | 0 | 0 | `MC-…` | `sck_tip=0`, `sck_sonpoz=0`, nerede cins 4 / `ÇEK` / 0, `sck_no`=çek no |
| Müşteri senedi | **2** | 4 kasa / **senet portföy kasası** (`SENET`) | 0 | 0 | `MS-…` | `sck_tip=1`, `sck_sonpoz=0`, nerede cins 4 / `SENET` / 0, `sck_duzen_tarih` |

`ODEME_EMIRLERI` ortak: `sck_refno=cha_trefno`, `sck_tutar`, `sck_vade`, `sck_doviz=0`, `sck_doviz_kur=1`,
`sck_sahip_cari_cins=0`, `sck_sahip_cari_kodu=müşteri`, `sck_borclu` (müşteri unvanı, 30), `sck_vdaire_no`
(vergi dairesi + no, 40), `sck_ilk_hareket_tarihi = sck_son_hareket_tarihi = evrak tarihi`,
`sck_ilk_evrak_seri/sira_no/satir_no` = makbuzun seri/sıra/satırı, `sck_imza` (banka no doluysa 1), çekte
`sck_bankano`, `sck_banka_adres1`, `sck_sube_adres2`, `sck_hesapno_sehir`, `Sck_TCMB_*`.

**Referans no:** `MC/MS/MK/MH` + `-` + firma no (3 hane) + `-` + şube no (3) + `-` + yıl + `-` + **8 haneli** sıra.
Sıra = aynı önek + firma + şube + yıl içindeki en büyük no + 1 (Fora `TahsilatSonRefNoBul`; `sck_tip` ile filtreler).
Fora'da havale tahsilatı yok; havale satırı **Mikro'nun kendi kaydından** alındı.

**Çek açıklaması (Fora):** açıklama boşsa `çek no / banka adı (8) / banka no (23)` (canlı: `/27703///`).

## 6. Canlı veride görülen sapmalar (başka bir yazıcıdan)

| Kayıt | Mikro'nun kendi kaydı | Sahadan gelen kayıt |
|---|---|---|
| Kart tahsilatı (`T` serisi, 130 satır) | `cha_karsidgrupno=7`, ödeme emri `sck_sonpoz=2`, `grupno=7` | `karsidgrupno=0`, ödeme emri `sonpoz=0`, `grupno=0` |
| Kart referans no | `MK-000-000-2026-00000126` (8 hane) | `MK-000-000-2026-00077`, `-54074` (dolgusuz) — Mikro'nun 8 haneli `LIKE` aramasına girmez |
| Havale (`T`/`ST`) | `karsidgrupno=9` | `karsidgrupno=0` |
| Kapalı fatura (`T`, `H`, `ST`) | — | `cha_uuid` boş (e-belge oluşturulmamış) |

Yeni writer bu sapmaları **kopyalamaz**; Mikro'nun kendi kaydını esas alır.

## 7. Satış siparişi ve irsaliye (Fora değerleri — firmada canlı örnek yok)

**`SIPARISLER` (Fora `Evrak.cs` sipariş satırı, satır başına bir kayıt):** `sip_tip=0`, `sip_cins=0`, `sip_satirno` 0'dan,
`sip_tarih`, `sip_teslim_tarih` (sevk/teslim tarihi), `sip_belgeno`/`sip_belge_tarih`, `sip_satici_kod`, `sip_musteri_kod`,
`sip_stok_kod`, `sip_b_fiyat` (brüt birim fiyat), `sip_miktar`, `sip_birim_pntr`, `sip_tutar = sip_b_fiyat × miktar`,
`sip_iskonto_1..6` (tutar × miktar), `sip_vergi_pntr`, `sip_vergi` (iskontolu net KDV), `sip_masvergi_pntr=4`, `sip_opno`
(ödeme planı), `sip_teslimturu`, `sip_aciklama`, `sip_depono`, `sip_cari_grupno=0`, `sip_doviz_cinsi/kuru`,
`sip_alt_doviz_kuru`, `sip_adresno`, `sip_iskonto1..6` (uygulama şekli), `sip_durumu` (StoktanSevkEdilecek),
`sip_fiyat_liste_no`, `sip_harekettipi` (Stok), `sip_cagrilabilir_fl` (parametre), `sip_teslim_miktar=0`,
`sip_kapat_fl=0`, onay: `sip_OnaylayanKulNo`. V15'te `sip_RECid_RECno = SCOPE_IDENTITY()`.

**Satış irsaliyesi (`STOK_HAREKETLERI`):** §2 kalem satırıyla aynı, farkları: `sth_evraktip=1` (ÇıkışIrsaliyesi),
`sth_fat_recid_* = 0` (henüz faturaya bağlı değil), `sth_malkbl_sevk_tarihi = sevk/teslim tarihi`, başlık CHA satırı yok.

## 8. Doğrulama testleri

`tests/ErpBridge.Erp.Mikro.Tests/Integration/MikroNativeDocumentConventionTests.cs` (salt okuma; yalnız
`ERPBridge_RUN_INTEGRATION=1` **ve** açıkça verilen `ERPBridge_MIKRO_WRITE_DB=MikroDB_V15_DEMO` ile çalışır — Docker
fikstürünün paylaşılan anahtarı tek başına bu testleri açmaz, PR #75 Codex bulgusu):
Mikro kayıtlarında NULL olmadığı; açık/kasaya kapalı/bankaya kapalı satış faturası ve satış iadesi başlık kodları;
fatura/iade kalem kodları ve iskonto zinciri bayrakları; 5 tahsilat yönteminin satır ve ödeme emri kodları; referans no biçimi.

`MikroDocumentWriteRunnerLiveTests` (Y3a; aynı iki anahtarla, yalnız izinli test kopyasında **yazar**): `ERPBT3` serisinde
tek `EVRAK_ACIKLAMALARI` satırlık deneme evrakı yazar, kendi satırlarını ve ledger kayıtlarını siler. Yazan tüm testler
`MikroWriteTestDatabase.Collection` koleksiyonunda sırayla çalışır.

> **Bağlantı notu:** bu PC'de `localhost` (paylaşılan bellek) ile async sorgular aralıklı "aktarım düzeyi hatası"
> verdi (mevcut `MikroSchemaContractTests` dahil, 3/3). `tcp:localhost` ile 5/5 geçti; testlerin varsayılanı `tcp:localhost`.

## 9. Firmanın kullanım biçimi (V15_02, 2026-09-17)
- `SIPARISLER`'de **1** satır var → firma sipariş kullanmıyor; satış doğrudan fatura.
- Satış faturası satırı 79.468, iade satırı 1.935, tahsilat satırı ~2.300.
- Sahadan gelen seriler: `T`, `H`, `ST`, `N`, `PZ`, `İS`, `D`, `P`, `ID`… (plasiyer/araç başına seri görünümünde).
- Veritabanına bugün (11:14) yeni kayıt girilmiş → **canlı kullanımda olan bir firma veritabanı**.

---

## 10. Alış faturası *(ERP yazım 2, Z0b — 2026-09-19)*

Kanıt: Fora `Evrak.cs` + `EvrakData.cs` (bu işi daha önce başarıyla yapan uygulama) **ve** canlı `MikroDB_V15_02`
(2024-01-01'den beri 718 açık alış faturası başlığı, örnekler 2025 kayıtlarından). İkisi **tam uyuşuyor**.

**`CARI_HESAP_HAREKETLERI` — tek başlık satırı (`cha_satir_no=0`)**

| Kolon | Değer | Kanıt |
|---|---|---|
| `cha_evrak_tip` | **0** (AlisFaturasi) | Fora `enum_cha_evrak_tip.AlisFaturasi=0`; canlı 718 satır |
| `cha_tip` | **1** (alacak — tedarikçiye borçlanıyoruz) | Fora `evraktipi==AlisFaturasi → cha_tip=Alacak`; canlı |
| `cha_cinsi` | **6** ToptanFatura (perakendede 7) | Fora `ticaretturu` switch; canlı `cinsi=6` |
| `cha_normal_Iade` | 0 | canlı |
| `cha_ticaret_turu` | 0 (yurt içi toptan) | canlı |
| `cha_cari_cins` / `cha_kod` | 0 / **tedarikçi kodu** (açık fatura) | canlı `caricins=0, kod=ÇINAR` |
| `cha_ciro_cari_kodu` | **tedarikçi kodu** — açık faturada da dolu | canlı (satışta müşteri kodu olduğu gibi) |
| `cha_tpoz` | **0** açık, **1** kapalı | canlı |
| `cha_aratoplam`, `cha_ft_iskonto1..6`, `cha_vergi1..5`, `cha_meblag` | satış faturasıyla **aynı** kovalama (§2) | Fora ortak kod yolu |
| `cha_uuid` | 36 karakter GUID (satış faturasındaki gibi) | canlı `uuidlen=36` |
| `cha_miktari` | 0 | canlı |

**Peşin alış = kapalı fatura** (satışın aynası, §3): `cha_cari_cins` **4** kasa / **2** banka, `cha_kod` = kasa/banka
kodu, `cha_ciro_cari_kodu` = **tedarikçi**, `cha_tpoz=1`. Canlı örnek: `kod=001, caricins=4, tpoz=1, ciro=POLEN GARDEN`.

**`STOK_HAREKETLERI` — kalem başına bir satır**

| Kolon | Değer | Kanıt |
|---|---|---|
| `sth_evraktip` | **3** (GirisFaturasi) | Fora `enum_sth_evraktip.GirisFaturasi=3`; canlı |
| `sth_tip` | **0** (giriş) | Fora; canlı |
| `sth_cins` | **0** Toptan (perakendede 1) | Fora; canlı |
| `sth_normal_iade` | 0 | canlı |
| `sth_cari_cinsi` / `sth_cari_kodu` | 0 / tedarikçi kodu | Fora `Carimiz`; canlı |
| `sth_giris_depo_no` / `sth_cikis_depo_no` | ikisi de depo no (canlıda 1/1) | canlı |
| `sth_fat_recid_recno` | başlık CHA satırının `cha_RECno`'su | canlı |
| `sth_tutar`, `sth_iskonto1..6`, `sth_isk_mas1..10`, `sth_vergi_pntr`, `sth_vergi` | satış faturası kalemiyle **aynı** kural (§2, §1 iskonto zinciri) | Fora ortak kod yolu |
| `sth_fiyat_liste_no` | 0 (alışta liste yok) | canlı |

**Sıra çakışması — dikkat:** alış faturası ve **satış iadesi aynı `cha_evrak_tip=0`'ı paylaşır**, yalnız
`cha_normal_Iade` (0 / 1) ile ayrılırlar; STH'de de ikisi `sth_evraktip=3`'tedir. Yani MAX+1 sırası **ikisini birlikte**
hesaplanmalıdır (Fora `Select MAX(cha_evrakno_sira) … WHERE cha_evrak_tip=0 AND cha_evrakno_seri=…` — iade filtresi yok).
Canlı veri bunu doğruluyor: 718 alış + 307 satış iadesi aynı numara uzayında.

**`EVRAK_ACIKLAMALARI`:** `egk_dosyano=51`, hareket tip **1**, evrak tip **0** (Fora `EvrakData.cs` switch).

---

## 11. Tediye makbuzu *(ERP yazım 2, Z0c — 2026-09-19)*

**Tahsilatın (§5) birebir aynası:** tek evrak, **ödeme yöntemi başına bir CHA satırı** (`cha_satir_no` 0'dan artar).
Ayrı kasa/banka satırı **yoktur** — hesap aynı satırda `cha_kasa_hizmet` + `cha_kasa_hizkod` ile taşınır.

| Kolon | Değer | Kanıt |
|---|---|---|
| `cha_evrak_tip` | **64** (TediyeMakbuzu; kasa tediye fişi 65) | Fora `enum_cha_evrak_tip`; canlı |
| `cha_tip` | **0** (borç — tahsilat alacaktı) | Fora `AddTahsilat`: `_evraktipi != Tahsilat → cha_tip = Borc`; canlı |
| `cha_cari_cins` / `cha_kod` | **0** (Carimiz) / cari kodu | Fora; canlı: 64'lü satırların **tamamı** `cari_cins=0` |
| `cha_kasa_hizmet` / `cha_kasa_hizkod` | **4** kasa / **2** banka + hesap kodu | Fora `cha_kasa_hizkod = kasa_banka_kodu`; canlı |
| `cha_normal_Iade` / `cha_tpoz` | 0 / **0** (açık) | Fora; canlı |
| `cha_vade` | vade günü `yyyyMMdd` sayı olarak | Fora |
| `cha_ft_iskonto*`, `cha_ft_masraf*`, `cha_vergi*`, `cha_yuvarlama` | 0 | Fora |

**`cha_cinsi` — tediyede "Firma" ailesi** (tahsilattaki "Müşteri" ailesinin karşılığı):

| cinsi | Anlam | `cha_kasa_hizmet` | Canlı örnek kod | Adet (2024+) |
|---|---|---|---|---|
| **0** | Nakit | 4 kasa | `001` | 423 |
| 1 | MüşteriÇeki (ciro edilen) | 4 kasa (portföy) | `ÇEK` | 30 |
| 2 | MüşteriSenedi (ciro edilen) | 4 kasa (portföy) | `SENET` | 70 |
| 3 | FirmaÇeki | 2 banka | `12`, `13` | 25 |
| 4 | FirmaSenedi | 4 kasa | `VERILEN-SENET` | 25 |
| **20** | **FirmaHavaleEmri** | 2 banka | `04`, `06`, `07` | 54 |
| 22 | FirmaKrediKartı | 2 banka | `08`, `10` | 60 |

Bu goal'ün kapsamı (D3) **nakit → `cinsi 0`, `kasa_hizmet 4`** ve **havale/EFT → `cinsi 20` (FirmaHavaleEmri),
`kasa_hizmet 2`**. Havalede tahsilatın `17` (MusteriHavaleSozu) değeri **kullanılmaz** — o gelen havaledir.

**`EVRAK_ACIKLAMALARI`:** dosya 51, hareket **0**, evrak tip **64** (Fora `EvrakData.cs` switch).

**`ODEME_EMIRLERI`:** nakit ve havalede yok (canlıda 64'lü nakit/havale satırlarında `cha_trefno` boş). Çek/senet
çıkışı bu goal'ün kapsamı dışında (D3), gerektiğinde §5'teki referans no kuralı örnek alınacak.

---

## 12. Cari kartı (yeni müşteri) *(ERP yazım 2, Z0d — 2026-09-19)*

Kaynak: Fora `CariExtensions.cs` (V15 INSERT, 95 kolon) + canlı `MikroDB_V15_02`'deki **524 cari**.

**Tekil indeks:** `NDX_CARI_HESAPLAR_02 (cari_kod)` — kod tek başına tekildir; ayrıca `(sektör|grup|temsilci|bölge, kod)`
bileşik tekil indeksleri var. Yani aynı kodla ikinci cari **veritabanı tarafından** reddedilir; writer çakışmayı
önceden görüp kalıcı `CUSTOMER_CODE_EXISTS` dönmelidir (D4).

**Alan genişlikleri:** `cari_kod` nvarchar(**25**), `cari_unvan1`/`cari_unvan2`/`cari_vdaire_adi` nvarchar(50).

**Bu firmanın kod alışkanlığı:** cari kodu **müşterinin adıdır** (`NEXT`, `LÜTFİ`, `EURO MARKET`, `BİZİM SÜPERMARKET`);
sayısal/öneki şema (`120.01.0001` gibi) **yok**, uzunluk 2–19 arasında dağılıyor. Plasiyerin kodu telefonda girmesi
(U2) firmanın bugünkü alışkanlığıyla uyumlu; Portal'da "kod öneki" ayarı bu firma için anlamsız (Z1c'de gözden geçir).

**Her caride aynı olan değerler** (524/524 — writer bunları sabit yazar):

| Kolon | Değer |
|---|---|
| `cari_fileid` | **31** |
| `cari_hareket_tipi` | 0 |
| `cari_doviz_cinsi` | 0 (TL) |
| `cari_doviz_cinsi1` / `cari_doviz_cinsi2` | **255** / **255** (tanımsız) — 524 carinin **522**'sinde; iki eski kartta 127. Writer 255 yazar (Mikro'nun bugünkü değeri) |
| `cari_vade_fark_yuz` | **25** |
| `cari_KurHesapSekli` | 1 |
| `cari_fatura_adres_no` / `cari_sevk_adres_no` | 1 / 1 |
| `cari_EftHesapNum` | 1 |
| `cari_RECid_DBCno` | **0** (524/524) — §1'deki V15 kimlik kuralı cari kartında da geçerli |
| `cari_odemeplan_no` | 0 |
| `cari_TeminatMekAlacakMuhKodu` / `...BorcMuhKodu` | **910** / **912** |
| `cari_VerilenDepozitoTeminatMuhKodu` / `cari_Alinan...` | **226** / **326** |

Son dört muhasebe kodu Fora'nın V16 INSERT'ünde de **sabit literal** olarak geçiyor (`'910','912','226','326'`) —
iki kaynak bağımsız olarak aynı değerleri veriyor.

**V15 kimlik (self-link):** Fora `CariExtensions.cs` INSERT'ün ardından
`UPDATE CARI_HESAPLAR SET cari_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE cari_RECno = (SELECT SCOPE_IDENTITY())`
çalıştırıyor — §1'deki genel kuralın cari kartındaki karşılığı. Canlıda `cari_RECid_DBCno=0` **524/524**,
`cari_RECid_RECno = cari_RECno` **522/524** (aynı iki eski kart `doviz_cinsi` sapmasını da gösterenler). Writer
bugünkü kuralı yazar: `DBCno=0` ve self-link.

**Karta göre değişen alanlar:**

| Kolon | Doluluk (524 caride) | Not |
|---|---|---|
| `cari_kod`, `cari_unvan1` | 524 | zorunlu |
| `cari_satis_fk` | 524 | **satış fiyat listesi no** — 1 (509), 2 (7), 3 (8); toplam 524, başka değer yok |
| `cari_bolge_kodu` | 420 | firma bölge kullanıyor (`ALANYA`, `BELEK`, `ADRASAN-OLİMPOS`) |
| `cari_vdaire_adi` / `cari_vdaire_no` | 264 | vergi dairesi (kurumsal müşterilerde) |
| `cari_CepTel` | 77 | |
| `cari_unvan2` | 39 | |
| `cari_baglanti_tipi` | 48 | |
| `cari_EMail` | 3 | |
| `cari_grup_kodu`, `cari_temsilci_kodu`, `cari_muh_kod`, `cari_sektor_kodu` | **0** | bu firma kullanmıyor — writer boş bırakır |

**V15/V16 farkı:** `cari_tipi` kolonu V15'te **yoktur** (Fora INSERT'ünde değişken kolon adıyla geçiliyor); V16'da
`cari_Guid`, `cari_efatura_fl`, `cari_def_efatura_cinsi` gibi kolonlar eklenir. Bu goal V15 yazıyor (U4).

## 13. Kasa masraf fişi (gider) *(ERP yazım 3, Y0a — 2026-09-19)*

**Fora ile canlı Mikro burada ayrışır.** Fora gideri `Masraf` sınıfıyla bir *hizmet faturası* olarak yazar
(`cha_evrak_tip = AlisFaturasi(0)`, `cha_cinsi = HizmetFaturasi(8)`, `Evrak.cs:5420-5460`). Firmanın kendi
Mikro ekranı ise **Kasa masraf fişi** yazar: `cha_evrak_tip = 37`. Fora'nın enum'unda `KasaMasrafFisi` tanımlı
ama Fora bu tipi **hiç yazmıyor** (tüm kaynakta tek kullanım yeri kaynak dosyasının kendisi). §6 kuralı gereği
**Mikro'nun kendi kaydı esastır → 37 yazılır.**

`enum_cha_evrak_tip` sayımı: `AlisFaturasi=0`, `TahsilatMakbuzu=1`, `SatisFaturasi=63`, `TediyeMakbuzu=64`
çapalarıyla doğrulandı; aynı sayımda `KasaMasrafFisi=37`. Canlı `MikroDB_V15_02`'de 151 adet 37'lik hareket var.

Gider, tediyenin **aynadaki hali değildir**: tediyede ödeyen hesap `cha_kasa_hizmet`/`cha_kasa_hizkod`'ta,
karşı taraf cari'dir. Giderde ise **gider kartı** `cha_kasa_hizmet`'tedir, ödeyen hesap `cha_cari_cins`/`cha_kod`'a geçer.

| Kolon | Değer | Kanıt |
|---|---|---|
| `cha_evrak_tip` | **37** (KasaMasrafFisi) | canlı 151 satır; Fora enum sayımı |
| `cha_tip` | **1** (alacak) | canlı |
| `cha_cinsi` | **0** (Nakit) — kasadan ödemede | canlı (151/151) |
| `cha_evrakno_seri` | **boş** | canlı: 151/151 seri boş |
| `cha_evrakno_sira` | seri içinde MAX+1 | canlı (ardışık) |
| `cha_cari_cins` | **4** (Kasamız) nakit; **2** (Bankamız) havale/kredi kartı | canlı 4; 2 için §11 kredi kartı deseni |
| `cha_kod` | ödeyen hesabın kodu (kasa kodu `001`, banka kodu `08`/`10`/`16`) | canlı |
| `cha_kasa_hizmet` | **5** (Giderimiz) | canlı; `enum_cha_cari_cins.Giderimiz=5` |
| `cha_kasa_hizkod` | **gider kartı kodu** (`MASRAF_HESAPLARI.his_kod`) | canlı: YAKIT, KİRA, AMBAR… |
| `cha_meblag`, `cha_aratoplam` | tutar (eşit) | canlı |
| `cha_vade` | evrak tarihi (yyyyMMdd) | canlı |
| `cha_d_kur`, `cha_altd_kur`, `cha_karsid_kur` | 1 | canlı |
| `cha_vergipntr`, `cha_vergi1` | KDV işaretçisi ve tutarı | canlı 0 (firma KDV'siz giriyor); telefon KDV gönderirse doldurulur |
| `cha_belge_no` | boş | canlı |
| **`EVRAK_ACIKLAMALARI`** | **satır yazılmaz** | canlı: `egk_evr_tip=37` için 0 kayıt (64 için 3, 0 için 347) |

Kredi kartı / havale ile ödemede `cha_cinsi` **§11'deki firma ailesinden** seçilir: havale **20**
(FirmaHavaleEmri), kredi kartı **22** (FirmaKrediKartı). Canlı tediyelerde `cinsi=22` satırlarının
`cha_kasa_hizkod`'u **banka kodudur** (`BANKALAR.ban_kod`; `FIRMA_KREDI_KARTI_TANIMLARI` tablosu boş),
yani kredi kartı Mikro'da bir banka hesabı üzerinden yürür.

**Gider kartları:** `MASRAF_HESAPLARI` (19 kart), anahtar `his_kod`, adı `his_isim`; ayrıca
`his_tipkod`, `his_sinifkod`, `his_grupkod`, `his_muhkod`, `his_birim_ad`. `HIZMET_HESAPLARI` bu firmada **boş**.

## 14. Sayım sonuçları *(ERP yazım 3, Y0b — 2026-09-19)*

Sayım cari/stok hareketi **değildir**: kendi tablosuna yazılır ve **stoğu kendiliğinden hareket ettirmez**.
Farkın stoğa işlenmesi Mikro'da ayrı bir "sayım sonuçlarını uygula/kesinleştir" işlemidir. Fora da
`SayimSonuclariGirisFisi` ile yalnız bu tabloya yazar.

Tablo: **`SAYIM_SONUCLARI`**, `sym_fileid = 28`.

| Kolon | Değer | Kanıt |
|---|---|---|
| `sym_tarihi` | sayım tarihi (saat 00:00) | canlı |
| `sym_depono` | depo no | canlı (1) |
| `sym_evrakno` | **int**, depo içinde MAX+1 | canlı: depo 1'de 1,2,3,5,6,17,21,23 |
| `sym_satirno` | 0'dan artan satır sırası | canlı (211…) |
| `sym_Stokkodu` | stok kodu | canlı |
| `sym_barkod` | barkod (yoksa stok kodu yazılmış) | canlı |
| `sym_miktar1` | sayılan miktar | canlı |
| `sym_birim_pntr` | 1 (ana birim) | canlı |
| `sym_reyonkodu`, `sym_koridorkodu`, `sym_rafkodu` | boş | canlı |
| `sym_miktar2..5`, `sym_renkno`, `sym_bedenno`, `sym_parti_kodu`, `sym_lot_no`, `sym_serino` | boş/0 | canlı |

Tabloda **kesinleştirme/uygulama izi tutan bir kolon yoktur** — yazılan fiş Mikro'da açılıp uygulanana
kadar yalnızca bir sayım kaydıdır. Canlıda 2948 satır / 8 fiş (01.10.2024 – 05.09.2026).

## 15. Alış faturasında evrak numarası *(ERP yazım 3, Y0c — 2026-09-19)*

Alış faturasında seri/sıra **bizim değil, tedarikçinin** numarasıdır: canlıda `cha_evrakno_seri`
tedarikçiye göre değişiyor (`ÇINAR`, `JUMBO`, `PEKER`, `CS`, `AYD`…) ve `cha_evrakno_sira` o serinin
kendi sırası olarak ilerliyor (ör. `CS` serisi 45927'de). 1025 alış faturasının 652'sinde seri dolu,
373'ü boş seride ardışık ilerliyor. `cha_belge_no` neredeyse hiç kullanılmıyor (44/1025).

Mikro'da **seri tanım tablosu yoktur** (`%SERI%`/`%SIRA%`/`%SAYAC%` aramasında yalnız
`STOK_SERINO_TANIMLARI` ve istasyon sayaçları çıkıyor). Yani "ERP'de tanımlı seri" pratikte
**veride kullanılmış seridir**: tedarikçinin daha önce kullandığı seri varsa o serinin MAX+1'i,
yoksa boş serinin MAX+1'i. §10'daki uyarı geçerli: alış faturası ile alış iadesi aynı numara
alanını paylaşır, MAX+1 `cha_normal_Iade`'ye göre **filtrelenmez**.
