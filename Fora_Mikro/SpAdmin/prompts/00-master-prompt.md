# 00 - Master Prompt

Build `SpAdmin`, the .NET 8 + SQL Server control plane for Sync Adapter.

Core decisions:

- Single central SQL Server database.
- All operational tenant data is scoped by `tenant_id`.
- Admin auth uses RBAC + 2FA.
- API keys are time-limited and stored only as hashes.
- Agents activate with API key, then use access/refresh tokens.
- Modules are controlled by subscription plans plus tenant feature flag overrides.
- Continuous logs are summary-only; detailed support bundles are uploaded only on request.

Non-negotiables:

- No plain API keys or tokens in the database.
- No admin mutation without audit event.
- No cross-tenant reads or writes.
- No support bundle without explicit request and authorization.
- No secret values in logs, UI errors, audit summaries or support bundles.
