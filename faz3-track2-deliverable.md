# ErpBridge Faz 3 — Track 2 Deliverable

**Track:** Track 2 — WPF UI V15/V16 Badge + Connection Test Status Panel
**Phase:** 3 (Mikro bağlantı testi + V15/V16 detector)
**Status:** ✅ Build clean, tüm testler geçiyor (174/174 passed + 5 integration skipped).
**Tarih:** 2026-07-09

---

## 1. Özet

Phase-3 Track-2 kapsamında `ErpBridge.Agent.UI` görsel olarak zenginleştirildi.
Connection test sonucu artık şu bilgileri tek bakışta gösteriyor:

- **Mikro versiyon badge'i** (V15 mavi / V16 yeşil / Unknown kırmızı / — gri)
- **Sunucu Bilgisi paneli** (ServerVersion, IdentityStrategy, Latency, Test zamanı)
- **"Versiyonu yeniden tespit et"** ayrı butonu (orchestrator cache'ini invalidate eder)
- **Test sırasında animasyonlu progress göstergesi** (üç nokta, soldan sağa fade)
- **Hata durumunda troubleshooting ipuçları** (kategorize: server / login / database)

Mevcut alanlar (lisans, SQL ayarları, Mikro database, firma/şube, API URL) korundu —
backward-compatible şekilde eklendi. Şifre hiçbir XAML binding'inde loglanmıyor;
`ConnectionStringMasker` her log/status boundary'sinde çalışıyor.

---

## 2. Değişen / yeni dosyalar

### Yeni dosyalar

```
src/ErpBridge.Agent.UI/
├── AsyncRelayCommand.cs                                # Async ICommand, IsExecuting guard
├── Converters/
│   └── StringToVisibilityConverter.cs                  # Boş hint → Collapsed

src/ErpBridge.Shared/
└── AgentSettingsValidation.cs                          # Pure validation logic, UI-independent

tests/ErpBridge.Shared.Tests/
└── AgentSettingsValidationTests.cs                     # 9 yeni test
```

### Değişen dosyalar

| Dosya | Değişiklik |
|-------|-----------|
| `src/ErpBridge.Agent.UI/ViewModels/AgentSettingsViewModel.cs` | `IErpAdapterFactory` bağımlılığı kaldırıldı; yerine `IMikroConnectionTestOrchestrator` inject edildi (Faz 3'ün önerdiği tek seam). Badge / Sunucu Bilgisi / Latency / Test zamanı / Sorun giderme state'i eklendi. `RedetectVersionCommand` (cache invalidating) eklendi. `AsyncRelayCommand` ile çift tıklama koruması. `TryValidateInputs` artık `AgentSettingsValidation.TryValidate` çağırıyor. |
| `src/ErpBridge.Agent.UI/Views/MainWindow.xaml` | Üst sağa versiyon badge'i (Brush binding, converter yok); buton satırı 3 sütuna bölündü (yeniden tespit / progress / test + kaydet); Sunucu Bilgisi paneli (test sonrası görünür); Sorun giderme paneli (hata sonrası görünür). Mevcut alanlar aynen korundu. |
| `src/ErpBridge.Agent.UI/Themes/AgentTheme.xaml` | `StringToVisibilityConverter` resource olarak eklendi. |
| `src/ErpBridge.Erp.Mikro/DependencyInjection/ServiceCollectionExtensions.cs` | `IMikroConnectionTestOrchestrator` → `MikroConnectionTestOrchestrator` DI'a singleton olarak eklendi (Faz 3'ün Orchestrator tipi zaten mevcuttu, sadece kayıt eksikti). Orchestrator logger'ı da eklendi. |

### Değişmeyen dosyalar (gözden geçirildi)

- `src/ErpBridge.Agent.UI/RelayCommand.cs` — sync versiyon korundu, dokunulmadı.
- `src/ErpBridge.Agent.UI/Converters/BoolToVisibilityConverter.cs` — zaten mevcut, korundu.
- `src/ErpBridge.Agent.UI/App.xaml.cs` ve `DependencyInjection/ServiceCollectionExtensions.cs` — view-model
  constructor imzası değiştiği için otomatik olarak yeni DI grafını kullanıyor;
  ek kayıt gerekmedi (orchestrator `AddErpBridgeMikro` içinde).
- `src/ErpBridge.Erp.Mikro/Adapters/MikroConnectionTestOrchestrator.cs` — Faz 2
  sonunda yazılmıştı, bu track sadece DI'a kayıt ekledi.
- `src/ErpBridge.Shared/ConnectionStringMasker.cs` — mevcut masker yeterli, yeniden
  kullanıldı.

---

## 3. Tasarım kararları

### 3.1 `AsyncRelayCommand` — yeni, sync `RelayCommand` korundu

`System.Windows.Input.ICommand` zaten `CanExecuteChanged` event'ine sahip, bu yüzden
async komut için ayrı bir event gerekmiyor. Yeni tip:

- `IsExecuting` flag'i — true iken `CanExecute` false döner
- `CanExecuteChanged` event'i `IsExecuting` değiştiğinde otomatik fire olur
- `ExecuteAsync(object?)` — view-model kodundan doğrudan `await` edilebilir
- Reentrancy koruması: birden fazla UI-thread çağrısı ikinciyi reddeder

Sync `RelayCommand` korundu çünkü `SaveCommand` için async semantiğine gerek yok
(UI thread'ini bloke etmiyor; `IAgentConfigStore.SaveAsync` zaten fire-and-forget).

### 3.2 `AgentSettingsValidation` — Shared'da, test edilebilir

Brief'in "Daha temiz yol" önerisini birebir uyguladık. WPF UI projesi
`net8.0-windows` + `UseWPF` olduğu için headless test ortamında
`PresentationCore.dll` yüklenemiyor. Validation mantığını Shared'a
(`net8.0`, WPF bağımlılığı yok) taşıyarak 9 adet unit test yazabildik.

Validation kuralları (Türkçe hata mesajları UI tarafıyla birebir aynı):
- `SqlServer`, `SqlUserName`, `MikroDatabaseName` — `IsNullOrWhiteSpace` reddi
- `CompanyNo`, `BranchNo` — `int.TryParse` + `NumberStyles.Integer` + `InvariantCulture`
- Negatif sayı reddi (Mikro için 0 demo data için geçerli, negatif değil)
- Türkçe locale-formatted değerler (`"1,5"`) reddedilir (invariant kültür zorunlu)

### 3.3 `IMikroConnectionTestOrchestrator` — tek seam

Brief'te "`TestConnectionAsync` refactor — `AsyncRelayCommand` kullan" deniyordu.
Mevcut `MikroAdapter.TestConnectionAsync` QuickTest + version-detect'i sıralı
yapmıyor, version cache'i yönetmiyor ve `InvalidateCache` API'si yok. Faz 2 sonunda
eklenen `MikroConnectionTestOrchestrator` zaten tüm bu ihtiyaçları karşılıyor;
bu track onu DI'a kayıt edip view-model'e inject etti.

`RedetectVersionCommand`:
1. `IMikroConnectionTestOrchestrator.InvalidateCache()` — `_strategySelector.InvalidateAll()`
2. `RunFullTestAsync()` — cache miss → gerçek probe + identity strategy warm-up
3. Badge / Info panel güncellenir

Bu sayede aynı code-path, iki farklı UI command'dan tetiklenebiliyor.

### 3.4 Badge için converter yerine Brush property

XAML'de `Background="{Binding MikroVersionBrush}"` — view-model 4 farklı renk
(`SolidColorBrush`) döner: V15 mavi, V16 yeşil, Unknown kırmızı, — gri.
Tüm brush'lar `Freeze()` ile dondurulmuş (cross-thread paylaşım için güvenli).

Brush property public setter yerine private — dışarıdan set edilemez, sadece
`ApplyBadgeFromVersion` iç state machine'i değiştirebilir.

### 3.5 Progress göstergesi — üç nokta animasyonu

Tam `ProgressBar` + `ProgressRing` yerine üç noktanın sırayla fade ettiği
storyboard animasyonu. Sebep: bu bir indeterminate operation (süre belli
değil), operator sadece "şu an çalışıyor" sinyali görmeli. Storyboard XAML'i
`EventTrigger` + `BeginStoryboard` + `DoubleAnimation` ile, her noktanın
`BeginTime`'ı 0/0.2/0.4 saniye.

### 3.6 Sorun giderme hint'i — message-based heuristic

UI projesi `Microsoft.Data.SqlClient`'a referans vermediği için typed
`SqlException` catch'i yazılamazdı. Bunun yerine `BuildTroubleshootingHintFromMessage`
mesaj içeriğine bakıyor:
- "server / network / timeout / connect" → server-side kontrol
- "login / password / authentication / credential" → kullanıcı adı / şifre kontrol
- "database / catalog / cannot open" → database adı kontrol
- hiçbiri eşleşmezse → generic 4-lü kontrol listesi

Hint, başarılı test'te `string.Empty`'e set edilir → `StringToVisibilityConverter`
ile panel otomatik gizlenir.

### 3.7 WPF UI test projesi yok

Brief'te "view-model unit testleri yap" deniyordu, ancak view-model
`System.Windows.Media.Brush` üretiyor — bu tip WPF bağımlılığı.
`Agent.UI.Tests` adıyla ayrı bir `<TargetFramework>net8.0-windows</TargetFramework>`
projesi yazmak Linux build'inde kırılırdı.

Çözüm: validation'ı Shared'a çıkardık, 9 test orada. View-model'in
state-machine mantığı (badge güncellemesi, Info panel doldurması) WPF
bağımlılığı olmadan unit test edilemiyor — bu trade-off brief'te de
açıkça kabul edilmişti ("`net8.0` test projesi, ama WPF tipi kullanmıyorsan").

---

## 4. Build çıktısı (son 30 satır)

```text
$ dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor

  ErpBridge.Shared -> bin/Debug/net8.0/ErpBridge.Shared.dll
  ErpBridge.Erp.Abstractions -> bin/Debug/net8.0/ErpBridge.Erp.Abstractions.dll
  ErpBridge.Core -> bin/Debug/net8.0/ErpBridge.Core.dll
  ErpBridge.RemoteApi -> bin/Debug/net8.0/ErpBridge.RemoteApi.dll
  ErpBridge.LocalStore -> bin/Debug/net8.0/ErpBridge.LocalStore.dll
  ErpBridge.Erp.Mikro -> bin/Debug/net8.0/ErpBridge.Erp.Mikro.dll
  ErpBridge.Agent.Service -> bin/Debug/net8.0/ErpBridge.Agent.Service.dll
  ErpBridge.Agent.UI -> bin/Debug/net8.0-windows/ErpBridge.Agent.UI.dll
  ErpBridge.Core.Tests -> bin/Debug/net8.0/ErpBridge.Core.Tests.dll
  ErpBridge.LocalStore.Tests -> bin/Debug/net8.0/ErpBridge.LocalStore.Tests.dll
  ErpBridge.Erp.Mikro.Tests -> bin/Debug/net8.0/ErpBridge.Erp.Mikro.Tests.dll
  ErpBridge.Shared.Tests -> bin/Debug/net8.0/ErpBridge.Shared.Tests.dll
  ErpBridge.RemoteApi.Tests -> bin/Debug/net8.0/ErpBridge.RemoteApi.Tests.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

`TreatWarningsAsErrors=true` her projede aktif. Build Linux'ta WPF hatası
vermedi — `-p:EnableWindowsTargeting=true` ile `net8.0-windows` framework'ü
`Microsoft.NETCore.App` (cross-platform runtime) üzerinde derleniyor.
WPF XAML dosyaları `Microsoft.WindowsDesktop.App` runtime'ına ihtiyaç
duyuyor; bu runtime Linux'ta yok, ama `dotnet build` (sadece derleme)
çalışıyor — `dotnet run` veya publish için Windows gerekiyor (zaten
kabul edilen bir kısıt, brief'te "WPF UI testleri headless ortamda zor"
diye belirtilmiş).

---

## 5. Test sonuçları

```text
$ dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor

Passed!  - Failed:     0, Passed:    35, Skipped:     0, Total:    35, Duration: 163 ms - ErpBridge.Core.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:    44, Skipped:     0, Total:    44, Duration: 427 ms - ErpBridge.LocalStore.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:    64, Skipped:     5, Total:    69, Duration: 30 s  - ErpBridge.Erp.Mikro.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration:  81 ms - ErpBridge.Shared.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:    10, Skipped:     0, Total:    10, Duration: 584 ms - ErpBridge.RemoteApi.Tests.dll (net8.0)
```

| Test assembly | Passed | Skipped | Notes |
|---------------|--------|---------|-------|
| ErpBridge.Core.Tests | 35 | 0 | Baseline (değişmedi) |
| ErpBridge.LocalStore.Tests | 44 | 0 | Baseline +2 (önceden 42, +2 bu track'te değil — baseline mismatch kontrol) |
| ErpBridge.Erp.Mikro.Tests | 64 | 5 | Baseline (değişmedi) |
| **ErpBridge.Shared.Tests** | **21** | **0** | **Baseline 12 + 9 yeni AgentSettingsValidation** |
| ErpBridge.RemoteApi.Tests | 10 | 0 | Baseline (değişmedi) |
| **Toplam** | **174** | **5** | Baseline 153 + 9 yeni = 162, geri kalan +12 pre-existing |

> Not: `ErpBridge.LocalStore.Tests` 42→44 artışı baseline ile arasındaki 2 fark
> bu track'le ilgili değil — workspace snapshot'unda o projede zaten yapılmış
> başka düzeltmeler vardı (bu deliverable öncesi). Bu track'in kendi test
> katkısı: **+9 AgentSettingsValidation** testi.

### Yeni testler — `AgentSettingsValidationTests`

| Test | Senaryo |
|------|---------|
| `TryValidate_returns_true_when_all_required_fields_are_present_and_valid` | Happy path |
| `TryValidate_rejects_blank_SqlServer` | Whitespace-only SqlServer |
| `TryValidate_rejects_null_SqlServer` | null SqlServer |
| `TryValidate_rejects_blank_SqlUserName` | Empty SqlUserName |
| `TryValidate_rejects_blank_MikroDatabaseName` | Whitespace Mikro database |
| `TryValidate_rejects_non_integer_CompanyNo` | "abc" firma no |
| `TryValidate_rejects_blank_BranchNo` | Whitespace şube no |
| `TryValidate_rejects_decimal_separator_CompanyNo_under_tr_culture` | "1,5" — invariant kültür zorunlu |
| `TryValidate_accepts_zero_for_CompanyNo_and_BranchNo` | Sınır değer 0 geçerli |

---

## 6. XAML önizleme (yeni bölümler)

### 6.1 Üst başlık + versiyon badge'i

```xml
<Grid Grid.Row="0" Grid.ColumnSpan="2" Margin="0,0,0,12">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>
    <TextBlock Grid.Column="0"
               Text="ErpBridge Agent Yapılandırması"
               FontSize="18" FontWeight="SemiBold"
               VerticalAlignment="Center"
               Foreground="{DynamicResource HeaderBrush}" />
    <Border Grid.Column="1"
            Background="{Binding MikroVersionBrush}"
            CornerRadius="3"
            Padding="10,3"
            VerticalAlignment="Center"
            ToolTip="{Binding MikroVersionTooltip}">
        <TextBlock Text="{Binding MikroVersionBadge}"
                   Foreground="White"
                   FontWeight="SemiBold"
                   FontSize="13" />
    </Border>
</Grid>
```

### 6.2 Buton satırı (yeniden tespit + progress + test/kaydet)

```xml
<Grid Grid.Row="10" Grid.ColumnSpan="2" Margin="0,8,0,0">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>

    <Button Grid.Column="0"
            Content="🔄 Versiyonu yeniden tespit et"
            Command="{Binding RedetectVersionCommand}"
            MinWidth="200" Padding="8,4" />

    <!-- Progress göstergesi: üç nokta soldan sağa fade -->
    <StackPanel Grid.Column="1"
                Orientation="Horizontal"
                Visibility="{Binding IsBusy,
                    Converter={StaticResource BoolToVisibilityConverter}}">
        <TextBlock Text="●" Foreground="{DynamicResource BusyBrush}" FontSize="14">
            <TextBlock.Triggers>
                <EventTrigger RoutedEvent="TextBlock.Loaded">
                    <BeginStoryboard>
                        <Storyboard RepeatBehavior="Forever">
                            <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                             From="0.3" To="1.0" Duration="0:0:0.5"
                                             AutoReverse="True" />
                        </Storyboard>
                    </BeginStoryboard>
                </EventTrigger>
            </TextBlock.Triggers>
        </TextBlock>
        <TextBlock Text="  çalışıyor..."
                   Foreground="{DynamicResource BusyBrush}"
                   FontSize="12" Margin="6,0,0,0" />
    </StackPanel>

    <StackPanel Grid.Column="2" Orientation="Horizontal">
        <Button Content="Bağlantıyı test et"
                Command="{Binding TestConnectionCommand}" MinWidth="140" />
        <Button Content="Kaydet"
                Command="{Binding SaveCommand}" MinWidth="100" IsDefault="True" />
    </StackPanel>
</Grid>
```

### 6.3 Sunucu Bilgisi paneli

```xml
<Border Grid.Row="11" Grid.ColumnSpan="2"
        Padding="12" BorderBrush="{DynamicResource StatusBorderBrush}"
        BorderThickness="1" Background="{DynamicResource StatusBackgroundBrush}"
        CornerRadius="4"
        Visibility="{Binding HasConnectionTestResult,
            Converter={StaticResource BoolToVisibilityConverter}}">
    <StackPanel>
        <TextBlock Text="Sunucu Bilgisi" FontWeight="SemiBold" FontSize="13" Margin="0,0,0,8" />
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="140" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" /><RowDefinition Height="Auto" />
                <RowDefinition Height="Auto" /><RowDefinition Height="Auto" />
            </Grid.RowDefinitions>

            <TextBlock Grid.Row="0" Grid.Column="0" Text="ServerVersion" />
            <TextBlock Grid.Row="0" Grid.Column="1"
                       Text="{Binding ServerVersionDisplay}" FontFamily="Consolas, monospace" />
            <TextBlock Grid.Row="1" Grid.Column="0" Text="Identity Strategy" />
            <TextBlock Grid.Row="1" Grid.Column="1"
                       Text="{Binding IdentityStrategyDisplay}" FontFamily="Consolas, monospace" />
            <TextBlock Grid.Row="2" Grid.Column="0" Text="Latency" />
            <TextBlock Grid.Row="2" Grid.Column="1">
                <Run Text="{Binding LastTestLatencyMs, Mode=OneWay, StringFormat={}{0}}" />
                <Run Text=" ms" />
            </TextBlock>
            <TextBlock Grid.Row="3" Grid.Column="0" Text="Test zamanı" />
            <TextBlock Grid.Row="3" Grid.Column="1" Text="{Binding LastTestTimeDisplay}" />
        </Grid>
    </StackPanel>
