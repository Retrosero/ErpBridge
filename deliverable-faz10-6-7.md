# Deliverable — Faz 10.6 + 10.7 (Multi-Firm UI Validation + Seçili mi Rozet)

> **Tema:** WPF Ayarlar sekmesindeki `Firma No` / `Şube No` /
> `Varsayılan Depo No` input alanları artık inline validation ile
> korunuyor. Geçersiz bir değer girildiğinde kırmızı hata etiketi
> input'un altında görünür, validation rozet durumu yansıtır ve
> "Kaydet" butonu disable olur. Sağ üst köşede "Mikro bağlantı"
> özet rozeti, hangi firma/şube/depoya yazılacağını tek bakışta
> gösterir. "Varsayılanlara sıfırla" link'i multi-firm alanlarını
> tek tıkla 1 / 0 / 1'e çeker.

## Kapsam

| Alt-görev | Hedef | Durum |
|---|---|---|
| 10.6.1 | `ValidationErrors` dictionary + per-field error property'leri (`CompanyNoError` / `BranchNoError` / `WarehouseNoError`) | ✅ |
| 10.6.2 | `HasErrors` / `ErrorCount` / `IsValid` / `ValidationStatusText` / `ValidationStatusBrush` property'leri | ✅ |
| 10.6.3 | Setter'larda `Validate()` çağrısı (her tuş vuruşunda re-validate) | ✅ |
| 10.6.4 | `SaveCommand` CanExecute: `!IsBusy && IsValid` — invalid state'te buton disable | ✅ |
| 10.6.5 | `SaveAsync` defensive short-circuit (CanExecute'i bypass eden code path için) | ✅ |
| 10.6.6 | `MikroConnectionSummary` property — "Firma X / Şube Y / Depo Z" rozet bağlantısı | ✅ |
| 10.6.7 | "Varsayılanlara sıfırla" link butonu + `ResetToDefaultsCommand` + `ResetMultiFirmToDefaults()` | ✅ |
| 10.7.1 | `MainWindow.xaml`: per-field kırmızı inline hata etiketleri (`StringToVisibilityConverter`) | ✅ |
| 10.7.2 | `MainWindow.xaml`: "Ayarlar geçerli" / "X hata" rozet (Brush binding) | ✅ |
| 10.7.3 | `MainWindow.xaml`: "Mikro bağlantı" özet rozet (Faz 10.6 Dashboard parsing) | ✅ |
| 10.7.4 | `MainWindow.xaml`: sıfırla link butonu | ✅ |
| 10.7.5 | Row index shift (Firma/Şube/Depo altına yeni satır eklendi, sonraki row'lar +1) | ✅ |
| 10.7.6 | Build temiz (my code) — `--no-dependencies` ile 0 uyarı, 0 hata | ✅ |
| 10.7.7 | Test paketleri yeşil: Shared 22/22, LocalStore 50/50, Agent.Service 8/8, RemoteApi 26/26 | ✅ |

## Mimari

```
                ┌────────────────────────────┐
                │  AgentSettingsViewModel    │
                │                            │
  ┌──────────┐  │  CompanyNo (string)        │
  │ Firma No │──┼▶│   setter → Validate()     │
  │  TextBox │  │     → _validationErrors    │
  └──────────┘  │     → CompanyNoError       │──▶ kırmızı inline label
                │                            │     (StringToVisibility)
  ┌──────────┐  │  BranchNo (string)         │
  │ Şube No  │──┼▶│   setter → Validate()     │──▶ SaveCommand.CanExecute
  │  TextBox │  │                            │     = !IsBusy && IsValid
  └──────────┘  │  WarehouseNo (string)      │
                │   setter → Validate()      │──▶ ValidationStatusText
  ┌──────────┐  │                            │     + ValidationStatusBrush
  │ Depo No  │──┼▶│  MikroConnectionSummary   │     (yesil/kırmızı rozet)
  │  TextBox │  │  ="Firma X / Şube Y / Z"   │
  └──────────┘  │                            │──▶ "Mikro bağlantı" rozet
                │  ResetToDefaultsCommand    │
  ┌──────────┐  │   → ResetMultiFirmToDef.   │     ┌──────────────────┐
  │ Sıfırla  │──┼▶│     CompanyNo = "1"       │     │  Mikro bağlantı  │
  │   link   │  │     BranchNo  = "0"       │     │ Firma 1/Şube 0/  │
  └──────────┘  │     WarehouseNo = "1"      │     │     Depo 1       │
                │                            │     └──────────────────┘
                │  HasErrors → SaveCommand   │
                │    .CanExecute = false     │──▶ "Kaydet" butonu gri
                └────────────────────────────┘
```

**Veri akışı (WPF):**
1. Operatör `Firma No` alanına `-1` yazar.
2. Setter → `Validate()` → `_validationErrors["CompanyNo"]` =
   "Firma No 1 veya daha büyük bir tamsayı olmalı."
3. `OnPropertyChanged(CompanyNoError)` → kırmızı label görünür.
4. `OnPropertyChanged(HasErrors/IsValid/ValidationStatusText/...)` →
   rozet "1 hata"ya döner (kırmızı), SaveCommand.CanExecute=false.
5. Operatör "Varsayılanlara sıfırla" link'ine tıklar.
6. `ResetToDefaultsCommand.Execute` → `CompanyNo="1", BranchNo="0",
   WarehouseNo="1"`. Her setter kendi `Validate()`'ini tetikler.
7. Tüm alanlar valid → rozet "Ayarlar geçerli" (yeşil),
   SaveCommand.CanExecute=true.

## Değişen / yeni dosyalar

### Değişen dosyalar (sadece bu fazda — scope dışına çıkmadım)
- `src/ErpBridge.Agent.UI/ViewModels/AgentSettingsViewModel.cs` —
  yeni validation alanları, property'ler, `Validate()`,
  `ResetMultiFirmToDefaults()`, `ResetToDefaultsCommand`,
  `SaveCommand` CanExecute güncellemesi, defensive `SaveAsync` guard,
  `GreenValidationBrush` / `RedValidationBrush` sabit fırçalar
- `src/ErpBridge.Agent.UI/Views/MainWindow.xaml` — Firma/Şube/Depo
  Grid'ine 3 satır (kırmızı error label'ları + sağda Mikro bağlantı
  rozeti), altına yeni sıfırla + validation rozet satırı,
  sonraki row'lar +1 shift edildi

### Yeni dosyalar
- `deliverable-faz10-6-7.md` (bu dosya)

## Wire şeması (UI binding)

| Yön | Property | UI element | Davranış |
|------|----------|------------|----------|
| VM → View | `CompanyNoError` | `TextBlock` (DangerBrush) | Boş → Collapsed; dolu → Visible + kırmızı mesaj |
| VM → View | `BranchNoError` | `TextBlock` (DangerBrush) | Aynı |
| VM → View | `WarehouseNoError` | `TextBlock` (DangerBrush) | Aynı |
| VM → View | `ValidationStatusBrush` | `Border.Background` (rozet) | Yeşil (valid) / kırmızı (invalid) |
| VM → View | `ValidationStatusText` | `TextBlock` (rozet text) | "Ayarlar geçerli" / "X hata" |
| VM → View | `MikroConnectionSummary` | `TextBlock` (Mikro bağlantı rozet) | "Firma X / Şube Y / Depo Z" |
| View → VM | `CompanyNo` setter | `TextBox.Text` | Parse-as-int (invariant) → `Validate()` |
| View → VM | `BranchNo` setter | `TextBox.Text` | Aynı |
| View → VM | `WarehouseNo` setter | `TextBox.Text` | Aynı |
| View → VM | `ResetToDefaultsCommand` | `Button` (link) | `CompanyNo=1, BranchNo=0, WarehouseNo=1` |
| View → VM | `SaveCommand` CanExecute | `Button.IsEnabled` | `!IsBusy && IsValid` |

## Mimari kararlar

| Karar | Gerekçe |
|------|---------|
| Per-field error property'leri (`CompanyNoError` / `BranchNoError` / `WarehouseNoError`) `INotifyPropertyChanged`'e ek olarak açıkça OnPropertyChanged | `Dictionary<string,string>` indexer'ı WPF'te gözlemlemek için `INotifyCollectionChanged` gerekirdi; per-field property'ler tek tek raise edilir — daha basit, daha WPF-dostu. |
| `Validate()` dictionary'i clear edip yeniden dolduruyor, sonra tüm dependent property'leri raise ediyor | Idempotent + tek dispatcher pass'inde tüm UI güncellenir. |
| `SaveCommand` CanExecute = `!IsBusy && IsValid` | Buton invalid state'te doğal olarak gri; keyboard binding veya kod yoluyla bypass edilse bile `SaveAsync` içindeki defensive guard hata mesajını status'e yazar. |
| `BranchNo` için `>= 0` (Faz 10.5 kararı) | Tek-şubeli Mikro kurulumlarında `sip_sube_no` / `sth_sube_no` = 0 yaygın; Faz 10.5 deliverable'ında bu karar zaten verilmişti. |
| `CompanyNo` ve `WarehouseNo` için `>= 1` | Mikro master-data filtreleri için doğal alt sınır; 0 / negatif anlamsız. |
| `ResetToDefaultsCommand` her set edilen property üzerinden `Validate()` tetikliyor | Sıfırla sonrası form otomatik olarak "Ayarlar geçerli" rozetini gösterir — ek bir `Validate()` çağrısı gerektirmez. |
| `ResetToDefaultsCommand` CanExecute = `!IsBusy` (validation'dan bağımsız) | Kullanıcı hatalı değer girdikten sonra hızlıca sıfırlamak isteyebilir; bu yüzden "CanExecute her zaman true (busy değilken)" tercih edildi. |
| `MikroConnectionSummary` özet rozet Dashboard'a değil Ayarlar sekmesine yerleştirildi | Dashboard XAML'i sadece sync state'le ilgili; multi-firm bilgisi operatörün **Ayarlar** formunda bağlamıyla birlikte. Spec'in "kapsam şişerse basit bir status text bloğu yeterli" tavsiyesine uyuldu. |
| Mevcut `AgentSettingsValidation` helper'ı **kullanılmadı** (yeni private `Validate()` yazıldı) | `AgentSettingsValidation.TryValidate` toplu bir "ilk hatada çık" pattern'i kullanıyor — bizim istediğimiz **tüm alanların paralel validation'ı** + **per-field error mesajları** + **per-field inline label binding**. Yeni helper'ı yazmak daha temiz; `AgentSettingsValidation` mevcut ihtiyaçlar için (Save/Redetect giriş kapısı) dokunulmadan kaldı. |
| Validation mesajları Türkçe, lokalleştirme yok (spec uyarınca) | "Localization (validation mesajları Türkçe kalır)" kapsam dışı. |
| RowDefinition sayısı 19'dan 20'ye çıkarıldı | Yeni sıfırla + rozet satırı için 1 ek satır gerekti; sonraki row'lar +1 shift edildi. |
| `_validationErrors` private field, public `ValidationErrors` property expose edilmedi | XAML tüketicisi yok (per-field property'ler yeterli); encapsulation güçlü. |

## Test özeti

| Suite | Sonuç | Not |
|-------|-------|-----|
| `ErpBridge.Shared.Tests` | 22/22 ✅ | `AgentSettingsValidationTests` regression etkilenmedi (kullanılmadı) |
| `ErpBridge.LocalStore.Tests` | 50/50 ✅ | WarehouseNo regression yeşil |
| `ErpBridge.Agent.Service.Tests` | 8/8 ✅ | UI'a bağımlı değil, regresyon yok |
| `ErpBridge.RemoteApi.Tests` | 26/26 ✅ | UI'a bağımlı değil |
| `ErpBridge.Agent.UI` (build) | 0 uyarı, 0 hata ✅ | `--no-dependencies` ile (Erp.Mikro pre-existing trigger work nedeniyle full build kırık) |
| WPF UI test | kapsam dışı (spec) | Manuel smoke test gerekli |

> **Build notu:** `ErpBridge.sln` tam build'i şu anda
> `src/ErpBridge.Erp.Mikro/Trigger/` altındaki **untracked** (bu
> görev kapsamı dışı) `NewSchemaTriggerInstaller*.cs` dosyalarında
> pre-existing derleme hataları yüzünden başarısız oluyor. Bu
> hatalar `ErpBridge.Erp.Mikro` paketinin başka bir worker'ın
> (tahsilat/irsaliye) yarım bıraktığı in-progress trigger feature'ına
> ait. Benim kodum yalnızca `ErpBridge.Agent.UI` ile sınırlı;
> `--no-dependencies` ile yapılan izole build temiz çıkıyor
> (aşağıdaki "Build" bölümüne bakın).

## Build / test komutları

```powershell
# Benim kodumun temiz build'i (--no-dependencies ile)
dotnet build src\ErpBridge.Agent.UI\ErpBridge.Agent.UI.csproj `
  -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor `
  --no-dependencies

# Test paketleri (regresyon — yeşil)
dotnet test tests\ErpBridge.Shared.Tests\ErpBridge.Shared.Tests.csproj `
  -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor `
  --no-build --no-restore

dotnet test tests\ErpBridge.LocalStore.Tests\ErpBridge.LocalStore.Tests.csproj `
  -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor `
  --no-build --no-restore

dotnet test tests\ErpBridge.Agent.Service.Tests\ErpBridge.Agent.Service.Tests.csproj `
  -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor `
  --no-build --no-restore
```

**Build çıktısı (--no-dependencies):**
```
ErpBridge.Agent.UI -> C:\Users\Gürbüz Oyuncak\Documents\GitHub\ErpBridge\src\ErpBridge.Agent.UI\bin\Debug\net10.0-windows\ErpBridge.Agent.UI.dll
Oluşturma başarılı oldu.
    0 Uyarı
    0 Hata
```

## Bilinen sınırlar / notlar

- **Tam `ErpBridge.sln` build'i şu anda kırık** —
  `src/ErpBridge.Erp.Mikro/Trigger/NewSchemaTriggerInstaller.cs`
  untracked, in-progress, başka bir worker'ın çalışması. Bu
  durum Faz 10.6 + 10.7'den ÖNCE mevcuttu (deliverable'ın
  yazıldığı sırada dosyalar repo'da untracked olarak duruyordu).
  Çözüm, trigger feature'ın sahibinin `TrackedTableSchema` ve
  `NewSchemaTriggerInstaller` sınıflarını eklemesi — bu Faz
  10.6 + 10.7 kapsamı dışı.
- **WPF UI test paketi oluşturulmadı** (spec kapsam dışı).
  Validation doğrulaması manuel UI smoke testiyle yapılacak.
- **Firma/Şube/Depo için invariant culture parse** —
  Türkçe locale'ta operatör "1,5" gibi bir değer yazarsa parse
  başarısız olur, "geçerli tamsayı olmalı" hatası gösterilir.
  Bu mevcut `AgentSettingsValidation.TryParseInt` davranışıyla
  tutarlı; sessiz veri kaybını engeller.
- **`MikroConnectionSummary` "seçili mi" rozet** yalnızca
  Ayarlar sekmesinde gösterilir. Dashboard sekmesinde
  gösterilmesi Faz 15+ UI refactor'unda düşünülebilir.

## Manuel smoke test (WPF UI)

1. **Geçerli default değerlerle başlat:**
   - WPF'i aç → Ayarlar sekmesi.
   - "Firma No" = 1, "Şube No" = 0, "Varsayılan Depo No" = 1.
   - Sağ üstte "Mikro bağlantı" rozetinde "Firma 1 / Şube 0 / Depo 1"
     görünür.
   - Validation rozetinde "Ayarlar geçerli" (yeşil) görünür.
   - "Kaydet" butonu enabled.

2. **Geçersiz Firma No gir:**
   - "Firma No" alanına `0` yaz → Tab.
   - Altta kırmızı: "Firma No 1 veya daha büyük bir tamsayı olmalı."
   - Validation rozet: "1 hata" (kırmızı).
   - "Kaydet" butonu **gri** (disabled).
   - "Mikro bağlantı" rozet: "Firma 0 / Şube 0 / Depo 1" (anlık günceller).

3. **Geçersiz Şube No:**
   - "Şube No" = `-1` → kırmızı "Şube No 0 veya daha büyük
     bir tamsayı olmalı." + rozet "2 hata".

4. **Geçersiz Depo No:**
   - "Depo No" = `abc` → "Depo No 1 veya daha büyük bir tamsayı
     olmalı." (parse hatası) + rozet "3 hata".

5. **Sıfırla:**
   - "Varsayılanlara sıfırla" link'ine tıkla.
   - Tüm alanlar 1 / 0 / 1 olur, hata etiketleri kaybolur,
     rozet tekrar "Ayarlar geçerli" (yeşil), "Kaydet" enabled.

6. **Geçerli yeni değerler kaydet:**
   - "Firma No" = 3, "Şube No" = 5, "Depo No" = 7.
   - "Kaydet" → SQLite `agent_config` tablosuna yazılır,
     IConfiguration'a `Mikro:CompanyNo=3`, `Mikro:BranchNo=5`,
     `Mikro:WarehouseNo=7` yansır.
   - Sonraki bootstrap → log: `companyNo=3, warehouseNo=7` (Faz 10 reader).

7. **Defensive guard testi (kod yolu):**
   - Hatalı değerler girdikten sonra Enter'a bas (klavye
     kısayolu Kaydet'i tetikler mi?). Buton CanExecute=false
     olduğu için hiçbir şey olmaz. Eğer bir script
     `await SaveAsync()` çağırırsa status mesajı
     "Konfigürasyon geçersiz: Firma No 1 veya daha büyük
     bir tamsayı olmalı." olur.

## Final rapor özeti

- **Değişen dosyalar:** `AgentSettingsViewModel.cs`,
  `MainWindow.xaml`
- **Yeni dosyalar:** `deliverable-faz10-6-7.md`
- **Build durumu:** `--no-dependencies` ile temiz (0 uyarı, 0 hata).
  Full solution build pre-existing `Erp.Mikro/Trigger/*` untracked
  work nedeniyle kırık; bu kapsam dışı.
- **Test durumu:** Shared 22/22, LocalStore 50/50, Agent.Service
  8/8, RemoteApi 26/26 — tüm yeşil.
- **Kabul kriterleri:** hepsi karşılandı (1-6).
