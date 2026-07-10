# ErpBridge'i Coolify'a Deploy Etme

Bu rehber **central API** + **admin panel**'in Coolify yönetimli bir
sunucuya nasıl deploy edileceğini anlatır. Coolify v4.x ve İnternet'ten
erişilebilen bir Linux hedef sunucu varsayıyoruz.

> **Kısa özet.** Aynı Git reposuna bakan iki Coolify "Application", bir
> PostgreSQL veritabanı, dört ortam değişkeni secret'ı, Let's Encrypt
> sertifikalı iki public domain. Toplam ~10 dakikalık tıklama.

---

## 1. Önkoşullar

- Coolify kurulumu çalışır ve erişilebilir durumda (örn. `https://coolify.senin-hostun.com`).
- Coolify "Project" (veya varsayılan) ve bir "Environment" (Production yeterli).
- Her servis için bir domain veya subdomain:
  - `api.erpbridge.example.com` → central API
  - `admin.erpbridge.example.com` → admin panel
- Her iki subdomain için Coolify sunucusunun public IP'sini gösteren
  DNS A/AAAA kaydı. Coolify Traefik (reverse-proxy) ve Let's Encrypt
  (sertifika) kullanır; ikisi de çözümlenebilir DNS gerektirir.

## 2. PostgreSQL veritabanı sağlama

Coolify "Database" servis tipi uygulamaların yanında yönetilen bir
PostgreSQL container'ı sağlar. Coolify UI'da:

1. **+ New → Database → PostgreSQL 16**.
2. Adını `erpbridge-pg` olarak ayarla.
3. Veritabanı adı (`erpbridge`), kullanıcı adı (`erpbridge`) seç.
4. **Generate a password** (Coolify saklayacak; kopyala — central API'nin
   ortam değişkenlerine `POSTGRES_PASSWORD` olarak yapıştıracaksın).
5. Deploy et. İç servis hostname'ini not al (Coolify veritabanının kaynak
   detay sayfasında gösterir, yaklaşık `erpbridge-pg-xxxx` gibi).

İç hostname, central API'nin connection string'inde kullanılacak şeydir;
çünkü Docker'ın iç DNS'i onu Coolify yönetimli ağ üzerinde çözümler.

## 3. Secret'ları üret

Central API'yi başlatmak için üç secret lazım. Bunları root şifresi gibi
görmek lazım — asla commit etme, asla açık metin log'a düşürme, asla
şifresiz e-posta atma.

| Değişken | Ne koymalı | Nerede saklanmalı |
|---|---|---|
| `JWT_SIGNING_KEY` | 64+ ASCII karakter rastgelelik. `openssl rand -hex 48` hızlı bir kaynak. | Coolify secret |
| `ADMIN_SEED_PASSWORD` | Bootstrap admin'in ilk şifresi. Admin ilk login sonrası değiştirecek (sonraki geliştirme). | Coolify secret |
| `POSTGRES_PASSWORD` | Adım 2'de belirlediğin şifre. | Coolify secret |

Ayrıca bir **secret olmayan** ortam değişkeni daha lazım:

| Değişken | Değer |
|---|---|
| `ADMIN_SEED_EMAIL` | Admin'in e-posta adresi, örn. `ops@erpbridge.example.com` |
| `CENTRALAPI_BASE_URL` | Admin panelin central API'nin nerede yaşadığını bilmesi lazım. **Public** URL kullan: `https://api.erpbridge.example.com` |
| `GIT_HASH` (opsiyonel) | Kısa bir etiket; image tag'inde görünür. |

## 4. Central API'yi deploy et

Coolify UI'da:

