---
name: sync-agent-security-release
description: Secure, package, test, and release customer-installed synchronization agents.
---

# Sync Agent Security Release

Use this skill before shipping Sync Adapter to customer machines.

## Security

- Require activation before sync: API key, tenant id, device id or machine fingerprint, license status and expiry.
- Store API tokens, SQL passwords and connection strings with DPAPI or Credential Manager.
- Redact secrets in logs, UI errors, support bundles and crash reports.
- Bind local API to `127.0.0.1` unless explicitly configured otherwise.
- Protect local API endpoints with a local token.
- Use table/entity allowlists.
- Use parameterized queries for all database access.

## Release

- Provide MSI/MSIX/WiX installer.
- Install/uninstall Windows Service repeatably.
- Document firewall behavior.
- Add versioned local store migrations.
- Include support package export.
- Use signed installer or trusted download channel.
- Provide rollback plan.

## Tests

- Invalid, expired, disabled and valid API key paths.
- Token refresh and heartbeat failure.
- Offline startup and recovery.
- Local API rejects non-local callers by default.
- Logs do not contain passwords, tokens or full connection strings.
- Duplicate job replay does not create duplicate ERP records.
- Database timeout and deadlock are retried safely.
