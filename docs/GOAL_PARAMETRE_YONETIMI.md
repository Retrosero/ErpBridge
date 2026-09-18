# Goal — Parametre Yönetimi (Fora paritesi: panelden tam kontrol)

Tarih: 2026-09-18 · Kapsam: ErpBridge.CentralApi + ErpBridge.Admin + ErpBridge.Agent.Service/UI
(+ Core, Erp.Mikro, Erp.Abstractions, LocalStore, Shared) + Sipariş Cepte (Android)

İlerleme **[GOAL_PARAMETRE_DURUM.md](GOAL_PARAMETRE_DURUM.md)** dosyasına yazılır; her görevden sonra,
o görevin PR'ı içinde güncellenir. Oturum kapanırsa oradan devam edilir.

---

## 0. İstek, bulgular ve kararlar

### İstek (kullanıcı, 2026-09-18)

> "Fora uygulaması hangi parametreleri kullanıyor, hangi alanların ayarlarını nereden çekip yapıyor
> önce incele; sonra panel uygulamamdan bunların hepsini kontrol etmek istiyorum."

`MikroDB_V16_03202403221700.bak` yedeği `MikroDB_V16_03` olarak sunucuya yüklendi (2026-09-18,
`F:\Mikro\v16xx\03\DATA\`). Fora'nın canlı parametre verisi ve decompile edilmiş kodu bu inceleme
için referans alındı.

---

### Bulgu 1 — Fora'nın ayar mimarisi: tek tablo, tek desen

Fora'nın **bütün** ayarları tek tabloda tutulur: `_FORA_PARAMETRELER`. Tabloyu uygulamanın kendisi
yaratır (`ParametreData.ForaParametrelerTablosuOlustur`).

| Kolon | Tip | Açıklama |
|---|---|---|
| `ID` | V15'te `int IDENTITY`, **V16'da `uniqueidentifier`** | Birincil anahtar |
| `ParametreProgram` | `nvarchar(40)` | Ayar kümesi (`akilli`, `YaziciAyarlari`, `foramikro`, …) |
| `ParametreUser` | `nvarchar(40)` | Kapsam anahtarı — mobil kullanıcı adı, şablon adı veya rapor kodu |
| `ParametreAnaGrubu` | `nvarchar(100)` | İkincil kapsam (çoğu programda boş) |
| `ParametreAltGrubu` | `nvarchar(100)` | Üçüncül kapsam (çoğu programda boş) |
| `ParametreID` | `int` | **Gerçek anahtar.** Sorgular hep bununla |
| `ParametreAdi` | `nvarchar(100)` | Yalnız okunabilirlik için; hiçbir sorgu buna bakmaz |
| `ParametreDegeri` | `nvarchar(max)` | Değer; tip yok, hepsi metin |

Mantıksal anahtar: **(Program, User, AnaGrubu, AltGrubu, ParametreID)**.
İndeks: ilk dördü üzerinde `NONCLUSTERED [01]` — `ParametreID` indekste **yok**.

### Bulgu 2 — Varsayılanlar veritabanında değil, uygulamanın **içinde**

`ParametreData.ParametreYaz` semantiği:

| Durum | Eylem |
|---|---|
| Değer = varsayılan, DB'de satır **yok** | hiçbir şey yapma |
| Değer = varsayılan, DB'de satır **var** | **satırı sil** |
| Değer ≠ varsayılan, DB'de satır yok | insert |
| Değer ≠ varsayılan, DB'de satır var | update (yalnız `ParametreDegeri`) |

Okuma simetrik (`ParametreOku`): önce koddaki varsayılan liste belleğe yüklenir, sonra DB satırları
üzerine bindirilir. Telefon tarafı da aynı (`ParametreSqlite.ParametreOku`, SQLite kopyası).

**Sonuç: `_FORA_PARAMETRELER` tek başına anlamsızdır.** Efektif değeri bilmek için katalog şarttır.
Canlı V16 yedeğinde 4 mobil kullanıcı için toplam **516 satır** var; katalogda kullanıcı başına
**1801 parametre** tanımlı. Gerçekte varsayılandan sapan yalnız **153 farklı `ParametreID`**.

### Bulgu 3 — Katalog: 13 program, 4.688 tanım

`Fora.Mikro.ParametreTanimlari.ParametrelerDefault` (4.882 satır):

| Katalog metodu | Program | Kapsam anahtarı (`ParametreUser`) | Adet |
|---|---|---:|---:|
| `MobilKullanici(User)` | `akilli` | mobil kullanıcı adı | **1801** |
| `GenelAktarimTxtCsvSablon(SablonAdi)` | `GenelAktarim` | şablon adı | 863 |
| `GenelAktarimSqlSablon(SablonAdi)` | `GenelAktarim` | şablon adı | 623 |
| `TahsilatAktarimTxtCsvSablon(SablonAdi)` | `TahsilatAktarim` | şablon adı | 436 |
| `BankaAktarim(SablonAdi)` | `BankaAktarim` | şablon adı | 424 |
| `TahsilatAktarimSqlSablon(SablonAdi)` | `TahsilatAktarim` | şablon adı | 313 |
| `ForaMikroKullanici(User)` | `foramikro` | masaüstü kullanıcı | 60 |
| `BankaAktarimGenel(SablonAdi)` | `BankaAktarim` | şablon adı | 48 |
| `ForaMikro()` | `ForaMikro` | — (firma geneli) | 35 |
| `BankaAktarimKriter(KriterAdi)` | `BankaAktarim` | kriter adı | 31 |
| `ComarchEdiGenelParametreler()` | `ComarchEdiGenel` | — | 14 |
| `B2B()` | `b2b` | — | 11 |
| `ComarchEdiIliskiParametreleri(IliskiID)` | `ComarchEdiIliski` | ilişki kimliği | 7 |
| `GenelAktarimKriter` / `TahsilatAktarimKriter` | ilgili | kriter adı | 7 + 7 |
| `MobilRaporStokSatis/StokEnvanter/StokSiparis/YapilacakTahsilatlar(RaporKodu)` | `MobilRapor*` | rapor kodu (1–9) | 2 + 2 + 2 + 2 |
| | | **TOPLAM** | **4.688** |

`YaziciAyarlari` programı bu listede **yok** — katalogu dinamiktir (aşağıda).

### Bulgu 4 — `akilli` (mobil kullanıcı) kataloğunun anatomisi

1801 parametrenin varsayılan değerine göre tip dağılımı:
**1037 bool (`0`/`1`)** · **82 sayı** · **501 metin** · **181 boş** (tip bağlamdan çıkarılacak).

İsim öbekleri (ilk camelCase belirteci):

| Öbek | Adet | Ne yapar |
|---|---:|---|
| `Goster*` (`Goster_AnaMenu_*` hariç) | 463 | Alan/sütun/buton görünürlüğü |
| `Metin*` | 415 | Arayüz metinleri, ziyaret anket seçenekleri, fiş başlıkları |
| `YeniCari*` | 137 | Yeni cari açma formu: hangi alan görünsün, zorunlu mu, varsayılanı ne |
| `Stok*` | 78 | Stok listeleme / ekleme / detay / arama davranışı |
| `Hak*` | 71 | Yetkiler: `HakDegistirebilme*`, `HakGorme*`, `HakListeleme*` |
| `Yazdirma*` | 56 | Yazdırma davranışı (ağ yazıcısı, otomatik basım, kopya sayısı) |
| `Koli*` | 55 | Koli etiketi tasarımı ve basımı |
| `Evrak*` | 55 | Evrak girişi/kaydı/açıklamaları/serileri |
| `Siparis*` | 49 | Sipariş karşılama ekranı |
| `Rapor*` | 46 | Hangi raporu alabilir, hangi rapor kodları |
| `SenkronizeEt_*` | 44 | Hangi ERP tablosu telefona senkronize edilsin |
| `Goster_AnaMenu_*` | 43 | Ana menüde hangi işlem görünsün |
| `Formul*` | 32 | Formüllü miktar girişi (en × boy × yükseklik) |
| `Firma*` | 29 | Fiş üstündeki firma bilgileri |
| `Default*` | 37 | Varsayılan kasa/depo/fiyat listesi/proje/sorumluluk merkezi/firma/şube |
| `EvrakSeri_*` | 14 | Evrak tipi başına seri kodu |
| `Zorunlu*` | 16 | Zorunlu alan bayrakları |
| `risk_hesabi_*` | 5 | Risk hesabı yüzdeleri |

### Bulgu 5 — Fora'nın kendi paneli: 63 sekme, 3.572 bağlama

`Fora.App.Win.Mikro.ForaAndroid.ForaAndroidKullaniciDuzenleme` — **38.703 satır**, 141 sekme nesnesi,
63 adlandırılmış sekme, 2.526 `Controls.Add`, 3.572 `_GetParametre(...)` bağlaması.

Sekmeler: Ana sayfa · Parametreler · Tanımlamalar · Listeleme görünümü · Listeleme seçenekleri ·
Fiyatlar · Evrak girişi · Evrak Tipleri · Kayıt görünümü · Ekleme görünümü · Ekleme seçenekleri ·
Sepet görünümü · Sipariş karşılama görünümü/seçenekleri · Stok · Stok listesi · Stok detayı
seçenekleri · Stok fiyat etiketi · Stok satış · Stok sipariş · Stok envanter analizi · Cari detayı
seçenekleri · Cari hesap ekstresi · Yeni cari hesap açma · Barkod · Koli etiketi · Depo · Depolar
arası sipariş/sevk/nakliye · Ziyaret anket · Satış faturası · Satış irsaliyesi · Alış faturası ·
Alış irsaliyesi · Alınan sipariş · Proforma sipariş · Tahsilat / Tediye makbuzu · Masraf ·
Diğer evraklar · Mal fazlası · Raporlar · Rapor isimleri · Yapılacak tahsilatlar · Kasa ve Banka
durum · Temsilci günlük raporu · Senkronizasyon · Senkronize edilecek tablolar · E-posta ayarları ·
Form dosyaları · Vergi oranları · Görünüm ve seçenekler · Değiştirme / Görme / Varsayılan değerler ·
Yardım · Diğer.

**Bağlama deseni tek tip ve makineyle çıkarılabilir:**

```
private TextEdit CariPersonelKodu;                                   → editör tipi
this.xtraTabPage30.Controls.Add(this.CariPersonelKodu);              → sekme üyeliği
this.CariPersonelKodu.Location = new Point(164, 207);                → yerleşim + en yakın etiket
CariPersonelKodu.Text = _kullaniciparametreleri
        ._GetParametre("CariPersonelKodu")._GetString;               → parametre + tip
```

Kontrol tipi → editör tipi: `CheckEdit`→bool · `SpinEdit`/`CalcEdit`→sayı · `TextEdit`/`MemoEdit`→metin
· `LookUpEdit`/`ComboBoxEdit`→liste · `CheckedListBoxControl`→çoklu seçim (virgülle ayrılmış değer).

Diğer editör ekranları (aynı desen): `Fora.App.Win.Mikro.Ayarlar` (5.529 satır — genel parametreler,
kullanıcı, yazıcı ayarları, form tasarımı) · `Aktarimlar.BankaAktarimi` (16.128) ·
`TopluEvrakGirisi.GenelEvrakSatirBazli.Import` (45.568) ·
`TopluEvrakGirisi.TahsilatEvrakSatirBazli.Import` (21.813) · `B2B` (452).

### Bulgu 6 — `YaziciAyarlari`: katalog değil, tasarımcı

`Fora.Mikro.Yazdirma.YaziciAyarlari` kapsamı farklı kullanır:
`ParametreUser` = **şablon adı** (kullanıcı değil), `AnaGrubu` ∈ {`GenelAyarlar`, `Alan`},
`AltGrubu` = alan adı.

- `GenelAyarlar`: 9 sabit parametre — `SablonAdi`, `SayfaKolonSayisi` (vars. 120),
  `SayfaSatirSayisi` (60), `SayfaDokumSayisi`, `DetayBaslangicSatiri`, `DetayBasiSatirSayisi`,
  `DetayBirSayfadakiKayitSayisi`, `AltBasliklarSadeceSonSayfadaYazilsin`, `StokGruplandirmaSecenegi`.
- `Alan` (her alan için 16 parametre): `Isim`, `BasilacakAlan` (UstBaslik/Satir/AltBaslik),
  `VeriTipi`, `Veri`, `Kolon`, `Satir`, `Genislik`, `Hizalama` (Sol/Orta/Sag),
  `DetayinBittigiYereKaydir`, `OndalikHaneSayisi`, `SonunaParaBirimiEkle`, `BasinaParaBirimiEkle`,
  `BinlikAyraci`, `OndalikAyraci`, `OnEk`, `SonEk`.

Yani karakter ızgarası üzerinde alan yerleşimi — düz bir form değil, **tasarımcı ekranı**.
Alan listesi dinamiktir (`alanekle`/`alansil`).

### Bulgu 7 — Kullanıcı listesi de parametre tablosundan geliyor

Ayrı mobil kullanıcı tablosu **yok**:

```sql
SELECT ParametreUser FROM _FORA_PARAMETRELER WITH (NOLOCK)
WHERE ParametreProgram='akilli' GROUP BY ParametreUser ORDER BY ParametreUser
```

Kullanıcı "açmak" = o kullanıcı için parametre yazmak. Şifre de parametredir (`ParametreID=1`,
`Sifre`), sabit gömülü anahtarla (`drjbq8777!#45`) şifrelenir.

### Bulgu 8 — Telefona ulaşma yolu

`_FORA_PARAMETRELER` üzerinde **2 tetikleyici** var; değişiklikler `_FORA_SYNC`
(`TriggerRECno`, `TabloID`, `KayitGuid`) değişim günlüğüne düşer ve telefona normal senkron
kanalıyla gider. Telefon aynı tabloyu SQLite'ta tutar ve aynı varsayılan-bindirme mantığını uygular.

### Bulgu 9 — ErpBridge'in bugünkü durumu: zincir kopuk

| Katman | Durum |
|---|---|
| `ParameterRecord` + `POST /api/v1/ingest/parameters` + `GET /api/v1/android/parameters` | **Var** (Faz 15.5/15.8), `_ERPB_PARAMETRELER` aynası olarak yazılmış |
| `_ERPB_PARAMETRELER` tablosu | **Hiçbir veritabanında yok.** `MikroDB_V15_02`'de yalnız `_ERPB_SENKRONIZASYON`, `_ERPB_SYNC`, `_ERPB_SYNC_DEL` var |
| Ajan tarafı okuyucu/yazıcı | **Yok** — ingest ucunu besleyen hiçbir kod yok |
| Varsayılan katalog kavramı | **Yok** — efektif değer hesaplanamaz |
| Admin `/parameters` sayfası | **Salt okunur** düz tablo: müşteri seç → 200/1000 satır → metin araması. Gruplama, sekme, düzenleme, kullanıcı seçimi, varsayılana dönme yok |
| Sipariş Cepte | Parametre ucunu **hiç çağırmıyor**. Kendi `app_settings`'inde yalnız evrak numaratörü anahtarları (`numerator.sales.prefix` vb.) var |

Yani uçlar boşa çalışıyor: yazan yok, okuyan yok, katalog yok.

---

### Kullanıcı kararları (2026-09-18)

| # | Karar |
|---|---|
| **K1** | Kapsam **Fora ile birebir**: `akilli` kataloğunun **1801 parametresinin tamamı** panelde yönetilir. Katalog decompile'dan otomatik çıkarılır (ad, ID, varsayılan, tip, sekme, etiket, yardım metni) |
| **K2** | Tek gerçek kaynak **merkez PostgreSQL**. Panel merkeze yazar; ajan merkezden çekip Mikro'daki `_ERPB_PARAMETRELER`'e **ayna** olarak yazar; telefon merkezden okur |
| **K3** | Fora ile aynı Mikro veritabanında çalışan müşteride `_FORA_PARAMETRELER` **salt okunur** taranır ve tek seferlik içe aktarılır. Fora'nın tablosuna **asla yazılmaz** |
| **K4** | Sipariş Cepte'nin parametreleri **gerçekten uygulaması bu goal'e dahildir**; ilk turda öncelikli öbekler (`Goster_AnaMenu_*`, `Hak*`, `EvrakSeri_*`, `Default*`), kalanı sonraki fazlarda |
| **K5** | Katmanlama **Fora ile birebir**: yalnız *katalog varsayılanı + kullanıcı sapması*. Firma/rol/profil katmanı **yok** |
| **K6** | `akilli` dışındaki **bütün** programlar dahil: `YaziciAyarlari`, `ForaMikro`/`foramikro`, `MobilRapor*`, `GenelAktarim`, `TahsilatAktarim`, `BankaAktarim`, `b2b`, `ComarchEdi*` → toplam **4.688 tanım** |
| **K7** | Bu goal `/goal` yetkisiyle müdahalesiz çalıştırılır (bkz. § 1) |

### Varsayılan kararlar (goal'ün seçimleri — itiraz edilirse değişir)

| # | Karar | Gerekçe |
|---|---|---|
| **D1** | Katalog **repoda versiyonlanmış veri dosyası** olarak durur: `catalog/parameters/*.json`, `schemaVersion` + `sourceBuild` alanlarıyla. Üreten script `scripts/extract-fora-catalog/` altında, yeniden çalıştırılabilir ve çıktısı deterministik | Fora'nın yeni sürümü çıkarsa katalog yeniden üretilir; elle bakım yapılmaz |
| **D2** | Katalog **koda gömülmez, tohumlanır**: migration `parameter_catalog_entries` tablosunu JSON'dan doldurur. Sürüm artınca fark uygulanır (yeni parametre eklenir, kaybolan `deprecated` işaretlenir, **silinmez**) | Eski değerler yetim kalmasın; katalog sorgulanabilir olsun |
| **D3** | Değer tablosu `parameter_values`: **`(TenantId, ErpCompanyId, Program, ScopeKind, ScopeId, AnaGrubu, AltGrubu, ParametreId)`** üzerinde tekil indeks. **Yalnız varsayılandan sapanlar tutulur** — Fora semantiği birebir; değer varsayılana eşitlenirse satır silinir | Fora'ya geri yazım ve içe aktarım kayıpsız olur; satır sayısı küçük kalır |
| **D3b** | **`ErpCompanyId` zorunludur ve sonradan değiştirilemez.** Bir kiracının birden çok `ErpCompany`'si olabilir ve her birinin kendi `SourceDatabase`/`CompanyNo`/`BranchNo`/`WarehouseNo`'su vardır; mevcut `ParameterRecord` yolu da zaten `SourceDatabase` ile süzüyor. Firma boyutu **değer, sürüm, API ve ayna** kapsamlarının hepsinde taşınır. Tek firmalı kiracıda da alan doldurulur | Firma boyutu olmadan bir firmanın deposu/şubesi/evrak serisi diğerinin üzerine yazar (Codex #111 P1) |
| **D4** | Kapsam bizde **`ScopeKind` + `ScopeId`** ikilisiyle taşınır. Katalog her set için `scopeKind` (`MobileUser`, `DesktopUser`, `ReportCode`, `ImportTemplate`, `CriteriaName`, `EdiRelation`, `PrinterTemplate`, `None`) ve **`scopeField`** taşır. `ScopeId` metindir | Tek şema, doğru anlam; panel doğru seçiciyi gösterir |
| **D4b** | **Kapsam her programda `ParametreUser`'da değil** (P0a'da kod okunarak bulundu). `akilli`, `foramikro`, `MobilRapor*`, `ComarchEdiIliski` kapsamı `ParametreUser`'da tutar; `GenelAktarim`, `TahsilatAktarim`, `BankaAktarim` ise `ParametreUser`'ı **boş** bırakıp kapsamı **`ParametreAltGrubu`**'nda tutar ve `ParametreAnaGrubu`'nu sabit ayırıcı olarak kullanır (`SqlAktarimSablon`, `AktarimSablon`, …). `YaziciAyarlari` üçünü birden kullanır (`User`=şablon adı, `AnaGrubu` ∈ {`GenelAyarlar`,`Alan`}, `AltGrubu`=alan adı). Bu yüzden **üç alan da** değer tablosunda ayrı saklanır (D3'te zaten var) ve katalog `scopeField` ile hangisinin kapsam yeri olduğunu söyler. Çıkarıcı bir setteki tüm satırlarda bu alanların **tek biçimli** olduğunu doğrular; değilse hata verir | Kapsamı tek alana sabitlemek aktarım ve yazıcı şablonlarını bozardı |
| **D5** | `akilli` için `ScopeId` = **`MobileUser.Id` (Guid)**, kullanıcı adı **değil**. Kullanıcı adı yeniden kullanılabilir bir etiket: silinen kullanıcının satırı geçmiş için korunur ve aynı adla yeni kullanıcı açılabilir (`MobileSeatService`, `MobileSeatsRelationalTests.Deleting_a_user_releases_the_seat_and_the_username`). Kullanıcı adına anahtarlamak yeni kullanıcıya silinenin `Hak*` yetkilerini ve evrak varsayılanlarını devrederdi. Kullanıcı adına çeviri **yalnız aynaya yazarken** yapılır | Kimlik değişmez olmalı (Codex #111 P1) |
| **D5b** | Ayna ve `/android/parameters` **yalnız aktif** (`IsActive`, `DeletedAtUtc == null`) mobil kullanıcıların değerlerini yayar. Silinmiş kullanıcının satırları veritabanında geçmiş olarak kalır; aynaya gitmez, telefona dönmez, panelde "silinmiş kullanıcı" başlığı altında salt okunur görünür | Aynı kullanıcı adını taşıyan iki satırdan yalnız biri aktif olabilir; ayna belirsizliğe düşmez |
| **D6** | **`Sifre` (ParametreID=1) hiçbir zaman taşınmaz, saklanmaz, panelde gösterilmez.** İçe aktarımda atlanır, katalogda `excluded: true` işaretlenir. Kimlik doğrulama `MobileUser.PasswordHash` ile | Fora sabit gömülü anahtarla şifreliyor; bu sırrı sistemimize taşımak güvenlik gerilemesi olur |
| **D7** | Mikro aynası: ajan `_ERPB_PARAMETRELER` tablosunu Fora'nın şemasıyla **birebir** kurar (V15 `int IDENTITY`, V16 `uniqueidentifier`) ve Fora'nın insert/update/delete semantiğini uygular. Tabloya **tetikleyici eklenmez** | Şema uyumu Fora'dan/Fora'ya geçişi ucuzlatır; tetikleyici eklemek müşterinin ERP'sine müdahaledir |
| **D8** | Mikro aynası **tek yönlü** (merkez → Mikro). Ajan aynada bulduğu elle yapılmış değişikliği merkeze geri yazmaz; yalnız **fark raporlar** ve panelde uyarı olarak gösterir | İki yönlü yazımda çakışma çözümü gerekir; K2 merkezi ana kaynak yaptı |
| **D9** | Sürümleme: `parameter_revisions(TenantId, ErpCompanyId, ScopeKind, ScopeId, Revision, UpdatedAtUtc)` — değer anahtarıyla aynı boyutlar (D3b, D5). Telefon ve ajan `If-None-Match`/`revision` ile ucuz yoklar; değişmemişse `304` | 1801 parametreyi her senkronda indirmemek için |
| **D10** | Denetim: her değişiklik `parameter_audit` satırı (kim, ne zaman, program/scope/ID, eski→yeni, kaynak: panel / içe aktarım / API). Saklama 365 gün, `log_settings` ile aynı desen | "Bu ayarı kim değiştirdi" sorusu cevaplanabilsin |
| **D11** | Panel sekme ağacı **katalogdan üretilir**, elle yazılmaz. 63 sekme = 63 Razor dosyası değil; tek jenerik ekran + katalog metadata'sı | 1801 alanı elle yazmak sürdürülemez |
| **D12** | Referans tipi alanlar (kasa kodu, depo no, fiyat listesi no, ödeme planı, proje, sorumluluk merkezi, cari personel) panelde **serbest metin değil seçici** olur; liste ajanın ERP'den çektiği lookup'lardan gelir. Lookup yoksa alan serbest metne düşer ve uyarı gösterir | Yanlış kod girip evrağı patlatmayı önler; ajan çevrimdışıyken panel çalışmaya devam eder |
| **D13** | Mevcut `ParameterRecord` / `/api/v1/ingest/parameters` / `/api/v1/android/parameters` **silinmez**. Yeni model yanına kurulur; `/android/parameters` geçiş süresince yeni modelden **efektif değerleri** döndürecek şekilde yeniden bağlanır, sözleşmesi korunur | Eski telefon sürümleri bozulmasın |
| **D14** | Sipariş Cepte parametreleri Room'da tek tabloda tutar (`parameter_values`) + katalog varsayılanları **APK'ya gömülü JSON** olarak gelir. Efektif değer telefonda hesaplanır — Fora'nın yaptığının aynısı | Çevrimdışı çalışsın; indirme boyutu küçük kalsın |
| **D15** | Telefonda parametre erişimi tek kapıdan: `ParameterProvider` (tip güvenli erişimciler, `bool(id)`, `int(id)`, `string(id)`, `csvList(id)`). Ekranlar doğrudan tabloya bakmaz | 1801 parametrenin kullanımı denetlenebilir kalsın |
| **D16** | Uygulanmamış parametreler **sessizce yok sayılmaz**: katalogda `implemented: false` taşır, panelde "bu sürümde etkisiz" rozetiyle gösterilir | Kullanıcı ayarı değiştirip hiçbir şey olmamasına şaşırmasın |
| **D17** | `ForaMikro` programındaki **vergi oranları** (`Vergi0–10 KisaAdi/UzunAdi/Yuzde`) panelde ayrı ve dikkatli ele alınır: değiştirmek evrak tutarlarını etkiler. Değiştirme denetim kaydı + onay adımı ister | Mali hesaplamayı yanlışlıkla bozmayı önler |
| **D18** | İçe aktarım **önizlemeli ve geri alınabilir**: `_FORA_PARAMETRELER` taranır, katalogla birleştirilip fark listesi çıkar, panel onayından sonra uygulanır. Aktarım bir `import_batch` kimliğiyle işaretlenir, tek tıkla geri alınır | 516 satırı körlemesine uygulamak riskli |
| **D19** | Aktarım şablonları (`GenelAktarim`, `TahsilatAktarim`, `BankaAktarim` — 2.784 tanım) ve `ComarchEdi*`/`b2b` panelde **son fazda** açılır; katalog ve saklama ilk fazlarda hazır olur | Sipariş Cepte ile ilgisi yok; değer üretimi en sonda |
| **D20** | Migration kuralı (Log Merkezi D9 ile aynı): mevcut tablolara **yalnız nullable kolon eklenir**. Kolon silme, tip değiştirme, tablo düşürme yok | Eski telefon/ajan sürümleri bozulmasın |

---

## 1. Yetkiler

Kullanıcı 2026-09-18'de onayladı ("Evet — diğer goal'lerdeki ile aynı"):

| Yetki | Karar |
|---|---|
| Dala push, PR açma | Serbest |
| CI yeşil + inceleme yorumları çözülmüşken `main`'e squash-merge + dalı silme | Serbest |
| Sipariş Cepte'yi Play **internal** kanalına yükleme | Serbest |
| İnsan gerektiren madde | Yapılabilen kısım yapılır, kalanı DURUM > "Seni Bekleyenler" |

**Asla:** `--force` push · CI kırmızıyken merge · `main`'e doğrudan push · Play production yayını ·
tablo/kolon silen veya tip değiştiren migration · `_FORA_PARAMETRELER`'e **yazma** ·
müşteri ERP'sine tetikleyici ekleme · `Sifre` parametresini taşıma ·
başka oturumun commit edilmemiş değişikliğine dokunma · `git stash pop`.

---

## 2. Çalışma kuralları

### Çalışma kopyaları
- **ErpBridge işleri yalnız `GitHub/eb-prm`** worktree'sinde (dal öneki `prm-`).
  Ana klasör (`GitHub/ErpBridge`) ve `eb-yazim2/3/4` başka işlere ait; oralarda
  `switch`/`checkout`/`stash`/`reset` yapılmaz.
- **Sipariş Cepte işleri yalnız `GitHub/sc-prm`** worktree'sinde
  (yoksa: `git -C siparis_cepte fetch && git -C siparis_cepte worktree add ../sc-prm -b <dal> origin/main`).
- Room migration numarası dal açılırken `origin/main`'deki `AppDatabase` sürümünden alınır.

### Dal ve PR
- Her görev için `main`'den yeni dal: `prm-<faz><harf>-<kısa-ad>` (örn. `prm-p1a-katalog-semasi`).
- Bir dal tek bir göreve odaklanır. Alakasız düzeltme çıkarsa ayrı dal/PR.
- Commit mesajı Türkçe, ne + neden; sonunda `Co-Authored-By: Claude Opus 5 <noreply@anthropic.com>`.

### Test ve build
- `dotnet build ErpBridge.sln -c Debug` → 0 uyarı / 0 hata (`TreatWarningsAsErrors=true`).
- `dotnet test ErpBridge.sln` yeşil.
- Katalog çıkarımı için **altın dosya testi**: script yeniden çalıştırıldığında çıktı bit-bit aynı olmalı.
- Canlı Mikro şema testi: `ERPBridge_RUN_INTEGRATION=1` (`MikroDB_V15_02` / `MikroDB_V16_03`).

### Bilgi bankası
Her faz kendi PR'ında `ErpBridge_knowledge_base/` içindeki ilgili dosyayı günceller
(02 — evrak/yazıcı parametreleri, 03 — veri sözlüğü). Katalog değişimi 02'ye yazılır.

---

## 3. Fazlar

Fazlar sırayla ilerler; **P0 bütün diğerlerinin ön koşuludur.**

### P0 — Katalog çıkarımı (ön koşul)

Decompile edilmiş Fora kodundan makine okunur katalog üretmek. Kod yazılmadan önce **veri** lazım.

| # | Görev | Çıktı |
|---|---|---|
| **P0a** | `ParametrelerDefault.cs` → `catalog/parameters/defaults.json`. 19 katalog seti, 13 program, 4.688 tanım: `program`, `scopeKind`, `scopeField`, sabit adresleme alanları, `id`, `name`, `default`. Ayrıştırma **Roslyn** ile — dört ZPL şablonu kaçırılmış tırnakla bitiyor (`…,N,\"`) ve satır bazlı regex'i sessizce bozuyor | 4.688 kayıt, tek biçimlilik doğrulaması, altın dosya testi |
| **P0b** | `ForaAndroidKullaniciDuzenleme.cs` → `catalog/parameters/ui.akilli.json`. Sekme ağacı (iç içe, 65 sekme), kontrol→parametre bağlaması, kontrol tipinden editör tipi, etiket metni, okuma sırası | **1.782 parametre yerleşti, %100'ü etiketli.** Katalogdaki 1.796 farklı adın tamamı hesapta: 1.782 yerleşti, 13'ünün Fora'nın kendi editöründe karşılığı yok, `Sifre` hiçbir kontrole bağlanmıyor |
| **P0c** | Diğer editör ekranları → `catalog/parameters/ui/*.json`. Toplu aktarım ekranları (~3.500 alan) P6'ya bırakıldı: panel ekranları orada yapılacak, varsayılan katalogları P0a'da zaten tamamlandı | 11 editör ekranı; her parametre hangi katalog setine ait olduğunu taşır |
| **P0d** | Tip çıkarsama ve doğrulama: 10 editör tipi. `reference` + `referenceKind` iki sinyalden (Fora'nın seçici kontrolleri **ve** ERP tablosundan beslenen combo'lar); sabit listelerin seçenekleri kaynaktan çıkarılır; Fora'nın maskelediği **ve** adı kimlik bilgisi söyleyen alanlar `secret`; Fora'ya özgü ekranla düzenlenen ya da birden çok kontrolde görünen değerler `composite` | Editör düzeni çıkarılan parametrelerin tipi var, `unknown` yok. **Aktarım setlerinin 2.777 tanımı tipsiz kalır** — tip yalnız editör ekranından gelir, o ekranlar P6'da. P0d ancak **P6a–P6c ile kapanır** |
| **P0e** | Çıkarım script'i `scripts/extract-fora-catalog/` (tek komut, deterministik çıktı) + altın dosya testi + `README` | Yeniden üretilebilirlik |
| **P0f** | El ile gözden geçirme raporu → [GOAL_PARAMETRE_KATALOG_RAPORU.md](GOAL_PARAMETRE_KATALOG_RAPORU.md): kapsam dökümü, Fora'nın kusurları, boşluk dökümü, panel tasarımını etkileyen bulgular, düşük güvenli noktalar ve sonraki fazlara devreden kararlar | Sonraki fazlar hangi kararı vereceğini biliyor |

**Bitiş ölçütü:** `catalog/parameters/` altındaki JSON'lar şema doğrulamasından geçiyor, script
iki kez çalıştırıldığında aynı çıktıyı veriyor, 4.688 tanımın tamamı tipli.

---

### P1 — Merkez veri modeli ve API

| # | Görev | Çıktı |
|---|---|---|
| **P1a** | `parameter_catalog_entries` tablosu + migration + JSON'dan tohumlama (D2). Sürüm farkı uygulama mantığı | Katalog sorgulanabilir |
| **P1b** | `parameter_values` tablosu (D3) + **`(TenantId, ErpCompanyId, Program, ScopeKind, ScopeId, AnaGrubu, AltGrubu, ParametreId)` tekil indeksi** (D3b, D5) + `ErpCompany`/`MobileUser` yabancı anahtarları + migration | Değer saklama |
| **P1c** | `ParameterResolver`: katalog varsayılanı + sapma → efektif değer; yazımda varsayılana eşitse satır silme (Fora semantiği) | Birim testler: insert/update/delete/no-op dört yol **+ iki firmanın aynı parametresi birbirini ezmiyor + silinen kullanıcının değeri yeni aynı adlı kullanıcıya geçmiyor** |
| **P1d** | `parameter_revisions` (D9) + `parameter_audit` (D10) | Sürüm + denetim |
| **P1e** | Admin API: `GET /api/v1/admin/parameters/catalog` (sekme ağacıyla), `GET/PUT /api/v1/admin/parameters/values`, toplu yazım, `POST .../reset`, `GET .../diff`. **Her uç zorunlu `erpCompanyId` alır**; scope `MobileUser` ise `scopeId` bir `MobileUser.Id` | Panelin ihtiyacı olan her uç |
| **P1f** | `GET /api/v1/android/parameters` yeniden bağlanır: eski sözleşme korunur (`sourceDatabase` → `ErpCompanyId` çözümlenir, çıktıdaki `ParametreUser` kullanıcı adı olarak dönmeye devam eder), veri yeni modelden efektif değerlerle gelir (D13). Yalnız aktif kullanıcılar (D5b). `revision` + `304` desteği | Eski istemci bozulmaz, yeni istemci ucuz yoklar |
| **P1g** | `GET/POST /api/v1/agents/parameters` — ajan için çekme ve ayna fark raporu ucu; **firma başına** (ajan birden çok firmaya atanmış olabilir, `AgentCompanyAssignment`) | Ajanın ihtiyacı |

**Bitiş ölçütü:** API testleri yeşil; `/android/parameters` eski ve yeni yoldan aynı sonucu veriyor.

---

### P2 — Panel (Admin) parametre ekranı

| # | Görev | Çıktı |
|---|---|---|
| **P2a** | Yeni `/parameters` ekranı: müşteri → **ERP firması** → program → scope (mobil kullanıcı / şablon / rapor kodu) seçimi. Kiracının tek firması varsa otomatik seçilir ama seçim her zaman görünür kalır (D3b) | İskelet |
| **P2b** | Katalogdan üretilen **63 sekmelik ağaç** (D11), sekme içi gruplar, alan sırası | Fora sekme paritesi |
| **P2c** | Editör bileşenleri: bool anahtarı, sayı, metin, çok satırlı metin, enum açılır liste, csv çoklu seçim, referans seçici (D12) | Her tip düzenlenebilir |
| **P2d** | Her alanda: efektif değer, varsayılan rozeti, "sapmış" işareti, **varsayılana dön** düğmesi, son değiştiren + tarih | Şeffaflık |
| **P2e** | Arama (ad, ID, etiket, sekme), "yalnız sapanlar" filtresi, "bu sürümde etkisiz" rozeti (D16) | 1801 alanda gezinebilirlik |
| **P2f** | Toplu işlemler: kullanıcıdan kullanıcıya kopyalama, JSON dışa/içe aktarma, seçili alanları sıfırlama | Operasyon kolaylığı |
| **P2g** | Değişiklik geçmişi görünümü (`parameter_audit`) | "Kim değiştirdi" |
| **P2h** | Vergi oranları için ayrı onay adımı (D17) | Mali güvenlik |

**Bitiş ölçütü:** 1801 `akilli` parametresinin tamamı panelden görülebiliyor ve düzenlenebiliyor;
sekme yapısı Fora ile örtüşüyor.

---

### P3 — Ajan: Mikro aynası ve Fora içe aktarımı

| # | Görev | Çıktı |
|---|---|---|
| **P3a** | `MikroParameterTableProvisioner`: `_ERPB_PARAMETRELER` tablosunu kurar (V15 int / V16 guid, D7) | Şema |
| **P3b** | `ParameterMirrorWorker`: merkezden **atandığı her firma için ayrı** çeker, o firmanın Mikro veritabanına insert/update/delete uygular (D3 semantiği), sonucu raporlar. `ScopeId` → `MobileUser.Username` çevirisi burada yapılır; yalnız aktif kullanıcılar (D5, D5b) | Tek yönlü ayna |
| **P3c** | `ForaParameterImporter`: `_FORA_PARAMETRELER` salt okunur tarama + katalogla birleştirme → merkeze **öneri** olarak yükleme. `Sifre` atlanır (D6). `ParametreUser` **aktif** `MobileUser`'a eşlenir; eşleşmeyen kullanıcı adı **otomatik kullanıcı açmaz**, "eşlenmemiş" olarak raporlanır | İçe aktarım verisi |
| **P3d** | Panelde "Fora'dan içe aktar" akışı: hedef firma seçimi → fark önizlemesi → eşlenmemiş kullanıcıların elle eşlenmesi → onay → uygula → `import_batch` ile geri alınabilir (D18) | Güvenli göç |
| **P3e** | Ayna fark raporu: Mikro'da elle değiştirilmiş satırlar panelde uyarı (D8) | Sürpriz yok |
| **P3f** | ERP lookup beslemesi: kasa, depo, fiyat listesi, ödeme planı, proje, sorumluluk merkezi, cari personel → panelin referans seçicileri (D12) | Doğru kod girişi |

**Bitiş ölçütü:** `MikroDB_V16_03` üzerinde entegrasyon testi — içe aktarım 516 satırı doğru
okuyor, ayna `_ERPB_PARAMETRELER`'e doğru yazıyor, `_FORA_PARAMETRELER` değişmemiş.

---

### P4 — Sipariş Cepte: öncelikli öbeklerin uygulanması

| # | Görev | Çıktı |
|---|---|---|
| **P4a** | Room `parameter_values` tablosu + gömülü varsayılan katalog JSON (D14) + `ParameterProvider` (D15) + çekme/yoklama işi (`revision`, D9) | Altyapı |
| **P4b** | `Goster_AnaMenu_*` (43) → ana menü görünürlüğü | Menü kontrolü |
| **P4c** | `Hak*` (71) → yetkiler: `HakDegistirebilme*` (evrak serisi, tarih, fiyat listesi, kasa, depo, temsilci…), `HakGorme*` (cari kredisi, adat vadesi…), `HakListeleme*` (bölge, grup, temsilci, rota) | Yetki kontrolü |
| **P4d** | `EvrakSeri_*` (14) + `Default*` (37) → evrak serisi, varsayılan kasa/depo/fiyat listesi/proje/sorumluluk merkezi/firma/şube | Evrak doğruluğu |
| **P4e** | `SenkronizeEt_*` (44) → bootstrap/senkron kapsamı | Veri hacmi kontrolü |
| **P4f** | Panelde "telefonda etkili" rozeti: uygulanan parametreler `implemented: true` olarak işaretlenir (D16) | Geri bildirim |

**Bitiş ölçütü:** Panelden bir parametre değiştirildiğinde telefon bir sonraki senkronda davranışını
değiştiriyor; uçtan uca duman testi (panel → merkez → telefon) yeşil. Play **internal** yüklemesi.

---

### P5 — Kalan `akilli` öbekleri

| # | Görev |
|---|---|
| **P5a** | `Stok*` (78) + `Listeleme*` + stok listeleme/ekleme/detay/arama görünümü |
| **P5b** | `Sepet*` + `Evrak*` (55) + `Siparis*` (49) — evrak girişi, sepet, sipariş karşılama |
| **P5c** | `YeniCari*` (137) — yeni cari açma formu |
| **P5d** | `Yazdirma*` (56) + `Koli*` (55) + `YaziciAyarlari` tasarımcısı (Bulgu 6) |
| **P5e** | `Rapor*` (46) + `MobilRapor*` rapor tanımları |
| **P5f** | `Metin*` (415) — arayüz metinleri, ziyaret anket seçenekleri, fiş başlıkları |
| **P5g** | `Formul*` (32), `Firma*` (29), `Zorunlu*` (16), `risk_hesabi_*` (5), `Barkod*`, `Depo*` ve artakalanlar |
| **P5h** | `ForaMikro`/`foramikro` (95) — vergi oranları (D17), yeni cari ön ekleri, masaüstü kullanıcı |

---

### P6 — Aktarım şablonları ve entegrasyon programları (D19)

| # | Görev |
|---|---|
| **P6a** | `GenelAktarim` (1.486) — editör düzeni ve tip çıkarımı (P0d'nin kalanı), SQL ve TXT/CSV şablonları, panel ekranı |
| **P6b** | `TahsilatAktarim` (756) — editör düzeni ve tip çıkarımı, panel ekranı |
| **P6c** | `BankaAktarim` (503) — editör düzeni ve tip çıkarımı, panel ekranı |
| **P6d** | `b2b` (11) + `ComarchEdiGenel` (14) + `ComarchEdiIliski` (7) |

---

### P7 — Kapanış

| # | Görev |
|---|---|
| **P7a** | `ErpBridge_knowledge_base/02` ve `/03` tam güncelleme; `docs/api-contracts.md`, `docs/android-changeset-api.md` |
| **P7b** | Eski `ParameterRecord` yolunun kullanımdan kaldırılma planı (silme **değil**, işaretleme) |
| **P7c** | Operatör kılavuzu: "Fora'dan geçiş" adım adım |

---

## 4. Riskler ve açık noktalar

| # | Risk | Azaltma |
|---|---|---|
| R1 | Decompile edilen Fora sürümü ile müşterideki sürüm farklı olabilir; katalog eksik/fazla parametre içerebilir | Katalogda `sourceBuild`; içe aktarımda katalogda **olmayan** `ParametreID` bulunursa atılmaz, `unknown` olarak saklanır ve panelde raporlanır |
| R2 | 1801 parametrenin çoğunun Sipariş Cepte'de karşılığı yok | D16 — `implemented: false` rozeti; kullanıcı ne etkisi olduğunu görür |
| R3 | P0b'de etiket eşleştirmesi (`Location` yakınlığı) bazı alanlarda yanlış etiket verebilir | P0f el ile gözden geçirme; şüpheli eşleşmeler raporlanır; panelde etiket yoksa parametre adı gösterilir |
| R4 | Vergi oranlarını (D17) yanlış değiştirmek evrak tutarlarını bozar | Ayrı onay adımı + denetim + geri alma |
| R5 | Mikro aynası müşterinin ERP'sine yeni tablo yazar | Yalnız `_ERPB_` önekli kendi tablomuz; tetikleyici yok (D7); tablo yoksa ajan kurar, varsa dokunmaz |
| R6 | `akilli` kataloğunda 181 parametrenin varsayılanı boş — tipi belirsiz | P0d'de kontrol tipinden çıkarılır; çıkarılamayan `string` kabul edilir ve raporlanır |
| R7 | Çok firmalı kiracıda bir firmanın ayarı diğerinin üzerine yazabilir | D3b — `ErpCompanyId` değer/sürüm/API/ayna kapsamlarının hepsinde zorunlu; P1c'de "iki firma birbirini ezmiyor" testi |
| R8 | Kullanıcı adı yeniden kullanılabilir; silinen kullanıcının yetkileri yeni kullanıcıya geçebilir | D5 — anahtar `MobileUser.Id`; D5b — yalnız aktif kullanıcılar yayılır; P1c'de regresyon testi |

---

## 5. Seni bekleyenler

DURUM dosyasına taşınacak; şu an bilinen tek madde:

- **Fora sürüm doğrulaması.** Decompile edilen `Fora Mikro.exe` hangi sürüm? Müşteride çalışan
  sürümle aynı mı? Katalog `sourceBuild` alanı için gerekli.
