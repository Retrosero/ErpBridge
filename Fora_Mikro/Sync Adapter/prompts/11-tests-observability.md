# 11 - Tests and Observability Prompt

Implement test and observability infrastructure.

Required:

- Unit tests for validators, transformers, redaction and idempotency.
- Integration tests for local SQLite queue.
- SQL writer tests using test database or fake repository abstraction.
- Health check endpoint for local UI.
- Support bundle export.

Observability:

- Correlation id for every job.
- Tenant id and job id in logs.
- No secret values in logs.
- Failed job summary with actionable error.
- Retry and dead-letter metrics.

Tests:

- Log redaction catches tokens and passwords.
- Support bundle excludes secrets.
- Failed job contains useful error message.
- Health check reports license, queue, Mikro DB and API connectivity.
