# 02 — ErpBridge Evrak Serileri ve Yazıcı Parametreleri Entegrasyonu

> **Hedef:** Bu doküman, sahadan mobil Bluetooth yazıcılar ile basılan fiş, fatura ve irsaliyelerin ERP tarafındaki evrak serileri (`cha_evrakno_seri`, `sth_evrakno_seri`, `sip_evrakno_seri`), sıra numaraları ve yazıcı şablon parametrelerinin ErpBridge tarafından nasıl yönetildiğini açıklar.

İlgili diğer modüller:
- Genel Sistem Mimarisi: [[00_System_Overview]]
- Muhasebe Yazıcıları: [[01_Accounting_Adapters]]
- Veri Sözlüğü & Şema: [[03_Data_Dictionary_and_Rules]]

---

## 1. ERP Tarafında Evrak Serileri ve Sıra Yönetimi

Saha satış mobil uygulamasında (`Siparis_Cepte`) termal yazıcıdan çıktı alındığında, basılan belgenin üzerindeki seri ve sıra numarası ERP'deki muhasebe kaydıyla birebir örtüşmelidir.

### Evrak Numaralandırma Kuralları
- **Fatura Serisi (`DocumentSerial`):** Mikro'da faturalar harf/rakam serisi ve artan tamsayı sıra numarası ile takip edilir (Örn: Seri `"A"`, Sıra `1042`).
- **İrsaliye Serisi:** Sevk irsaliyeleri için ayrı bir seri kodu tahsis edilir (Örn: `"IRS"`).
- **Tahsilat Makbuzu Serisi:** Tahsilatlar için kasa veya banka makbuz serisi kullanılır (Örn: `"THS"`).

### Telefon belgelerinde seri ve sıra (ERP yazım goal'ü, 2026-09-17)
- **Seri Portal'dan gelir, telefondan değil:** Portal "ERP aktarım ayarları" (`erp_write_settings`: sipariş, irsaliye,
  fatura, iade, tahsilat serisi; en çok 6 karakter, boş = serisiz) ve kullanıcı başına geçersiz kılma
  (`mobile_user_erp_mappings`, plasiyer başına seri). Ajan işi kiralarken `erpContext.series` ile alır.
- **Sıra ajanda verilir:** yazım transaction'ında aynı numarayı paylaşan tüm Mikro tablolarında `UPDLOCK, HOLDLOCK`
  ile `MAX+1` (`MikroDocumentNumbering`). Telefon belge basarken Mikro numarasını henüz bilmez; fişte kendi referansını
  (`MOB-…` / basım referansı) kullanır. Yazılınca seri-sıra telefona "Mikro'ya yazıldı: T-1234" olarak döner
  (`GET /api/v1/ingest/jobs/status`) ama basılmış fiş değişmez.
- Eski sürümlerde bu belgede geçen `default_invoice_serial` parametresi hiç var olmadı (bkz. § 2 düzeltmesi);
  ERP'ye yazılan telefon belgesinin serisini **belirlemez**.

---

## 2. Parametre Altyapısı (kod okunarak doğrulandı, 2026-09-18)

> **Düzeltme:** Bu bölüm daha önce `POST /api/v1/android/parameters` ucunu `receipt_header_title`,
> `printer_paper_width`, `auto_print_receipt` gibi alanlarla anlatıyordu. **Böyle bir sözleşme hiç
> var olmadı.** Doğrulanmış gerçek durum aşağıdadır. (Madde 1'deki `default_invoice_serial`
> göndermesi de bu uydurma sözleşmeye aitti.)

### Gerçek uçlar
| Uç | Yön | Not |
|---|---|---|
| `POST /api/v1/ingest/parameters` | ajan → merkez | API anahtarı; gövde `{ sourceDatabase, parameters[] }` |
| `GET /api/v1/android/parameters?sourceDatabase=` | merkez → telefon | `{ tenantId, count, items[] }` |
| `GET /api/v1/admin/parameters` | merkez → Admin | sayfalı; `program`, `user`, `sourceDatabase` süzgeçleri |

