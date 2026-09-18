# P0f — Parametre kataloğu gözden geçirme raporu

Tarih: 2026-09-18 · Kaynak: `catalog/parameters/` · Üretici: `tools/ForaCatalogExtractor`
Goal: [GOAL_PARAMETRE_YONETIMI.md](GOAL_PARAMETRE_YONETIMI.md)

P0 fazının kapanış raporu. Katalog çıkarımı bitti; bu belge **neyin elde olduğunu, neyin
eksik kaldığını ve sonraki fazların hangi kararları vermesi gerektiğini** tek yerde toplar.

---

## 1. Kapsam: ne çıkarıldı

`catalog/parameters/defaults.json` — **21 set, 14 program, 4.713 tanım** (2 kaynak dosyadan).

| Katalog seti | Program | Tanım | Farklı ad | Yerleşen | Editör düzeni |
|---|---|---:|---:|---:|---|
| `MobilKullanici` | `akilli` | 1801 | 1796 | 1782 | `ui/akilli.json` |
| `ForaMikroKullanici` | `foramikro` | 60 | 60 | 60 | `ui/foramikro-kullanici.json` |
| `ForaMikro` | `ForaMikro` | 35 | 35 | 32 | `ui/foramikro-genel.json` |
| `alanekle` | `YaziciAyarlari` | 16 | 16 | 14 | `ui/yaziciayarlari.json` |
| `genelayarlaritanimla` | `YaziciAyarlari` | 9 | 9 | 8 | `ui/yaziciayarlari.json` |
| `ComarchEdiGenelParametreler` | `ComarchEdiGenel` | 14 | 14 | 14 | `ui/comarchedi-genel.json` |
| `B2B` | `b2b` | 11 | 11 | 11 | `ui/b2b.json` |
| `ComarchEdiIliskiParametreleri` | `ComarchEdiIliski` | 7 | 7 | 7 | `ui/comarchedi-iliski.json` |
| `MobilRaporStokSatis` | `MobilRaporStokSatis` | 2 | 2 | 2 | `ui/mobilrapor-stoksatis.json` |
| `MobilRaporStokEnvanter` | `MobilRaporStokEnvanter` | 2 | 2 | 2 | `ui/mobilrapor-stokenvanter.json` |
| `MobilRaporStokSiparis` | `MobilRaporStokSiparis` | 2 | 2 | 2 | `ui/mobilrapor-stoksiparis.json` |
| `MobilRaporYapilacakTahsilatlar` | `MobilRaporYapilacakTahsilatlar` | 2 | 2 | 2 | `ui/mobilrapor-yapilacaktahsilatlar.json` |
| `GenelAktarimTxtCsvSablon` | `GenelAktarim` | 863 | 863 | — | **P6** |
| `GenelAktarimSqlSablon` | `GenelAktarim` | 623 | 623 | — | **P6** |
| `TahsilatAktarimTxtCsvSablon` | `TahsilatAktarim` | 436 | 436 | — | **P6** |
| `BankaAktarim` | `BankaAktarim` | 424 | 424 | — | **P6** |
| `TahsilatAktarimSqlSablon` | `TahsilatAktarim` | 313 | 313 | — | **P6** |
| `BankaAktarimGenel` | `BankaAktarim` | 48 | 48 | — | **P6** |
| `BankaAktarimKriter` | `BankaAktarim` | 31 | 31 | — | **P6** |
| `GenelAktarimKriter` | `GenelAktarim` | 7 | 7 | — | **P6** |
| `TahsilatAktarimKriter` | `TahsilatAktarim` | 7 | 7 | — | **P6** |

P0'ın borcu olan **varsayılan katalog 21 setin tamamı için tam**. Aktarım setlerinin *editör
düzeni* (sekme/etiket/tip) P6'ya bırakıldı; panel ekranları orada yapılacak.

---

## 2. Fora'da bulunan dört kusur

İnceleme sırasında Fora'nın kendi kodunda dört kusur çıktı. Üçü veri kaybettiriyor. Hiçbiri
atılmadı: katalogda işaretlendi, sonraki fazlar bilerek karar versin diye.

