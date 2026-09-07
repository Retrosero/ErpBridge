# 10 - Release Prompt

Implement SpAdmin deployment and release readiness.

Required:

- Production configuration model.
- SQL Server migration command.
- Backup and restore documentation.
- Monitoring and alerting hooks.
- Rollback plan.
- Seed data for roles, modules and default plans.

Acceptance:

- Fresh database migrates.
- Roles and modules are seeded idempotently.
- Health checks include DB and storage.
- Deployment docs explain TLS, connection strings and secret storage.
- Rollback path is documented.
