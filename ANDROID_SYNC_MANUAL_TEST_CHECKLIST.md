# Android Sync Manual Test Checklist

- [ ] Test tenantı ve anonimleştirilmiş büyük ürün/cari/hareket verisi hazırlayın.
- [ ] Temiz kurulumda lisans/cihaz token akışını tamamlayın; token ve lisans anahtarının loglarda görünmediğini doğrulayın.
- [ ] Büyük ilk cari ve ürün sync işlemlerinde ilerlemeyi, UI yanıtını ve her sayfanın kaydını gözlemleyin.
- [ ] Aynı sync'i tekrar çalıştırın; duplicate kayıt oluşmadığını doğrulayın.
- [ ] Sync ortasında ağı kesin, uygulamayı kapatın ve yeniden açın; veri bütünlüğünü ve retry/checkpoint davranışını kontrol edin.
- [ ] HTTP timeout, 401/403, bozuk kayıt ve Room hata senaryolarını test tenantında üretin.
- [ ] Offline iken hata oluşturun; ağ döndüğünde telemetry kuyruğunun yalnızca bir kez gönderildiğini doğrulayın.
- [ ] Crash sonrası yeniden açılışta crash dosyasının kuyruğa taşındığını doğrulayın.
- [ ] Admin panelinde hata listesi, detay, fingerprint, stack trace maskesi, tenant filtresi ve durum değişikliğini test edin.
- [ ] Tenant adminin başka tenant verisini göremediğini; super adminin yetkili veriyi görebildiğini doğrulayın.
