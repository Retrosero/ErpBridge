# Deliverable — Wave 5A: Admin Refresh Token Flow

> **Faz:** Admin refresh token flow (Wave 5A)
> **Tarih:** 2026-09-08
> **Durum:** ✅ **TAMAMLANDI** (deliverable root tarafından yazıldı)
> **Build:** 0 hata / 0 uyarı
> **Test:** 7 yeni CentralApi test, mevcut 175 testi bozulmadı

## 1. Kapsam

Admin (Blazor) kullanıcıları için JWT access token + refresh token çifti.
Access token (1 saat) expire olduğunda refresh token ile yenilenebilir.
Refresh token tek kullanımlık: her `/refresh` çağrısında eski revoke edilir
ve yeni üretilir.

## 2. Mimari

```
┌─────────────────┐  POST /admin/auth/login          ┌──────────────────┐
│ Blazor Admin UI │ ────────────────────────────────▶│  CentralApi      │
│ TokenStore      │ ← { accessToken (1h),           │  AdminAuth       │
│ (in-memory)     │     refreshToken (30d),          │  + JwtTokenService│
│                 │     expiresAtUtc }                │  + RefreshToken  │
└─────────────────┘                                   │    rotation      │
        │                                              └──────────────────┘
        │ 401 (access token expire)
        ▼
   POST /admin/auth/refresh
   { "refreshToken": "..." }
        │
        ▼
   200 + yeni accessToken + yeni refreshToken (eski revoke)
        │
   401 (refresh token revoke veya expire)
        ▼
   → Blazor login ekranına yönlendir
```

## 3. Yeni / değişen dosyalar

### Yeni (4)
- `src/ErpBridge.CentralApi/Domain/RefreshToken.cs` — entity (TokenHash, ExpiresAtUtc, RevokedAtUtc, ReplacedByTokenId).
- `src/ErpBridge.CentralApi/Migrations/20260908054538_AddAdminRefreshTokens.cs` — EF Core migration (refresh_tokens tablosu + 2 unique index).
- `src/ErpBridge.CentralApi/Migrations/20260908054538_AddAdminRefreshTokens.Designer.cs` — model snapshot.
- `tests/ErpBridge.CentralApi.Tests/Endpoints/AdminRefreshTokenEndpointsTests.cs` — 7 yeni test.

### Değişen (2)
- `src/ErpBridge.CentralApi/Data/CentralApiDbContext.cs` — `DbSet<RefreshToken> RefreshTokens` + tablo konfigürasyonu.
- `src/ErpBridge.CentralApi/Endpoints/AdminAuthEndpoints.cs` — login response'una `refreshToken` + `refreshTokenExpiresAtUtc` eklendi, `/refresh` + `/logout` (revoke) yeni route'lar.

## 4. API sözleşmesi

```
POST /api/v1/admin/auth/login
  Body: { "email": "...", "password": "..." }
  → 200 {
      "accessToken": "eyJ...",
      "accessTokenExpiresAtUtc": "2026-09-08T11:00:00Z",
      "refreshToken": "rt_<base64url>",
      "refreshTokenExpiresAtUtc": "2026-10-08T10:00:00Z"
    }

POST /api/v1/admin/auth/refresh
  Body: { "refreshToken": "rt_<base64url>" }
  → 200 { ... yeni accessToken + yeni refreshToken, eski revoke }
  → 401 { "errorCode": "INVALID_REFRESH_TOKEN" } (revoked / expired / malformed)

POST /api/v1/admin/auth/logout
  Body: { "refreshToken": "rt_<base64url>" }
  → 204 (refresh token revoke edilir)
```

## 5. Güvenlik notları

- **Token hash format:** `SHA-256(rawToken)`. DB'de cleartext saklanmaz; rotation'da
  eski hash revoke edilir, yeni raw token döner.
- **Tek kullanımlık (rotation):** Her refresh sonrası eski `ReplacedByTokenId`
  set edilir ve `RevokedAtUtc` işaretlenir. Revoke edilmiş token ile ikinci
  kez refresh denenir → 401 (replay attack koruması).
- **Ömür:** Access token 1 saat, refresh token 30 gün.
- **IP loglama:** `CreatedByIp` kolonu; admin logout'ta güncellenir.

## 6. Test özeti

| Suite | Yeni | Toplam | Durum |
|-------|------|--------|-------|
| `AdminRefreshTokenEndpointsTests` | 7 | 7 | ✅ |
| **Yeni toplam** | **7** | | ✅ |
| CentralApi (regression) | 175 | 182 | ✅ |
| Tüm solution (regression) | 585 | 592 | ✅ |

Yeni testler:
- `Refresh_returns_new_access_and_refresh_tokens` (rotation)
- `Refresh_with_revoked_token_returns_401`
- `Refresh_with_expired_token_returns_401`
- `Refresh_with_invalid_token_returns_401`
- `Refresh_with_malformed_token_returns_401`
- `Login_returns_refresh_token_in_response`
- `Logout_revokes_refresh_token`

## 7. Build & test

```
$ dotnet build ErpBridge.sln -c Debug
  → 0 Uyarı, 0 Hata

$ dotnet test ErpBridge.sln --no-build
  → 592 PASSED, 0 FAILED, 20 SKIPPED
```

## 8. Bilinen sınırlar / sonraki adımlar

- **Blazor UI tarafı (Logout.razor, MainLayout.razor) hâlâ eski davranışta** —
  access token expire olunca login'e yönlendirir; refresh token ile
  otomatik yenileme UI'a eklenmeli. Bu Faz 16+ için.
- **Agent (scope=agent) refresh token YAPILMADI** — agent 24/7 çalışır, JWT
  ömrü 24 saat, otomatik login yapıyor. Sadece admin (UI) için refresh.
- **Migration production'a uygulanmalı** — `ErpBridge.CentralApi --migrate`.
- **Refresh token cleanup job** — expire olmuş token'lar 30 gün sonra DB'de
  şişer. Cleanup worker Faz 16+'da eklenecek.
