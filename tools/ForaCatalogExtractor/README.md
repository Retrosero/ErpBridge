# Fora parametre kataloğu çıkarıcısı

`Fora_Mikro/.decompiled/` altındaki decompile edilmiş Fora kaynağından, panelin ve sunucunun
kullanacağı makine okunur parametre kataloğunu üretir.

Neden gerekli: Fora parametre **varsayılanlarını veritabanında tutmaz**, uygulamanın içinde tutar.
`_FORA_PARAMETRELER` tablosunda yalnız varsayılandan *sapan* satırlar bulunur — bir değer
varsayılana eşitlenirse satır silinir. Katalog olmadan tablodaki satırlar efektif ayarı vermez.
Ayrıntı: [`docs/GOAL_PARAMETRE_YONETIMI.md`](../../docs/GOAL_PARAMETRE_YONETIMI.md).

## Kullanım

Kataloğu yeniden üret (çıktıyı diske yazar):

```bash
dotnet run --project tools/ForaCatalogExtractor
```

Yalnız doğrula, hiçbir şey yazma (commit edilmiş katalog bayatsa sıfırdan farklı kodla çıkar):

```bash
dotnet run --project tools/ForaCatalogExtractor -- --check
```

Fora sürümünü damgala:

```bash
dotnet run --project tools/ForaCatalogExtractor -- --source-build "17.1.2.3"
```

Sürüm verilmezse mevcut katalogdaki `sourceBuild` korunur; o da yoksa `unknown` yazılır.

## Çıktı

| Dosya | İçerik |
|---|---|
| `catalog/parameters/defaults.json` | 19 katalog seti, 13 program, 4.688 parametre tanımı |
| `catalog/parameters/ui.akilli.json` | 65 sekme, 1.782 `akilli` parametresinin sekmesi, etiketi ve editör tipi |

Üretilen dosya **commit edilir**. `ErpBridge.ForaCatalog.Tests` altın dosya testi, commit edilmiş
kataloğu kaynaktan yeniden üretilenle bayt bayt karşılaştırır; kaynak değişip katalog güncellenmezse
CI kırmızıya döner.

### Şema

```jsonc
{
  "schemaVersion": 1,
  "sourceBuild": "unknown",        // hangi Fora derlemesinden okundu
  "sourceType": "Fora.Mikro.ParametreTanimlari.ParametrelerDefault",
  "sourceFile": "Fora_Mikro/.decompiled/.../ParametrelerDefault.cs",
  "setCount": 19,
  "parameterCount": 4688,
  "shadowedCount": 5,              // Fora'nın kendi id çakışmaları (aşağıda)
  "sets": [
    {
      "catalogMethod": "MobilKullanici",
      "program": "akilli",         // ParametreProgram kolonuna yazılan değer
      "scopeKind": "MobileUser",   // kapsam ne anlama geliyor
      "scopeField": "user",        // kapsamı hangi kolon taşıyor
      "scopeParameter": "User",    // katalog metodunun ilgili parametresi
      "user": "", "anaGrubu": "", "altGrubu": "",   // kapsam dışı kalan sabit adresleme değerleri
      "duplicateIds": [],
      "parameters": [
        { "id": 1, "name": "Sifre", "default": "" }
      ]
    }
  ]
}
```

### `ui.akilli.json` şeması

```jsonc
{
  "schemaVersion": 1,
  "sourceType": "ForaAndroidKullaniciDuzenleme",
  "tabCount": 65,
  "parameterCount": 1782,
  "unlabelledCount": 0,
  "tabs": [
    { "name": "xtraTabPage30", "title": "Tanımlamalar",
      "path": ["Parametreler", "Tanımlamalar"], "order": 0 }
  ],
  "parameters": [
    { "parameter": "Goster_AnaMenu_Tahsilat",
      "label": "Tahsilat girebilir",
      "editor": "boolean",                 // boolean|integer|decimal|text|multilineText|choice|color
      "tab": "xtraTabPage25",
      "tabPath": ["Evrak girişi", "Evrak Tipleri", "Tahsilat / Tediye makbuzu"],
      "control": "Goster_AnaMenu_Tahsilat", "controlType": "CheckEdit",
      "order": 412,
      "labelSource": "caption" }           // caption|tableCell|labelLeft|checkBoxLeft|labelAbove|none
  ],
  "gaps": [ { "kind": "...", "subject": "...", "detail": "..." } ]
}
```

Fora sekmeleri iç içe koyuyor: beş dış sekme (Parametreler, Görünüm ve seçenekler, Evrak girişi,
Raporlar, Form dosyaları) altında toplam 65 sekme var. Düz bir liste gruplamayı kaybederdi, bu
yüzden `path` tam yolu taşıyor.