| # | Nerede | Ne | Sonuç |
|---|---|---|---|
| **K1** | `TahsilatAktarimTxtCsvSablon` | `belge_tarihi_yil/ay/gun_baslangic` üçü de `ParametreID=16`; aynısı 17 (`*_uzunluk`) ve 604 (`cari_kod2_*`) için. Genel şablon aynı alanlarda doğru şekilde 16/18/20 kullanıyor | `_GetParametre(int)` ilk eşleşmeyi döndürdüğü için **5 kayıt Fora'da da okunamıyor**. `duplicateIds` + `shadowed` ile işaretli |
| **K2** | `ForaAndroidKullaniciDuzenleme` | `GosterZyrt_Temsilci_Ozel_24…94_Var_Yok` kutucukları `…_12…19_Var_Yok` parametrelerini gösteriyor ama kendi parametrelerine kaydediyor | **Fora'da o ekranı açıp kaydetmek 8 parametreyi başkalarının değeriyle sessizce eziyor.** `bindingMismatch` ile işaretli |
| **K3** | `MobilKullanici` | 5 parametre adı iki farklı ID ile tanımlı (639–643 ve 644–648, aynı varsayılanlarla) | `_GetParametre(string)` ilk eşleşmeyi döndürüyor; ikinci kopyalar hiç okunamıyor, varsayılanlarında kalıyor. **1.801 kayıt = 1.796 kullanılabilir ad.** `duplicateNames` ile işaretli |
| **K4** | `MobilKullanici` + `ForaMikro` | 13 parametrenin Fora'nın kendi editöründe karşılığı yok | Fora'da yalnız veritabanından elle değiştirilebiliyorlar. `notInEditor` ile işaretli |

### K4'ün listesi

`Vergi0KisaAdi`, `Vergi0UzunAdi`, `Vergi0Yuzde` (hem `akilli` hem `ForaMikro` editöründe yok —
Fora "Tanımsız" KDV satırını hiçbir yerde göstermiyor) · `HakGormeCariAdresler` ·
`GormeCariUnvan` · `KullaniciEtkinlikleriniKayitEt` · `StokEklemeArtiEksiButonlariGoster` ·
`SiparisKarsilamaFotoGenislik` · `SiparisKarsilamaFotoYukseklik` ·
`EvrakSeri_KonsinyeIrsaliyesi` · `EvrakSeri_KonsinyedenIadeIrsaliyesi` ·
`Goster_AnaMenu_Konsinye_Irsaliyesi` · `Goster_AnaMenu_Konsinyeden_Iade_Irsaliyesi`

**Karar gerekiyor (P2):** bunlar panelde gösterilsin mi? Fora'da gösterilmemeleri bir kusur
olabileceği gibi kasıtlı da olabilir (konsinye evrakları mobilde hiç uygulanmamış olabilir).
Öneri: **gösterilsinler**, "Fora editöründe yok" rozetiyle — panel Fora'nın eksiğini kapatır.

### Boşluk dökümü

Hiçbir parametre sessizce kaybolmuyor; çıkarılamayan her şey `gaps` altında listeleniyor.

| Tür | Kayıt | Ne demek |
|---|---:|---|
| `notInEditor` | 16 | Katalogda var, Fora'nın editöründe yok. **13 farklı parametre** — `Vergi0KisaAdi/UzunAdi/Yuzde` üçü hem `akilli` hem `ForaMikro` editöründe eksik olduğu için iki kez sayılıyor |
| `bindingMismatch` | 8 | K2 — ezilen parametreler (`GosterZyrt_Temsilci_Ozel_24…94_Var_Yok`) |
| `unboundParameter` | 7 | Formda okunuyor ama bir kontrole bağlanmıyor: `akilli.Sifre`; `comarchedi-iliski`'nin okuduğu 3 bağlantı ayarı (`KullaniciAdi`, `Sifre`, `ZamanAsimi` — kendi editörleri var); `yaziciayarlari`'nın 3 iç alanı (`Isim`, `SablonAdi`, `VeriTipi`) |
| `noLabel` + `noTab` | 4 + 4 | Dört mobil rapor ekranındaki `RaporJson` — Fora'nın kendi ekranıyla düzenlenen `composite` değer, ne etiketi ne sekmesi var |

---

## 3. Panel tasarımını doğrudan etkileyen bulgular

### 3.1 `akilli`'nin yarısı tek bir tekrar eden yapı

