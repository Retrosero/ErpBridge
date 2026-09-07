# Bootstrap timeout düzeltmesi

## Değişen dosyalar

- `src/ErpBridge.Erp.Mikro/Adapters/MikroAdapter.cs`
  - Tam ve incremental bootstrap bölümleri bağımsız görevler olarak eşzamanlı okunur.
  - Parent/child kayıt bağlama işlemi tüm okumalar tamamlandıktan sonra korunur.
- `src/ErpBridge.Erp.Mikro/Readers/MikroDbReader.cs`
  - Incremental müşteri okumasında cari bakiye ledger taraması değişen carilerle sınırlandırıldı.
- `src/ErpBridge.RemoteApi/DependencyInjection/ServiceCollectionExtensions.cs`
  - Bootstrap endpoint'i için çift retry katmanı kaldırıldı; bootstrap retry/fallback akışı Core'da tek sahibi tarafından yürütülür.

## Davranış

Bootstrap artık 14 SQL okumasını seri beklemez. Bir sorgu yavaşladığında diğer bölümler gereksiz yere arkasında beklemez; ayrıca bir bootstrap isteği HttpClient Polly katmanında ikinci kez retry edilmez.

## Doğrulama

- `git diff --check`: temiz.
- `dotnet build ErpBridge.sln --no-restore`: çalıştırılamadı; makinede istenen .NET SDK `10.0.302` yok, kurulu SDK `8.0.424`.
- `dotnet test tests/ErpBridge.Erp.Mikro.Tests/ErpBridge.Erp.Mikro.Tests.csproj --no-restore`: aynı SDK çözümleme engeline takıldı.
