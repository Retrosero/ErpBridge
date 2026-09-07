# 10 - Expiry Data Purge Prompt

Implement license expiry behavior.

Behavior:

- Stop new sync loops.
- Cancel active leases safely.
- Purge local tokens and refresh tokens.
- Purge local bootstrap cache.
- Purge pending local queue.
- Mark local state as expired.
- Notify central API when possible.

Must not delete:

- Mikro ERP documents.
- Mikro legal accounting records.
- Redacted audit history required for support.

Tests:

- Expired heartbeat triggers purge.
- Disabled tenant triggers purge.
- ERP database is not modified by purge service.
- UI shows expired state.
- Re-activation after renewal starts cleanly.
