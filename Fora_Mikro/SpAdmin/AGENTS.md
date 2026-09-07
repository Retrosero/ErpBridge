# SpAdmin Agent Rules

Bu dosya SpAdmin control plane ve superadmin paneli gelistirilirken uyulacak baglayici kurallari tanimlar.

## Tenant Kurallari

- Operasyonel her tabloda `tenant_id` bulunur veya tablo global katalog olarak acikca isaretlenir.
- Tenant verisi sorgularinda `tenant_id` filtresi zorunludur.
- Tenant silme varsayilan olarak soft-delete yapar.
- Hard-delete yalnizca ayrica tanimli retention policy ve audit kaydi ile yapilir.
- Bir tenant'in admini baska tenant verisini gormez.

## Lisans ve API Key Kurallari

- API key DB'de plain text tutulmaz; sadece guclu hash ve key prefix saklanir.
- API key yalnizca aktivasyon veya yenileme icin kullanilir.
- Agent gunluk calismada access token ve refresh token kullanir.
- Refresh token DB'de hashlenmis tutulur.
- Expired, disabled veya revoked lisans sync baslatamaz.
- Lisans tarihi ve modul override degisikligi yalnizca `SuperAdmin` tarafindan yapilir.

## Modul Kurallari

- Tum moduller `module_catalog` icinde tanimlanir.
- Plan modulleri `plan_modules` ile belirlenir.
- Tenant bazli istisnalar `tenant_module_overrides` ile yapilir.
- Heartbeat cevabi etkin modul listesini dondurur.
- Kapali modul icin Android job kabul edilmez ve agent job cekmez.

## Auth ve Audit Kurallari

- Admin panelinde RBAC ve 2FA zorunludur.
- Her admin islemi audit event uretir.
- Audit event; actor, role, tenant, action, target, old/new summary, ip, user agent ve correlation id icermelidir.
- Audit kayitlari sonradan sessizce degistirilemez.
- Support bundle talebi, lisans degisikligi, modul degisikligi, tenant disable ve API key revoke mutlaka auditlenir.

## Log ve Secret Kurallari

- API key, token, SQL password, full connection string ve musteri gizli alanlari loglanmaz.
- Support bundle upload edilmeden once agent tarafinda redaction uygulanir; server tarafinda ikinci redaction kontrolu yapilir.
- Detayli loglar surekli toplanmaz; yalnizca support bundle talebiyle gelir.
- Support bundle dosya icerigi SQL Server'da tutulmaz; guvenli dosya/object storage referansi tutulur.

## API Kurallari

- Tum endpointler HTTPS varsayar.
- Admin endpointleri role/policy bazli yetkilendirilir.
- Agent endpointleri tenant, device ve token binding kontrolu yapar.
- Mobile endpointleri tenant lisansi ve modul flag kontrolu yapmadan belge kabul etmez.
- Error response correlation id icermelidir.
