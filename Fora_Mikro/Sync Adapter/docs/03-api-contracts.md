# 03 - API Contracts

Bu dokuman ilk surum icin minimum public API sozlesmelerini tanimlar. Tum endpointler HTTPS uzerinden calisir.

## Mobile API

### `POST /mobile/v1/documents`

Android uygulamanin evrak, tahsilat, ziyaret ve cari kayitlarini merkezi API'ye gonderdigi endpoint.

Request:

```json
{
  "tenantId": "tenant-001",
  "deviceId": "android-device-001",
  "userCode": "plasiyer1",
  "documentType": "sales_invoice",
  "externalId": "android-uuid-001",
  "occurredAt": "2026-06-25T10:30:00+03:00",
  "payloadVersion": 1,
  "payload": {}
}
```

Response:

```json
{
  "accepted": true,
  "jobId": "job-001",
  "status": "queued"
}
```

### `GET /mobile/v1/bootstrap`

Android'i besleyen cari, stok, fiyat, depo, kasa/banka ve kullanici ayarlarini doner.

Query:

- `tenantId`
- `deviceId`
- `userCode`
- `since`

Response:

```json
{
  "serverTime": "2026-06-25T10:30:00+03:00",
  "fullRefreshRequired": false,
  "data": {
    "customers": [],
    "stocks": [],
    "prices": [],
    "warehouses": [],
    "cashAccounts": [],
    "bankAccounts": [],
    "settings": {}
  }
}
```

## Agent API

### `POST /agent/v1/activate`

Agent kurulumunda API key, tenant ve device binding yapar.

Request:

```json
{
  "apiKey": "redacted",
  "tenantId": "tenant-001",
  "machineFingerprint": "fingerprint",
  "agentVersion": "1.0.0"
}
```

Response:

```json
{
  "activated": true,
  "accessToken": "redacted",
  "refreshToken": "redacted",
  "expiresAt": "2026-07-25T00:00:00+03:00",
  "configVersion": 1
}
```

### `POST /agent/v1/heartbeat`

Agent durumunu bildirir ve lisans/config state alir.

Request:

```json
{
  "tenantId": "tenant-001",
  "deviceId": "agent-device-001",
  "agentVersion": "1.0.0",
  "status": "healthy",
  "lastSyncAt": "2026-06-25T10:30:00+03:00"
}
```

Response:

```json
{
  "licenseStatus": "valid",
  "expiresAt": "2026-07-25T00:00:00+03:00",
  "configVersion": 1,
  "commands": []
}
```

### `GET /agent/v1/jobs`

Agent pending isleri ceker.

Query:

- `tenantId`
- `limit`
- `capabilities`

Response:

```json
{
  "jobs": [
    {
      "jobId": "job-001",
      "entityType": "document",
      "operation": "write_to_mikro",
      "documentType": "sales_invoice",
      "externalId": "android-uuid-001",
      "payloadVersion": 1,
      "payload": {}
    }
  ]
}
```

### `POST /agent/v1/jobs/{jobId}/ack`

Mikro yazimi sonucunu bildirir.

Request:

```json
{
  "tenantId": "tenant-001",
  "status": "completed",
  "mikro": {
    "version": 16,
    "database": "MikroDB_V16",
    "evrakSeri": "A",
    "evrakSira": 123,
    "recno": null,
    "guid": "00000000-0000-0000-0000-000000000000"
  },
  "error": null
}
```

## Error Model

Standart hata cevabi:

```json
{
  "errorCode": "validation_error",
  "message": "Customer code is required.",
  "correlationId": "corr-001"
}
```

Hata kodlari:

- `validation_error`
- `unauthorized`
- `license_expired`
- `tenant_mismatch`
- `device_mismatch`
- `transient_error`
- `mikro_write_error`
- `duplicate_document`
