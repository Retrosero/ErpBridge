# 05 - Security and Audit Rules

## RBAC

Permissions role bazli tanimlanir.

- `SuperAdmin`: tum islemler.
- `Support`: agent durum, log summary, support bundle request, failed job goruntuleme.
- `Sales`: tenant ve lisans bilgisi goruntuleme, renewal talebi olusturma.
- `TenantAdmin`: sadece kendi tenant'ina ait sinirli durum ve cihaz bilgisi.
- `ReadOnly`: salt okunur sistem raporlari.

## 2FA

- SuperAdmin ve Support icin 2FA zorunludur.
- TenantAdmin icin configurable ama onerilen olarak aciktir.
- Recovery code hashlenmis saklanir.
- MFA secret encrypted saklanir.

## Audit Event

Her kritik islem `audit_events` kaydi uretir.

Zorunlu alanlar:

- actor
- role
- action
- target type/id
- tenant id
- old/new summary
- ip address
- user agent
- correlation id
- timestamp

## Secret Redaction

Redaction patternleri:

- API key
- Bearer token
- Refresh token
- SQL password
- Full connection string
- Machine fingerprint raw degeri
- Mobile session token

Redaction hem agent upload oncesi hem server ingest sirasinda calisir.

## Support Bundle

- Support bundle yalnizca aktif ve yetkili admin talebiyle yuklenir.
- Talebin suresi vardir.
- Bundle dosyasi storage'da saklanir, DB'de yalnizca metadata ve URI tutulur.
- Download islemi auditlenir.
- Bundle icindeki secret scan basarisizsa dosya quarantine durumuna alinir.

## Data Retention

- Audit events uzun sureli saklanir.
- Heartbeat raw history kisa sureli saklanabilir, status snapshot daha uzun saklanir.
- Support bundle dosyalari retention policy sonunda silinir.
- Tenant soft-delete sonrasi hard-delete ayrica onay ve audit ister.

## Admin Session Security

- Session timeout uygulanir.
- Refresh token rotation kullanilir.
- Suspicious login veya role escalation olaylari auditlenir.
- Failed login rate limit uygulanir.
