# 04 - Module Management Prompt

Implement subscription plan and module management.

Tables:

- `module_catalog`
- `subscription_plans`
- `plan_modules`
- `tenant_module_overrides`

Modules:

- `sales_order`
- `sales_invoice`
- `sales_dispatch`
- `collection`
- `stock_sync`
- `customer_sync`
- `visit_tracking`
- `warehouse_transfer`
- `reports`
- `support_bundle`

Rules:

- Effective modules = plan defaults + tenant overrides.
- Disabled tenant has no enabled modules.
- Module override requires SuperAdmin.
- Module changes increase tenant config version.
- Module changes create audit events.

Tests:

- Plan defaults apply.
- Tenant override disables module.
- Tenant override enables trial module.
- Non-SuperAdmin cannot change override.
