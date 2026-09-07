# 05 - Agent Activation Heartbeat Prompt

Implement Sync Adapter agent activation and heartbeat.

Endpoints:

- `POST /agent/v1/activate`
- `POST /agent/v1/heartbeat`

Activation checks:

- API key hash exists.
- API key not expired/revoked.
- Tenant active.
- Machine fingerprint allowed or new device accepted.

Heartbeat response:

- License status.
- Expiry date.
- Enabled modules.
- Config version.
- Commands such as `purge_local_cache`.

Tests:

- Valid activation creates device and token.
- Wrong tenant rejected.
- Expired key rejected.
- Expired tenant heartbeat returns purge command.
- Module list reflects overrides.