1.782 alanın **830'u** (%47) "Parametreler / Ziyaret anket" sekmesinde ve düz bir liste değil:

- **100 soru × 8 parametre** = 800. Her soru için 4 cevap tipi (`Var_Yok`, `Evet_Hayir`,
  `Derece`, `Metin`) ve her tip için bir görünürlük bayrağı (`GosterZyrt_…`) + bir metin
  (`MetinZyrt_…`).
- Artı 30 sabit faaliyet bayrağı (`GosterZyrt_Satis_Yapildi`, `MetinZyrt_Siparis_Alindi` …).

**Panel bunu 830 ayrı alan olarak çizmemeli.** 100 satırlık bir tablo: her satır bir soru,
sütunlar cevap tipleri. P2b bunu özel olarak ele almalı, yoksa ekran kullanılamaz olur.

Bu çıkarıldığında `akilli`'nin gerçek "farklı ayar" sayısı ~950'ye iniyor.

### 3.2 Sekme dağılımı

| Dış sekme | Alan |
|---|---:|
| Parametreler | 978 (830'u ziyaret anketi) |
| Evrak girişi | 404 |
| Görünüm ve seçenekler | 328 |
| Raporlar | 64 |
| Form dosyaları | 8 |

### 3.3 Editör tipleri

`boolean` 965 · `text` 734 · `integer` 117 · `choice` 63 · `decimal` 29 · `reference` 11 ·
`color` 6 · `secret` 5 · `composite` 5 · `multilineText` 1. **`unknown` yok.**

**`reference` (11)** iki sinyalden çıkarıldı: Fora'nın kendi seçici kontrolleri (`CariSecimi`,
`DepoSecimi`, `KargoSecimi`, `EkipKoduSecimi`) **ve** `DataSource`'u bir ERP veri sınıfına inen
combo'lar (`DepoData.GetDepolarDataTable` → 5 depo alanı, `MikroKullaniciData` → 2 kullanıcı
alanı). İkincisi olmadan o 7 alan sıradan `choice` görünüyordu ve panel geçersiz kod girilmesine
izin verirdi.

**`choice` (63):** hepsinin seçenek listesi kaynaktan çıkarıldı (`options`), çünkü Fora onları
kod içinde `new DataTable { Rows = { {0,"Telefon"}, … } }` ile kuruyor. Panelin liste uydurması
gerekmiyor.

**`secret` (5):** Fora'nın maskelediği 3 alan + adı kimlik bilgisi söyleyen 2 alan
(`comarchedi-genel.Sifre`, `..._SqlSifre` — Fora bunları **maskelemiyor**). Hangi sinyalden
geldiği `secretSource` ile kayıtlı.

**`color` (6) renk değil, renk *kanalı* — P2c'de fark edildi.** Fora `ColorPickEdit` kullanıyor
ama her parametre **tek bir 0–255 kanalı** saklıyor: `SiparisKarsilamaCariColorRed/Green/Blue`
üçü birlikte bir rengi veriyor (varsayılanlar `85`, `125`, `172` gibi). Panelde renk seçici
çizmek `#aabbcc` yazardı ve telefon o alanı sayı olarak okuyor; doğrusu kanal başına 0–255
sayı kutusu. Altı alanın hepsi bu kalıpta.

**Yine de D12 için bu yeterli değil.** `akilli` tarafında kasa kodu, depo no, fiyat listesi no,
ödeme planı gibi alanlar Fora'da **düz `TextEdit`/`SpinEdit`** olarak duruyor — Fora onları elle
yazdırıyor, hiçbir listeye bağlamıyor, dolayısıyla çıkarılacak sinyal yok. Örnek:
`DefaultNakitKasaKodu`, `DefaultKaynakDepoNo`, `DefaultFiyatListeNo`, `DefaultProjeKodu`,
`DefaultSorumlulukMerkeziKodu`.

**Ad kalıbı tek başına yetmiyor — denendi.** `*KasaKodu`, `*DepoNo`, `*FiyatListeNo`,
`*ProjeKodu`, `*SorumlulukMerkeziKodu`, `*OdemePlani*`, `*FirmaNo`, `*SubeNo`,
`*CariPersonelKodu` kalıpları 40 alan yakalıyor, ama **11'i referans değil yetki bayrağı**:
`HakDegistirebilmeKasaKodu` "kasa kodunu değiştirebilir" demek, bir kasa kodu değil. Kör bir ad
kuralı bunları seçici yapardı.

