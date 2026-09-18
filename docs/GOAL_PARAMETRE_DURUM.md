# Goal Durumu — Parametre Yönetimi

Son güncelleme: 2026-09-18 (P2b)
Görev listesi: [GOAL_PARAMETRE_YONETIMI.md](GOAL_PARAMETRE_YONETIMI.md)

> **Her görevden sonra, o görevin PR'ı içinde güncellenir.** Oturum kapanırsa buradan devam edilir.
> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı (kapı) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| P0 — Katalog çıkarımı (ön koşul) | 7 | 7 | ✅ |
| P1 — Merkez veri modeli ve API | 7 | 7 | ✅ |
| P2 — Panel (Admin) parametre ekranı | 8 | 2 | 🔄 |
| P3 — Ajan: Mikro aynası ve Fora içe aktarımı | 6 | 0 | ⬜ |
| P4 — Sipariş Cepte: öncelikli öbekler | 6 | 0 | ⬜ |
| P5 — Kalan `akilli` öbekleri | 8 | 0 | ⬜ |
| P6 — Aktarım şablonları ve entegrasyonlar | 4 | 0 | ⬜ |
| P7 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** P2c — editör bileşenleri

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| P00 | Plan dalını `main`'e al | ✅ | [#111](https://github.com/Retrosero/ErpBridge/pull/111) | **Codex 2 P1 bulgu, ikisi de kodda doğrulandı ve planda düzeltildi:** (1) değer anahtarında ERP firması yoktu — çok firmalı kiracıda bir firmanın depo/şube/seri ayarı diğerinin üzerine yazardı → D3b, `ErpCompanyId` değer/sürüm/API/ayna kapsamlarında zorunlu; (2) kullanıcı adı değişmez kimlik değil (`MobileSeatsRelationalTests.Deleting_a_user_releases_the_seat_and_the_username`: silinen satır geçmiş için kalıyor, aynı adla yeni kullanıcı açılabiliyor) — kullanıcı adına anahtarlamak silinenin `Hak*` yetkilerini devrederdi → D5 `MobileUser.Id`, D5b yalnız aktif kullanıcılar yayılır |
| P0a | `ParametrelerDefault.cs` → `defaults.json` (4.688 tanım) | ✅ | — | 19 set, 13 program, 4.688 tanım. **İki bulgu:** (1) kapsam her programda `ParametreUser`'da değil — aktarım şablonları `ParametreAltGrubu` kullanıyor, `ParametreAnaGrubu` sabit ayırıcı → D4b eklendi, katalog `scopeField` taşıyor; (2) `TahsilatAktarimTxtCsvSablon`'da **Fora'nın kendi hatası**: `belge_tarihi_yil/ay/gun_baslangic` üçü de id 16 (genel şablonda doğru şekilde 16/18/20), aynısı 17 ve 604'te. `_GetParametre(int)` ilk eşleşmeyi döndürdüğü için 5 kayıt Fora'da da okunamıyor → atılmıyor, `shadowed` ile işaretleniyor |
| P0b | `ForaAndroidKullaniciDuzenleme.cs` → `ui.akilli.json` | ✅ | — | 65 sekme (5 dış sekme altında iç içe), **1.782 parametre yerleşti, hepsi etiketli**. Etiket kaynakları: `caption` 733 (her `CheckEdit` kendi başlığını taşıyor), `tableCell` 641, `labelLeft` 387, `checkBoxLeft` 21. Editör dağılımı: 926 bool, 680 metin, 100 tam sayı, 53 seçim, 16 ondalık, 6 renk, 1 çok satırlı. **Üçüncü Fora hatası:** `GosterZyrt_Temsilci_Ozel_12..19_Var_Yok` yanlış kutucuklara yükleniyor ama doğru parametreye kaydediliyor — Fora'da o ekranı açıp kaydetmek bir parametreyi başkasının değeriyle eziyor; kaydetme yönü esas alındı, 8 uyuşmazlık `bindingMismatch` olarak raporlanıyor. `Sifre` hiçbir kontrole bağlanmıyor (D6 zaten hariç tutuyor). **Codex 2 P2 bulgu düzeltildi:** (1) editörde hiç geçmeyen parametreler boşluk olarak raporlanmıyordu — 13 parametre sessizce kayboluyordu (`Vergi0*`, `HakGormeCariAdresler`, konsinye evrak serileri…) → `notInEditor` boşluğu eklendi ve "hepsi yerleşti" iddiası düzeltildi; (2) `ui.akilli.json`'da `sourceBuild` yoktu (D1/R1 ihlali) → eklendi. Ayrıca **dördüncü Fora hatası** bulundu: 5 parametre adı iki farklı ID ile tanımlı (639–643 ve 644–648); `_GetParametre(string)` ilk eşleşmeyi döndürdüğü için ikinci kopyalar okunamıyor. 1.801 kayıt = 1.796 kullanılabilir ad |
| P0c | Diğer editör ekranlarının UI metadata'sı | ✅ | — | **11 editör ekranı** çıkarıldı (`catalog/parameters/ui/*.json`): akilli 1.782, foramikro-kullanici 60, foramikro-genel 32, yaziciayarlari 22, comarchedi-genel 14, b2b 11, comarchedi-iliski 7, 4× mobilrapor 2'şer. `YaziciAyarlari` kataloğu da eklendi (Fora'nın tek dinamik programı, `ParametrelerDefault`'ta yok) → katalog 21 set / 14 program / 4.713 tanım. **Üç çıkarıcı kusuru bulundu ve düzeltildi:** (1) `base.Controls.Add(...)` ile doğrudan forma eklenen kontroller kaçıyordu — `ComarchEdiGenelParametrelerForm` 26 boşluktan 0'a indi; (2) sekmesiz form için `noTab` uydurulyordu; (3) uyuşmazlık tespiti "bir parametre birden çok kontrole yükleniyor" durumunu kusur sanıyordu — yazıcı tasarımcısında `Veri`, `VeriTipi`'ne göre üç kontrolden birine bağlanıyor, meşru. Tespit "bir kontrol bir parametreyi gösterip başkasını kaydediyor" durumuna daraltıldı; akilli'deki 8 gerçek kusur kaldı, özneleri artık ezilen parametreler (24…94). **Kapsam kararı:** toplu aktarım ekranlarının (~3.500 alan) UI metadata'sı P6'ya bırakıldı; varsayılan katalogları zaten tam |
| P0d | Tip çıkarsama ve doğrulama | ✅ | — | **P0 kapsamı için bitti.** Editör düzeni çıkarılan her parametrenin tipi var; aktarım setlerinin tipi ancak onların editör ekranları çıkarılınca doğabilir ve o ekranlar D19 gereği P6'da — borç P6a–P6c görev tanımlarına yazıldı, kaybolmuyor. Editör düzeni çıkarılan 1.936 parametrenin tipi var, `unknown` yok (test sabitliyor): `boolean` 965, `text` 734, `integer` 117, `choice` 63, `decimal` 29, `reference` 11, `color` 6, `secret` 5, `composite` 5, `multilineText` 1. **Kalan 2.777 tanım (aktarım setleri) tipsiz** — tip yalnız editör ekranından çıkıyor ve o ekranlar P6'ya bırakıldı. P0d ancak P6a–P6c ile kapanır |
| P0e | Çıkarım script'i + altın dosya testi | ✅ | — | `tools/ForaCatalogExtractor` (Roslyn; dört ZPL şablonu kaçırılmış tırnakla bitiyor, regex sessizce bozuyor). `--check` bayat katalogda sıfırdan farklı kodla çıkar. `tests/ErpBridge.ForaCatalog.Tests` 13 test: bayt bayt altın dosya, iki kez çalıştırınca aynı çıktı, kapsam alanı iddiaları, çakışma kayıtları, Türkçe ve kaçırılmış tırnak korunumu, hatalı girdilerin reddi. Proje merkezi paket yönetiminin dışında: `Directory.Packages.props` geçişli sabitleme açtığı için Roslyn'i oraya eklemek EF Design üzerinden Roslyn çeken her projenin kilit dosyasını yeniden yazardı |
| P0f | El ile gözden geçirme raporu | ✅ | — | [GOAL_PARAMETRE_KATALOG_RAPORU.md](GOAL_PARAMETRE_KATALOG_RAPORU.md). Kapsam dökümü (21 set), Fora'nın 4 kusuru, boşluk dökümü, panel tasarımını etkileyen 4 bulgu, 5 düşük güvenli nokta ve sonraki fazlara devreden 7 karar. **En önemlisi:** `akilli`'nin 1.782 alanının 830'u (%47) tek bir tekrar eden yapı — ziyaret anketi 100 soru × 8 parametre; panel bunu düz liste olarak çizerse ekran kullanılamaz olur (P2b). İkincisi: kasa/depo/fiyat listesi gibi 29 alan Fora'da düz metin, referans türü ad kalıbından çıkarılmalı ama kalıp tek başına 11 yetki bayrağını da yakalıyor (P2c) |
| P1a | `parameter_catalog_entries` + tohumlama | ✅ | — | Katalog derlemeye gömülü (`catalog/parameters/*.json`, ~1.5 MB); API konteynerde çalışıyor ve dosyadan okusa katalog kaybolabilirdi. **4.708 satır** tohumlanıyor — 4.713 tanımın gölgelenmiş 5'i hariç, çünkü onlar Fora'da da okunamıyor ve `(set, id)` anahtarını bozarlardı. Tohumlama **satır silmiyor**: geri çekilen parametre `IsDeprecated` ile işaretleniyor (D2), geri gelirse canlandırılıyor. `IsImplemented` yeniden tohumlamada korunuyor (D16) — onu katalog değil ürün bilir. Testlerde kapalı (`Parameters:SeedCatalogOnStartup`), 4.700 satırı her test sınıfı için eklemek ispatladığından pahalı; tohumlama mantığı doğrudan test ediliyor |
| P1b | `parameter_values` + migration | ✅ | — | Yalnız **sapmalar** saklanıyor (D3, Fora semantiği). Adresleme, katalog girdisine yabancı anahtarla yapılıyor: o girdi `(program, AnaGrubu, AltGrubu, ParametreID)` dörtlüsünü belirliyor, çünkü `(program, id)` çifti **tekil değil** — 3.679 çiftin 994'ü birden çok parametreyi gösteriyor. Kapsam iki kolonla taşınıyor: `MobileUserId` (D5, kullanıcı adı değil kimlik) ve `Scope1`/`Scope2` (şablon adı, kriter adı, rapor kodu; yazıcı şablonu alanı ikisini birden kullanıyor — katalogda ölçüldü, en fazla iki kapsam kolonu var). `ErpCompanyId` zorunlu (D3b). Silme davranışı: kiracı cascade, firma ve katalog girdisi restrict, mobil kullanıcı **restrict** — silinen kullanıcının değerleri geçmiş olarak kalır (D5b). Testler SQLite ile koşuyor; in-memory sağlayıcı benzersiz indeksi yok sayıyor |
| P1c | `ParameterResolver` (Fora semantiği) | ✅ | — | Efektif değer = katalog varsayılanı + saklı sapma. Yazma Fora'nın **dört yolunu** birebir uyguluyor: varsayılana eşit + satır yok → hiçbir şey, varsayılana eşit + satır var → **sil**, farklı + satır yok → ekle, farklı + satır var → güncelle. Bayat satır bırakmak, kimsenin seçmediği bir varsayılanı ezmeye devam ederdi. `ParameterScope` kapsamı doğruluyor: mobil kullanıcı parametresini kullanıcı belirtmeden ya da yazıcı alanını tek adla yazmak **reddediliyor** — öyle bir satırı ne okuma bulur ne de ayna Mikro'ya yerleştirebilir. Sorgular her boyutu karşılaştırıyor (firma, kullanıcı, iki kapsam kolonu); birini atlamak bir firmanın ayarının diğerine sızma yolu |
| P1d | `parameter_revisions` + `parameter_audit` | ✅ | — | **Sürüm sayacı (D9):** kapsam başına tek sayı; bir mobil kullanıcının seti 1.801 parametre, sayaç olmadan telefon hiçbir şeyin değişmediğini anlamak için hepsini çekmek zorunda kalırdı. Değerle **aynı kaydetmede** artıyor — değişmemiş sayacın "hiçbir şey kımıldamadı" demesi gerekiyor. Bir firmanın değişikliği diğerinin sayacını oynatmıyor. **Denetim (D10):** her yazma kim/ne zaman/eski→yeni/kaynak ile kaydediliyor, `ChangeContext` artık **zorunlu** — parametre bir plasiyerin fiyat değiştirip değiştiremeyeceğine karar verebiliyor, atıfsız değişiklik fark edilmeden pahalıya patlayan türden. **Kimlik bilgileri maskeleniyor:** şifreleri düz metin tutan bir denetim kaydı, denetim kaydı olmamasından kötü; "boştu" ile "doluydu" ayrımı korunuyor. Kayıt parametreyi **katalog girdisiyle** gösteriyor, adla değil |
| P1e | Admin API uçları | ✅ | [#122](https://github.com/Retrosero/ErpBridge/pull/122) | `/api/v1/admin/parameters` altında beş uç: `GET /sets` (katalogdaki setler ve her birinin nasıl adreslendiği), `GET /values` (bir setin bir kapsamdaki değerleri — yürürlükteki değer, varsayılanı, sapma olup olmadığı, sürüm; `onlyOverridden` ile yalnız sapmalar), `PUT /values` (toplu yazma, her değişiklik `Inserted`/`Updated`/`Deleted`/`Unchanged` sonucuyla), `POST /values/reset`, `GET /audit`. Parametre **katalog kaydıyla** adresleniyor, adla değil — 3.365 ayrı adın 862'si birden fazla sette geçiyor. Kapsam parametreyi adresleyemiyorsa yazma **400** dönüyor: hiçbir okumanın bulamayacağı, ayna Mikro'ya yerleştiremeyeceği satır yazmaktansa. Setler bellekte gruplanıyor — birkaç düzine set var ve dört kolonlu gruplama testlerin koştuğu her sağlayıcıda çevrilmiyor. 8 uç testi |
| P1f | `/android/parameters` yeniden bağlama + `revision`/`304` | ✅ | [#123](https://github.com/Retrosero/ErpBridge/pull/123) | Sözleşme aynı, **kaynak** değişti (D13): değerler içe aktarılmış `_FORA_PARAMETRELER` aynasından değil katalog varsayılanı + saklı sapmadan geliyor. Kazanım: kimsenin değiştirmediği parametre artık **eksik dönmüyor** — eski ayna yalnız sapmaları tuttuğu için telefon kendi gömdüğü varsayılana düşüyordu, artık Fora'nın varsayılanı dönüyor. `ParametreUser` hâlâ kullanıcı adı taşıyor — eski istemcinin eşleştirdiği şey o. Yalnız **aktif** kullanıcılar yayılıyor (D5b, R8). `sourceDatabase` verilmezse kiracının bütün aktif firmaları kapsanıyor — eski ucun süzgeçsiz davranışı. **`ETag`/`304`:** yanıt kullanıcı başına 1.801 satır, ETag kapsanan kapsamlardan + her birinin sürüm sayacından + **katalog damgasından** üretiliyor; katalog damgası şart, çünkü yeniden tohumlama bir varsayılanı oynatabilir ve istemci artık uymayan bir kopyayı tutmaya devam ederdi. `revision` toplam olarak dönüyor ama **bilgilendirme** — kullanıcı silinince düşebildiği için 304 kararı ETag'e dayanıyor. Çözümleyiciye toplu okuma eklendi (`ResolveManyAsync`/`RevisionsAsync`): kullanıcı başına ayrı çözümleme 1.801 katalog satırını her kullanıcı için yeniden okurdu. 9 uç testi |
| P1g | Ajan uçları | ✅ | [#124](https://github.com/Retrosero/ErpBridge/pull/124) | `GET /api/v1/agents/parameters?erpCompanyId=` ve `POST /api/v1/agents/parameters/report`. **Firma başına zorunlu:** ajan birkaç firmaya atanmış olabilir, her biri ayrı bir Mikro veritabanı; firma adlandırmayan çağrı bir firmanın ayarını diğerinin veritabanına yazardı. Çekme ucu **delta değil tam hedef durum** döndürüyor: ajan tabloyu ona eşitliyor, yani merkezin artık listelemediği satır ajanın sildiği satır. Delta olsaydı iki tarafın en son ne gönderildiği konusunda anlaşması gerekirdi ve kaçırılan tek bir koşu, kimsenin seçmediği bir varsayılanı ezmeye devam eden bayat satır bırakırdı. Yalnız **sapmalar** (D3) ve yalnız **aktif** kullanıcılar (D5b). `MobileUser.Id` → `ParametreUser` çevirisi uçta yapılıyor — Mikro kullanıcı adıyla adresliyor, ajanın tahmin etmesine bırakılmıyor. **Rapor ucu:** ne yazıldığı (insert/update/delete/fail) ve Mikro'da **elle değiştirilmiş** bulunan satırlar (`parameter_mirror_reports` + `parameter_mirror_drifts`). Fark **kaydediliyor, geri yazılmıyor** — ayna tek yönlü (D8); panel uyarısı P3e'de bunu okuyacak. Kataloglanmamış parametreyi gösteren fark satırı düşürülüyor, çünkü hiçbir şeyi göstermeyen bir satır sonradan okunamaz. Atanmamış firma, başka kiracının firması ve olmayan firma **aynı 403**'ü alıyor — ajanın başka kiracıların hangi firmalara sahip olduğunu öğrenmesi gerekmiyor. Migration yalnız **tablo ekliyor** (D20). 11 uç testi |
| P2a | `/parameters` ekran iskeleti + scope seçimi | ✅ | [#125](https://github.com/Retrosero/ErpBridge/pull/125) | Seçim zinciri: müşteri → **ERP firması** → program → parametre kümesi → kapsam. Program ve küme listesi `/sets` ucundan, yani **katalogdan** geliyor — elle yazılmış bir liste, Fora kaynaklarından üretilen katalogdan sessizce ayrışırdı. Firma seçici tek firmada **otomatik seçiliyor ama gizlenmiyor** (D3b): operatör hangi veritabanını değiştirdiğini görmek zorunda. Kapsam alanı kümenin `ScopeKind`'ına göre şekilleniyor — mobil kullanıcı için **yalnız aktif** kullanıcıların listesi, adlandırılmış kapsamlar için bir ya da iki metin kutusu, firma geneli için hiçbiri. Kapsam tamamlanmadan **Getir düğmesi açılmıyor**: kullanıcı belirtmeden yazılan mobil kullanıcı parametresi hiçbir okumanın bulamayacağı bir satır olurdu. Eski ham ayna görünümü silinmedi, `/parameters/mikro-aynasi`'na taşındı ve salt okunur olduğu belirtildi (D13, P7b). 8 bUnit testi; eski ekranın testleri `ParameterMirrorTests`'e taşındı |
| P2b | Katalogdan üretilen sekme ağacı | ✅ | [#126](https://github.com/Retrosero/ErpBridge/pull/126) | Ağaç `TabPath`'ten **üretiliyor**, yazılmıyor (D11) — 63 sekme 63 Razor dosyası olsaydı, Fora kaynaklarından yeniden üretilen katalogla elle eş tutulması gerekirdi. Ölçülen yapı: `akilli`'de **54 ayrı sekme yolu**, 5 kök, en fazla 4 derinlik. Sekme sırası parametrelerin geliş sırası — sunucu zaten Fora'nın editör sırasıyla döndürüyor, ayrı bir sıra alanına gerek yok. Her düğüm **altındaki toplamı** ve **sapmış sayısını** gösteriyor: 63 sekmede işin nerede olduğunu söyleyen şey bu. **Tekrar eden aileler gruplandı:** 1.782 alanın 830'u tek bir sekmede (Ziyaret anket) ve bunların 800'ü dört soru türü × 100 slot — düz liste olarak çizilince ekran kullanılamaz hale geliyordu (P0f Bulgu 1). Gruplama **addaki rakam dizisi maskelenerek** türetiliyor (`..._#_Derece`), yani bu aileye özel değil; üçten az üyesi olan kalıp grup sayılmıyor, çünkü bir çifti katlamak kazandırdığından çok gizler. Grup ilk üyesinin yerini alıyor — operatör alanı Fora'nın koyduğu yerde arıyor. **Fora'da editörü olmayan 13 parametre gizlenmiyor**, "Fora'da ekranı olmayanlar" başlığı altında duruyor: düşürmek, düzeltmeye çalıştığımız kusuru tekrarlamak olurdu. Ağaç ve gruplama mantığı `ParameterLayout` içinde, Razor'dan ayrı ve doğrudan test edilebilir: 9 birim + 3 bUnit testi |
| P2c | Editör bileşenleri (7 tip) | ⬜ | — | |
| P2d | Efektif değer / varsayılan rozeti / sıfırlama | ⬜ | — | |
| P2e | Arama, "yalnız sapanlar", "etkisiz" rozeti | ⬜ | — | |
| P2f | Toplu işlemler (kopyalama, dışa/içe aktarma) | ⬜ | — | |
| P2g | Değişiklik geçmişi görünümü | ⬜ | — | |
| P2h | Vergi oranları onay adımı | ⬜ | — | |
| P3a | `_ERPB_PARAMETRELER` kurulumu (V15/V16) | ⬜ | — | |
| P3b | `ParameterMirrorWorker` (merkez → Mikro) | ⬜ | — | |
| P3c | `ForaParameterImporter` (salt okunur) | ⬜ | — | |
| P3d | Panelde "Fora'dan içe aktar" akışı | ⬜ | — | |
| P3e | Ayna fark raporu | ⬜ | — | |
| P3f | ERP lookup beslemesi | ⬜ | — | |
| P4a | Room tablosu + gömülü katalog + `ParameterProvider` | ⬜ | — | |
| P4b | `Goster_AnaMenu_*` (43) | ⬜ | — | |
| P4c | `Hak*` (71) | ⬜ | — | |
| P4d | `EvrakSeri_*` (14) + `Default*` (37) | ⬜ | — | |
| P4e | `SenkronizeEt_*` (44) | ⬜ | — | |
| P4f | "Telefonda etkili" rozeti | ⬜ | — | |
| P5a | `Stok*` + `Listeleme*` | ⬜ | — | |
| P5b | `Sepet*` + `Evrak*` + `Siparis*` | ⬜ | — | |
| P5c | `YeniCari*` (137) | ⬜ | — | |
| P5d | `Yazdirma*` + `Koli*` + `YaziciAyarlari` tasarımcısı | ⬜ | — | |
| P5e | `Rapor*` + `MobilRapor*` | ⬜ | — | |
| P5f | `Metin*` (415) | ⬜ | — | |
| P5g | `Formul*`, `Firma*`, `Zorunlu*`, `risk_hesabi_*`, artakalanlar | ⬜ | — | |
| P5h | `ForaMikro`/`foramikro` (95) + vergi oranları | ⬜ | — | |
| P6a | `GenelAktarim` (1.486) | ⬜ | — | |
| P6b | `TahsilatAktarim` (756) | ⬜ | — | |
| P6c | `BankaAktarim` (503) | ⬜ | — | |
| P6d | `b2b` + `ComarchEdi*` | ⬜ | — | |
| P7a | Bilgi bankası + API sözleşmeleri | ⬜ | — | |
| P7b | Eski `ParameterRecord` yolunun işaretlenmesi | ⬜ | — | |
| P7c | Operatör kılavuzu (Fora'dan geçiş) | ⬜ | — | |

---

## Seni bekleyenler

| # | Madde | Neden insan gerekiyor |
|---|---|---|
| B1 | **Fora sürüm doğrulaması.** Decompile edilen `Fora Mikro.exe` hangi sürüm ve müşteride çalışan sürümle aynı mı? | Katalog `sourceBuild` alanı için gerekli; müşteri kurulumuna bakılmalı |

---

## Notlar

- **2026-09-18:** `MikroDB_V16_03202403221700.bak` yedeği `MikroDB_V16_03` olarak yüklendi
  (`F:\Mikro\v16xx\03\DATA\`). Fora'nın canlı parametre verisi (516 satır, 4 mobil kullanıcı) ve
  `_FORA_SYNC` tetikleyicileri buradan incelendi. Yedek 2024-03-22 tarihli — şema referansı olarak
  kullanılır, veri karşılaştırması için değil.