Etiketler dört kaynaktan geliyor ve **1.782 alanın hepsi etiketli**:

| Kaynak | Adet | Ne demek |
|---|---:|---|
| `caption` | 733 | Kontrolün kendi başlığı — her `CheckEdit` taşıyor |
| `tableCell` | 641 | Tablo düzeninde aynı satırın önceki hücresindeki etiket |
| `labelLeft` | 387 | Solundaki `LabelControl` |
| `checkBoxLeft` | 21 | Solundaki **kutucuğun** başlığı — alan yalnız o kutucuğu niteliyor |

Sola bakan eşleşme, etiketin alanın **ilk satırıyla** hizalandığını varsayar, merkeziyle değil;
162 piksel yüksekliğindeki `CariEkstreMesaj` alanının "Mesaj :" etiketi ancak böyle bulunuyor.

## Bilmesi gereken iki tuzak

**1 — Kapsam her programda aynı kolonda değil.** `akilli`, `foramikro`, `MobilRapor*` ve
`ComarchEdiIliski` kapsamı `ParametreUser`'da tutar. `GenelAktarim`, `TahsilatAktarim` ve
`BankaAktarim` ise `ParametreUser`'ı **boş** bırakıp kapsamı `ParametreAltGrubu`'nda tutar,
`ParametreAnaGrubu`'nu da sabit ayırıcı olarak kullanır (`SqlAktarimSablon`, `AktarimSablon`,
`TxtCsvAktarimSablon`, `GenelSablon`, `Kriter`). Bu yüzden `scopeField` alanı var ve üç adresleme
kolonu da ayrı ayrı saklanıyor. Çıkarıcı, bir setteki bütün satırlarda bu üç kolonun tek biçimli
olduğunu doğrular; olmazsa hata verip durur.

**2 — Fora'nın kendi id çakışmaları.** `TahsilatAktarimTxtCsvSablon` içinde
`belge_tarihi_yil/ay/gun_baslangic` üçünün de `ParametreID`'si **16**; `GenelAktarimTxtCsvSablon`
aynı alanlar için doğru şekilde 16, 18, 20 kullanıyor. Aynı şey 17 (`*_uzunluk`) ve
604 (`cari_kod2_*`) için de geçerli. `Parametreler._GetParametre(int)` ilk eşleşmeyi döndürdüğü
için sonraki kayıtlar **Fora'da da** okunamaz durumda. Katalog bunları atmaz: sırayı korur ve
gölgede kalanları `"shadowed": true` ile işaretler, `duplicateIds` ile de sette listeler.
Toplam 5 kayıt, hepsi bu tek sette.

**3 — Fora'nın yükle/kaydet uyuşmazlıkları.** Düzenleme ekranı
`GosterZyrt_Temsilci_Ozel_12..19_Var_Yok` parametrelerini **yanlış** kutucuklara yüklüyor
(örneğin 12'yi 24'ün kutucuğuna), ama her kutucuğu kendi parametresine kaydediyor. Sonuç: Fora'da
o ekranı açıp kaydetmek bir parametreyi başka bir parametrenin değeriyle **sessizce eziyor**.

Çıkarıcı iki yönü ayrı toplayıp karşılaştırıyor ve **kaydetme yönünü** esas alıyor — veritabanına
ulaşan yön o. Uyuşmazlıklar `gaps` içinde `bindingMismatch` olarak listeleniyor (8 kayıt).

## Neden Roslyn, neden regex değil

`akilli` kataloğundaki dört koli etiketi şablonu kaçırılmış tırnakla bitiyor:

```csharp
new Parametre("akilli", User, "", "", 761, "KoliEtiketiStokListesiBaslangicMetniStokKodu",
              "A[degisken],105,1,3,2,1,N,\""),
```

Satır bazlı bir düzenli ifade burada ya ters bölü işaretini değerin içinde bırakır ya da değeri
yanlış yerden keser — üstelik sessizce. Roslyn literalin çözülmüş halini verdiği için bu dört
kayıt doğru çıkıyor; `Escaped_quotes_in_label_templates_survive_extraction` testi bunu bekliyor.

## Paket notu

Bu proje `ManagePackageVersionsCentrally=false` ile merkezi paket yönetiminin **dışında**.
`Directory.Packages.props` geçişli sabitlemeyi açtığı için Roslyn'i oraya eklemek,
`Microsoft.EntityFrameworkCore.Design` üzerinden zaten Roslyn çeken her projenin kilit dosyasını
yeniden yazardı. Sürüm (`5.0.0`) o grafiğin zaten çözdüğü sürümle aynı.