Doğru kural **ad kalıbı + editör tipi bool değil** → **29 aday**:

| Referans | Aday | Örnek |
|---|---:|---|
| depo | 9 | `DefaultKaynakDepoNo`, `DefaultHedefDepoNo`, `DefaultNakliyeDepoNo`, `DepolarArasiSiparisKaynakDepoNo`, `DepolarArasiSiparisHedefDepoNo`, `StokListelemeDepoNo`, `StokFiyatGorKameraDepoNo`, `EvrakIcindeStokDepoNo`, `DepoNo` |
| fiyatListesi | 4 | `DefaultFiyatListeNo`, `StokListelemeIkinciFiyatListeNo` |
| kasa | 4 | `DefaultNakitKasaKodu`, `DefaultSenetKasaKodu` |
| odemePlani | 3 | `YeniCariVarsayilanOdemePlani` |
| firma / sube | 2 + 2 | `DefaultMikroFirmaNo`, `DefaultMikroSubeNo` |
| proje | 2 | `DefaultProjeKodu` |
| sorumlulukMerkezi | 2 | `DefaultSorumlulukMerkeziKodu` |
| cariPersonel | 1 | `CariPersonelKodu` |

Bunlardan biri tekil referans bile değil: `GosterilmeyecekOdemePlaniNolari` virgülle ayrılmış
**çoklu seçim**. Aynı desen `GosterilmeyecekDepoNolari`, `GosterilmeyecekProjeKodlari`,
`GosterilmeyecekSorumlulukMerkeziKodlari`, `GosterilmeyecekFiyatListesiNolari` için de geçerli.

**Karar gerekiyor (P2c/P3f):** bu 29 alan elle gözden geçirilip referans türü atanmalı;
`Gosterilmeyecek*` ailesi çoklu seçim olarak işaretlenmeli. Otomatik türetip körlemesine
güvenmek yanlış olur.

**`composite` (4):** rapor tanımı JSON'u (`RaporJson`). Fora'da kendi ekranı var; panelde
metin kutusu gibi göstermek yanlış olur, ayrı editör gerekir (P5e).

### 3.4 Parametre adı tek başına anahtar değil

3.365 farklı adın **862'si** birden fazla sette geçiyor. En kalabalık örtüşmeler aktarım
şablonları arasında (392 ad `GenelAktarimSql`+`TxtCsv`, 221 ad dört aktarım setinde birden),
ama `akilli` ile de çakışmalar var: 33 ad `ForaMikro` ile, 7 ad `ComarchEdiGenel` ile ortak
(`Sifre`, `ProjeKodu`, `SorumlulukMerkeziKodu`, `EvrakSeri` …).

**P1 için bağlayıcı — kısaltmayın.** Değer tablosu ve API **D3'ün tam anahtarını** kullanmalı:
`(TenantId, ErpCompanyId, Program, ScopeKind, ScopeId, AnaGrubu, AltGrubu, ParametreID)`.

`(program, ParametreID)` çifti **tekil değil**: gölgesiz 3.679 çiftin **994'ü** birden fazla
parametreyi gösteriyor. Örnek: `BankaAktarim` programında `ParametreID=1`, üç ayrı seti birden
adresliyor — `AktarimSablon`, `GenelSablon` ve `Kriter` (`AnaGrubu` ile ayrışıyorlar). Bu çiftle
tohumlamak ya da sorgulamak **başka bir parametrenin üzerine yazar veya yanlışını döndürür**.

Ölçüldü: `(Program, AnaGrubu, AltGrubu, ParametreID)` tekil — yani `AnaGrubu`/`AltGrubu` anahtarın
zorunlu parçası, kapsam kolonları da (D3, D4b) ayrıca gerekli.

---

## 3.5 Uygulama envanteri (D16) — bugün hiçbiri

D16, panelin "bu sürümde etkisiz" rozetini gösterebilmesi için her parametrenin Sipariş Cepte'de
gerçekten uygulanıp uygulanmadığını bilmesini istiyor. Bu raporun tespiti:

