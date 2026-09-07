# 07 - Admin Dashboard Prompt

Implement the first SpAdmin dashboard.

Views:

- Tenant list with license status and expiry.
- Expiring licenses.
- Agent health overview.
- Failed jobs summary.
- Module state per tenant.
- Support bundle request status.

Rules:

- Tenant scoped users only see their tenant.
- SuperAdmin sees all tenants.
- Dashboard queries must not leak cross-tenant counts to TenantAdmin.
- All displayed errors are redacted.

Tests:

- SuperAdmin all-tenant view.
- TenantAdmin scoped view.
- Expiring license filter.
- Agent offline indicator.
