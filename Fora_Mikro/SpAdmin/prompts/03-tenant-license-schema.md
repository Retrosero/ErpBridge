# 03 - Tenant License Schema Prompt

Implement tenant, license, API key, agent device and token schema.

Tables:

- `tenants`
- `tenant_contacts`
- `tenant_settings`
- `api_keys`
- `agent_devices`
- `agent_tokens`
- `license_events`
- `audit_events`

Rules:

- API key is shown once and stored only as hash.
- Refresh tokens are stored only as hash.
- Tenant delete is soft-delete.
- License changes create `license_events` and `audit_events`.
- Expired tenants cannot accept new mobile documents.

Tests:

- API key lookup by hash.
- Revoked key denied.
- Expired license denied.
- License renewal audit created.