**Bugün hiçbir parametre uygulanmıyor.** Sipariş Cepte `GET /api/v1/android/parameters` ucunu
**hiç çağırmıyor** (P0'ın açılış bulgusu); kendi `app_settings` tablosunda yalnız evrak numaratörü
anahtarları var. Yani başlangıç envanteri boş ve panelin her alanı rozetle göstermesi gerekiyor.

| Öbek | Parametre | Durum | Hangi faz uygulayacak |
|---|---:|---|---|
| `Goster_AnaMenu_*` | 43 | uygulanmıyor | P4b |
| `Hak*` | 71 | uygulanmıyor | P4c |
| `EvrakSeri_*` + `Default*` | 51 | uygulanmıyor | P4d |
| `SenkronizeEt_*` | 44 | uygulanmıyor | P4e |
| Kalan `akilli` öbekleri | 1.573 | uygulanmıyor | P5 |
| `foramikro`, `ForaMikro`, `YaziciAyarlari`, `b2b`, `ComarchEdi*`, `MobilRapor*` | 154 | uygulanmıyor | P5h, P6d |
| Aktarım setleri | 2.752 | uygulanmıyor | P6a–P6c |

**Taşıyıcı alan:** `parameter_catalog_entries.IsImplemented`, varsayılanı `false` (P1a). Katalog
değil **ürün** bilir, bu yüzden yeniden tohumlamada korunuyor; bir özellik indikçe ilgili faz onu
`true`'ya çeker. Panel `false` olanı "bu sürümde etkisiz" rozetiyle gösterir.

---

## 4. Düşük güvenli noktalar (insan gözü isteyen)

| # | Konu | Neden şüpheli | Öneri |
|---|---|---|---|
| G1 | Fora sürümü | Decompile edilen `Fora Mikro.exe`'nin sürümü bilinmiyor; katalog `sourceBuild: "unknown"` | Müşteri kurulumundaki sürüme bakılıp `--source-build` ile damgalanmalı (DURUM > B1) |
| G2 | Etiket eşleştirmesi | 387 alanın etiketi konum yakınlığıyla bulundu (`labelLeft`), 21'i komşu kutucuğun başlığından (`checkBoxLeft`). Yoğun ekranlarda yanlış etiket kapmış olabilir | Panel ilk çalıştığında sekme sekme göz gezdirilmeli; `labelSource` alanı hangi alanların şüpheli olduğunu söylüyor |
| G3 | Okuma sırası | Tablo düzenindeki alanlar için konum yaklaşık hesaplandı (satır×24, sütun×120 piksel) | Yalnız sıralama için kullanılıyor; yanlışsa alanların sırası bozulur, değeri değil |
| G4 | ~~`choice` seçenekleri~~ | **Kapandı.** 63 alanın hepsinin seçenekleri kaynaktan çıkarıldı | — |
| G5 | K4 listesi | 13 parametrenin Fora'da editörü yok; kasıtlı mı kusur mu bilinmiyor | § 2'deki karar |

---

## 5. Sonraki fazlara devreden kararlar

| Karar | Faz | Özet |
|---|---|---|
| K4'teki 13 parametre panelde gösterilsin mi | P2 | Öneri: evet, "Fora editöründe yok" rozetiyle |
| Ziyaret anketi 100 satırlık tablo olarak çizilsin | P2b | 830 alanı düz liste yapmak ekranı kullanılamaz kılar |
| Referans türü parametre adından çıkarılsın | P2c / P3f | Kontrol tipi yalnız 4 alanda yetiyor; kasa/depo/fiyat listesi ad kalıbından |
| ~~`choice` seçenek listeleri~~ | — | Kapandı: 63 alanın seçenekleri katalogda |
| `secret` alanların değeri hiç taşınsın mı | P1 | 5 kimlik bilgisi; D6'nın `Sifre` için verdiği karar bunlara da genişletilmeli mi? |
| `composite` rapor editörü | P5e | Rapor tanımı JSON'u için ayrı ekran |
| K1–K3 kusurlarının taşınması | P3c | İçe aktarımda gölgelenmiş kayıtlar atlanmalı; K2'de hangi değerin doğru olduğu operatöre sorulmalı |
| Aktarım ekranlarının UI metadata'sı | P6 | ~3.500 alan; varsayılanları hazır |
