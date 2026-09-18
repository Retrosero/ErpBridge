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

| Kolon | Değer | Kanıt |
|---|---|---|
| `cha_evrak_tip` | **64** (TediyeMakbuzu; kasa tediye fişi 65) | Fora `enum_cha_evrak_tip`; canlı 708 satır |
| `cha_tip` | **0** (borç — cariye ödüyoruz, tahsilatın tersi) | canlı |
| `cha_cari_cins` | 0 (cari satırı) | canlı: 64'lü satırların tamamı `cari_cins=0` |
| `cha_cinsi` | ödeme aracına göre: **0** nakit (432), **1** çek (33), **2** senet (70), ayrıca canlıda 3, 4, 20, 22 | canlı dağılım |
| `EVRAK_ACIKLAMALARI` | dosya 51, hareket **0**, evrak tip **64** | Fora `EvrakData.cs` switch |

Tahsilatın (§5) aynası: karşı taraf (kasa/banka) satırı ayrı `cha_satir_no` ile aynı evrakta yazılır. **Açık uç:**
canlıdaki `cha_cinsi` 3, 4, 20, 22 değerleri henüz çözülmedi; Z3b'den önce `enum_cha_cinsi`'den okunup buraya eklenecek
(bu goal'ün kapsamı nakit ve banka olduğu için yazımı engellemiyor).

---
