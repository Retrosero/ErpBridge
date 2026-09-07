# 02 - License Activation Prompt

Implement license activation and heartbeat for Sync Adapter.

Services:

- `LicenseService`
- `DeviceFingerprintService`
- `SecretStore`
- `RemoteApiClient`
- `ExpiryPurgeService`

Contracts:

- `POST /agent/v1/activate`
- `POST /agent/v1/heartbeat`

Rules:

- API key, access token, refresh token and SQL password must be stored with DPAPI or Credential Manager.
- Invalid, disabled, expired or device-mismatch licenses prevent sync.
- Expired license purges local tokens, bootstrap cache and pending queue.
- ERP records are never deleted by expiry logic.

Tests:

- Valid activation.
- Invalid API key.
- Expired key.
- Disabled tenant.
- Device mismatch.
- Log redaction for all secret types.
