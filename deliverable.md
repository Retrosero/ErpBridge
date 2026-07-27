# Canlı ERP senkronizasyonu

## Değişiklikler

- SQL Server Change Tracking tabanlı 2 saniyelik canlı Mikro tablo izleme eklendi.
- Yetkisiz kurulumlar için salt-okunur uyumluluk modu ve 6 saatlik güvenlik uzlaştırması eklendi.
- Etkilenen bootstrap bölümleri hash envanteriyle delta olarak gönderiliyor; fiziksel ve mantıksal silmeler kaldırılıyor.
- Windows servisi canlı izleme sahibi oldu; Dashboard zaman ayarını kaldırıp servis durumu, son algılama ve son aktarımı gösteriyor.
- Merkezi API tenant snapshot'ını yerinde güncelliyor; Android veri sözleşmesi değişmedi.
- UI ve LocalSystem servisi için eski DPAPI sırları güvenli biçimde makine kapsamına taşınıyor.
- Kurulum betiği tek masaüstü kısayoluna ek olarak otomatik başlayan Windows servisini kuruyor.

## Doğrulama

- `dotnet build ErpBridge.sln -c Release`: başarılı, 0 uyarı / 0 hata.
- Core testleri: 62/62 başarılı.
- Mikro testleri: 89 başarılı, 16 ortam bağımlı entegrasyon testi atlandı.
- LocalStore testleri: 48/48 başarılı.
- Agent.Service testleri: 8/8 başarılı.
- RemoteApi testleri: 11/11 başarılı.
- CentralApi bootstrap testleri: 6/6 başarılı.
- Yerel `MikroDB_V15_02` veritabanında 17 kaynak tablo için Change Tracking etkinleştirildi.
- `ErpBridgeAgent` Windows servisi Automatic/Running olarak kuruldu.
