# 04 - Admin API Contracts

Tum admin endpointleri HTTPS, authenticated session/JWT ve RBAC policy gerektirir.

## Tenant APIs

### `GET /admin/v1/tenants`

Query:

- `status`
- `expiresBefore`
- `search`
- `page`
- `pageSize`

### `POST /admin/v1/tenants`

```json
{
  "companyName": "Example Ltd",
  "taxNumber": "1234567890",
  "planCode": "pro",
  "licenseStartsAt": "2026-06-25T00:00:00Z",
  "licenseExpiresAt": "2027-06-25T00:00:00Z"
}
```

### `PATCH /admin/v1/tenants/{tenantId}`

Tenant status, contact summary and plan fields update edilir.

## License APIs

### `POST /admin/v1/tenants/{tenantId}/api-keys`

Yeni API key uretir. Plain key sadece response icinde bir kez dondurulur.

```json
{
  "startsAt": "2026-06-25T00:00:00Z",
  "expiresAt": "2027-06-25T00:00:00Z",
  "reason": "Initial license"
}
```

### `PATCH /admin/v1/tenants/{tenantId}/license`

```json
{
  "status": "valid",
  "expiresAt": "2027-06-25T00:00:00Z",
  "reason": "Renewal"
}
```

## Module APIs

### `GET /admin/v1/modules`

Global module catalog dondurur.

### `GET /admin/v1/tenants/{tenantId}/modules`

Plan + override sonrasi etkin modul listesini dondurur.

### `PATCH /admin/v1/tenants/{tenantId}/modules`

```json
{
  "overrides": [
    {
      "moduleCode": "warehouse_transfer",
      "enabled": true,
      "limits": {},
      "reason": "Temporary trial"
    }
  ]
}
```

## Agent APIs

### `GET /admin/v1/agents`

Agent cihazlari, tenant, durum ve son heartbeat bilgisini listeler.

### `GET /admin/v1/tenants/{tenantId}/agents`

Tenant'a ait agentlari listeler.

### `POST /admin/v1/tenants/{tenantId}/agents/{deviceId}/revoke`

Cihazi revoke eder ve refresh tokenlari gecersiz kilar.

## Log and Support APIs

### `GET /admin/v1/tenants/{tenantId}/logs/summary`

Ozet loglari dondurur:

- level
- eventCode
- count
- firstSeenAt
- lastSeenAt

### `POST /admin/v1/tenants/{tenantId}/support-bundles/request`

```json
{
  "deviceId": "00000000-0000-0000-0000-000000000000",
  "reason": "Investigate failed invoices",
  "expiresAt": "2026-06-26T00:00:00Z"
}
```

## Agent Public APIs

### `POST /agent/v1/activate`

API key, tenant id, machine fingerprint ve agent version alir; token, expiry, modules ve config dondurur.

### `POST /agent/v1/heartbeat`

Agent health summary yollar; lisans status, enabled modules, config version ve commands alir.

### `GET /agent/v1/jobs`

Enabled module ve tenant lisans durumuna gore job lease eder.

### `POST /agent/v1/jobs/{jobId}/ack`

Mikro yazim sonucunu ve ERP evrak metadata bilgisini kaydeder.

### `POST /agent/v1/support-bundles/{requestId}`

Talep edilen redacted support bundle'i upload eder.

## Error Model

```json
{
  "errorCode": "module_disabled",
  "message": "Module is not enabled for this tenant.",
  "correlationId": "corr-001"
}
```

Hata kodlari:

- `unauthorized`
- `forbidden`
- `tenant_not_found`
- `license_expired`
- `api_key_revoked`
- `device_mismatch`
- `module_disabled`
- `validation_error`
- `conflict`
