# 01 - Project Shell Prompt

Create the initial SpAdmin .NET 8 solution.

Projects:

- `SpAdmin.Api`: Admin, Agent and Mobile APIs.
- `SpAdmin.Web`: Admin web UI.
- `SpAdmin.Core`: domain models, policies and interfaces.
- `SpAdmin.Infrastructure`: SQL Server, storage, auth, email/SMS, logging.
- `SpAdmin.Workers`: expiry, retention, lease cleanup, notification jobs.
- `SpAdmin.Tests`: unit and integration tests.

Implement:

- SQL Server connection and migration skeleton.
- Global error model with correlation id.
- Health checks.
- Redacted logging.
- Basic module/tenant/license option objects.

Acceptance:

- Solution builds.
- Health endpoint works.
- Secret redaction tests pass.
