# 02 - Database Schema

Tum tarih alanlari UTC saklanir. Tum tenant scoped tablolarda `tenant_id` bulunur ve indekslenir. API key, token ve secret alanlar plain text saklanmaz.

## Identity and RBAC

### `admin_users`

- `id` uniqueidentifier primary key
- `email` nvarchar(320) unique
- `password_hash` nvarchar(500)
- `display_name` nvarchar(200)
- `status` nvarchar(30)
- `last_login_at` datetime2 null
- `created_at` datetime2
- `updated_at` datetime2

### `admin_roles`

- `id` int primary key
- `name` nvarchar(50) unique
- Varsayilan roller: `SuperAdmin`, `Support`, `Sales`, `TenantAdmin`, `ReadOnly`

### `admin_user_roles`

- `admin_user_id` uniqueidentifier
- `role_id` int
- `tenant_id` uniqueidentifier null

### `admin_mfa_secrets`

- `admin_user_id` uniqueidentifier
- `secret_ciphertext` varbinary(max)
- `enabled_at` datetime2
- `recovery_codes_hash` nvarchar(max)

### `admin_sessions`

- `id` uniqueidentifier
- `admin_user_id` uniqueidentifier
- `refresh_token_hash` nvarchar(500)
- `expires_at` datetime2
- `revoked_at` datetime2 null

## Tenant

### `tenants`

- `tenant_id` uniqueidentifier primary key
- `company_name` nvarchar(250)
- `tax_number` nvarchar(50) null
- `status` nvarchar(30)
- `plan_id` int
- `license_starts_at` datetime2
- `license_expires_at` datetime2
- `created_at` datetime2
- `updated_at` datetime2
- `deleted_at` datetime2 null

Indexes:

- `IX_tenants_status`
- `IX_tenants_license_expires_at`

### `tenant_contacts`

- `id` uniqueidentifier
- `tenant_id` uniqueidentifier
- `name` nvarchar(200)
- `email` nvarchar(320)
- `phone` nvarchar(50)
- `role` nvarchar(100)

### `tenant_settings`

- `tenant_id` uniqueidentifier
- `setting_key` nvarchar(100)
- `setting_value` nvarchar(max)
- `is_secret` bit

### `tenant_status_snapshots`

- `id` bigint identity
- `tenant_id` uniqueidentifier
- `status_json` nvarchar(max)
- `created_at` datetime2

## License and Agent

### `api_keys`

- `id` uniqueidentifier
- `tenant_id` uniqueidentifier
- `key_prefix` nvarchar(20)
- `key_hash` nvarchar(500)
- `status` nvarchar(30)
- `starts_at` datetime2
- `expires_at` datetime2
- `last_used_at` datetime2 null
- `revoked_at` datetime2 null
- `created_by` uniqueidentifier
- `created_at` datetime2

Unique:

- `UX_api_keys_key_hash`

### `agent_devices`

- `device_id` uniqueidentifier primary key
- `tenant_id` uniqueidentifier
- `machine_fingerprint_hash` nvarchar(500)
- `display_name` nvarchar(200)
- `agent_version` nvarchar(50)
- `os_info` nvarchar(250)
- `status` nvarchar(30)
- `last_heartbeat_at` datetime2 null
- `created_at` datetime2

### `agent_tokens`

- `id` uniqueidentifier
- `tenant_id` uniqueidentifier
- `device_id` uniqueidentifier
- `refresh_token_hash` nvarchar(500)
- `expires_at` datetime2
- `revoked_at` datetime2 null
- `created_at` datetime2

### `license_events`

- `id` bigint identity
- `tenant_id` uniqueidentifier
- `event_type` nvarchar(50)
- `old_status` nvarchar(30) null
- `new_status` nvarchar(30)
- `old_expires_at` datetime2 null
- `new_expires_at` datetime2 null
- `actor_admin_user_id` uniqueidentifier null
- `created_at` datetime2

## Module Management

### `module_catalog`

- `module_id` int primary key
- `code` nvarchar(80) unique
- `name` nvarchar(150)
- `description` nvarchar(500)
- `status` nvarchar(30)

Initial codes:

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

### `subscription_plans`

- `plan_id` int primary key
- `code` nvarchar(80) unique
- `name` nvarchar(150)
- `status` nvarchar(30)

### `plan_modules`

- `plan_id` int
- `module_id` int
- `enabled` bit
- `limits_json` nvarchar(max) null

### `tenant_module_overrides`

