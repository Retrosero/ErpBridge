# 03 - License and Module Rules

## API Key Lifecycle

1. SuperAdmin tenant icin API key olusturur.
2. Key sadece olusturma aninda plain text gosterilir.
3. DB'ye `key_prefix` ve `key_hash` yazilir.
4. Agent aktivasyonda API key, tenant id ve machine fingerprint gonderir.
5. SpAdmin key hash, tenant status, expiry ve device policy kontrol eder.
6. Basarili aktivasyonda agent device kaydi ve refresh token olusturulur.
7. Gunluk calisma access/refresh token ile devam eder.

## License Status

- `valid`: agent sync yapabilir.
- `expiring`: agent sync yapabilir, UI ve SpAdmin uyari verir.
- `expired`: agent sync yapamaz; purge command dondurulur.
- `disabled`: agent sync yapamaz; purge command dondurulur.
- `revoked`: API key veya token kullanilamaz.
- `device_mismatch`: machine fingerprint uyusmaz.

## Heartbeat License Response

Heartbeat cevabi su bilgileri tasir:

- `licenseStatus`
- `expiresAt`
- `enabledModules`
- `configVersion`
- `commands`

Expired/disabled durumda `commands` icinde `purge_local_cache` yer alir.

## Modul Modeli

Etkin moduller su sirayla hesaplanir:

1. Tenant'in `subscription_plans` kaydi okunur.
2. Planin `plan_modules` default modulleri uygulanir.
3. `tenant_module_overrides` kayitlari defaultlari ezer.
4. Disabled tenant icin tum moduller kapali sayilir.

## Modul Kapama Etkileri

- Android kapali modul icin belge gonderirse API `module_disabled` hatasi dondurur.
- Agent kapali modulun job'larini cekmez.
- Var olan pending job'lar module disabled olunca `blocked_by_license` durumuna alinabilir.
- Modul tekrar acilirsa blocked job'lar yeniden pending yapilabilir.

## Expiry Davranisi

SpAdmin tarafinda:

- Mobile sessionlar revoke edilir.
- Yeni mobile document kabul edilmez.
- Agent heartbeat `expired` dondurur.
- Agent'a local cache/token/pending queue purge komutu verilir.

Silinmeyecekler:

- Mikro ERP kayitlari.
- Job ack gecmisi.
- Audit events.
- License events.

## Renewal

Lisans yenilenince:

- Tenant `license_expires_at` guncellenir.
- API key gerekirse yenilenir.
- Agent bir sonraki heartbeat'te `valid` durumu alir.
- Config version artirilir.

## Audit Zorunluluklari

Su islemler auditlenir:

- API key create/revoke.
- License extend/expire/disable/reactivate.
- Plan degisikligi.
- Module override ekleme/kaldirma.
- Device revoke.
- Support bundle request.
