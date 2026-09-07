# 02 - Auth RBAC 2FA Prompt

Implement admin authentication, RBAC and 2FA.

Required:

- Admin login/logout.
- Password hashing.
- Session or JWT auth.
- Roles: `SuperAdmin`, `Support`, `Sales`, `TenantAdmin`, `ReadOnly`.
- Permission policies per endpoint.
- TOTP 2FA.
- Recovery codes stored as hashes.

Rules:

- SuperAdmin and Support require 2FA.
- Role changes require audit event.
- Failed login is rate limited.
- TenantAdmin is tenant scoped.

Tests:

- Wrong password denied.
- Missing 2FA denied for required roles.
- Support cannot change license expiry.
- TenantAdmin cannot read other tenant.