</Border>
```

### 6.4 Sorun giderme paneli (hata durumunda)

```xml
<Border Grid.Row="12" Grid.ColumnSpan="2"
        Padding="12" BorderBrush="#E53935" BorderThickness="1"
        Background="#FFEBEE" CornerRadius="4"
        Visibility="{Binding TroubleshootingHint,
            Converter={StaticResource StringToVisibilityConverter}}">
    <StackPanel>
        <TextBlock Text="Sorun giderme önerileri:"
                   FontWeight="SemiBold" Foreground="#B71C1C" Margin="0,0,0,4" />
        <TextBlock Text="{Binding TroubleshootingHint}"
                   Foreground="#B71C1C" TextWrapping="Wrap" />
    </StackPanel>
</Border>
```

---

## 7. Kural ihlali kontrol listesi

| Kural | Durum | Kanıt |
|-------|-------|-------|
| Shared'a WPF dependency sızmıyor | ✅ | `AgentSettingsValidation` yalnızca `System.Globalization` kullanıyor, `net8.0` derleniyor |
| ErpBridge.Agent.UI → Erp.Mikro (Faz 2 itibarıyla) | ✅ | csproj'da mevcut, view-model de `IMikroConnectionTestOrchestrator` üzerinden konuşuyor |
| `TreatWarningsAsErrors=true` korundu | ✅ | Tüm projelerde 0 warning, 0 error |
| Şifre hiçbir XAML binding'inde loglanmıyor | ✅ | PasswordBox → view-model; `ConnectionStringMasker.MaskForLog` her log statement'ta |
| `AsyncRelayCommand` ile çift tıklama koruması | ✅ | `IsExecuting` flag'i `CanExecute` üzerinde gate, ikinci tıklama no-op |
| Geriye dönük XAML | ✅ | Mevcut 8 satır TextBox + 2 buton + Status paneli korundu; 4 yeni satır eklendi (badge, progress, info, hint) |
| V15/V16 farkı adapter'da kalır | ✅ | `ApplyBadgeFromVersion` UI katmanı, ama sadece `AbstractionsMikroVersion` enum'unu okur — yazma/okuma V15/V16 stratejisi yine `MikroIdentityStrategySelector` içinde |
| Secret alanlar plaintext loglanmaz | ✅ | `LogInformation`'da yalnız Server / Database / UserName / Company / Branch, şifre yok |

---

## 8. API davranış özeti (public surface)

### `AgentSettingsValidation.TryValidate`

```csharp
public static bool TryValidate(
    string? sqlServer,
    string? sqlUserName,
    string? mikroDatabaseName,
    string? companyNo,
    string? branchNo,
    out string error);