- `id` uniqueidentifier
- `tenant_id` uniqueidentifier
- `module_id` int
- `enabled` bit
- `limits_json` nvarchar(max) null
- `reason` nvarchar(500)
- `updated_by` uniqueidentifier
- `updated_at` datetime2

## Sync Operations

### `agent_heartbeats`

- `id` bigint identity
- `tenant_id` uniqueidentifier
- `device_id` uniqueidentifier
- `agent_version` nvarchar(50)
- `status` nvarchar(30)
- `license_status` nvarchar(30)
- `queue_depth` int
- `last_sync_at` datetime2 null
- `summary_json` nvarchar(max)
- `created_at` datetime2

### `agent_health_events`

- `id` bigint identity
- `tenant_id` uniqueidentifier
- `device_id` uniqueidentifier
- `severity` nvarchar(20)
- `event_code` nvarchar(100)
- `message` nvarchar(1000)
- `created_at` datetime2

### `sync_jobs`

- `job_id` uniqueidentifier primary key
- `tenant_id` uniqueidentifier
- `entity_type` nvarchar(50)
- `operation` nvarchar(80)
- `document_type` nvarchar(80) null
- `external_id` nvarchar(120)
- `status` nvarchar(30)
- `payload_json` nvarchar(max)
- `leased_device_id` uniqueidentifier null
- `lease_expires_at` datetime2 null
- `created_at` datetime2
- `updated_at` datetime2

Unique:

- `UX_sync_jobs_tenant_type_external` on `tenant_id`, `document_type`, `external_id`

### `sync_job_attempts`

- `id` bigint identity
- `tenant_id` uniqueidentifier
- `job_id` uniqueidentifier
- `device_id` uniqueidentifier null
- `attempt_no` int
- `status` nvarchar(30)
- `error_code` nvarchar(100) null
- `error_message` nvarchar(1000) null
- `created_at` datetime2

### `job_acks`

- `id` bigint identity
- `tenant_id` uniqueidentifier
- `job_id` uniqueidentifier
- `status` nvarchar(30)
- `mikro_version` int null
- `mikro_database` nvarchar(200) null
- `evrak_seri` nvarchar(20) null
- `evrak_sira` int null
- `recno` int null
- `guid` uniqueidentifier null
- `created_at` datetime2

## Mobile and Cache

### `tenant_data_cache`

- `id` bigint identity
- `tenant_id` uniqueidentifier
- `entity_type` nvarchar(80)
- `entity_key` nvarchar(150)
- `data_json` nvarchar(max)
- `source_updated_at` datetime2
- `expires_at` datetime2 null

### `mobile_devices`

- `device_id` uniqueidentifier
- `tenant_id` uniqueidentifier
- `user_code` nvarchar(100)
- `status` nvarchar(30)
- `last_seen_at` datetime2 null

### `mobile_sessions`

- `id` uniqueidentifier
- `tenant_id` uniqueidentifier
- `device_id` uniqueidentifier
- `session_token_hash` nvarchar(500)
- `expires_at` datetime2
- `revoked_at` datetime2 null

## Logs and Support

### `agent_log_summaries`

- `id` bigint identity
- `tenant_id` uniqueidentifier
- `device_id` uniqueidentifier
- `level` nvarchar(20)
- `event_code` nvarchar(100)
- `message_template` nvarchar(500)
- `count` int
- `first_seen_at` datetime2
- `last_seen_at` datetime2

### `support_bundle_requests`

- `request_id` uniqueidentifier
- `tenant_id` uniqueidentifier
- `device_id` uniqueidentifier
- `status` nvarchar(30)
- `requested_by` uniqueidentifier
- `reason` nvarchar(500)
- `expires_at` datetime2
- `created_at` datetime2

### `support_bundles`

- `bundle_id` uniqueidentifier
- `request_id` uniqueidentifier
- `tenant_id` uniqueidentifier
- `device_id` uniqueidentifier
- `storage_uri` nvarchar(1000)
- `sha256` nvarchar(100)
- `size_bytes` bigint
- `uploaded_at` datetime2

## Audit

### `audit_events`

- `id` bigint identity
- `tenant_id` uniqueidentifier null
- `actor_admin_user_id` uniqueidentifier null
- `actor_role` nvarchar(80)
- `action` nvarchar(120)
- `target_type` nvarchar(80)
- `target_id` nvarchar(120)
- `old_summary_json` nvarchar(max) null
- `new_summary_json` nvarchar(max) null
- `ip_address` nvarchar(80)
- `user_agent` nvarchar(500)
- `correlation_id` nvarchar(100)
- `created_at` datetime2
