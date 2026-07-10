﻿# ErpBridge'i Coolify'a Deploy Etme

Bu rehber **central API** + **admin panel**'in Coolify yÃ¶netimli bir
sunucuya nasÄ±l deploy edileceÄŸini anlatÄ±r. Coolify v4.x ve Ä°nternet'ten
eriÅŸilebilen bir Linux hedef sunucu varsayÄ±yoruz.

> **KÄ±sa Ã¶zet.** AynÄ± Git reposuna bakan iki Coolify "Application", bir
> PostgreSQL veritabanÄ±, dÃ¶rt ortam deÄŸiÅŸkeni secret'Ä±, Let's Encrypt
> sertifikalÄ± iki public domain. Toplam ~10 dakikalÄ±k tÄ±klama.

---

## 1. Ã–nkoÅŸullar

- Coolify kurulumu Ã§alÄ±ÅŸÄ±r ve eriÅŸilebilir durumda (Ã¶rn. `https://coolify.senin-hostun.com`).
- Coolify "Project" (veya varsayÄ±lan) ve bir "Environment" (Production yeterli).
- Her servis iÃ§in bir domain veya subdomain:
  - `api.erpbridge.example.com` â†’ central API
  - `admin.erpbridge.example.com` â†’ admin panel
- Her iki subdomain iÃ§in Coolify sunucusunun public IP'sini gÃ¶steren
  DNS A/AAAA kaydÄ±. Coolify Traefik (reverse-proxy) ve Let's Encrypt
  (sertifika) kullanÄ±r; ikisi de Ã§Ã¶zÃ¼mlenebilir DNS gerektirir.

## 2. PostgreSQL veritabanÄ± saÄŸlama

Coolify "Database" servis tipi uygulamalarÄ±n yanÄ±nda yÃ¶netilen bir
PostgreSQL container'Ä± saÄŸlar. Coolify UI'da:

1. **+ New â†’ Database â†’ PostgreSQL 16**.
2. AdÄ±nÄ± `erpbridge-pg` olarak ayarla.
3. VeritabanÄ± adÄ± (`erpbridge`), kullanÄ±cÄ± adÄ± (`erpbridge`) seÃ§.
4. **Generate a password** (Coolify saklayacak; kopyala â€” central API'nin
   ortam deÄŸiÅŸkenlerine `POSTGRES_PASSWORD` olarak yapÄ±ÅŸtÄ±racaksÄ±n).
5. Deploy et. Ä°Ã§ servis hostname'ini not al (Coolify veritabanÄ±nÄ±n kaynak
   detay sayfasÄ±nda gÃ¶sterir, yaklaÅŸÄ±k `erpbridge-pg-xxxx` gibi).

Ä°Ã§ hostname, central API'nin connection string'inde kullanÄ±lacak ÅŸeydir;
Ã§Ã¼nkÃ¼ Docker'Ä±n iÃ§ DNS'i onu Coolify yÃ¶netimli aÄŸ Ã¼zerinde Ã§Ã¶zÃ¼mler.

## 3. Secret'larÄ± Ã¼ret

Central API'yi baÅŸlatmak iÃ§in Ã¼Ã§ secret lazÄ±m. BunlarÄ± root ÅŸifresi gibi
gÃ¶rmek lazÄ±m â€” asla commit etme, asla aÃ§Ä±k metin log'a dÃ¼ÅŸÃ¼rme, asla
ÅŸifresiz e-posta atma.

| DeÄŸiÅŸken | Ne koymalÄ± | Nerede saklanmalÄ± |
|---|---|---|
| `JWT_SIGNING_KEY` | 64+ ASCII karakter rastgelelik. `openssl rand -hex 48` hÄ±zlÄ± bir kaynak. | Coolify secret |
| `ADMIN_SEED_PASSWORD` | Bootstrap admin'in ilk ÅŸifresi. Admin ilk login sonrasÄ± deÄŸiÅŸtirecek (sonraki geliÅŸtirme). | Coolify secret |
| `POSTGRES_PASSWORD` | AdÄ±m 2'de belirlediÄŸin ÅŸifre. | Coolify secret |

AyrÄ±ca bir **secret olmayan** ortam deÄŸiÅŸkeni daha lazÄ±m:

| DeÄŸiÅŸken | DeÄŸer |
|---|---|
| `ADMIN_SEED_EMAIL` | Admin'in e-posta adresi, Ã¶rn. `ops@erpbridge.example.com` |
| `CENTRALAPI_BASE_URL` | Admin panelin central API'nin nerede yaÅŸadÄ±ÄŸÄ±nÄ± bilmesi lazÄ±m. **Public** URL kullan: `https://api.erpbridge.example.com` |
| `GIT_HASH` (opsiyonel) | KÄ±sa bir etiket; image tag'inde gÃ¶rÃ¼nÃ¼r. |

## 4. Central API'yi deploy et

Coolify UI'da:

