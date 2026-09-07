# SpAdmin

SpAdmin, Sync Adapter urununun merkezi control plane ve superadmin panelidir. Firmalari, tenantlari, API key tarihlerini, modulleri, agent cihazlarini, heartbeat durumlarini, ozet loglari ve destek paketlerini yonetir.

## Ana Kararlar

- Stack: .NET 8 + SQL Server.
- Tenant izolasyonu: tek merkezi veritabani ve tum operasyonel tablolarda `tenant_id`.
- Kimlik: RBAC + 2FA.
- Roller: `SuperAdmin`, `Support`, `Sales`, `TenantAdmin`, `ReadOnly`.
- Lisans: sureli API key aktivasyonu, gunluk calismada access/refresh token.
- Modul yonetimi: subscription plan + feature flag override.
- Log modeli: surekli ozet log/health; detayli support bundle yalnizca talep uzerine.

## Sync Adapter ile Baglanti

Sync Adapter agent, musteri makinesinden SpAdmin/Central API'ye HTTPS ile baglanir. Agent aktivasyon, heartbeat, job pull ve ack islemlerini bu control plane uzerinden yapar. Musteri portu acilmaz; SQL bilgileri merkezi sunucuya tasinmaz.

## Ana Bilesenler

- Admin Web UI: tenant, lisans, modul, agent ve log yonetimi.
- Admin API: superadmin ve destek operasyonlari.
- Agent API: aktivasyon, heartbeat, job pull, ack ve support bundle upload.
- Mobile API: Android uygulama icin belge gonderme ve bootstrap veri alma.
- SQL Server DB: tenant, lisans, modul, queue, audit ve log metadata.
- Secure file storage: support bundle dosyalari icin DB disi storage.

Detaylar icin `docs/`, gelistirme kurallari icin `AGENTS.md`, adim adim promptlar icin `prompts/` klasorune bak.
