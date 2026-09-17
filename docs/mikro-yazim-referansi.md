# Mikro V15 — Satış, İade ve Tahsilat Evrakları Nasıl Yazılır

Tarih: 2026-09-17 · Kaynaklar: (1) `Fora_Mikro/.decompiled` — `Core/Fora.Mikro.Evraklar/Evrak.cs`
(`StokCariHesapHareketiOlustur`, tahsilat satırı, stok satırı), `DataSql/Fora.Mikro.Data.Sql/EvrakData.cs`
(`YeniNormalEvrakKaydet`, `V15_TahsilatOdemeEmriYaz`, `TahsilatSonRefNoBul`); (2) `localhost/MikroDB_V15_02`'de
**Mikro'nun kendi ekranından girilmiş** kayıtlar (seri boş, e-belge `cha_uuid` dolu olanlar) ve sahadan gelen `T/H/ST`
serili kayıtlar. İkisinin uyuştuğu yerler "doğrulandı", ayrıştığı yerler ayrıca belirtildi.

> Enum sayıları Fora enum'larının sıra değerleridir; canlı verideki değerlerle birebir karşılaştırıldı.
> Kolon listesi tam değildir — varsayılan (0 / boş / 1899-12-30) kalan kolonlar Y0b'de Fora INSERT listesinden eklenecek.

---

## 1. Ortak kurallar

| Konu | Kural | Kanıt |
|---|---|---|
| Kimlik (V15) | `*_RECno` identity; `*_RECid_DBCno=0`, `*_RECid_RECno = *_RECno` (aynı transaction'da UPDATE) | Fora INSERT + `UPDATE … SET *_RECid_RECno = SCOPE_IDENTITY()`; canlı veride eşit |
| Dosya no | `cha_fileid=51`, `sth_fileid=16`, `sck_fileid=54` | canlı |
| Kullanıcı | `*_create_user`, `*_lastup_user` = Mikro kullanıcı no (Fora: parametre `MikroUserNo`); tarih `getdate()` | canlı (1, 4) |
| Tarih | `cha_tarihi`, `cha_belge_tarih`, `sth_tarih`, `sth_belge_tarih`, `sth_malkbl_sevk_tarihi` saatsiz gün | canlı |
| Döviz | TL: `cha_d_cins=0`, `cha_d_kur=1`, `cha_altd_kur=1`, `cha_karsid_kur=1`; `sth_har_doviz_kuru=sth_alt_doviz_kuru=sth_stok_doviz_kuru=1` | canlı |
| Seri/sıra | Aynı evrakın tüm satırları aynı `evrakno_seri` + `evrakno_sira`; satırlar `cha_satir_no` / `sth_satirno` 0'dan artar. Sıra: seri + evrak türü içinde MAX+1; Fora yazmadan önce `EvrakVarMi` ile çakışma kontrolü yapar | Fora + canlı |
| KDV oranı | `dbo.fn_VergiYuzde(pntr)` → V15_02'de 1=%0, 2=%1, 3=%10, 4=%20, 5=%26. Stok kartındaki pointer `sto_toptan_vergi` / `sto_perakende_vergi`; bu firmada 4.567 stokun hepsi 1 (%0) | canlı |
| KDV dahil fiyat | Mikro fiyat listesi tanımında bayrak var: `STOK_SATIS_FIYAT_LISTE_TANIMLARI.sfl_kdvdahil` (V15_02'deki 3 listede 0) | canlı |
| Açıklama | `EVRAK_ACIKLAMALARI` — `egk_dosyano=51`; satış faturası `(hareket 0, evr_tip 63)`, iade/alış `(1, 0)`, tahsilat `(1, 1)`, tediye `(0, 64)`, irsaliye `dosyano 16` | Fora switch + canlı dağılım |

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
| `cha_ciro_cari_kodu` | **müşteri kodu** (stok satırlarının `sth_cari_kodu`'su da müşteri) |
| `cha_tpoz` | **1** (kapalı) |
| `cha_aciklama` | Fora: müşteri unvanı (40 karakter) |

Canlıda: `63/0/6 cc=4 tpoz=1` → **4.851** kayıt (sahadan `T`, `H`… serileri), `cc=2 tpoz=1` → 57 kayıt.
**Sonuç:** kapalı faturada müşterinin cari ekstresinde borç/alacak hareketi oluşmaz; para doğrudan kasaya/bankaya girer.

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

## 7. Firmanın kullanım biçimi (V15_02, 2026-09-17)
- `SIPARISLER`'de **1** satır var → firma sipariş kullanmıyor; satış doğrudan fatura.
- Satış faturası satırı 79.468, iade satırı 1.935, tahsilat satırı ~2.300.
- Sahadan gelen seriler: `T`, `H`, `ST`, `N`, `PZ`, `İS`, `D`, `P`, `ID`… (plasiyer/araç başına seri görünümünde).
- Veritabanına bugün (11:14) yeni kayıt girilmiş → **canlı kullanımda olan bir firma veritabanı**.