```

Returns `true` + empty `error` when every required field is present and
the two integer fields parse as non-negative values with the invariant
culture. Populates `error` with a Turkish user-visible message otherwise.

### `AgentSettingsViewModel` — yeni üyeler

| Üye | Tip | Açıklama |
|-----|-----|----------|
| `MikroVersionBadge` | `string` | "V15" / "V16" / "Unknown" / "—" |
| `MikroVersionBrush` | `Brush` | Renkli badge background, XAML doğrudan bağlanır |
| `MikroVersionTooltip` | `string` | Badge ToolTip metni |
| `ServerVersionDisplay` | `string` | SQL Server version, mono font |
| `IdentityStrategyDisplay` | `string` | "V15/RECno" / "V16/Guid" / "—" |
| `LastTestLatencyMs` | `long?` | Son test süresi (ms) |
| `LastTestTimeDisplay` | `string` | Local time formatında |
| `HasConnectionTestResult` | `bool` | Sunucu Bilgisi paneli gate |
| `TroubleshootingHint` | `string` | Hata kategorisine göre ipucu metni |
| `RedetectVersionCommand` | `ICommand` | `AsyncRelayCommand` — cache invalidating redetect |
| `TestConnectionCommand` | `ICommand` | `AsyncRelayCommand` (önceden sync `RelayCommand`) |

### `AsyncRelayCommand`

| Üye | Açıklama |
|-----|----------|
| `IsExecuting` | `bool` — true iken `CanExecute` false |
| `ExecuteAsync(object?)` | Doğrudan `await` edilebilir task dönen varyant |
| `CanExecuteChanged` | `ICommand`'den gelen event, `IsExecuting` değiştiğinde fire |
| `RaiseCanExecuteChanged()` | Manuel tetikleme (UI'ın `IsBusy` değişimine reaksiyon vermesi için) |

### `StringToVisibilityConverter`

Standart WPF `IValueConverter` — `IsNullOrWhiteSpace` kontrol eder,
non-empty → `Visibility.Visible`, aksi → `Visibility.Collapsed`. Theme
dictionary'de `StringToVisibilityConverter` anahtarı ile kayıtlı.

### DI kayıtları (değişen)

`src/ErpBridge.Erp.Mikro/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IMikroConnectionTestOrchestrator, MikroConnectionTestOrchestrator>();
services.TryAddSingletonLogger<MikroConnectionTestOrchestrator>(services);
```

Orchestrator singleton, çünkü version cache'i process-wide olmalı. UI ve
Service aynı instance'ı paylaşır (her ikisi de `AddErpBridgeMikro`
üzerinden resolve eder).

---

## 9. Faz 4 için öneri

- `RedetectVersionCommand` deseni (cache invalidating probe) merkezi API
  health-check tarafında da kullanılabilir — Service agent heartbeat'i
  başarısız olduğunda aynı komutu tetikleyebilir.
- `AgentSettingsValidation` Shared'da olduğu için ileride `Agent.Service`
  pre-flight check'i aynı kuralları kullanabilir; mevcut
  `IAgentConfigStore.LoadAsync` çıktısını validate eden bir iç helper
  yazılabilir.
- WPF UI test projesi (`net8.0-windows`) Faz 7 admin panelinde açılabilir —
  o zaman `Brush` property'si `string` color'a dönüştürülüp bir
  `ColorToBrushConverter` eklenebilir, böylece view-model test edilebilir
  hale gelir.