Satır şeması (`ParameterRecord` — Fora'nın `_FORA_PARAMETRELER` tablosunun aynası):
`ParametreProgram` · `ParametreUser` · `ParametreAnaGrubu` · `ParametreAltGrubu` · `ParametreID` ·
`ParametreAdi` · `ParametreDegeri`.

### Bilinen boşluklar
- `_ERPB_PARAMETRELER` tablosu **hiçbir veritabanında yok**; ingest ucunu besleyen ajan kodu da yok.
  Uçlar bugün boşta çalışıyor.
- Sipariş Cepte `/api/v1/android/parameters`'ı **hiç çağırmıyor**.
- **Varsayılan katalog kavramı yok.** Fora'da varsayılanlar uygulamanın içindedir, tabloda yalnız
  *sapmalar* tutulur; katalog olmadan tablodaki satırlar efektif ayarı vermez.

Kapatma planı: [`docs/GOAL_PARAMETRE_YONETIMI.md`](../docs/GOAL_PARAMETRE_YONETIMI.md).

### Fora'nın parametre modeli (referans — `MikroDB_V16_03` ve decompile üzerinden)
Tek tablo, mantıksal anahtar **(Program, User, AnaGrubu, AltGrubu, ParametreID)**. `ParametreID` int
ve gerçek anahtardır; `ParametreAdi` yalnız okunabilirlik için tutulur, hiçbir sorgu ona bakmaz.
`ID` kolonu V15'te `int IDENTITY`, **V16'da `uniqueidentifier`**.

Yazım semantiği (`ParametreData.ParametreYaz`): değer varsayılana eşitse satır **silinir** (veya hiç
yazılmaz), farklıysa insert/update. Okuma simetrik: katalog varsayılanları belleğe yüklenir, DB
satırları üzerine bindirilir. Telefon tarafı SQLite'ta aynısını yapar (`ParametreSqlite`).

Katalog: `Fora.Mikro.ParametreTanimlari.ParametrelerDefault` — 13 program, **4.688 tanım**.
En büyüğü `akilli` (mobil kullanıcı ayarları): **kullanıcı başına 1801 parametre**; Fora'nın kendi
düzenleme ekranı 63 sekme ve 3.572 kontrol bağlaması içerir.

`YaziciAyarlari` programı kapsamı farklı kullanır: `ParametreUser` = **şablon adı** (kullanıcı değil),
`AnaGrubu` ∈ {`GenelAyarlar`, `Alan`}, `AltGrubu` = alan adı. `GenelAyarlar` 9 parametre
(`SayfaKolonSayisi` vars. 120, `SayfaSatirSayisi` 60, `DetayBaslangicSatiri` 15 …); her alan 16
parametre (`BasilacakAlan` UstBaslik/Satir/AltBaslik, `Kolon`, `Satir`, `Genislik`, `Hizalama`
Sol/Orta/Sag, `OndalikHaneSayisi`, `OnEk`, `SonEk` …). Yani düz form değil, karakter ızgarası
üzerinde bir **şablon tasarımcısı**.

Mobil kullanıcı listesi de bu tablodan türetilir:
`SELECT ParametreUser FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' GROUP BY ParametreUser`.
Şifre de parametredir (`ParametreID=1`, sabit gömülü anahtarla şifreli) — **ErpBridge bunu taşımaz**.

Tabloda 2 tetikleyici vardır; değişiklikler `_FORA_SYNC` (`TriggerRECno`, `TabloID`, `KayitGuid`)
değişim günlüğüne düşer ve telefona normal senkron kanalıyla gider.

---

## 3. Resmî E-Fatura / E-İrsaliye Süreçleri ve Bilgi Fişi Ayrımı

> **ERP yazım goal'ü kararı (K15, 2026-09-17):** telefon belgesi Mikro'ya e-belge ayrımı yapılmadan yazılır; e-Fatura /
> e-Arşiv / e-İrsaliye ofiste Mikro'dan düzenlenir. Telefonda e-belge mükellefi müşteriyi ayırıp bilgi fişi basmak ayrı
> bir goal'dür — aşağıdaki modlar hedef tasarımdır, bu goal'de uygulanmadı.

1. **Bilgi Fişi Modu (Thermal Receipt):**
   Sahada e-Fatura / e-Arşiv mükellefi olan işletmeler için termal yazıcıdan çıkan belge mali bir fatura değil, müşteri bilgilendirme ve teslim tesellüm fişidir. Üzerinde *"Bu belge mali değer taşımaz, e-Fatura merkezden düzenlenecektir"* ibaresi yer alır.
2. **Matbu Evrak Modu:**
   İnternetsiz ortamda matbu kağıda basılan irsaliyeli faturalarda, mobil uygulama basılan seri ve sıra numarasını `externalId` ve evrak no olarak ErpBridge'e gönderir. ErpBridge bu numarayı Mikro'nun ilgili tablosuna aynen yazar.