1. **+ New â†’ Application â†’ Docker Compose**.
2. **Git Repository** â€” kendi ErpBridge fork'unu gÃ¶ster. Branch: `main`
   (veya release branch'in).
3. **Docker Compose Location** â€” `docker-compose.coolify.yml` olarak bÄ±rak.
4. **Base Directory** â€” boÅŸ bÄ±rak (dosya repo kÃ¶kÃ¼nde).
5. **Build Pack** â€” "Dockerfile" / "Automatic" tespitini aÃ§Ä±k bÄ±rak.
6. Coolify compose dosyasÄ±nÄ± parse edip iki servisi listeleyecek:
   `centralapi` ve `admin`. Åimdilik `centralapi`'ya tÄ±kla.
7. `centralapi` servis detay sayfasÄ±nda:
   - **General → Port Exposes**: `4001`.
   - **General → FQDN**: `https://api.erpbridge.example.com`.
     Coolify Let's Encrypt sertifikasÄ±nÄ± otomatik alacak.
   - **Environment Variables**:
     - `POSTGRES_PASSWORD` â†’ *(Secret)* â€” adÄ±m 2'deki ÅŸifreyi yapÄ±ÅŸtÄ±r.
     - `JWT_SIGNING_KEY` â†’ *(Secret)* â€” adÄ±m 3'teki deÄŸeri yapÄ±ÅŸtÄ±r.
     - `ADMIN_SEED_EMAIL` â†’ `ops@erpbridge.example.com`.
     - `ADMIN_SEED_PASSWORD` â†’ *(Secret)* â€” adÄ±m 3'teki deÄŸeri yapÄ±ÅŸtÄ±r.
   - **Healthchecks** â€” Coolify'in `/health` iÃ§in default HTTP check'ini aÃ§Ä±k bÄ±rak.
8. **Deploy**'a tÄ±kla. Build loglarÄ±nÄ± izle:
   - `dotnet restore` + `dotnet publish` hatasÄ±z bitmeli.
   - `webhook_deliveries` / `api_keys` tablolarÄ± ilk aÃ§Ä±lÄ±ÅŸta EF Core'un
     `Database.Migrate()`'Ä± ile otomatik oluÅŸur.
   - `EnsureSeedAdmin` her aÃ§Ä±lÄ±ÅŸta Ã§alÄ±ÅŸÄ±r ama idempotent; o email ile
     admin yoksa satÄ±r oluÅŸur.
9. Laptop'tan duman testi:
   ```
   curl https://api.erpbridge.example.com/health
   ```
   `{"status":"ok"}` dÃ¶nmeli.

## 5. Admin panelini deploy et

Coolify UI'da:

1. AynÄ± compose dosyasÄ± zaten `admin` servisini tanÄ±mlÄ±yor. Ä°Ã§ine tÄ±kla.
2. **General → Port Exposes**: `4002` (admin paneli; centralapi 4001).
3. **General → FQDN**: `https://admin.erpbridge.example.com`.
4. **Environment Variables**:
   - `CENTRALAPI_BASE_URL` → `https://api.erpbridge.example.com`
     *(iÃ§ Docker hostname'i deÄŸil, **public** FQDN olmalÄ±)*.
5. **Deploy**'a tÄ±kla. Build'in bitmesini bekle.
6. `https://admin.erpbridge.example.com` adresini aÃ§. Blazor login
   sayfasÄ±yla karÅŸÄ±laÅŸmalÄ±sÄ±n.

## 6. Ä°lk kez login

1. `ADMIN_SEED_EMAIL` ve `ADMIN_SEED_PASSWORD` ile giriÅŸ yap (adÄ±m 3).
2. **Tenants â†’ New Tenant**. Ä°lk mÃ¼ÅŸterini oluÅŸtur.
3. **Licenses â†’ Issue license**. Ãœretilen key'i Windows Agent'Ä±n
   konfigÃ¼rasyonuna yapÄ±ÅŸtÄ±r.
4. Agent kayÄ±t olduktan sonra **Agents** altÄ±nda gÃ¶receksin.
5. MÃ¼ÅŸteriden API Ã¼zerinden iÅŸ almaya baÅŸlamak iÃ§in:
   - **API Keys â†’ Create key** ile tenant iÃ§in bir key Ã¼ret.
   - Raw `AK-...` deÄŸerini mÃ¼ÅŸterinin backend'ine ver; onlar da
     `POST https://api.erpbridge.example.com/api/v1/ingest/jobs` Ã§aÄŸrÄ±sÄ±nÄ±
     `Authorization: Bearer AK-...` ve `X-Tenant-Id: <guid>` ile yapar.
6. MÃ¼ÅŸterinin ERP'sinden event callback almak iÃ§in:
   - **Webhooks â†’ Register endpoint**.
   - `whsec_...` secret'Ä±nÄ± mÃ¼ÅŸterinin receiver'Ä±na ver ki
     `ErpBridge-Signature` header'Ä±nÄ± HMAC-SHA256 ile doÄŸrulayabilsin
     (`"<timestamp>.<body>"` Ã¼zerinden).

## 7. Yedekleme

Coolify'in PostgreSQL kaynaÄŸÄ±nda S3-uyumlu depolamaya tek tÄ±kla yedek
bulunur. VeritabanÄ±nÄ±n "Backups" sekmesinden yapÄ±landÄ±r. GÃ¼nlÃ¼k snapshot
zamanlayabilirsin; ÅŸema kÃ¼Ã§Ã¼k (PII yok, sadece `api_keys` hash kolonlarÄ±
ve webhook secret'larÄ± â€” raw deÄŸerler mÃ¼ÅŸteride, snapshot'tan kurtarÄ±lamaz).

## 8. GÃ¼ncelleme

YapÄ±landÄ±rÄ±lmÄ±ÅŸ Git branch'ine commit at. Coolify'in "Auto Deploy"
webhook'u (veya manuel **Deploy** butonu) etkilenen image'larÄ± yeniden
build edip servisleri yeniden baÅŸlatÄ±r. Migration'lar central API'nin
yeni container'Ä± ilk aÃ§Ä±lÄ±ÅŸÄ±nda otomatik Ã§alÄ±ÅŸÄ±r.

**Schema-breaking deÄŸiÅŸikliklerde** sÄ±ra Ã¶nemli: Ã¶nce central API'yi
deploy et, saÄŸlÄ±k kontrolÃ¼ `ok` dÃ¶ndÃ¼ÄŸÃ¼nÃ¼ gÃ¶r, sonra admin panelini
deploy et. Admin panel stateless bir Blazor Server uygulamasÄ±; schema
 sahibi deÄŸil.

## 9. Sorun Giderme

| Belirti | OlasÄ± neden | Ã‡Ã¶zÃ¼m |
|---|---|---|
| Central API sÃ¼rekli yeniden baÅŸlÄ±yor; loglarda `Jwt:SigningKey must be at least 32 bytes long` | `JWT_SIGNING_KEY` Ã§ok kÄ±sa. | `openssl rand -hex 48` ile yeniden Ã¼ret. |
| `dotnet restore` Mikro V15/V16 NuGet'lerinde hata veriyor | Coolify'in build runner'Ä± iÃ§ NuGet feed'ine eriÅŸemiyor olabilir. | Paketleri mirror'la ya da `NuGet.config`'i eriÅŸilebilir public mirror'a Ã§evir. |
| Admin panelde her iÅŸlem "Network error" | `CENTRALAPI_BASE_URL` admin container'Ä±nÄ±n iÃ§inden iÃ§ Docker hostname'ine ayarlÄ± â€” yanlÄ±ÅŸ. | **Public** URL `https://api.erpbridge.example.com` kullan. |
| Her yerde `401 Unauthorized` | Coolify sunucusu ile laptop arasÄ±nda saat farkÄ±. JWT doÄŸrulamasÄ± Â±30s oynamayÄ± tolere eder; daha bÃ¼yÃ¼k fark tÃ¼m Ã§aÄŸrÄ±larÄ± 401 yapar. | `chrony` veya `systemd-timesyncd` ile saatleri senkronize et. |
| `Database.Migrate()` "relation already exists" hatasÄ± | Ã–nceki deploy ortasÄ±nda kesildi. | VeritabanÄ±na baÄŸlan, `__EFMigrationsHistory`'deki yarÄ±m migration satÄ±rÄ±nÄ± sil, yeniden deploy et. |

---

## Ek: ortam deÄŸiÅŸkeni referansÄ±

### Central API

| DeÄŸiÅŸken | Zorunlu mu | Ã–rnek | Notlar |
|---|---|---|---|
| `ConnectionStrings__CentralApi` | evet | `Host=erpbridge-pg;Port=5432;Database=erpbridge;Username=erpbridge;Password=â€¦` | Coolify'in **iÃ§** hostname'ini kullan. |
| `Jwt__SigningKey` | evet | (64 hex karakter) | â‰¥32 byte. HS256 iÃ§in zorunlu. |
| `Jwt__Issuer` | hayÄ±r | `ErpBridge.CentralApi` | |
| `Jwt__Audience` | hayÄ±r | `ErpBridge.Agents` | |
| `Jwt__AccessTokenMinutes` | hayÄ±r | `60` | Agent JWT Ã¶mrÃ¼. |
| `Admin__SeedEmail` | evet | `ops@erpbridge.example.com` | BoÅŸ bÄ±rakmak seed'i devre dÄ±ÅŸÄ± bÄ±rakÄ±r. |
| `Admin__SeedPassword` | evet | (bootstrap ÅŸifre) | BoÅŸ bÄ±rakmak seed'i devre dÄ±ÅŸÄ± bÄ±rakÄ±r. |
| `Admin__SeedDisplayName` | hayÄ±r | `Bootstrap Admin` | |
| `PORT` | hayır | `4001` | Container tarafı port. Sadece host'ta 4001 çakışıyorsa override et; Dockerfile defaultuyla eşleşir. |

### Admin panel

| DeÄŸiÅŸken | Zorunlu mu | Ã–rnek | Notlar |
|---|---|---|---|
| `CentralApi__BaseUrl` | evet | `https://api.erpbridge.example.com` | Public URL; iÃ§ Docker hostname'ini kullanma. |
| `ASPNETCORE_URLS` | hayÄ±r | `http://+:8080` | |