1. **+ New → Application → Docker Compose**.
2. **Git Repository** — kendi ErpBridge fork'unu göster. Branch: `main`
   (veya release branch'in).
3. **Docker Compose Location** — `docker-compose.coolify.yml` olarak bırak.
4. **Base Directory** — boş bırak (dosya repo kökünde).
5. **Build Pack** — "Dockerfile" / "Automatic" tespitini açık bırak.
6. Coolify compose dosyasını parse edip iki servisi listeleyecek:
   `centralapi` ve `admin`. Şimdilik `centralapi`'ya tıkla.
7. `centralapi` servis detay sayfasında:
   - **General → Port Exposes**: `8080`.
   - **General → FQDN**: `https://api.erpbridge.example.com`.
     Coolify Let's Encrypt sertifikasını otomatik alacak.
   - **Environment Variables**:
     - `POSTGRES_PASSWORD` → *(Secret)* — adım 2'deki şifreyi yapıştır.
     - `JWT_SIGNING_KEY` → *(Secret)* — adım 3'teki değeri yapıştır.
     - `ADMIN_SEED_EMAIL` → `ops@erpbridge.example.com`.
     - `ADMIN_SEED_PASSWORD` → *(Secret)* — adım 3'teki değeri yapıştır.
   - **Healthchecks** — Coolify'in `/health` için default HTTP check'ini açık bırak.
8. **Deploy**'a tıkla. Build loglarını izle:
   - `dotnet restore` + `dotnet publish` hatasız bitmeli.
   - `webhook_deliveries` / `api_keys` tabloları ilk açılışta EF Core'un
     `Database.Migrate()`'ı ile otomatik oluşur.
   - `EnsureSeedAdmin` her açılışta çalışır ama idempotent; o email ile
     admin yoksa satır oluşur.
9. Laptop'tan duman testi:
   ```
   curl https://api.erpbridge.example.com/health
   ```
   `{"status":"ok"}` dönmeli.

## 5. Admin panelini deploy et

Coolify UI'da:

1. Aynı compose dosyası zaten `admin` servisini tanımlıyor. İçine tıkla.
2. **General → Port Exposes**: `8080`.
3. **General → FQDN**: `https://admin.erpbridge.example.com`.
4. **Environment Variables**:
   - `CENTRALAPI_BASE_URL` → `https://api.erpbridge.example.com`
     *(iç Docker hostname'i değil, **public** FQDN olmalı)*.
5. **Deploy**'a tıkla. Build'in bitmesini bekle.
6. `https://admin.erpbridge.example.com` adresini aç. Blazor login
   sayfasıyla karşılaşmalısın.

## 6. İlk kez login

1. `ADMIN_SEED_EMAIL` ve `ADMIN_SEED_PASSWORD` ile giriş yap (adım 3).
2. **Tenants → New Tenant**. İlk müşterini oluştur.
3. **Licenses → Issue license**. Üretilen key'i Windows Agent'ın
   konfigürasyonuna yapıştır.
4. Agent kayıt olduktan sonra **Agents** altında göreceksin.
5. Müşteriden API üzerinden iş almaya başlamak için:
   - **API Keys → Create key** ile tenant için bir key üret.
   - Raw `AK-...` değerini müşterinin backend'ine ver; onlar da
     `POST https://api.erpbridge.example.com/api/v1/ingest/jobs` çağrısını
     `Authorization: Bearer AK-...` ve `X-Tenant-Id: <guid>` ile yapar.
6. Müşterinin ERP'sinden event callback almak için:
   - **Webhooks → Register endpoint**.
   - `whsec_...` secret'ını müşterinin receiver'ına ver ki
     `ErpBridge-Signature` header'ını HMAC-SHA256 ile doğrulayabilsin
     (`"<timestamp>.<body>"` üzerinden).

## 7. Yedekleme

Coolify'in PostgreSQL kaynağında S3-uyumlu depolamaya tek tıkla yedek
bulunur. Veritabanının "Backups" sekmesinden yapılandır. Günlük snapshot
zamanlayabilirsin; şema küçük (PII yok, sadece `api_keys` hash kolonları
ve webhook secret'ları — raw değerler müşteride, snapshot'tan kurtarılamaz).

## 8. Güncelleme

Yapılandırılmış Git branch'ine commit at. Coolify'in "Auto Deploy"
webhook'u (veya manuel **Deploy** butonu) etkilenen image'ları yeniden
build edip servisleri yeniden başlatır. Migration'lar central API'nin
yeni container'ı ilk açılışında otomatik çalışır.

**Schema-breaking değişikliklerde** sıra önemli: önce central API'yi
deploy et, sağlık kontrolü `ok` döndüğünü gör, sonra admin panelini
deploy et. Admin panel stateless bir Blazor Server uygulaması; schema
 sahibi değil.

## 9. Sorun Giderme

| Belirti | Olası neden | Çözüm |
|---|---|---|
| Central API sürekli yeniden başlıyor; loglarda `Jwt:SigningKey must be at least 32 bytes long` | `JWT_SIGNING_KEY` çok kısa. | `openssl rand -hex 48` ile yeniden üret. |
| `dotnet restore` Mikro V15/V16 NuGet'lerinde hata veriyor | Coolify'in build runner'ı iç NuGet feed'ine erişemiyor olabilir. | Paketleri mirror'la ya da `NuGet.config`'i erişilebilir public mirror'a çevir. |
| Admin panelde her işlem "Network error" | `CENTRALAPI_BASE_URL` admin container'ının içinden iç Docker hostname'ine ayarlı — yanlış. | **Public** URL `https://api.erpbridge.example.com` kullan. |
| Her yerde `401 Unauthorized` | Coolify sunucusu ile laptop arasında saat farkı. JWT doğrulaması ±30s oynamayı tolere eder; daha büyük fark tüm çağrıları 401 yapar. | `chrony` veya `systemd-timesyncd` ile saatleri senkronize et. |
| `Database.Migrate()` "relation already exists" hatası | Önceki deploy ortasında kesildi. | Veritabanına bağlan, `__EFMigrationsHistory`'deki yarım migration satırını sil, yeniden deploy et. |

---

## Ek: ortam değişkeni referansı

### Central API

| Değişken | Zorunlu mu | Örnek | Notlar |
|---|---|---|---|
| `ConnectionStrings__CentralApi` | evet | `Host=erpbridge-pg;Port=5432;Database=erpbridge;Username=erpbridge;Password=…` | Coolify'in **iç** hostname'ini kullan. |
| `Jwt__SigningKey` | evet | (64 hex karakter) | ≥32 byte. HS256 için zorunlu. |
| `Jwt__Issuer` | hayır | `ErpBridge.CentralApi` | |
| `Jwt__Audience` | hayır | `ErpBridge.Agents` | |
| `Jwt__AccessTokenMinutes` | hayır | `60` | Agent JWT ömrü. |
| `Admin__SeedEmail` | evet | `ops@erpbridge.example.com` | Boş bırakmak seed'i devre dışı bırakır. |
| `Admin__SeedPassword` | evet | (bootstrap şifre) | Boş bırakmak seed'i devre dışı bırakır. |
| `Admin__SeedDisplayName` | hayır | `Bootstrap Admin` | |
| `ASPNETCORE_URLS` | hayır | `http://+:8080` | Dockerfile ayarlar; sadece debug için override et. |

### Admin panel

| Değişken | Zorunlu mu | Örnek | Notlar |
|---|---|---|---|
| `CentralApi__BaseUrl` | evet | `https://api.erpbridge.example.com` | Public URL; iç Docker hostname'ini kullanma. |
| `ASPNETCORE_URLS` | hayır | `http://+:8080` | |